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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

// ReSharper disable LocalizableElement
namespace Gear.GUI
{
    /// @brief %GUI Control to show Hub status
    /// @version v22.06.01 - Added custom debugger text.
    [DefaultProperty("Name"), DebuggerDisplay("{TextForDebugger,nq}")]
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

        /// <summary></summary>
        /// @version v22.06.01 - Added to implement conditional painting.
        private readonly List<IRequestRepaintable> _repaintableList;

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

        /// <summary>Returns a summary text of this class, to be used in debugger view.</summary>
        /// @version v22.06.01 - Added to provide debugging info.
        private string TextForDebugger =>
            $"{{{GetType().FullName}, Id: {(_cpuHost == null ? "[none yet]" : _cpuHost.InstanceNumber.ToString("D2"))} }}";

        /// @brief Default constructor
        /// @issue{30} Linux-Mono: Version 22.06.02 crashes directly after loading a binary.
        /// @version v22.06.03 - Hotfix for issue #30.
        public HubView()
        {
            _repaintableList = new List<IRequestRepaintable>();
            InitializeComponent();
            RegisterRepaintableControls();
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
            //subscribe some properties of PropellerCPU to be notified to this Control
            _cpuHost.PropertyChanged += CoreFreq_PropertyChanged;
            _cpuHost.PropertyChanged += XtalFreq_PropertyChanged;
            _cpuHost.PropertyChanged += ClockModeLabel_PropertyChanged;
            DataChanged(false);
        }

        /// <summary>Maintain a list of Controls with support of request
        /// full re-paint.</summary>
        /// @version v22.06.01 - Added to implement conditional painting.
        private void RegisterRepaintableControls()
        {
            foreach (object obj in Controls)
                if (obj is IRequestRepaintable complaintControl)
                    _repaintableList.Add(complaintControl);
        }

