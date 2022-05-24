/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * SystemXtal.cs
 * Provides a clock source for the propeller CPU
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

namespace Gear.EmulationCore
{
    /// @brief Provides a clock source for the propeller CPU.
    public class SystemXtal : ClockSource
    {
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private double _frequency;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private double _clockLeft;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private double _secondsPerCycle;

        /// <summary>How much time (in seconds) left until to tick.</summary>
        public override double TimeUntilClock => _clockLeft;

        /// <summary>Default constructor.</summary>
        public SystemXtal()
        {
            _frequency = -1;
            _clockLeft = 1;
            _secondsPerCycle = 0.0;
        }

        /// <summary></summary>
        public void Disable()
        {
            SetFrequency(-1);
        }

        /// <summary></summary>
        /// <param name="frequency"></param>
        public void SetFrequency(double frequency)
        {
            _frequency = frequency;
            if (_frequency > 0)
            {
                _secondsPerCycle = 1.0 / frequency;
                if (_clockLeft > _secondsPerCycle)
                    _clockLeft = _secondsPerCycle;
            }
            else
                _clockLeft = 1;
        }

        /// <summary></summary>
        /// <param name="time"></param>
        public override void AdvanceClock(double time)
        {
            if (_frequency <= 0)
                return;
            _clockLeft -= time;
            if (_clockLeft <= 0)
                _clockLeft += _secondsPerCycle;
        }
    }
}
