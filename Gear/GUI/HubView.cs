/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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
using Gear.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief GUI Control to show Hub status
    /// @version v20.09.01 - Modified to use custom format.
    public partial class HubView : UserControl
    {
        /// @brief Reference to propeller cpu instance.
        private PropellerCPU m_Host;

        /// @brief Current Culture to modify its Number format.
        /// @since @version v20.09.01 - Added.
        private readonly CultureInfo currentCultureMod = 
            (CultureInfo)CultureInfo.CurrentCulture.Clone();

        /// @brief Storage for frequency format.
        /// @since @version v20.09.01 - Added.
        private NumberFormatEnum _reqFormatValue;

        /// @brief Frequency format to be displayed.
        /// @since @version v20.09.01 - Added.
        private NumberFormatEnum FreqFormatValue
        {
            get => _reqFormatValue;
            set
            {
                _reqFormatValue = value;
                currentCultureMod.NumberFormat = 
                    NumberFormatEnumExtension.GetFormatInfo(_reqFormatValue);
            }
        }

        /// @brief Time format to be displayed.
        /// @since v20.09.01 - Added.
        public TimeUnitsEnum TimeUnit { get; set; }

        /// @brief Property to set the %Propeller %Host.
        public PropellerCPU Host
        {
            set
            {
                m_Host = value;
                DataChanged();
            }
        }

        /// @brief Default constructor
        /// @version v20.09.01 - Modified to retrieve last formats.
        public HubView()
        {
            InitializeComponent();
            timeUnitSelector.SyncValues();
            //Assign delegates for formatting text of timeUnitSelector
            var textFormats = new DelegatesPerTimeUnitsList(
                timeUnitSelector.ExcludedUnits,
                new SortedList<TimeUnitsEnum, FormatToTextDelegate>()
                {
                    {TimeUnitsEnum.ns, StandardTimeFormatText},
                    {TimeUnitsEnum.us, StandardTimeFormatText},
                    {TimeUnitsEnum.ms, StandardTimeFormatText},
                    {TimeUnitsEnum.s,  StandardTimeFormatText},
                    {TimeUnitsEnum.min_s, MinutesTimeFormatText}
                }
            );
            timeUnitSelector.AssignTextFormats(textFormats);
            //retrieve saved settings
            UpdateFreqFormat();
            UpdateHubTimeUnit();
            //update depending UI names
            UpdateFreqToolTips();
            UpdateTimeText();
        }

        /// @brief Update the value of FreqFormat from default setting.
        /// @since v20.09.01 - Added.
        public void UpdateFreqFormat()
        {
            FreqFormatValue = Properties.Settings.Default.FreqFormat;
        }

        public void UpdateHubTimeUnit()
        {
            TimeUnit = Properties.Settings.Default.HubTimeUnit;
            //set unit combobox selected item
            timeUnitSelector.TimeUnitSelected = TimeUnit;
        }

        /// @brief Update Counter and Frequency labels with Monospace fonf.
        /// @since v20.09.01 - Added.
        public void SetFontSpecialLabels()
        {
            Font MonoFont = new Font(FontFamily.GenericMonospace, 8.25F, 
                FontStyle.Regular, GraphicsUnit.Point);
            coreFrequency.Font = MonoFont;
            xtalFrequency.Font = MonoFont;
            clockMode.Font = MonoFont;
            systemCounter.Font = MonoFont;
            elapsedTime.Font = MonoFont;
        }

        /// @brief Update screen data on event.
        /// @version v20.09.01 - Modified to use custom format.
        public void DataChanged()
        {
            if (m_Host == null)
                return;

            pinDIR.Value = m_Host.DIR;
            pinIN.Value = m_Host.IN;
            pinFloating.Value = m_Host.Floating;
            pinLocksFree.Value = m_Host.LocksFree;
            pinLocks.Value = m_Host.Locks;

            UpdateCounterFreqTexts();
            UpdateTimeText();
            clockMode.Text = m_Host.Clock;

            ringMeter.Value = m_Host.Ring;
        }

        /// @brief Update Counter and Frequency labels with current format.
        /// @since v20.09.01 - Added.
        public void UpdateCounterFreqTexts()
        {
            systemCounter.Text = FreqFormatText(m_Host.Counter);
            coreFrequency.Text = FreqFormatText(m_Host.CoreFrequency);
            xtalFrequency.Text = FreqFormatText(m_Host.XtalFrequency);
        }

        /// @brief Update Time labels with current format and unit.
        /// @since v20.09.01 - Added.
        public void UpdateTimeText()
        {
            if (m_Host != null)
                elapsedTime.Text = 
                    timeUnitSelector.GetFormatedText(m_Host.EmulatorTime);
        }

        /// @brief Format the value to string, considering the value 
        ///  of FreqFormatValue.
        /// @param val Value to format to string.
        /// @returns The text formatted.
        /// @since v20.09.01 - Added.
        private string FreqFormatText(uint val)
        {
            return string.Format(currentCultureMod, "{0,17:#,##0}", val);
        }


        /// @brief Format the value to string, for all time units except 
        ///  Minutes (TimeUnitsEnum.min_s).
        /// @details Implements Gear.Utils.FormatToTextDelegate delegate.
        /// @param unit Time unit to use.
        /// @param val Value to format to string.
        /// @returns The formatted text.
        /// @since v20.09.01 - Added.
        private string StandardTimeFormatText(TimeUnitsEnum unit, double val)
        {
            double factor = ((unit <= TimeUnitsEnum.s) ?
                timeUnitSelector.FactorSelected : 1.0);
            string decimalsSymbols = new string('0', 3 * (int)unit - 2);
            string numFormat = $"{{0,17:#,##0.{decimalsSymbols}}}";
            double value = (timeUnitSelector.IsMultiplyFactor) ?
                val * factor : val / factor;
            return string.Format(currentCultureMod, numFormat, value);
        }

        /// @brief Format the value to string, only for Minutes (TimeUnitsEnum.min_s).
        /// @details Implements Gear.Utils.FormatToTextDelegate delegate.
        /// @param unit Time unit to use.
        /// @param val Value to format to string.
        /// @returns The formatted text.
        /// @since v20.09.01 - Added.
        private string MinutesTimeFormatText(TimeUnitsEnum unit, double val)
        {
            if (!timeUnitSelector.IsMultiplyFactor)
                return string.Format(currentCultureMod,
                    "{0,3:#0}:{1,13:00.0000000000}",
                    Math.Floor(val / timeUnitSelector.FactorSelected), 
                        val % timeUnitSelector.FactorSelected);
            else
                return string.Format(currentCultureMod,
                    "{0,3:#0}:{1,13:00.0000000000}",
                    Math.Floor(val * timeUnitSelector.FactorSelected), 
                        val % (1.0 / timeUnitSelector.FactorSelected));
        }

        /// @brief Update frequency labels tool tips.
        /// @param val Format to use for frequency labels.
        /// @since v20.09.01 - Added.
        private void UpdateFreqToolTips()
        {
            string txt = string.Format("\r\n(Click to change Format from [{0}])", FreqFormatValue);
            toolTip1.SetToolTip(this.systemCounter, "System Counter Value" + txt);
            toolTip1.SetToolTip(this.xtalFrequency, "Crystal Frequency" + txt);
            toolTip1.SetToolTip(this.coreFrequency, "Core Frequency" + txt);
        }

        /// @brief Change the frequencies labels format, remembering the user setting.
        /// @param sender
        /// @param e
        /// @since v20.09.01 - Added.
        private void FrequencyLabels_Click(object sender, EventArgs e)
        {
            if (FreqFormatValue < NumberFormatEnum.GetValues(typeof(NumberFormatEnum)).Cast<NumberFormatEnum>().Max())
                ++FreqFormatValue;            else
                FreqFormatValue = NumberFormatEnum.GetValues(typeof(NumberFormatEnum)).Cast<NumberFormatEnum>().Min();
            UpdateFreqToolTips();
            DataChanged();
            //remember the setting
            Properties.Settings.Default.FreqFormat = FreqFormatValue;
            Properties.Settings.Default.Save();
        }

        /// @brief Change the time unit, remembering the user setting.
        /// @param sender
        /// @param e
        /// @since v20.09.01 - Added.
        private void TimeUnitSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.DesignMode)
            {
                TimeUnit = timeUnitSelector.TimeUnitSelected;
                UpdateTimeText();
                //remember the setting
                Properties.Settings.Default.HubTimeUnit = TimeUnit;
                Properties.Settings.Default.Save();
            }
        }

        /// @brief Change the time unit, remembering the user setting.
        /// @param sender
        /// @param e
        /// @since v20.09.01 - Added.
        private void ElapsedTime_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    timeUnitSelector.SelectNext();
                    break;
                case MouseButtons.Right:
                    timeUnitSelector.SelectPrev();
                    break;
                default:
                    break;
            }
        }

    } //end class HubView 

} //end namespace Gear.GUI

