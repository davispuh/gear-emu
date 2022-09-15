/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * PLL.cs
 * Provides the logic to drive a PLL clock signal.
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

// ReSharper disable InconsistentNaming

namespace Gear.EmulationCore
{
    /// <summary>provides the logic to drive a %PLL clock signal.</summary>
    public class PLL
    {
        // --- PLL Output settings ---

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private double _fixedMultiplier;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private uint _cyclesPerSecond;

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private ulong _pinA;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private ulong _pinB;

        // --- PLL Signal generation variables ---

        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private double _frequency;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private double _secondsPerCycle;

        /// <summary></summary>
        public ulong Pins =>
            Output ? _pinA : _pinB;

        /// <summary>How much time (in seconds) left until to tick.</summary>
        public double TimeUntilClock { get; private set; }

        /// <summary></summary>
        public bool Output { get; private set; }

        /// <summary>Default Constructor.</summary>
        public PLL()
        {
            _fixedMultiplier = 0.0;
            _cyclesPerSecond = 0;
            _pinA = 0x0;
            _pinB = 0x0;
            _frequency = 0.0;
            _secondsPerCycle = 0.0;
            TimeUntilClock = 0.0;
            Output = false;
        }

        /// <summary></summary>
        /// <param name="cyclesPerSecond"></param>
        /// @version v22.05.04 - Parameter name changed to clarify meaning of it.
        public void SetBaseFrequency(uint cyclesPerSecond)
        {
            _cyclesPerSecond = cyclesPerSecond;
            Feed(_fixedMultiplier);
        }

        /// <summary></summary>
        /// <param name="multiplier"></param>
        /// @version v22.03.02 - Bugfix the max frequency to 128Mhz according to
        /// <c>%Propeller Datasheet v1.4</c>, section "4.9. Cog Counters".
        public void Feed(double multiplier)
        {
            _fixedMultiplier = multiplier;
            double targetFrequency = _cyclesPerSecond * multiplier;
            // The target frequency must fall between 500KHz and 120MHz
            if (targetFrequency > 128000000 || targetFrequency < 500000)
                Disable();
            else
                SetFrequency(targetFrequency * 2);
        }

        /// <summary></summary>
        /// <param name="pinA"></param>
        /// <param name="pinB"></param>
        public void DrivePins(ulong pinA, ulong pinB)
        {
            _pinA = pinA;
            _pinB = pinB;
        }

        /// <summary></summary>
        public void Disable()
        {
            DrivePins(0x0, 0x0);  //clear pins in both banks
            _secondsPerCycle = 0;
            TimeUntilClock = 1.0;
        }

        /// <summary></summary>
        /// <param name="frequency"></param>
        public void SetFrequency(double frequency)
        {
            _frequency = frequency;
            _secondsPerCycle = 1.0 / frequency;
            if (TimeUntilClock > _secondsPerCycle)
                TimeUntilClock = _secondsPerCycle;
        }

        /// <summary></summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool AdvanceClock(double time)
        {
            if (_frequency <= 0.0)
                return false;
            TimeUntilClock -= time;
            if (TimeUntilClock > 0.0)
                return false;
            TimeUntilClock += _secondsPerCycle;
            // Toggle output
            Output = !Output;
            return true;
        }
    }
}
