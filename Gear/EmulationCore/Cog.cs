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

using Gear.Propeller;

/// @copydoc Gear.EmulationCore
namespace Gear.EmulationCore
{
    public enum CogRunState
    {
        STATE_EXECUTE,          //!< Waiting for instruction to finish executing

        WAIT_LOAD_PROGRAM,      //!< %Cog is loading program memory
        WAIT_CYCLES,            //!< %Cog is executing an instruction, and waiting an alloted ammount of cycles
        WAIT_PREWAIT,           //!< Waits for an allotted number of cycles before changing to a new state

        BOOT_INTERPRETER,       //!< Interpreter is booting up
        WAIT_INTERPRETER,       //!< Interpreter is executing an instruction
        EXEC_INTERPRETER,       //!< Interpreter is fetching instruction

        WAIT_PEQ,               //!< Waits for pins to match
        WAIT_PNE,               //!< Waits for pins to NOT match
        WAIT_CNT,               //!< Waits for count
        WAIT_VID,               //!< Waits for video

        HUB_RDBYTE,             //!< Waiting to read byte
        HUB_RDWORD,             //!< Waiting to read word
        HUB_RDLONG,             //!< Waiting to read uint
        HUB_HUBOP,              //!< Waiting to perform hub operation
    }

    /// @todo Document class Gear.EmulationCore.Cog. 
    /// 
    abstract public partial class Cog
    {
        // Runtime variables
        protected uint[] Memory;            //!< Program Memory

        protected PropellerCPU Hub;         //!< Host processor
        protected volatile uint PC;         //!< Program Cursor
        protected volatile int BP;          //!< Breakpoint Address

        protected int StateCount;           //!< Arguement for the current state
        protected CogRunState State;        //!< Current COG state
        protected CogRunState NextState;    //!< Next state COG state

        protected uint ProgramAddress;      //!< @todo Document member Cog.ProgramAddress
        protected uint ParamAddress;        //!< @todo Document member Cog.ParamAddress

        protected FreqGenerator FreqA;      //!< @todo Document member Cog.FreqA
        protected FreqGenerator FreqB;      //!< @todo Document member Cog.FreqB
        protected VideoGenerator Video;     //!< @todo Document member Cog.Video
        protected PLLGroup PhaseLockedLoop; //!< @todo Document member Cog.PhaseLockedLoop

        /// @todo Document constructor Gear.EmulationCore.Cog()
        /// 
        public Cog(PropellerCPU host, uint programAddress, uint param, uint frequency, PLLGroup pll)
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
            Memory[(int)Assembly.RegisterAddress.PAR] = param;
            State = CogRunState.WAIT_LOAD_PROGRAM;
            StateCount = 0;

            // Clear the special purpose registers
            for (int i = (int)Assembly.RegisterAddress.CNT; i <= 0x1FF; i++)
            {
                this[i] = 0;
            }

            SetClock(frequency);
        }

        /// @todo Document property Gear.EmulationCore.Cog.BreakPoint
        /// 
        public int BreakPoint
        {
            get { return BP; }
            set { BP = value; }
        }

        /// @brief Property to return complete OUT pins (P0..P63)
        /// Analyze all sources of pin changes in the cog: OUTA, OUTB, the two counters 
        /// and the video generator.
        public ulong OUT
        {
            get
            {
                return Memory[(int)Assembly.RegisterAddress.OUTA] |
                    (Memory[(int)Assembly.RegisterAddress.OUTB] << 32) |
                    FreqA.Output |
                    FreqB.Output |
                    Video.Output;
            }
        }

        /// @brief Property to return only OUTA pins.
        /// Analyze all sources of pin changes in the cog for OUTA pins (P31..P0): the two counters 
        /// and the video generator.
        public uint OUTA
        {
            get
            {
                return Memory[(int)Assembly.RegisterAddress.OUTA] |
                    ((uint)FreqA.Output |
                    (uint)FreqB.Output |
                    (uint)Video.Output);
            }
        }

        /// @brief Property to return only OUTB pins.
        /// Analyze all sources of pin changes in the cog for OUTB pins (P63..P32): the two counters 
        /// and the video generator.
        public uint OUTB
        {
            get
            {
                return
                    (uint)((FreqA.Output |
                    FreqB.Output |
                    Video.Output) >> 32) |
                    Memory[(int)Assembly.RegisterAddress.OUTB];
            }
        }

