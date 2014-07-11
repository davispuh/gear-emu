/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * Cog.CS
 * Base class for a cog processor.  Abstract and must be extended.
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
    public enum CogRunState
    {
        STATE_EXECUTE,          // Waiting for instruction to finish executing

        WAIT_LOAD_PROGRAM,      // Cog is loading program memory
        WAIT_CYCLES,            // Cog is executing an instruction, and waiting an alloted ammount of cycles
        WAIT_PREWAIT,           // Waits for an allotted number of cycles before changing to a new state

        BOOT_INTERPRETER,       // Interpreter is booting up
        WAIT_INTERPRETER,       // Interpreter is executing an instruction
        EXEC_INTERPRETER,       // Interpreter is fetching instruction

        WAIT_PEQ,               // Waits for pins to match
        WAIT_PNE,               // Waits for pins to NOT match
        WAIT_CNT,               // Waits for count
        WAIT_VID,               // Waits for video

        HUB_RDBYTE,             // Waiting to read byte
        HUB_RDWORD,             // Waiting to read word
        HUB_RDLONG,             // Waiting to read uint
        HUB_HUBOP,              // Waiting to perform hub operation
    }

    public enum CogSpecialAddress : uint
    {
        COGID       = 0x1E9,
        INITCOGID   = 0x1EF,
        PAR         = 0x1F0,
        CNT         = 0x1F1,
        INA         = 0x1F2,
        INB         = 0x1F3,
        OUTA        = 0x1F4,
        OUTB        = 0x1F5,
        DIRA        = 0x1F6,
        DIRB        = 0x1F7,
        CNTA        = 0x1F8,
        CNTB        = 0x1F9,
        FRQA        = 0x1FA,
        FRQB        = 0x1FB,
        PHSA        = 0x1FC,
        PHSB        = 0x1FD,
        VCFG        = 0x1FE,
        VSCL        = 0x1FF
    }

    public enum CogConditionCodes : uint
    {
        IF_NEVER        = 0x00,
        IF_A            = 0x01,
        IF_NC_AND_NZ    = 0x01,
        IF_NZ_AND_NC    = 0x01,
        IF_NC_AND_Z     = 0x02,
        IF_Z_AND_NC     = 0x02,
        IF_NC           = 0x03,
        IF_AE           = 0x03,
        IF_NZ_AND_C     = 0x04,
        IF_C_AND_NZ     = 0x04,
        IF_NZ           = 0x05,
        IF_NE           = 0x05,
        IF_C_NE_Z       = 0x06,
        IF_Z_NE_C       = 0x06,
        IF_NC_OR_NZ     = 0x07,
        IF_NZ_OR_NC     = 0x07,
        IF_C_AND_Z      = 0x08,
        IF_Z_AND_C      = 0x08,
        IF_C_EQ_Z       = 0x09,
        IF_Z_EQ_C       = 0x09,
        IF_E            = 0x0A,
        IF_Z            = 0x0A,
        IF_NC_OR_Z      = 0x0B,
        IF_Z_OR_NC      = 0x0B,
        IF_B            = 0x0C,
        IF_C            = 0x0C,
        IF_NZ_OR_C      = 0x0D,
        IF_C_OR_NZ      = 0x0D,
        IF_Z_OR_C       = 0x0E,
        IF_BE           = 0x0E,
        IF_C_OR_Z       = 0x0E,
        IF_ALWAYS       = 0x0F
    }

    abstract public partial class Cog
    {
        // Runtime variables
        protected uint[] Memory;            // Program Memory

        protected Propeller Hub;            // Host processor
        protected volatile uint PC;         // Program Cursor
        protected volatile int BP;          // Breakpoint Address

        protected int StateCount;           // Arguement for the current state
        protected CogRunState State;        // Current COG state
        protected CogRunState NextState;    // Next state COG state

        protected uint ProgramAddress;
        protected uint ParamAddress;

        protected FreqGenerator FreqA;
        protected FreqGenerator FreqB;
        protected VideoGenerator Video;
        protected PLLGroup PhaseLockedLoop;

        public Cog(Propeller host, uint programAddress, uint param, uint frequency, PLLGroup pll)
        {
            Hub = host;

            Memory = new uint[0x200];     // 512 longs of memory
            ProgramAddress = programAddress;
            ParamAddress = param;

            FreqA = new FreqGenerator(host, pll, true);
            FreqB = new FreqGenerator(host, pll, false);
            Video = new VideoGenerator(host);
            PhaseLockedLoop = pll;

            // Attach the video generator to PLLs
            PhaseLockedLoop.SetupPLL(Video);

            PC = 0;
            BP = -1;    // Breakpoint disabled

            // We are in boot time load
            Memory[(int)CogSpecialAddress.PAR] = param;
            State = CogRunState.WAIT_LOAD_PROGRAM;
            StateCount = 0;

            // Clear the special purpose registers
            for (int i = (int)CogSpecialAddress.CNT; i <= 0x1FF; i++)
            {
                this[i] = 0;
            }

            SetClock(frequency);
        }

        public int BreakPoint
        {
            get { return BP; }
            set { BP = value; }
        }

        public ulong OUT
        {
            get
            {
                return Memory[(int)CogSpecialAddress.OUTA] |
                    (Memory[(int)CogSpecialAddress.OUTB] << 32) |
                    FreqA.Output |
                    FreqB.Output |
                    Video.Output;
            }
        }

        public uint OUTA
        {
            get
            {
                return Memory[(int)CogSpecialAddress.OUTA] |
                    ((uint)FreqA.Output |
                    (uint)FreqB.Output |
                    (uint)Video.Output);
            }
        }

        public uint OUTB
        {
            get
            {
                return
                    (uint)((FreqA.Output |
                    FreqB.Output |
                    Video.Output) >> 32) |
                    Memory[(int)CogSpecialAddress.OUTB];
            }
        }

        public ulong DIR
        {
            get
            {
                return (Memory[(int)CogSpecialAddress.DIRB] << 32) |
                    Memory[(int)CogSpecialAddress.DIRA];
            }
        }

        public uint DIRA
        {
            get
            {
                return Memory[(int)CogSpecialAddress.DIRA];
            }
        }

        public uint DIRB
        {
            get
            {
                return Memory[(int)CogSpecialAddress.DIRB];
            }
        }

        public uint ProgramCursor
        {
            get { return PC; }
            set { PC = value; }
        }

        public string CogState
        {
            get
            {
                switch (State)
                {
                    case CogRunState.HUB_HUBOP:
                    case CogRunState.HUB_RDBYTE:
                    case CogRunState.HUB_RDWORD:
                    case CogRunState.HUB_RDLONG:
                        return "Waiting for hub";
                    case CogRunState.BOOT_INTERPRETER:
                        return "Interpreter Boot";
                    case CogRunState.EXEC_INTERPRETER:
                        return "Interpreter Fetch";
                    case CogRunState.WAIT_INTERPRETER:
                        return "Interpreter Processing";
                    case CogRunState.STATE_EXECUTE:
                    case CogRunState.WAIT_PREWAIT:
                    case CogRunState.WAIT_CYCLES:
                        return "Running instruction";
                    case CogRunState.WAIT_LOAD_PROGRAM:
                        return "Loading Program";
                    case CogRunState.WAIT_CNT:
                        return "Waiting (CNT)";
                    case CogRunState.WAIT_PEQ:
                        return "Waiting (PEQ)";
                    case CogRunState.WAIT_PNE:
                        return "Waiting (PNE)";
                    case CogRunState.WAIT_VID:
                        return "Waiting (video)";
                    default:
                        return "ERROR";
                }
            }
        }

        public uint this[int i]
        {
            get
            {
                if (i >= 0x200)
                {
                    return 0x55;
                }
                else
                {
                    // ASB: change to show special registers, because their values are in
                    // variables in Cog object and not in memory array.
                    if (i >= (int)CogSpecialAddress.PAR)
                    {
                        return ReadLong((uint)i);
                    }
                    else
                    {
                        // ASB: end of change
                        return Memory[i];
                    }
                }
            }

            set
            {
                if (i < 0x200)
                {
                    Memory[i] = value;
                }
            }
        }

        public static bool ConditionCompare(CogConditionCodes condition, bool a, bool b)
        {
            switch (condition)
            {
                case CogConditionCodes.IF_NEVER:
                    break;
                case CogConditionCodes.IF_NZ_AND_NC:
                    if (!a && !b) return false;
                    break;
                case CogConditionCodes.IF_NC_AND_Z:
                    if (a && !b) return false;
                    break;
                case CogConditionCodes.IF_NC:
                    if (!b) return false;
                    break;
                case CogConditionCodes.IF_C_AND_NZ:
                    if (!a && b) return false;
                    break;
                case CogConditionCodes.IF_NZ:
                    if (!a) return false;
                    break;
                case CogConditionCodes.IF_C_NE_Z:
                    if (a != b) return false;
                    break;
                case CogConditionCodes.IF_NC_OR_NZ:
                    if (!a || !b) return false;
                    break;
                case CogConditionCodes.IF_C_AND_Z:
                    if (a && b) return false;
                    break;
                case CogConditionCodes.IF_C_EQ_Z:
                    if (a == b) return false;
                    break;
                case CogConditionCodes.IF_Z:
                    if (a) return false;
                    break;
                case CogConditionCodes.IF_NC_OR_Z:
                    if (a || !b) return false;
                    break;
                case CogConditionCodes.IF_C:
                    if (b) return false;
                    break;
                case CogConditionCodes.IF_C_OR_NZ:
                    if (!a || b) return false;
                    break;
                case CogConditionCodes.IF_Z_OR_C:
                    if (a || b) return false;
                    break;
                case CogConditionCodes.IF_ALWAYS:
                    return false;
            }

            return true;
        }

        public virtual void HubAccessable()
        {
            switch (State)
            {
                case CogRunState.WAIT_LOAD_PROGRAM:
                    Memory[StateCount++] = Hub.ReadLong(ProgramAddress);
                    ProgramAddress += 4;

                    if (StateCount == 0x1F0)
                    {
                        StateCount = 0;
                        Boot();
                    }
                    break;
            }
        }

        public void DetachVideoHooks()
        {
            // Detach the video hook
            PhaseLockedLoop.Destroy();
            // Detach the aural hook
            Video.DetachAural();
        }

        public bool Step()
        {
            bool result = DoInstruction();
            // Run our frequency counters
            FreqA.Tick(Hub.IN);
            FreqB.Tick(Hub.IN);
            return result;    // false - we hit a breakpoint
        }

        public void SetClock(uint freq)
        {
            FreqA.SetClock(freq);
            FreqB.SetClock(freq);
        }

        public void StepInstruction()
        {
            int i = 0x2000;    // Maximum of 8k clocks (covers load instruction)
            do
            {
                Hub.Step();
            }
            while (State != CogRunState.EXEC_INTERPRETER && State != CogRunState.STATE_EXECUTE && --i > 0);
        }

        public uint ReadLong(uint address)
        {
            // ASB: changed case values to use CogSpecialAddress enum, intead of direct hex values
            // ASB: added (cast) to use CogSpecialAddress enum
            switch ((CogSpecialAddress)(address & 0x1FF))
            {
                case CogSpecialAddress.CNT:
                    return Hub.Counter;
                case CogSpecialAddress.INA:
                    return Hub.INA;
                case CogSpecialAddress.INB:
                    return Hub.INB;
                case CogSpecialAddress.CNTA:
                    return FreqA.CTR;
                case CogSpecialAddress.CNTB:
                    return FreqB.CTR;
                case CogSpecialAddress.FRQA:
                    return FreqA.FRQ;
                case CogSpecialAddress.FRQB:
                    return FreqB.FRQ;
                case CogSpecialAddress.PHSA:
                    return FreqA.PHS;
                case CogSpecialAddress.PHSB:
                    return FreqB.PHS;
                case CogSpecialAddress.VCFG:
                    return Video.CFG;
                case CogSpecialAddress.VSCL:
                    return Video.SCL;
                default:
                    return Memory[address & 0x1FF];
            }
        }

        protected void WriteLong(uint address, uint data)
        {
            // ASB: changed case values to use CogSpecialAddress enum, intead of direct hex values
            // ASB: added (cast) to use CogSpecialAddress enum
            switch ((CogSpecialAddress)(address & 0x1FF))
            {
                // Read only registers
                // case CogSpecialAddress.PAR: // ASB: PAR register changed to writeable
                case CogSpecialAddress.CNT:
                case CogSpecialAddress.INA:
                case CogSpecialAddress.INB:
                    return;
                case CogSpecialAddress.CNTA:
                    FreqA.CTR = data;
                    break;
                case CogSpecialAddress.CNTB:
                    FreqB.CTR = data;
                    break;
                case CogSpecialAddress.FRQA:
                    FreqA.FRQ = data;
                    break;
                case CogSpecialAddress.FRQB:
                    FreqB.FRQ = data;
                    break;
                case CogSpecialAddress.PHSA:
                    FreqA.PHS = data;
                    break;
                case CogSpecialAddress.PHSB:
                    FreqB.PHS = data;
                    break;
                case CogSpecialAddress.VCFG:
                    Video.CFG = data;
                    break;
                case CogSpecialAddress.VSCL:
                    Video.SCL = data;
                    break;
                default:
                    Memory[address & 0x1FF] = data;
                    return;
            }
        }

        abstract public bool DoInstruction();
        abstract public void Boot();
    }
}
