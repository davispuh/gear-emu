/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * PropellerCPU.cs
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

/// @todo Document Gear.EmulationCore namespace.
/// 
namespace Gear.EmulationCore
{
    /// @brief Identifiers for hub operations.
    public enum HubOperationCodes : uint
    {
        /// @brief Setting the clock.
        HUBOP_CLKSET  = 0,
        /// @brief Getting the Cog ID.
        HUBOP_COGID   = 1,
        /// @brief Start or restart a Cog by ID or next available.
        HUBOP_COGINIT = 2,
        /// @brief Stop Cog by its ID.
        HUBOP_COGSTOP = 3,
        /// @brief Check out new semaphore and get its ID.
        HUBOP_LOCKNEW = 4,
        /// @brief Return semaphore back to semaphore pool, releasing it for future LOCKNEW requests.
        HUBOP_LOCKRET = 5,
        /// @brief Set semaphore to true and get its previous state.
        HUBOP_LOCKSET = 6,
        /// @brief Clear semaphore to false and get its previous state.
        HUBOP_LOCKCLR = 7   
    }

    /// @brief Possible pin states for P1 Chip.
    public enum PinState
    {
        /// @brief Pin Floating.
        FLOATING  = 4,
        /// @brief Output Low (0V)
        OUTPUT_LO = 2,
        /// @brief Output Hi (3.3V)
        OUTPUT_HI = 3,
        /// @brief Input Low (0V)
        INPUT_LO  = 0,
        /// @brief Input Hi (3.3V)
        INPUT_HI  = 1,   
    }

    /// @brief Class to emulate the core of Propeller P1 chip.
    /// @details Conceptually it comprehends the ROM, RAM (hub memory), clock , locks, hub ring, main 
    /// pin state, and references to each cog (with their own cog memory, counters, frequency 
    /// generator, program cursor).
    public partial class PropellerCPU
    {
        /// @brief Name of Constants for setting Clock.
        /// 
        static private string[] CLKSEL = new string[] {
            "RCFAST",   // Internal fast oscillator:    $00000001
            "RCSLOW",   // Internal slow oscillator:    $00000002
            "XINPUT",   // External clock/oscillator:   $00000004
            "PLL1X",    // External frequency times 1:  $00000040
            "PLL2X",    // External frequency times 2:  $00000080
            "PLL4X",    // External frequency times 4:  $00000100
            "PLL8X",    // External frequency times 8:  $00000200
            "PLL16X"    // External frequency times 16: $00000400
        };

        /// @brief Name of external clock constants.
        /// 
        static private string[] OSCM = new string[] {
            "XINPUT+",  // External clock/oscillator:     $00000004
            "XTAL1+",   // External low-speed crystal:    $00000008
            "XTAL2+",   // External medium-speed crystal: $00000010 
            "XTAL3+"    // External high-speed crystal:   $00000020
        };

        /// @brief Array of cogs in the CPU.
        private Cog[] Cogs;
        /// @brief Number of cogs Running in the CPU.
        /// @details Helpful to detect when all the cogs are stopped so you can stop the emulator.
        /// @version 14.09.29 - Added to help detecting the complete stop of the CPU. 
        private uint CogsRunning;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.Memory
        private byte[] Memory;      
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.ResetMemory
        private byte[] ResetMemory;

        //!< @todo Document member Gear.EmulationCore.PropellerCPU.LocksAvailable
        private bool[] LocksAvailable;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.LocksState
        private bool[] LocksState;

        //!< @todo Document member Gear.EmulationCore.PropellerCPU.ClockSources
        private ClockSource[] ClockSources;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.CoreClockSource
        private SystemXtal CoreClockSource; 

        //!< @todo Document member Gear.EmulationCore.PropellerCPU.RingPosition
        private uint RingPosition;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.PinHi
        private ulong PinHi;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.PinFloat
        private ulong PinFloat;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.SystemCounter
        private uint SystemCounter;