        /// <summary>Event handler when PropellerCPU.ClockMode had changed.</summary>
        /// The name of property (e.PropertyName) could be null, meaning all
        /// the properties of the object are changed, so it must be accepted too.
        /// References: <list type="bullet">
        /// <item>https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler?view=netframework-4.7.2</item>
        /// <item>https://docs.microsoft.com/en-us/dotnet/standard/events/#event-handlers</item>
        /// </list>
        /// <param name="sender">PropellerCPU instance. Should be only one per Emulator.</param>
        /// <param name="e">Arguments of the event like the name of the property.</param>
        /// @version v22.05.04 - Added.
        private void ClockModeLabel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null |
                (e.PropertyName != null && e.PropertyName == nameof(PropellerCPU.ClockMode)))
                if (sender is PropellerCPU cpu)
                    clockModeLabel.Text = cpu.GetClockString();
        }

        /// <summary>Event handler when PropellerCPU.XtalFrequency had changed.</summary>
        /// The name of property (e.PropertyName) could be null, meaning all
        /// the properties of the object are changed, so it must be accepted too.
        /// References: <list type="bullet">
        /// <item>https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler?view=netframework-4.7.2</item>
        /// <item>https://docs.microsoft.com/en-us/dotnet/standard/events/#event-handlers</item>
        /// </list>
        /// <param name="sender">PropellerCPU instance. Should be only one per Emulator.</param>
        /// <param name="e">Arguments of the event like the name of the property.</param>
        /// @version v22.05.04 - Added.
        private void XtalFreq_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null |
                (e.PropertyName != null && e.PropertyName == nameof(PropellerCPU.XtalFrequency)))
                if (sender is PropellerCPU cpu)
                    xtalFrequencyLabel.Text = FreqFormatText(cpu.XtalFrequency);
        }

        /// <summary>Event handler when PropellerCPU.CoreFrequency had changed.</summary>
        /// The name of property (e.PropertyName) could be null, meaning all
        /// the properties of the object are changed, so it must be accepted too.
        /// References: <list type="bullet">
        /// <item>https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler?view=netframework-4.7.2</item>
        /// <item>https://docs.microsoft.com/en-us/dotnet/standard/events/#event-handlers</item>
        /// </list>
        /// <param name="sender">PropellerCPU instance. Should be only one per Emulator.</param>
        /// <param name="e">Arguments of the event like the name of the property.</param>
        /// @version v22.05.04 - Added.
        private void CoreFreq_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null |
                (e.PropertyName != null && e.PropertyName == nameof(PropellerCPU.CoreFrequency)))
                if (sender is PropellerCPU cpu)
                    coreFrequencyLabel.Text = FreqFormatText(cpu.CoreFrequency);
        }

        /// @brief Update the value of FreqFormat from default setting.
        /// @version v20.09.01 - Added.
        public void UpdateFreqFormat()
        {
            FreqFormatValue = Properties.Settings.Default.FreqFormat;
        }

        /// <summary> Update the value of TimeUnit from default setting.</summary>
        /// @version v20.09.01 - Added.
        public void UpdateHubTimeUnit()
        {
            TimeUnit = Properties.Settings.Default.HubTimeUnit;
            //set unit combobox selected item
            timeUnitSelector.TimeUnitSelected = TimeUnit;
        }

        /// @brief Update Counter and Frequency labels with Monospace font.
        /// @version v20.09.01 - Added.
        public void SetFontSpecialLabels()
        {
            Font monoFont = new Font(FontFamily.GenericMonospace, 8.25F,
                FontStyle.Regular, GraphicsUnit.Point);
            coreFrequencyLabel.Font = monoFont;
            xtalFrequencyLabel.Font = monoFont;
            clockModeLabel.Font = monoFont;
            systemCounterLabel.Font = monoFont;
            elapsedTimeLabel.Font = monoFont;
        }

        /// @brief Update screen data on event.
        /// @param force TRUE to request full painting, FALSE to paint only
        /// differences.
        /// @version v22.06.01 - Modified signature to implement conditional
        /// painting.
        /// @todo Analyze bottleneck on DataChanged()
        public void DataChanged(bool force)
        {
            if (_cpuHost == null)
                return;
            if (force)
                RequestFullOnNextRepaint();
            pinLocksFree.Value = _cpuHost.LocksFree;
            pinLocks.Value = _cpuHost.Locks;
            pinDIR.Value = _cpuHost.RegisterDIR;
            pinIN.Value = _cpuHost.RegisterIN;
            pinFloating.Value = _cpuHost.Floating;
            UpdateCounterText();
            UpdateTimeText();
            ringMeter.Value = _cpuHost.RingPosition;
        }

        /// <summary>Request full painting of objects on List.</summary>
        /// @version v22.06.01 - Added to implement conditional painting.
        public void RequestFullOnNextRepaint()
        {
            foreach (IRequestRepaintable objRepaintable in _repaintableList)
                objRepaintable.RequestFullOnNextRepaint();
        }

        /// <summary>Update Counter label with current format and value.</summary>
        /// @version v22.05.04 - Added by the splitting of old %UpdateCounterFreqTexts() method.
        public void UpdateCounterText()
        {
            if (_cpuHost == null)
                return;
            systemCounterLabel.Text = FreqFormatText(_cpuHost.Counter);
        }

        /// @brief Update Frequency labels with current format and values.
        /// @version v22.05.04 - Added by the splitting of old %UpdateCounterFreqTexts() method.
        /// @todo Analyze bottleneck here
        public void UpdateFrequenciesTexts()
        {
            if (_cpuHost == null)
                return;
            coreFrequencyLabel.Text = FreqFormatText(_cpuHost.CoreFrequency);
            xtalFrequencyLabel.Text = FreqFormatText(_cpuHost.XtalFrequency);
        }

        /// @brief Update Time labels with current format and unit.
        /// @version v20.09.01 - Added.
        public void UpdateTimeText()
        {
            if (_cpuHost != null)
                elapsedTimeLabel.Text =
                    timeUnitSelector.GetFormattedText(_cpuHost.EmulatorTime);
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
            double factor = unit <= TimeUnitsEnum.s ?
                timeUnitSelector.FactorSelected : 1.0;
            string decimalsSymbols = new string('0', 3 * (int)unit - 2);
            string numFormat = $"{{0,17:#,##0.{decimalsSymbols}}}";
            double value = timeUnitSelector.IsMultiplyFactor ?
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
            toolTip1.SetToolTip(systemCounterLabel, "System Counter Value" + txt);
            toolTip1.SetToolTip(xtalFrequencyLabel, "Crystal Frequency" + txt);
            toolTip1.SetToolTip(coreFrequencyLabel, "Core Frequency" + txt);
        }

        /// @brief Change the frequencies labels format, remembering the user setting.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v20.09.01 - Added.
        private void FrequencyLabels_Click(object sender, EventArgs e)
        {
            if (FreqFormatValue < Enum.GetValues(typeof(NumberFormatEnum)).Cast<NumberFormatEnum>().Max())
                ++FreqFormatValue;
            else
                FreqFormatValue = Enum.GetValues(typeof(NumberFormatEnum)).Cast<NumberFormatEnum>().Min();
            UpdateFreqToolTips();
            DataChanged(false);
            UpdateFrequenciesTexts();
            //remember the setting
            Properties.Settings.Default.FreqFormat = FreqFormatValue;
            Properties.Settings.Default.Save();
        }

        /// @brief Change the time unit, remembering the user setting.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v20.09.01 - Added.
        private void TimeUnitSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                TimeUnit = timeUnitSelector.TimeUnitSelected;
                UpdateTimeText();
                //remember the setting
                Properties.Settings.Default.HubTimeUnit = TimeUnit;
                Properties.Settings.Default.Save();
            }
        }

        /// @brief Change the time unit, remembering the user setting.
        /// @param sender Reference to object where event was raised.
        /// @param e Mouse event data arguments.
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
            }
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.01 - Added to implement conditional painting.
        private void HubView_SizeChanged(object sender, EventArgs e)
        {
            if (!Visible)
                return;
            foreach (IRequestRepaintable repaintable in _repaintableList)
                if (!repaintable.IsThisFullyVisible())
                    repaintable.RequestFullOnNextRepaint();
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.01 - Added to implement conditional painting.
        private void HubView_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
                return;
            RequestFullOnNextRepaint();
        }

        /// <summary></summary>
        /// <param name="e">Paint event data arguments.</param>
        /// @version v22.06.02 - Modified to font aliasing style for text of
        /// the control.
        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
                RequestFullOnNextRepaint();
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            base.OnPaint(e);
        }
    }
}
