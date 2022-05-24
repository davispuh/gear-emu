/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * ClockSource.CS
 * Abstract base class for a clock signal provider
 * --------------------------------------------------------------------------------
 *
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
    /// <summary>Abstract base class for a clock signal provider.</summary>
    public abstract class ClockSource
    {
        /// <summary>How much time (in seconds) left until to tick.</summary>
        public abstract double TimeUntilClock { get; }

        /// <summary></summary>
        /// <param name="time"></param>
        public abstract void AdvanceClock(double time);
    }
}
