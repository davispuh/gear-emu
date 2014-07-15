using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Disassembler
{
    public partial class Spin
    {
        public enum MemoryAction
        {
            UNKNOWN_0,
            UNKNOWN_1,
            UNKNOWN_2,
            UNKNOWN_3,
            PUSH,
            POP,
            EFFECT,
            UNKNOWN_7
        }

        public enum ArgumentMode
        {
            None,
            Effect,
            SignedOffset,
            PackedLiteral,
            UnsignedOffset,
            UnsignedEffectedOffset,
            ByteLiteral,
            WordLiteral,
            NearLongLiteral,
            LongLiteral,
            ObjCallPair,
            MemoryOpCode
        }

        public class Register : Disassembler.Register
        {
            public Register(string Name)
            {
                this.Name = Name;
            }
        }

        public class ParsedMemoryOperation
        {
            public byte Opcode           { get; private set; }
            public byte Register         { get; private set; }
            public bool AssemblyRegister { get; private set; }
            public MemoryAction Action   { get; private set; }

            public ParsedMemoryOperation(byte Opcode)
            {
                this.Opcode           = Opcode;
                this.Register         = (byte        )( Opcode       & 0x0F);
                this.AssemblyRegister =               ((Opcode >> 4) & 0x01) == 0x01;
                this.Action           = (MemoryAction)((Opcode >> 5) & 0x0F);
            }

            public Disassembler.Register GetRegister()
            {
                if (this.AssemblyRegister)
                {
                    return Assembly.Registers[this.Register];
                }
                else
                {
                    return Registers[this.Register];
                };
            }
        }

        public class Instruction
        {
            public string        Name          { get; private set; }
            public string        NameBrief     { get; private set; }
            public ArgumentMode  ArgumentMode  { get; private set; }

            public Instruction(string Name, string NameBrief, ArgumentMode ArgumentMode)
            {
                this.Name          = Name;
                this.NameBrief     = NameBrief;
                this.ArgumentMode  = ArgumentMode;
            }
        }
    }
}
