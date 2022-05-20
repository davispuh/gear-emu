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
    /// <summary></summary>
    public static class Spin
    {
        /// <summary></summary>
        public class ParsedMemoryOperation
        {
            /// <summary></summary>
            /// @version v22.05.03 - Changed name to follow naming conventions and Removed setter not used.
            public byte OpCode { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public uint Address { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public byte Register { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public bool AssemblyRegister { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public Propeller.Spin.MemoryAction Action { get; }

            /// <summary></summary>
            /// @version v22.05.03 - Refactored to a Property from old Method %GetRegister().
            public string RegisterName =>
                AssemblyRegister ?
                    Propeller.Assembly.Registers[Register].Name :
                    Propeller.Spin.Registers[Register].Name;

            /// <summary>Default Constructor.</summary>
            /// <param name="opCode"></param>
            /// @version v22.05.03 - Parameter name changed to follow naming conventions.
            public ParsedMemoryOperation(byte opCode)
            {
                OpCode = opCode;
                Address = (uint)(opCode | 0x1E0);
                Register = (byte)(opCode & 0x00F);
                AssemblyRegister = ((opCode >> 4) & 0x001) == 0x01;
                Action = (Propeller.Spin.MemoryAction)((opCode >> 5) & 0x00F);
            }
        }

        /// <summary></summary>
        /// <param name="sourceAssignment"></param>
        /// <param name="parsedAssignment"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        /// @version v22.05.03 - Parameter names changed to follow naming conventions and throws more specifics exception.
        public static Propeller.Spin.SubAssignment GetSubAssignment(
            Propeller.Spin.Assignment sourceAssignment, ParsedAssignment parsedAssignment)
        {
            if (sourceAssignment is null)
                throw new ArgumentNullException(nameof(sourceAssignment));
            if (parsedAssignment is null)
                throw new ArgumentNullException(nameof(parsedAssignment));
            switch (sourceAssignment.AssignmentType)
            {
                case Propeller.Spin.AssignmentTypeEnum.WriteRepeat:
                    return sourceAssignment.SubAssignmentsArray[parsedAssignment.Bit1 ? 1 : 0];
                case Propeller.Spin.AssignmentTypeEnum.Normal:
                    return sourceAssignment.SubAssignmentsArray[parsedAssignment.Bit2 ? 1 : 0];
                case Propeller.Spin.AssignmentTypeEnum.Size:
                    return sourceAssignment.SubAssignmentsArray[(int)parsedAssignment.Size];
                default:
                    throw new InvalidEnumArgumentException(sourceAssignment.AssignmentType.ToString(),
                        (int)sourceAssignment.AssignmentType, typeof(Propeller.Spin.AssignmentTypeEnum));
            }
        }

        /// <summary></summary>
        public class ParsedAssignment
        {
            /// <summary></summary>
            /// @version v22.05.03 - Changed name to follow naming conventions and Removed setter not used.
            public byte OpCode { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public bool Push { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public bool Math { get; }
            /// <summary></summary>
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
            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public Propeller.Spin.AssignmentSize Size { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public bool Swap { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public Propeller.Spin.Assignment SourceAssignment { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Removed setter not used.
            public Propeller.Spin.MathInstruction MathAssignment { get; }

            /// <summary></summary>
            /// @version v22.05.03 - Changed name to follow naming conventions.
            private Propeller.Spin.SubAssignment _sourceSubAssignment;

            /// <summary>Default Constructor.</summary>
            /// <param name="opCode"></param>
            /// @version v22.05.03 - Parameter name changed to follow naming conventions.
            public ParsedAssignment(byte opCode)
            {
                OpCode = opCode;
                Push = ((opCode >> 7) & 0x01) == 0x01;
                Math = ((opCode >> 6) & 0x01) == 0x01;
                ASG = (byte)((opCode >> 3) & 0x07);
                MTH = (byte)(opCode & 0x1F);
                Bit1 = ((opCode >> 1) & 0x01) == 0x01;
                Bit2 = ((opCode >> 2) & 0x01) == 0x01;
                Size = (Propeller.Spin.AssignmentSize)((opCode >> 1) & 0x03);
                Swap = ((opCode >> 5) & 0x01) == 0x01;

                if (Math)
                    MathAssignment = Propeller.Spin.MathInstructions[MTH];
                else
                    SourceAssignment = Propeller.Spin.Assignments[ASG];
                _sourceSubAssignment = null;
            }

            /// <summary></summary>
            /// <returns></returns>
            public Propeller.Spin.SubAssignment GetSubAssignment() =>
                Math ?
                    null :
                    _sourceSubAssignment ??
                    (_sourceSubAssignment = Spin.GetSubAssignment(SourceAssignment, this));

            /// <summary></summary>
            /// <returns></returns>
            public BasicInstruction GetBasicInstruction() =>
                Math ?
                    (BasicInstruction)MathAssignment :
                    GetSubAssignment();
        }
    }
}
