/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * Spin.cs
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

using Gear.Propeller;
using System;
using System.ComponentModel;

// ReSharper disable InconsistentNaming
namespace Gear.Disassembler
{
    /// <summary>SPIN Interpreter Decoder Utilities.</summary>
    public static class Spin
    {
        /// <summary>Decode SPIN Memory operation.</summary>
        /// @version v22.08.01 - Renamed class to be more meaningfully.
        public class DecodedMemoryOperation
        {
            /// <summary>Return the code of memory operation.</summary>
            /// @version v22.05.03 - Changed name to follow naming conventions
            /// and Removed setter not used.
            public byte OpCode { get; }

            /// <summary>Return the associated memory address.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public uint Address { get; }

            /// <summary>Return the associated register value, or 0x0 if there
            /// isn't any.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public byte Register { get; }

            /// <summary>Return the associated assembly register value, or
            /// 0x0 if there isn't any.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public bool AssemblyRegister { get; }

            /// <summary>Return the memory action type of this operation.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public Propeller.Spin.MemoryActionEnum Action { get; }

            /// <summary>Return a register name, if any.</summary>
            /// @version v22.05.03 - Refactored to a Property from old Method
            /// `%GetRegister()`.
            public string RegisterName =>
                AssemblyRegister ?
                    Propeller.Assembly.Registers[Register].Name :
                    Propeller.Spin.Registers[Register].Name;

            /// <summary>Default Constructor.</summary>
            /// <param name="opCode">Coded value of memory operation.</param>
            /// @version v22.08.01 - Renamed class constructor to follow class
            /// name change.
            public DecodedMemoryOperation(byte opCode)
            {
                OpCode = opCode;
                Address = (uint)(opCode | 0x1E0);
                Register = (byte)(opCode & 0x00F);
                AssemblyRegister = ((opCode >> 4) & 0x001) == 0x01;
                Action = (Propeller.Spin.MemoryActionEnum)((opCode >> 5) & 0x00F);
            }
        }

        /// <summary>Get the assignment variant of this instruction.</summary>
        /// <param name="sourceAssignment">Source assignment.</param>
        /// <param name="decodedAssignment">Decoded assignment.</param>
        /// <returns>Assignment variant.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        /// @version v22.08.01 - Renamed method according to returned
        /// class name that changes.
        public static Propeller.Spin.AssignmentVariant GetAssignmentVariant(
            Propeller.Spin.Assignment sourceAssignment, DecodedAssignment decodedAssignment)
        {
            if (sourceAssignment is null)
                throw new ArgumentNullException(nameof(sourceAssignment));
            if (decodedAssignment is null)
                throw new ArgumentNullException(nameof(decodedAssignment));
            switch (sourceAssignment.AssignmentType)
            {
                case Propeller.Spin.AssignmentTypeEnum.WriteRepeat:
                    return sourceAssignment.AssignmentVariantsArray[decodedAssignment.Bit1 ? 1 : 0];
                case Propeller.Spin.AssignmentTypeEnum.Normal:
                    return sourceAssignment.AssignmentVariantsArray[decodedAssignment.Bit2 ? 1 : 0];
                case Propeller.Spin.AssignmentTypeEnum.Size:
                    return sourceAssignment.AssignmentVariantsArray[(int)decodedAssignment.Size];
                default:
                    throw new InvalidEnumArgumentException(
                        sourceAssignment.AssignmentType.ToString(),
                        (int)sourceAssignment.AssignmentType,
                        typeof(Propeller.Spin.AssignmentTypeEnum));
            }
        }

        /// <summary>Decoded SPIN assignment.</summary>
        public class DecodedAssignment
        {
            /// <summary>Return the code of assignment operation.</summary>
            /// @version v22.05.03 - Changed name to follow naming conventions
            /// and Removed setter not used.
            public byte OpCode { get; }

            /// <summary>Flag if it imply a Push operation.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public bool Push { get; }

            /// <summary>Flag if it imply a Math operation.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public bool Math { get; }

            /// <summary>Flag if it imply a assignment operation.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public byte ASG { get; }

            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public byte MTH { get; }

            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public bool Bit1 { get; }

            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public bool Bit2 { get; }

            /// <summary>Return the size type of assignment.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public Propeller.Spin.AssignmentSize Size { get; }

            /// <summary>Flag if parameters are in reversed or sequential
            /// order.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public bool Swap { get; }

            /// <summary>Source assignment related to this decoded assignment.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public Propeller.Spin.Assignment SourceAssignment { get; }

            /// <summary>Math assignment related to this decoded assignment,
            /// if any.</summary>
            /// @version v22.05.03 - Removed setter not used.
            public Propeller.Spin.MathInstruction MathAssignment { get; }

            /// <summary>Source assignment variant related to this Assignment.</summary>
            /// @version v22.08.01 - Renamed property according to returned
            /// class name was changed.
            private Propeller.Spin.AssignmentVariant _sourceAssignmentVariant;

            /// <summary>Default Constructor.</summary>
            /// <param name="operationToDecode">Coded value of this Assignment.</param>
            /// @version v22.08.01 - Renamed class constructor to follow class
            /// name change. Parameter name changed to be more meaningful.
            public DecodedAssignment(byte operationToDecode)
            {
                OpCode = operationToDecode;
                Push = ((operationToDecode >> 7) & 0x01) == 0x01;
                Math = ((operationToDecode >> 6) & 0x01) == 0x01;
                ASG = (byte)((operationToDecode >> 3) & 0x07);
                MTH = (byte)(operationToDecode & 0x1F);
                Bit1 = ((operationToDecode >> 1) & 0x01) == 0x01;
                Bit2 = ((operationToDecode >> 2) & 0x01) == 0x01;
                Size = (Propeller.Spin.AssignmentSize)((operationToDecode >> 1) & 0x03);
                Swap = ((operationToDecode >> 5) & 0x01) == 0x01;

                if (Math)
                    MathAssignment = Propeller.Spin.MathInstructions[MTH];
                else
                    SourceAssignment = Propeller.Spin.Assignments[ASG];
                _sourceAssignmentVariant = null;
            }

            /// <summary>Return Assignment Variant associated to this
            /// Assignment, if any.</summary>
            /// <returns>Appropriate Assignment Variant or null if it does not
            /// exist.</returns>
            /// @version v22.08.01 - Renamed method according to returned
            /// class name was changed.
            public Propeller.Spin.AssignmentVariant GetAssignmentVariant() =>
                Math ?
                    null :
                    _sourceAssignmentVariant ??
                    (_sourceAssignmentVariant = Spin.GetAssignmentVariant(SourceAssignment, this));

            /// <summary>Return the basic instruction instance associated to
            /// this Assignment</summary>
            /// <returns>Appropriate basic instruction.</returns>
            public BasicInstruction GetBasicInstruction() =>
                Math ?
                    (BasicInstruction)MathAssignment :
                    GetAssignmentVariant();
        }
    }
}
