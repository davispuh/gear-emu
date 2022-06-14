/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * PropellerCPU.cs
 * Define a %Propeller P1 emulator
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

using Gear.GUI;
using Gear.PluginSupport;
using Gear.Propeller;
using Gear.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

// ReSharper disable StringLiteralTypo
// ReSharper disable InvalidXmlDocComment
// ReSharper disable InconsistentNaming
/// @brief Core of the emulation for %Propeller (model objects).
namespace Gear.EmulationCore
{
    /// @brief Identifiers for hub operations.
    /// @version v22.05.04 - Refactored enum codes to be clearer and to
    /// follow naming conventions.
    public enum HubOperationCodes : byte
    {
        /// @brief Setting the clock.
        ClkSet = 0,
        /// @brief Getting the %Cog ID.
        CogId = 1,
        /// @brief Start or restart a %Cog by ID or next available.
        CogInit = 2,
        /// @brief Stop %Cog by its ID.
        CogStop = 3,
        /// @brief Check out new semaphore and get its ID.
        LockNew = 4,
        /// @brief Return semaphore back to semaphore pool, releasing it for future
        /// LOCKNEW requests.
        LockReturn = 5,
        /// @brief Set semaphore to true and get its previous state.
        LockSet = 6,
        /// @brief Clear semaphore to false and get its previous state.
        LockClear = 7
    }

    /// @brief Possible pin states for P1 Chip.
    public enum PinState : byte
    {
        /// @brief Pin Floating.
        FLOATING = 4,
        /// @brief Output Low (0V)
        OUTPUT_LO = 2,
        /// @brief Output Hi (3.3V)
        OUTPUT_HI = 3,
        /// @brief Input Low (0V)
        INPUT_LO = 0,
        /// @brief Input Hi (3.3V)
        INPUT_HI = 1,
    }

