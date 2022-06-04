/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * VideoGenerator.cs
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

// ReSharper disable InconsistentNaming
namespace Gear.EmulationCore
{
    /// @brief Video mode Field.
    /// @details The 2-bit video mode selects the type and orientation of video output.
    ///
    /// Source: Table 8 - The Video Mode Field, %Propeller P8X32A Datasheet V1.4.0.
    public enum VideoMode
    {
        /// <summary>Disabled, no video generated.</summary>
        Disabled = 0x00000000,
        /// <summary>VGA mode; 8-bit parallel output on VPins 7:0</summary>
        VgaMode = 0x20000000,
        /// <summary>Composite Mode 1; broadcast on VPins 7:4, base band on VPins 3:0</summary>
        Composite1 = 0x40000000,
        /// <summary>Composite Mode 2; base band on VPins 7:4, broadcast on VPins 3:0</summary>
        Composite2 = 0x60000000
    }

    /// <summary>Color mode for video generation.</summary>
    public enum ColorMode
    {
        /// <summary>Two colors.</summary>
        TwoColor  = 0x00000000,
        /// <summary>Four colors.</summary>
        FourColor = 0x10000000
    }

    /// @brief Video Generator Circuit emulation.
    public class VideoGenerator
    {
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _scale;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _config;

        // Scale Registers

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _pixelClockStart;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private bool _scaleDirty;

        // Configuration Registers

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions
        /// and to clarify meaning of it.
        private VideoMode _videoMode;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions
        /// and to clarify meaning of it.
        private ColorMode _colorMode;

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private bool _chroma0;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private bool _chroma1;

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _auralSub;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private int _videoGroup;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private ulong _videoPins;

        // Serialization values

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _pixelLoad;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _colorLoad;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _shiftOut;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _discrete;

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private byte _phaseAccumulator;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private bool _vhfCarrier;

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private ulong _outputLoad;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private ulong _broadcastUp;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private ulong _broadcastDown;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private ulong _baseBandOut;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private ulong _auralBit;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private bool _auralOutput;

        // Misc Variables

        /// <summary>Reference to the PropellerCPU instance where this
        /// object belongs.</summary>
        /// @version v22.05.03 - Name changed to use the same convention for
        /// a PropellerCPU instance reference.
        private readonly PropellerCPU _cpuHost;
        /// <summary>Cog instance where this object belongs.</summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private readonly Cog _cog;

        /// <summary></summary>
        /// @version v22.05.03 - Merge between property and private member.
        public uint FrameCount { get; private set; }

        /// <summary></summary>
        /// @version v22.05.03 - Added, replacing odd method %FrameCounters()
        /// and private member %FrameClocks.
        public uint FrameClock { get; private set; }

        /// <summary></summary>
        /// @version v22.05.03 - Added, replacing odd method %FrameCounters()
        /// and private member %PixelClocks.
        public uint PixelClock { get; private set; }

        /// <summary></summary>
        /// @version v22.05.04 - Changed property name to clarify meaning of it.
        public uint RegisterCFG
        {
            get => _config;
            set
            {
                if (_config == value)
                    return;
                _config = value;
                // Detach from our old aural hook
                if (_cpuHost.GetPLL(_auralSub) != null)
                    _cpuHost.GetPLL(_auralSub).RemoveHook(this);
                _videoMode = (VideoMode)(_config & 0x60000000);
                _colorMode = (ColorMode)(_config & 0x10000000);
                _chroma0 = (_config & 0x04000000) != 0;
                _chroma1 = (_config & 0x08000000) != 0;
                _auralSub = (_config >> 23) & 0x7;
                _videoGroup = (int)(_config >> 6) & 0x38;      // _videoGroup is the first pin to output from
                _videoPins = (_config & 0xFF) << _videoGroup;  // _videoPins becomes a 64bit pin mask
                // Attach to the new aural sub PLL
                if (_cpuHost.GetPLL(_auralSub) != null)
                    _cpuHost.GetPLL(_auralSub).AttachHook(this);
                // Reset counters
                _pixelClockStart = (_scale >> 12) & 0xFF;
                FrameClock = _scale & 0xFFF;  // Copy our frame clocks out of the scale register
                FrameCount = 0;
            }
        }

        /// <summary></summary>
        /// @version v22.05.04 - Changed property name to clarify meaning of it.
        public uint RegisterSCL
        {
            get => _scale;
            set
            {
                if (_scale == value)
                    return;
                _scale = value;
                _scaleDirty = true;
            }
        }

        /// <summary></summary>
        public bool Ready => FrameClock <= 0;

        /// <summary></summary>
        public ulong Output => _outputLoad & _videoPins;

        /// <summary>Default constructor.</summary>
        /// <param name="cpuHost">PropellerCPU instance where this object
        /// belongs.</param>
        /// <param name="cogOwner">Cog instance where this object belongs.</param>
        /// @version v22.05.03 - Parameter names changed to use the same
        /// convention for a PropellerCPU instance reference and to clarify
        /// meaning of them.
        public VideoGenerator(PropellerCPU cpuHost, Cog cogOwner)
        {
            _cpuHost = cpuHost;
            _cog = cogOwner;
            // Clear our phase accumulator
            _phaseAccumulator = 0;
            // Scale is dirty
            _scaleDirty = true;
        }

        /// <summary></summary>
        public void DetachAural()
        {
            if (_cpuHost.GetPLL(_auralSub) != null)
                _cpuHost.GetPLL(_auralSub).RemoveHook(this);
        }

