/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * HubView.cs
 * Propeller \ Hub settings viewer class
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Gear.EmulationCore;

namespace Gear.GUI
{
    public partial class HubView : UserControl
    {
        private Propeller m_Host;

        public HubView()
        {
            InitializeComponent();
        }

        public Propeller Host
        {
            set
            {
                m_Host = value;
                DataChanged();
            }
        }

        public void DataChanged()
        {
            if (m_Host == null)
                return;

            pinDIR.Value = m_Host.DIR;
            pinIN.Value = m_Host.IN;
            pinFloating.Value = m_Host.Floating;
            pinLocksFree.Value = m_Host.LocksFree;
            pinLocks.Value = m_Host.Locks;

            systemCounter.Text = m_Host.Counter.ToString();
            coreFrequency.Text = m_Host.CoreFrequency.ToString() + "hz";
            xtalFrequency.Text = m_Host.XtalFrequency.ToString() + "hz";
            clockMode.Text = m_Host.Clock;

            ringMeter.Value = m_Host.Ring;
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void HubView_Load(object sender, EventArgs e)
        {
        }
    }
}
