/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. PropellerCPU Debugger
 * Copyright 2007 - Robert Vandiver
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

using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.EmulationCore
{
    public enum CogInstructionCodes : uint
    {
        RWBYTE  = 0x00000000,
        RWWORD  = 0x04000000,
        RWLONG  = 0x08000000,
        HUBOP   = 0x0C000000,
        MUL     = 0x10000000,
        MULS    = 0x14000000,
        ENC     = 0x18000000,
        ONES    = 0x1C000000,
        ROR     = 0x20000000,
        ROL     = 0x24000000,
        SHR     = 0x28000000,
        SHL     = 0x2C000000,
        RCR     = 0x30000000,
        RCL     = 0x34000000,
        SAR     = 0x38000000,
        REV     = 0x3C000000,
        MINS    = 0x40000000,
        MAXS    = 0x44000000,
        MIN     = 0x48000000,
        MAX     = 0x4C000000,
        MOVS    = 0x50000000,
        MOVD    = 0x54000000,
        MOVI    = 0x58000000,
        JMPRET  = 0x5C000000,
        AND     = 0x60000000,
        ANDN    = 0x64000000,
        OR      = 0x68000000,
        XOR     = 0x6C000000,
        MUXC    = 0x70000000,
        MUXNC   = 0x74000000,
        MUXZ    = 0x78000000,
        MUXNZ   = 0x7C000000,
        ADD     = 0x80000000,
        SUB     = 0x84000000,
        ADDABS  = 0x88000000,
        SUBABS  = 0x8C000000,
        SUMC    = 0x90000000,
        SUMNC   = 0x94000000,
        SUMZ    = 0x98000000,
        SUMNZ   = 0x9C000000,
        MOV     = 0xA0000000,
        NEG     = 0xA4000000,
        ABS     = 0xA8000000,
        ABSNEG  = 0xAC000000,
        NEGC    = 0xB0000000,
        NEGNC   = 0xB4000000,
        NEGZ    = 0xB8000000,
        NEGNZ   = 0xBC000000,
        CMPS    = 0xC0000000,
        CMPSX   = 0xC4000000,
        ADDX    = 0xC8000000,
        SUBX    = 0xCC000000,
        ADDS    = 0xD0000000,
        SUBS    = 0xD4000000,
        ADDSX   = 0xD8000000,
        SUBSX   = 0xDC000000,
        CMPSUB  = 0xE0000000,
        DJNZ    = 0xE4000000,
        TJNZ    = 0xE8000000,
        TJZ     = 0xEC000000,
        WAITPEQ = 0xF0000000,
        WAITPNE = 0xF4000000,
        WAITCNT = 0xF8000000,
        WAITVID = 0xFC000000
    }

    class NativeCog : Cog
    {
        // Decode fields

        protected uint Operation;
        protected CogInstructionCodes InstructionCode;
        protected CogConditionCodes ConditionCode;

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

        protected bool Carry;               // Carry Flag
        protected bool Zero;                // Zero Flag

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

        public NativeCog(PropellerCPU host,
            uint programAddress, uint paramAddress, uint frequency,
            PLLGroup pll)
            : base(host, programAddress, paramAddress, frequency, pll)
        {
            Carry = false;
            Zero = false;
        }

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

        public override void Boot()
        {
            State = CogRunState.STATE_EXECUTE;
            Carry = false;
            Zero = false;

            PC = 0;
            // Prefetch first instruction
            Operation = ReadLong(0);
        }

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
                    {
                        DataResult = Hub.ReadByte(SourceValue);
                        ZeroResult = DataResult == 0;
                        // TODO: Find Carry
                    }
                    else
                    {
                        Hub.WriteByte(SourceValue, DestinationValue);
                        // TODO: Find Zero and Carry
                    }
                    WriteBackResult();
                    return;
                case CogRunState.HUB_RDWORD:
                    if (WriteResult)
                    {
                        DataResult = Hub.ReadWord(SourceValue);
                        ZeroResult = DataResult == 0;
                        // TODO: Find Carry
                    }
                    else
                    {
                        Hub.WriteWord(SourceValue, DestinationValue);
                        // TODO: Find Zero and Carry
                    }
                    WriteBackResult();
                    return;
                case CogRunState.HUB_RDLONG:
                    if (WriteResult)
                    {
                        DataResult = Hub.ReadLong(SourceValue);
                        ZeroResult = DataResult == 0;
                        // TODO: Find Carry
                    }
                    else
                    {
                        Hub.WriteLong(SourceValue, DestinationValue);
                        // TODO: Find Zero and Carry
                    }
                    WriteBackResult();
                    return;
            }

            base.HubAccessable();
        }

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
                            Zero = maskedIn == 0;
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
                            Zero = maskedIn == 0;
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
                            ZeroResult = DataResult == 0;
                            WriteBackResult();
                            return true;
                        }
                        return true;
                    }
                case CogRunState.WAIT_VID:
                    if (Video.Ready)
                    {
                        Video.Feed(DestinationValue, SourceValue);
                        // TODO: Determine carry, zero, and result
                        WriteBackResult();
                    }
                    return true;

                // Non-execute states are ignored
                default:
                    return true;
            }

            PC = (PC + 1) & 0x1FF;

            InstructionCode = (CogInstructionCodes)(Operation & 0xFC000000);
            ConditionCode = (CogConditionCodes)((Operation & 0x003C0000) >> 18);

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
                case CogInstructionCodes.MUL:
                case CogInstructionCodes.MULS:
                case CogInstructionCodes.ENC:
                case CogInstructionCodes.ONES:
                    // TODO: RAISE INVALID OP CODE HERE

                    DataResult = 0;
                    Carry = true;
                    Zero = true;

                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                // HUB Operations (6 cycles for instruction decode... 1-15 cycles for operation)
                case CogInstructionCodes.RWBYTE:
                    State = CogRunState.WAIT_PREWAIT;
                    NextState = CogRunState.HUB_RDBYTE;
                    StateCount = 6;
                    break;
                case CogInstructionCodes.RWWORD:
                    State = CogRunState.WAIT_PREWAIT;
                    NextState = CogRunState.HUB_RDWORD;
                    StateCount = 6;
                    break;
                case CogInstructionCodes.RWLONG:
                    State = CogRunState.WAIT_PREWAIT;
                    NextState = CogRunState.HUB_RDLONG;
                    StateCount = 6;
                    break;
                case CogInstructionCodes.HUBOP:
                    State = CogRunState.WAIT_PREWAIT;
                    NextState = CogRunState.HUB_HUBOP;
                    StateCount = 6;
                    break;

                // Standard operations (4 cycles)
                case CogInstructionCodes.ROR:
                    InstructionROR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.ROL:
                    InstructionROL();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.RCR:
                    InstructionRCR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.RCL:
                    InstructionRCL();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.SHR:
                    InstructionSHR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.SHL:
                    InstructionSHL();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.SAR:
                    InstructionSAR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case CogInstructionCodes.OR:
                    InstructionOR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.XOR:
                    InstructionXOR();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.AND:
                    InstructionAND();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.ANDN:
                    InstructionANDN();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case CogInstructionCodes.MUXC:
                    InstructionMUXC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.MUXNC:
                    InstructionMUXNC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.MUXZ:
                    InstructionMUXZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.MUXNZ:
                    InstructionMUXNZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case CogInstructionCodes.REV:
                    InstructionREV();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case CogInstructionCodes.NEG:
                    InstructionNEG();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.ABS:
                    InstructionABS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.ABSNEG:
                    InstructionABSNEG();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.NEGC:
                    InstructionNEGC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.NEGNC:
                    InstructionNEGNC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.NEGZ:
                    InstructionNEGZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.NEGNZ:
                    InstructionNEGNZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case CogInstructionCodes.MOV:
                    InstructionMOV();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.MOVS:
                    InstructionMOVS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.MOVD:
                    InstructionMOVD();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.MOVI:
                    InstructionMOVI();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.JMPRET:
                    InstructionJMPRET();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case CogInstructionCodes.MINS:
                    InstructionMINS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.MAXS:
                    InstructionMAXS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.MIN:
                    InstructionMIN();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.MAX:
                    InstructionMAX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case CogInstructionCodes.ADD:
                    InstructionADD();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.ADDABS:
                    InstructionADDABS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.ADDX:
                    InstructionADDX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.ADDS:
                    InstructionADDS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.ADDSX:
                    InstructionADDSX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case CogInstructionCodes.SUB:
                    InstructionSUB();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.SUBABS:
                    InstructionSUBABS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.SUBX:
                    InstructionSUBX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.SUBS:
                    InstructionSUBS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.SUBSX:
                    InstructionSUBSX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case CogInstructionCodes.SUMC:
                    InstructionSUMC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.SUMNC:
                    InstructionSUMNC();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.SUMZ:
                    InstructionSUMZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.SUMNZ:
                    InstructionSUMNZ();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                case CogInstructionCodes.CMPS:
                    InstructionCMPS();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.CMPSX:
                    InstructionCMPSX();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.CMPSUB:
                    InstructionCMPSUB();
                    State = CogRunState.WAIT_CYCLES;
                    StateCount = 4;
                    break;

                // Decrement and continue instructions ( 4/8 cycles )
                case CogInstructionCodes.DJNZ:
                    StateCount = InstructionDJNZ() ? 4 : 8;
                    State = CogRunState.WAIT_CYCLES;
                    break;
                case CogInstructionCodes.TJNZ:
                    StateCount = InstructionTJNZ() ? 4 : 8;
                    State = CogRunState.WAIT_CYCLES;
                    break;
                case CogInstructionCodes.TJZ:
                    StateCount = InstructionTJZ() ? 4 : 8;
                    State = CogRunState.WAIT_CYCLES;
                    break;

                // Delay execution instructions ( 4 cycles for instruction decode, 1+ for instruction )
                case CogInstructionCodes.WAITPEQ:
                    NextState = CogRunState.WAIT_PEQ;
                    State = CogRunState.WAIT_PREWAIT;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.WAITPNE:
                    NextState = CogRunState.WAIT_PNE;
                    State = CogRunState.WAIT_PREWAIT;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.WAITCNT:
                    NextState = CogRunState.WAIT_CNT;
                    State = CogRunState.WAIT_PREWAIT;
                    StateCount = 4;
                    break;
                case CogInstructionCodes.WAITVID:
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

        private void InstructionMOV()
        {
            DataResult = SourceValue;
            ZeroResult = DataResult == 0;
            CarryResult = (SourceValue & 0x80000000) != 0;
        }

        private void InstructionMOVS()
        {
            DataResult = (DestinationValue & 0xFFFFFE00) | (SourceValue & 0x000001FF);
            ZeroResult = DataResult == 0;
            // TODO: Find out what carry REALLY does
            CarryResult = Carry;
        }

        private void InstructionMOVD()
        {
            DataResult = (DestinationValue & 0xFFFC01FF) | ((SourceValue & 0x000001FF) << 9);
            ZeroResult = DataResult == 0;
            // TODO: Find out what carry REALLY does
            CarryResult = Carry;
        }

        private void InstructionMOVI()
        {
            DataResult = (DestinationValue & 0x007FFFFF) | ((SourceValue & 0x000001FF) << 23);
            ZeroResult = DataResult == 0;
            // TODO: Find out what carry REALLY does
            CarryResult = Carry;
        }

        private void InstructionJMPRET()
        {
            DataResult = (DestinationValue & 0xFFFFFE00) | (PC & 0x000001FF);
            PC = SourceValue & 0x1FF;
            ZeroResult = DataResult == 0;
            // TODO: Find out what carry REALLY does
            CarryResult = Carry;
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
            ZeroResult = Zero && (DataResult == 0);
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
            ZeroResult = Zero && (DataResult == 0);
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
    }
}