        //!< @todo Document member Gear.EmulationCore.PropellerCPU.XtalFreq
        private uint XtalFreq;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.CoreFreq
        private uint CoreFreq;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.ClockMode
        private byte ClockMode;
        /// @brief Array for the state of each pin.
        /// @details Mainly used to expose to plugins the pin state of
        private PinState[] PinStates;

        //!< @todo Document member Gear.EmulationCore.PropellerCPU.pinChange
        private bool pinChange;

        /// @brief Emulation Time in secounds units.
        private double Time;

        /// @brief Reference to the emulator instance running this CPU.
        private Emulator emulator;

        /// @brief Versionated List of Handlers for clock ticks on plugins.
        private List<PluginBase> TickHandlers;      
        /// @brief Versionated List of Handlers for Pin changes on plugins.
        private List<PluginBase> PinHandlers;
        /// @brief List of active PlugIns (include system ones, like cog views, etc).
        private List<PluginBase> PlugIns;          

        //Expose constants declarations of P1 Chip to use on the emulation. 
        /// @brief Cogs implemented in emulator for P1 Chip.
        public const int TOTAL_COGS      = 8;
        /// @brief Number of lock availables in P1 Chip.
        public const int TOTAL_LOCKS     = 8;
        /// @brief Number of pins of P1 Chip.
        public const int TOTAL_PINS      = 64;
        /// @brief Pin mask for all the 64 pins of P1 Chip.
        public const ulong PIN_FULL_MASK = 0xFFFFFFFFFFFFFFFF;
        /// @brief Total main memory implemented on P1 Chip (Hub RAM + ROM).
        public const int TOTAL_MEMORY    = 0x10000;
        /// @brief Total RAM hub memory implemented on P1 Chip.
        public const int TOTAL_RAM       = 0x8000;

