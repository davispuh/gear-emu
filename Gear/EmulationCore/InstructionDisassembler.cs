/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * InstructionDisassembler.cs
 * Provides a method for creating the string equlivilent of a propeller operation
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

using System;
using System.Collections.Generic;
using System.Text;
using Gear.Disassembler;

namespace Gear.EmulationCore
{
    public static class InstructionDisassembler
    {

        static private Dictionary<byte, string> EffectCodes;
        static private Dictionary<byte, string> BriefEffectCodes;
        static private Dictionary<byte, string> BigEffectCodes;

        static InstructionDisassembler()
        {
            BigEffectCodes = new Dictionary<byte, string>();

            BigEffectCodes[0x0] = "COPY";
            BigEffectCodes[0x8] = "FORWARD_RANDOM";
            BigEffectCodes[0xC] = "REVERSE_RANDOM";
            BigEffectCodes[0x10] = "EXTEND_8";
            BigEffectCodes[0x14] = "EXTEND_16";
            BigEffectCodes[0x18] = "BIT_CLEAR";
            BigEffectCodes[0x1C] = "BIT_SET";
            BigEffectCodes[0x20] = "PRE_INCREMENT_BITS";
            BigEffectCodes[0x22] = "PRE_INCREMENT_BYTE";
            BigEffectCodes[0x24] = "PRE_INCREMENT_WORD";
            BigEffectCodes[0x26] = "PRE_INCREMENT_LONG";
            BigEffectCodes[0x28] = "POST_INCREMENT_BITS";
            BigEffectCodes[0x2A] = "POST_INCREMENT_BYTE";
            BigEffectCodes[0x2C] = "POST_INCREMENT_WORD";
            BigEffectCodes[0x2E] = "POST_INCREMENT_LONG";
            BigEffectCodes[0x30] = "PRE_DECREMENT_BITS";
            BigEffectCodes[0x32] = "PRE_DECREMENT_BYTE";
            BigEffectCodes[0x34] = "PRE_DECREMENT_WORD";
            BigEffectCodes[0x36] = "PRE_DECREMENT_LONG";
            BigEffectCodes[0x38] = "POST_DECREMENT_BITS";
            BigEffectCodes[0x3A] = "POST_DECREMENT_BYTE";
            BigEffectCodes[0x3C] = "POST_DECREMENT_WORD";
            BigEffectCodes[0x3E] = "POST_DECREMENT_LONG";
            BigEffectCodes[0x40] = "ROTATE_RIGHT";
            BigEffectCodes[0x41] = "ROTATE_LEFT";
            BigEffectCodes[0x42] = "SHIFT_RIGHT";
            BigEffectCodes[0x43] = "SHIFT_LEFT";
            BigEffectCodes[0x44] = "MINIMUM";
            BigEffectCodes[0x45] = "MAXIMUM";
            BigEffectCodes[0x46] = "NEGATE";
            BigEffectCodes[0x47] = "COMPLEMENT";
            BigEffectCodes[0x48] = "BIT_AND";
            BigEffectCodes[0x49] = "ABSOLUTE_VALUE";
            BigEffectCodes[0x4A] = "BIT_OR";
            BigEffectCodes[0x4B] = "BIT_XOR";
            BigEffectCodes[0x4C] = "ADD";
            BigEffectCodes[0x4D] = "SUBTRACT";
            BigEffectCodes[0x4E] = "ARITH_SHIFT_RIGHT";
            BigEffectCodes[0x4F] = "BIT_REVERSE";
            BigEffectCodes[0x50] = "LOGICAL_AND";
            BigEffectCodes[0x51] = "ENCODE";
            BigEffectCodes[0x52] = "LOGICAL_OR";
            BigEffectCodes[0x53] = "DECODE";
            BigEffectCodes[0x54] = "MULTIPLY";
            BigEffectCodes[0x55] = "MULTIPLY_HI";
            BigEffectCodes[0x56] = "DIVIDE";
            BigEffectCodes[0x57] = "MODULO";
            BigEffectCodes[0x58] = "SQUARE_ROOT";
            BigEffectCodes[0x59] = "LESS";
            BigEffectCodes[0x5A] = "GREATER";
            BigEffectCodes[0x5B] = "NOT_EQUAL";
            BigEffectCodes[0x5C] = "EQUAL";
            BigEffectCodes[0x5D] = "LESS_EQUAL";
            BigEffectCodes[0x5E] = "GREATER_EQUAL";
            BigEffectCodes[0x5F] = "NOT";

            BriefEffectCodes = new Dictionary<byte, string>();

            BriefEffectCodes[0x0] = "(dup)";
            BriefEffectCodes[0x8] = "(a=?a)";
            BriefEffectCodes[0xC] = "(a=a?)";
            BriefEffectCodes[0x10] = "(a=8<<a)";
            BriefEffectCodes[0x14] = "(a=16<<a)";
            BriefEffectCodes[0x18] = "(a=~)";
            BriefEffectCodes[0x1C] = "(a=~~)";
            BriefEffectCodes[0x20] = "(++bits)";
            BriefEffectCodes[0x22] = "(++byte)";
            BriefEffectCodes[0x24] = "(++word)";
            BriefEffectCodes[0x26] = "(++long)";
            BriefEffectCodes[0x28] = "(bits++)";
            BriefEffectCodes[0x2A] = "(byte++)";
            BriefEffectCodes[0x2C] = "(word++)";
            BriefEffectCodes[0x2E] = "(long++)";
            BriefEffectCodes[0x30] = "(--bits)";
            BriefEffectCodes[0x32] = "(--byte)";
            BriefEffectCodes[0x34] = "(--word)";
            BriefEffectCodes[0x36] = "(--long)";
            BriefEffectCodes[0x38] = "(bits--)";
            BriefEffectCodes[0x3A] = "(byte--)";
            BriefEffectCodes[0x3C] = "(word--)";
            BriefEffectCodes[0x3E] = "(long--)";
            BriefEffectCodes[0x40] = "(b=b->a)";
            BriefEffectCodes[0x41] = "(b=b<-a)";
            BriefEffectCodes[0x42] = "(b=b>>a)";
            BriefEffectCodes[0x43] = "(b=b<<a)";
            BriefEffectCodes[0x44] = "(b=min(b,a))";
            BriefEffectCodes[0x45] = "(b=max(b,a))";
            BriefEffectCodes[0x46] = "(b=-b)";
            BriefEffectCodes[0x47] = "(b=inv(b))";
            BriefEffectCodes[0x48] = "(b&=a)";
            BriefEffectCodes[0x49] = "(a=||a)";
            BriefEffectCodes[0x4A] = "(b|=a)";
            BriefEffectCodes[0x4B] = "(b^=a)";
            BriefEffectCodes[0x4C] = "(b+=a)";
            BriefEffectCodes[0x4D] = "(b-=a)";
            BriefEffectCodes[0x4E] = "(b=b~>a)";
            BriefEffectCodes[0x4F] = "(b=b><a)";
            BriefEffectCodes[0x50] = "(b=b AND a)";
            BriefEffectCodes[0x51] = "(a=>|a)";
            BriefEffectCodes[0x52] = "(b=b OR a)";
            BriefEffectCodes[0x53] = "(a=|<a)";
            BriefEffectCodes[0x54] = "(b*=a)";
            BriefEffectCodes[0x55] = "(b=(b*a)>>32";
            BriefEffectCodes[0x56] = "(b/=a)";
            BriefEffectCodes[0x57] = "(b=b mod a)";
            BriefEffectCodes[0x58] = "(a=^^a)";
            BriefEffectCodes[0x59] = "(b<=a)";
            BriefEffectCodes[0x5A] = "(b>=a)";
            BriefEffectCodes[0x5B] = "(b<>=a)";
            BriefEffectCodes[0x5C] = "(b===a)";
            BriefEffectCodes[0x5D] = "(b=<=a)";
            BriefEffectCodes[0x5E] = "(b=>=a)";
            BriefEffectCodes[0x5F] = "(a=!a)";

            EffectCodes = BigEffectCodes;
        }

