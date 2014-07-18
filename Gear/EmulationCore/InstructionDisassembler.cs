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
                        effect += " " + DataUnpacker.GetSignedPackedOffset(memory);
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
                    return String.Format(format, Name, DataUnpacker.GetPackedOffset(memory));
                case Spin.ArgumentMode.UnsignedEffectedOffset:
                    return String.Format("{0} {1} {2}", Name, DataUnpacker.GetPackedOffset(memory), GetEffectCode(memory, useShortOpcodes));
                case Spin.ArgumentMode.Effect:
                    return String.Format(format, Name, GetEffectCode(memory, useShortOpcodes));
                case Spin.ArgumentMode.SignedOffset:
                    return String.Format(format, Name, DataUnpacker.GetSignedOffset(memory));
                case Spin.ArgumentMode.PackedLiteral:
                    return String.Format(format, Name, DataUnpacker.GetPackedLiteral(memory));
                case Spin.ArgumentMode.ByteLiteral:
                    return String.Format(format, Name, memory.ReadByte());
                case Spin.ArgumentMode.WordLiteral:
                    return String.Format(format, Name, DataUnpacker.GetWordLiteral(memory));
                case Spin.ArgumentMode.NearLongLiteral:
                    return String.Format(format, Name, DataUnpacker.GetNearLongLiteral(memory));
                case Spin.ArgumentMode.LongLiteral:
                    return String.Format(format, Name, DataUnpacker.GetLongLiteral(memory));
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
