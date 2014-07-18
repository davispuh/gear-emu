using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Propeller
{
    public partial class Assembly
    {
        public enum InstructionType
        {
            Normal,
            WR,
            Hub,
            Jump
        }

        public class Register : Propeller.Register
        {
            public bool Read  { get; private set; }
            public bool Write { get; private set; }

            public Register(string Name, bool Read, bool Write)
            {
                this.Name  = Name;
                this.Read  = Read;
                this.Write = Write;
            }
        }

        public class SubInstruction
        {
            public string Name           { get; private set; }
            public bool   Destination    { get; private set; }
            public bool   Source         { get; private set; }
            public bool   WZ             { get; private set; }
            public bool   WC             { get; private set; }
            public bool   WR             { get; private set; }
            public bool   ImmediateValue { get; private set; }

            public SubInstruction(string Name, bool Destination, bool Source, bool WZ, bool WC, bool WR, bool ImmediateValue)
            {
                this.Name           = Name;
                this.Destination    = Destination;
                this.Source         = Source;
                this.WZ             = WZ;
                this.WC             = WC;
                this.WR             = WR;
                this.ImmediateValue = ImmediateValue;
            }
        }

        public class Instruction
        {
            public InstructionType Type             { get; private set; }
            public SubInstruction[] SubInstructions { get; private set; }

            public Instruction(InstructionType Type, SubInstruction[] SubInstructions)
            {
                this.Type = Type;
                this.SubInstructions = SubInstructions;
            }
        }
    }
}
