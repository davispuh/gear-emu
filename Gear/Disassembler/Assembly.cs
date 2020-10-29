/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2020 - Gear Developers
 * --------------------------------------------------------------------------------
 * Assembly.cs
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

namespace Gear.Disassembler
{
    public static class Assembly
    {
        public static Propeller.Assembly.SubInstruction GetSubInstruction(Propeller.Assembly.Instruction SourceInstruction, ParsedInstruction ParsedInstruction)
        {
            switch (SourceInstruction.Type)
            {
                case Propeller.Assembly.InstructionType.Normal:
                    return SourceInstruction.SubInstructions[0];
                case Propeller.Assembly.InstructionType.WR:
                    return SourceInstruction.SubInstructions[(ParsedInstruction.ZCRI & ParsedInstruction.WriteResultFlag) == ParsedInstruction.WriteResultFlag ? 0 : 1];
                case Propeller.Assembly.InstructionType.Hub:
                    return SourceInstruction.SubInstructions[ParsedInstruction.SRC & 0x7];
                case Propeller.Assembly.InstructionType.Jump:
                    int num = ParsedInstruction.ZCRI & 0x3;
                    if (num <= 1)
                    {
                        num = 0;
                        if (ParsedInstruction.SRC == 0)
                        {
                            num = 1;
                        }
                    }
                    return SourceInstruction.SubInstructions[num];
            }
            throw new Exception("Unknown Instruction Type: " + SourceInstruction.Type.ToString());
        }

        public class ParsedInstruction
        {
            public const byte WriteZeroFlag      = 0x8;
            public const byte WriteCarryFlag     = 0x4;
            public const byte WriteResultFlag    = 0x2;
            public const byte ImmediateValueFlag = 0x1;

            public uint   Opcode { get; private set; }  //!< Instruction's 32-bit opcode
            public byte   INSTR  { get; private set; }  //!< Indicates the instruction being executed
            public byte   ZCRI   { get; private set; }  //!< Indicates instruction’s effect status and SRC field meaning
            public byte   CON    { get; private set; }  //!< Indicates the condition in which to execute the instruction
            public ushort DEST   { get; private set; }  //!< Contains the destination register address
            public ushort SRC    { get; private set; }  //!< Contains the source register address or 9-bit literal value

            public Propeller.Assembly.Instruction SourceInstruction { get; private set; }

            private Propeller.Assembly.SubInstruction SourceSubInstruction;

            public ParsedInstruction(uint Opcode)
            {
                this.Opcode = Opcode;
                this.INSTR  = (byte  )((Opcode >> 26) & 0x03F);  // (bits 31:26)
                this.ZCRI   = (byte  )((Opcode >> 22) & 0x00F);  // (bits 25:22)
                this.CON    = (byte  )((Opcode >> 18) & 0x00F);  // (bits 21:18)
                this.DEST   = (ushort)((Opcode >>  9) & 0x1FF);  // (bits 17:09)
                this.SRC    = (ushort)( Opcode        & 0x1FF);  // (bits 08:00)

                this.SourceInstruction = Propeller.Assembly.Instructions[this.INSTR];
                this.SourceSubInstruction = null;
            }

            public Propeller.Assembly.SubInstruction GetSubInstruction()
            {
                if (this.SourceSubInstruction == null)
                {
                    this.SourceSubInstruction = Assembly.GetSubInstruction(this.SourceInstruction, this);
                }
                return this.SourceSubInstruction;
            }

            public bool WriteZero()
            {
                return (this.ZCRI & WriteZeroFlag) == WriteZeroFlag && this.GetSubInstruction().WZ;
            }

            public bool WriteCarry()
            {
                return (this.ZCRI & WriteCarryFlag) == WriteCarryFlag && this.GetSubInstruction().WC;
            }

            public bool WriteResult()
            {
                return (this.ZCRI & WriteResultFlag) == WriteResultFlag && this.GetSubInstruction().WR;
            }

            public bool NoResult()
            {
                return (this.ZCRI & WriteResultFlag) == 0 && this.GetSubInstruction().WR;
            }

            public bool ImmediateValue()
            {
                return (this.ZCRI & ImmediateValueFlag) == ImmediateValueFlag;
            }

        }

    }
}
