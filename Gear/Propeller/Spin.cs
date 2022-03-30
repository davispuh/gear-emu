/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * Spin.cs
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
                this.Type           = Type;
                this.SubAssignments = SubAssignments;
            }
        }

        public class MathInstruction : Propeller.BasicInstruction
        {
            public MathInstruction(string Name, string NameBrief)
            {
                this.Name      = Name;
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
