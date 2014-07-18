using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Propeller
{
    public partial class Assembly
    {
        /// @brief %Cog RAM Special Purpose Registers.
        ///
        /// Source: Table 15 - %Cog RAM Special Purpose Registers, %Propeller P8X32A Datasheet V1.4.0.
        public enum RegisterAddress : uint
        {
            COGID     = 0x1E9,    //!<
            INITCOGID = 0x1EF,    //!<
            PAR       = 0x1F0,    //!< Boot Parameter
            CNT       = 0x1F1,    //!< System Counter
            INA       = 0x1F2,    //!< Input States for P31 - P0.
            INB       = 0x1F3,    //!< Input States for P63 - P32.
            OUTA      = 0x1F4,    //!< Output States for P31 - P0.
            OUTB      = 0x1F5,    //!< Output States for P63 - P32.
            DIRA      = 0x1F6,    //!< Direction States for P31 - P0.
            DIRB      = 0x1F7,    //!< Direction States for P63 - P32.
            CNTA      = 0x1F8,    //!< Counter A Control.
            CNTB      = 0x1F9,    //!< Counter B Control.
            FRQA      = 0x1FA,    //!< Counter A Frequency.
            FRQB      = 0x1FB,    //!< Counter B Frequency.
            PHSA      = 0x1FC,    //!< Counter A Phase.
            PHSB      = 0x1FD,    //!< Counter B Phase.
            VCFG      = 0x1FE,    //!< Video Configuration.
            VSCL      = 0x1FF     //!< Video Scale.
        }

        public const int RegisterBaseAddress = 0x1F0;

        static public readonly Register[] Registers = new Register[] {
            new Register("PAR ", true, false),  //  $1F0
            new Register("CNT ", true, false),  //  $1F1
            new Register("INA ", true, false),  //  $1F2
            new Register("INB ", true, false),  //  $1F3
            new Register("OUTA", true, true ),  //  $1F4
            new Register("OUTB", true, true ),  //  $1F5
            new Register("DIRA", true, true ),  //  $1F6
            new Register("DIRB", true, true ),  //  $1F7
            new Register("CTRA", true, true ),  //  $1F8
            new Register("CTRB", true, true ),  //  $1F9
            new Register("FRQA", true, true ),  //  $1FA
            new Register("FRQB", true, true ),  //  $1FB
            new Register("PHSA", true, true ),  //  $1FC
            new Register("PHSB", true, true ),  //  $1FD
            new Register("VCFG", true, true ),  //  $1FE
            new Register("VSCL", true, true ),  //  $1FF
        };
    }
}
