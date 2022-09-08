/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * InstructionDisassembler.cs
 * Provides methods to decode and create equivalent text of a PASM operation
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
using System;
using System.ComponentModel;

namespace Gear.EmulationCore
{
    /// @brief Provides methods to decode and create equivalent text of a
    /// PASM operation.
    public static class InstructionDisassembler
    {
        /// <summary>Translate PASM OpCodes to text.</summary>
        /// <param name="memoryValue">PASM OpCode value.</param>
        /// <param name="displayAsHexadecimal">Flag to show value for operation
        /// as hexadecimal or decimal base.</param>
        /// <returns>Translated text from decoded PASM OpCodes.</returns>
        /// @version v22.08.01 - Added parameter `displayAsHexadecimal` to
        /// show decimal or hexadecimal representation of values. Changed name
        /// of local variable to be more meaningfully.
        public static string AssemblyText(uint memoryValue, bool displayAsHexadecimal)
        {
            Assembly.DecodedPASMInstruction instruction =
                new Assembly.DecodedPASMInstruction(memoryValue);
            string text;
            if (instruction.CON > 0x00)
            {
                Propeller.Assembly.InstructionVariant actualInstructionVariant =
                    instruction.GetInstructionVariant();
                string srcString = string.Empty;
                string destString = string.Empty;
                string formatForValue = displayAsHexadecimal ?
                    "${0:X3}" :
                    "{0}";
                if (actualInstructionVariant.UseSource)
                    srcString = instruction.SRC >= Propeller.Assembly.RegisterBaseAddress ?
                        $"{(instruction.ImmediateValue() ? "#" : string.Empty)}{Propeller.Assembly.Registers[instruction.SRC - Propeller.Assembly.RegisterBaseAddress].Name}" :
                        $"{(instruction.ImmediateValue() ? "#" : string.Empty)}" + string.Format(formatForValue, instruction.SRC);
                if (actualInstructionVariant.UseDestination)
                    destString = instruction.DEST >= Propeller.Assembly.RegisterBaseAddress ?
                        $"{Propeller.Assembly.Registers[instruction.DEST - Propeller.Assembly.RegisterBaseAddress].Name}" :
                        string.Format(formatForValue, instruction.DEST);
                text =
                    $"{Propeller.Assembly.Conditions[instruction.CON].Inst1} {actualInstructionVariant.Name} {destString}{(actualInstructionVariant.UseSource && actualInstructionVariant.UseDestination ? ", " : string.Empty)}{srcString}";
                text += $"{(instruction.WriteResult() ? " WR" : string.Empty)}{(instruction.NoResult() ? " NR" : string.Empty)}{(instruction.WriteZero() ? " WZ" : string.Empty)}{(instruction.WriteCarry() ? " WC" : string.Empty)}";
            }
            else
                text = $"{Propeller.Assembly.Conditions[0].Inst2} {Propeller.Assembly.Conditions[0].Inst3}";
            return text;
        }

        /// <summary>Get effects of a SPIN instruction, reading from main
        /// memory, decoding it to a text.</summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory range.</param>
        /// <param name="useShortOpCodes">Flag to use short or long Code texts.</param>
        /// <returns>Decoded text of effects of the instruction.</returns>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        /// @warning Changes on flow or reading sequence on this method should
        /// affect accordingly the method AdvanceReadingEffect().
        /// @version v22.09.01 - Changed method name to be more meaningfully.
        private static string GetEffectsOp(Propeller.MemorySegment memorySegment,
            bool useShortOpCodes)
        {
            Spin.DecodedAssignment decodedAssignment =
                new Spin.DecodedAssignment(memorySegment.ReadByte());
            string effect = decodedAssignment.Push ?
                string.Empty :
                "POP ";
            if (useShortOpCodes)
                effect += $"({decodedAssignment.GetBasicInstruction().NameBrief}{(decodedAssignment.Swap ? ",reverse" : string.Empty)})";
            else
                effect += $"{decodedAssignment.GetBasicInstruction().Name}{(decodedAssignment.Swap ? " REVERSE" : string.Empty)}";
            if (decodedAssignment.Math)
                return effect;
            Propeller.Spin.AssignmentVariant assignmentVariant = decodedAssignment.GetAssignmentVariant();
            switch (assignmentVariant.ArgumentMode)
            {
                case Propeller.Spin.ArgumentModeEnum.None:
                    break;
                case Propeller.Spin.ArgumentModeEnum.SignedPackedOffset:
                    effect += $" {DataDecoder.GetSignedPackedOffset(memorySegment)}";
                    break;
                default:
                    throw new InvalidEnumArgumentException(
                        $@"Unexpected Spin Argument Mode: {assignmentVariant.ArgumentMode}");
            }
            return effect;
        }

