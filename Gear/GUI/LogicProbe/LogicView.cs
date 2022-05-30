/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * LogicView.cs
 * Logical pin waveform viewer
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
using System.Windows.Forms;

namespace Gear.GUI.LogicProbe
{
    /// @brief Logical pin waveform viewer
    public partial class LogicView : PluginSupport.PluginBase
    {
        /// <summary></summary>
        /// @version v.22.06.01 - Added.
        private const int LeftMargin = 64;
        /// @brief Current Culture to modify its Number format.
        /// @version v20.09.01 - Added.
        private readonly CultureInfo _currentCultureMod =
            (CultureInfo)CultureInfo.CurrentCulture.Clone();

        /// <summary></summary>
        private readonly Font _monoFont;
        /// <summary></summary>
        private Bitmap _backBuffer;

        /// <summary></summary>
        private readonly List<LogicRow> _pins = new List<LogicRow>();
        /// <summary></summary>
        private readonly LogicDigital[] _digitalPins = new LogicDigital[PropellerCPU.TotalPins];
        /// <summary></summary>
        private double _timeScale;
        /// <summary></summary>
        private double _marker;

        /// <summary></summary>
        public override string Title => "Logic Probe";

        /// <summary></summary>
        public override bool IsClosable => false;

        /// <summary></summary>
        public override bool IsUserPlugin => false;

        /// @brief Default Constructor.
        /// @param chip CPU to reference the view.
        /// @todo Parallelism [complex:low, cycles:64] point in loops of logicView._digitalPins[]
        /// @todo Parallelism [complex:low, cycles:typ32 ]point in loops of logicView._pins[]
        public LogicView(PropellerCPU chip) : base(chip)
        {
            InitializeComponent();

            _monoFont = new Font(FontFamily.GenericMonospace, 10) ?? Font;

            toolStripCmbBoxUnit.SyncValues();
            //Assign delegates for formatting text of timeUnitSelector
            var textFormats = new DelegatesPerTimeUnitsList(
                toolStripCmbBoxUnit.ExcludedUnits,
                new SortedList<TimeUnitsEnum, FormatToTextDelegate>()
                {
                    {TimeUnitsEnum.ns, StandardTimeFormatText},
                    {TimeUnitsEnum.us, StandardTimeFormatText},
                    {TimeUnitsEnum.ms, StandardTimeFormatText},
                    {TimeUnitsEnum.s,  StandardTimeFormatText}
                }
            );
            toolStripCmbBoxUnit.AssignTextFormats(textFormats);

            //retrieve the default settings for grid
            UpdateLastTimeFrame();
            UpdateLastTickMarkGrid();
            UpdateTimeUnit();
            //Set text for timeFrame & tickMark text boxes, using time unit.
            UpdateFrameAndTickText();

            for (int i = 0; i < PropellerCPU.TotalPins; i++)  //TODO Parallelism [complex:low, cycles:64] point in loop _digitalPins[]
                _digitalPins[i] = new LogicDigital(i);

            for (int i = 0; i < PropellerCPU.PhysicalPins; i++)  //TODO Parallelism [complex:low, cycles:typ32] point in loop _pins[]
                _pins.Add(_digitalPins[i]);
        }

        /// @brief Update the value of TimeScale from default setting.
        /// @version v20.09.01 - Added.
        public void UpdateLastTimeFrame()
        {
            _timeScale = Properties.Settings.Default.LastTimeFrame;
        }

        /// @brief Update the value of Marker from default setting.
        /// @version v20.09.01 - Added.
        public void UpdateLastTickMarkGrid()
        {
            _marker = Properties.Settings.Default.LastTickMarkGrid;
        }

        /// @brief Update the time unit for logic view from default setting.
        /// @version v20.09.01 - Added.
        public void UpdateTimeUnit()
        {
            toolStripCmbBoxUnit.TimeUnitSelected =
                Properties.Settings.Default.LogicViewTimeUnit;
        }

