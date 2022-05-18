/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * AssemblyConditions.cs
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

using System.Collections.ObjectModel;

// ReSharper disable InconsistentNaming
namespace Gear.Propeller
{
    public partial class Assembly
    {
        /// <summary>Condition codes.</summary>
        public enum ConditionCodes : byte
        {
            /// <Summary>Never execute</summary>
            IF_NEVER = 0x00,
            /// <Summary>if above (!C & !Z)</summary>
            IF_A = 0x01,
            /// <Summary>if C clear and Z clear</summary>
            IF_NC_AND_NZ = 0x01,
            /// <Summary>if Z clear and C clear</summary>
            IF_NZ_AND_NC = 0x01,
            /// <Summary>if C clear and Z set</summary>
            IF_NC_AND_Z = 0x02,
            /// <Summary>if C set and Z clear</summary>
            IF_Z_AND_NC = 0x02,
            /// <Summary>if C clear</summary>
            IF_NC = 0x03,
            /// <Summary>if above/equal (!C)</summary>
            IF_AE = 0x03,
            /// <Summary>if Z clear and C set</summary>
            IF_NZ_AND_C = 0x04,
            /// <Summary>if C set and Z clear</summary>
            IF_C_AND_NZ = 0x04,
            /// <Summary>if Z clear</summary>
            IF_NZ = 0x05,
            /// <Summary>if not equal (!Z)</summary>
            IF_NE = 0x05,
            /// <Summary>if C not equal to Z</summary>
            IF_C_NE_Z = 0x06,
            /// <Summary>if Z not equal to C</summary>
            IF_Z_NE_C = 0x06,
            /// <Summary>if C clear or Z clear</summary>
            IF_NC_OR_NZ = 0x07,
            /// <Summary>if Z clear or C clear</summary>
            IF_NZ_OR_NC = 0x07,
            /// <Summary>if C set and Z set</summary>
            IF_C_AND_Z = 0x08,
            /// <Summary>if Z set and C set</summary>
            IF_Z_AND_C = 0x08,
            /// <Summary>if C equal to Z</summary>
            IF_C_EQ_Z = 0x09,
            /// <Summary>if Z equal to C</summary>
            IF_Z_EQ_C = 0x09,
            /// <Summary>if equal (Z)</summary>
            IF_E = 0x0A,
            /// <Summary>if Z set</summary>
            IF_Z = 0x0A,
            /// <Summary>if C clear or Z set</summary>
            IF_NC_OR_Z = 0x0B,
            /// <Summary>if Z set or C clear</summary>
            IF_Z_OR_NC = 0x0B,
            /// <Summary>if below (C)</summary>
            IF_B = 0x0C,
            /// <Summary>if C set</summary>
            IF_C = 0x0C,
            /// <Summary>if Z clear or C set</summary>
            IF_NZ_OR_C = 0x0D,
            /// <Summary>if C set or Z clear</summary>
            IF_C_OR_NZ = 0x0D,
            /// <Summary>if Z set or C set</summary>
            IF_Z_OR_C = 0x0E,
            /// <Summary>if below/equal (C | Z)</summary>
            IF_BE = 0x0E,
            /// <Summary>if C set or Z set</summary>
            IF_C_OR_Z = 0x0E,
            /// <Summary>Always execute</summary>
            IF_ALWAYS = 0x0F
        }

        /// <summary>
        /// Declaration of P1 PASM %Cog %Condition codes (from enum ConditionCodes).
        /// </summary>
        /// @version v22.05.01 - Fix security warning CA2105: 'Array fields
        /// should not be read only', adopting ReadOnlyCollection static
        /// object as storage.
        public static readonly ReadOnlyCollection<Condition> Conditions =
            new ReadOnlyCollection<Condition>(new[]
            {
                new Condition("IF_NEVER    ", "            ", "NOP    "      ),  //  0000
                new Condition("IF_NC_AND_NZ", "IF_A        ", "IF_NZ_AND_NC" ),  //  0001
                new Condition("IF_NC_AND_Z ", "IF_Z_AND_NC "                 ),  //  0010
                new Condition("IF_NC       ", "IF_AE       "                 ),  //  0011
                new Condition("IF_C_AND_NZ ", "IF_NZ_AND_C "                 ),  //  0100
                new Condition("IF_NZ       ", "IF_NE       "                 ),  //  0101
                new Condition("IF_C_NE_Z   ", "IF_Z_NE_C   "                 ),  //  0110
                new Condition("IF_NC_OR_NZ ", "IF_NZ_OR_NC "                 ),  //  0111
                new Condition("IF_C_AND_Z  ", "IF_Z_AND_C  "                 ),  //  1000
                new Condition("IF_C_EQ_Z   ", "IF_Z_EQ_C   "                 ),  //  1001
                new Condition("IF_Z        ", "IF_E        "                 ),  //  1010
                new Condition("IF_NC_OR_Z  ", "IF_Z_OR_NC  "                 ),  //  1011
                new Condition("IF_C        ", "IF_B        "                 ),  //  1100
                new Condition("IF_C_OR_NZ  ", "IF_NZ_OR_C  "                 ),  //  1101
                new Condition("IF_Z_OR_C   ", "IF_BE       ", "IF_C_OR_Z   " ),  //  1110
                new Condition("            ", "IF_ALWAYS   "                 )   //  1111
            });
    }
}
