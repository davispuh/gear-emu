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

using System;

namespace Gear.Disassembler
{
    /// <summary>
    ///
    /// </summary>
    public static class Spin
    {
        /// <summary>
        ///
        /// </summary>
        public class ParsedMemoryOperation
        {
            public byte                        Opcode           { get; private set; }
            public uint                        Address          { get; private set; }
            public byte                        Register         { get; private set; }
            public bool                        AssemblyRegister { get; private set; }
            public Propeller.Spin.MemoryAction Action           { get; private set; }

            /// <summary>
            ///
            /// </summary>
            /// <param name="Opcode"></param>
            public ParsedMemoryOperation(byte Opcode)
            {
                this.Opcode           =                                Opcode;
                this.Address          = (uint                       )( Opcode       | 0x1E0);
                this.Register         = (byte                       )( Opcode       & 0x00F);
                this.AssemblyRegister =                              ((Opcode >> 4) & 0x001) == 0x01;
                this.Action           = (Propeller.Spin.MemoryAction)((Opcode >> 5) & 0x00F);
            }

            /// <summary>
            ///
            /// </summary>
            /// <returns></returns>
            public Propeller.Register GetRegister()
            {
                if (this.AssemblyRegister)
                {
                    return Propeller.Assembly.Registers[this.Register];
                }
                else
                {
                    return Propeller.Spin.Registers[this.Register];
                };
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="SourceAssignment"></param>
        /// <param name="ParsedAssignment"></param>
        /// <returns></returns>
        public static Propeller.Spin.SubAssignment GetSubAssignment(Propeller.Spin.Assignment SourceAssignment, ParsedAssignment ParsedAssignment)
        {
            switch (SourceAssignment.Type)
            {
                case Propeller.Spin.AssignmentType.WriteRepeat:
                    return SourceAssignment.SubAssignments[ParsedAssignment.Bit1 ? 1 : 0];
                case Propeller.Spin.AssignmentType.Normal:
                    return SourceAssignment.SubAssignments[ParsedAssignment.Bit2 ? 1 : 0];
                case Propeller.Spin.AssignmentType.Size:
                    return SourceAssignment.SubAssignments[(int)ParsedAssignment.Size];
            }
            throw new Exception("Unknown Assignment Type: " + SourceAssignment.Type.ToString());
        }

        /// <summary>
        ///
        /// </summary>
        public class ParsedAssignment
        {
            public  byte                          Opcode            { get; private set; }
            public  bool                           Push             { get; private set; }
            public  bool                           Math             { get; private set; }
            public  byte                           ASG              { get; private set; }
            public  byte                           MTH              { get; private set; }
            public  bool                           Bit1             { get; private set; }
            public  bool                           Bit2             { get; private set; }
            public  Propeller.Spin.AssignmentSize  Size             { get; private set; }
            public  bool                           Swap             { get; private set; }
            public  Propeller.Spin.Assignment      SourceAssignment { get; private set; }
            public  Propeller.Spin.MathInstruction MathAssignment   { get; private set; }

            private Propeller.Spin.SubAssignment   SourceSubAssignment;

            /// <summary>
            ///
            /// </summary>
            /// <param name="Opcode"></param>
            public ParsedAssignment(byte Opcode)
            {
                this.Opcode =                   Opcode;
                this.Push   =                 ((Opcode >> 7) & 0x01) == 0x01;
                this.Math   =                 ((Opcode >> 6) & 0x01) == 0x01;
                this.ASG    =           (byte)((Opcode >> 3) & 0x07);
                this.MTH    =           (byte) (Opcode       & 0x1F);
                this.Bit1   =                 ((Opcode >> 1) & 0x01) == 0x01;
                this.Bit2   =                 ((Opcode >> 2) & 0x01) == 0x01;
                this.Size = (Propeller.Spin.AssignmentSize)((Opcode >> 1) & 0x03);
                this.Swap   =                 ((Opcode >> 5) & 0x01) == 0x01;

                if (this.Math)
                {
                    this.MathAssignment = Propeller.Spin.MathInstructions[this.MTH];
                }
                else
                {
                    this.SourceAssignment = Propeller.Spin.Assignments[this.ASG];
                }
                this.SourceSubAssignment = null;
            }

            /// <summary>
            ///
            /// </summary>
            /// <returns></returns>
            public Propeller.Spin.SubAssignment GetSubAssignment()
            {
                if (this.Math)
                {
                    return null;
                }
                else if (this.SourceSubAssignment == null)
                {
                    this.SourceSubAssignment = Spin.GetSubAssignment(this.SourceAssignment, this);
                }
                return this.SourceSubAssignment;
            }

            /// <summary>
            ///
            /// </summary>
            /// <returns></returns>
            public Propeller.BasicInstruction GetBasicInstruction()
            {
                if (this.Math)
                {
                    return this.MathAssignment;
                };
                return GetSubAssignment();
            }
        }
    }
}
