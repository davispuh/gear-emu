/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * Cog.cs
 * Base class for a cog processor.  Abstract and must be extended.
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
using Gear.Propeller;

// ReSharper disable InconsistentNaming

namespace Gear.EmulationCore
{
    /// <summary>States of a %Cog.</summary>
    /// @version v22.05.04 - Changed to HubOperation enum code.
    public enum CogRunState : byte
    {
        /// <summary>Waiting for instruction to finish executing.</summary>
        StateExecute,
        /// <summary>%Cog is loading program memory.</summary>
        WaitLoadProgram,
        /// <summary>%Cog is executing an instruction, and waiting an allotted
        /// amount of cycles.</summary>
        WaitCycles,
        /// <summary>Waits for an allotted number of cycles before changing to
        /// a new state.</summary>
        WaitPreWait,

        /// <summary>Interpreter is booting up.</summary>
        BootInterpreter,
        /// <summary>Interpreter is executing an instruction.</summary>
        WaitInterpreter,
        /// <summary>Interpreter is fetching instruction.</summary>
        ExecInterpreter,

        /// <summary>Waits for pins to match.</summary>
        WaitPinsEqual,
        /// <summary>Waits for pins to NOT match.</summary>
        WaitPinsNotEqual,
        /// <summary>Waits for count.</summary>
        WaitCount,
        /// <summary>Waits for video.</summary>
        WaitVideo,

        /// <summary>Waiting to read byte.</summary>
        HubReadByte,
        /// <summary>Waiting to read word.</summary>
        HubReadWord,
        /// <summary>Waiting to read uint.</summary>
        HubReadLong,
        /// <summary>Waiting to perform hub operation.</summary>
        HubOperation
    }

    /// <summary>Video frame states.</summary>
    /// @pullreq{18} Enum added.
    /// @version v22.08.01 - Shorted enum names.
    public enum FrameState : byte
    {
        /// <summary>Not in a video time frame.</summary>
        None,
        /// <summary>In a video time frame.</summary>
        Hit,
        /// <summary>Left video time frame.</summary>
        Miss
    }

    /// <summary>Base class for a %Cog emulator.</summary>
    public abstract class Cog
    {
        /// <summary>Total cog memory implemented on P1 Chip.</summary>
        public const int TotalCogMemory = 0x200;  // 512 longs of memory
        /// <summary>Mask to assure fit on cog memory.</summary>
        /// @version v22.05.03 - Added.
        public const int MaskCogMemory = 0x1FF;

        /// <summary>%Cog Memory.</summary>
        /// @version v22.05.03 - Name changed to follow naming conventions.
        private readonly uint[] _cogMemory;

        /// <summary>First frequency generator of this %cog.</summary>
        /// @version v22.05.03 - Name changed to follow naming conventions.
        private readonly FreqGenerator _freqAGenerator;
        /// <summary>Second frequency generator of this %cog.</summary>
        /// @version v22.05.03 - Name changed to follow naming conventions.
        private readonly FreqGenerator _freqBGenerator;
        /// <summary>Video generator of this %cog.</summary>
        /// @version v22.05.03 - Name changed to follow naming conventions.
        private readonly VideoGenerator _videoGenerator;
        /// <summary>Frequency PLL Group generator.</summary>
        /// @version v22.05.03 - Name changed to follow naming conventions.
        private readonly PLLGroup _pllGroup;

        /// <summary>Current %Cog state.</summary>
        private protected CogRunState State;
        /// <summary>Next %Cog state.</summary>
        private protected CogRunState NextState;
        /// <summary>Program cursor address.</summary>
        /// @version v22.08.01 - Changed to private access.
        private uint ProgramAddress;

        /// <summary>Reference to the PropellerCPU instance where this
        /// object belongs.</summary>
        /// @version v22.05.03 - Property generated from private member (Hub)
        /// and changed name to use the same convention for a PropellerCPU
        /// instance reference.
        private protected PropellerCPU CpuHost { get; }

        /// <summary>Cog number, zero based.</summary>
        /// @version v22.05.04 - Transformed to a property.
        public int CogNum { get; }

        /// @brief The actual position where the program is executing in this cog.
        /// @version v22.05.03 - Changed to a property with auto value and
        /// protected visibility, from private member (PC).
        public uint ProgramCursor { get; private protected set; }

