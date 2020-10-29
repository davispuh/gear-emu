/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2020 - Gear Developers
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

namespace Gear.Propeller
{
    public static partial class Assembly
    {
        public enum InstructionType
        {
            Normal,
            WR,
            Hub,
            Jump
        }

        public class Register : Propeller.Register
        {
            public bool Read  { get; private set; }
            public bool Write { get; private set; }

            public Register(string Name, bool Read, bool Write)
            {
                this.Name  = Name;
                this.Read  = Read;
                this.Write = Write;
            }
        }

        public class SubInstruction
        {
            public string Name           { get; private set; }
            public bool   Destination    { get; private set; }
            public bool   Source         { get; private set; }
            public bool   WZ             { get; private set; }
            public bool   WC             { get; private set; }
            public bool   WR             { get; private set; }
            public bool   ImmediateValue { get; private set; }

            public SubInstruction(string Name, bool Destination, bool Source, bool WZ, bool WC, bool WR, bool ImmediateValue)
            {
                this.Name           = Name;
                this.Destination    = Destination;
                this.Source         = Source;
                this.WZ             = WZ;
                this.WC             = WC;
                this.WR             = WR;
                this.ImmediateValue = ImmediateValue;
            }
        }

        public class Instruction
        {
            public InstructionType Type             { get; private set; }
            public SubInstruction[] SubInstructions { get; private set; }

            public Instruction(InstructionType Type, SubInstruction[] SubInstructions)
            {
                this.Type = Type;
                this.SubInstructions = SubInstructions;
            }
        }
    }
}
