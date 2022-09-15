/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * FloatedWindow.cs
 * Window container for a floated plugin
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
using System.Windows.Forms;

namespace Gear.GUI
{
    /// <summary>Window container for a floated plugin.</summary>
    public partial class FloatedWindow : Form
    {
        /// <summary>Reference to Emulator form.</summary>
        private readonly Emulator _sourceEmulator;
        /// <summary>Control to handle floating.</summary>
        /// @version v22.06.01 - Added.
        private Control _control;

        /// <summary>Default constructor.</summary>
        /// <param name="relatedEmulator"></param>
        /// <param name="controlToFloat"></param>
        /// @version v22.06.01 - Changed signature to add parameter
        /// to reference the Control to keep floating.
        public FloatedWindow(Emulator relatedEmulator, Control controlToFloat)
        {
            _sourceEmulator = relatedEmulator;
            _control = controlToFloat;
            InitializeComponent();
        }

        /// <summary>When close the floated window, restore the control
        /// to the Emulator owns it.</summary>
        /// <param name="e">Event data arguments.</param>
        protected override void OnClosed(EventArgs e)
        {
            _sourceEmulator.UnFloatCtrl(_control);
            _control = null;
            base.OnClosed(e);
        }
    }
}
