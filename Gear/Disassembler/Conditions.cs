using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Disassembler
{
    public partial class Assembly
    {
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