    /// @brief Class to emulate the core of %Propeller P1 chip.
    /// @details Conceptually it comprehends the ROM, RAM (hub memory), clock , locks, hub ring,
    /// main pin state, and references to each cog (with their own cog memory, counters, frequency
    /// generator, program cursor).
    /// @version v22.06.01 - Added custom debugger text.
    [DefaultProperty("Name"), DebuggerDisplay("{TextForDebugger,nq}")]
    public class PropellerCPU : DirectMemory, INotifyPropertyChanged
    {
        /// @brief Name of Constants for setting clock of the %Cpu.
        private static readonly string[] CLKSEL = {
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
        private static readonly string[] OSCM = {
            "XINPUT+",  // External clock/oscillator:     $00000004
            "XTAL1+",   // External low-speed crystal:    $00000008
            "XTAL2+",   // External medium-speed crystal: $00000010
            "XTAL3+"    // External high-speed crystal:   $00000020
        };

        //Constants declarations of P1 Chip
        /// @brief Number of Cogs implemented in emulator for P1 Chip.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        public const int TotalCogs = 8;
        /// @brief Number of locks availability in P1 Chip.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        public const int TotalLocks = 8;
        /// @brief Number of pins of P1 Chip.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        public const int TotalPins = 64;
        /// @brief Number of physical pins of P1 Chip.
        /// @version v22.06.01 - Added.
        public const int PhysicalPins = 32;
        /// @brief Pin mask for all the 64 pins of P1 Chip.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        public const ulong PinFullMask = 0xFFFFFFFFFFFFFFFF;
        /// @brief Total Main memory implemented on P1 Chip (Hub RAM + ROM).
        /// @version v22.05.04 - Name changed to follow naming conventions.
        public const int TotalMemory = 0x10000;
        /// @brief Total RAM Hub memory implemented on P1 Chip.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        public const int TotalRAM = 0x8000;
        /// @brief max RAM address implemented on P1 Chip.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        public const int MaxRAMAddress = 0xFFFF;

        /// <summary>Global counter for CPU instances.</summary>
        /// @version v22.06.01 - Added.
        private static int _instances;

        /// <summary>Identification number of this instance of CPU.</summary>
        /// @version v22.06.01 - Added.
        public int InstanceNumber { get; }

        /// @brief Array of cogs in the CPU.
        private readonly Cog[] _cogs;
        /// @brief Number of cogs Running in the CPU.
        /// @details Helpful to detect when all the cogs are stopped so you can
        /// stop the emulator.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private uint _cogsRunning;

        /// <summary></summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private byte[] _resetMemory;

        /// <summary></summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private readonly bool[] _locksAvailable;
        /// <summary></summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private readonly bool[] _locksState;

        /// <summary></summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private readonly ClockSource[] _clockSources;
        /// <summary></summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private readonly SystemXtal _coreClockSource;

        /// <summary>Pins driven by a plugin, system or user types.</summary>
        /// @version v22.05.04 - Name changed to clarify.
        private ulong _pinsDriven;
        /// <summary></summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private ulong _pinsFloating;
        /// <summary>Indicator of some pin has changed.</summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private bool _pinChanged;
        /// <summary>Array for the state of each pin.</summary>
        /// Mainly used to expose to plugins the pin state of CPU.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private readonly PinState[] _pinStates;

        /// <summary>Codified clock mode.</summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private byte _clockMode;

        /// <summary>Remember original Clock mode, used to reset the CPU.</summary>
        /// @version v22.05.04 - Added.
        private byte _originalClockMode;
        /// <summary>Frequency of external crystal</summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private uint _xtalFreq;
        /// <summary>Frequency of CPU Core.</summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private uint _coreFreq;

        /// @brief Reference to the emulator instance running this CPU.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private readonly Emulator _emulator;

        /// @brief List of Handlers for clock ticks on plugins.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private readonly List<PluginBase> _tickHandlers;
        /// @brief List of Handlers for Pin changes on plugins.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private readonly List<PluginBase> _pinHandlers;
        /// @brief List of active PlugIns (include system ones, like cog views, etc).
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private readonly List<PluginBase> _plugIns;

        /// <summary>Event to launch when subscribed properties are changed.</summary>
        /// @version v22.05.04 - Added to implement interface INotifyPropertyChanged.
        public event PropertyChangedEventHandler PropertyChanged;

        /// @brief Emulation Time in seconds.
        /// @version v22.05.04 - Blended property with former internal member
        /// Time, to a auto value with private setter.
        public double EmulatorTime { get; private set; }
        /// <summary>System Counter in ticks units.</summary>
        /// @version v22.05.04 - Blended property with former internal member
        /// SystemCounter, to a auto value with private setter.
        public uint Counter { get; private set; }
        /// <summary></summary>
        /// @version v22.05.04 - Blended property with former internal member
        /// RingPosition, to a auto value with private setter.
        public uint RingPosition { get; private set; }

        #region Registers
        /// <summary>Property to return only register of <c>DIRA</c> pins
        /// (<c>P31..P0</c>).</summary>
        /// @version v22.05.04 - Property name changed to clarify meaning of it.
        /// @todo Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterDIRA
        public uint RegisterDIRA
        {
            get
            {
                uint direction = 0;
                for (int i = 0; i < _cogs.Length; i++)  //TODO Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterDIRA
                    if (_cogs[i] != null)
                        direction |= _cogs[i].RegisterDIRA;
                return direction;
            }
        }

        /// <summary> Property to return only register of <c>DIRB</c> pins
        /// (<c>P63..P32</c>).</summary>
        /// @version v22.05.04 - Property name changed to clarify meaning of it.
        /// @todo Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterDIRB
        public uint RegisterDIRB
        {
            get
            {
                uint direction = 0;
                for (int i = 0; i < _cogs.Length; i++)  //TODO Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterDIRB
                    if (_cogs[i] != null)
                        direction |= _cogs[i].RegisterDIRB;
                return direction;
            }
        }

        /// <summary>Property to return only register of <c>INA</c> pins
        /// (<c>P31..P0</c>).</summary>
        /// @version v22.05.04 - Property name changed to clarify meaning of it.
        /// @todo Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterOUTA
        public uint RegisterINA
        {
            get
            {
                uint localOut = 0;
                uint directionOut = RegisterDIRA;
                for (int i = 0; i < _cogs.Length; i++)  //TODO Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterOUTA
                    if (_cogs[i] != null)
                        localOut |= _cogs[i].RegisterOUTA;
                return (localOut & directionOut) | ((uint)_pinsDriven & ~directionOut);
            }
        }

        /// <summary>Property to return only register of <c>INB</c> pins
        /// (<c>P63..P32</c>).</summary>
        /// @version v22.05.04 - Property name changed to clarify meaning of it.
        /// @todo Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterOUTB
        public uint RegisterINB
        {
            get
            {
                uint localOut = 0;
                uint directionOut = RegisterDIRB;
                for (int i = 0; i < _cogs.Length; i++)  //TODO Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterOUTB
                    if (_cogs[i] != null)
                        localOut |= _cogs[i].RegisterOUTB;
                return (localOut & directionOut) | ((uint)(_pinsDriven >> 32) & ~directionOut);
            }
        }

        /// @brief Property to return complete register of <c>DIR</c> pins
        /// (<c>P63..P0</c>).
        /// @details Only take Pin use of ACTIVES cogs, making OR between them.
        /// @version v22.05.04 - Property name changed to clarify meaning of it.
        /// @todo Parallelism [complex:low, cycles:8] point in loop of_cogs[].RegisterDIR
        public ulong RegisterDIR
        {
            get
            {
                ulong direction = 0;
                for (int i = 0; i < _cogs.Length; i++)  //TODO Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterDIR
                    if (_cogs[i] != null)
                        direction |= _cogs[i].RegisterDIR;
                return direction;
            }
        }

        /// @brief Property to return complete register of <c>IN</c> pins
        /// (<c>P63..P0</c>).
        /// @details Only take Pin use of ACTIVES cogs.
        /// @version v22.05.04 - Property name changed to clarify meaning of it.
        /// @todo Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterOUT
        public ulong RegisterIN
        {
            get
            {
                ulong localOut = 0;
                ulong directionOut = RegisterDIR;   //get total pins Dir (P63..P0)
                for (int i = 0; i < _cogs.Length; i++)  //TODO Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterOUT
                    if (_cogs[i] != null)
                        localOut |= _cogs[i].RegisterOUT;
                return (localOut & directionOut) | (_pinsDriven & ~directionOut);
            }
        }

        /// @brief Property to return complete register of <c>OUT</c> pins
        /// (<c>P63..P0</c>).
        /// @details Only take Pin use of ACTIVES cogs, making OR between them.
        /// @version v22.05.04 - Property name changed to clarify meaning of it.
        /// @todo Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterOUT
        public ulong RegisterOUT
        {
            get
            {
                ulong localOut = 0;
                for (int i = 0; i < _cogs.Length; i++)  //TODO Parallelism [complex:low, cycles:8] point in loop _cogs[].RegisterOUT
                    if (_cogs[i] != null)
                        localOut |= _cogs[i].RegisterOUT;
                return localOut;
            }
        }
        #endregion

        /// <summary></summary>
        public ulong Floating => _pinsFloating & ~RegisterDIR;

        /// <summary></summary>
        /// @todo Parallelism [complex:low, cycles:8] point in loop _locksState[]
        public byte Locks
        {
            get
            {
                byte b = 0;
                for (int i = 0; i < TotalLocks; i++)  //TODO Parallelism [complex:low, cycles:8] point in loop _locksState[]
                    b |= (byte)(_locksState[i] ?
                        1 << i :
                        0b0);
                return b;
            }
        }

        /// <summary></summary>
        /// @todo Parallelism [complex:low, cycles:8] point in loop _locksAvailable[]
        public byte LocksFree
        {
            get
            {
                byte b = 0;
                for (int i = 0; i < TotalLocks; i++)  //TODO Parallelism [complex:low, cycles:8] point in loop _locksAvailable[]
                    b |= (byte)(_locksAvailable[i] ?
                        1 << i :
                        0);
                return b;
            }
        }

        /// <summary>Determine clock mode, also establishing CoreFrequency.</summary>
        /// <value>Codify the clock mode of CPU.</value>
        /// @version v22.05.04 - Blended former property Clock with internal
        /// member ClockMode, to a auto value with private setter. Also notify
        /// of property changed.
        /// @todo [possible bug] Analyze decoded clock modes on property ClockMode. Why do we need separated logic on Initialize()?
        public byte ClockMode
        {
            get => _clockMode;
            set
            {
                if (_clockMode == value)
                    return;
                _clockMode = value;
                if ((_clockMode & 0x80) != 0)
                {
                    Reset();
                    return;
                }
                switch (_clockMode & 0x7)
                {
                    case 0:
                        CoreFrequency = 12000000;
                        break;
                    case 1:
                        CoreFrequency = 20000;
                        break;
                    case 2:
                        CoreFrequency = XtalFrequency;
                        break;
                    case 3:
                        CoreFrequency = (_clockMode & 0x40) != 0 ?
                            XtalFrequency * 1 :
                            0;
                        break;
                    case 4:
                        CoreFrequency = (_clockMode & 0x40) != 0 ?
                            XtalFrequency * 2 :
                            0;
                        break;
                    case 5:
                        CoreFrequency = (_clockMode & 0x40) != 0 ?
                            XtalFrequency * 4 :
                            0;
                        break;
                    case 6:
                        CoreFrequency = (_clockMode & 0x40) != 0 ?
                            XtalFrequency * 8 :
                            0;
                        break;
                    case 7:
                        CoreFrequency = (_clockMode & 0x40) != 0 ?
                            XtalFrequency * 16 :
                            0;
                        break;
                }
                foreach (Cog cog in _cogs)
                    cog?.SetClock(CoreFrequency);
                _coreClockSource.SetFrequency(CoreFrequency);
                OnPropertyChanged(nameof(ClockMode));
            }
        }

        /// <summary>Frequency of Crystal connected to CPU.</summary>
        /// @version v22.05.04 - Modified to notify of property changed.
        public uint XtalFrequency
        {
            get => _xtalFreq;
            private set
            {
                if (_xtalFreq == value)
                    return;
                _xtalFreq = value;
                OnPropertyChanged(nameof(XtalFrequency));
            }
        }

        /// <summary>Frequency of CPU Core.</summary>
        /// @version v22.05.04 - Modified to notify of property changed.
        public uint CoreFrequency
        {
            get => _coreFreq;
            private set
            {
                if (_coreFreq == value)
                    return;
                _coreFreq = value;
                OnPropertyChanged(nameof(CoreFrequency));
            }
        }

        /// <summary>Returns a summary text of this class, to be used in debugger view.</summary>
        /// @version v22.06.01 - Added to provide debugging info.
        private string TextForDebugger =>
            $"{{{GetType().FullName}, Id: {InstanceNumber:D2}, Cogs running: {_cogsRunning:D} }}";

        /// <summary>Default Constructor.</summary>
        /// <param name="emulator">Reference to the Gear.GUI.Emulator instance
        /// controlling this PropellerCPU.</param>
        /// @version v22.06.01 - Support added for instance numbering.
        public PropellerCPU(Emulator emulator)
        {
            _emulator = emulator;
            InstanceNumber = ++_instances;
            _cogs = new Cog[TotalCogs];  // 8 general purpose cogs
            for (int i = 0; i < TotalCogs; i++)
                _cogs[i] = null;
            _pinsDriven = 0x0;
            _pinsFloating = PinFullMask;  //all pins floating initially
            _cogsRunning = 0;
            _tickHandlers = new List<PluginBase>();
            _pinHandlers = new List<PluginBase>();
            _plugIns = new List<PluginBase>();
            EmulatorTime = 0.0;
            RingPosition = 0x0;
            _locksAvailable = new bool[TotalLocks]; // 8 general purpose semaphores
            _locksState = new bool[TotalLocks];
            Memory = new byte[TotalMemory];        // 64k of memory (top 32k read-only BIOS)
            _pinStates = new PinState[TotalPins];   // We have 64 pins we will be passing on
            for (int idx = 0; idx < TotalPins; idx++)
                _pinStates[idx] = PinState.FLOATING;
            _clockSources = new ClockSource[TotalCogs];
            _coreClockSource = new SystemXtal();
            // Put ROM it top part of main RAM.
            Resources.BiosImage.CopyTo(Memory, TotalMemory - TotalRAM);
        }

        /// <summary>Default destructor.</summary>
        ~PropellerCPU() => _instances--;

        /// @brief Set a breakpoint at this CPU, showing that in the emulator
        /// where this runs.
        public void BreakPoint()
        {
            _emulator.BreakPoint();
        }

        /// @brief Validate binary length and copy its contents to %Propeller
        /// chip memory.
        /// @param program Byte stream to file.
        /// @throw ArgumentNullException If parameter array for program is null.
        /// @throw BinarySizeException If binary file size is zero.
        /// @throw BinarySizeException If binary file size is bigger than physical
        /// memory of P1.
        /// @issue{20} Detect binary size bigger than physical memory of CPU.
        /// @version v20.09.02 - Added.
        /// @todo [possible bug] Analyze initial decoded clock modes. Why do we need separated logic on Initialize()?
        public void Initialize(byte[] program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));
            if (program.Length == 0)
                throw new BinarySizeException("Binary to load is 0 bytes long!");
            if (program.Length > TotalRAM)
                throw new BinarySizeException(
                    "Binary to load is bigger than physical memory of P1 CPU :\r\n" +
                    $"Program size is {program.Length} > {TotalRAM} bytes of Total RAM!");
            //Clean up all the memory
            for (int i = 0; i < TotalRAM; i++)
                Memory[i] = 0;
            //copy program
            program.CopyTo(Memory, 0);
            _resetMemory = new byte[Memory.Length];
            Memory.CopyTo(_resetMemory, 0);
            CoreFrequency = DirectReadLong(0);
            //read clock mode from binary, to get initial clock mode
            _originalClockMode = DirectReadByte(4);
            if ((_originalClockMode & 0x18) != 0)
            {
                int pll = (_originalClockMode & 7) - 3;
                if (pll >= 0)
                    XtalFrequency = CoreFrequency / (uint)(1 << pll);
                else if (pll == -1)
                    XtalFrequency = CoreFrequency;
            }
            ClockMode = _originalClockMode;
            // Write termination code (just in case)
            uint address = (uint)DirectReadWord(0x0A) - 8;  // Load the end of the binary
            DirectWriteLong(address, 0xFFFFF9FF);
            DirectWriteLong(address + 4, 0xFFFFF9FF);
            Reset();
        }

