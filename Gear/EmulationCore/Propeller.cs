/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * Propeller.cs
 * Provides the body object of a propeller emulator
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Gear;
using Gear.PluginSupport;
using Gear.GUI;

namespace Gear.EmulationCore
{
    public enum HubOperationCodes : uint
    {
        HUBOP_CLKSET = 0,
        HUBOP_COGID = 1,
        HUBOP_COGINIT = 2,
        HUBOP_COGSTOP = 3,
        HUBOP_LOCKNEW = 4,
        HUBOP_LOCKRET = 5,
        HUBOP_LOCKSET = 6,
        HUBOP_LOCKCLR = 7
    }

    public enum PinState
    {
        FLOATING,
        OUTPUT_LO,
        OUTPUT_HI,
        INPUT_LO,
        INPUT_HI,
    }

    public partial class Propeller
    {
        static private string[] CLKSEL = new string[] {
            "RCFAST",
            "RCSLOW",
            "XINPUT",
            "PLL1X",
            "PLL2X",
            "PLL4X",
            "PLL8X",
            "PLL16X"
        };

        static private string[] OSCM = new string[] {
            "XINPUT+", "XTAL1+", "XTAL2+", "XTAL3+"
        };

        private Cog[] Cogs;
        private byte[] Memory;
        private byte[] ResetMemory;

        private bool[] LocksAvailable;
        private bool[] LocksState;

        private ClockSource[] ClockSources;
        private SystemXtal CoreClockSource;

        private uint RingPosition;
        private ulong PinHi;
        private ulong PinFloat;
        private uint SystemCounter;

        private uint XtalFreq;
        private uint CoreFreq;
        private byte ClockMode;
        private PinState[] PinStates;

        private bool pinChange;

        private double Time;

        private Emulator emulator;

        private List<PluginBase> TickHandlers;
        private List<PluginBase> PinNoiseHandlers;
        private List<PluginBase> PlugIns;

        const int TOTAL_COGS = 8;
        const int TOTAL_LOCKS = 8;
        const int TOTAL_PINS = 64;
        const int TOTAL_MEMORY = 0x10000;
        const int TOTAL_RAM = 0x8000;

        public Propeller(Emulator em)
        {
            emulator = em;
            Cogs = new Cog[TOTAL_COGS];             // 8 general purpose cogs

            for (int i = 0; i < TOTAL_COGS; i++)
                Cogs[i] = null;

            PinHi = 0;
            PinFloat = 0xFFFFFFFFFFFFFFFF;

            TickHandlers = new List<PluginBase>();
            PinNoiseHandlers = new List<PluginBase>();
            PlugIns = new List<PluginBase>();

            Time = 0;
            RingPosition = 0;
            LocksAvailable = new bool[TOTAL_LOCKS]; // 8 general purpose semaphors
            LocksState = new bool[TOTAL_LOCKS];

            Memory = new byte[TOTAL_MEMORY];        // 64k of memory (top 32k read-only bios)

            PinStates = new PinState[TOTAL_PINS];   // We have 64 pins we will be passing on

            ClockSources = new ClockSource[TOTAL_COGS];
            CoreClockSource = new SystemXtal();

            // Put rom it top part of main ram.
            Resources.BiosImage.CopyTo(Memory, TOTAL_MEMORY - TOTAL_RAM);
        }

        public void BreakPoint()
        {
            emulator.BreakPoint();
        }

        public double EmulatorTime
        {
            get
            {
                return Time;
            }
        }

        public uint Counter
        {
            get
            {
                return SystemCounter;
            }
        }

        public uint Ring
        {
            get
            {
                return RingPosition;
            }
        }

        public byte this[int offset]
        {
            get
            {
                if (offset >= Memory.Length)
                    return 0x55;
                return Memory[offset];
            }
        }

        public uint DIRA
        {
            get
            {
                uint direction = 0;
                for (int i = 0; i < Cogs.Length; i++)
                {
                    if (Cogs[i] != null)
                        direction |= Cogs[i].DIRA;
                }
                return direction;
            }
        }

