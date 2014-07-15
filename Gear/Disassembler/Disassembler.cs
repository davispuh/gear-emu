using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Disassembler
{
    public class Disassembler
    {
        public class Register
        {
            public string Name { get; protected set; }
        }

        public class BasicInstruction
        {
            public string Name      { get; protected set; }
            public string NameBrief { get; protected set; }
        }
    }
}
