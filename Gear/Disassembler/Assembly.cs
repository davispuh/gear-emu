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
/// <summary></summary>
namespace Gear.Disassembler
{
    /// <summary></summary>
    public static class Assembly
    {
        /// <summary></summary>
        /// <param name="sourceInstruction"></param>
        /// <param name="parsedInstruction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        /// @version v22.05.03 - Parameter names changed to follow naming
        /// conventions and throws more specifics exception.
        public static Propeller.Assembly.SubInstruction GetSubInstruction(
            Propeller.Assembly.Instruction sourceInstruction,
            ParsedInstruction parsedInstruction)
        {
            if (sourceInstruction == null)
                throw new ArgumentNullException(nameof(sourceInstruction));
            if (parsedInstruction == null)
                throw new ArgumentNullException(nameof(parsedInstruction));
            switch (sourceInstruction.InstructionType)
            {
                case Propeller.Assembly.InstructionTypeEnum.Normal:
                    return sourceInstruction.SubInstructions[0];
                case Propeller.Assembly.InstructionTypeEnum.ReadWrite:
                    return sourceInstruction.SubInstructions[
                        (parsedInstruction.ZCRI & ParsedInstruction.WriteResultFlag) ==
                        ParsedInstruction.WriteResultFlag ? 0 : 1];
                case Propeller.Assembly.InstructionTypeEnum.Hub:
                    return sourceInstruction.SubInstructions[parsedInstruction.SRC & 0x7];
                case Propeller.Assembly.InstructionTypeEnum.Jump:
                    int num = parsedInstruction.ZCRI & 0x3;
                    if (num <= 1)
                    {
                        num = 0;
                        if (parsedInstruction.SRC == 0)
                            num = 1;
                    }
                    return sourceInstruction.SubInstructions[num];
                default:
                    throw new InvalidEnumArgumentException(
                        sourceInstruction.InstructionType.ToString(),
                        (int)sourceInstruction.InstructionType,
                        typeof(Propeller.Assembly.InstructionTypeEnum));
            }
        }

        /// <summary></summary>
        public class ParsedInstruction
        {
            /// <summary></summary>
            public const byte WriteZeroFlag      = 0x8;
            /// <summary></summary>
            public const byte WriteCarryFlag     = 0x4;
            /// <summary></summary>
            public const byte WriteResultFlag    = 0x2;
            /// <summary></summary>
            public const byte ImmediateValueFlag = 0x1;

            /// Instruction's 32-bit OpCode.
            /// @version v22.05.03 - Changed name to follow naming conventions
            /// and Removed setter not used.
            public uint OpCode { get; }
            /// Indicates the instruction being executed.
            /// @version v22.05.03 - Removed setter not used.
            public byte INSTR { get; }
            /// Indicates instruction’s effect status and SRC field meaning.
            /// @version v22.05.03 - Removed setter not used.
            public byte ZCRI { get; }
            /// Indicates the condition in which to execute the instruction.
            /// @version v22.05.03 - Removed setter not used.
            public byte CON { get; }
            /// Contains the destination register address.
            /// @version v22.05.03 - Removed setter not used.
            public ushort DEST { get; }
            /// Contains the source register address or 9-bit literal value.
            /// @version v22.05.03 - Removed setter not used.
            public ushort SRC { get; }

            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public Propeller.Assembly.Instruction SourceInstruction { get; }

            /// <summary></summary>
            /// @version v22.05.03 - Changed name to follow naming conventions.
            private Propeller.Assembly.SubInstruction _sourceSubInstruction;

            /// <summary>Default constructor.</summary>
            /// <param name="unEncodedOp"></param>
            /// @version v22.05.03 - Parameter name changed to follow naming
            /// conventions.
            public ParsedInstruction(uint unEncodedOp)
            {
                OpCode = unEncodedOp;
                INSTR  = (byte  )((unEncodedOp >> 26) & 0x03F);  // (bits 31:26)
                ZCRI   = (byte  )((unEncodedOp >> 22) & 0x00F);  // (bits 25:22)
                CON    = (byte  )((unEncodedOp >> 18) & 0x00F);  // (bits 21:18)
                DEST   = (ushort)((unEncodedOp >>  9) & 0x1FF);  // (bits 17:09)
                SRC    = (ushort)( unEncodedOp        & 0x1FF);  // (bits 08:00)
                SourceInstruction = Propeller.Assembly.Instructions[INSTR];
                _sourceSubInstruction = null;
            }

            /// <summary></summary>
            /// <returns></returns>
            public Propeller.Assembly.SubInstruction GetSubInstruction() =>
                _sourceSubInstruction ??
                (_sourceSubInstruction = Assembly.GetSubInstruction(SourceInstruction, this));

            /// <summary></summary>
            /// <returns></returns>
            public bool WriteZero() =>
                (ZCRI & WriteZeroFlag) == WriteZeroFlag && GetSubInstruction().UseWZ_Effect;

            /// <summary></summary>
            /// <returns></returns>
            public bool WriteCarry() =>
                (ZCRI & WriteCarryFlag) == WriteCarryFlag && GetSubInstruction().UseWC_Effect;

            /// <summary></summary>
            /// <returns></returns>
            public bool WriteResult() =>
                (ZCRI & WriteResultFlag) == WriteResultFlag && GetSubInstruction().UseWR_Effect;

            /// <summary></summary>
            /// <returns></returns>
            public bool NoResult() =>
                (ZCRI & WriteResultFlag) == 0 && GetSubInstruction().UseWR_Effect;

            /// <summary></summary>
            /// <returns></returns>
            public bool ImmediateValue() =>
                (ZCRI & ImmediateValueFlag) == ImmediateValueFlag;
        }
    }
}