        /// <summary>Raise the event PropertyChanged when subscribed
        /// property changes.</summary>
        /// <param name="propertyName">Name of the changed property.</param>
        /// References: <list type="bullet">
        /// <item>https://stackoverflow.com/questions/5883282/binding-property-to-control-in-winforms</item>
        /// <item>https://docs.microsoft.com/en-us/archive/msdn-magazine/2016/july/data-binding-a-better-way-to-implement-data-binding-in-net</item>
        /// <item>https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=netframework-4.7.2</item>
        /// </list>
        /// @version v22.05.04 - Added to implement interface INotifyPropertyChanged.
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// @brief Return a reference to the cog.
        /// @param cogId Cog number to get.
        /// @returns Return the reference to the cog.
        public Cog GetCog(int cogId)
        {
            return cogId < 0 || cogId >= _cogs.Length ?
                null :
                _cogs[cogId];
        }

        /// <summary>Decode the clock mode of the CPU.</summary>
        /// @version v22.05.04 - Method name changed to clarify meaning
        /// of it.
        /// @todo [possible bug] Analyze valid clock modes and compare with decoded texts.
        public string GetClockString()
        {
            string mode = string.Empty;
            if ((ClockMode & 0x80) != 0)
                mode += "RESET+";
            if ((ClockMode & 0x40) != 0)
                mode += "PLL+";
            if ((ClockMode & 0x20) != 0)
                mode += OSCM[(ClockMode & 0x18) >> 3];
            return mode + CLKSEL[ClockMode & 0x7];
        }

