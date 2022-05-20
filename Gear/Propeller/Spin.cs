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

// ReSharper disable InconsistentNaming
namespace Gear.Propeller
{
    /// <summary></summary>
    public static partial class Spin
    {
        /// <summary></summary>
        public enum MemoryAction : byte
        {
            /// <summary></summary>
            UNKNOWN_0,
            /// <summary></summary>
            UNKNOWN_1,
            /// <summary></summary>
            UNKNOWN_2,
            /// <summary></summary>
            UNKNOWN_3,
            /// <summary></summary>
            PUSH,
            /// <summary></summary>
            POP,
            /// <summary></summary>
            EFFECT,
            /// <summary></summary>
            UNKNOWN_7
        }

        /// <summary></summary>
        public enum ArgumentMode : byte
        {
            /// <summary></summary>
            None,
            /// <summary></summary>
            Effect,
            /// <summary></summary>
            SignedOffset,
            /// <summary></summary>
            SignedPackedOffset,
            /// <summary></summary>
            PackedLiteral,
            /// <summary></summary>
            UnsignedOffset,
            /// <summary></summary>
            UnsignedEffectedOffset,
            /// <summary></summary>
            ByteLiteral,
            /// <summary></summary>
            WordLiteral,
            /// <summary></summary>
            NearLongLiteral,
            /// <summary></summary>
            LongLiteral,
            /// <summary></summary>
            ObjCallPair,
            /// <summary></summary>
            MemoryOpCode
        }

        /// <summary>Assignment type.</summary>
        public enum AssignmentTypeEnum : byte
        {
            /// <summary></summary>
            WriteRepeat,
            /// <summary></summary>
            Normal,
            /// <summary></summary>
            Size
        }

        /// <summary>Possible size of an assignment.</summary>
        public enum AssignmentSize : byte
        {
            /// <summary></summary>
            Bit,
            /// <summary></summary>
            Byte,
            /// <summary></summary>
            Word,
            /// <summary></summary>
            Long
        }

        /// <summary></summary>
        public enum AssignmentSizeTypeEnum : byte
        {
            /// <summary></summary>
            Unspecified,
            /// <summary></summary>
            Mask,
            /// <summary></summary>
            Bit,
            /// <summary></summary>
            Byte,
            /// <summary></summary>
            Word,
            /// <summary></summary>
            Long
        }

        /// <summary>Container for %Spin %Register.</summary>
        public class Register : Propeller.Register
        {
            /// <summary>Default Constructor.</summary>
            /// <param name="name">Name of %Spin register</param>
            public Register(string name)
            {
                Name = name;
            }
        }

        /// <summary>Container for %Spin %SubAssignment.</summary>
        public class SubAssignment : BasicInstruction
        {
            /// <summary></summary>
            /// @version v22.05.02 - Name changed to clarify meaning of it.
            public bool CanPost { get; }
            /// <summary></summary>
            public ArgumentMode ArgumentMode { get; }
            /// <summary></summary>
            /// @version v22.05.02 - Name changed to clarify meaning of it.
            public AssignmentSizeTypeEnum AssignmentSizeType { get; }

            /// <summary>Default Constructor.</summary>
            /// <param name="name">Full name of sub assignment.</param>
            /// <param name="nameBrief">Brief name of sub assignment.</param>
            /// <param name="postEnable"></param>
            /// <param name="argumentMode"></param>
            /// <param name="assignmentSizeType"></param>
            /// @version v22.05.02 - Changed parameters names to clarify meaning of each one.
            public SubAssignment(string name, string nameBrief, bool postEnable,
            ArgumentMode argumentMode, AssignmentSizeTypeEnum assignmentSizeType)
            {
                Name = name;
                NameBrief = nameBrief;
                CanPost = postEnable;
                ArgumentMode = argumentMode;
                AssignmentSizeType = assignmentSizeType;
            }
        }

        /// <summary>Container to define %Spin %Assignment for variables.</summary>
        public class Assignment
        {
            /// <summary>Type of assignment.</summary>
            /// @version v22.05.02 - Name changed to clarify meaning of it.
            public AssignmentTypeEnum AssignmentType { get; }
            /// <summary></summary>
            /// @version v22.05.02 - Name changed to clarify meaning of it.
            public SubAssignment[] SubAssignmentsArray { get; }

            /// <summary>Default Constructor.</summary>
            /// <param name="assignmentType"></param>
            /// <param name="subAssignmentsArray"></param>
            /// @version v22.05.02 - Changed parameters names to clarify
            /// meaning of each one.
            public Assignment(AssignmentTypeEnum assignmentType,
                SubAssignment[] subAssignmentsArray)
            {
                AssignmentType = assignmentType;
                SubAssignmentsArray = subAssignmentsArray;
            }
        }

        /// <summary>Container to define %Spin math instruction.</summary>
        public class MathInstruction : BasicInstruction
        {
            /// <summary>Default Constructor.</summary>
            /// <param name="name">Full name of math instruction.</param>
            /// <param name="nameBrief">Brief name of math instruction.</param>
            public MathInstruction(string name, string nameBrief)
            {
                Name = name;
                NameBrief = nameBrief;
            }
        }

        /// <summary>Container to define %Spin instructions.</summary>
        public class Instruction : BasicInstruction
        {
            /// <summary></summary>
            public ArgumentMode ArgumentMode { get; }

            /// <summary>Default Constructor.</summary>
            /// <param name="name">Full name of %Spin instruction.</param>
            /// <param name="nameBrief">Brief name of %Spin instruction.</param>
            /// <param name="argumentMode"></param>
            public Instruction(string name, string nameBrief,
                ArgumentMode argumentMode)
            {
                Name = name;
                NameBrief = nameBrief;
                ArgumentMode = argumentMode;
            }
        }
    }
}
