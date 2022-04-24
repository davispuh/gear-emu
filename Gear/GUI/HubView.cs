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
        private PropellerCPU _cpuHost;

        /// @brief Current Culture to modify its Number format.
        /// @version v20.09.01 - Added.
        private readonly CultureInfo _currentCultureMod =
            (CultureInfo)CultureInfo.CurrentCulture.Clone();

        /// @brief Storage for frequency format.
        /// @version v20.09.01 - Added.
        private NumberFormatEnum _reqFormatValue;

        /// @brief Frequency format to be displayed.
        /// @version v22.04.02 - Check to update only on changes.
        private NumberFormatEnum FreqFormatValue
        {
            get => _reqFormatValue;
            set
            {
                if (_reqFormatValue == value)
                    return;
                _reqFormatValue = value;
                _currentCultureMod.NumberFormat =
                    NumberFormatEnumExtension.GetFormatInfo(_reqFormatValue);
            }
        }

        /// @brief Time format to be displayed.
        /// @version v20.09.01 - Added.
        public TimeUnitsEnum TimeUnit { get; set; }

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

        /// <summary>Set the %Propeller %Host.</summary>
        /// <exception cref="ArgumentNullException">If host is null.</exception>
        /// @version v22.04.02 - Converted to Set method from Property.
        public void SetHost(PropellerCPU host)
        {
            _cpuHost = host ?? throw new ArgumentNullException(nameof(host));
            DataChanged();
        }

        /// @brief Update the value of FreqFormat from default setting.
        /// @version v20.09.01 - Added.
        public void UpdateFreqFormat()
        {
            FreqFormatValue = Properties.Settings.Default.FreqFormat;
        }

        /// <summary>
        /// Update the value of TimeUnit from default setting.
        /// </summary>
        /// @version v20.09.01 - Added.
        public void UpdateHubTimeUnit()
        {
            TimeUnit = Properties.Settings.Default.HubTimeUnit;
            //set unit combobox selected item
            timeUnitSelector.TimeUnitSelected = TimeUnit;
        }

        /// @brief Update Counter and Frequency labels with Monospace fonf.
        /// @version v20.09.01 - Added.
        public void SetFontSpecialLabels()
        {
            Font monoFont = new Font(FontFamily.GenericMonospace, 8.25F,
                FontStyle.Regular, GraphicsUnit.Point);
            coreFrequency.Font = monoFont;
            xtalFrequency.Font = monoFont;
            clockMode.Font = monoFont;
            systemCounter.Font = monoFont;
            elapsedTime.Font = monoFont;
        }

        /// @brief Update screen data on event.
        /// @version v20.09.01 - Modified to use custom format.
        public void DataChanged()
        {
            if (_cpuHost == null)
                return;

            pinDIR.Value = _cpuHost.DIR;
            pinIN.Value = _cpuHost.IN;
            pinFloating.Value = _cpuHost.Floating;
            pinLocksFree.Value = _cpuHost.LocksFree;
            pinLocks.Value = _cpuHost.Locks;

            UpdateCounterFreqTexts();
            UpdateTimeText();
            clockMode.Text = _cpuHost.Clock;

            ringMeter.Value = _cpuHost.Ring;
        }

        /// @brief Update Counter and Frequency labels with current format.
        /// @version v22.04.02 - Check for non null.
        public void UpdateCounterFreqTexts()
        {
            if (_cpuHost == null)
                return;
            systemCounter.Text = FreqFormatText(_cpuHost.Counter);
            coreFrequency.Text = FreqFormatText(_cpuHost.CoreFrequency);
            xtalFrequency.Text = FreqFormatText(_cpuHost.XtalFrequency);
        }

        /// @brief Update Time labels with current format and unit.
        /// @version v20.09.01 - Added.
        public void UpdateTimeText()
        {
            if (_cpuHost != null)
                elapsedTime.Text =
                    timeUnitSelector.GetFormatedText(_cpuHost.EmulatorTime);
        }

        /// @brief Format the value to string, considering the value
        ///  of FreqFormatValue.
        /// @param val Value to format to string.
        /// @returns The text formatted.
        /// @version v20.09.01 - Added.
        private string FreqFormatText(uint val)
        {
            return string.Format(_currentCultureMod, "{0,17:#,##0}", val);
        }


        /// @brief Format the value to string, for all time units except
        ///  Minutes (TimeUnitsEnum.min_s).
        /// @details Implements Gear.Utils.FormatToTextDelegate delegate.
        /// @param unit Time unit to use.
        /// @param val Value to format to string.
        /// @returns The formatted text.
        /// @version v20.09.01 - Added.
        private string StandardTimeFormatText(TimeUnitsEnum unit, double val)
        {
            double factor = ((unit <= TimeUnitsEnum.s) ?
                timeUnitSelector.FactorSelected : 1.0);
            string decimalsSymbols = new string('0', 3 * (int)unit - 2);
            string numFormat = $"{{0,17:#,##0.{decimalsSymbols}}}";
            double value = (timeUnitSelector.IsMultiplyFactor) ?
                val * factor : val / factor;
            return string.Format(_currentCultureMod, numFormat, value);
        }

        /// @brief Format the value to string, only for Minutes (TimeUnitsEnum.min_s).
        /// @details Implements Gear.Utils.FormatToTextDelegate delegate.
        /// @param unit Time unit to use.
        /// @param val Value to format to string.
        /// @returns The formatted text.
        /// @version v20.09.01 - Added.
        private string MinutesTimeFormatText(TimeUnitsEnum unit, double val)
        {
            if (!timeUnitSelector.IsMultiplyFactor)
                return string.Format(_currentCultureMod,
                    "{0,3:#0}:{1,13:00.0000000000}",
                    Math.Floor(val / timeUnitSelector.FactorSelected),
                        val % timeUnitSelector.FactorSelected);
            else
                return string.Format(_currentCultureMod,
                    "{0,3:#0}:{1,13:00.0000000000}",
                    Math.Floor(val * timeUnitSelector.FactorSelected),
                        val % (1.0 / timeUnitSelector.FactorSelected));
        }

        /// @brief Update frequency labels tool tips.
        /// @param val Format to use for frequency labels.
        /// @version v22.04.02 - Changed to string interpolation.
        private void UpdateFreqToolTips()
        {
            string txt = $"\r\n(Click to change Format from [{FreqFormatValue}])";
            toolTip1.SetToolTip(systemCounter, "System Counter Value" + txt);
            toolTip1.SetToolTip(xtalFrequency, "Crystal Frequency" + txt);
            toolTip1.SetToolTip(coreFrequency, "Core Frequency" + txt);
        }

        /// @brief Change the frequencies labels format, remembering the user setting.
        /// @param sender
        /// @param e
        /// @version v20.09.01 - Added.
        private void FrequencyLabels_Click(object sender, EventArgs e)
        {
            if (FreqFormatValue < Enum.GetValues(typeof(NumberFormatEnum)).Cast<NumberFormatEnum>().Max())
                ++FreqFormatValue;            else
                FreqFormatValue = Enum.GetValues(typeof(NumberFormatEnum)).Cast<NumberFormatEnum>().Min();
            UpdateFreqToolTips();
            DataChanged();
            //remember the setting
            Properties.Settings.Default.FreqFormat = FreqFormatValue;
            Properties.Settings.Default.Save();
        }

        /// @brief Change the time unit, remembering the user setting.
        /// @param sender
        /// @param e
        /// @version v20.09.01 - Added.
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
        /// @version v20.09.01 - Added.
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