        /// <summary></summary>
        /// <param name="cogId">Cog number to get.</param>
        /// <returns></returns>
        public PLLGroup GetPLL(uint cogId)
        {
            return cogId >= _clockSources.Length ?
                null :
                (PLLGroup)_clockSources[cogId];
        }

        /// @brief Include a plugin in active plugin list of propeller instance.
        /// @details It see if the plugin exist already to insert or not.
        /// @param plugin Compiled plugin reference to include.
        /// @throws ArgumentNullException If parameter plugin is null.
        /// @version v22.05.04 - Added exception.
        public void IncludePlugin(PluginBase plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            if (!_plugIns.Contains(plugin))
                _plugIns.Add(plugin);
        }

        /// @brief Remove a plugin from the active plugin list of propeller instance
        /// @details Only if the plugin exists on the list, this method removes from it.
        /// Before detach, the `OnClose()` method of plugin is invoked, to do
        /// housekeeping, for example to clear pins managed by the plugin.
        /// @param plugin Compiled plugin reference to remove.
        /// @throws ArgumentNullException If parameter plugin is null.
        /// @version v22.05.04 - Added exception.
        public void RemovePlugin(PluginBase plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            if (!_plugIns.Contains(plugin))
                return;
            //call the event of instantiated plugin before remove
            plugin.OnClose();
            plugin.Dispose();
            _plugIns.Remove(plugin);
        }

