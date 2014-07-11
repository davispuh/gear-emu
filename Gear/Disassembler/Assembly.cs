using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Disassembler
{
    public partial class Assembly
    {
        public enum InstructionType
        {
            Normal,
            WR,
            Hub,
            Jump
        }

        public class Register : Disassembler.Register
        {
            public bool Read  { get; private set; }
            public bool Write { get; private set; }

            public Register(string Name, bool Read, bool Write)
            {
                this.Name  = Name;
                this.Read  = Read;
                this.Write = Write;
            }
        }

        public class SubInstruction
        {
            public string Name           { get; private set; }
            public bool   Destination    { get; private set; }
            public bool   Source         { get; private set; }
            public bool   WZ             { get; private set; }
            public bool   WC             { get; private set; }
            public bool   WR             { get; private set; }
            public bool   ImmediateValue { get; private set; }

            public SubInstruction(string Name, bool Destination, bool Source, bool WZ, bool WC, bool WR, bool ImmediateValue)
            {
                this.Name           = Name;
                this.Destination    = Destination;
                this.Source         = Source;
                this.WZ             = WZ;
                this.WC             = WC;
                this.WR             = WR;
                this.ImmediateValue = ImmediateValue;
            }
        }

        public class Instruction
        {
            public InstructionType  Type            { get; private set; }
            public SubInstruction[] SubInstructions { get; private set; }

            public Instruction(InstructionType Type, SubInstruction[] SubInstructions)
            {
                this.Type            = Type;
                this.SubInstructions = SubInstructions;
            }

            public SubInstruction GetSubInstruction(ParsedInstruction ParsedInstruction)
            {
                switch (this.Type)
                {
                    case InstructionType.Normal:
                        return this.SubInstructions[0];
                    case InstructionType.WR:
                        return this.SubInstructions[(ParsedInstruction.ZCRI & ParsedInstruction.WriteResultFlag) == ParsedInstruction.WriteResultFlag ? 0 : 1];
                    case InstructionType.Hub:
                        return this.SubInstructions[ParsedInstruction.SRC & 0x7];
                    case InstructionType.Jump:
                        int num = ParsedInstruction.ZCRI & 0x3;
                        if (num <= 1)
                        {
                            num = 0;
                            if (ParsedInstruction.SRC == 0)
                            {
                                num = 1;
                            }
                        }
                        return this.SubInstructions[num];
                }
                throw new Exception("Uknown Instruction Type: " + this.Type.ToString());
            }
        }

        public class ParsedInstruction
        {
            public const byte WriteZeroFlag      = 0x8;
            public const byte WriteCarryFlag     = 0x4;
            public const byte WriteResultFlag    = 0x2;
            public const byte ImmediateValueFlag = 0x1;

            public uint   Opcode { get; private set; }  // Instruction's 32-bit opcode
            public byte   INSTR  { get; private set; }  // Indicates the instruction being executed
            public byte   ZCRI   { get; private set; }  // Indicates instruction’s effect status and SRC field meaning
            public byte   CON    { get; private set; }  // Indicates the condition in which to execute the instruction
            public ushort DEST   { get; private set; }  // Contains the destination register address
            public ushort SRC    { get; private set; }  // Contains the source register address or 9-bit literal value

            public Instruction SourceInstruction { get; private set; }

            private SubInstruction SourceSubInstruction;

            public ParsedInstruction(uint Opcode)
            {
                this.Opcode = Opcode;
                this.INSTR  = (byte  )((Opcode >> 26) & 0x03F);  // (bits 31:26)
                this.ZCRI   = (byte  )((Opcode >> 22) & 0x00F);  // (bits 25:22)
                this.CON    = (byte  )((Opcode >> 18) & 0x00F);  // (bits 21:18)
                this.DEST   = (ushort)((Opcode >>  9) & 0x1FF);  // (bits 17:09)
                this.SRC    = (ushort)( Opcode        & 0x1FF);  // (bits 08:00)

                this.SourceInstruction = Instructions[this.INSTR];
                this.SourceSubInstruction = null;
            }

            public SubInstruction GetSubInstruction()
            {
                if (this.SourceSubInstruction == null)
                {
                    this.SourceSubInstruction = this.SourceInstruction.GetSubInstruction(this);
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