        /// @todo Document property Gear.EmulationCore.Cog.DIR.
        /// 
        public ulong DIR
        {
            get
            {
                return (Memory[(int)Assembly.RegisterAddress.DIRB] << 32) |
                    Memory[(int)Assembly.RegisterAddress.DIRA];
            }
        }

        /// @todo Document property Gear.EmulationCore.Cog.DIRA.
        /// 
        public uint DIRA
        {
            get
            {
                return Memory[(int)Assembly.RegisterAddress.DIRA];
            }
        }

        /// @todo Document property Gear.EmulationCore.Cog.DIRB.
        /// 
        public uint DIRB
        {
            get
            {
                return Memory[(int)Assembly.RegisterAddress.DIRB];
            }
        }

        /// @todo Document property Gear.EmulationCore.Cog.ProgramCursor.
        /// 
        public uint ProgramCursor
        {
            get { return PC; }
            set { PC = value; }
        }

        /// @todo Document property Gear.EmulationCore.Cog.CogState.
        /// 
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

        /// @todo Document property Gear.EmulationCore.Cog.operator[].
        /// 
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
                    // show special registers, because their values are in
                    // variables in Cog object and not in memory array.
                    if (i >= (int)Assembly.RegisterAddress.PAR)
                    {
                        return ReadLong((uint)i);
                    }
                    else
                    {
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

        /// @todo Document method Gear.EmulationCore.Cog.ConditionCompare.
        /// 
        public static bool ConditionCompare(Assembly.ConditionCodes condition, bool a, bool b)
        {
            switch (condition)
            {
                case Assembly.ConditionCodes.IF_NEVER:
                    break;
                case Assembly.ConditionCodes.IF_NZ_AND_NC:
                    if (!a && !b) return false;
                    break;
                case Assembly.ConditionCodes.IF_NC_AND_Z:
                    if (a && !b) return false;
                    break;
                case Assembly.ConditionCodes.IF_NC:
                    if (!b) return false;
                    break;
                case Assembly.ConditionCodes.IF_C_AND_NZ:
                    if (!a && b) return false;
                    break;
                case Assembly.ConditionCodes.IF_NZ:
                    if (!a) return false;
                    break;
                case Assembly.ConditionCodes.IF_C_NE_Z:
                    if (a != b) return false;
                    break;
                case Assembly.ConditionCodes.IF_NC_OR_NZ:
                    if (!a || !b) return false;
                    break;
                case Assembly.ConditionCodes.IF_C_AND_Z:
                    if (a && b) return false;
                    break;
                case Assembly.ConditionCodes.IF_C_EQ_Z:
                    if (a == b) return false;
                    break;
                case Assembly.ConditionCodes.IF_Z:
                    if (a) return false;
                    break;
                case Assembly.ConditionCodes.IF_NC_OR_Z:
                    if (a || !b) return false;
                    break;
                case Assembly.ConditionCodes.IF_C:
                    if (b) return false;
                    break;
                case Assembly.ConditionCodes.IF_C_OR_NZ:
                    if (!a || b) return false;
                    break;
                case Assembly.ConditionCodes.IF_Z_OR_C:
                    if (a || b) return false;
                    break;
                case Assembly.ConditionCodes.IF_ALWAYS:
                    return false;
            }

            return true;
        }

        /// @todo Document method Gear.EmulationCore.Cog.HubAccessable().
        /// 
        public virtual void HubAccessable()
        {
            switch (State)
            {
                case CogRunState.WAIT_LOAD_PROGRAM:
                    Memory[StateCount++] = Hub.DirectReadLong(ProgramAddress);
                    ProgramAddress += 4;

                    if (StateCount == 0x1F0)
                    {
                        StateCount = 0;
                        Boot();
                    }
                    break;
            }
        }

        /// @todo Document method Gear.EmulationCore.Cog.DetachVideoHooks()
        /// 
        public void DetachVideoHooks()
        {
            // Detach the video hook
            PhaseLockedLoop.Destroy();
            // Detach the aural hook
            Video.DetachAural();
        }

        /// @todo Document method Gear.EmulationCore.Cog.Step
        /// 
        public bool Step()
        {
            bool result = DoInstruction();
            // Run our frequency counters
            FreqA.Tick(Hub.IN);
            FreqB.Tick(Hub.IN);
            return result;    // false - we hit a breakpoint
        }

        /// @todo Document method Gear.EmulationCore.Cog.SetClock().
        /// 
        public void SetClock(uint freq)
        {
            FreqA.SetClock(freq);
            FreqB.SetClock(freq);
        }

        /// @todo Document method Gear.EmulationCore.Cog.StepInstruction().
        public void StepInstruction()
        {
            int i = 0x2000;    // Maximum of 8k clocks (covers load instruction)
            do
            {
                Hub.Step();
            }
            while (State != CogRunState.EXEC_INTERPRETER && State != CogRunState.STATE_EXECUTE && --i > 0);
        }

        /// @todo Document method Gear.EmulationCore.Cog.ReadLong().
        /// 
        public uint ReadLong(uint address)
        {
            // values using CogSpecialAddress enum, intead of direct hex values
            switch ((Assembly.RegisterAddress)(address & 0x1FF))
            {
                case Assembly.RegisterAddress.CNT:
                    return Hub.Counter;
                case Assembly.RegisterAddress.INA:
                    return Hub.INA;
                case Assembly.RegisterAddress.INB:
                    return Hub.INB;
                case Assembly.RegisterAddress.CNTA:
                    return FreqA.CTR;
                case Assembly.RegisterAddress.CNTB:
                    return FreqB.CTR;
                case Assembly.RegisterAddress.FRQA:
                    return FreqA.FRQ;
                case Assembly.RegisterAddress.FRQB:
                    return FreqB.FRQ;
                case Assembly.RegisterAddress.PHSA:
                    return FreqA.PHS;
                case Assembly.RegisterAddress.PHSB:
                    return FreqB.PHS;
                case Assembly.RegisterAddress.VCFG:
                    return Video.CFG;
                case Assembly.RegisterAddress.VSCL:
                    return Video.SCL;
                default:
                    return Memory[address & 0x1FF];
            }
        }

        /// @brief Write cog RAM with a specified value
        /// This method take care of special cog address that in this class aren't writed in memory array Cog.Memory.
        /// @param[in] address Address to write
        /// @param[in] data Data to write in address
        /// @note PAR address is a special case, because unless Propeller Manual V1.2 specifications says it is a 
        /// read-only register, there are claims that in reality it is writeable as explains
        /// <a href="http://forums.parallax.com/showthread.php/115909-PASM-simulator-debugger)">Forum thread "PASM simulator / debugger?</a>.
        /// They claims that some parallax video drivers in PASM changes the PAR register, and GEAR didn't emulate that.
        protected void WriteLong(uint address, uint data)
        {
            // values using CogSpecialAddress enum, intead of direct hex values
            switch ((Assembly.RegisterAddress)(address & 0x1FF))
            {
                // Read only registers
                // case CogSpecialAddress.PAR: // PAR register changed to writeable
                case Assembly.RegisterAddress.CNT:
                case Assembly.RegisterAddress.INA:
                case Assembly.RegisterAddress.INB:
                    return;
                case Assembly.RegisterAddress.CNTA:
                    FreqA.CTR = data;
                    break;
                case Assembly.RegisterAddress.CNTB:
                    FreqB.CTR = data;
                    break;
                case Assembly.RegisterAddress.FRQA:
                    FreqA.FRQ = data;
                    break;
                case Assembly.RegisterAddress.FRQB:
                    FreqB.FRQ = data;
                    break;
                case Assembly.RegisterAddress.PHSA:
                    FreqA.PHS = data;
                    break;
                case Assembly.RegisterAddress.PHSB:
                    FreqB.PHS = data;
                    break;
                case Assembly.RegisterAddress.VCFG:
                    Video.CFG = data;
                    break;
                case Assembly.RegisterAddress.VSCL:
                    Video.SCL = data;
                    break;
                default:
                    Memory[address & 0x1FF] = data;
                    return;
            }
        }

        /// @todo Document method Gear.EmulationCore.Cog.DoInstruction()
        abstract public bool DoInstruction();

        /// @todo Document method Gear.EmulationCore.Cog.Boot()
        abstract public void Boot();
    }
}
