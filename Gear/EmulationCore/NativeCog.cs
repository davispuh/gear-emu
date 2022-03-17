/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. PropellerCPU Debugger
 * Copyright 2020 - Gear Developers
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
    class NativeCog : Cog
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

        private void InstructionRCR()
        {
            int bits = (int)SourceValue & 31;
            DataResult = DestinationValue >> bits;
            if (Carry)
                DataResult |= ~(0xFFFFFFFF >> bits);

            CarryResult = (DestinationValue & 0x00000001) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionRCL()
        {
            int bits = (int)SourceValue & 31;
            DataResult = DestinationValue << bits;
            if (Carry)
                DataResult |= ~(0xFFFFFFFF << bits);

            CarryResult = (DestinationValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionROR()
        {
            int bits = (int)SourceValue & 31;
            ulong mask = DestinationValue | ((ulong)DestinationValue << 32);

            DataResult = (uint)(mask >> bits);
            CarryResult = (DestinationValue & 0x00000001) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionROL()
        {
            int bits = (int)SourceValue & 31;
            ulong mask = DestinationValue | ((ulong)DestinationValue << 32);

            DataResult = (uint)(mask << bits >> 32);
            CarryResult = (DestinationValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionSHR()
        {
            int bits = (int)SourceValue & 31;

            DataResult = DestinationValue >> bits;
            CarryResult = (DestinationValue & 0x00000001) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionSHL()
        {
            int bits = (int)SourceValue & 31;

            DataResult = DestinationValue << bits;
            CarryResult = (DestinationValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionSAR()
        {
            int bits = (int)SourceValue & 31;

            DataResult = DestinationValue >> bits;
            if ((DestinationValue & 0x80000000) != 0)
                DataResult |= ~(0xFFFFFFFF >> bits);

            CarryResult = (DestinationValue & 0x00000001) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionOR()
        {
            DataResult = SourceValue | DestinationValue;
            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        private void InstructionXOR()
        {
            DataResult = SourceValue ^ DestinationValue;
            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        private void InstructionAND()
        {
            DataResult = SourceValue & DestinationValue;
            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        private void InstructionANDN()
        {
            DataResult = DestinationValue & ~SourceValue;
            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        private void InstructionMUXC()
        {
            if (Carry)
                DataResult = DestinationValue | SourceValue;
            else
                DataResult = DestinationValue & ~SourceValue;

            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        private void InstructionMUXNC()
        {
            if (!Carry)
                DataResult = DestinationValue | SourceValue;
            else
                DataResult = DestinationValue & ~SourceValue;

            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        private void InstructionMUXZ()
        {
            if (Zero)
                DataResult = DestinationValue | SourceValue;
            else
                DataResult = DestinationValue & ~SourceValue;

            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        private void InstructionMUXNZ()
        {
            if (!Zero)
                DataResult = DestinationValue | SourceValue;
            else
                DataResult = DestinationValue & ~SourceValue;

            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        private void InstructionREV()
        {
            int shift = 0;

            DataResult = 0;
            // if (SourceValue < 32)
            // {
                for (int i = 31 - ((int)SourceValue & 31); i >= 0; i--)
                    DataResult |= ((DestinationValue >> i) & 1) << (shift++);
                ZeroResult = DataResult == 0;
            // }
            // else
            // {
            //    ZeroResult = true;
            // }
            CarryResult = (DestinationValue & 1) != 0;
        }

        private void InstructionABS()
        {
            if ((SourceValue & 0x80000000) != 0)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionABSNEG()
        {
            if ((SourceValue & 0x80000000) == 0)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionNEG()
        {
            DataResult = (uint)-(int)SourceValue;
            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionNEGC()
        {
            if (Carry)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionNEGNC()
        {
            if (!Carry)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionNEGZ()
        {
            if (Zero)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        private void InstructionNEGNZ()
        {
            if (!Zero)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// @brief Execute instruction MOV: Set a register to a value.
        /// @details Effects: Set D to S
        private void InstructionMOV()
        {
            DataResult = SourceValue;
            ZeroResult = (DataResult == 0);
            CarryResult = ((SourceValue & 0x80000000) != 0);
        }

        /// @brief Execute instruction MOVS: Set a register's source field to a value.
        /// @details Effects: Insert S[8..0] into D[8..0]
        private void InstructionMOVS()
        {
            DataResult = (DestinationValue & 0xFFFFFE00) | (SourceValue & 0x000001FF);
            ZeroResult = (DataResult == 0);
            /// @todo InstructionMOVS - Find out what carry REALLY does in hardware.
            CarryResult = Carry;
        }

        /// @brief Execute instruction MOVD: Set a register's destination field to a value.
        /// @details Effects: Insert S[8..0] into D[17..9]
        private void InstructionMOVD()
        {
            DataResult = (DestinationValue & 0xFFFC01FF) | ((SourceValue & 0x000001FF) << 9);
            ZeroResult = (DataResult == 0);
            /// @todo InstructionMOVD - Find out what carry REALLY does in hardware.
            CarryResult = Carry;
        }

        /// @brief Execute instruction MOVI: Set a register's instruction and effects fields 
        /// to a value.
        /// @details Effects: Insert S[8..0] into D[31..23]
        private void InstructionMOVI()
        {
            DataResult = (DestinationValue & 0x007FFFFF) | ((SourceValue & 0x000001FF) << 23);
            ZeroResult = (DataResult == 0);
            /// @todo InstructionMOVI - Find out what carry REALLY does in hardware.
            CarryResult = Carry;
        }

        /// @brief Execute instruction JMPRET: Jump to address with intention to "return" 
        /// to another address.
        /// @details Effects: Insert PC+1 into D[8..0] and set PC to S[8..0].
        /// @version V15.03.26 - corrected Carry flag according to Propeller Manual v1.2.
        private void InstructionJMPRET()
        {
            DataResult = (DestinationValue & 0xFFFFFE00) | (PC & 0x000001FF);
            PC = SourceValue & 0x1FF;
            ZeroResult = (DataResult == 0);
            //Note from Propeller Manual v1.2: "The C flag is set (1) unless PC+1 equals 0; very 
            // unlikely since it would require the JMPRET to be executed from the top of 
            // cog RAM ($1FF; special purpose register VSCL)."
            CarryResult = (PC != 0);
        }

        private void InstructionMINS()
        {
            if ((int)DestinationValue < (int)SourceValue)
            {
                DataResult = SourceValue;
                CarryResult = true;
            }
            else
            {
                DataResult = DestinationValue;
                CarryResult = false;
            }

            ZeroResult = DestinationValue == SourceValue;
        }

        private void InstructionMAXS()
        {
            if ((int)DestinationValue < (int)SourceValue)
            {
                DataResult = DestinationValue;
                CarryResult = true;
            }
            else
            {
                DataResult = SourceValue;
                CarryResult = false;
            }

            ZeroResult = DestinationValue == SourceValue;
        }

        private void InstructionMIN()
        {
            if (DestinationValue < SourceValue)
            {
                DataResult = SourceValue;
                CarryResult = true;
            }
            else
            {
                DataResult = DestinationValue;
                CarryResult = false;
            }

            ZeroResult = DestinationValue == SourceValue;
        }

        private void InstructionMAX()
        {
            if (DestinationValue < SourceValue)
            {
                DataResult = DestinationValue;
                CarryResult = true;
            }
            else
            {
                DataResult = SourceValue;
                CarryResult = false;
            }

            ZeroResult = DestinationValue == SourceValue;
        }

        private void InstructionADD()
        {
            ulong result = (ulong)SourceValue + (ulong)DestinationValue;
            DataResult = (uint)result;
            ZeroResult = DataResult == 0;
            CarryResult = (result & 0x100000000) != 0;
        }

        private void InstructionADDABS()
        {
            ulong result = (ulong)DestinationValue;

            if ((SourceValue & 0x80000000) != 0)
                result += (ulong)-(int)SourceValue;
            else
                result += (ulong)SourceValue;

            DataResult = (uint)result;
            ZeroResult = DataResult == 0;
            CarryResult = (result & 0x100000000) != 0;
        }

        private void InstructionADDX()
        {
            ulong result = (ulong)SourceValue + (ulong)DestinationValue + (ulong)(Carry ? 1 : 0);

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = (result & 0x100000000) != 0;
        }

        private void InstructionADDS()
        {
            long result = (int)SourceValue + (int)DestinationValue;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        private void InstructionADDSX()
        {
            long result = (int)SourceValue + (int)DestinationValue + (Carry ? 1 : 0);

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        private void InstructionSUB()
        {
            ulong result = (ulong)DestinationValue - (ulong)SourceValue;
            DataResult = (uint)result;
            ZeroResult = DataResult == 0;
            CarryResult = (result & 0x100000000) != 0;
        }

        private void InstructionSUBABS()
        {
            long result = DestinationValue;

            if ((SourceValue & 0x80000000) != 0)
                result -= (long)-(int)SourceValue;
            else
                result -= (long)SourceValue;

            DataResult = (uint)result;
            ZeroResult = DataResult == 0;
            CarryResult = (result & 0x100000000) != 0;
        }

        private void InstructionSUBX()
        {
            ulong result = (ulong)DestinationValue - (ulong)SourceValue;

            if (Carry)
                result--;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = (result & 0x100000000) != 0;
        }

        private void InstructionSUBS()
        {
            long result = (int)DestinationValue - (int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) != 0
                && ((SourceValue ^ DataResult) & 0x80000000) == 0;
        }

        private void InstructionSUBSX()
        {
            long result = (int)DestinationValue - (int)SourceValue;

            if (Carry)
                result--;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) != 0
                && ((SourceValue ^ DataResult) & 0x80000000) == 0;
        }

        private void InstructionSUMC()
        {
            long result = (int)DestinationValue;

            if (Carry)
                result -= (int)SourceValue;
            else
                result += (int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        private void InstructionSUMNC()
        {
            long result = (int)DestinationValue;

            if (!Carry)
                result -= (int)SourceValue;
            else
                result += (int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        private void InstructionSUMZ()
        {
            long result = (int)DestinationValue;

            if (Zero)
                result -= (int)SourceValue;
            else
                result += (int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        private void InstructionSUMNZ()
        {
            long result = (int)DestinationValue;

            if (!Zero)
                result -= (int)SourceValue;
            else
                result += (int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        private void InstructionCMPS()
        {
            long result = (long)(int)DestinationValue - (long)(int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = (DataResult == 0);
            CarryResult = (DataResult & 0x80000000) == 0x80000000;
        }

        private void InstructionCMPSX()
        {
            long result = (long)(int)DestinationValue - (long)(int)SourceValue;

            if (Carry)
                result--;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = (DataResult & 0x80000000) == 0x80000000;
            if (((SourceValue ^ DestinationValue) & 0x80000000) != 0)
                CarryResult = !CarryResult;
        }

        private void InstructionCMPSUB()
        {
            if (DestinationValue >= SourceValue)
            {
                DataResult = DestinationValue - SourceValue;
                ZeroResult = (DataResult == 0);
            }
            else
            {
                DataResult = DestinationValue;
                ZeroResult = false;
            }
            CarryResult = DestinationValue >= SourceValue;
        }

        private bool InstructionDJNZ()
        {
            DataResult = DestinationValue - 1;
            CarryResult = DestinationValue == 0;
            ZeroResult = DataResult == 0;

            if (!ZeroResult)
                PC = SourceValue & 0x1FF;

            return !ZeroResult;
        }

        private bool InstructionTJNZ()
        {
            DataResult = DestinationValue;
            CarryResult = false;
            ZeroResult = DataResult == 0;

            if (!ZeroResult)
                PC = SourceValue & 0x1FF;

            return !ZeroResult;
        }

        private bool InstructionTJZ()
        {
            DataResult = DestinationValue;
            CarryResult = false;
            ZeroResult = DataResult == 0;

            if (ZeroResult)
                PC = SourceValue & 0x1FF;

            return ZeroResult;
        }

        private void InstructionWAITVID()
        {
            ulong result = (ulong)SourceValue + (ulong)DestinationValue;
            DataResult = (uint)result;
            ZeroResult = DataResult == 0;
            CarryResult = (result & 0x100000000) != 0;
        }
    }
}
