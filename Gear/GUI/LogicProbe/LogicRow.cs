/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * LogicRow.cs
 * Abstract baseclass for a LogicView channel
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
using System.Text;
using System.Drawing;

using Gear.EmulationCore;

namespace Gear.GUI.LogicProbe
{
    public abstract class LogicRow
    {
        public abstract string Name { get; }
        public abstract double MinTime { get; }
        public abstract int Height { get; }
        public abstract int Draw(Graphics g, int top, float left, float width, double minTime, double scale);
        public abstract void Click();
        public abstract void Reset(); // ASB: new method to clear previus samples
    }
}
