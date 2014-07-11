/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * FreqGenerator.cs
 * Frequency Generator Circuit emulation
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
    public enum CounterMode : uint
    {
        DISABLED,

        PLL_INTERNAL,
        PLL_SINGLE_ENDED,
        PLL_DIFFERENTIAL,

        NCO_SINGLE_ENDED,
        NCO_DIFFERENTIAL,

        DUTY_SINGLE_ENDED,
        DUTY_DIFFERENTIAL,

        POS_DETECTOR,
        POS_DETECTOR_FEEDBACK,
        POSEDGE_DETECTOR,
        POSEDGE_DETECTOR_FEEDBACK,

        NEG_DETECTOR,
        NEG_DETECTOR_FEEDBACK,
        NEGEDGE_DETECTOR,
        NEGEDGE_DETECTOR_FEEDBACK,

        // ASB: New logic modes not implemented on gear v1.11
        LOGIC_NEVER,
        LOGIC_NOTA_AND_NOTB,
        LOGIC_A_AND_NOTB,
        LOGIC_NOTB,
        LOGIC_NOTA_AND_B,
        LOGIC_NOTA,
        LOGIC_A_DIFF_B,
        LOGIC_NOTA_OR_NOTB,
        LOGIC_A_AND_B,
        LOGIC_A_EQ_B,
        LOGIC_A,
        LOGIC_A_OR_NOTB,
        LOGIC_B,
        LOGIC_NOTA_OR_B,
        LOGIC_A_OR_B,
        LOGIC_ALWAYS
    }

    public class FreqGenerator
    {
        private uint Control;
        private uint Frequency;
        private uint PhaseAccumulator;

        private CounterMode CtrMode;
        private ulong PinAMask;
        private ulong PinBMask;

        private Propeller Host;

        private bool PinA;
        private bool PinA_;
        private bool PinB;

        private bool OutA;
        private bool OutB;

        private bool FreqA; // THIS IS UGLY AND I HATE IT.
        private uint Divider;

        private PLLGroup PhaseLockLoop;

        public uint CTR
        {
            get
            {
                return Control;
            }
            set
            {
                Control = value;

                Divider = (uint)128 >> (int)((value >> 23) & 0x7);

                // The generators and outputs are
                OutA = OutB = false;

                PinAMask = (uint)1 << (int)(value & 0x3F);
                PinBMask = (uint)1 << (int)((value >> 9) & 0x3F);
                CtrMode = (CounterMode)((value & 0x7c000000) >> 26);

                // Setup PLL
                switch (CtrMode)
                {
                    case CounterMode.PLL_INTERNAL:
                        PhaseLockLoop.DrivePins(FreqA, 0, 0);
                        PhaseLockLoop.Feed(FreqA, FRQ / (double)0x100000000 * 16.0 / Divider);
                        break;
                    case CounterMode.PLL_SINGLE_ENDED:
                        PhaseLockLoop.DrivePins(FreqA, PinAMask, 0);
                        PhaseLockLoop.Feed(FreqA, FRQ / (double)0x100000000 * 16.0 / Divider);
                        break;
                    case CounterMode.PLL_DIFFERENTIAL:
                        PhaseLockLoop.DrivePins(FreqA, PinAMask, PinBMask);
                        PhaseLockLoop.Feed(FreqA, FRQ / (double)0x100000000 * 16.0 / Divider);
                        break;
                    default:
                        PhaseLockLoop.Disable(FreqA);
                        break;
                }
            }
        }

        public uint FRQ
        {
            get
            {
                return Frequency;
            }
            set
            {
                Frequency = value;

                switch (CtrMode)
                {
                    case CounterMode.PLL_INTERNAL:
                    case CounterMode.PLL_SINGLE_ENDED:
                    case CounterMode.PLL_DIFFERENTIAL:
                    case CounterMode.LOGIC_NEVER:       // ASB: do nothing for LOGIC_NEVER

                        // This is a special dejitter function
                        // The edge-sensitive system resulted in unstable
                        // output frequencies

                        PhaseLockLoop.Feed(FreqA, FRQ / (double)0x100000000 * 16.0 / Divider);
                        break;
                    default:
                        PhaseLockLoop.Disable(FreqA);
                        break;
                }
            }
        }

        public uint PHS
        {
            get
            {
                return PhaseAccumulator;
            }
            set
            {
                PhaseAccumulator = value;
            }
        }

        public ulong Output
        {
            get
            {
                return (OutA ? PinAMask : 0) | (OutB ? PinBMask : 0) | PhaseLockLoop.Pins;
            }
        }

        public FreqGenerator(Propeller host, PLLGroup phaseLockLoop, bool freqA)
        {
            Host = host;
            OutA = false;
            OutB = false;
            FreqA = freqA;

            PhaseLockLoop = phaseLockLoop;
        }

        public void SetClock(uint clock)
        {
            PhaseLockLoop.SetBaseFrequency(clock);
        }

        public void Tick(ulong pins)
        {
            switch (CtrMode)
            {
                case CounterMode.DISABLED:
                case CounterMode.PLL_INTERNAL:
                case CounterMode.PLL_SINGLE_ENDED:
                case CounterMode.PLL_DIFFERENTIAL:
                    break;
                case CounterMode.NCO_SINGLE_ENDED:
                    PHS += FRQ;
                    OutA = (PHS & 0x80000000) != 0;
                    break;
                case CounterMode.NCO_DIFFERENTIAL:
                    PHS += FRQ;
                    OutA = (PHS & 0x80000000) != 0;
                    OutB = !OutA;
                    break;
                case CounterMode.DUTY_SINGLE_ENDED:
                    {
                        long o = (long)PHS + (long)FRQ;

                        PHS = (uint)o;
                        OutA = o > 0xFFFFFFFF;
                    }
                    break;
                case CounterMode.DUTY_DIFFERENTIAL:
                    {
                        long o = (long)PHS + (long)FRQ;

                        PHS = (uint)o;
                        OutA = o > 0xFFFFFFFF;
                        OutB = !OutA;
                    }
                    break;
                case CounterMode.POS_DETECTOR:
                    if (PinA)
                        PHS += FRQ;
                    break;
                case CounterMode.POS_DETECTOR_FEEDBACK:
                    if (PinA)
                        PHS += FRQ;
                    OutB = !PinA;
                    break;
                case CounterMode.POSEDGE_DETECTOR:
                    if (PinA && !PinA_)
                        PHS += FRQ;
                    break;
                case CounterMode.POSEDGE_DETECTOR_FEEDBACK:
                    if (PinA && !PinA_)
                        PHS += FRQ;
                    OutB = !PinA;
                    break;
                case CounterMode.NEG_DETECTOR:
                    if (!PinA)
                        PHS += FRQ;
                    break;
                case CounterMode.NEG_DETECTOR_FEEDBACK:
                    if (!PinA)
                        PHS += FRQ;
                    OutB = !PinA;
                    break;
                case CounterMode.NEGEDGE_DETECTOR:
                    if (!PinA && PinA_)
                        PHS += FRQ;
                    break;
                case CounterMode.NEGEDGE_DETECTOR_FEEDBACK:
                    if (!PinA && PinA_)
                        PHS += FRQ;
                    OutB = !PinA;
                    break;
                default:
                    // ASB: changed to NOT ConditionCompare(.) to repair Logic Modes Counter
                    if (!Cog.ConditionCompare((CogConditionCodes)((int)CtrMode - 16), PinA, PinB))
                    {
                        PHS += FRQ;
                    }
                    break;
            }

            // Cycle in our previous pin states

            ulong Input = Host.IN;

            PinA_ = PinA;   // Delay by 2
            PinA = (Input & PinAMask) != 0;
            PinB = (Input & PinBMask) != 0;
        }
    }
}
