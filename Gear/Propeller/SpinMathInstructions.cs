/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * SpinMathInstruction.cs
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
        static public readonly MathInstruction[] MathInstructions = new MathInstruction[] {
            new MathInstruction("ROTATE_RIGHT",      "b=b->a"),       //  00000    rotate right
            new MathInstruction("ROTATE_LEFT",       "b=b<-a"),       //  00001    rotate left
            new MathInstruction("SHIFT_RIGHT",       "b=b>>a"),       //  00010    shift right
            new MathInstruction("SHIFT_LEFT",        "b=b<<a"),       //  00011    shift left
            new MathInstruction("LIMIT_MIN",         "b=min(b,a)"),   //  00100    limit minimum (signed)
            new MathInstruction("LIMIT_MAX",         "b=max(b,a)"),   //  00101    limit maximum (signed)
            new MathInstruction("NEGATE",            "b=-b"),         //  00110    negate
            new MathInstruction("COMPLEMENT",        "b=inv(b)"),     //  00111    bitwise not
            new MathInstruction("BIT_AND",           "b&=a"),         //  01000    bitwise and
            new MathInstruction("ABSOLUTE_VALUE",    "a=||a"),        //  01001    absolute
            new MathInstruction("BIT_OR",            "b|=a"),         //  01010    bitwise or
            new MathInstruction("BIT_XOR",           "b^=a"),         //  01011    bitwise xor
            new MathInstruction("ADD",               "b+=a"),         //  01100    add
            new MathInstruction("SUBTRACT",          "b-=a"),         //  01101    subtract
            new MathInstruction("ARITH_SHIFT_RIGHT", "b=b~>a"),       //  01110    shift arithmetic right
            new MathInstruction("BIT_REVERSE",       "b=b><a"),       //  01111    reverse bits
            new MathInstruction("LOGICAL_AND",       "b=b AND a"),    //  10000    boolean and
            new MathInstruction("ENCODE",            "a=>|a"),        //  10001    encode (0-32)
            new MathInstruction("LOGICAL_OR",        "b=b OR a"),     //  10010    boolean or
            new MathInstruction("DECODE",            "a=|<a"),        //  10011    decode
            new MathInstruction("MULTIPLY",          "b*=a"),         //  10100    multiply, return lower half (signed)
            new MathInstruction("MULTIPLY_HI",       "b=(b*a)>>32"),  //  10101    multiply, return upper half (signed)
            new MathInstruction("DIVIDE",            "b/=a"),         //  10110    divide, return quotient (signed)
            new MathInstruction("MODULO",            "b=b mod a"),    //  10111    divide, return remainder (signed)
            new MathInstruction("SQUARE_ROOT",       "a=^^a"),        //  11000    square root
            new MathInstruction("LESS",              "b<=a"),         //  11001    test below (signed)
            new MathInstruction("GREATER",           "b>=a"),         //  11010    test above (signed)
            new MathInstruction("NOT_EQUAL",         "b<>=a"),        //  11011    test not equal
            new MathInstruction("EQUAL",             "b===a"),        //  11100    test equal
            new MathInstruction("LESS_EQUAL",        "b=<=a"),        //  11101    test below or equal (signed)
            new MathInstruction("GREATER_EQUAL",     "b=>=a"),        //  11110    test above or equal (signed)
            new MathInstruction("LOGICAL_NOT",       "a=!a")          //  11111    boolean not
        };
    }
}