        /// @brief Add a plugin to be notified on clock ticks.
        /// @details It see if the plugin exist already to insert or not.
        /// @param plugin Compiled plugin reference to include.
        /// @throws ArgumentNullException If parameter plugin is null.
        public void NotifyOnClock(PluginBase plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            if (!_tickHandlers.Contains(plugin))
                _tickHandlers.Add(plugin);
        }

        /// @brief Remove a plugin from the clock notify list.
        /// @details Only if the plugin exists on the list, this method removes from it.
        /// @param plugin Compiled plugin reference to remove.
        /// @throws ArgumentNullException If parameter plugin is null.
        /// @version v22.05.04 - Added exception.
        public void RemoveOnClock(PluginBase plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            if (_tickHandlers.Contains(plugin))
                _tickHandlers.Remove(plugin);
        }

        /// @brief Add a plugin to be notified on pin changes.
        /// @details It see if the plugin exist already to insert or not.
        /// @param plugin Compiled plugin reference to include.
        /// @throws ArgumentNullException If parameter plugin is null.
        /// @version v22.05.04 - Added exception.
        public void NotifyOnPins(PluginBase plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            if (!_pinHandlers.Contains(plugin))
                _pinHandlers.Add(plugin);
        }

        /// @brief Remove a plugin from the pin changed notify list.
        /// @details Only if the plugin exists on the list, this method removes from it.
        /// @param plugin Compiled plugin reference to remove.
        /// @throws ArgumentNullException If parameter plugin is null.
        /// @version v22.05.04 - Added exception.
        public void RemoveOnPins(PluginBase plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            if (_pinHandlers.Contains(plugin))
                _pinHandlers.Remove(plugin);
        }

