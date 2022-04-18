/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * PLL.cs
 * Object class providing the logic to drive a PLL clock signal
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
    // @brief provides the logic to drive a PLL clock signal.
    public class PLL
    {
        // --- PLL Output settings ---

        private double FixedMultiplier;
        private uint CyclesPerSecond;

        private bool OutputData;

        private ulong PinA;
        private ulong PinB;

        // --- PLL Signal generation variables ---

        private double Frequency;
        private double ClockLeft;
        private double SecondsPerCycle;

        public ulong Pins
        {
            get
            {
                return (OutputData ? PinA : PinB);
            }
        }


        public double TimeUntilClock
        {
            get
            {
                return ClockLeft;
            }
        }

        public bool Output
        {
            get
            {
                return OutputData;
            }
        }

        public void SetBaseFrequency(uint cps)
        {
            CyclesPerSecond = cps;

            Feed(FixedMultiplier);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="multiplier"></param>
        /// @version v22.03.02 - Bugfix the max frequency to 128Mhz according to
        /// <c>%Propeller Datasheet v1.4</c>, section "4.9. Cog Counters".
        public void Feed(double multiplier)
        {
            FixedMultiplier = multiplier;

            double targetFrequency = (double)CyclesPerSecond * multiplier;

            // The target frequency must fall between 500KHz and 120MHz
            if (targetFrequency > 128000000 || targetFrequency < 500000)
                Disable();
            else
                SetFrequency(targetFrequency * 2);
        }

        public void DrivePins(ulong pinA, ulong pinB)
        {
            PinA = pinA;
            PinB = pinB;
        }

        public void Disable()
        {
            DrivePins(0, 0);

            SecondsPerCycle = 0;
            ClockLeft = 1;
        }

        public void SetFrequency(double frequency)
        {
            Frequency = frequency;

            SecondsPerCycle = 1.0 / frequency;

            if (ClockLeft > SecondsPerCycle)
                ClockLeft = SecondsPerCycle;
        }

        public bool AdvanceClock(double time)
        {
            if (Frequency <= 0)
                return false;

            ClockLeft -= time;

            if (ClockLeft <= 0)
            {
                ClockLeft += SecondsPerCycle;

                // Toggle output
                OutputData = !OutputData;
                return true;
            }

            return false;
        }

    }
}