        public uint DIRB
        {
            get
            {
                uint direction = 0;
                for (int i = 0; i < Cogs.Length; i++)
                    if (Cogs[i] != null)
                        direction |= Cogs[i].DIRB;
                return direction;
            }
        }

        public uint INA
        {
            get
            {
                uint localOut = 0;
                uint directionOut = DIRA;

                for (int i = 0; i < Cogs.Length; i++)
                    if (Cogs[i] != null)
                        localOut |= Cogs[i].OUTA;

                return (localOut & directionOut) | ((uint)PinHi & ~directionOut);
            }
        }

        public uint INB
        {
            get
            {
                uint localOut = 0;
                uint directionOut = DIRB;

                for (int i = 0; i < Cogs.Length; i++)
                    if (Cogs[i] != null)
                        localOut |= Cogs[i].OUTB;

                return (localOut & directionOut) | ((uint)(PinHi >> 32) & ~directionOut);
            }
        }

        public ulong DIR
        {
            get
            {
                ulong direction = 0;
                for (int i = 0; i < Cogs.Length; i++)
                    if (Cogs[i] != null)
                        direction |= Cogs[i].DIR;
                return direction;
            }
        }

        public ulong IN
        {
            get
            {
                ulong localOut = 0;
                ulong directionOut = DIR;

                for (int i = 0; i < Cogs.Length; i++)
                {
                    if (Cogs[i] == null)
                        continue;
                    localOut |= Cogs[i].OUT;
                }

                return (localOut & directionOut) | (PinHi & ~directionOut);
            }
        }

        public ulong OUT
        {
            get
            {
                ulong localOut = 0;

                for (int i = 0; i < Cogs.Length; i++)
                {
                    if (Cogs[i] != null)
                        localOut |= Cogs[i].OUT;
                }

                return localOut;
            }
        }

        public ulong Floating
        {
            get
            {
                return PinFloat & ~DIR;
            }
        }

        public byte Locks
        {
            get
            {
                byte b = 0;

                for (int i = 0; i < TOTAL_LOCKS; i++)
                    b |= (byte)(LocksState[i] ? (1 << i) : 0);

                return b;
            }
        }

        public string Clock
        {
            get
            {
                string mode = "";

                if ((ClockMode & 0x80) != 0)
                    mode += "RESET+";
                if ((ClockMode & 0x40) != 0)
                    mode += "PLL+";
                if ((ClockMode & 0x20) != 0)
                    mode += OSCM[(ClockMode & 0x18) >> 3];

                return mode + CLKSEL[ClockMode & 0x7];
            }
        }

        public uint XtalFrequency
        {
            get { return XtalFreq; }
        }

        public uint CoreFrequency
        {
            get { return CoreFreq; }
        }

        public byte LocksFree
        {
            get
            {
                byte b = 0;

                for (int i = 0; i < TOTAL_LOCKS; i++)
                    b |= (byte)(LocksAvailable[i] ? (1 << i) : 0);

                return b;
            }
        }

        public void Initialize(byte[] program)
        {
            if (program.Length > TOTAL_RAM)
                return;

            for (int i = 0; i < TOTAL_RAM; i++)
                Memory[i] = 0;

            program.CopyTo(Memory, 0);
            ResetMemory = new byte[Memory.Length];
            Memory.CopyTo(ResetMemory, 0);

            CoreFreq = ReadLong(0);
            ClockMode = ReadByte(4);

            if ((ClockMode & 0x18) != 0)
            {
                int pll = (ClockMode & 7) - 3;
                if (pll >= 0)
                    XtalFreq = CoreFreq / (uint)(1 << pll);
                else if (pll == -1)
                    XtalFreq = CoreFreq;
            }

            // Write termination code (just in case)
            uint address = (uint)ReadWord(0x0A) - 8;  // Load the end of the binary
            WriteLong(address, 0xFFFFF9FF);
            WriteLong(address + 4, 0xFFFFF9FF);

            Reset();
        }

        public Cog GetCog(int id)
        {
            if (id > Cogs.Length)
                return null;

            return Cogs[id];
        }

