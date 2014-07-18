using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Propeller
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

        public class Register : Propeller.Register
        {
            public Register(string Name)
            {
                this.Name = Name;
            }
        }

        public class SubAssignment : Propeller.BasicInstruction
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
        }

        public class MathInstruction : Propeller.BasicInstruction
        {
            public MathInstruction(string Name, string NameBrief)
            {
                this.Name = Name;
                this.NameBrief = NameBrief;
            }
        }

        public class Instruction : Propeller.BasicInstruction
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