        /// @brief PropellerCPU Constructor.
        /// @param em Reference to the Gear.GUI.Emulator instance controlling this PropellerCPU.
        public PropellerCPU(Emulator em)
        {
            emulator = em;
            Cogs = new Cog[TOTAL_COGS];             // 8 general purpose cogs

            for (int i = 0; i < TOTAL_COGS; i++)
                Cogs[i] = null;

            PinHi = 0;
            PinFloat = PIN_FULL_MASK;
            CogsRunning = 0;

            TickHandlers = new List<PluginBase>();
            PinHandlers = new List<PluginBase>();
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

        /// @brief Set a breakpoint at this CPU, showing that in the emulator where this runs.
        public void BreakPoint()
        {
            emulator.BreakPoint();
        }

        /// @todo Document property Gear.EmulationCore.PropellerCPU.EmulatorTime
        /// 
        public double EmulatorTime
        {
            get
            {
                return Time;
            }
        }

        /// @todo Document property Gear.EmulationCore.PropellerCPU.Counter
        /// 
        public uint Counter
        {
            get
            {
                return SystemCounter;
            }
        }

        /// @todo Document property Gear.EmulationCore.PropellerCPU.Ring
        /// 
        public uint Ring
        {
            get
            {
                return RingPosition;
            }
        }

        /// @todo Document operator Gear.EmulationCore.PropellerCPU[]
        /// 
        public byte this[int offset]
        {
            get
            {
                if (offset >= Memory.Length)
                    return 0x55;
                return Memory[offset];
            }
        }

        /// @todo Document property Gear.EmulationCore.PropellerCPU.DIRA
        /// 
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

        /// @todo Document property Gear.EmulationCore.PropellerCPU.DIRB
        /// 
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

        /// @todo Document property Gear.EmulationCore.PropellerCPU.INA
        /// 
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

        /// @todo Document property Gear.EmulationCore.PropellerCPU.INB
        /// 
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

        /// @brief Property for total DIR of pins (P63..P0).
        /// @details Only take Pin use of ACTIVES cogs, making OR between them.
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

        /// @brief Property for total IN of pins (P63..P0).
        /// @details Only take Pin use of ACTIVES cogs.
        public ulong IN
        {
            get
            {
                ulong localOut = 0;
                ulong directionOut = DIR;   //get total pins Dir (P63..P0)

                for (int i = 0; i < Cogs.Length; i++)
                {
                    if (Cogs[i] == null)
                        continue;
                    localOut |= Cogs[i].OUT;
                }

                return (localOut & directionOut) | (PinHi & ~directionOut);
            }
        }

        /// @brief Property for total OUT of pins (P63..P0).
        /// @details Only take Pin use of ACTIVES cogs, making OR between them.
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

        /// @todo Document property Gear.EmulationCore.PropellerCPU.Floating
        /// 
        public ulong Floating
        {
            get
            {
                return PinFloat & ~DIR;
            }
        }

        /// @todo Document property Gear.EmulationCore.PropellerCPU.Locks
        /// 
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

        /// @todo Document property Gear.EmulationCore.PropellerCPU.Clock
        /// 
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

        /// @todo Document property Gear.EmulationCore.PropellerCPU.XtalFrequency
        /// 
        public uint XtalFrequency
        {
            get { return XtalFreq; }
        }

        /// @todo Document property Gear.EmulationCore.PropellerCPU.CoreFrequency
        /// 
        public uint CoreFrequency
        {
            get { return CoreFreq; }
        }

        /// @todo Document property Gear.EmulationCore.PropellerCPU.LocksFree
        /// 
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

        /// @todo Document method Gear.EmulationCore.PropellerCPU.Initialize().
        /// 
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

        /// @brief Return the cog.
        /// @param[in] id Cog number to get the reference.
        /// @returns Return the reference to the cog.
        public Cog GetCog(int id)
        {
            if (id > Cogs.Length)
                return null;

            return Cogs[id];
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.GetPLL().
        /// 
        public PLLGroup GetPLL(uint cog)
        {
            if (cog >= ClockSources.Length)
                return null;

            return (PLLGroup)ClockSources[cog];
        }

        /// @brief Include a plugin in active plugin list of propeller instance.
        /// @details It see if the plugin exist already to insert or not.
        /// @param[in] plugin Compiled plugin reference to include.
        public void IncludePlugin(PluginBase plugin)
        {
            if (!(PlugIns.Contains(plugin)))
            {
                PlugIns.Add(plugin);   //add to the list of plugins
            }
        }

        /// @brief Remove a plugin from the active plugin list of propeller instance
        /// @details Only if the plugin exists on the list, this method removes from it.
        /// Before detach, the `OnClose()` method of plugin is invoqued, to do
        /// housekeeping, for example to clear pins managed by the plugin.
        /// @param[in] plugin Compiled plugin reference to remove
        public void RemovePlugin(PluginBase plugin)
        {
            if (PlugIns.Contains(plugin))
            {
                plugin.OnClose();      //call the event of instanciated plugin before remove 
                PlugIns.Remove(plugin);
            }
        }

        /// @brief Add a plugin to be notified on clock ticks.
        /// @details It see if the plugin exist already to insert or not.
        /// @param plugin Compiled plugin reference to include.
        public void NotifyOnClock(PluginBase plugin)
        {
            if (!(TickHandlers.Contains(plugin)))
                TickHandlers.Add(plugin);
        }

        /// @brief Remove a plugin from the clock notify list.
        /// @details Only if the plugin exists on the list, this method removes from it. 
        /// @param plugin Compiled plugin reference to remove.
        public void RemoveOnClock(PluginBase plugin)
        {
            if (TickHandlers.Contains(plugin))
                TickHandlers.Remove(plugin);
        }

        /// @brief Add a plugin to be notified on pin changes.
        /// @details It see if the plugin exist already to insert or not.
        /// @param plugin Compiled plugin reference to include.
        public void NotifyOnPins(PluginBase plugin)
        {
            if (!(PinHandlers.Contains(plugin)))
                PinHandlers.Add(plugin);
        }

        /// @brief Remove a plugin from the pin changed notify list.
        /// @details Only if the plugin exists on the list, this method removes from it. 
        /// @param plugin Compiled plugin reference to remove.
        public void RemoveOnPins(PluginBase plugin)
        {
            if (PinHandlers.Contains(plugin))
                PinHandlers.Remove(plugin);
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.SetClockMode().
        /// 
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

        /// @brief Reset the propeller CPU to initial state.
        /// @details Release cog instances, clock sources, clear locks and pins, and reset plugins.
        /// @version 14.7.21 - Separate reset loops for clocksources, cogs and locks.
        public void Reset()
        {
            ResetMemory.CopyTo(Memory, 0);

            SystemCounter = 0;
            Time = 0;
            RingPosition = 0;
            for (int i = 0; i < ClockSources.Length; i++)   //clear clock source references
            {
                ClockSources[i] = null;
            }
            for (int i = 0; i < TOTAL_COGS; i++)    //clear cog references
            {
                Cogs[i] = null;
            }
            CogsRunning = 0;

            for (int i = 0; i < TOTAL_LOCKS; i++)    //clear locks state
            {
                LocksAvailable[i] = true;
                LocksState[i] = false;
            }

            PinChanged();   //update situation of pins

            foreach (PluginBase plugin in PlugIns)
            {
                plugin.OnReset();
            }

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
            CogsRunning = 1;
        }

        /// @brief Stop a cog of the P1 Chip.
        /// @param[in] cog Cog number to stop.
        public void Stop(int cog)
        {
            if (cog >= TOTAL_COGS || cog < 0)
                return;

            if (Cogs[cog] != null)
            {
                Cogs[cog].DetachVideoHooks();
                Cogs[cog] = null;
                ClockSources[cog] = null;
            }

        }

        /// @brief Advance one clock step.
        /// @details Inside it calls the OnClock() method for each plugin as clock advances. Also 
        /// update the pins, by efect of calling each cog and source of clocks.
        public bool Step()
        {
            ulong pinsPrev;
            ulong dirPrev;
            int sourceTicked;
            double minimumTime;
            bool cogResult;
            bool result = true;

            do
            {
                minimumTime = CoreClockSource.TimeUntilClock;
                sourceTicked = -1;

                // Preserve initial state of the pins
                pinsPrev = IN;
                dirPrev = DIR;

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

                if (sourceTicked != -1 && ((pinsPrev != IN || dirPrev != DIR) || pinChange))
                    PinChanged();
            }
            while (sourceTicked != -1);

            // CPU advances on the main clock source
            RingPosition = (RingPosition + 1) & 0xF;    // 16 positions on the ring counter

            //execute a step on each cog
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

            if (pinsPrev != IN || dirPrev != DIR || pinChange)
                PinChanged();

            pinsPrev = IN;
            dirPrev = DIR;

            // Advance the system counter
            SystemCounter++;

            // Run each module of the list on time event (calling OnClock()).
            foreach (PluginBase plugin in TickHandlers)
            {
                plugin.OnClock(Time, SystemCounter);
            }

            if (pinsPrev != IN || dirPrev != DIR || pinChange)
                PinChanged();

            return result;
        }

        /// @brief Update pin information seeking changes.
        /// @details Consider changes in DIRA and DIRB, and also generated in plugins.
        /// Inside it calls the OnPinChange() method for each plugin.
        public void PinChanged()
        {
            this.pinChange = false;
            ulong outState = OUT; 
            ulong dirState = DIR;
            for (ulong mask = 1UL, i = 0UL; i < (ulong)(TOTAL_PINS); mask <<= 1, i++)
            {
                if ( (dirState & mask) == 0UL)	//if Pin i has direction set to INPUT
                {
                    if ( (PinFloat & mask) != 0UL)
                        PinStates[i] = PinState.FLOATING;
                    else if ( (PinHi & mask) != 0UL)
                        PinStates[i] = PinState.INPUT_HI;
                    else
                        PinStates[i] = PinState.INPUT_LO;
                }
                else                     //then Pin i has direction set to OUTPUT
                {
                    if ( (outState & mask) != 0UL)
                        PinStates[i] = PinState.OUTPUT_HI;
                    else
                        PinStates[i] = PinState.OUTPUT_LO;
                }
            }

            //traverse across plugins that use OnPinChange()
            foreach (PluginBase plugin in PinHandlers)
                plugin.OnPinChange(Time, PinStates);
        }

        /// @brief Drive a pin of Propeller.
        /// It validates the pin range, or do nothing, to be safe on external plugin use.
        /// @version 14.8.10 - Added validation for pin range.
        public void DrivePin(int pin, bool Floating, bool Hi)
        {
            if ( (pin >= 0) & (pin < TOTAL_PINS) )  //prevent pin overflow.
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
            };
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.ReadByte().
        /// 
        public byte ReadByte(uint address)
        {
            return Memory[address & 0xFFFF];
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.ReadWord().
        /// 
        public ushort ReadWord(uint address)
        {
            address &= 0xFFFFFFFE;
            return (ushort)(Memory[(address++) & 0xFFFF]
                | (Memory[(address++) & 0xFFFF] << 8));
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.ReadLong().
        /// 
        public uint ReadLong(uint address)
        {
            address &= 0xFFFFFFFC;

            return (uint)Memory[(address++) & 0xFFFF]
                | (uint)(Memory[(address++) & 0xFFFF] << 8)
                | (uint)(Memory[(address++) & 0xFFFF] << 16)
                | (uint)(Memory[(address++) & 0xFFFF] << 24);
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.WriteByte().
        /// 
        public void WriteByte(uint address, uint value)
        {
            if ((address & 0x8000) != 0)
                return;
            Memory[(address++) & 0x7FFF] = (byte)value;
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.WriteWord().
        /// 
        public void WriteWord(uint address, uint value)
        {
            address &= 0xFFFFFFFE;
            WriteByte(address++, (byte)value);
            WriteByte(address++, (byte)(value >> 8));
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.WriteLong().
        /// 
        public void WriteLong(uint address, uint value)
        {
            address &= 0xFFFFFFFC;
            WriteByte(address++, (byte)value);
            WriteByte(address++, (byte)(value >> 8));
            WriteByte(address++, (byte)(value >> 16));
            WriteByte(address++, (byte)(value >> 24));
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.LockSet().
        /// 
        public uint LockSet(uint number, bool set)
        {
            bool oldSetting = LocksState[number & 0x7];
            LocksState[number & 0x7] = set;
            return oldSetting ? 0xFFFFFFFF : 0;
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.LockReturn().
        /// 
        public void LockReturn(uint number)
        {
            LocksAvailable[number & 0x7] = true;
        }

        /// @brief Create a new lock.
        /// @returns Lock ID of the new lock created.
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

        /// @brief Determine the ID of the cog.
        /// @param caller Cog instance to determine the its ID.
        /// @returns Cog ID.
        public uint CogID(Cog caller)
        {
            for (uint i = 0; i < Cogs.Length; i++)
                if (caller == Cogs[i])
                    return i;

            return 0;
        }

        /// @brief Execute the hub operations.
        /// @details This method is called from a cog to do the operations related to all the CPU.
        /// @version 14.10.02 - corrected problem in COGSTOP return.
        /// @param caller Reference to the caller Cog of this method.
        /// @param operation Hub operation to execute.
        /// @param argument Parameter given to the opcode (destination field in PASM).
        /// @param[out] carry Carry flag that could be affected by the operation.
        /// @param[out] zero Zero flag that could be affected by the operation.
        /// @returns Value depending on operation.
        /// @note Reference of supported Operations, based in Propeller Manual v1.2:
        /// @arg HUBOP_CLKSET - page 271.
        /// @arg HUBOP_COGID - page 283.
        /// @arg HUBOP_COGINIT - page 284.
        /// @arg HUBOP_COGSTOP - page 286.
        /// @arg HUBOP_LOCKNEW - page 304.
        /// @arg HUBOP_LOCKRET - page 305.
        /// @arg HUBOP_LOCKSET - page 306.
        /// @arg HUBOP_LOCKCLR - page 303.
        public uint HubOp(Cog caller, uint operation, uint argument, ref bool carry, ref bool zero)
        {
            uint maskedArg = (argument & 0x7);
            uint cog = (uint)Cogs.Length;
            switch ((HubOperationCodes)operation)
            {
                case HubOperationCodes.HUBOP_CLKSET:
                    zero = false;
                    carry = false;
                    SetClockMode((byte)argument);
                    break;

                case HubOperationCodes.HUBOP_COGID:
                    carry = false;
                    cog = CogID(caller);
                    zero = (cog == 0) ? true : false;
                    return cog;

                case HubOperationCodes.HUBOP_COGINIT:
                    //determine witch cog start
                    if ((argument & 0x8) != 0)   //if free cog should be started (bit 3 is set)
                    {
                        for (uint i = 0; i < Cogs.Length; i++)  //assign the first free cog
                        {
                            if (Cogs[i] == null)
                            {
                                cog = i;
                                break;
                            }
                        }
                        if (cog >= Cogs.Length)
                        {
                            carry = true;   //no free cog
                            return 0xFFFFFFFF;
                        }
                        else 
                            carry = false;
                    }
                    else  // instead specific cog should be started
                        cog = maskedArg;
                    
                    zero = (cog == 0) ? true : false;

                    PLLGroup pll = new PLLGroup();
                    ClockSources[cog] = (ClockSource)pll;
                    uint param = (argument >> 16) & 0xFFFC;     //decode param value
                    uint progm = (argument >> 2) & 0xFFFC;      //decode program addr to load to
                    if (progm == 0xF004)
                        Cogs[cog] = new InterpretedCog(this, param, CoreFreq, pll);
                    else
                        Cogs[cog] = new NativeCog(this, progm, param, CoreFreq, pll);
                    CogsRunning++;
                    return (uint)cog;

                case HubOperationCodes.HUBOP_COGSTOP:
                    zero = (maskedArg == 0) ? true: false;
                    carry = (CogsRunning < TOTAL_COGS) ? false : true;
                    Stop((int)maskedArg);
                    CogsRunning--;
                    return maskedArg;

                case HubOperationCodes.HUBOP_LOCKCLR:
                    zero = (maskedArg == 0) ? true : false;
                    carry = LocksState[maskedArg];
                    LocksState[maskedArg] = false;
                    return argument;

                case HubOperationCodes.HUBOP_LOCKNEW:
                    zero = false;   // initial value if no Locks available
                    carry = true;   // initial value if no Locks available
                    for (uint i = 0; i < LocksAvailable.Length; i++)
                    {
                        if (LocksAvailable[i])
                        {
                            LocksAvailable[i] = false;
                            carry = false;
                            if (i == 0) 
                                zero = true;
                            return i;
                        }
                    }
                    return 7;   // if all are occupied, return a 7, but carry is true

                case HubOperationCodes.HUBOP_LOCKRET:
                    zero = (maskedArg == 0) ? true : false;
                    carry = true;   // initial value if no Locks available
                    for (uint i = 0; i < LocksAvailable.Length; i++)
                    {
                        if (LocksAvailable[i])
                        {
                            carry = false;
                        }
                    }
                    LocksAvailable[maskedArg] = true;
                    return maskedArg;

                case HubOperationCodes.HUBOP_LOCKSET:
                    zero = (maskedArg == 0) ? true : false;
                    carry = LocksState[maskedArg];
                    LocksState[maskedArg] = true;
                    return maskedArg;

                default:
                    // TODO: RAISE EXCEPTION
                    break;
            }

            return 0;
        }

        /// @brief Notify all the plugins about the closing event.
        /// @version 14.10.26 - added.
        public void OnClose(object sender, FormClosingEventArgs e)
        { 
            foreach(PluginBase plugin in PlugIns)
            {
                plugin.OnClose();
            }
        }

    }
}