        /// <summary>Get stack and memory options of a SPIN instruction,
        /// reading from main memory, decoding it to a text.</summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory segment.</param>
        /// <param name="useShortOpCodes">Flag to use short or long code texts.</param>
        /// <returns></returns>
        /// @warning Changes on flow or reading sequence on this method should
        /// affect accordingly the method AdvanceMemoryOp().
        /// @version v22.08.01 - Changed name of parameter `memorySegment` to be
        /// more meaningfully. Visibility changed to private.
        private static string GetMemoryOp(Propeller.MemorySegment memorySegment,
            bool useShortOpCodes)
        {
            Spin.DecodedMemoryOperation operation =
                new Spin.DecodedMemoryOperation(memorySegment.ReadByte());
            string name = operation.RegisterName;
            switch (operation.Action)
            {
                case Propeller.Spin.MemoryActionEnum.PUSH:
                    return $"PUSH {name}";
                case Propeller.Spin.MemoryActionEnum.POP:
                    return $"POP {name}";
                case Propeller.Spin.MemoryActionEnum.EFFECT:
                    return $"USING {name} {GetEffectsOp(memorySegment, useShortOpCodes)}";
                default:
                    return $"UNKNOWN_{operation.Action} {name}";
            }
        }

        /// <summary>Translate SPIN bytecode to text.</summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory range.</param>
        /// <param name="displayAsHexadecimal">Flag to show value for operation
        /// as hexadecimal or decimal base.</param>
        /// <param name="useShortOpCodes">Flag to use short or long Code texts.</param>
        /// <returns>Translated text from SPIN bytecode.</returns>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        /// @warning Changes on flow or reading sequence on this method should
        /// affect accordingly the method InterpretedInstructionLength().
        /// @version v22.08.01 - Changed name of parameter `memorySegment` to be
        /// more meaningfully.
        public static string InterpreterText(Propeller.MemorySegment memorySegment,
            bool displayAsHexadecimal, bool useShortOpCodes)
        {
            string format = displayAsHexadecimal ?
                "{0} ${1:X}" :
                "{0} {1}";
            Propeller.Spin.Instruction instruction =
                Propeller.Spin.Instructions[memorySegment.ReadByte()];
            string name = useShortOpCodes ?
                instruction.NameBrief :
                instruction.Name;
            switch (instruction.ArgumentMode)
            {
                case Propeller.Spin.ArgumentModeEnum.None:
                    return name;
                case Propeller.Spin.ArgumentModeEnum.UnsignedOffset:
                    return string.Format(format, name, DataDecoder.GetPackedOffset(memorySegment));
                case Propeller.Spin.ArgumentModeEnum.UnsignedEffectedOffset:
                    return $"{name} {DataDecoder.GetPackedOffset(memorySegment)} {GetEffectsOp(memorySegment, useShortOpCodes)}";
                case Propeller.Spin.ArgumentModeEnum.Effect:
                    return string.Format(format, name, GetEffectsOp(memorySegment, useShortOpCodes));
                case Propeller.Spin.ArgumentModeEnum.SignedOffset:
                    return string.Format(format, name, DataDecoder.GetSignedOffset(memorySegment));
                case Propeller.Spin.ArgumentModeEnum.PackedLiteral:
                    return string.Format(format, name, DataDecoder.GetPackedLiteral(memorySegment));
                case Propeller.Spin.ArgumentModeEnum.ByteLiteral:
                    return string.Format(format, name, memorySegment.ReadByte());
                case Propeller.Spin.ArgumentModeEnum.WordLiteral:
                    return string.Format(format, name, DataDecoder.GetWordLiteral(memorySegment));
                case Propeller.Spin.ArgumentModeEnum.NearLongLiteral:
                    return string.Format(format, name, DataDecoder.GetNearLongLiteral(memorySegment));
                case Propeller.Spin.ArgumentModeEnum.LongLiteral:
                    return string.Format(format, name, DataDecoder.GetLongLiteral(memorySegment));
                case Propeller.Spin.ArgumentModeEnum.ObjCallPair:
                    return $"{name} {memorySegment.ReadByte()}.{memorySegment.ReadByte()}";
                case Propeller.Spin.ArgumentModeEnum.MemoryOpCode:
                    return $"{name} {GetMemoryOp(memorySegment, useShortOpCodes)}";
                default:
                    throw new InvalidEnumArgumentException(
                        $@"Unknown Spin Argument Mode: {instruction.ArgumentMode}");
            }
        }
    }
}
