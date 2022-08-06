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
    /// <summary>SPIN Language definitions.</summary>
    public static partial class Spin
    {
        /// <summary>Type of SPIN memory action.</summary>
        /// @version v22.07.xx - Renamed enum to be more meaningfully.
        public enum MemoryActionEnum : byte
        {
            /// <summary></summary>
            UNKNOWN_0,
            /// <summary></summary>
            UNKNOWN_1,
            /// <summary></summary>
            UNKNOWN_2,
            /// <summary></summary>
            UNKNOWN_3,
            /// <summary>Push a value into stack.</summary>
            PUSH,
            /// <summary>Remove a value from stack.</summary>
            POP,
            /// <summary></summary>
            EFFECT,
            /// <summary></summary>
            UNKNOWN_7
        }

        /// <summary>Type of argument mode.</summary>
        /// @version v22.07.xx - Renamed enum to be more meaningfully.
        public enum ArgumentModeEnum : byte
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
            /// <summary>Bit - 1 bit.</summary>
            Bit,
            /// <summary>Byte - 8 bits.</summary>
            Byte,
            /// <summary>Word - 16 bits.</summary>
            Word,
            /// <summary>Long - 32 bits.</summary>
            Long
        }

        /// <summary>Types of assignment sizes.</summary>
        public enum AssignmentSizeTypeEnum : byte
        {
            /// <summary>Without an assignment size.</summary>
            Unspecified,
            /// <summary>Mask assignment size.</summary>
            Mask,
            /// <summary>Bit assignment size - 1 bit.</summary>
            Bit,
            /// <summary>Byte assignment size - 8 bits.</summary>
            Byte,
            /// <summary>Word assignment size - 16 bits.</summary>
            Word,
            /// <summary>Long assignment size - 32 bits.</summary>
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

        /// <summary>Container for %Spin %AssignmentVariant definitions.</summary>
        /// @version v22.07.xx - Changed class name to be more meaningful from
        /// former `SubAssignment`.
        public class AssignmentVariant : BasicInstruction
        {
            /// <summary></summary>
            /// @version v22.05.02 - Name changed to clarify meaning of it.
            public bool CanPost { get; }

            /// <summary></summary>
            public ArgumentModeEnum ArgumentMode { get; }

            /// <summary>Return the type of assignment size.</summary>
            /// @version v22.05.02 - Name changed to clarify meaning of it.
            public AssignmentSizeTypeEnum AssignmentSizeType { get; }

            /// <summary>Default Constructor.</summary>
            /// <param name="name">Full name of assignment variant.</param>
            /// <param name="nameBrief">Brief name of assignment variant.</param>
            /// <param name="postEnable"></param>
            /// <param name="argumentMode">Type of argument mode.</param>
            /// <param name="assignmentSizeType">Type of assignment size.</param>
            /// @version v22.07.xx - Renamed class constructor to follow class
            /// name was changed.
            public AssignmentVariant(string name, string nameBrief, bool postEnable,
            ArgumentModeEnum argumentMode, AssignmentSizeTypeEnum assignmentSizeType)
            {
                Name = name;
                NameBrief = nameBrief;
                CanPost = postEnable;
                ArgumentMode = argumentMode;
                AssignmentSizeType = assignmentSizeType;
            }
        }

        /// <summary>Container to define %Spin %Assignment instances for
        /// variables.</summary>
        public class Assignment
        {
            /// <summary>Type of assignment.</summary>
            /// @version v22.05.02 - Name changed to clarify meaning of it.
            public AssignmentTypeEnum AssignmentType { get; }

            /// <summary>Return the Assignment Variants Array associated to
            /// this Assignment object.</summary>
            /// @version v22.07.xx - Name changed to clarify meaning of it,
            /// from former `SubAssignmentsArray`.
            public AssignmentVariant[] AssignmentVariantsArray { get; }

            /// <summary>Default Constructor.</summary>
            /// <param name="assignmentType">Assignment type.</param>
            /// <param name="assignmentVariantsArray">Array of Assignment
            /// Variants for this object.</param>
            /// @version v22.07.xx - Changed parameter name following name
            /// change of its type.
            public Assignment(AssignmentTypeEnum assignmentType,
                AssignmentVariant[] assignmentVariantsArray)
            {
                AssignmentType = assignmentType;
                AssignmentVariantsArray = assignmentVariantsArray;
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

        /// <summary>Container to define %Spin instructions instances.</summary>
        public class Instruction : BasicInstruction
        {
            /// <summary>Return Argument Mode associated to this instruction.</summary>
            public ArgumentModeEnum ArgumentMode { get; }

            /// <summary>Default Constructor.</summary>
            /// <param name="name">Full name of %Spin instruction.</param>
            /// <param name="nameBrief">Brief name of %Spin instruction.</param>
            /// <param name="argumentMode">Type of argument mode.</param>
            public Instruction(string name, string nameBrief,
                ArgumentModeEnum argumentMode)
            {
                Name = name;
                NameBrief = nameBrief;
                ArgumentMode = argumentMode;
            }
        }
    }
}