        /// @brief Break Point position in the program running in this cog.
        /// @version v22.05.03 - Changed to a property with auto value.
        public int BreakPointCogCursor { get; set; }

        /// @brief Number of cycles to wait for the current state.
        /// @version v22.08.01 - Changed to private protected access.
        private protected int StateCount { get; set; }

        /// <summary>Indicates video frame end and whether in <c>WAIT_VID</c></summary>
        /// @pullreq{18} Property added.
        /// @version v22.05.03 - Property generated from protected member.
        private protected FrameState FrameFlag { get; set; }
        /// <summary>Break if frame flag is set.</summary>
        /// @pullreq{18} Property added.
        /// @version v22.08.01 - Changed to private access.
        private FrameState FrameBreak { get; set; }

        /// @brief Property to return complete register of <c>OUT</c> pins
        /// (<c>P63..P0</c>).
        /// @details Analyze all sources of pin changes in the cog: <c>OUTA</c>,
        /// <c>OUTB</c>, the two counters and the video generator of this cog.
        /// @version v22.05.03 - Property name changed to clarify meaning of it.
        public ulong RegisterOUT =>
            _cogMemory[(int)Assembly.RegisterAddress.OUTA] |
            //Bugfix OUTB must be shifted on 64 bits long
            ((ulong)_cogMemory[(int)Assembly.RegisterAddress.OUTB] << 32) |
            _freqAGenerator.Output |
            _freqBGenerator.Output |
            _videoGenerator.Output;

        /// @brief Property to return only register of <c>OUTA</c> pins
        /// (<c>P31..P0</c>).
        /// @details Analyze all sources of pin changes in the cog for <c>OUTA</c>,
        /// pins (<c>P31..P0</c>): the two counters and the video generator
        /// of this cog.
        /// @version v22.05.03 - Property name changed to clarify meaning of it.
        public uint RegisterOUTA =>
            _cogMemory[(int)Assembly.RegisterAddress.OUTA] |
            (uint)_freqAGenerator.Output |
            (uint)_freqBGenerator.Output |
            (uint)_videoGenerator.Output;

        /// @brief Property to return only register of <c>OUTB</c> pins
        /// (<c>P63..P32</c>).
        /// @details Analyze all sources of pin changes in the cog for <c>OUTB</c>
        /// pins (<c>P63..P32</c>): the two counters and the video generator
        /// of this cog.
        /// @version v22.05.03 - Property name changed to clarify meaning of it.
        public uint RegisterOUTB =>
            (uint)((_freqAGenerator.Output |
                    _freqBGenerator.Output |
                    _videoGenerator.Output) >> 32) |
            _cogMemory[(int)Assembly.RegisterAddress.OUTB];

        /// @brief Property to return complete register of <c>DIR</c> pin
        /// (<c>P63..P0</c>).
        /// @version v22.05.03 - Property name changed to clarify meaning of it.
        public ulong RegisterDIR =>
            //Bugfix DIRB must be shifted on 64 bits long
            ((ulong)_cogMemory[(int)Assembly.RegisterAddress.DIRB] << 32) |
            _cogMemory[(int)Assembly.RegisterAddress.DIRA];

        /// <summary>Property to return only register of <c>DIRA</c> pins
        /// (<c>P31..P0</c>).</summary>
        /// @version v22.05.03 - Property name changed to clarify meaning of it.
        public uint RegisterDIRA =>
            _cogMemory[(int)Assembly.RegisterAddress.DIRA];

        /// <summary>Property to return only register of <c>DIRB</c> pins
        /// (<c>P63..P32</c>).</summary>
        /// @version v22.05.03 - Property name changed to clarify meaning of it.
        public uint RegisterDIRB =>
            _cogMemory[(int)Assembly.RegisterAddress.DIRB];

