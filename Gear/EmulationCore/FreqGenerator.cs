/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * FreqGenerator.cs
 * Frequency Generator Circuit emulation
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

using Gear.Propeller;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace Gear.EmulationCore
{
    /// <summary>Counter modes of operation.</summary>
    /// @remark Source: Table 6 - Counter Modes (CTRMODE Field Values), %Propeller
    /// P8X32A Datasheet V1.4.0.
    /// @version v14.7.3 - Missing logic modes implemented (LOGIC_NEVER ... LOGIC_ALWAYS)
    public enum CounterMode : byte
    {
        /// <summary>Counter disabled (off)</summary>
        DISABLED = 0b00000,

        /// <summary>%PLL internal (video mode)</summary>
        PLL_INTERNAL = 0b00001,
        /// <summary>%PLL single-ended</summary>
        PLL_SINGLE_ENDED = 0b00010,
        /// <summary>%PLL differential</summary>
        PLL_DIFFERENTIAL = 0b00011,

        /// <summary>NCO single-ended</summary>
        NCO_SINGLE_ENDED = 0b00100,
        /// <summary>NCO differential</summary>
        NCO_DIFFERENTIAL = 0b00101,

        /// <summary>DUTY single-ended</summary>
        DUTY_SINGLE_ENDED = 0b00110,
        /// <summary>DUTY differential</summary>
        DUTY_DIFFERENTIAL = 0b00111,

        /// <summary>POS detector</summary>
        POS_DETECTOR = 0b01000,
        /// <summary>POS detector with feedback</summary>
        POS_DETECTOR_FEEDBACK = 0b01001,
        /// <summary>POSEDGE detector</summary>
        POSEDGE_DETECTOR = 0b01010,
        /// <summary>POSEDGE detector w/ feedback</summary>
        POSEDGE_DETECTOR_FEEDBACK = 0b01011,

        /// <summary>NEG detector</summary>
        NEG_DETECTOR = 0b01100,
        /// <summary>NEG detector with feedback</summary>
        NEG_DETECTOR_FEEDBACK = 0b01101,
        /// <summary>NEGEDGE detector</summary>
        NEGEDGE_DETECTOR = 0b01110,
        /// <summary>NEGEDGE detector w/ feedback</summary>
        NEGEDGE_DETECTOR_FEEDBACK = 0b01111,

        // Logic modes implemented on gear v14.7.3
        /// <summary>LOGIC never</summary>
        LOGIC_NEVER = 0b10000,
        /// <summary>LOGIC !A & !B</summary>
        LOGIC_NOTA_AND_NOTB = 0b10001,
        /// <summary>LOGIC A & !B</summary>
        LOGIC_A_AND_NOTB = 0b10010,
        /// <summary>LOGIC !B</summary>
        LOGIC_NOTB = 0b10011,
        /// <summary>LOGIC !A & B</summary>
        LOGIC_NOTA_AND_B = 0b10100,
        /// <summary>LOGIC !A</summary>
        LOGIC_NOTA = 0b10101,
        /// <summary>LOGIC A != B</summary>
        LOGIC_A_DIFF_B = 0b10110,
        /// <summary>LOGIC !A, !B</summary>
        LOGIC_NOTA_OR_NOTB = 0b10111,
        /// <summary>LOGIC A & B</summary>
        LOGIC_A_AND_B = 0b11000,
        /// <summary>LOGIC A == B</summary>
        LOGIC_A_EQ_B = 0b11001,
        /// <summary>LOGIC A</summary>
        LOGIC_A = 0b11010,
        /// <summary>LOGIC A, !B</summary>
        LOGIC_A_OR_NOTB = 0b11011,
        /// <summary>LOGIC B</summary>
        LOGIC_B = 0b11100,
        /// <summary>LOGIC !A, B</summary>
        LOGIC_NOTA_OR_B = 0b11101,
        /// <summary>LOGIC A, B</summary>
        LOGIC_A_OR_B = 0b11110,
        /// <summary>LOGIC always</summary>s
        LOGIC_ALWAYS = 0b11111
    }

    /// <summary>Frequency Generator Circuit emulation.</summary>
    public class FreqGenerator
    {
        /// <summary>Reference to the PropellerCPU instance where this
        /// object belongs.</summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private readonly PropellerCPU _cpuHost;

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _control;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _frequency;

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private CounterMode _ctrMode;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private ulong _pinAMask;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private ulong _pinBMask;

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private bool _pinA;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private bool _pinADelayed;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private bool _pinB;

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private bool _outA;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private bool _outB;

        /// <summary>Identity of this frequency generator: A or B.</summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private readonly bool _isFreqGeneratorA;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _divider;

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private readonly PLLGroup _pllGroup;

        /// <summary></summary>
        /// @version v22.05.04 - Changed property name to clarify meaning of it.
        public uint RegisterCTR
        {
            get => _control;
            set
            {
                if (_control == value)
                    return;
                _control = value;
                _divider = (uint)128 >> (int)((value >> 23) & 0x7);
                // The generators and outputs are
                _outA = _outB = false;
                _pinAMask = (uint)1 << (int)(value & 0x3F);
                _pinBMask = (uint)1 << (int)((value >> 9) & 0x3F);
                _ctrMode = (CounterMode)((value & 0x7c000000) >> 26);
                // Setup PLL
                switch (_ctrMode)
                {
                    case CounterMode.PLL_INTERNAL:
                        _pllGroup.DrivePins(_isFreqGeneratorA, 0, 0);
                        _pllGroup.Feed(_isFreqGeneratorA, RegisterFRQ / (double)0x100000000 * 16.0 / _divider);
                        break;
                    case CounterMode.PLL_SINGLE_ENDED:
                        _pllGroup.DrivePins(_isFreqGeneratorA, _pinAMask, 0);
                        _pllGroup.Feed(_isFreqGeneratorA, RegisterFRQ / (double)0x100000000 * 16.0 / _divider);
                        break;
                    case CounterMode.PLL_DIFFERENTIAL:
                        _pllGroup.DrivePins(_isFreqGeneratorA, _pinAMask, _pinBMask);
                        _pllGroup.Feed(_isFreqGeneratorA, RegisterFRQ / (double)0x100000000 * 16.0 / _divider);
                        break;
                    default:
                        _pllGroup.Disable(_isFreqGeneratorA);
                        break;
                }
            }
        }

        /// <summary></summary>
        /// @version v22.05.04 - Changed property name to clarify meaning of it.
        public uint RegisterFRQ
        {
            get => _frequency;
            set
            {
                if (_frequency == value)
                    return;
                _frequency = value;
                switch (_ctrMode)
                {
                    case CounterMode.PLL_INTERNAL:
                    case CounterMode.PLL_SINGLE_ENDED:
                    case CounterMode.PLL_DIFFERENTIAL:
                    case CounterMode.LOGIC_NEVER:       // do nothing for LOGIC_NEVER
                        // This is a special de-jitter function
                        // The edge-sensitive system resulted in unstable
                        // output frequencies
                        _pllGroup.Feed(_isFreqGeneratorA, RegisterFRQ / (double)0x100000000 * 16.0 / _divider);
                        break;
                    default:
                        _pllGroup.Disable(_isFreqGeneratorA);
                        break;
                }
            }
        }

        /// <summary></summary>
        /// @version v22.05.04 - Changed property name to clarify meaning of it.
        public uint RegisterPHS { get; set; }

        /// <summary></summary>
        public ulong Output =>
            (_outA ? _pinAMask : 0) |
            (_outB ? _pinBMask : 0) |
            _pllGroup.Pins;

        /// <summary>Default Constructor.</summary>
        /// <param name="cpuHost">PropellerCPU instance where this object belongs.</param>
        /// <param name="pllGroup"></param>
        /// <param name="isFreqGeneratorA">Identity of this frequency generator: A or B.</param>
        /// @version v22.05.04 - Parameter names changed to use the same
        /// convention for a PropellerCPU instance reference and to clarify
        /// meaning of them.
        public FreqGenerator(PropellerCPU cpuHost, PLLGroup pllGroup, bool isFreqGeneratorA)
        {
            _cpuHost = cpuHost;
            _outA = false;
            _outB = false;
            _isFreqGeneratorA = isFreqGeneratorA;
            _pllGroup = pllGroup;
        }

        /// <summary></summary>
        /// <param name="clock"></param>
        public void SetClock(uint clock)
        {
            _pllGroup.SetBaseFrequency(clock);
        }

        /// <summary></summary>
        /// @version v22.05.04 - Invert sense of return value
        /// on ConditionCompare(), to be intuitive and changed method signature
        /// to use the value from register IN from Emulator.
        public void Tick()
        {
            switch (_ctrMode)
            {
                case CounterMode.DISABLED:
                case CounterMode.PLL_INTERNAL:
                case CounterMode.PLL_SINGLE_ENDED:
                case CounterMode.PLL_DIFFERENTIAL:
                    break;
                case CounterMode.NCO_SINGLE_ENDED:
                    RegisterPHS += RegisterFRQ;
                    _outA = (RegisterPHS & 0x80000000) != 0;
                    break;
                case CounterMode.NCO_DIFFERENTIAL:
                    RegisterPHS += RegisterFRQ;
                    _outA = (RegisterPHS & 0x80000000) != 0;
                    _outB = !_outA;
                    break;
                case CounterMode.DUTY_SINGLE_ENDED:
                {
                    long newVal = (long)RegisterPHS + RegisterFRQ;
                    RegisterPHS = (uint)newVal;
                    _outA = newVal > 0xFFFFFFFF;
                }
                    break;
                case CounterMode.DUTY_DIFFERENTIAL:
                {
                    long newVal = (long)RegisterPHS + RegisterFRQ;
                    RegisterPHS = (uint)newVal;
                    _outA = newVal > 0xFFFFFFFF;
                    _outB = !_outA;
                }
                    break;
                case CounterMode.POS_DETECTOR:
                    if (_pinA)
                        RegisterPHS += RegisterFRQ;
                    break;
                case CounterMode.POS_DETECTOR_FEEDBACK:
                    if (_pinA)
                        RegisterPHS += RegisterFRQ;
                    _outB = !_pinA;
                    break;
                case CounterMode.POSEDGE_DETECTOR:
                    if (_pinA && !_pinADelayed)
                        RegisterPHS += RegisterFRQ;
                    break;
                case CounterMode.POSEDGE_DETECTOR_FEEDBACK:
                    if (_pinA && !_pinADelayed)
                        RegisterPHS += RegisterFRQ;
                    _outB = !_pinA;
                    break;
                case CounterMode.NEG_DETECTOR:
                    if (!_pinA)
                        RegisterPHS += RegisterFRQ;
                    break;
                case CounterMode.NEG_DETECTOR_FEEDBACK:
                    if (!_pinA)
                        RegisterPHS += RegisterFRQ;
                    _outB = !_pinA;
                    break;
                case CounterMode.NEGEDGE_DETECTOR:
                    if (!_pinA && _pinADelayed)
                        RegisterPHS += RegisterFRQ;
                    break;
                case CounterMode.NEGEDGE_DETECTOR_FEEDBACK:
                    if (!_pinA && _pinADelayed)
                        RegisterPHS += RegisterFRQ;
                    _outB = !_pinA;
                    break;
                case CounterMode.LOGIC_NEVER:
                case CounterMode.LOGIC_NOTA_AND_NOTB:
                case CounterMode.LOGIC_A_AND_NOTB:
                case CounterMode.LOGIC_NOTB:
                case CounterMode.LOGIC_NOTA_AND_B:
                case CounterMode.LOGIC_NOTA:
                case CounterMode.LOGIC_A_DIFF_B:
                case CounterMode.LOGIC_NOTA_OR_NOTB:
                case CounterMode.LOGIC_A_AND_B:
                case CounterMode.LOGIC_A_EQ_B:
                case CounterMode.LOGIC_A:
                case CounterMode.LOGIC_A_OR_NOTB:
                case CounterMode.LOGIC_B:
                case CounterMode.LOGIC_NOTA_OR_B:
                case CounterMode.LOGIC_A_OR_B:
                case CounterMode.LOGIC_ALWAYS:
                default:
                    // changed to intuitive return value on ConditionCompare(.)
                    if (Cog.ConditionCompare((Assembly.ConditionCodes)((int)_ctrMode - 16), _pinA, _pinB))
                        RegisterPHS += RegisterFRQ;
                    break;
            }
            // Cycle in our previous pin states
            _pinADelayed = _pinA;   // Delay by 2
            _pinA = (_cpuHost.RegisterIN & _pinAMask) != 0;
            _pinB = (_cpuHost.RegisterIN & _pinBMask) != 0;
        }
    }
}
