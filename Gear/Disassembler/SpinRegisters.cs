using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Disassembler
{
    public partial class Spin
    {
        static public readonly Register[] Registers = new Register[] {
            new Register("MEM_0"),
            new Register("MEM_1"),
            new Register("MEM_2"),
            new Register("MEM_3"),
            new Register("MEM_4"),
            new Register("MEM_5"),
            new Register("MEM_6"),
            new Register("MEM_7"),
            new Register("MEM_8"),
            new Register("MEM_9"),
            new Register("MEM_A"),
            new Register("MEM_B"),
            new Register("MEM_C"),
            new Register("MEM_D"),
            new Register("MEM_E"),
            new Register("MEM_F")
        };
    }
}