        /// @brief Update UI values for TimeScale and Marker using Format.
        /// @version v20.09.01 - Added.
        public void UpdateFrameAndTickText()
        {
            timeFrameBox.Text = toolStripCmbBoxUnit.GetFormattedText(_timeScale).Trim();
            tickMarkBox.Text = toolStripCmbBoxUnit.GetFormattedText(_marker).Trim();
            timeFrameBox.Invalidate();
            tickMarkBox.Invalidate();
        }

        /// @brief Format the value to string, for all time units.
        /// @details Implements Gear.Utils.FormatToTextDelegate delegate.
        /// @param unit Time unit to use.
        /// @param val Value to format to string.
        /// @returns The formatted text.
        /// @version v20.09.01 - Added.
        private string StandardTimeFormatText(TimeUnitsEnum unit, double val)
        {
            double factor = toolStripCmbBoxUnit.FactorSelected;
            string decimalsSymbols = new string('0', 3 * (int)unit - 2);
            string numFormat = $"{{0,12:#,##0.{decimalsSymbols}}}";
            double value = toolStripCmbBoxUnit.IsMultiplyFactor ?
                val * factor :
                val / factor;
            return string.Format(_currentCultureMod, numFormat, value);
        }

        /// <summary></summary>
        public override void PresentChip()
        {
            Chip.NotifyOnPins(this);
        }

        /// @brief Clean samples of LogicRow
        /// @todo Parallelism [complex:low, cycles:64] point in loop of logicView._digitalPins[]
        public override void OnReset()
        {
            for (int i = 0; i < PropellerCPU.TotalPins; i++)  //TODO Parallelism [complex:low, cycles:64] point in loop
                _digitalPins[i].Reset();
        }

        /// @brief Save the last used settings of the grid view before close.
        /// @version v22.03.02 - Bugfix to save TimeUnitSelected with the other
        /// properties in logic view.
        public override void OnClose()
        {
            Properties.Settings.Default.LastTimeFrame = _timeScale;
            Properties.Settings.Default.LastTickMarkGrid = _marker;
            Properties.Settings.Default.LogicViewTimeUnit =
                toolStripCmbBoxUnit.TimeUnitSelected;
            Properties.Settings.Default.Save();
        }

        /// <summary></summary>
        /// <param name="time"></param>
        /// <param name="pinStates"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.06.01 - Exception added.
        /// @todo Parallelism [complex:medium, cycles:64] point in loop of LogicView._digitalPins[]
        public override void OnPinChange(double time, PinState[] pinStates)
        {
            if (pinStates == null)
                throw new ArgumentNullException(nameof(pinStates));
            for (int i = 0; i < PropellerCPU.TotalPins; i++)  //TODO Parallelism [complex:medium, cycles:64] point in LogicView._digitalPins[]
                _digitalPins[i].Update(pinStates[i], time);
        }