        public PLLGroup GetPLL(uint cog)
        {
            if (cog >= ClockSources.Length)
                return null;

            return (PLLGroup)ClockSources[cog];
        }

        public void IncludePlugin(PluginBase mod)
        {
            if (!(PlugIns.Contains(mod)))
                PlugIns.Add(mod);
        }

        public void RemovePlugin(PluginBase mod)
        {
            if (PlugIns.Contains(mod))
                PlugIns.Remove(mod);
        }

        public void NotifyOnClock(PluginBase mod)
        {
            if (!(TickHandlers.Contains(mod)))
                TickHandlers.Add(mod);
        }

        public void RemoveOnClock(PluginBase mod)
        {
            if (TickHandlers.Contains(mod))
                TickHandlers.Remove(mod);
        }

        public void NotifyOnPins(PluginBase mod)
        {
            if (!(PinNoiseHandlers.Contains(mod)))
                PinNoiseHandlers.Add(mod);
        }

        public void RemoveOnPins(PluginBase mod)
        {
            if (PinNoiseHandlers.Contains(mod))
                PinNoiseHandlers.Remove(mod);
        }

        public void SetClockMode(byte mode)
        {
            ClockMode = mode;

            if ((mode & 0x80) != 0)
            {
                Reset();
                return;
            }

            switch (mode & 0x7)
            {
                case 0:
                    CoreFreq = 12000000;
                    break;
                case 1:
                    CoreFreq = 20000;
                    break;
                case 2:
                    CoreFreq = XtalFreq;
                    break;
                case 3:
                    CoreFreq = ((mode & 0x40) != 0) ? XtalFreq * 1 : 0;
                    break;
                case 4:
                    CoreFreq = ((mode & 0x40) != 0) ? XtalFreq * 2 : 0;
                    break;
                case 5:
                    CoreFreq = ((mode & 0x40) != 0) ? XtalFreq * 4 : 0;
                    break;
                case 6:
                    CoreFreq = ((mode & 0x40) != 0) ? XtalFreq * 8 : 0;
                    break;
                case 7:
                    CoreFreq = ((mode & 0x40) != 0) ? XtalFreq * 16 : 0;
                    break;
            }

            for (int i = 0; i < Cogs.Length; i++)
                if (Cogs[i] != null)
                    Cogs[i].SetClock(CoreFreq);

            CoreClockSource.SetFrequency(CoreFreq);
        }

        public void Reset()
        {
            ResetMemory.CopyTo(Memory, 0);

            SystemCounter = 0;
            Time = 0;
            RingPosition = 0;
            for (int i = 0; i < ClockSources.Length; i++)
            {
                ClockSources[i] = null;
                Cogs[i] = null;
                LocksAvailable[i] = true;
            }

            foreach (PluginBase mod in PlugIns)
                mod.OnReset();

            PinChanged();

            SetClockMode((byte)(ClockMode & 0x7F));

            // Start the runtime in interpreted mode (fake boot)

            // Pushes the 3 primary offsets (local offset, var offset, and object offset)
            // Stack -1 is the boot parameter

            uint InitFrame = ReadWord(10);

            WriteWord(InitFrame - 8, ReadWord(6));  // Object
            WriteWord(InitFrame - 6, ReadWord(8));  // Var
            WriteWord(InitFrame - 4, ReadWord(12)); // Local
            WriteWord(InitFrame - 2, ReadWord(14)); // Stack

            // Boot parameter is Inital PC in the lo word, and the stack frame in the hi word
            ClockSources[0] = new PLLGroup();

            Cogs[0] = new InterpretedCog(this, InitFrame, CoreFreq, (PLLGroup)ClockSources[0]);
        }

        public void Stop(int cog)
        {
            if (cog >= 8 || cog < 0)
                return;

            if (Cogs[cog] != null)
            {
                Cogs[cog].DetachVideoHooks();
                Cogs[cog] = null;
                ClockSources[cog] = null;
            }
        }