        public static string AssemblyText(uint Operation)
        {
            Assembly.ParsedInstruction instr = new Assembly.ParsedInstruction(Operation);
            string text;
            if (instr.CON > 0x00)
            {
                Assembly.SubInstruction ActualInstruction = instr.GetSubInstruction();

                string SrcString = "";
                string DestString = "";

                if (ActualInstruction.Source)
                {
                    if (instr.SRC >= Assembly.RegisterBaseAddress)
                    {
                        SrcString = String.Format("{0}{1}",
                                instr.ImmediateValue() ? "#" : "",
                                Assembly.Registers[instr.SRC - Assembly.RegisterBaseAddress].Name);
                    }
                    else
                    {
                        SrcString = String.Format("{0}${1:X3}",
                                instr.ImmediateValue() ? "#" : "",
                                instr.SRC);
                    }
                }

                if (ActualInstruction.Destination)
                {
                    if (instr.DEST >= Assembly.RegisterBaseAddress)
                    {
                        DestString = String.Format("{0}", Assembly.Registers[instr.DEST - Assembly.RegisterBaseAddress].Name);
                    }
                    else
                    {
                        DestString = String.Format("${0:X3}", instr.DEST);
                    }
                }

                text = String.Format("{0} {1} {2}{3}{4}", new object[] {
                    Assembly.Conditions[instr.CON][0],
                    ActualInstruction.Name,
                    DestString,
                    (ActualInstruction.Source && ActualInstruction.Destination) ? ", " : "",
                    SrcString }
                );

                if (instr.WriteResult())
                {
                    text += " WR";
                }

                if (instr.NoResult())
                {
                    text += " NR";
                }

                if (instr.WriteZero())
                {
                    text += " WZ";
                }

                if (instr.WriteCarry())
                {
                    text += " WC";
                }
            }
            else
            {
                text = String.Format("{0} {1}", new object[] { Assembly.Conditions[0][1], Assembly.Conditions[0][2] });
            }
            return text;
        }