        /// <summary></summary>
        /// <param name="colors"></param>
        /// <param name="pixels"></param>
        public void Feed(uint colors, uint pixels)
        {
            _colorLoad = colors;
            _pixelLoad = pixels;
            if (_scaleDirty)
            {
                _pixelClockStart = (_scale >> 12) & 0xFF;
                _scaleDirty = false;
            }
            FrameClock = _scale & 0xFFF;    // Copy our frame clocks out of the scale register
            PixelClock = _pixelClockStart;  // update PixelClock, similar to verilog
        }

        /// <summary></summary>
        /// <param name="color"></param>
        private void FillComposite(uint color)
        {
            // The top four bits of the 'color' for the current pixel is added to this,
            // If bit 3 of the color is set, then the MSB signifies a +/- 1 offset on
            // the luma output.  (1/16 phase divider with a 16 entry shifter).  The
            // lower four bits are unused.

            // Output LUMA
            ulong baseBand = color & 0x7;
            ulong broadcast = baseBand;
            // Output Chroma
            if ((color & 0x08) != 0)
            {
                // We assume that the phase accumulator 4 LSBs are clear
                ulong shiftedPhase = (_phaseAccumulator + color) & 0x80;

                // --- BASEBAND MODE ---

                // Mix if necessary
                if (_chroma0)
                {
                    if (shiftedPhase != 0)
                        baseBand = (baseBand + 1) & 0x7;
                    else
                        baseBand = (baseBand - 1) & 0x7;
                }
                // Mix chroma on pin 3
                baseBand |= shiftedPhase >> 4;

                // --- BROADCAST MODE ---

                // Mix if necessary
                if (_chroma1)
                {
                    if (shiftedPhase != 0)
                        broadcast = (broadcast + 1) & 0x7;
                    else
                        broadcast = (broadcast - 1) & 0x7;
                }
            }
            _baseBandOut = baseBand << _videoGroup;
            _broadcastUp = (7 - (broadcast >> 1)) << _videoGroup;
            _broadcastDown = ((broadcast + 1) >> 1) << _videoGroup;
        }

        /// <summary></summary>
        private void UpdateCompositeOut()
        {
            if (_videoMode != VideoMode.Composite1 && _videoMode != VideoMode.Composite2)
                return;
            _outputLoad = _baseBandOut | (_vhfCarrier ? _broadcastUp : _broadcastDown);
            if (_auralOutput ^ _vhfCarrier)
                _outputLoad |= _auralBit;
        }

        /// <summary></summary>
        /// <param name="level"></param>
        public void AuralTick(bool level)
        {
            _auralOutput = level;
            UpdateCompositeOut();
        }

        /// <summary></summary>
        /// <param name="level"></param>
        public void CarrierTick(bool level)
        {
            _vhfCarrier = level;
            UpdateCompositeOut();
        }
        /// <summary></summary>
        /// <param name="level"></param>
        /// @pullreq{29} Changed video generation Procedure to fit Verilog Code.
        /// @version v22.03.01 - Improved accuracy of Video generator.
        public void ColorTick(bool level)
        {
            // Only tick color on rising edge
            if (level != true)
                return;
            // Avoid extra logic when video generator is disabled
            if (_videoMode == VideoMode.Disabled)
            {
                _outputLoad = 0;
                return;
            }
            // Output the load to the pins
            switch (_videoMode)
            {
                case VideoMode.VgaMode:
                    _outputLoad = _discrete << _videoGroup;
                    break;
                case VideoMode.Composite1: // 0..3 Baseband 4..7 Broadcast
                    FillComposite(_shiftOut);
                    _broadcastUp <<= 4;
                    _broadcastDown <<= 4;
                    _auralBit = (ulong)0x8 << 4 << _videoGroup;
                    UpdateCompositeOut();
                    break;
                case VideoMode.Composite2: // 4..7 Baseband 0..3 Broadcast
                    FillComposite(_shiftOut);
                    _baseBandOut <<= 4;
                    _auralBit = (ulong)0x8 << _videoGroup;
                    UpdateCompositeOut();
                    break;
            }
            // Composite Processes gets updated one tick later than VGA
            _shiftOut = _discrete;
            // Find the pixel color which will be processed by VGA Directly
            switch (_colorMode)
            {
                // Four color video
                case ColorMode.FourColor:
                    _discrete = (_colorLoad >> (int)((_pixelLoad & 3) << 3)) & 0xFF;
                    break;
                // Two color mode
                case ColorMode.TwoColor:
                default:
                    _discrete = (_colorLoad >> (int)((_pixelLoad & 1) << 3)) & 0xFF;
                    break;
            }
            // Decrement clocks
            --FrameClock;
            FrameClock &= 0xFFF;
            --PixelClock;
            PixelClock &= 0xFF;
            // Check to see if we are at the end of our frame clocks
            if (FrameClock == 0)
            {
                _cog.GetVideoData(out uint colors, out uint pixels);
                Feed(colors, pixels);
                ++FrameCount;
            }
            // Clock up to our pixel accumulator (wait for pixel)
            else if (PixelClock == 0)
            {
                // Find the pixel color we need to shift out
                switch (_colorMode)
                {
                    // Four color video
                    case ColorMode.FourColor:
                        _pixelLoad = (_pixelLoad & 0xC0000000) | (_pixelLoad >> 2);
                        break;
                    // Two color mode
                    case ColorMode.TwoColor:
                    default:
                        _pixelLoad = (_pixelLoad & 0x80000000) | (_pixelLoad >> 1);
                        break;
                }
                // Fill our pixel clock countdown using the scale register
                PixelClock = _pixelClockStart;
            }
            // Accumulate phase (/16 divider)
            _phaseAccumulator += 0x10;
        }
    }
}
