/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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

using Gear.Disassembler;
using System.ComponentModel;

namespace Gear.EmulationCore
{
    /// @brief Provides a method for creating the string equivalent of a propeller operation.
    public static class InstructionDisassembler
    {
        /// @brief Translate to text the bytecode OpCodes.
        /// @param operation Bytecode OpCodes.
        /// @return Explicative text from decoded bytecode.
        /// @version v22.05.03 - Strings transformed to interpolation and
        /// Parameter name changed to follow naming conventions.
        public static string AssemblyText(uint operation)
        {
            Assembly.ParsedInstruction instr = new Assembly.ParsedInstruction(operation);
            string text;
            if (instr.CON > 0x00)
            {
                Propeller.Assembly.SubInstruction actualInstruction = instr.GetSubInstruction();
                string srcString = string.Empty;
                string destString = string.Empty;
                if (actualInstruction.UseSource)
                    srcString = instr.SRC >= Propeller.Assembly.RegisterBaseAddress ?
                        $"{(instr.ImmediateValue() ? "#" : string.Empty)}{Propeller.Assembly.Registers[instr.SRC - Propeller.Assembly.RegisterBaseAddress].Name}" :
                        $"{(instr.ImmediateValue() ? "#" : string.Empty)}${instr.SRC:X3}";
                if (actualInstruction.UseDestination)
                    destString = instr.DEST >= Propeller.Assembly.RegisterBaseAddress ?
                        $"{Propeller.Assembly.Registers[instr.DEST - Propeller.Assembly.RegisterBaseAddress].Name}" :
                        $"${instr.DEST:X3}";
                text =
                    $"{Propeller.Assembly.Conditions[instr.CON].Inst1} {actualInstruction.Name} {destString}{(actualInstruction.UseSource && actualInstruction.UseDestination ? ", " : string.Empty)}{srcString}";
                text += $"{(instr.WriteResult() ? " WR" : string.Empty)}{(instr.NoResult() ? " NR" : string.Empty)}{(instr.WriteZero() ? " WZ" : string.Empty)}{(instr.WriteCarry() ? " WC" : string.Empty)}";
            }
            else
                text = $"{Propeller.Assembly.Conditions[0].Inst2} {Propeller.Assembly.Conditions[0].Inst3}";
            return text;
        }

        /// <summary></summary>
        /// <param name="memory"></param>
        /// <param name="useShortOpCodes"></param>
        /// <returns></returns>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        /// @version v22.04.02 - Strings transformed to interpolation.
        private static string GetUsingCode(Propeller.MemoryManager memory,
            bool useShortOpCodes)
        {
            Spin.ParsedAssignment parsedAssignment = new Spin.ParsedAssignment(memory.ReadByte());
            string effect = parsedAssignment.Push ?
                string.Empty :
                "POP ";
            if (useShortOpCodes)
                effect += $"({parsedAssignment.GetBasicInstruction().NameBrief}{(parsedAssignment.Swap ? ",reverse" : string.Empty)})";
            else
                effect += $"{parsedAssignment.GetBasicInstruction().Name}{(parsedAssignment.Swap ? " REVERSE" : string.Empty)}";
            if (parsedAssignment.Math)
                return effect;
            Propeller.Spin.SubAssignment subAssignment = parsedAssignment.GetSubAssignment();
            switch (subAssignment.ArgumentMode)
            {
                case Propeller.Spin.ArgumentMode.None:
                    break;
                case Propeller.Spin.ArgumentMode.SignedPackedOffset:
                    effect += $" {DataUnpacker.GetSignedPackedOffset(memory)}";
                    break;
                default:
                    throw new InvalidEnumArgumentException(
                        $"Unexpected Spin Argument Mode: {subAssignment.ArgumentMode}");
            }
            return effect;
        }

        /// <summary></summary>
        /// <param name="memory"></param>
        /// <param name="useShortOpCodes"></param>
        /// <returns></returns>
        /// @version v22.05.03 - Modify to use Refactored Property
        /// Spin.ParsedMemoryOperation.RegisterName, changed local variable
        /// name to clarify meaning of it and Strings transformed
        /// to interpolation.
        public static string GetMemoryOp(Propeller.MemoryManager memory,
            bool useShortOpCodes)
        {
            Spin.ParsedMemoryOperation operation =
                new Spin.ParsedMemoryOperation(memory.ReadByte());
            string name = operation.RegisterName;
            switch (operation.Action)
            {
                case Propeller.Spin.MemoryAction.PUSH:
                    return $"PUSH {name}";
                case Propeller.Spin.MemoryAction.POP:
                    return $"POP {name}";
                case Propeller.Spin.MemoryAction.EFFECT:
                    return $"USING {name} {GetUsingCode(memory, useShortOpCodes)}";
                default:
                    return $"UNKNOWN_{operation.Action} {name}";
            }
        }

        /// <summary></summary>
        /// <param name="memory"></param>
        /// <param name="displayAsHexadecimal"></param>
        /// <param name="useShortOpCodes"></param>
        /// <returns></returns>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        /// @version v22.04.02 - Strings transformed to interpolation.
        public static string InterpreterText(Propeller.MemoryManager memory,
            bool displayAsHexadecimal, bool useShortOpCodes)
        {
            string format = displayAsHexadecimal ?
                "{0} ${1:X}" :
                "{0} {1}";
            Propeller.Spin.Instruction instruction =
                Propeller.Spin.Instructions[memory.ReadByte()];
            string name = useShortOpCodes ?
                instruction.NameBrief :
                instruction.Name;
            switch (instruction.ArgumentMode)
            {
                case Propeller.Spin.ArgumentMode.None:
                    return name;
                case Propeller.Spin.ArgumentMode.UnsignedOffset:
                    return string.Format(format, name, DataUnpacker.GetPackedOffset(memory));
                case Propeller.Spin.ArgumentMode.UnsignedEffectedOffset:
                    return $"{name} {DataUnpacker.GetPackedOffset(memory)} {GetUsingCode(memory, useShortOpCodes)}";
                case Propeller.Spin.ArgumentMode.Effect:
                    return string.Format(format, name, GetUsingCode(memory, useShortOpCodes));
                case Propeller.Spin.ArgumentMode.SignedOffset:
                    return string.Format(format, name, DataUnpacker.GetSignedOffset(memory));
                case Propeller.Spin.ArgumentMode.PackedLiteral:
                    return string.Format(format, name, DataUnpacker.GetPackedLiteral(memory));
                case Propeller.Spin.ArgumentMode.ByteLiteral:
                    return string.Format(format, name, memory.ReadByte());
                case Propeller.Spin.ArgumentMode.WordLiteral:
                    return string.Format(format, name, DataUnpacker.GetWordLiteral(memory));
                case Propeller.Spin.ArgumentMode.NearLongLiteral:
                    return string.Format(format, name, DataUnpacker.GetNearLongLiteral(memory));
                case Propeller.Spin.ArgumentMode.LongLiteral:
                    return string.Format(format, name, DataUnpacker.GetLongLiteral(memory));
                case Propeller.Spin.ArgumentMode.ObjCallPair:
                    return $"{name} {memory.ReadByte()}.{memory.ReadByte()}";
                case Propeller.Spin.ArgumentMode.MemoryOpCode:
                    return $"{name} {GetMemoryOp(memory, useShortOpCodes)}";
                default:
                    throw new InvalidEnumArgumentException(
                        $"Unknown Spin Argument Mode: {instruction.ArgumentMode}");
            }
        }
    }
}