        private static uint GetPackedLiteral(Propeller chip, ref uint address)
        {
            byte op = chip.ReadByte(address++);

            if (op >= 0x80)
                // TODO: COMPLAIN!
                return 0x55555555;

            uint data = (uint)2 << (op & 0x1F);

            if ((op & 0x20) != 0)
                data--;
            if ((op & 0x40) != 0)
                data = ~data;

            return data;
        }

        private static int GetPackedOffset(Propeller chip, ref uint address)
        {
            ushort op = chip.ReadByte(address++);

            if ((op & 0x80) == 0)
                return (op & 0x7F);

            op &= 0x7F;

            return (op << 8) | (chip.ReadByte(address++));
        }

        private static short GetSignedPackedOffset(Propeller chip, ref uint address)
        {
            short op = chip.ReadByte(address++);
            bool extended = (op & 0x80) != 0;

            op = (short)((op & 0x7F) | ((op << 1) & 0x80));

            if (!extended)
                return (short)(sbyte)op;

            return (short)((op << 8) | chip.ReadByte(address++));
        }

        private static string GetEffectCode(Propeller chip, ref uint address)
        {
            byte op = chip.ReadByte(address++);
            string effect = ((op & 0x80) == 0) ? "POP " : "";
            op &= 0x7F;

            if (op == 0x02)
            {
                effect += String.Format("REPEAT_COMPARE {0}", GetSignedPackedOffset(chip, ref address));
            }
            else if (op == 0x06)
            {
                effect += String.Format("REPEAT_COMPARE_STEP {0}", GetSignedPackedOffset(chip, ref address));
            }
            else if (EffectCodes.ContainsKey(op))
            {
                effect += EffectCodes[op];
            }
            else
            {
                effect += String.Format("UNKNOWN_EFFECT_{0:X}", op);
            }

            return effect;
        }

