/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * VideoGenerator.CS
 * Video Generator Circuit Base
 * --------------------------------------------------------------------------------
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * --------------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.EmulationCore
{
    public enum VMode : int
    {
        DISABLED        = 0x00000000,
        VGA_MODE        = 0x20000000,
        COMPOSITE_1     = 0x40000000,
        COMPOSITE_2     = 0x60000000
    }

    public enum CMode : int
    {
        TWO_COLOR       = 0x00000000,
        FOUR_COLOR      = 0x10000000
    }

    public class VideoGenerator
    {
        private uint Scale;
        private uint Config;

        // Scale Registers

        private uint PixelClockStart;
        private uint PixelClocks;
        private uint FrameClocks;
        private bool ScaleDirty;

        // Configuration Registers

        private VMode VideoMode;
        private CMode ColorMode;

        private bool Chroma0;
        private bool Chroma1;

        private uint AuralSub;
        private int VGroup;
        private ulong VPins;

        // Serialization values

        private uint PixelLoad;
        private uint ColorLoad;
        private uint ShiftOut;

        private byte PhaseAccumulator;
        private bool VHFCarrier;

        private ulong OutputLoad;
        private ulong BroadcastUp;
        private ulong BroadcastDown;
        private ulong BasebandOut;
        private ulong AuralBit;
        private bool AuralOutput;

        // Misc Variables

        private Propeller Chip;

        public uint CFG
        {
            get
            {
                return Config;
            }
            set
            {
                Config = value;

                // Detach from our old aural hook
                if (Chip.GetPLL(AuralSub) != null)
                    Chip.GetPLL(AuralSub).RemoveHook(this);

                VideoMode = (VMode)(Config & 0x60000000);
                ColorMode = (CMode)(Config & 0x10000000);

                Chroma0 = (Config & 0x04000000) != 0;
                Chroma1 = (Config & 0x08000000) != 0;

                AuralSub = (Config >> 23) & 0x7;
                VGroup = (int)(Config >> 6) & 0x38;                 // VGroup is the first pin to output from
                VPins = (Config & 0xFF) << VGroup;                  // Pins becomes a 64bit pin mask

                // Attach to the new aural sub PLL
                if (Chip.GetPLL(AuralSub) != null)
                    Chip.GetPLL(AuralSub).AttachHook(this);
            }
        }

        public uint SCL
        {
            get
            {
                return Scale;
            }
            set
            {
                if (Scale != value)
                {
                    ScaleDirty = true;
                    Scale = value;
                }
            }
        }

        public bool Ready
        {
            get
            {
                return (FrameClocks <= 0);
            }
        }

        public ulong Output
        {
            get
            {
                return OutputLoad & VPins;
            }
        }

        public VideoGenerator(Propeller chip)
        {
            // Clear our phase accumulator
            PhaseAccumulator = 0;
            Chip = chip;

            // Scale is dirty
            ScaleDirty = true;
        }

        public void DetachAural()
        {
            if (Chip.GetPLL(AuralSub) != null)
                Chip.GetPLL(AuralSub).RemoveHook(this);
        }

        public void Feed(uint colors, uint pixels)
        {
            ColorLoad = colors;
            PixelLoad = pixels;

            if (ScaleDirty)
            {
                PixelClockStart = (Scale >> 12) & 0xFF;
                ScaleDirty = false;
            }

            FrameClocks = Scale & 0xFFF;    // Copy our frameclocks out of the scale register
            PixelClocks = 1;                // Always serialize out the first pixel on first clock
        }

        private void FillComposite(uint color)
        {
            // The top four bits of the 'color' for the current pixel is added to this,
            // If bit 3 of the color is set, then the MSB signifies a +/- 1 offset on
            // the luma output.  (1/16 phase divider with a 16 entry shifter).  The
            // lower four bits are unused.

            // Output LUMA
            ulong baseband = color & 0x7;
            ulong broadcast = baseband;

            // Output Chroma
            if ((color & 0x08) != 0)
            {
                // We assume that the phase accumulator 4 LSBs are clear
                ulong shiftedPhase = (PhaseAccumulator + color) & 0x80;

                // --- BASEBAND MODE ---

                // Mix if nessessary
                if (Chroma0)
                {
                    if (shiftedPhase != 0)
                        baseband = (baseband + 1) & 0x7;
                    else
                        baseband = (baseband - 1) & 0x7;
                }

                // Mix chroma on pin 3
                baseband |= shiftedPhase >> 4;

                // --- BROADCAST MODE ---

                // Mix if nessessary
                if (Chroma1)
                {
                    if (shiftedPhase != 0)
                        broadcast = (broadcast + 1) & 0x7;
                    else
                        broadcast = (broadcast - 1) & 0x7;
                }

                //output |= AuralOutput ? (ulong)8 : (ulong)0;
            }

            BasebandOut = baseband << VGroup;
            BroadcastUp = (7 - (broadcast >> 1)) << VGroup;
            BroadcastDown = ((broadcast + 1) >> 1) << VGroup;
        }

        private void UpdateCompositeOut()
        {
            if (VideoMode != VMode.COMPOSITE_1 && VideoMode != VMode.COMPOSITE_2)
                return;

            OutputLoad = BasebandOut | (VHFCarrier ? BroadcastUp : BroadcastDown);

            if (AuralOutput ^ VHFCarrier)
                OutputLoad |= AuralBit;
        }

        public void AuralTick(bool level)
        {
            AuralOutput = level;
            UpdateCompositeOut();
        }

        public void CarrierTick(bool level)
        {
            VHFCarrier = level;
            UpdateCompositeOut();
        }

        public void ColorTick(bool level)
        {
            // Only tick color on rising edge
            if (level != true)
                return;

            // Check to see if we are at the end of our frame clocks
            if (FrameClocks <= 0)
                return;

            FrameClocks--;

            // Avoid extra logic when video generator is disabled
            if (VideoMode == VMode.DISABLED)
            {
                OutputLoad = 0;
                return;
            }

            // Clock up to our pixel accumulator (wait for pixel)
            if (--PixelClocks <= 0)
            {
                // Find the pixel color we need to shift out
                switch (ColorMode)
                {
                    // Four color video
                    case CMode.FOUR_COLOR:
                        ShiftOut = (ColorLoad >> (int)((PixelLoad & 3) << 3)) & 0xFF;
                        PixelLoad >>= 2;
                        break;
                    // Two color mode
                    default:
                        ShiftOut = (ColorLoad >> (int)((PixelLoad & 1) << 3)) & 0xFF;
                        PixelLoad >>= 1;
                        break;
                }

                // Fill our pixel clock countdown using the scale register
                PixelClocks = PixelClockStart;
            }

            // Output the load to the pins
            switch (VideoMode)
            {
                case VMode.VGA_MODE:
                    OutputLoad = ShiftOut << VGroup;
                    break;
                case VMode.COMPOSITE_1: // 0..3 Baseband 4..7 Broadcast
                    FillComposite(ShiftOut);

                    BroadcastUp <<= 4;
                    BroadcastDown <<= 4;
                    AuralBit = (ulong)0x8 << 4 << VGroup;

                    UpdateCompositeOut();
                    break;
                case VMode.COMPOSITE_2: // 4..7 Baseband 0..3 Broadcast
                    FillComposite(ShiftOut);

                    BasebandOut <<= 4;
                    AuralBit = (ulong)0x8 << VGroup;

                    UpdateCompositeOut();
                    break;
            }

            // Accumulate phase (/16 divider)
            PhaseAccumulator += 0x10;
        }
    }
}
