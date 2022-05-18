/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * SpinAssignments.cs
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

using System.Collections.ObjectModel;

namespace Gear.Propeller
{
    partial class Spin
    {
        /// <summary>Declaration of P1 %Spin %Assignments for variables.</summary>
        /// @version v22.05.02 - Fix security warning CA2105: 'Array fields
        /// should not be read only', adopting ReadOnlyCollection static
        /// object as storage.
        public static readonly ReadOnlyCollection<Assignment> Assignments =
            new ReadOnlyCollection<Assignment>(new[]
            {
                new Assignment(AssignmentTypeEnum.WriteRepeat, new[] {                                                                      //    000
                    new SubAssignment("COPY",                "dup",         false, ArgumentMode.None,               AssignmentSizeTypeEnum.Unspecified),  //  p000000-            write
                    new SubAssignment("REPEAT_COMPARE",      "Repeat",      false, ArgumentMode.SignedPackedOffset, AssignmentSizeTypeEnum.Unspecified),  //  -000001-            repeat-var loop      +1..2 address
                    new SubAssignment("REPEAT_COMPARE_STEP", "Repeat Step", false, ArgumentMode.SignedPackedOffset, AssignmentSizeTypeEnum.Unspecified)   //  -000011-            repeat-var pop step  +1..2 address
                }),
                new Assignment(AssignmentTypeEnum.Normal,      new[] {                                                                      //    001
                    new SubAssignment("FORWARD_RANDOM",      "a=?a",        false, ArgumentMode.None,               AssignmentSizeTypeEnum.Long),         //  p00010--    ?var    random forward (long)
                    new SubAssignment("REVERSE_RANDOM",      "a=a?",        true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Long)          //  p00011--    var?    random reverse (long)
                }),
                new Assignment(AssignmentTypeEnum.Normal,      new[] {                                                                      //    010
                    new SubAssignment("EXTEND_8",            "a=8<<a",      false, ArgumentMode.None,               AssignmentSizeTypeEnum.Byte),         //  p00100--    ~var    sign-extend byte
                    new SubAssignment("EXTEND_16",           "a=16<<a",     false, ArgumentMode.None,               AssignmentSizeTypeEnum.Word)          //  p00101--    ~~var   sign-extend word
                }),
                new Assignment(AssignmentTypeEnum.Normal,      new[] {                                                                      //    011
                    new SubAssignment("BIT_CLEAR",           "a=~",         true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Bit),          //  p00110--    var~    post-clear
                    new SubAssignment("BIT_SET",             "a=~~",        true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Bit)           //  p00111--    var~~   post-set
                }),
                new Assignment(AssignmentTypeEnum.Size,        new[] {                                                                      //    100
                    new SubAssignment("PRE_INCREMENT_BITS",  "++bits",      false, ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p010000-    ++var   pre-inc (bit)
                    new SubAssignment("PRE_INCREMENT_BYTE",  "++byte",      false, ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p010001-    ++var   pre-inc (byte)
                    new SubAssignment("PRE_INCREMENT_WORD",  "++word",      false, ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p010010-    ++var   pre-inc (word)
                    new SubAssignment("PRE_INCREMENT_LONG",  "++long",      false, ArgumentMode.None,               AssignmentSizeTypeEnum.Mask)          //  p010011-    ++var   pre-inc (long)
                }),
                new Assignment(AssignmentTypeEnum.Size,        new[] {                                                                      //    101
                    new SubAssignment("POST_INCREMENT_BITS", "bits++",      true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p010100-    var++   post-inc (bit)
                    new SubAssignment("POST_INCREMENT_BYTE", "byte++",      true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p010101-    var++   post-inc (byte)
                    new SubAssignment("POST_INCREMENT_WORD", "word++",      true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p010110-    var++   post-inc (word)
                    new SubAssignment("POST_INCREMENT_LONG", "long++",      true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Mask)          //  p010111-    var++   post-inc (long)
                }),
                new Assignment(AssignmentTypeEnum.Size,        new[] {                                                                      //    110
                    new SubAssignment("PRE_DECREMENT_BITS",  "--bits",      false, ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p011000-    --var   pre-dec (bit)
                    new SubAssignment("PRE_DECREMENT_BYTE",  "--byte",      false, ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p011001-    --var   pre-dec (byte)
                    new SubAssignment("PRE_DECREMENT_WORD",  "--word",      false, ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p011010-    --var   pre-dec (word)
                    new SubAssignment("PRE_DECREMENT_LONG",  "--long",      false, ArgumentMode.None,               AssignmentSizeTypeEnum.Mask)          //  p011011-    --var   pre-dec (long)
                }),
                new Assignment(AssignmentTypeEnum.Size,        new[] {                                                                      //    111
                    new SubAssignment("POST_DECREMENT_BITS", "bits--",      true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p011100-    var--   post-dec (bit)
                    new SubAssignment("POST_DECREMENT_BYTE", "byte--",      true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p011101-    var--   post-dec (byte)
                    new SubAssignment("POST_DECREMENT_WORD", "word--",      true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Mask),         //  p011110-    var--   post-dec (word)
                    new SubAssignment("POST_DECREMENT_LONG", "long--",      true,  ArgumentMode.None,               AssignmentSizeTypeEnum.Mask)          //  p011111-    var--   post-dec (long)
                })
            });
    }
}
