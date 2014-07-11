using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Disassembler
{
    public partial class Assembly
    {
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
