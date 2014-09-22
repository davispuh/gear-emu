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
    /// 
    public enum HubOperationCodes : uint
    {
        HUBOP_CLKSET  = 0,  //!< Setting the clock
        HUBOP_COGID   = 1,  //!< Getting the Cog ID
        HUBOP_COGINIT = 2,  //!< Start or restart a Cog by ID
        HUBOP_COGSTOP = 3,  //!< Stop Cog by its ID
        HUBOP_LOCKNEW = 4,  //!< Check out new semaphore and get its ID
        //!< Return semaphore back to semaphore pool, releasing it for future LOCKNEW requests.
        HUBOP_LOCKRET = 5,  
        HUBOP_LOCKSET = 6,  //!< Set semaphore to true and get its previous state
        HUBOP_LOCKCLR = 7   //!< Clear semaphore to false and get its previous state
    }

    /// @brief Possible pin states.
    /// 
    public enum PinState
    {
        FLOATING,   //!< Pin Floating
        OUTPUT_LO,  //!< Output Low (0V)
        OUTPUT_HI,  //!< Output Hi (3.3V)
        INPUT_LO,   //!< Input Low (0V)
        INPUT_HI,   //!< Input Hi (3.3V)
    }

    /// @todo Document Gear.EmulationCore.PropellerCPU class.
    /// 
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

        //!< @todo Document member Gear.EmulationCore.PropellerCPU.Cogs
        private Cog[] Cogs;         
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
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.PinStates
        private PinState[] PinStates;

        //!< @todo Document member Gear.EmulationCore.PropellerCPU.pinChange
        private bool pinChange;

        //!< @todo Document member Gear.EmulationCore.PropellerCPU.Time
        private double Time;

        //!< @todo Document member Gear.EmulationCore.PropellerCPU.emulator
        private Emulator emulator;

        /// @brief Versionated List of Handlers for clock ticks on plugins.
        private VersionatedContainerCollection TickHandlers;      
        /// @brief Versionated List of Handlers for Pin changes on plugins.
        private VersionatedContainerCollection PinHandlers;
        /// @brief List of active PlugIns (include system ones, like cog views, etc).
        private List<PluginBase> PlugIns;          

        //Expose constants declarations to use on the project. 
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.TOTAl_COGS
        public const int TOTAL_COGS   = 8;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.TOTAL_LOCKS
        public const int TOTAL_LOCKS  = 8;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.TOTAL_PINS
        public const int TOTAL_PINS   = 64;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.TOTAL_MEMORY
        public const int TOTAL_MEMORY = 0x10000;
        //!< @todo Document member Gear.EmulationCore.PropellerCPU.TOTAL_RAM
        public const int TOTAL_RAM    = 0x8000;     

        /// @brief PropellerCPU Constructor.
        /// @param em Reference to the Gear.GUI.Emulator instance controlling this PropellerCPU.
        public PropellerCPU(Emulator em)
        {
            emulator = em;
            Cogs = new Cog[TOTAL_COGS];             // 8 general purpose cogs

            for (int i = 0; i < TOTAL_COGS; i++)
                Cogs[i] = null;

            PinHi = 0;
            PinFloat = 0xFFFFFFFFFFFFFFFF;

            TickHandlers = new VersionatedContainerCollection();
            PinHandlers = new VersionatedContainerCollection();
//            PlugInsVer = new VersionatedContainerCollection();
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

        /// @todo Document method Gear.EmulationCore.PropellerBreakPoint().
        /// 
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

        /// @todo Document method Gear.EmulationCore.PropellerCPU.GetCog().
        /// 
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
                plugin.Versioning = new PluginVersioning(plugin);   //save versioning run time info
            }
        }

        /// @brief Remove a plugin from the active plugin list of propeller instance
        /// @details Only if the plugin exists on the list, this method removes from it.
        /// Before detach, the `OnClose()` method of plugin is invoqued, to do
        /// housekeeping, for example to clear pins managed by the plugin.
        /// @param[in] plugin Compiled plugin reference to remove
        public void RemovePlugin(PluginBase plugin)
        {
            //if (PlugInsVer.Contains(plugin, PluginVersioning.memberType.PresentChip))
            //    PlugInsVer.Remove(plugin);
            if (PlugIns.Contains(plugin))
            {
                plugin.OnClose();      //call the event of instanciated plugin before remove 
                plugin.Versioning.Dispose();   //do some internal clean up
                PlugIns.Remove(plugin);
            }
        }

        /// @brief Add a plugin to be notified on clock ticks.
        /// @details It see if the plugin exist already to insert or not.
        /// @param plugin Compiled plugin reference to include.
        public void NotifyOnClock(PluginBase plugin)
        {
            if (!(TickHandlers.Contains(plugin, PluginVersioning.memberType.OnClock)))
                try
                {
                    TickHandlers.Add(plugin, PluginVersioning.memberType.OnClock);
                }
                catch (VersioningPluginException e)
                {
                    MessageBox.Show(e.Message + "\r\n" + this.ToString() + ".NotifyOnClock()",
                        "Plugin Version Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
        }

        /// @brief Remove a plugin from the clock notify list.
        /// @details Only if the plugin exists on the list, this method removes from it. 
        /// @param plugin Compiled plugin reference to remove.
        public void RemoveOnClock(PluginBase plugin)
        {
            if (TickHandlers.Contains(plugin, PluginVersioning.memberType.OnClock))
                TickHandlers.Remove(plugin);
        }

        /// @brief Add a plugin to be notified on pin changes.
        /// @details It see if the plugin exist already to insert or not.
        /// @param plugin Compiled plugin reference to include.
        public void NotifyOnPins(PluginBase plugin)
        {
            if (!(PinHandlers.Contains(plugin, PluginVersioning.memberType.OnPinChange)))
                try
                {
                    PinHandlers.Add(plugin, PluginVersioning.memberType.OnPinChange);
                }
                catch (VersioningPluginException e)
                {
                    MessageBox.Show(e.Message + "\r\n" + this.ToString() + ".NotifyOnPins()",
                        "Plugin Version Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
        }

        /// @brief Remove a plugin from the pin changed notify list.
        /// @details Only if the plugin exists on the list, this method removes from it. 
        /// @param plugin Compiled plugin reference to remove.
        public void RemoveOnPins(PluginBase plugin)
        {
            if (PinHandlers.Contains(plugin, PluginVersioning.memberType.OnPinChange))
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
            for (int i = 0; i < TOTAL_LOCKS; i++)    //clear locks state
            {
                LocksAvailable[i] = true;
                LocksState[i] = false;
            }

            foreach (PluginBase mod in PlugIns)
            {
                mod.OnReset();
            }

            PinChanged();   //update situation of pins

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

        /// @brief Stop a %cog in the %propeller.
        ///
        /// @param[in] cog %Cog number to stop.
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

            //build the max parameter list needed for versions of PluginBase.OnClock() method.
            PluginVersioning.ParamMemberInfo[] parms = { 
                new PluginVersioning.ParamMemberInfo("time", Time), 
                new PluginVersioning.ParamMemberInfo("sysCounter", SystemCounter) 
            };
            // Run each module of the list on time event (calling the appropiate OnClock()).
            foreach (VersionatedContainer cont in TickHandlers)
                cont.Invoke(parms);

            if (pins != IN || dir != DIR || pinChange)
                PinChanged();

            return result;
        }

        /// @brief Update pin information when are changes.
        /// @details Consider changes in DIRA and DIRB, and also generated in plugins.
        /// Inside it calls the OnPinChange() method for each plugin.
        public void PinChanged()
        {
            ulong pinsState = OUT;  //get total pins (P63..P0) OUT state

            pinChange = false;

            for (int i = 0; i < TOTAL_PINS; i++)    //loop for each pin of the chip
            {
                if (((DIR >> i) & 1) == 0)  //if Pin i has direction set to INPUT
                {
                    if (((PinFloat >> i) & 1) != 0)
                        PinStates[i] = PinState.FLOATING;
                    else if (((PinHi >> i) & 1) != 0)
                        PinStates[i] = PinState.INPUT_HI;
                    else
                        PinStates[i] = PinState.INPUT_LO;
                }
                else                     //then Pin i has direction set to OUTPUT
                {
                    if (((pinsState >> i) & 1) != 0)
                        PinStates[i] = PinState.OUTPUT_HI;
                    else
                        PinStates[i] = PinState.OUTPUT_LO;
                }
            }

            //build the max parameter list needed for versions of PluginBase.OnPinChange() method.
            PluginVersioning.ParamMemberInfo[] parms = { 
                new PluginVersioning.ParamMemberInfo("time", Time), 
                new PluginVersioning.ParamMemberInfo("pins", PinStates)
            };
            //traverse across plugins that use OnPinChange()
            foreach (VersionatedContainer cont in PinHandlers)
                cont.Invoke(parms);
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

        /// @todo Document method Gear.EmulationCore.PropellerCPU.NewLock().
        /// 
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

        /// @todo Document method Gear.EmulationCore.PropellerCPU.CogID().
        /// 
        public uint CogID(Cog caller)
        {
            for (uint i = 0; i < Cogs.Length; i++)
                if (caller == Cogs[i])
                    return i;

            return 0;
        }

        /// @todo Document method Gear.EmulationCore.PropellerCPU.HubOp().
        /// 
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
                        if ((arguement & TOTAL_COGS) != 0)
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
