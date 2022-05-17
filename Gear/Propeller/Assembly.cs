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

namespace Gear.Propeller
{
    /// <summary></summary>
    public static partial class Assembly
    {
        /// <summary>Instructions types.</summary>
        /// @version v22.05.01 - Name changed to clarify meaning of it.
        public enum InstructionTypeEnum : byte
        {
            /// <summary></summary>
            Normal,
            /// <summary></summary>
            WR,
            /// <summary></summary>
            Hub,
            /// <summary></summary>
            Jump
        }

        /// <summary>
        /// Container for PASM registers names and Read/Write characteristics.
        /// </summary>
        public class Register : Propeller.Register
        {
            /// <summary>Could be read.</summary>
            /// @version v22.05.01 - Name changed to clarify meaning of it.
            public bool CanRead  { get; }
            /// <summary>Could be written.</summary>
            /// @version v22.05.01 - Name changed to clarify meaning of it.
            public bool CanWrite { get; }

            /// <summary>Default constructor.</summary>
            /// <param name="name">Name of register.</param>
            /// <param name="readEnable">Permit Read.</param>
            /// <param name="writeEnable">Permit Write.</param>
            /// @version v22.05.01 - Parameter names changed to clarify meaning of it.
            public Register(string name, bool readEnable, bool writeEnable)
            {
                Name  = name;
                CanRead  = readEnable;
                CanWrite = writeEnable;
            }
        }

        /// <summary></summary>
        public class SubInstruction
        {
            /// <summary></summary>
            public string Name { get; }
            /// <summary></summary>
            public bool Destination { get; }
            /// <summary></summary>
            public bool Source { get; }
            /// <summary></summary>
            /// @version v22.05.01 - Name changed to clarify meaning of it.
            public bool WZEffect { get; }
            /// <summary></summary>
            /// @version v22.05.01 - Name changed to clarify meaning of it.
            public bool WCEffect { get; }
            /// <summary></summary>
            /// @version v22.05.01 - Name changed to clarify meaning of it.
            public bool WREffect { get; }
            /// <summary></summary>
            public bool ImmediateValue { get; }

            /// <summary>Default constructor.</summary>
            /// <param name="name"></param>
            /// <param name="destination"></param>
            /// <param name="source"></param>
            /// <param name="wzEffect"></param>
            /// <param name="wcEffect"></param>
            /// <param name="wrEffect"></param>
            /// <param name="immediateValue"></param>
            /// @version v22.05.01 - 
            public SubInstruction(string name, bool destination, bool source, bool wzEffect, bool wcEffect, bool wrEffect, bool immediateValue)
            {
                Name           = name;
                Destination    = destination;
                Source         = source;
                WZEffect       = wzEffect;
                WCEffect       = wcEffect;
                WREffect       = wrEffect;
                ImmediateValue = immediateValue;
            }
        }

        /// <summary>
        /// Container for PASM instructions and associated sub instructions.
        /// </summary>
        public class Instruction
        {
            /// <summary></summary>
            public InstructionTypeEnum InstructionType { get; }
            /// <summary></summary>
            public SubInstruction[] SubInstructions { get; }

            /// <summary></summary>
            /// <param name="instructionType"></param>
            /// <param name="subInstructions"></param>
            public Instruction(InstructionTypeEnum instructionType, SubInstruction[] subInstructions)
            {
                InstructionType = instructionType;
                SubInstructions = subInstructions;
            }
        }

        /// <summary>Container to define PASM conditions names.</summary>
        /// @version v22.05.01 - Added to be a specific container suitable
        /// to be used in a static ReadOnlyCollection<>.
        public class Condition
        {
            /// <summary></summary>
            public string Inst1 { get; }
            /// <summary></summary>
            public string Inst2 { get; }
            /// <summary></summary>
            public string Inst3 { get; }

            /// <summary>Constructor with 3 parameters.</summary>
            /// <param name="inst1"></param>
            /// <param name="inst2"></param>
            /// <param name="inst3"></param>
            public Condition(string inst1, string inst2, string inst3)
            {
                Inst1 = inst1;
                Inst2 = inst2;
                Inst3 = inst3;
            }
            /// <summary>Constructor with 2 parameters.</summary>
            /// <param name="inst1"></param>
            /// <param name="inst2"></param>
            public Condition(string inst1, string inst2)
            {
                Inst1 = inst1;
                Inst2 = inst2;
                Inst3 = string.Empty;
            }
        }
    }
}