        /// @brief Reset the propeller CPU to initial state.
        /// @details Release cog instances, clock sources, clear locks
        /// and pins, and reset plugins.
        /// @version v22.05.03 - Added new required parameter cog number to
        /// new Interpreted cog.
        public void Reset()
        {
            _resetMemory.CopyTo(Memory, 0x0);
            Counter = 0;
            EmulatorTime = 0.0;
            RingPosition = 0;
            //clear clock source references
            for (int i = 0; i < TotalCogs; i++)
                _clockSources[i] = null;
            //clear cog references
            for (int i = 0; i < TotalCogs; i++)
                _cogs[i] = null;
            _cogsRunning = 0;
            for (int i = 0; i < TotalLocks; i++)    //clear locks state
            {
                _locksAvailable[i] = true;
                _locksState[i] = false;
            }
            ClockMode = _originalClockMode;
            PinChanged();   //update situation of pins
            foreach (PluginBase plugin in _plugIns)
                plugin.OnReset();
            // Start the runtime in interpreted mode (fake boot)
            // Pushes the 3 primary offsets (local offset, var offset, and object offset)
            // Stack -1 is the boot parameter
            uint initFrame = DirectReadWord(10);
            DirectWriteWord(initFrame - 8, DirectReadWord(6));  // Object
            DirectWriteWord(initFrame - 6, DirectReadWord(8));  // Var
            DirectWriteWord(initFrame - 4, DirectReadWord(12)); // Local
            DirectWriteWord(initFrame - 2, DirectReadWord(14)); // Stack
            // Boot parameter is Initial PC in the lo word, and the stack frame in the hi word
            _clockSources[0] = new PLLGroup();
            _cogs[0] = new InterpretedCog(this, 0, initFrame, CoreFrequency, (PLLGroup)_clockSources[0]);
            _cogsRunning = 1;
        }

        /// @brief Stop a cog of the P1 Chip.
        /// @param cogId Cog number to stop.
        public void Stop(int cogId)
        {
            if (cogId < 0 | cogId >= TotalCogs)
                return;
            if (_cogs[cogId] == null)
                return;
            _cogs[cogId].DetachVideoHooks();
            _cogs[cogId] = null;
            _clockSources[cogId] = null;
        }

        /// @brief Advance one clock step.
        /// @details Inside it calls the OnClock() method for each plugin as clock advances. Also
        /// update the pins, by effect of calling each cog and source of clocks.
        /// @returns Success of all cog status (=true), or if some fail (=false).
        /// @todo Parallelism [complex:low, cycles:8] point in loop of _clockSources[]
        /// @todo Parallelism [complex:high, cycles:up to 8] point in loop of cog.Step()
        /// @todo Parallelism [complex:medium, cycles:varies] point in loop of Plugin.OnClock()
        public bool Step()
        {
            ulong pinsPrev;
            ulong dirPrev;
            int sourceTicked;
            bool result = true;
            do
            {
                double minimumTime = _coreClockSource.TimeUntilClock;
                sourceTicked = -1;
                // Preserve initial state of the pins
                pinsPrev = RegisterIN;
                dirPrev = RegisterDIR;
                for (int i = 0; i < TotalCogs; i++)  //TODO Parallelism [complex:low, cycles:8] point in loop of _clockSources[]
                {
                    if (_clockSources[i] == null)
                        continue;
                    double clockTime = _clockSources[i].TimeUntilClock;
                    if (clockTime < minimumTime)
                    {
                        minimumTime = clockTime;
                        sourceTicked = i;
                    }
                }
                //advances clocks
                _coreClockSource.AdvanceClock(minimumTime);
                foreach (ClockSource clockSource in _clockSources)
                    clockSource?.AdvanceClock(minimumTime);
                // Time increment
                EmulatorTime += minimumTime;
                if (sourceTicked != -1 && (pinsPrev != RegisterIN || dirPrev != RegisterDIR || _pinChanged))
                    PinChanged();
            }
            while (sourceTicked != -1);

            // CPU advances on the main clock source
            RingPosition = (RingPosition + 1) & 0xF;    // 16 positions on the ring counter
            //execute a step on each cog
            for (int i = 0; i < TotalCogs; i++)  //TODO Parallelism [complex:high, cycles:up to 8] point in loop of cog.Step()
                if (_cogs[i] != null)
                    result &= _cogs[i].Step();

            // Every other clock, a cog gets a tick
            if ((RingPosition & 1) == 0)
            {
                uint cog = RingPosition >> 1;
                if (_cogs[cog] != null)
                    _cogs[cog].RequestHubOperation();
            }
            if (pinsPrev != RegisterIN || dirPrev != RegisterDIR || _pinChanged)
                PinChanged();

            pinsPrev = RegisterIN;
            dirPrev = RegisterDIR;
            // Advance the system counter
            Counter++;
            // Run each module of the list on Time event (calling OnClock()).
            foreach (PluginBase plugin in _tickHandlers)
                plugin.OnClock(EmulatorTime, Counter);  // TODO Parallelism [complex:medium, cycles:varies] point in Plugin.OnClock()
            if (pinsPrev != RegisterIN || dirPrev != RegisterDIR || _pinChanged)
                PinChanged();

            return result;
        }