        /// <summary>Property to get or set %Cog memory value.</summary>
        /// <param name="cogAddress">Cog memory address.</param>
        /// <returns>Value of memory at address.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// @todo Check apparent error on set method, not writing to hub for special registers.
        /// @version v22.08.01 - Changed visibility to a protected setter. Parameter name changed to be more meaningful.
        public uint this[int cogAddress]
        {
            get
            {
                if (cogAddress < 0 || cogAddress >= TotalCogMemory)
                    throw new ArgumentOutOfRangeException(nameof(cogAddress));
                // show special registers, because their values are in
                // variables in Cog object and not in memory array.
                return cogAddress >= (int)Assembly.RegisterAddress.PAR ?
                    ReadLong((uint)cogAddress) :
                    _cogMemory[cogAddress];
            }
            protected set
            {
                if (cogAddress < 0 || cogAddress >= TotalCogMemory)
                    throw new ArgumentOutOfRangeException(nameof(cogAddress));
                _cogMemory[cogAddress] = value;
            }
        }

        /// @brief Show a string with human readable state of a cog.
        /// @version v22.08.01 - Corrected text for waiting video.
        public string CogStateString
        {
            get
            {
                switch (State)
                {
                    case CogRunState.HubOperation:
                    case CogRunState.HubReadByte:
                    case CogRunState.HubReadWord:
                    case CogRunState.HubReadLong:
                        return "Waiting for Hub";
                    case CogRunState.BootInterpreter:
                        return "Interpreter Boot";
                    case CogRunState.ExecInterpreter:
                        return "Interpreter Fetch";
                    case CogRunState.WaitInterpreter:
                        return "Interpreter Processing";
                    case CogRunState.StateExecute:
                    case CogRunState.WaitPreWait:
                    case CogRunState.WaitCycles:
                        return "Running instruction";
                    case CogRunState.WaitLoadProgram:
                        return "Loading Program";
                    case CogRunState.WaitCount:
                        return "Waiting (CNT)";
                    case CogRunState.WaitPinsEqual:
                        return "Waiting (PEQ)";
                    case CogRunState.WaitPinsNotEqual:
                        return "Waiting (PNE)";
                    case CogRunState.WaitVideo:
                        return "Waiting Video";
                    default:
                        return "ERROR";
                }
            }
        }

        /// <summary>String explanation of video state.</summary>
        /// @pullreq{18} Property added.
        /// @version v22.08.01 - Modified to new presentation format on screen.
        public string VideoFrameString
        {
            get
            {
                string hitState;
                switch (FrameFlag)
                {
                    case FrameState.Hit:
                        hitState = "Hit ";
                        break;
                    case FrameState.Miss:
                        hitState = "Miss";
                        break;
                    case FrameState.None:
                    default:
                        hitState = "None";
                        break;
                }
                return $"{hitState}/{_videoGenerator.FrameClock:D4}/{_videoGenerator.PixelClock:D3}";
            }
        }

        /// <summary>Default constructor.</summary>
        /// <param name="cpuHost">PropellerCPU instance where this object
        /// belongs.</param>
        /// <param name="cogNum">Cog number from PropellerCPU.</param>
        /// <param name="programAddress">Start of program to load from main
        /// memory.</param>
        /// <param name="param">PARAM value given to the Cog.</param>
        /// <param name="frequency">Frequency running the cog (the same as the
        /// propeller cpu).</param>
        /// <param name="pllGroup"></param>
        /// @pullreq{18} Initialization of added properties.
        /// @version v22.05.03 - Parameter names changed to follow naming conventions.
        protected Cog(PropellerCPU cpuHost, int cogNum, uint programAddress, uint param, uint frequency, PLLGroup pllGroup)
        {
            CpuHost = cpuHost;
            CogNum = cogNum;
            _cogMemory = new uint[TotalCogMemory];
            ProgramAddress = programAddress;

            _freqAGenerator = new FreqGenerator(cpuHost, pllGroup, true);
            _freqBGenerator = new FreqGenerator(cpuHost, pllGroup, false);
            _videoGenerator = new VideoGenerator(cpuHost, this);
            _pllGroup = pllGroup;
            // Attach the video generator to PLLs
            _pllGroup.SetupPLL(_videoGenerator);
            ProgramCursor = 0;
            BreakPointCogCursor = -1;    // Breakpoint disabled initially
            FrameFlag = FrameState.None;   //Added on pull request #18
            FrameBreak = FrameState.Miss;  //Added on pull request #18
            // We are in boot time load
            _cogMemory[(int)Assembly.RegisterAddress.PAR] = param;
            State = CogRunState.WaitLoadProgram;
            StateCount = 0;
            // Clear the special purpose registers
            for (int i = (int)Assembly.RegisterAddress.CNT; i < TotalCogMemory; i++)
                this[i] = 0x0;
            SetClock(frequency);
        }

