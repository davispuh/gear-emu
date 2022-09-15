/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * LogicRow.cs
 * Abstract base class for a LogicView channel
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

using System.Drawing;

namespace Gear.GUI.LogicProbe
{
    /// <summary>Abstract base class for a LogicView channel.</summary>
    public abstract class LogicRow
    {
        /// <summary>Name of channel.</summary>
        public abstract string Name { get; }

        /// <summary></summary>
        public abstract double MinTime { get; }

        /// <summary>Height this channel on screen.</summary>
        public abstract int Height { get; }

        /// <summary></summary>
        /// <param name="graph"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="width"></param>
        /// <param name="minTime"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public abstract int Draw(Graphics graph, int top, float left, float width, double minTime, double scale);

        /// <summary></summary>
        public abstract void Click();

        /// <summary>Clear previous samples.</summary>
        public abstract void Reset();
    }
}