        /// <summary></summary>
        /// <param name="force"></param>
        /// @todo Parallelism [complex:low, cycles:typ32] point in loops of LogicView._pins[]
        /// @todo Parallelism [complex:high, cycles:typ32] point in loops of LogicView._pins[]
        public override void Repaint(bool force)
        {
            if (Chip == null || _pins == null)
                return;
            viewOffset.Maximum = _pins.Count - 1;
            Graphics graph = Graphics.FromImage(_backBuffer);
            graph.FillRectangle(SystemBrushes.Control, 0, 0, LeftMargin, waveView.ClientSize.Height);
            graph.FillRectangle(Brushes.Black, LeftMargin, 0, waveView.ClientSize.Width, waveView.ClientSize.Height);

            if (_pins.Count <= 0)
                return;
            if (viewOffset.Value < 0)
                viewOffset.Value = 0;
            double maxTime;
            double minTime;
            if (timeAdjustBar.Value == timeAdjustBar.Maximum)
            {
                maxTime = Chip.EmulatorTime;
                minTime = maxTime - _timeScale;
            }
            else
            {
                //find minimum time of all pins
                double minimum = _pins[0].MinTime;
                for (int i = 1; i < _pins.Count; i++)  //TODO Parallelism [complex:low, cycles:typ32] point in loop _pins[]
                    if (minimum < _pins[i].MinTime)
                        minimum = _pins[i].MinTime;
                // Only allow it to scale to the minimum time
                double range = Chip.EmulatorTime - minimum - _timeScale;
                if (range > 0)
                    minTime = range * (timeAdjustBar.Value + timeAdjustBar.LargeChange) /
                              timeAdjustBar.Maximum + minimum;
                else
                {
                    maxTime = Chip.EmulatorTime;
                    minTime = maxTime - _timeScale;
                }
            }

            // -------------------------------

            // Position of the first marker
            double markAt = minTime / _marker;
            // Use the fractional portion as a base offset
            markAt = (Math.Ceiling(markAt) - markAt) * _marker / _timeScale;
            // Convert to client space
            markAt = markAt * (waveView.ClientSize.Width - LeftMargin) + LeftMargin;
            //draw big frame
            while (markAt < ClientSize.Width)
            {
                graph.DrawLine(Pens.Gray, (float)markAt, 0, (float)markAt, waveView.ClientSize.Height);
                markAt += _marker / _timeScale * (waveView.ClientSize.Width - LeftMargin);
            }
            //draw each logicRow and tag
            for (int i = viewOffset.Value, yPosition = 0;
                yPosition < waveView.Height && i < _pins.Count; i++)  //TODO Parallelism [complex:high, cycles:typ32] point in loop _pins[]
            {
                graph.DrawString(_pins[i].Name, _monoFont, Brushes.Black, 8, yPosition);
                yPosition += _pins[i].Draw(graph, yPosition,
                    LeftMargin, waveView.ClientSize.Width - LeftMargin,
                    minTime, _timeScale);
                graph.DrawLine(Pens.Gray, LeftMargin, yPosition, waveView.ClientSize.Width, yPosition);
            }
            waveView.CreateGraphics().DrawImageUnscaled(_backBuffer, 0, 0);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void OnSized(object sender, EventArgs e)
        {
            if (waveView.Width > 0 && waveView.Height > 0)
                _backBuffer = new Bitmap(
                    waveView.Width,
                    waveView.Height);
            else
                _backBuffer = new Bitmap(1, 1);
            Repaint(true);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void ScrollChanged(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void WaveView_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(_backBuffer, 0, 0);
        }

        /// <summary>Update the grid view with the new settings of scale
        /// and markers.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.04.01 - Bugfix on no detecting modification of timeFrame or tickMark on LogicView.
        private void UpdateGridButton_Click(object sender, EventArgs e)
        {
            double aux = TimeUnitsEnumExtension.TransformUnitsFactor(
                    toolStripCmbBoxUnit.TimeUnitSelected,
                    out bool inverseOperation,
                    toolStripCmbBoxUnit.BaseUnit);
            try
            {
                _timeScale = inverseOperation ?
                    Convert.ToDouble(timeFrameBox.Text) / aux :
                    Convert.ToDouble(timeFrameBox.Text) * aux;
            }
            catch (FormatException)
            {
                timeFrameBox.Text = _timeScale.ToString(_currentCultureMod);
            }

            try
            {
                _marker = inverseOperation ?
                    Convert.ToDouble(tickMarkBox.Text) / aux :
                    Convert.ToDouble(tickMarkBox.Text) * aux;
            }
            catch (FormatException)
            {
                tickMarkBox.Text = _marker.ToString(_currentCultureMod);
            }
            //Bugfix to detect next modification of timeFrame or tickMark
            timeFrameBox.Modified = false;
            tickMarkBox.Modified = false;
            updateGridButton.Enabled = false;
            updateGridButton.Checked = false;
            Repaint(true);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void TimeChanged(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        /// <summary> </summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void OnClick(object sender, EventArgs e)
        {
            if (_pins.Count == 0)
                return;
            MouseEventArgs mouse = (MouseEventArgs)e;
            if (mouse.X < LeftMargin)
                return;
            for (int i = viewOffset.Value, yPosition = 0;
                yPosition < waveView.Height && i < _pins.Count; i++)
            {
                yPosition += _pins[i].Height;
                if (mouse.Y >= yPosition)
                    continue;
                _pins[i].Click();
                Repaint(true);
                return;
            }
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void OnDblClick(object sender, EventArgs e)
        {
            if (_pins.Count == 0)
                return;
            MouseEventArgs mouse = (MouseEventArgs)e;
            if (mouse.X < LeftMargin)
                return;
            for (int i = viewOffset.Value, p = 0;
                p < waveView.Height && i < _pins.Count; i++)
            {
                p += _pins[i].Height;
                if (mouse.Y >= p)
                    continue;
                _pins.RemoveAt(i);
                Repaint(true);
                return;
            }
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.01 - Method name changed to clarify it, and
        /// changed to use string interpolation.
        private void AddDigitalButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (string s in pinsTextBox.Text.Split(','))
                {
                    if (s.Contains(".."))
                    {
                        string[] range = s.Split(new char[] { '.' },
                            StringSplitOptions.RemoveEmptyEntries);
                        if (range.Length < 2)
                        {
                            MessageBox.Show("Invalid range value", "Pin value Problem",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        int start = Convert.ToUInt16(range[0]);
                        int end = Convert.ToUInt16(range[1]);
                        int step = start < end ? 1 : -1;
                        while (start != end)
                        {
                            _pins.Add(_digitalPins[start]);
                            start += step;
                        }
                        _pins.Add(_digitalPins[end]);
                    }
                    else
                        _pins.Add(_digitalPins[Convert.ToUInt16(s)]);
                }
                pinsTextBox.Text = string.Empty;
            }
            catch (FormatException)
            {
                MessageBox.Show($"Value needs to be a valid number between 0 and {PropellerCPU.TotalPins - 1}.",
                        "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show($"You must specify a pin between 0 and {PropellerCPU.TotalPins - 1}",
                    "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (OverflowException)
            {
                MessageBox.Show($"You must specify a pin between 0 and {PropellerCPU.TotalPins - 1}",
                    "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            Repaint(true);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.01 - Method name changed to clarify it, and
        /// changed to use string interpolation.
        private void AddAnalogButton_Click(object sender, EventArgs e)
        {
            string[] numbersStr = pinsTextBox.Text.Split(',');
            List<LogicDigital> pins = new List<LogicDigital>();
            foreach (string numberStr in numbersStr)
            {
                try
                {
                    if (numberStr.Contains(".."))
                    {
                        string[] range = numberStr.Split(new char[] { '.' },
                            StringSplitOptions.RemoveEmptyEntries);
                        if (range.Length < 2)
                        {
                            MessageBox.Show("Invalid range value", "Pin value Problem",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        int start = Convert.ToUInt16(range[0]);
                        int end = Convert.ToUInt16(range[1]);
                        int step = start < end ? 1 : -1;
                        while (start != end)
                        {
                            pins.Add(_digitalPins[start]);
                            start += step;
                        }
                        pins.Add(_digitalPins[end]);
                    }
                    else
                    {
                        pins.Add(_digitalPins[Convert.ToUInt16(numberStr)]);
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show(
                        $"Pin Value needs to be a valid number between 0 and {PropellerCPU.TotalPins - 1}.",
                        "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show($"You must specify a pin between 0 and {PropellerCPU.TotalPins - 1}",
                        "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                catch (OverflowException)
                {
                    MessageBox.Show($"You must specify a pin between 0 and {PropellerCPU.TotalPins - 1}",
                        "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            pinsTextBox.Text = string.Empty;
            _pins.Add(new LogicAnalog(pins.ToArray()));
            Repaint(true);
        }

        /// @brief Change the time unit, remembering the user setting.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v22.03.02 - Bugfix to save TimeUnitSelected with the other
        /// properties in logic view.
        private void ToolStripCmbBoxUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
                UpdateFrameAndTickText();
        }

        /// @brief When the value is changed, enable Update button.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        private void TimeFrameBox_ModifiedChanged(object sender, EventArgs e)
        {
            if (updateGridButton.Enabled)
                return;
            updateGridButton.Enabled = true;
            updateGridButton.Checked = true;
        }

        /// @brief When the value is changed, enable Update button.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        private void TickMarkBox_ModifiedChanged(object sender, EventArgs e)
        {
            if (updateGridButton.Enabled)
                return;
            updateGridButton.Enabled = true;
            updateGridButton.Checked = true;
        }
    }
}
