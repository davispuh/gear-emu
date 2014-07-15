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
            SignedPackedOffset,
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

        public enum AssignmentType
        {
            WriteRepeat,
            Normal,
            Size
        }

        public enum AssignmentSize
        {
            Bit,
            Byte,
            Word,
            Long
        }

        public enum AssignmentSizeType
        {
            Unspecified,
            Mask,
            Bit,
            Byte,
            Word,
            Long
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
            public uint Address          { get; private set; }
            public byte Register         { get; private set; }
            public bool AssemblyRegister { get; private set; }
            public MemoryAction Action   { get; private set; }

            public ParsedMemoryOperation(byte Opcode)
            {
                this.Opcode           = Opcode;
                this.Address          = (uint        )( Opcode       | 0x1E0);
                this.Register         = (byte        )( Opcode       & 0x00F);
                this.AssemblyRegister =               ((Opcode >> 4) & 0x001) == 0x01;
                this.Action           = (MemoryAction)((Opcode >> 5) & 0x00F);
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

        public class SubAssignment : Disassembler.BasicInstruction
        {
            public bool               Post         { get; private set; }
            public ArgumentMode       ArgumentMode { get; private set; }
            public AssignmentSizeType SizeType     { get; private set; }

            public SubAssignment(string Name, string NameBrief, bool Post, ArgumentMode ArgumentMode, AssignmentSizeType SizeType)
            {
                this.Name         = Name;
                this.NameBrief    = NameBrief;
                this.Post         = Post;
                this.ArgumentMode = ArgumentMode;
                this.SizeType     = SizeType;
            }
        }

        public class Assignment
        {
            public AssignmentType Type { get; private set; }
            public SubAssignment[] SubAssignments { get; private set; }

            public Assignment(AssignmentType Type, SubAssignment[] SubAssignments)
            {
                this.Type = Type;
                this.SubAssignments = SubAssignments;
            }

            public SubAssignment GetSubAssignment(ParsedAssignment ParsedAssignment)
            {
                switch (this.Type)
                {
                    case AssignmentType.WriteRepeat:
                        return this.SubAssignments[ParsedAssignment.Bit1 ? 1 : 0];
                    case AssignmentType.Normal:
                        return this.SubAssignments[ParsedAssignment.Bit2 ? 1 : 0];
                    case AssignmentType.Size:
                        return this.SubAssignments[(int)ParsedAssignment.Size];
                }
                throw new Exception("Uknown Assignment Type: " + this.Type.ToString());
            }
        }

        public class ParsedAssignment
        {
            public byte            Opcode           { get; private set; }
            public bool            Push             { get; private set; }
            public bool            Math             { get; private set; }
            public byte            ASG              { get; private set; }
            public byte            MTH              { get; private set; }
            public bool            Bit1             { get; private set; }
            public bool            Bit2             { get; private set; }
            public AssignmentSize  Size             { get; private set; }
            public bool            Swap             { get; private set; }
            public Assignment      SourceAssignment { get; private set; }
            public MathInstruction MathAssignment   { get; private set; }

            private SubAssignment SourceSubAssignment;

            public ParsedAssignment(byte Opcode)
            {
                this.Opcode =                   Opcode;
                this.Push   =                 ((Opcode >> 7) & 0x01) == 0x01;
                this.Math   =                 ((Opcode >> 6) & 0x01) == 0x01;
                this.ASG    =           (byte)((Opcode >> 3) & 0x07);
                this.MTH    =           (byte) (Opcode       & 0x1F);
                this.Bit1   =                 ((Opcode >> 1) & 0x01) == 0x01;
                this.Bit2   =                 ((Opcode >> 2) & 0x01) == 0x01;
                this.Size   = (AssignmentSize)((Opcode >> 1) & 0x03);
                this.Swap   =                 ((Opcode >> 5) & 0x01) == 0x01;

                if (this.Math)
                {
                    this.MathAssignment = MathInstructions[this.MTH];
                }
                else
                {
                    this.SourceAssignment = Assignments[this.ASG];
                }
                this.SourceSubAssignment = null;
            }

            public SubAssignment GetSubAssignment()
            {
                if (this.Math)
                {
                    return null;
                } else if (this.SourceSubAssignment == null)
                {
                    this.SourceSubAssignment = this.SourceAssignment.GetSubAssignment(this);
                }
                return this.SourceSubAssignment;
            }

            public Disassembler.BasicInstruction GetBasicInstruction()
            {
                if (this.Math)
                {
                    return this.MathAssignment;
                };
                return GetSubAssignment();
            }
        }

        public class MathInstruction : Disassembler.BasicInstruction
        {
            public MathInstruction(string Name, string NameBrief)
            {
                this.Name = Name;
                this.NameBrief = NameBrief;
            }
        }

        public class Instruction : Disassembler.BasicInstruction
        {
            public ArgumentMode ArgumentMode { get; private set; }

            public Instruction(string Name, string NameBrief, ArgumentMode ArgumentMode)
            {
                this.Name          = Name;
                this.NameBrief     = NameBrief;
                this.ArgumentMode  = ArgumentMode;
            }
        }
    }
}
