/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. PropellerCPU Debugger
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * NativeCog.cs
 * Object class for a native assembly (machine code) cog
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

    /// @brief Derived class from Cog, to emulate running PASM code.
    public partial class NativeCog : Cog
    {
        // Decode fields

        protected uint Operation;
        protected Assembly.InstructionCodes InstructionCode;
        protected Assembly.ConditionCodes ConditionCode;

        protected bool WriteZero;
        protected bool WriteCarry;
        protected bool WriteResult;
        protected bool ImmediateValue;

        protected uint SourceValue;
        protected uint Destination;
        protected uint DestinationValue;

        // Encode fields

        protected uint DataResult;
        protected bool CarryResult;
        protected bool ZeroResult;

        protected bool Carry;               //!< Carry Flag
        protected bool Zero;                //!< Zero Flag

        public bool CarryFlag
        {
            get { return Carry; }
            set { Carry = value; }
        }

        public bool ZeroFlag
        {
            get { return Zero; }
            set { Zero = value; }
        }

        /// @brief Default constructor for a Cog running in PASM mode.
        /// @param host PropellerCPU where this cog resides.
        /// @param programAddress Start of program to load from main memory.
        /// @param paramAddress PARAM value given to the Cog.
        /// @param frequency Frequency running the cog (the same as the Propeller).
        /// @param pll PLL Multiplier running the cog (the same as the Propeller).
        public NativeCog(PropellerCPU host,
            uint programAddress, uint paramAddress, uint frequency,
            PLLGroup pll)
            : base(host, programAddress, paramAddress, frequency, pll)
        {
            Carry = false;
            Zero = false;
        }

        /// @brief Determine what effect will be executed after this operation.
        /// @details The possibles are Write Result, Zero flag or Carry flag, or mix between them.
        private void WriteBackResult()
        {
            if (WriteResult)
                WriteLong(Destination, DataResult);
            if (WriteZero)
                Zero = ZeroResult;
            if (WriteCarry)
                Carry = CarryResult;

            State = CogRunState.STATE_EXECUTE;
        }

        /// @brief Setup the cog to a initial state after boot it.
        public override void Boot()
        {
            State = CogRunState.STATE_EXECUTE;
            Carry = false;
            Zero = false;

            PC = 0;
            // Prefetch first instruction
            Operation = ReadLong(0);
        }

        /// @brief Determine if the Hub is accessible in that moment of time, setting the state 
        /// accordantly.
        /// @version v15.03.26 - corrected zero and carry values for missing HUBOPS.
        public override void HubAccessable()
        {
            switch (State)
            {
                case CogRunState.HUB_HUBOP:
                    DataResult = Hub.HubOp(this, SourceValue, DestinationValue, ref CarryResult, 
                        ref ZeroResult);
                    WriteBackResult();
                    return;

                case CogRunState.HUB_RDBYTE:
                    if (WriteResult)
                        DataResult = Hub.DirectReadByte(SourceValue);
                    else
                        Hub.DirectWriteByte(SourceValue, (byte)DestinationValue);
                    ZeroResult = (DataResult == 0); //set as state Manual v1.2
                    // Don't affect Carry as state Manual v1.2
                    WriteBackResult();
                    return;

                case CogRunState.HUB_RDWORD:
                    if (WriteResult)
                        DataResult = Hub.DirectReadWord(SourceValue);
                    else
                        Hub.DirectWriteWord(SourceValue, (ushort)DestinationValue);
                    ZeroResult = (DataResult == 0); //set as state Manual v1.2
                    // Don't affect Carry as state Manual v1.2
                    WriteBackResult();
                    return;
                case CogRunState.HUB_RDLONG:
                    if (WriteResult)
                        DataResult = Hub.DirectReadLong(SourceValue);
                    else
                        Hub.DirectWriteLong(SourceValue, DestinationValue);
                    ZeroResult = (DataResult == 0);  //set as state Manual v1.2
                    // Don't affect Carry as state Manual v1.2
                    WriteBackResult();
                    return;
            }

            base.HubAccessable();
        }

        /// @brief Execute a PASM instruction in this cog.
        /// @returns TRUE if it is the opportunity to trigger a breakpoint, or FALSE if not.
        override public bool DoInstruction()
        {
            switch (State)
            {
                // Delay State
                case CogRunState.WAIT_CYCLES:
                    if (--StateCount == 1)
                        WriteBackResult();
                    return true;
                // Clocked, but not executed
                case CogRunState.WAIT_PREWAIT:
                    if (--StateCount == 1)
                        State = NextState;
                    return true;

                // Execute State
                case CogRunState.STATE_EXECUTE:
                    break;

                case CogRunState.WAIT_PNE:
                    {
                        uint maskedIn = (Carry ? Hub.INB : Hub.INA) & SourceValue;
                        if (maskedIn != DestinationValue)
                        {
                            DataResult = maskedIn;
                            Zero = (maskedIn == 0);
                            // TODO: DETERMINE CARRY
                            WriteBackResult();
                        }
                        return true;
                    }
                case CogRunState.WAIT_PEQ:
                    {
                        uint maskedIn = (Carry ? Hub.INB : Hub.INA) & SourceValue;
                        if (maskedIn == DestinationValue)
                        {
                            DataResult = maskedIn;
                            Zero = (maskedIn == 0);
                            // TODO: DETERMINE CARRY
                            WriteBackResult();
                            return true;
                        }
                        return true;
                    }
                case CogRunState.WAIT_CNT:
                    {
                        long target = Hub.Counter;

                        if (DestinationValue == target)
                        {
                            target += SourceValue;
                            DataResult = (uint)target;
                            CarryResult = target > 0xFFFFFFFF;
                            ZeroResult = (DataResult == 0);
                            WriteBackResult();
                            return true;
                        }
                        return true;
                    }
                case CogRunState.WAIT_VID:
                    // Logic Changed: GetVideoData now clears VAIT_VID state
                    // if (Video.Ready)
                    // {
                    //     Video.Feed(DestinationValue, SourceValue);
                    //     // TODO: Determine carry, zero, and result
                    //     WriteBackResult();
                    // }
                    return true;

                // Non-execute states are ignored
                default:
                    return true;
            }

            PC = (PC + 1) & 0x1FF;

            frameFlag = FrameState.frameNone;

            InstructionCode = (Assembly.InstructionCodes)(Operation & 0xFC000000);
            ConditionCode = (Assembly.ConditionCodes)((Operation & 0x003C0000) >> 18);

            WriteZero = (Operation & 0x02000000) != 0;
            WriteCarry = (Operation & 0x01000000) != 0;
            WriteResult = (Operation & 0x00800000) != 0;
            ImmediateValue = (Operation & 0x00400000) != 0;

            SourceValue = Operation & 0x1FF;

            if (!ImmediateValue)
                SourceValue = ReadLong(SourceValue);

            Destination = (Operation >> 9) & 0x1FF;
            DestinationValue = ReadLong(Destination);

            if (ConditionCompare(ConditionCode, Zero, Carry))
            {
                Operation = ReadLong(PC);
                State = CogRunState.WAIT_PREWAIT;
                NextState = CogRunState.STATE_EXECUTE;
                StateCount = 4;
                return true;
            }

            switch (InstructionCode)
            {
                // RESERVED FOR FUTURE EXPANSION
                case Assembly.InstructionCodes.MUL:
                case Assembly.InstructionCodes.MULS:
                case Assembly.InstructionCodes.ENC:
                case Assembly.InstructionCodes.ONES:
                    // TODO: RAISE INVALID OP CODE HERE

                    DataResult = 0;
                    Carry = true;
                    Zero = true;

                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                // HUB Operations (6 cycles for instruction decode... 1-15 cycles for operation)
                case Assembly.InstructionCodes.RWBYTE:
                    State = CogRunState.WAIT_PREWAIT;
                    NextState = CogRunState.HUB_RDBYTE;
                    StateCount = 6;
                    break;
                case Assembly.InstructionCodes.RWWORD:
                    State = CogRunState.WAIT_PREWAIT;
                    NextState = CogRunState.HUB_RDWORD;
                    StateCount = 6;
                    break;
                case Assembly.InstructionCodes.RWLONG:
                    State = CogRunState.WAIT_PREWAIT;
                    NextState = CogRunState.HUB_RDLONG;
                    StateCount = 6;
                    break;
                case Assembly.InstructionCodes.HUBOP:
                    State = CogRunState.WAIT_PREWAIT;
                    NextState = CogRunState.HUB_HUBOP;
                    StateCount = 6;
                    break;

                // Standard operations (4 cycles)
                case Assembly.InstructionCodes.ROR:
                    InstructionROR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ROL:
                    InstructionROL();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.RCR:
                    InstructionRCR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.RCL:
                    InstructionRCL();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SHR:
                    InstructionSHR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SHL:
                    InstructionSHL();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SAR:
                    InstructionSAR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.OR:
                    InstructionOR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.XOR:
                    InstructionXOR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.AND:
                    InstructionAND();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ANDN:
                    InstructionANDN();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.MUXC:
                    InstructionMUXC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MUXNC:
                    InstructionMUXNC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MUXZ:
                    InstructionMUXZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MUXNZ:
                    InstructionMUXNZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.REV:
                    InstructionREV();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.NEG:
                    InstructionNEG();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ABS:
                    InstructionABS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ABSNEG:
                    InstructionABSNEG();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.NEGC:
                    InstructionNEGC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.NEGNC:
                    InstructionNEGNC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.NEGZ:
                    InstructionNEGZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.NEGNZ:
                    InstructionNEGNZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.MOV:
                    InstructionMOV();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MOVS:
                    InstructionMOVS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MOVD:
                    InstructionMOVD();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MOVI:
                    InstructionMOVI();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.JMPRET:
                    InstructionJMPRET();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.MINS:
                    InstructionMINS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MAXS:
                    InstructionMAXS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MIN:
                    InstructionMIN();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MAX:
                    InstructionMAX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.ADD:
                    InstructionADD();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ADDABS:
                    InstructionADDABS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ADDX:
                    InstructionADDX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ADDS:
                    InstructionADDS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ADDSX:
                    InstructionADDSX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.SUB:
                    InstructionSUB();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUBABS:
                    InstructionSUBABS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUBX:
                    InstructionSUBX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUBS:
                    InstructionSUBS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUBSX:
                    InstructionSUBSX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.SUMC:
                    InstructionSUMC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUMNC:
                    InstructionSUMNC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUMZ:
                    InstructionSUMZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUMNZ:
                    InstructionSUMNZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.CMPS:
                    InstructionCMPS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.CMPSX:
                    InstructionCMPSX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.CMPSUB:
                    InstructionCMPSUB();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                // Decrement and continue instructions ( 4/8 cycles )
                case Assembly.InstructionCodes.DJNZ:
                    StateCount = InstructionDJNZ() ? 4 : 8;
                    State = CogRunState.WAIT_CYCLES;
                    break;
                case Assembly.InstructionCodes.TJNZ:
                    StateCount = InstructionTJNZ() ? 4 : 8;
                    State = CogRunState.WAIT_CYCLES;
                    break;
                case Assembly.InstructionCodes.TJZ:
                    StateCount = InstructionTJZ() ? 4 : 8;
                    State = CogRunState.WAIT_CYCLES;
                    break;

                // Delay execution instructions ( 4 cycles for instruction decode, 1+ for instruction )
                case Assembly.InstructionCodes.WAITPEQ:
                    NextState = CogRunState.WAIT_PEQ;
                    State = CogRunState.WAIT_PREWAIT;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.WAITPNE:
                    NextState = CogRunState.WAIT_PNE;
                    State = CogRunState.WAIT_PREWAIT;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.WAITCNT:
                    NextState = CogRunState.WAIT_CNT;
                    State = CogRunState.WAIT_PREWAIT;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.WAITVID:
                    InstructionWAITVID();
                    NextState = CogRunState.WAIT_VID;
                    State = CogRunState.WAIT_PREWAIT;
                    StateCount = 4;
                    break;
            }

            // Prefetch instruction
            Operation = ReadLong(PC);
            // Check if it's time to trigger a breakpoint
            return PC != BreakPointCogCursor;
        }

        override public void GetVideoData (out uint colours, out uint pixels)
        {
            colours = DestinationValue;
            pixels = SourceValue;
            if (State == CogRunState.WAIT_VID)
            {
                State = CogRunState.WAIT_CYCLES;
                StateCount = 3; // Minimum of 7 clocks in total
                frameFlag = FrameState.frameHit;
            }
            else
            {
                // Frame counter ran out while not in WAIT_VID
                frameFlag = FrameState.frameMiss;
            }
        }

    }
}
