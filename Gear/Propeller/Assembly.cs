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
    /// <summary>PASM assembly language definitions.</summary>
    public static partial class Assembly
    {
        /// <summary>Instructions types.</summary>
        /// @version v22.05.03 - Name of one value changed to clarify
        /// meaning of it.
        public enum InstructionTypeEnum : byte
        {
            /// <summary>Normal instruction.</summary>
            Normal,
            /// <summary>Read/Write capable.</summary>
            ReadWrite,
            /// <summary>Require Hub access.</summary>
            Hub,
            /// <summary>Jump type.</summary>
            Jump
        }

        /// <summary>Container for PASM registers names and Read/Write
        /// characteristics.</summary>
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

        /// <summary>Container for a PASM instruction variant.</summary>
        /// @version v22.07.xx - Renamed class to be more meaningfully.
        public class InstructionVariant
        {
            /// <summary>Code name of instruction.</summary>
            public string Name { get; }

            /// <summary>Flag if use Destination bits.</summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseDestination { get; }

            /// <summary>Flag if use Source bits.</summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseSource { get; }

            /// <summary>Flag if use WZ Effect (zero) bit.</summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseWZ_Effect { get; }

            /// <summary>Flag if use WC Effect (carry) bit.</summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseWC_Effect { get; }

            /// <summary>Flag if use WR_Effect (Destination Register
            /// modified) bit.</summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseWR_Effect { get; }

            /// <summary>Flag if use Immediate Value.</summary>
            /// @version v22.05.03 - Name changed to clarify meaning of it.
            public bool UseImmediateValue { get; }

            /// <summary>Text representation of coding on 32 bits.</summary>
            /// @version v22.05.03 - Added.
            public string Representation { get; }

            /// <summary>Default constructor.</summary>
            /// <param name="name">PASM code name of instruction variant.</param>
            /// <param name="useDestination">Flag to indicate if use
            /// Destination bits.</param>
            /// <param name="useSource">Flag to indicate if use Source bits.</param>
            /// <param name="useWZ_Effect">Flag to indicate if use WZ Effect
            /// (zero) bit.</param>
            /// <param name="useWC_Effect">Flag to indicate if use WC Effect
            /// (carry) bit.</param>
            /// <param name="useWR_Effect">Flag to indicate if use WR_Effect
            /// (Destination Register modified) bit.</param>
            /// <param name="useImmediateValue">Flag to indicate if use
            /// Immediate Value.</param>
            /// <param name="representation">Text representation of coding of
            /// this instruction variant.</param>
            /// @version v22.07.xx - Renamed class constructor to follow class
            /// name was changed.
            public InstructionVariant(string name, bool useDestination,
                bool useSource, bool useWZ_Effect, bool useWC_Effect,
                bool useWR_Effect, bool useImmediateValue,
                string representation)
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
        /// instructions variants.</summary>
        public class Instruction
        {
            /// <summary>Type of instruction.</summary>
            public InstructionTypeEnum InstructionType { get; }

            /// <summary>Array of instruction variants of this Instruction.</summary>
            /// @version v22.07.xx - Property renamed according to the return
            /// class name was changed to follow its class renaming.
            public InstructionVariant[] InstructionVariants { get; }

            /// <summary>Default constructor.</summary>
            /// <param name="instructionType">Type of instruction.</param>
            /// <param name="instructionVariants">Array of instruction variants
            /// of this instruction.</param>
            /// @version v22.07.xx - Parameter name changed to follow its
            /// class renaming.
            public Instruction(InstructionTypeEnum instructionType,
                InstructionVariant[] instructionVariants)
            {
                InstructionType = instructionType;
                InstructionVariants = instructionVariants;
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
