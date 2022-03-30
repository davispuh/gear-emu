/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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

using Gear.Propeller;

namespace Gear.EmulationCore
{
    /// <summary>
    /// States of a %Cog
    /// </summary>
    public enum CogRunState
    {
        /// @brief Waiting for instruction to finish executing.
        STATE_EXECUTE,
        /// @brief %Cog is loading program memory.
        WAIT_LOAD_PROGRAM,
        /// @brief %Cog is executing an instruction, and waiting an alloted amount of cycles
        WAIT_CYCLES,
        /// @brief Waits for an allotted number of cycles before changing to a new state
        WAIT_PREWAIT,

        /// @brief Interpreter is booting up
        BOOT_INTERPRETER,
        /// @brief Interpreter is executing an instruction
        WAIT_INTERPRETER,
        /// @brief Interpreter is fetching instruction
        EXEC_INTERPRETER,

        /// @brief Waits for pins to match
        WAIT_PEQ,
        /// @brief Waits for pins to NOT match
        WAIT_PNE,
        /// @brief Waits for count
        WAIT_CNT,
        /// @brief Waits for video
        WAIT_VID,

        /// @brief Waiting to read byte
        HUB_RDBYTE,
        /// @brief Waiting to read word
        HUB_RDWORD,
        /// @brief Waiting to read uint
        HUB_RDLONG,
        /// @brief Waiting to perform hub operation
        HUB_HUBOP
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FrameState
    {
        frameNone,
        frameHit,
        frameMiss
    }

    /// <summary>
    /// Base class for a %Cog emulator.
    /// </summary>
    abstract public partial class Cog
    {
        // Runtime variables
        protected uint[] Memory;            //!< %Cog Memory.

        protected PropellerCPU Hub;         //!< Host processor
        protected volatile uint PC;         //!< Program Cursor
        protected volatile int BreakPointCogCursor; //!< Breakpoint Address

        protected int StateCount;           //!< Argument for the current state
        protected CogRunState State;        //!< Current %Cog state
        protected CogRunState NextState;    //!< Next state %Cog state

        protected uint ProgramAddress;      //!< @todo Document member Cog.ProgramAddress
        protected uint ParamAddress;        //!< @todo Document member Cog.ParamAddress
        protected FrameState frameFlag;     //!< @brief Indicates video frame end and whether in WAIT_VID
        protected FrameState frameBreak;    //!< @brief Break if frameFlag higher

        protected FreqGenerator FreqA;      //!< @todo Document member Cog.FreqA
        protected FreqGenerator FreqB;      //!< @todo Document member Cog.FreqB
        protected VideoGenerator Video;     //!< @todo Document member Cog.Video
        protected PLLGroup PhaseLockedLoop; //!< @todo Document member Cog.PhaseLockedLoop

        /// @brief Total %Cog memory implemented on P1 Chip.
        public const int TOTAL_COG_MEMORY = 0x200;  // 512 longs of memory

