/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * PluginBase.cs
 * Abstract superclass for emulator plugins
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Gear.EmulationCore;

namespace Gear.PluginSupport
{
    public class PluginBase : UserControl
    {
        public virtual string Title { get { return "Bus Module"; } }
        public virtual void PresentChip(Propeller host) { }
        public virtual void OnReset() { }
        public virtual void OnClock(double time) { }
        public virtual void OnPinChange(double time, PinState[] pins) { }
        public virtual void Repaint(bool force) { }
        public virtual Boolean AllowHotKeys { get { return true; } }
        public virtual Boolean IsClosable { get { return true; } }
    }
}
