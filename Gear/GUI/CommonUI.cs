/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * CommonUI.cs
 * Common objects for User Interface
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

namespace Gear.GUI
{
    /// <summary>Interface to request full re-paint of a control.</summary>
    /// @version v22.06.02 - Moved to a separated file.
    public interface IRequestRepaintable
    {
        /// <summary>Determine if this control is visible on screen.</summary>
        /// <returns>TRUE if all the control area is visible on screen,
        /// else FALSE.</returns>
        bool IsThisFullyVisible();
        /// <summary>Request full paint on next Repaint event.</summary>
        void RequestFullOnNextRepaint();
    }
}