        /// @brief Default constructor.
        public Cog(PropellerCPU host, uint programAddress, uint param, uint frequency, PLLGroup pll)
        {
            Hub = host;

            Memory = new uint[TOTAL_COG_MEMORY];
            ProgramAddress = programAddress;
            ParamAddress = param;

            FreqA = new FreqGenerator(host, pll, true);
            FreqB = new FreqGenerator(host, pll, false);
            Video = new VideoGenerator(host, this);
            PhaseLockedLoop = pll;

            // Attach the video generator to PLLs
            PhaseLockedLoop.SetupPLL(Video);

            PC = 0;
            BreakPointCogCursor = -1;    // Breakpoint disabled initially
            frameFlag = FrameState.frameNone;
            frameBreak = FrameState.frameMiss;

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

        /// @brief Break Point position in the program running in this cog.
        public int BreakPoint
        {
            get { return BreakPointCogCursor; }
            set { BreakPointCogCursor = value; }
        }

        /// @brief Get video frames count
        public uint VideoFrames
        {
            get { return Video.Frames; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string VideoFramesString
        {
            get
            {
                string hitState;
                Video.FrameCounters(out _, out uint framesCtr, out uint pixelsCtr);
                switch (frameFlag)
                {
                    case FrameState.frameHit:
                        hitState = "H";
                        break;
                    case FrameState.frameMiss:
                        hitState = "M";
                        break;
                    default:
                        hitState = " ";
                        break;
                }
                return string.Format("{0} {1:D4} {2:D3}", hitState, framesCtr, pixelsCtr);
            }
        }

        /// @brief Property to return complete OUT pins (P0..P63)
        /// @details Analyze all sources of pin changes in the cog: <c>OUTA</c>,
        /// <c>OUTB</c>, the two counters and the video generator.
        /// @version v22.03.02 - Bugfix corrected LONG registers not assigned
        /// correctly in %Cog - <c>DIR</c>, <c>OUT</c>.
        public ulong OUT
        {
            get
            {
                return Memory[(int)Assembly.RegisterAddress.OUTA] |
                    //Bugfix OUTB must be shifted on 64 bits long
                    ((ulong)Memory[(int)Assembly.RegisterAddress.OUTB] << 32) |
                    FreqA.Output |
                    FreqB.Output |
                    Video.Output;
            }
        }

        /// @brief Set video frame condition on which to break
        public FrameState VideoBreak
        {
            set
            {
                frameBreak = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string VideoStateString
        {
            get
            {
                switch (frameFlag)
                {
                    case FrameState.frameMiss: return " - Frame Miss";
                    case FrameState.frameHit: return " - Frame End";
                }
                return "";
            }
        }

        /// @brief Property to return only OUTA pins.
        /// Analyze all sources of pin changes in the cog for OUTA pins (P31..P0): the two 
        /// counters and the video generator.
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
        /// Analyze all sources of pin changes in the cog for OUTB pins (P63..P32): the 
        /// two counters and the video generator.
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

        /// @brief Property to return complete register of <c>DIR</c> pins
        /// @version v22.03.02 - Bugfix corrected LONG registers not assigned
        /// correctly in %Cog - <c>DIR</c>, <c>OUT</c>.
        public ulong DIR
        {
            get
            {
                //Bugfix DIRB must be shifted on 64 bits long
                return ((ulong)Memory[(int)Assembly.RegisterAddress.DIRB] << 32) |
                       Memory[(int)Assembly.RegisterAddress.DIRA];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public uint DIRA
        {
            get
            {
                return Memory[(int)Assembly.RegisterAddress.DIRA];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public uint DIRB
        {
            get
            {
                return Memory[(int)Assembly.RegisterAddress.DIRB];
            }
        }

        /// @brief The actual position where the program is executing in this cog.
        public uint ProgramCursor
        {
            get { return PC; }
            set { PC = value; }
        }

        /// @brief Show a string with human readable state of a cog.
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public uint this[int i]
        {
            get
            {
                if (i >= TOTAL_COG_MEMORY)
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
                if (i < TOTAL_COG_MEMORY)
                {
                    Memory[i] = value;
                }
            }
        }

        /// <summary>
        /// Evaluate a precondition for an PASM instruction
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public void DetachVideoHooks()
        {
            // Detach the video hook
            PhaseLockedLoop.Destroy();
            // Detach the aural hook
            Video.DetachAural();
        }

        /// @brief Execute a Step in the cog.
        public bool Step()
        {
            bool result = DoInstruction();
            // Run our frequency counters
            FreqA.Tick(Hub.IN);
            FreqB.Tick(Hub.IN);
            result = result && (frameFlag <= frameBreak);    // false - we hit a breakpoint
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="freq"></param>
        public void SetClock(uint freq)
        {
            FreqA.SetClock(freq);
            FreqB.SetClock(freq);
        }

        /// <summary>
        /// 
        /// </summary>
        public void StepInstruction()
        {
            int i = 0x2000;    // Maximum of 8k clocks (covers load instruction)
            do
            {
                Hub.Step();
            }
            while ( (State != CogRunState.EXEC_INTERPRETER) && 
                (State != CogRunState.STATE_EXECUTE && --i > 0) &&
                (frameFlag <= frameBreak));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public uint ReadLong(uint address)
        {
            // values using Assembly.RegisterAddress enum, instead of direct hex values
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

        /// @brief Write cog RAM with a specified value.
        /// @details This method take care of special cog address that in this class aren't 
        /// write in memory array Cog.Memory.
        /// @param address Address to write.
        /// @param data Data to write in address.
        /// @note PAR address is a special case, because unless Propeller Manual V1.2 
        /// specifications says it is a read-only register, there are claims that in reality it 
        /// is writable as explains 
        /// <a href="https://forums.parallax.com/discussion/comment/839684/#Comment_839684">
        /// Forum thread "PASM simulator / debugger?</a>.
        /// They claims that some parallax video drivers in PASM changes the PAR register, 
        /// and GEAR didn't emulate that.
        protected void WriteLong(uint address, uint data)
        {
            // values using Assembly.RegisterAddress enum, instead of direct hex values
            switch ((Assembly.RegisterAddress)(address & 0x1FF))
            {
                // Read only registers
                // case Assembly.RegisterAddress.PAR: // PAR register changed to writable
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

        /// @brief Execute a instruction in this cog.
        /// @returns TRUE if it is time to trigger a breakpoint, or FALSE if not.
        abstract public bool DoInstruction();

        /// <summary>
        /// 
        /// </summary>
        abstract public void Boot();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colours"></param>
        /// <param name="pixels"></param>
        abstract public void GetVideoData(out uint colours, out uint pixels);
    }
}
