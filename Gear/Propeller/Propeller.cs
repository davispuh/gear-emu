using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gear.Propeller
{
    public class Register
    {
        public string Name { get; protected set; }
    }

    public class BasicInstruction
    {
        public string Name { get; protected set; }
        public string NameBrief { get; protected set; }
    }
}
