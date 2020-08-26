/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * Conditions.cs
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
    public partial class Assembly
    {

        public enum ConditionCodes : uint
        {
            IF_NEVER     = 0x00, //!< Never execute
            IF_A         = 0x01, //!< if above (!C & !Z)
            IF_NC_AND_NZ = 0x01, //!< if C clear and Z clear
            IF_NZ_AND_NC = 0x01, //!< if Z clear and C clear
            IF_NC_AND_Z  = 0x02, //!< if C clear and Z set
            IF_Z_AND_NC  = 0x02, //!< if C set and Z clear
            IF_NC        = 0x03, //!< if C clear
            IF_AE        = 0x03, //!< if above/equal (!C)
            IF_NZ_AND_C  = 0x04, //!< if Z clear and C set
            IF_C_AND_NZ  = 0x04, //!< if C set and Z clear
            IF_NZ        = 0x05, //!< if Z clear
            IF_NE        = 0x05, //!< if not equal (!Z)
            IF_C_NE_Z    = 0x06, //!< if C not equal to Z
            IF_Z_NE_C    = 0x06, //!< if Z not equal to C
            IF_NC_OR_NZ  = 0x07, //!< if C clear or Z clear
            IF_NZ_OR_NC  = 0x07, //!< if Z clear or C clear
            IF_C_AND_Z   = 0x08, //!< if C set and Z set
            IF_Z_AND_C   = 0x08, //!< if Z set and C set
            IF_C_EQ_Z    = 0x09, //!< if C equal to Z
            IF_Z_EQ_C    = 0x09, //!< if Z equal to C
            IF_E         = 0x0A, //!< if equal (Z)
            IF_Z         = 0x0A, //!< if Z set
            IF_NC_OR_Z   = 0x0B, //!< if C clear or Z set
            IF_Z_OR_NC   = 0x0B, //!< if Z set or C clear
            IF_B         = 0x0C, //!< if below (C)
            IF_C         = 0x0C, //!< if C set
            IF_NZ_OR_C   = 0x0D, //!< if Z clear or C set
            IF_C_OR_NZ   = 0x0D, //!< if C set or Z clear
            IF_Z_OR_C    = 0x0E, //!< if Z set or C set
            IF_BE        = 0x0E, //!< if below/equal (C | Z)
            IF_C_OR_Z    = 0x0E, //!< if C set or Z set
            IF_ALWAYS    = 0x0F  //!< Always execute
        }

        static public readonly string[][] Conditions = new string[][] {
            new string[] { "IF_NEVER    ", "            ", "NOP    "      },  //  0000
            new string[] { "IF_NC_AND_NZ", "IF_A        ", "IF_NZ_AND_NC" },  //  0001
            new string[] { "IF_NC_AND_Z ", "IF_Z_AND_NC "                 },  //  0010
            new string[] { "IF_NC       ", "IF_AE       "                 },  //  0011
            new string[] { "IF_C_AND_NZ ", "IF_NZ_AND_C "                 },  //  0100
            new string[] { "IF_NZ       ", "IF_NE       "                 },  //  0101
            new string[] { "IF_C_NE_Z   ", "IF_Z_NE_C   "                 },  //  0110
            new string[] { "IF_NC_OR_NZ ", "IF_NZ_OR_NC "                 },  //  0111
            new string[] { "IF_C_AND_Z  ", "IF_Z_AND_C  "                 },  //  1000
            new string[] { "IF_C_EQ_Z   ", "IF_Z_EQ_C   "                 },  //  1001
            new string[] { "IF_Z        ", "IF_E        "                 },  //  1010
            new string[] { "IF_NC_OR_Z  ", "IF_Z_OR_NC  "                 },  //  1011
            new string[] { "IF_C        ", "IF_B        "                 },  //  1100
            new string[] { "IF_C_OR_NZ  ", "IF_NZ_OR_C  "                 },  //  1101
            new string[] { "IF_Z_OR_C   ", "IF_BE       ", "IF_C_OR_Z   " },  //  1110
            new string[] { "            ", "IF_ALWAYS   ",                }   //  1111
        };
    }
}
