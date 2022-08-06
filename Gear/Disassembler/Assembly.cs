/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * Assembly.cs
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
using System.ComponentModel;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
/// <summary>%Disassembler Utilities.</summary>
namespace Gear.Disassembler
{
    /// <summary>PASM Decoder Utilities.</summary>
    public static class Assembly
    {
        /// <summary>Get the instruction variant for each case.</summary>
        /// <param name="sourceInstruction">Source Instruction.</param>
        /// <param name="decodedPASMInstruction">Decoded Instruction.</param>
        /// <returns>Instruction variant.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        /// @version v22.07.xx - Method renamed to be more meaningful and
        /// Parameter name changed to follow its class renaming. Changed
        /// visibility from `public`.
        private static Propeller.Assembly.InstructionVariant GetInstructionVariant(
            Propeller.Assembly.Instruction sourceInstruction,
            DecodedPASMInstruction decodedPASMInstruction)
        {
            if (sourceInstruction == null)
                throw new ArgumentNullException(nameof(sourceInstruction));
            if (decodedPASMInstruction == null)
                throw new ArgumentNullException(nameof(decodedPASMInstruction));
            switch (sourceInstruction.InstructionType)
            {
                case Propeller.Assembly.InstructionTypeEnum.Normal:
                    return sourceInstruction.InstructionVariants[0];
                case Propeller.Assembly.InstructionTypeEnum.ReadWrite:
                    return sourceInstruction.InstructionVariants[
                        (decodedPASMInstruction.ZCRI & DecodedPASMInstruction.WriteResultFlag) ==
                        DecodedPASMInstruction.WriteResultFlag ? 0 : 1];
                case Propeller.Assembly.InstructionTypeEnum.Hub:
                    return sourceInstruction.InstructionVariants[decodedPASMInstruction.SRC & 0x7];
                case Propeller.Assembly.InstructionTypeEnum.Jump:
                    int num = decodedPASMInstruction.ZCRI & 0x3;
                    if (num > 1)
                        return sourceInstruction.InstructionVariants[num];
                    num = 0;
                    if (decodedPASMInstruction.SRC == 0)
                        num = 1;
                    return sourceInstruction.InstructionVariants[num];
                default:
                    throw new InvalidEnumArgumentException(
                        sourceInstruction.InstructionType.ToString(),
                        (int)sourceInstruction.InstructionType,
                        typeof(Propeller.Assembly.InstructionTypeEnum));
            }
        }

        /// <summary>Decode an PASM instruction.</summary>
        /// @version v22.07.xx - Renamed class to be more meaningfully.
        public class DecodedPASMInstruction
        {
            /// <summary></summary>
            public const byte WriteZeroFlag      = 0x8;
            /// <summary></summary>
            public const byte WriteCarryFlag     = 0x4;
            /// <summary></summary>
            public const byte WriteResultFlag    = 0x2;
            /// <summary></summary>
            public const byte ImmediateValueFlag = 0x1;

            /// <summary>Source instruction variant associated to this
            /// Instruction.</summary>
            /// @version v22.07.xx - Renamed member according to returned class
            /// name was changed.
            private Propeller.Assembly.InstructionVariant _sourceInstructionVariant;

            /// <summary>Instruction's 32-bit OpCode.</summary>
            /// @version v22.05.03 - Changed name to follow naming conventions
            /// and Removed setter not used.
            public uint OpCode { get; }

            /// <summary>Indicates the instruction being executed.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public byte INSTR { get; }

            /// <summary>Indicates instruction’s effect status and SRC field
            /// meaning.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public byte ZCRI { get; }

            /// <summary>Indicates the condition to be satisfied to execute
            /// the instruction.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public byte CON { get; }

            /// <summary>Contains the destination register address.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public ushort DEST { get; }

            /// <summary>Contains the source register address or 9-bit literal
            /// value.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public ushort SRC { get; }

            /// <summary>Source Instruction.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public Propeller.Assembly.Instruction SourceInstruction { get; }

            /// <summary>Default constructor.</summary>
            /// <param name="operationToDecode">Memory value to decode.</param>
            /// @version v22.07.xx - Renamed class constructor to follow class
            /// name was changed. Parameter name changed to be more meaningful.
            public DecodedPASMInstruction(uint operationToDecode)
            {
                OpCode = operationToDecode;
                INSTR  = (byte  )((operationToDecode >> 26) & 0x03F);  // (bits 31:26)
                ZCRI   = (byte  )((operationToDecode >> 22) & 0x00F);  // (bits 25:22)
                CON    = (byte  )((operationToDecode >> 18) & 0x00F);  // (bits 21:18)
                DEST   = (ushort)((operationToDecode >>  9) & 0x1FF);  // (bits 17:09)
                SRC    = (ushort)( operationToDecode        & 0x1FF);  // (bits 08:00)
                SourceInstruction = Propeller.Assembly.Instructions[INSTR];
                _sourceInstructionVariant = null;
            }

            /// <summary>Get the instruction variant of this instruction.</summary>
            /// <returns>Instruction variant.</returns>
            /// @version v22.07.xx - Renamed method according to returned
            /// class name was changed.
            public Propeller.Assembly.InstructionVariant GetInstructionVariant() =>
                _sourceInstructionVariant ??
                (_sourceInstructionVariant = Assembly.GetInstructionVariant(SourceInstruction, this));

            /// <summary>Evaluate the WriteZero condition of this instruction.</summary>
            /// <returns>True if value is 0, else false.</returns>
            /// @version v22.07.xx - Changed method invocation of former
            /// `GetSubInstruction()` according to new name of it.
            public bool WriteZero() =>
                (ZCRI & WriteZeroFlag) == WriteZeroFlag &&
                GetInstructionVariant().UseWZ_Effect;

            /// <summary>Evaluate the WriteCarry condition of this instruction.</summary>
            /// <returns>True if Carry bit is set, else false.</returns>
            /// @version v22.07.xx - Changed method invocation of former
            /// `GetSubInstruction()` according to new name of it.
            public bool WriteCarry() =>
                (ZCRI & WriteCarryFlag) == WriteCarryFlag &&
                GetInstructionVariant().UseWC_Effect;

            /// <summary>Evaluate the WriteResult condition of this
            /// instruction.</summary>
            /// <returns>True if Destination Register was modified, else false.</returns>
            /// @version v22.07.xx - Changed method invocation of former
            /// `GetSubInstruction()` according to new name of it.
            public bool WriteResult() =>
                (ZCRI & WriteResultFlag) == WriteResultFlag &&
                GetInstructionVariant().UseWR_Effect;

            /// <summary>Evaluate the NoResult condition of this instruction.</summary>
            /// <returns>True if Destination Register not modified, else false.</returns>
            /// @version v22.07.xx - Changed method invocation of former
            /// `GetSubInstruction()` according to new name of it.
            public bool NoResult() =>
                (ZCRI & WriteResultFlag) == 0 &&
                GetInstructionVariant().UseWR_Effect;

            /// <summary>Evaluate the ImmediateValue condition of this
            /// instruction.</summary>
            /// <returns>True if immediate flag is set, else false.</returns>
            public bool ImmediateValue() =>
                (ZCRI & ImmediateValueFlag) == ImmediateValueFlag;
        }
    }
}