        /// @brief Update pin information seeking changes.
        /// @details Consider changes in registers DIRA and DIRB, and also
        /// generated in plugins.
        /// Inside it calls the OnPinChange() method for each plugin.
        /// @todo Parallelism [complex:low, cycles:64] point in loop _pinStates[]
        /// @todo Parallelism [complex:low, cycles:low-many] point in loop plugin.OnPinChange()
        public void PinChanged()
        {
            _pinChanged = false;
            ulong outState = RegisterOUT;
            ulong dirState = RegisterDIR;
            for (ulong mask = 1UL, i = 0UL; i < TotalPins; mask <<= 1, i++)// TODO Parallelism [complex:low, cycles:64] point on loop _pinStates[]
            {
                //if Pin i has direction set to INPUT
                if ((dirState & mask) == 0UL)
                {
                    if ((_pinsFloating & mask) != 0UL)
                        _pinStates[i] = PinState.FLOATING;
                    else
                        _pinStates[i] = (_pinsDriven & mask) != 0UL ?
                            PinState.INPUT_HI :
                            PinState.INPUT_LO;
                }
                //then Pin i has direction set to OUTPUT
                else
                    _pinStates[i] = (outState & mask) != 0UL ?
                        PinState.OUTPUT_HI :
                        PinState.OUTPUT_LO;
            }
            //traverse across plugins that use OnPinChange()
            foreach (PluginBase plugin in _pinHandlers)    // TODO Parallelism [complex:low, cycles:low-many] point in loop plugin.OnPinChange()
                plugin.OnPinChange(EmulatorTime, _pinStates);
        }

        /// <summary>Modify the state of a pin of CPU.</summary>
        /// <remarks>It validates the pin range, or do nothing, to be safe
        /// on external plugin use.</remarks>
        /// <param name="pin">Pin number to drive.</param>
        /// <param name="floating">Boolean to left the pin floating (=true)
        /// or to set on input/output mode (=false).</param>
        /// <param name="isHigh">Boolean to set on High state (=true)
        /// or to set on Low (=false).</param>
        /// @version v22.04.02 - Parameter name change, to clarify its purpose.
        public void DrivePin(int pin, bool floating, bool isHigh)
        {
            if (pin < 0 | pin >= TotalPins)
                return;
            ulong mask = (ulong)0x1 << pin;
            if (floating)
                _pinsFloating |= mask;   //set bit to 1
            else
                _pinsFloating &= ~mask;  //set bit to 0
            if (isHigh)
                _pinsDriven |= mask;  //set bit to 1
            else
                _pinsDriven &= ~mask; //set bit to 0
            _pinChanged = true;
        }

        /// <summary></summary>
        /// <param name="number"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public uint LockSet(uint number, bool set)
        {
            bool oldSetting = _locksState[number & 0x7];
            _locksState[number & 0x7] = set;
            return oldSetting ? 0xFFFFFFFF : 0;
        }

        /// <summary>Release a lock bit to available pool.</summary>
        /// <param name="number"></param>
        public void LockReturn(uint number)
        {
            _locksAvailable[number & 0x7] = true;
        }

        /// @brief Get a new lock, marking it as not free from the available
        /// pool.
        /// @returns Lock ID of the new lock created.
        public uint NewLock()
        {
            for (uint i = 0; i < _locksAvailable.Length; i++)
                if (_locksAvailable[i])
                {
                    _locksAvailable[i] = false;
                    return i;
                }
            return 0xFFFFFFFF;
        }