        /// @brief Set video frame condition on which to break
        /// @version v22.05.03 - Converted to Set method from Property.
        public void SetVideoBreak(FrameState newState) =>
            FrameBreak = newState;

        /// <summary>Evaluate a precondition for an PASM instruction.</summary>
        /// <param name="conditionCode">Code of condition to test.</param>
        /// <param name="leftOp">First operand.</param>
        /// <param name="rightOp">Second operand.</param>
        /// <returns>Returns TRUE if logic condition is valid, else FALSE.</returns>
        /// @version v22.05.04 - Invert sense of return value on
        /// ConditionCompare(), to a intuitive one.
        public static bool ConditionCompare(Assembly.ConditionCodes conditionCode,
            bool leftOp, bool rightOp)
        {
            switch (conditionCode)
            {
                case Assembly.ConditionCodes.IF_NEVER:
                default:
                    break;
                case Assembly.ConditionCodes.IF_NZ_AND_NC:
                    if (!leftOp && !rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_NC_AND_Z:
                    if (leftOp && !rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_NC:
                    if (!rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_C_AND_NZ:
                    if (!leftOp && rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_NZ:
                    if (!leftOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_C_NE_Z:
                    if (leftOp != rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_NC_OR_NZ:
                    if (!leftOp || !rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_C_AND_Z:
                    if (leftOp && rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_C_EQ_Z:
                    if (leftOp == rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_Z:
                    if (leftOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_NC_OR_Z:
                    if (leftOp || !rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_C:
                    if (rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_C_OR_NZ:
                    if (!leftOp || rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_Z_OR_C:
                    if (leftOp || rightOp) return true;
                    break;
                case Assembly.ConditionCodes.IF_ALWAYS:
                    return true;
            }
            return false;
        }

        /// <summary>Adjust the timing to wait the finish of pending Hub
        /// operation for this %Cog.</summary>
        /// @version v22.05.04 - Method name changed to clarify meaning of it.
        public virtual void RequestHubOperation()
        {
            if (State != CogRunState.WaitLoadProgram)
                return;
            _cogMemory[StateCount++] = CpuHost.DirectReadLong(ProgramAddress);
            ProgramAddress += 4;
            if (StateCount != 0x1F0)
                return;
            StateCount = 0;
            Boot();
        }

        /// <summary>Detach video hooks of this %Cog.</summary>
        public void DetachVideoHooks()
        {
            // Detach the video hook
            _pllGroup.Destroy();
            // Detach the aural hook
            _videoGenerator.DetachAural();
        }

        /// <summary>Set Clock of both frequency generators.</summary>
        /// <param name="freq">Frequency to set.</param>
        public void SetClock(uint freq)
        {
            _freqAGenerator.SetClock(freq);
            _freqBGenerator.SetClock(freq);
        }

        /// <summary>Execute a clock step in this cog.</summary>
        /// <returns></returns>
        /// @version v22.05.04 - Adapted to follow the new signature of
        /// FreqGenerator.Tick().
        public bool Step()
        {
            bool result = DoInstruction();
            // Run our frequency counters
            _freqAGenerator.Tick();
            _freqBGenerator.Tick();
            result = result && FrameFlag <= FrameBreak;    // false - we hit a breakpoint
            return result;
        }

        /// <summary>Execute a step to process an instruction in this cog.</summary>
        public void StepInstruction()
        {
            int i = 0x2000;    // Maximum of 8k clocks (covers load instruction)
            do
            {
                CpuHost.Step();
            }
            while ( State != CogRunState.ExecInterpreter &&
                    State != CogRunState.StateExecute &&
                    --i > 0 &&
                    FrameFlag <= FrameBreak);
        }

        /// <summary>Read a long from cog RAM at specified value.</summary>
        /// <param name="address">Address to Read.</param>
        /// <returns>Long value.</returns>
        /// @version v22.09.01 - Modified to use mask in a single place.
        public uint ReadLong(uint address)
        {
            address &= MaskCogMemory;
            // values using Assembly.RegisterAddress enum, instead of direct hex values
            switch ((Assembly.RegisterAddress)address)
            {
                case Assembly.RegisterAddress.CNT:
                    return CpuHost.Counter;
                case Assembly.RegisterAddress.INA:
                    return CpuHost.RegisterINA;
                case Assembly.RegisterAddress.INB:
                    return CpuHost.RegisterINB;
                case Assembly.RegisterAddress.CNTA:
                    return _freqAGenerator.RegisterCTR;
                case Assembly.RegisterAddress.CNTB:
                    return _freqBGenerator.RegisterCTR;
                case Assembly.RegisterAddress.FRQA:
                    return _freqAGenerator.RegisterFRQ;
                case Assembly.RegisterAddress.FRQB:
                    return _freqBGenerator.RegisterFRQ;
                case Assembly.RegisterAddress.PHSA:
                    return _freqAGenerator.RegisterPHS;
                case Assembly.RegisterAddress.PHSB:
                    return _freqBGenerator.RegisterPHS;
                case Assembly.RegisterAddress.VCFG:
                    return _videoGenerator.RegisterCFG;
                case Assembly.RegisterAddress.VSCL:
                    return _videoGenerator.RegisterSCL;
                default:
                    return _cogMemory[address];
            }
        }

        /// <summary>Write cog RAM at specified value.</summary>
        /// <remarks>This method take care of special cog address that in
        /// this class aren't write in memory array _cogMemory.</remarks>
        /// <param name="address">Address to write.</param>
        /// <param name="data">Data to write.</param>
        /// @note PAR address is a special case, because unless %Propeller Manual V1.2
        /// specifications says it is a read-only register, there are evidence that in reality it
        /// is writable as explains
        /// <a href="https://forums.parallax.com/discussion/comment/839684/#Comment_839684">
        /// Forum thread "PASM simulator / debugger?</a>.
        /// They show that some parallax video drivers in PASM changes the PAR register,
        /// and GEAR didn't emulate that.
        private protected void WriteLong(uint address, uint data)
        {
            // values using Assembly.RegisterAddress enum, instead of direct hex values
            switch ((Assembly.RegisterAddress)(address & MaskCogMemory))
            {
                // Read only registers
                // case Assembly.RegisterAddress.PAR: // PAR register changed to writable
                case Assembly.RegisterAddress.CNT:
                case Assembly.RegisterAddress.INA:
                case Assembly.RegisterAddress.INB:
                    return;
                case Assembly.RegisterAddress.CNTA:
                    _freqAGenerator.RegisterCTR = data;
                    break;
                case Assembly.RegisterAddress.CNTB:
                    _freqBGenerator.RegisterCTR = data;
                    break;
                case Assembly.RegisterAddress.FRQA:
                    _freqAGenerator.RegisterFRQ = data;
                    break;
                case Assembly.RegisterAddress.FRQB:
                    _freqBGenerator.RegisterFRQ = data;
                    break;
                case Assembly.RegisterAddress.PHSA:
                    _freqAGenerator.RegisterPHS = data;
                    break;
                case Assembly.RegisterAddress.PHSB:
                    _freqBGenerator.RegisterPHS = data;
                    break;
                case Assembly.RegisterAddress.VCFG:
                    _videoGenerator.RegisterCFG = data;
                    break;
                case Assembly.RegisterAddress.VSCL:
                    _videoGenerator.RegisterSCL = data;
                    break;
                default:
                    _cogMemory[address & MaskCogMemory] = data;
                    return;
            }
        }

        /// @brief Execute a instruction in this cog.
        /// @returns TRUE if it is time to trigger a breakpoint, or FALSE if not.
        public abstract bool DoInstruction();

        /// <summary>Setup the cog to a initial state after boot it.</summary>
        /// @version v22.05.04 - Changed visibility of method.
        private protected abstract void Boot();

        /// <summary></summary>
        /// <param name="colors"></param>
        /// <param name="pixels"></param>
        /// @pullreq{18} Method added.
        public abstract void GetVideoData(out uint colors, out uint pixels);
    }
}
