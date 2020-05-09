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

using Gear.EmulationCore;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief Available Frequency Formats.
    /// @since @version 20.05.00 - Added.
    public enum FreqFormatEnum
    {
        /// @brief No format (old default).
        None = 0,
        /// @brief Format given from system default.
        System_Default,
        /// @brief Using '_' as separator.
        Parallax_SPIN
    }

    /// @brief GUI Control to show Hub status
    /// @version 20.05.00 - Modified to use custom format.
    public partial class HubView : UserControl
    {
        private PropellerCPU m_Host;

        /// @brief Frequency format to display.
        /// @since @version 20.05.00 - Added.
        private FreqFormatEnum freqFormatValue;

        /// @brief Defaul constructor
        /// @version 20.05.00 - Modified to retrieve last format.
        public HubView()
        {
            InitializeComponent();
            freqFormatValue = Properties.Settings.Default.FreqFormat;
        }
        /// @brief Property to set the %Propeller %Host.
        public PropellerCPU Host
        {
            set
            {
                m_Host = value;
                DataChanged();
            }
        }

        /// @brief Update screen data on event.
        /// @version 20.05.00 - Modified to use custom format.
        public void DataChanged()
        {
            if (m_Host == null)
                return;

            pinDIR.Value = m_Host.DIR;
            pinIN.Value = m_Host.IN;
            pinFloating.Value = m_Host.Floating;
            pinLocksFree.Value = m_Host.LocksFree;
            pinLocks.Value = m_Host.Locks;

            systemCounter.Text = FreqFormatText(m_Host.Counter);
            coreFrequency.Text = FreqFormatText(m_Host.CoreFrequency) + " hz";
            xtalFrequency.Text = FreqFormatText(m_Host.XtalFrequency) + " hz";
            clockMode.Text = m_Host.Clock;

            ringMeter.Value = m_Host.Ring;
        }

        /// @brief Format the value to string, considering the value of freqFormatValue.
        /// @param val Value to format to string.
        /// @returns The text formatted.
        /// @since 20.05.00 - Added.
        private string FreqFormatText(uint val)
        {
            string retVal = string.Empty;
            switch (freqFormatValue)
            {
                case FreqFormatEnum.None:
                    retVal = val.ToString();
                    break;
                case FreqFormatEnum.System_Default:
                    retVal = val.ToString("N0", CultureInfo.InvariantCulture);
                    break;
                case FreqFormatEnum.Parallax_SPIN:
                    {
                        string formatTxt = string.Empty;
                        uint digits = (val == 0) ? 1 : ((uint)Math.Floor(Math.Log10(val))) + 1;
                        for (uint i = digits; i > 0; i--)
                        {
                            if (i > 1)
                                formatTxt += "#";
                            else
                                formatTxt += "0";
                            if ((i > 3) & ((i % 3) == 1))
                                formatTxt += "_";
                        }
                        retVal = val.ToString(formatTxt, CultureInfo.InvariantCulture);
                    }
                    break;
            }
            return retVal;
        }

        /// @brief Update frequency labels tool tips.
        /// @param val Format to use for frequency labels.
        /// @since 20.05.00 - Added.
        private void UpdateFreqToolTips()
        {
            string txt = string.Format(" (Click to change Format from [{0}])", freqFormatValue);
            toolTip1.SetToolTip(this.systemCounter, "System Counter Value" + txt);
            toolTip1.SetToolTip(this.xtalFrequency, "Crystal Frequency" + txt);
            toolTip1.SetToolTip(this.coreFrequency, "Core Frequency" + txt);
        }

        /// @brief Update the frequencies labels tooltips.
        /// @param sender
        /// @param e
        /// @since 20.05.00 - Added.
        private void Label_MouseMove(object sender, EventArgs e)
        {
            UpdateFreqToolTips();
        }

        /// @brief Change the frequencies labels format, remembering the user setting.
        /// @param sender
        /// @param e
        /// @since 20.05.00 - Added.
        private void FrequencyLabels_Click(object sender, EventArgs e)
        {
            if (freqFormatValue < FreqFormatEnum.Parallax_SPIN)
                ++freqFormatValue;
            else
                freqFormatValue = FreqFormatEnum.None;
            DataChanged();
            //remember the setting
            Properties.Settings.Default.FreqFormat = freqFormatValue;
            Properties.Settings.Default.Save();
        }

    }
}