        /// @brief Execute the requested hub operation.
        /// @details This method is called from a cog to do the operations
        /// related to all the CPU.
        /// @param callerCog Reference to the caller Cog of this method.
        /// @param operation Hub operation to execute.
        /// @param argument Parameter given to the op-code (destination field in PASM).
        /// @param[in,out] carryFlag Carry flag that could be affected by the operation.
        /// @param[in,out] zeroFlag Zero flag that could be affected by the operation.
        /// @returns Value depending on operation.
        /// @version v22.05.04 - Changed name of method to clarify its meaning,
        /// added new required parameter cog number to new cogs created, using
        /// new property Cog.CogNum and changed parameter names to clarify
        /// meaning of them.
        public uint ExecuteHubOperation(Cog callerCog, uint operation, uint argument,
            ref bool carryFlag, ref bool zeroFlag)
        {
            uint maskedArg = argument & 0x7;
            uint cogIdx = TotalCogs;
            switch ((HubOperationCodes)operation)
            {
                case HubOperationCodes.ClkSet:
                    zeroFlag = false;
                    carryFlag = false;
                    ClockMode = (byte)argument;
                    break;

                case HubOperationCodes.CogId:
                    carryFlag = false;
                    cogIdx = (uint)callerCog.CogNum;
                    zeroFlag = cogIdx == 0;
                    return cogIdx;

                case HubOperationCodes.CogInit:
                    //determine witch cog start
                    if ((argument & 0x8) != 0)   //if free cog should be started (bit 3 is set)
                    {
                        //assign the first free cog
                        for (uint i = 0; i < TotalCogs; i++)
                           if (_cogs[i] == null)
                           {
                                cogIdx = i;
                                break;
                           }
                        //check for no free cog
                        if (cogIdx >= TotalCogs)
                        {
                            carryFlag = true;
                            return 0xFFFFFFFF;
                        }
                        carryFlag = false;
                    }
                    else  // instead specific cog should be started
                        cogIdx = maskedArg;

                    zeroFlag = cogIdx == 0;
                    PLLGroup pllGroup = new PLLGroup();
                    _clockSources[cogIdx] = pllGroup;
                    //decode param value
                    uint paramAddress = (argument >> 16) & 0xFFFC;
                    //decode program address to load to
                    uint programAddress = (argument >> 2) & 0xFFFC;
                    if (programAddress == 0xF004)
                        _cogs[cogIdx] = new InterpretedCog(this, (int)cogIdx, paramAddress, CoreFrequency, pllGroup);
                    else
                        _cogs[cogIdx] = new NativeCog(this, (int)cogIdx, programAddress, paramAddress, CoreFrequency, pllGroup);
                    _cogsRunning++;
                    return cogIdx;

                case HubOperationCodes.CogStop:
                    zeroFlag = maskedArg == 0;
                    carryFlag = _cogsRunning >= TotalCogs;
                    Stop((int)maskedArg);
                    _cogsRunning--;
                    return maskedArg;

                case HubOperationCodes.LockClear:
                    zeroFlag = maskedArg == 0;
                    carryFlag = _locksState[maskedArg];
                    _locksState[maskedArg] = false;
                    return argument;

                case HubOperationCodes.LockNew:
                    zeroFlag = false;   // initial value if no Locks available
                    carryFlag = true;   // initial value if no Locks available
                    for (uint i = 0; i < TotalLocks; i++)
                    {
                        if (!_locksAvailable[i])
                            continue;
                        _locksAvailable[i] = false;
                        carryFlag = false;
                        if (i == 0)
                            zeroFlag = true;
                        return i;
                    }
                    return 7;   // if all are occupied, return a 7, but carry is true

                case HubOperationCodes.LockReturn:
                    zeroFlag = maskedArg == 0;
                    carryFlag = true;   // initial value if no Locks available
                    for (uint i = 0; i < TotalLocks; i++)
                        if (_locksAvailable[i])
                            carryFlag = false;
                    _locksAvailable[maskedArg] = true;
                    return maskedArg;

                case HubOperationCodes.LockSet:
                    zeroFlag = maskedArg == 0;
                    carryFlag = _locksState[maskedArg];
                    _locksState[maskedArg] = true;
                    return maskedArg;
            }
            return 0;
        }
        /// @fn public uint ExecuteHubOperation(Cog callerCog, uint operation, uint argument, ref bool carryFlag, ref bool zeroFlag)
        ///
        /// @note Reference of supported Operations, based in Propeller Manual v1.2:
        /// @arg HUBOP_CLKSET  - page 271.
        /// @arg HUBOP_COGID   - page 283.
        /// @arg HUBOP_COGINIT - page 284.
        /// @arg HUBOP_COGSTOP - page 286.
        /// @arg HUBOP_LOCKNEW - page 304.
        /// @arg HUBOP_LOCKRET - page 305.
        /// @arg HUBOP_LOCKSET - page 306.
        /// @arg HUBOP_LOCKCLR - page 303.
        // End extra documentation for Method public uint HubOp(Cog caller, uint operation, uint argument, ref bool carry, ref bool zero)
        // --------------------------------------------------------------------


        /// <summary>Notify all the plugins about the closing event.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // @version 15.03.26 - Added.
        public void OnClose(object sender, FormClosingEventArgs e)
        {
            foreach(PluginBase plugin in _plugIns)
                plugin.OnClose();
        }

    }
}
