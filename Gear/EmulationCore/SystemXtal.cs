/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
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
    public class SystemXtal : ClockSource
    {
        private double Frequency;
        private double ClockLeft;
        private double SecondsPerCycle;

        public override double TimeUntilClock
        {
            get
            {
                return ClockLeft;
            }
        }

        public SystemXtal()
        {
            Frequency = -1;
            ClockLeft = 1;
        }

        public void Disable()
        {
            SetFrequency(-1);
        }

        public void SetFrequency(double frequency)
        {
            Frequency = frequency;

            if (Frequency > 0)
            {
                SecondsPerCycle = 1.0 / frequency;

                if (ClockLeft > SecondsPerCycle)
                    ClockLeft = SecondsPerCycle;
            }
            else
            {
                ClockLeft = 1;
            }
        }

        public override void AdvanceClock(double time)
        {
            if (Frequency <= 0)
                return;

            ClockLeft -= time;

            if (ClockLeft <= 0)
                ClockLeft += SecondsPerCycle;
        }
    }
}