        public static string GetMemoryOp(Propeller chip, ref uint address)
        {
            Spin.ParsedMemoryOperation OP = new Spin.ParsedMemoryOperation(chip.ReadByte(address++));

            string Name = OP.GetRegister().Name;

            switch (OP.Action)
            {
                case Spin.MemoryAction.PUSH:
                    return String.Format("PUSH {0}", Name);
                case Spin.MemoryAction.POP:
                    return String.Format("POP {0}", Name);
                case Spin.MemoryAction.EFFECT:
                    return String.Format("EFFECT {0} {1}", Name, GetEffectCode(chip, ref address));
                default:
                    return String.Format("UNKNOWN_{0} {1}", OP.Action, Name);
            }
        }

        public static string InterpreterText(Propeller chip, ref uint address, bool displayAsHexadecimal, bool useShortOpcodes)
        {
            string format;
            if (displayAsHexadecimal)
            {
                format = "{0} ${1:X}";
            }
            else
            {
                format = "{0} {1}";
            }

            Spin.Instruction Instr = Spin.Instructions[chip.ReadByte(address++)];

            string Name;
            if (useShortOpcodes)
            {
                Name = Instr.NameBrief;
                EffectCodes = BriefEffectCodes;
            }
            else
            {
                Name = Instr.Name;
                EffectCodes = BigEffectCodes;
            }

            switch (Instr.ArgumentMode)
            {
                case Spin.ArgumentMode.UnsignedOffset:
                    return String.Format(format, Name, GetPackedOffset(chip, ref address));
                case Spin.ArgumentMode.UnsignedEffectedOffset:
                    {
                        int arg = GetPackedOffset(chip, ref address);
                        format = "{0} {1} {2}";
                        return String.Format(format, Name, arg, GetEffectCode(chip, ref address));
                    }
                case Spin.ArgumentMode.Effect:
                    return String.Format(format, Name, GetEffectCode(chip, ref address));
                case Spin.ArgumentMode.SignedOffset:
                    {
                        uint result = chip.ReadByte(address++);

                        if ((result & 0x80) == 0)
                        {
                            if ((result & 0x40) != 0)
                                result |= 0xFFFFFF80;
                        }
                        else
                        {
                            result = (result << 8) | chip.ReadByte(address++);

                            if ((result & 0x4000) != 0)
                                result |= 0xFFFF8000;
                        }
                        return String.Format(format, Name, (int)result);
                    }
                case Spin.ArgumentMode.PackedLiteral:
                    return String.Format(format, Name, GetPackedLiteral(chip, ref address));
                case Spin.ArgumentMode.ByteLiteral:
                    return String.Format(format, Name, chip.ReadByte(address++));
                case Spin.ArgumentMode.WordLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 2; i++)
                        {
                            result <<= 8;
                            result |= chip.ReadByte(address++);
                        }
                        return String.Format(format, Name, result);
                    }
                case Spin.ArgumentMode.NearLongLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            result <<= 8;
                            result |= chip.ReadByte(address++);
                        }
                        return String.Format(format, Name, result);
                    }
                case Spin.ArgumentMode.LongLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            result <<= 8;
                            result |= chip.ReadByte(address++);
                        }
                        return String.Format(format, Name, result);
                    }
                case Spin.ArgumentMode.ObjCallPair:
                    {
                        byte obj = chip.ReadByte(address++);
                        byte funct = chip.ReadByte(address++);
                        return String.Format("{0} {1}.{2}", Name, obj, funct);
                    }
                case Spin.ArgumentMode.MemoryOpCode:
                    return String.Format("{0} {1}", Name, GetMemoryOp(chip, ref address));
            }

            return Name;
        }
    }
}
