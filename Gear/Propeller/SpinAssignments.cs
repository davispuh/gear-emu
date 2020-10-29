/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2020 - Gear Developers
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

namespace Gear.Propeller
{
    public partial class Spin
    {
        static public readonly Assignment[] Assignments = new Assignment[] {
            new Assignment(AssignmentType.WriteRepeat, new SubAssignment[] {                                                                      //    000
                new SubAssignment("COPY",                "dup",         false, ArgumentMode.None,               AssignmentSizeType.Unspecified),  //  p000000-            write
                new SubAssignment("REPEAT_COMPARE",      "Repeat",      false, ArgumentMode.SignedPackedOffset, AssignmentSizeType.Unspecified),  //  -000001-            repeat-var loop      +1..2 address
                new SubAssignment("REPEAT_COMPARE_STEP", "Repeat Step", false, ArgumentMode.SignedPackedOffset, AssignmentSizeType.Unspecified)   //  -000011-            repeat-var pop step  +1..2 address
            }),
            new Assignment(AssignmentType.Normal,      new SubAssignment[] {                                                                      //    001
                new SubAssignment("FORWARD_RANDOM",      "a=?a",        false, ArgumentMode.None,               AssignmentSizeType.Long),         //  p00010--    ?var    random forward (long)
                new SubAssignment("REVERSE_RANDOM",      "a=a?",        true,  ArgumentMode.None,               AssignmentSizeType.Long)          //  p00011--    var?    random reverse (long)
            }),
            new Assignment(AssignmentType.Normal,      new SubAssignment[] {                                                                      //    010
                new SubAssignment("EXTEND_8",            "a=8<<a",      false, ArgumentMode.None,               AssignmentSizeType.Byte),         //  p00100--    ~var    sign-extend byte
                new SubAssignment("EXTEND_16",           "a=16<<a",     false, ArgumentMode.None,               AssignmentSizeType.Word)          //  p00101--    ~~var   sign-extend word
            }),
            new Assignment(AssignmentType.Normal,      new SubAssignment[] {                                                                      //    011
                new SubAssignment("BIT_CLEAR",           "a=~",         true,  ArgumentMode.None,               AssignmentSizeType.Bit),          //  p00110--    var~    post-clear
                new SubAssignment("BIT_SET",             "a=~~",        true,  ArgumentMode.None,               AssignmentSizeType.Bit)           //  p00111--    var~~   post-set
            }),
            new Assignment(AssignmentType.Size,        new SubAssignment[] {                                                                      //    100
                new SubAssignment("PRE_INCREMENT_BITS",  "++bits",      false, ArgumentMode.None,               AssignmentSizeType.Mask),         //  p010000-    ++var   pre-inc (bit)
                new SubAssignment("PRE_INCREMENT_BYTE",  "++byte",      false, ArgumentMode.None,               AssignmentSizeType.Mask),         //  p010001-    ++var   pre-inc (byte)
                new SubAssignment("PRE_INCREMENT_WORD",  "++word",      false, ArgumentMode.None,               AssignmentSizeType.Mask),         //  p010010-    ++var   pre-inc (word)
                new SubAssignment("PRE_INCREMENT_LONG",  "++long",      false, ArgumentMode.None,               AssignmentSizeType.Mask)          //  p010011-    ++var   pre-inc (long)
            }),
            new Assignment(AssignmentType.Size,        new SubAssignment[] {                                                                      //    101
                new SubAssignment("POST_INCREMENT_BITS", "bits++",      true,  ArgumentMode.None,               AssignmentSizeType.Mask),         //  p010100-    var++   post-inc (bit)
                new SubAssignment("POST_INCREMENT_BYTE", "byte++",      true,  ArgumentMode.None,               AssignmentSizeType.Mask),         //  p010101-    var++   post-inc (byte)
                new SubAssignment("POST_INCREMENT_WORD", "word++",      true,  ArgumentMode.None,               AssignmentSizeType.Mask),         //  p010110-    var++   post-inc (word)
                new SubAssignment("POST_INCREMENT_LONG", "long++",      true,  ArgumentMode.None,               AssignmentSizeType.Mask)          //  p010111-    var++   post-inc (long)
            }),
            new Assignment(AssignmentType.Size,        new SubAssignment[] {                                                                      //    110
                new SubAssignment("PRE_DECREMENT_BITS",  "--bits",      false, ArgumentMode.None,               AssignmentSizeType.Mask),         //  p011000-    --var   pre-dec (bit)
                new SubAssignment("PRE_DECREMENT_BYTE",  "--byte",      false, ArgumentMode.None,               AssignmentSizeType.Mask),         //  p011001-    --var   pre-dec (byte)
                new SubAssignment("PRE_DECREMENT_WORD",  "--word",      false, ArgumentMode.None,               AssignmentSizeType.Mask),         //  p011010-    --var   pre-dec (word)
                new SubAssignment("PRE_DECREMENT_LONG",  "--long",      false, ArgumentMode.None,               AssignmentSizeType.Mask)          //  p011011-    --var   pre-dec (long)
            }),
            new Assignment(AssignmentType.Size,        new SubAssignment[] {                                                                      //    111
                new SubAssignment("POST_DECREMENT_BITS", "bits--",      true,  ArgumentMode.None,               AssignmentSizeType.Mask),         //  p011100-    var--   post-dec (bit)
                new SubAssignment("POST_DECREMENT_BYTE", "byte--",      true,  ArgumentMode.None,               AssignmentSizeType.Mask),         //  p011101-    var--   post-dec (byte)
                new SubAssignment("POST_DECREMENT_WORD", "word--",      true,  ArgumentMode.None,               AssignmentSizeType.Mask),         //  p011110-    var--   post-dec (word)
                new SubAssignment("POST_DECREMENT_LONG", "long--",      true,  ArgumentMode.None,               AssignmentSizeType.Mask)          //  p011111-    var--   post-dec (long)
            })
        };
    }
}
