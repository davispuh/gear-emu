/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * InstructionDisassembler.cs
 * Provides a method for creating the string equivalent of a propeller operation
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
        static InstructionDisassembler()
        {
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

        private static uint GetPackedLiteral(PropellerCPU chip, ref uint address)
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

        private static int GetPackedOffset(PropellerCPU chip, ref uint address)
        {
            ushort op = chip.ReadByte(address++);

            if ((op & 0x80) == 0)
                return (op & 0x7F);

            op &= 0x7F;

            return (op << 8) | (chip.ReadByte(address++));
        }

        private static short GetSignedPackedOffset(PropellerCPU chip, ref uint address)
        {
            short op = chip.ReadByte(address++);
            bool extended = (op & 0x80) != 0;

            op = (short)((op & 0x7F) | ((op << 1) & 0x80));

            if (!extended)
                return (short)(sbyte)op;

            return (short)((op << 8) | chip.ReadByte(address++));
        }

        private static string GetEffectCode(PropellerCPU chip, ref uint address, bool useShortOpcodes)
        {
            Spin.ParsedAssignment ParsedAssignment = new Spin.ParsedAssignment(chip.ReadByte(address++));

            string effect = ParsedAssignment.Push ? "" : "POP ";

            if (useShortOpcodes)
            {
                effect += "(" + ParsedAssignment.GetBasicInstruction().NameBrief + ")";
            }
            else
            {
                effect += ParsedAssignment.GetBasicInstruction().Name;
            }

            if (!ParsedAssignment.Math)
            {
                Spin.SubAssignment SubAssignment = ParsedAssignment.GetSubAssignment();
                switch (SubAssignment.ArgumentMode)
                {
                    case Spin.ArgumentMode.None:
                        break;
                    case Spin.ArgumentMode.SignedPackedOffset:
                        effect += " " + GetSignedPackedOffset(chip, ref address);
                        break;
                    default:
                        throw new Exception("Unexpected Spin Argument Mode: " + SubAssignment.ArgumentMode.ToString());
                }
            }

            return effect;
        }

        public static string GetMemoryOp(PropellerCPU chip, ref uint address, bool useShortOpcodes)
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
                    return String.Format("EFFECT {0} {1}", Name, GetEffectCode(chip, ref address, useShortOpcodes));
                default:
                    return String.Format("UNKNOWN_{0} {1}", OP.Action, Name);
            }
        }

        public static string InterpreterText(PropellerCPU chip, ref uint address, bool displayAsHexadecimal, bool useShortOpcodes)
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
            }
            else
            {
                Name = Instr.Name;
            }

            switch (Instr.ArgumentMode)
            {
                case Spin.ArgumentMode.None:
                    return Name;
                case Spin.ArgumentMode.UnsignedOffset:
                    return String.Format(format, Name, GetPackedOffset(chip, ref address));
                case Spin.ArgumentMode.UnsignedEffectedOffset:
                    {
                        int arg = GetPackedOffset(chip, ref address);
                        format = "{0} {1} {2}";
                        return String.Format(format, Name, arg, GetEffectCode(chip, ref address, useShortOpcodes));
                    }
                case Spin.ArgumentMode.Effect:
                    return String.Format(format, Name, GetEffectCode(chip, ref address, useShortOpcodes));
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
                    return String.Format("{0} {1}", Name, GetMemoryOp(chip, ref address, useShortOpcodes));
                default:
                    throw new Exception("Uknown Spin Argument Mode: " + Instr.ArgumentMode.ToString());
            }
        }
    }
}
