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

// ReSharper disable InconsistentNaming
namespace Gear.Propeller
{
    /// <summary></summary>
    public static partial class Assembly
    {
        /// <summary>Instructions types.</summary>
        /// @version v22.05.03 - Name of one value changed to clarify
        /// meaning of it.
        public enum InstructionTypeEnum : byte
        {
            /// <summary></summary>
            Normal,
            /// <summary></summary>
            ReadWrite,
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
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseDestination { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseSource { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseWZ_Effect { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseWC_Effect { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseWR_Effect { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseImmediateValue { get; }
            /// <summary></summary>
            /// @version v22.05.03 - Added.
            public string Representation { get; }

            /// <summary>Default constructor.</summary>
            /// <param name="name">PASM code name of sub instruction.</param>
            /// <param name="useDestination"></param>
            /// <param name="useSource"></param>
            /// <param name="useWZ_Effect"></param>
            /// <param name="useWC_Effect"></param>
            /// <param name="useWR_Effect"></param>
            /// <param name="useImmediateValue"></param>
            /// <param name="representation"></param>
            /// @version v22.05.03 - Parameter names changed to follow naming
            /// conventions and clarify meaning of them.
            public SubInstruction(string name, bool useDestination, bool useSource,
                bool useWZ_Effect, bool useWC_Effect, bool useWR_Effect,
                bool useImmediateValue, string representation)
            {
                Name = name;
                UseDestination = useDestination;
                UseSource = useSource;
                UseWZ_Effect = useWZ_Effect;
                UseWC_Effect = useWC_Effect;
                UseWR_Effect = useWR_Effect;
                UseImmediateValue = useImmediateValue;
                Representation = representation;
            }
        }

        /// <summary>Container for PASM instructions and associated
        /// sub instructions.</summary>
        public class Instruction
        {
            /// <summary></summary>
            public InstructionTypeEnum InstructionType { get; }
            /// <summary></summary>
            public SubInstruction[] SubInstructions { get; }

            /// <summary>Default constructor.</summary>
            /// <param name="instructionType">Type of instruction.</param>
            /// <param name="subInstructions">Array of sub instruction of this
            /// instruction.</param>
            /// @version v22.05.01 - Parameter names changed to follow
            /// naming conventions.
            public Instruction(InstructionTypeEnum instructionType,
                SubInstruction[] subInstructions)
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
