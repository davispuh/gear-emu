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
                Propeller.Assembly.SubInstruction ActualInstruction = instr.GetSubInstruction();

                string SrcString = string.Empty;
                string DestString = string.Empty;

                if (ActualInstruction.Source)
                {
                    if (instr.SRC >= Propeller.Assembly.RegisterBaseAddress)
                    {
                        SrcString = String.Format("{0}{1}",
                                instr.ImmediateValue() ? "#" : string.Empty,
                                Propeller.Assembly.Registers[instr.SRC - Propeller.Assembly.RegisterBaseAddress].Name);
                    }
                    else
                    {
                        SrcString = String.Format("{0}${1:X3}",
                                instr.ImmediateValue() ? "#" : string.Empty,
                                instr.SRC);
                    }
                }

                if (ActualInstruction.Destination)
                {
                    if (instr.DEST >= Propeller.Assembly.RegisterBaseAddress)
                    {
                        DestString = String.Format("{0}", Propeller.Assembly.Registers[instr.DEST - Propeller.Assembly.RegisterBaseAddress].Name);
                    }
                    else
                    {
                        DestString = String.Format("${0:X3}", instr.DEST);
                    }
                }

                text = String.Format("{0} {1} {2}{3}{4}", new object[] {
                    Propeller.Assembly.Conditions[instr.CON][0],
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
                text = String.Format("{0} {1}", new object[] { Propeller.Assembly.Conditions[0][1], Propeller.Assembly.Conditions[0][2] });
            }
            return text;
        }

        private static string GetEffectCode(Propeller.MemoryManager memory, bool useShortOpcodes)
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
                Propeller.Spin.SubAssignment SubAssignment = ParsedAssignment.GetSubAssignment();
                switch (SubAssignment.ArgumentMode)
                {
                    case Propeller.Spin.ArgumentMode.None:
                        break;
                    case Propeller.Spin.ArgumentMode.SignedPackedOffset:
                        effect += " " + DataUnpacker.GetSignedPackedOffset(memory);
                        break;
                    default:
                        throw new Exception("Unexpected Spin Argument Mode: " + SubAssignment.ArgumentMode.ToString());
                }
            }

            return effect;
        }

        public static string GetMemoryOp(Propeller.MemoryManager memory, bool useShortOpcodes)
        {
            Spin.ParsedMemoryOperation OP = new Spin.ParsedMemoryOperation(memory.ReadByte());

            string Name = OP.GetRegister().Name;

            switch (OP.Action)
            {
                case Propeller.Spin.MemoryAction.PUSH:
                    return String.Format("PUSH {0}", Name);
                case Propeller.Spin.MemoryAction.POP:
                    return String.Format("POP {0}", Name);
                case Propeller.Spin.MemoryAction.EFFECT:
                    return String.Format("EFFECT {0} {1}", Name, GetEffectCode(memory, useShortOpcodes));
                default:
                    return String.Format("UNKNOWN_{0} {1}", OP.Action, Name);
            }
        }

        public static string InterpreterText(Propeller.MemoryManager memory, bool displayAsHexadecimal, bool useShortOpcodes)
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

            Propeller.Spin.Instruction Instr = Propeller.Spin.Instructions[memory.ReadByte()];

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
                case Propeller.Spin.ArgumentMode.None:
                    return Name;
                case Propeller.Spin.ArgumentMode.UnsignedOffset:
                    return String.Format(format, Name, DataUnpacker.GetPackedOffset(memory));
                case Propeller.Spin.ArgumentMode.UnsignedEffectedOffset:
                    return String.Format("{0} {1} {2}", Name, DataUnpacker.GetPackedOffset(memory), GetEffectCode(memory, useShortOpcodes));
                case Propeller.Spin.ArgumentMode.Effect:
                    return String.Format(format, Name, GetEffectCode(memory, useShortOpcodes));
                case Propeller.Spin.ArgumentMode.SignedOffset:
                    return String.Format(format, Name, DataUnpacker.GetSignedOffset(memory));
                case Propeller.Spin.ArgumentMode.PackedLiteral:
                    return String.Format(format, Name, DataUnpacker.GetPackedLiteral(memory));
                case Propeller.Spin.ArgumentMode.ByteLiteral:
                    return String.Format(format, Name, memory.ReadByte());
                case Propeller.Spin.ArgumentMode.WordLiteral:
                    return String.Format(format, Name, DataUnpacker.GetWordLiteral(memory));
                case Propeller.Spin.ArgumentMode.NearLongLiteral:
                    return String.Format(format, Name, DataUnpacker.GetNearLongLiteral(memory));
                case Propeller.Spin.ArgumentMode.LongLiteral:
                    return String.Format(format, Name, DataUnpacker.GetLongLiteral(memory));
                case Propeller.Spin.ArgumentMode.ObjCallPair:
                    {
                        byte obj = memory.ReadByte();
                        byte funct = memory.ReadByte();
                        return String.Format("{0} {1}.{2}", Name, obj, funct);
                    }
                case Propeller.Spin.ArgumentMode.MemoryOpCode:
                    return String.Format("{0} {1}", Name, GetMemoryOp(memory, useShortOpcodes));
                default:
                    throw new Exception("Unknown Spin Argument Mode: " + Instr.ArgumentMode.ToString());
            }
        }
    }
}