        public bool Step()
        {
            ulong pins;
            ulong dir;
            int sourceTicked;
            bool cogResult;
            bool result = true;

            do
            {
                double minimumTime = CoreClockSource.TimeUntilClock;
                sourceTicked = -1;

                // Preserve initial state of the pins
                pins = IN;
                dir = DIR;

                for (int i = 0; i < ClockSources.Length; i++)
                {
                    if (ClockSources[i] == null)
                        continue;

                    double clockTime = ClockSources[i].TimeUntilClock;
                    if (clockTime < minimumTime)
                    {
                        minimumTime = clockTime;
                        sourceTicked = i;
                    }
                }

                CoreClockSource.AdvanceClock(minimumTime);

                for (int i = 0; i < ClockSources.Length; i++)
                {
                    if (ClockSources[i] == null)
                        continue;

                    ClockSources[i].AdvanceClock(minimumTime);
                }

                Time += minimumTime; // Time increment

                if (sourceTicked != -1 && ((pins != IN || dir != DIR) || pinChange))
                    PinChanged();
            }
            while (sourceTicked != -1);

            // CPU advances on the main clock source
            RingPosition = (RingPosition + 1) & 0xF;    // 16 positions on the ring counter

            for (int i = 0; i < Cogs.Length; i++)
                if (Cogs[i] != null)
                {
                    cogResult = Cogs[i].Step();
                    result &= cogResult;
                }

            if ((RingPosition & 1) == 0)  // Every other clock, a cog gets a tick
            {
                uint cog = RingPosition >> 1;
                if (Cogs[cog] != null)
                    Cogs[cog].HubAccessable();
            }

            if (pins != IN || dir != DIR || pinChange)
                PinChanged();

            pins = IN;
            dir = DIR;

            // Advance the system counter
            SystemCounter++;

            // Run our modules on time event
            foreach (PluginBase mod in TickHandlers)
            {
                mod.OnClock(Time);
            }

            if (pins != IN || dir != DIR || pinChange)
                PinChanged();

            return result;
        }

        public void PinChanged()
        {
            ulong pinsState = OUT;

            pinChange = false;

            for (int i = 0; i < 64; i++)
            {
                if (((DIR >> i) & 1) == 0)
                {
                    if (((PinFloat >> i) & 1) != 0)
                        PinStates[i] = PinState.FLOATING;
                    else if (((PinHi >> i) & 1) != 0)
                        PinStates[i] = PinState.INPUT_HI;
                    else
                        PinStates[i] = PinState.INPUT_LO;
                }
                else
                {
                    if (((pinsState >> i) & 1) != 0)
                        PinStates[i] = PinState.OUTPUT_HI;
                    else
                        PinStates[i] = PinState.OUTPUT_LO;
                }
            }

            foreach (PluginBase mod in PinNoiseHandlers)
                mod.OnPinChange(Time, PinStates);
        }

        public void DrivePin(int pin, bool Floating, bool Hi)
        {
            ulong mask = (ulong)1 << pin;

            if (Floating)
                PinFloat |= mask;
            else
                PinFloat &= ~mask;

            if (Hi)
                PinHi |= mask;
            else
                PinHi &= ~mask;

            pinChange = true;
        }

        public byte ReadByte(uint address)
        {
            return Memory[address & 0xFFFF];
        }

        public ushort ReadWord(uint address)
        {
            address &= 0xFFFFFFFE;
            return (ushort)(Memory[(address++) & 0xFFFF]
                | (Memory[(address++) & 0xFFFF] << 8));
        }

        public uint ReadLong(uint address)
        {
            address &= 0xFFFFFFFC;

            return (uint)Memory[(address++) & 0xFFFF]
                | (uint)(Memory[(address++) & 0xFFFF] << 8)
                | (uint)(Memory[(address++) & 0xFFFF] << 16)
                | (uint)(Memory[(address++) & 0xFFFF] << 24);
        }

        public void WriteByte(uint address, uint value)
        {
            if ((address & 0x8000) != 0)
                return;
            Memory[(address++) & 0x7FFF] = (byte)value;
        }

