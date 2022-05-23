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

        /// @brief Default constructor for a %Cog running in PASM mode.
        /// @param cpuHost PropellerCPU where this cog resides.
        /// @param cogNum
        /// @param programAddress Start of program to load from main memory.
        /// @param paramAddress PARAM value given to the Cog.
        /// @param frequency Frequency running the cog (the same as the propeller cpu).
        /// @param pllGroup PLL Multiplier running the cog (the same as the propeller cpu).
        /// @version v22.05.03 - Parameter name changed to use the same
        /// convention for a PropellerCPU instance reference.
        public NativeCog(PropellerCPU cpuHost, int cogNum, uint programAddress,
            uint paramAddress, uint frequency, PLLGroup pllGroup)
            : base(cpuHost, cogNum, programAddress, paramAddress, frequency, pllGroup)
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

            State = CogRunState.StateExecute;
        }

        /// @brief Setup the cog to a initial state after boot it.
        /// @version v22.05.04 - Changed visibility of method.
        private protected override void Boot()
        {
            State = CogRunState.StateExecute;
            Carry = false;
            Zero = false;
            ProgramCursor = 0;
            // Prefetch first instruction
            Operation = ReadLong(0);
        }

        /// @brief Determine if the Hub is accessible in that moment of time, setting the state
        /// accordant.
        /// @version v22.05.03 - Method name changed to clarify meaning of it.
        public override void ExecuteHubOperation()
        {
            switch (State)
            {
                case CogRunState.HubHubOperation:
                    DataResult = CpuHost.HubOp(this, SourceValue, DestinationValue, ref CarryResult,
                        ref ZeroResult);
                    WriteBackResult();
                    return;
                case CogRunState.HubReadByte:
                    if (WriteResult)
                        DataResult = CpuHost.DirectReadByte(SourceValue);
                    else
                        CpuHost.DirectWriteByte(SourceValue, (byte)DestinationValue);
                    ZeroResult = (DataResult == 0); //set as state Manual v1.2
                    // Don't affect Carry as state Manual v1.2
                    WriteBackResult();
                    return;
                case CogRunState.HubReadWord:
                    if (WriteResult)
                        DataResult = CpuHost.DirectReadWord(SourceValue);
                    else
                        CpuHost.DirectWriteWord(SourceValue, (ushort)DestinationValue);
                    ZeroResult = (DataResult == 0); //set as state Manual v1.2
                    // Don't affect Carry as state Manual v1.2
                    WriteBackResult();
                    return;
                case CogRunState.HubReadLong:
                    if (WriteResult)
                        DataResult = CpuHost.DirectReadLong(SourceValue);
                    else
                        CpuHost.DirectWriteLong(SourceValue, DestinationValue);
                    ZeroResult = (DataResult == 0);  //set as state Manual v1.2
                    // Don't affect Carry as state Manual v1.2
                    WriteBackResult();
                    return;
            }
            base.ExecuteHubOperation();
        }

        /// @brief Execute a PASM instruction in this cog.
        /// @returns TRUE if it is the opportunity to trigger a breakpoint,
        /// or FALSE if not.
        /// @version v22.05.04 - Invert sense of return value
        /// on ConditionCompare(), to be intuitive.
        override public bool DoInstruction()
        {
            switch (State)
            {
                // Delay State
                case CogRunState.WaitCycles:
                    if (--StateCount == 1)
                        WriteBackResult();
                    return true;
                // Clocked, but not executed
                case CogRunState.WaitPreWait:
                    if (--StateCount == 1)
                        State = NextState;
                    return true;

                // Execute State
                case CogRunState.StateExecute:
                    break;

                case CogRunState.WaitPinsNotEqual:
                    {
                        uint maskedIn = (Carry ? CpuHost.INB : CpuHost.INA) & SourceValue;
                        if (maskedIn != DestinationValue)
                        {
                            DataResult = maskedIn;
                            Zero = (maskedIn == 0);
                            // TODO: DETERMINE CARRY
                            WriteBackResult();
                        }
                        return true;
                    }
                case CogRunState.WaitPinsEqual:
                    {
                        uint maskedIn = (Carry ? CpuHost.INB : CpuHost.INA) & SourceValue;
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
                case CogRunState.WaitCount:
                    {
                        long target = CpuHost.Counter;

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
                case CogRunState.WaitVideo:
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

            ProgramCursor = (ProgramCursor + 1) & MaskCogMemory;

            FrameFlag = FrameState.FrameNone;

            InstructionCode = (Assembly.InstructionCodes)(Operation & 0xFC000000);
            ConditionCode = (Assembly.ConditionCodes)((Operation & 0x003C0000) >> 18);

            WriteZero = (Operation & 0x02000000) != 0;
            WriteCarry = (Operation & 0x01000000) != 0;
            WriteResult = (Operation & 0x00800000) != 0;
            ImmediateValue = (Operation & 0x00400000) != 0;

            SourceValue = Operation & MaskCogMemory;

            if (!ImmediateValue)
                SourceValue = ReadLong(SourceValue);

            Destination = (Operation >> 9) & MaskCogMemory;
            DestinationValue = ReadLong(Destination);
            // changed to NOT ConditionCompare() to recover intuitive return value on that function
            if (!ConditionCompare(ConditionCode, Zero, Carry))
            {
                Operation = ReadLong(ProgramCursor);
                State = CogRunState.WaitPreWait;
                NextState = CogRunState.StateExecute;
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

                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                // HUB Operations (6 cycles for instruction decode... 1-15 cycles for operation)
                case Assembly.InstructionCodes.RWBYTE:
                    State = CogRunState.WaitPreWait;
                    NextState = CogRunState.HubReadByte;
                    StateCount = 6;
                    break;
                case Assembly.InstructionCodes.RWWORD:
                    State = CogRunState.WaitPreWait;
                    NextState = CogRunState.HubReadWord;
                    StateCount = 6;
                    break;
                case Assembly.InstructionCodes.RWLONG:
                    State = CogRunState.WaitPreWait;
                    NextState = CogRunState.HubReadLong;
                    StateCount = 6;
                    break;
                case Assembly.InstructionCodes.HUBOP:
                    State = CogRunState.WaitPreWait;
                    NextState = CogRunState.HubHubOperation;
                    StateCount = 6;
                    break;

                // Standard operations (4 cycles)
                case Assembly.InstructionCodes.ROR:
                    InstructionROR();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ROL:
                    InstructionROL();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.RCR:
                    InstructionRCR();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.RCL:
                    InstructionRCL();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SHR:
                    InstructionSHR();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SHL:
                    InstructionSHL();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SAR:
                    InstructionSAR();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.OR:
                    InstructionOR();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.XOR:
                    InstructionXOR();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.AND:
                    InstructionAND();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ANDN:
                    InstructionANDN();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.MUXC:
                    InstructionMUXC();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MUXNC:
                    InstructionMUXNC();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MUXZ:
                    InstructionMUXZ();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MUXNZ:
                    InstructionMUXNZ();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.REV:
                    InstructionREV();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.NEG:
                    InstructionNEG();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ABS:
                    InstructionABS();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ABSNEG:
                    InstructionABSNEG();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.NEGC:
                    InstructionNEGC();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.NEGNC:
                    InstructionNEGNC();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.NEGZ:
                    InstructionNEGZ();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.NEGNZ:
                    InstructionNEGNZ();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.MOV:
                    InstructionMOV();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MOVS:
                    InstructionMOVS();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MOVD:
                    InstructionMOVD();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MOVI:
                    InstructionMOVI();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.JMPRET:
                    InstructionJMPRET();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.MINS:
                    InstructionMINS();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MAXS:
                    InstructionMAXS();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MIN:
                    InstructionMIN();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.MAX:
                    InstructionMAX();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.ADD:
                    InstructionADD();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ADDABS:
                    InstructionADDABS();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ADDX:
                    InstructionADDX();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ADDS:
                    InstructionADDS();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.ADDSX:
                    InstructionADDSX();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.SUB:
                    InstructionSUB();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUBABS:
                    InstructionSUBABS();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUBX:
                    InstructionSUBX();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUBS:
                    InstructionSUBS();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUBSX:
                    InstructionSUBSX();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.SUMC:
                    InstructionSUMC();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUMNC:
                    InstructionSUMNC();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUMZ:
                    InstructionSUMZ();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.SUMNZ:
                    InstructionSUMNZ();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                case Assembly.InstructionCodes.CMPS:
                    InstructionCMPS();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.CMPSX:
                    InstructionCMPSX();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.CMPSUB:
                    InstructionCMPSUB();
                    State = CogRunState.WaitCycles;
                    StateCount = 4;
                    break;

                // Decrement and continue instructions ( 4/8 cycles )
                case Assembly.InstructionCodes.DJNZ:
                    StateCount = InstructionDJNZ() ? 4 : 8;
                    State = CogRunState.WaitCycles;
                    break;
                case Assembly.InstructionCodes.TJNZ:
                    StateCount = InstructionTJNZ() ? 4 : 8;
                    State = CogRunState.WaitCycles;
                    break;
                case Assembly.InstructionCodes.TJZ:
                    StateCount = InstructionTJZ() ? 4 : 8;
                    State = CogRunState.WaitCycles;
                    break;

                // Delay execution instructions ( 4 cycles for instruction decode, 1+ for instruction )
                case Assembly.InstructionCodes.WAITPEQ:
                    NextState = CogRunState.WaitPinsEqual;
                    State = CogRunState.WaitPreWait;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.WAITPNE:
                    NextState = CogRunState.WaitPinsNotEqual;
                    State = CogRunState.WaitPreWait;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.WAITCNT:
                    NextState = CogRunState.WaitCount;
                    State = CogRunState.WaitPreWait;
                    StateCount = 4;
                    break;
                case Assembly.InstructionCodes.WAITVID:
                    InstructionWAITVID();
                    NextState = CogRunState.WaitVideo;
                    State = CogRunState.WaitPreWait;
                    StateCount = 4;
                    break;
            }

            // Prefetch instruction
            Operation = ReadLong(ProgramCursor);
            // Check if it's time to trigger a breakpoint
            return ProgramCursor != BreakPointCogCursor;
        }

        override public void GetVideoData (out uint colors, out uint pixels)
        {
            colors = DestinationValue;
            pixels = SourceValue;
            if (State == CogRunState.WaitVideo)
            {
                State = CogRunState.WaitCycles;
                StateCount = 3; // Minimum of 7 clocks in total
                FrameFlag = FrameState.FrameHit;
            }
            else
            {
                // Frame counter ran out while not in WAIT_VID
                FrameFlag = FrameState.FrameMiss;
            }
        }

    }
}
