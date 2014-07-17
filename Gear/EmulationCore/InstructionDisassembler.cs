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

        private static uint GetPackedLiteral(MemoryManager memory)
        {
            byte op = memory.ReadByte();

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

        private static int GetPackedOffset(MemoryManager memory)
        {
            ushort op = memory.ReadByte();

            if ((op & 0x80) == 0)
                return (op & 0x7F);

            op &= 0x7F;

            return (op << 8) | (memory.ReadByte());
        }

        private static short GetSignedPackedOffset(MemoryManager memory)
        {
            short op = memory.ReadByte();
            bool extended = (op & 0x80) != 0;

            op = (short)((op & 0x7F) | ((op << 1) & 0x80));

            if (!extended)
                return (short)(sbyte)op;

            return (short)((op << 8) | memory.ReadByte());
        }

        private static string GetEffectCode(MemoryManager memory, bool useShortOpcodes)
        {
            Spin.ParsedAssignment ParsedAssignment = new Spin.ParsedAssignment(memory.ReadByte());

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
                        effect += " " + GetSignedPackedOffset(memory);
                        break;
                    default:
                        throw new Exception("Unexpected Spin Argument Mode: " + SubAssignment.ArgumentMode.ToString());
                }
            }

            return effect;
        }

        public static string GetMemoryOp(MemoryManager memory, bool useShortOpcodes)
        {
            Spin.ParsedMemoryOperation OP = new Spin.ParsedMemoryOperation(memory.ReadByte());

            string Name = OP.GetRegister().Name;

            switch (OP.Action)
            {
                case Spin.MemoryAction.PUSH:
                    return String.Format("PUSH {0}", Name);
                case Spin.MemoryAction.POP:
                    return String.Format("POP {0}", Name);
                case Spin.MemoryAction.EFFECT:
                    return String.Format("EFFECT {0} {1}", Name, GetEffectCode(memory, useShortOpcodes));
                default:
                    return String.Format("UNKNOWN_{0} {1}", OP.Action, Name);
            }
        }

        public static string InterpreterText(MemoryManager memory, bool displayAsHexadecimal, bool useShortOpcodes)
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

            Spin.Instruction Instr = Spin.Instructions[memory.ReadByte()];

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
                    return String.Format(format, Name, GetPackedOffset(memory));
                case Spin.ArgumentMode.UnsignedEffectedOffset:
                    {
                        int arg = GetPackedOffset(memory);
                        format = "{0} {1} {2}";
                        return String.Format(format, Name, arg, GetEffectCode(memory, useShortOpcodes));
                    }
                case Spin.ArgumentMode.Effect:
                    return String.Format(format, Name, GetEffectCode(memory, useShortOpcodes));
                case Spin.ArgumentMode.SignedOffset:
                    {
                        uint result = memory.ReadByte();

                        if ((result & 0x80) == 0)
                        {
                            if ((result & 0x40) != 0)
                                result |= 0xFFFFFF80;
                        }
                        else
                        {
                            result = (result << 8) | memory.ReadByte();

                            if ((result & 0x4000) != 0)
                                result |= 0xFFFF8000;
                        }
                        return String.Format(format, Name, (int)result);
                    }
                case Spin.ArgumentMode.PackedLiteral:
                    return String.Format(format, Name, GetPackedLiteral(memory));
                case Spin.ArgumentMode.ByteLiteral:
                    return String.Format(format, Name, memory.ReadByte());
                case Spin.ArgumentMode.WordLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 2; i++)
                        {
                            result <<= 8;
                            result |= memory.ReadByte();
                        }
                        return String.Format(format, Name, result);
                    }
                case Spin.ArgumentMode.NearLongLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            result <<= 8;
                            result |= memory.ReadByte();
                        }
                        return String.Format(format, Name, result);
                    }
                case Spin.ArgumentMode.LongLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            result <<= 8;
                            result |= memory.ReadByte();
                        }
                        return String.Format(format, Name, result);
                    }
                case Spin.ArgumentMode.ObjCallPair:
                    {
                        byte obj = memory.ReadByte();
                        byte funct = memory.ReadByte();
                        return String.Format("{0} {1}.{2}", Name, obj, funct);
                    }
                case Spin.ArgumentMode.MemoryOpCode:
                    return String.Format("{0} {1}", Name, GetMemoryOp(memory, useShortOpcodes));
                default:
                    throw new Exception("Uknown Spin Argument Mode: " + Instr.ArgumentMode.ToString());
            }
        }
    }
}