        public void WriteWord(uint address, uint value)
        {
            address &= 0xFFFFFFFE;
            WriteByte(address++, (byte)value);
            WriteByte(address++, (byte)(value >> 8));
        }

        public void WriteLong(uint address, uint value)
        {
            address &= 0xFFFFFFFC;
            WriteByte(address++, (byte)value);
            WriteByte(address++, (byte)(value >> 8));
            WriteByte(address++, (byte)(value >> 16));
            WriteByte(address++, (byte)(value >> 24));
        }

        public uint LockSet(uint number, bool set)
        {
            bool oldSetting = LocksState[number & 0x7];
            LocksState[number & 0x7] = set;
            return oldSetting ? 0xFFFFFFFF : 0;
        }

        public void LockReturn(uint number)
        {
            LocksAvailable[number & 0x7] = true;
        }

        public uint NewLock()
        {
            for (uint i = 0; i < LocksAvailable.Length; i++)
                if (LocksAvailable[i])
                {
                    LocksAvailable[i] = false;
                    return i;
                }

            return 0xFFFFFFFF;
        }

        public uint CogID(Cog caller)
        {
            for (uint i = 0; i < Cogs.Length; i++)
                if (caller == Cogs[i])
                    return i;

            return 0;
        }

        public uint HubOp(Cog caller, uint operation, uint arguement, ref bool carry)
        {
            switch ((HubOperationCodes)operation)
            {
                case HubOperationCodes.HUBOP_CLKSET:
                    SetClockMode((byte)arguement);
                    break;
                case HubOperationCodes.HUBOP_COGID:
                    {
                        // TODO: DETERMINE CARRY
                        return CogID(caller);
                    }
                case HubOperationCodes.HUBOP_COGINIT:
                    {
                        uint cog = (uint)Cogs.Length;
                        uint param = (arguement >> 16) & 0xFFFC;
                        uint progm = (arguement >> 2) & 0xFFFC;

                        // Start a new cog?
                        if ((arguement & 0x08) != 0)
                        {
                            for (uint i = 0; i < Cogs.Length; i++)
                            {
                                if (Cogs[i] == null)
                                {
                                    cog = i;
                                    break;
                                }
                            }

                            if (cog >= Cogs.Length)
                            {
                                carry = true;
                                return 0xFFFFFFFF;
                            }
                        }
                        else
                        {
                            cog = (arguement & 7);
                        }

                        PLLGroup pll = new PLLGroup();

                        ClockSources[cog] = (ClockSource)pll;

                        if (progm == 0xF004)
                            Cogs[cog] = new InterpretedCog(this, param, CoreFreq, pll);
                        else
                            Cogs[cog] = new NativeCog(this, progm, param, CoreFreq, pll);

                        carry = false;
                        return (uint)cog;
                    }
                case HubOperationCodes.HUBOP_COGSTOP:
                    Stop((int)(arguement & 7));

                    // TODO: DETERMINE CARRY
                    // TODO: DETERMINE RESULT
                    return arguement;
                case HubOperationCodes.HUBOP_LOCKCLR:
                    carry = LocksState[arguement & 7];
                    LocksState[arguement & 7] = false;
                    // TODO: DETERMINE RESULT
                    return arguement;
                case HubOperationCodes.HUBOP_LOCKNEW:
                    for (uint i = 0; i < LocksAvailable.Length; i++)
                    {
                        if (LocksAvailable[i])
                        {
                            LocksAvailable[i] = false;
                            carry = false;
                            return i;
                        }
                    }
                    carry = true;   // No Locks available
                    return 0;       // Return 0 ?
                case HubOperationCodes.HUBOP_LOCKRET:
                    LocksAvailable[arguement & 7] = true;
                    // TODO: DETERMINE CARRY
                    // TODO: DETERMINE RESULT
                    return arguement;
                case HubOperationCodes.HUBOP_LOCKSET:
                    carry = LocksState[arguement & 7];
                    LocksState[arguement & 7] = true;
                    // TODO: DETERMINE RESULT
                    return arguement;
                default:
                    // TODO: RAISE EXCEPTION
                    break;
            }

            return 0;
        }

    }
}
