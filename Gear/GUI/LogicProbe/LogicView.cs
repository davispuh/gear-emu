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
    public partial class LogicView : Gear.PluginSupport.PluginBase
    {
        /// @brief Current Culture to modify its Number format.
        /// @version v20.09.01 - Added.
        private readonly CultureInfo currentCultureMod =
            (CultureInfo)CultureInfo.CurrentCulture.Clone();

        private readonly Font MonoFont;
        private Bitmap BackBuffer;

        private readonly List<LogicRow> Pins = new List<LogicRow>();
        private readonly LogicDigital[] DigitalPins = new LogicDigital[64];
        private double TimeScale;
        private double Marker;

        public override string Title
        {
            get { return "Logic Probe"; }
        }

        public override bool IsClosable
        {
            get { return false; }
        }

        public override bool IsUserPlugin
        {
            get { return false; }
        }

        /// @brief Default Constructor.
        /// @param chip CPU to reference the view.
        public LogicView(PropellerCPU chip) : base(chip)
        {
            InitializeComponent();

            MonoFont = new Font(FontFamily.GenericMonospace, 10);
            if (MonoFont == null)
                MonoFont = this.Font;

            toolStripComboBox1.SyncValues();
            //Assign delegates for formatting text of timeUnitSelector
            var textFormats = new DelegatesPerTimeUnitsList(
                toolStripComboBox1.ExcludedUnits,
                new SortedList<TimeUnitsEnum, FormatToTextDelegate>()
                {
                    {TimeUnitsEnum.ns, StandardTimeFormatText},
                    {TimeUnitsEnum.us, StandardTimeFormatText},
                    {TimeUnitsEnum.ms, StandardTimeFormatText},
                    {TimeUnitsEnum.s,  StandardTimeFormatText}
                }
            );
            toolStripComboBox1.AssignTextFormats(textFormats);

            //retrieve the default settings for grid
            UpdateLastTimeFrame();
            UpdateLastTickMarkGrid();
            UpdateTimeUnit();
            //Set text for timeFrame & tickMark text boxes, using time unit.
            UpdateFrameAndTickText();

            for (int i = 0; i < DigitalPins.Length; i++)
                DigitalPins[i] = new LogicDigital(i);

            for (int i = 0; i < 32; i++)
                Pins.Add(DigitalPins[i]);
        }

        /// @brief Update the value of TimeScale from default setting.
        /// @version v20.09.01 - Added.
        public void UpdateLastTimeFrame()
        {
            TimeScale = Properties.Settings.Default.LastTimeFrame;
        }

        /// @brief Update the value of Marker from default setting.
        /// @version v20.09.01 - Added.
        public void UpdateLastTickMarkGrid()
        {
            Marker = Properties.Settings.Default.LastTickMarkGrid;
        }

        /// @brief Update the time unit for logic view from default setting.
        /// @version v20.09.01 - Added.
        public void UpdateTimeUnit()
        {
            toolStripComboBox1.TimeUnitSelected =
                Properties.Settings.Default.LogicViewTimeUnit;
        }

        /// @brief Update UI values for TimeScale and Marker using Format.
        /// @version v20.09.01 - Added.
        public void UpdateFrameAndTickText()
        {
            timeFrameBox.Text = toolStripComboBox1.GetFormatedText(TimeScale).Trim();
            tickMarkBox.Text = toolStripComboBox1.GetFormatedText(Marker).Trim();
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
            double factor = toolStripComboBox1.FactorSelected;
            string decimalsSymbols = new string('0', 3 * (int)unit - 2);
            string numFormat = $"{{0,12:#,##0.{decimalsSymbols}}}";
            double value = (toolStripComboBox1.IsMultiplyFactor) ?
                val * factor : val / factor;
            return string.Format(currentCultureMod, numFormat, value);
        }

        /// @brief
        ///
        public override void PresentChip()
        {
            Chip.NotifyOnPins(this);
        }

        /// @brief Clean samples of LogicRow
        ///
        public override void OnReset()
        {
            for (int i = 0; i < DigitalPins.Length; i++)
            {
                DigitalPins[i].Reset();
            }
        }

        /// @brief Save the last used settings of the grid view before close.
        /// @version v22.03.02 - Bugfix to save TimeUnitSelected with the other
        /// properties in logic view.
        public override void OnClose()
        {
            Properties.Settings.Default.LastTimeFrame = TimeScale;
            Properties.Settings.Default.LastTickMarkGrid = Marker;
            Properties.Settings.Default.LogicViewTimeUnit =
                toolStripComboBox1.TimeUnitSelected;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time"></param>
        /// <param name="states"></param>
        public override void OnPinChange(double time, PinState[] states)
        {
            for (int i = 0; i < states.Length; i++)
            {
                DigitalPins[i].Update(states[i], time);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tick"></param>
        public override void Repaint(bool tick)
        {
            if (Chip == null || Pins == null)
                return;

            viewOffset.Maximum = Pins.Count - 1;

            Graphics g = Graphics.FromImage((Image)BackBuffer);

            g.FillRectangle(SystemBrushes.Control, 0, 0, 64, waveView.ClientSize.Height);
            g.FillRectangle(Brushes.Black, 64, 0, waveView.ClientSize.Width, waveView.ClientSize.Height);

            if (Pins.Count <= 0)
                return;

            if (viewOffset.Value < 0)
                viewOffset.Value = 0;

            double maxTime;
            double minTime;

            if (timeAdjustBar.Value == timeAdjustBar.Maximum)
            {
                maxTime = Chip.EmulatorTime;
                minTime = maxTime - TimeScale;
            }
            else
            {
                double minimum = Pins[0].MinTime;

                for (int i = 1; i < Pins.Count; i++)
                    if (minimum < Pins[i].MinTime)
                        minimum = Pins[i].MinTime;

                double range = (Chip.EmulatorTime - minimum) - TimeScale;  // Only allow it to scale to the minimum time

                if (range > 0)
                {
                    minTime = range * (timeAdjustBar.Value + timeAdjustBar.LargeChange) /
                              timeAdjustBar.Maximum + minimum;
                    maxTime = minTime + TimeScale;
                }
                else
                {
                    maxTime = Chip.EmulatorTime;
                    minTime = maxTime - TimeScale;
                }
            }

            // -------------------------------

            // Position of the first marker
            double markAt = minTime / Marker;
            // Use the fractional portion as a base offset
            markAt = (Math.Ceiling(markAt) - markAt) * Marker / TimeScale;
            // Convert to client space
            markAt = markAt * (waveView.ClientSize.Width - 64) + 64;

            while (markAt < ClientSize.Width)
            {
                g.DrawLine(Pens.Gray, (float)markAt, 0, (float)markAt, waveView.ClientSize.Height);
                markAt += (Marker / TimeScale) * (waveView.ClientSize.Width - 64);
            }

            for (int i = viewOffset.Value, p = 0;
                p < waveView.Height && i < Pins.Count; i++)
            {
                g.DrawString(Pins[i].Name, MonoFont, Brushes.Black, 8, p);
                p += Pins[i].Draw(g, p,
                    64, waveView.ClientSize.Width - 64,
                    minTime, TimeScale);
                g.DrawLine(Pens.Gray, 64, p, waveView.ClientSize.Width, p);
            }

            waveView.CreateGraphics().DrawImageUnscaled(BackBuffer, 0, 0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSized(object sender, EventArgs e)
        {
            if (waveView.Width > 0 && waveView.Height > 0)
                BackBuffer = new Bitmap(
                    waveView.Width,
                    waveView.Height);
            else
                BackBuffer = new Bitmap(1, 1);

            Repaint(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollChanged(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveView_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }

        /// <summary>
        /// Update the grid view with the new settings of scale and markers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// @version v22.04.01 - Bugfix on no detecting modification of timeFrame or tickMark on LogicView.
        private void UpdateGridButton_Click(object sender, EventArgs e)
        {
            double aux = TimeUnitsEnumExtension.TransformUnitsFactor(
                    toolStripComboBox1.TimeUnitSelected,
                    out bool inverseOperation,
                    toolStripComboBox1.BaseUnit);
            try
            {
                TimeScale = (inverseOperation) ?
                    Convert.ToDouble(timeFrameBox.Text) / aux :
                    Convert.ToDouble(timeFrameBox.Text) * aux;
            }
            catch (FormatException)
            {
                timeFrameBox.Text = TimeScale.ToString();
            }

            try
            {
                Marker = (inverseOperation) ?
                    Convert.ToDouble(tickMarkBox.Text) / aux :
                    Convert.ToDouble(tickMarkBox.Text) * aux;
            }
            catch (FormatException)
            {
                tickMarkBox.Text = Marker.ToString();
            }

            //Bugfix to detect next modification of timeFrame or tickMark
            timeFrameBox.Modified = false;
            tickMarkBox.Modified = false;
            updateGridButton.Enabled = false;
            updateGridButton.Checked = false;

            Repaint(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeChanged(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClick(object sender, EventArgs e)
        {
            if (Pins.Count == 0)
                return;

            MouseEventArgs mouse = (MouseEventArgs)e;
            if (mouse.X < 64)
                return;

            for (int i = viewOffset.Value, p = 0;
                p < waveView.Height && i < Pins.Count; i++)
            {
                p += Pins[i].Height;

                if (mouse.Y >= p)
                    continue;

                Pins[i].Click();
                Repaint(true);
                return;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDblClick(object sender, EventArgs e)
        {
            if (Pins.Count == 0)
                return;

            MouseEventArgs mouse = (MouseEventArgs)e;
            if (mouse.X < 64)
                return;

            for (int i = viewOffset.Value, p = 0;
                p < waveView.Height && i < Pins.Count; i++)
            {
                p += Pins[i].Height;

                if (mouse.Y >= p)
                    continue;

                Pins.RemoveAt(i);
                Repaint(true);
                return;
            }

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DigitalButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (string s in pinsTextBox.Text.Split(','))
                {
                    if (s.Contains(".."))
                    {
                        string[] range = s.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                        if (range.Length < 2)
                        {
                            MessageBox.Show("Invalid range value", "Pin value Problem",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        int start = Convert.ToUInt16(range[0]);
                        int end = Convert.ToUInt16(range[1]);
                        int step = (start < end) ? 1 : -1;

                        while (start != end)
                        {
                            Pins.Add(DigitalPins[start]);
                            start += step;
                        }
                        Pins.Add(DigitalPins[end]);
                    }
                    else
                    {
                        Pins.Add(DigitalPins[Convert.ToUInt16(s)]);
                    }
                }
                pinsTextBox.Text = string.Empty;
            }
            catch (FormatException)
            {
                MessageBox.Show(string.Format("Value needs to be a valid number between 0 and {0}.",
                        PropellerCPU.TOTAL_PINS - 1),
                        "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show(string.Format("You must specify a pin between 0 and {0}",
                    PropellerCPU.TOTAL_PINS - 1),
                    "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (OverflowException)
            {
                MessageBox.Show(string.Format("You must specify a pin between 0 and {0}",
                    PropellerCPU.TOTAL_PINS - 1),
                    "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            Repaint(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalogButton_Click(object sender, EventArgs e)
        {
            string[] numbers = pinsTextBox.Text.Split(',');
            List<LogicDigital> pins = new List<LogicDigital>();

            for (int i = 0; i < numbers.Length; i++)
            {
                try
                {
                    if (numbers[i].Contains(".."))
                    {
                        string[] range = numbers[i].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                        if (range.Length < 2)
                        {
                            MessageBox.Show("Invalid range value", "Pin value Problem",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        int start = Convert.ToUInt16(range[0]);
                        int end = Convert.ToUInt16(range[1]);
                        int step = (start < end) ? 1 : -1;

                        while (start != end)
                        {
                            pins.Add(DigitalPins[start]);
                            start += step;
                        }
                        pins.Add(DigitalPins[end]);
                    }
                    else
                    {
                        pins.Add(DigitalPins[Convert.ToUInt16(numbers[i])]);
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show(string.Format("Pin Value needs to be a valid number between 0 and {0}.",
                        PropellerCPU.TOTAL_PINS - 1),
                        "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show(string.Format("You must specify a pin between 0 and {0}",
                        PropellerCPU.TOTAL_PINS - 1),
                        "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                catch (OverflowException)
                {
                    MessageBox.Show(string.Format("You must specify a pin between 0 and {0}",
                        PropellerCPU.TOTAL_PINS - 1),
                        "Pin value Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            pinsTextBox.Text = string.Empty;
            Pins.Add(new LogicAnalog(pins.ToArray()));
            Repaint(true);
        }

        /// @brief Change the time unit, remembering the user setting.
        /// @param sender
        /// @param e
        /// @version v22.03.02 - Bugfix to save TimeUnitSelected with the other
        /// properties in logic view.
        private void ToolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.DesignMode)
                UpdateFrameAndTickText();
        }

        /// @brief When the value is changed, enable Update button.
        /// @param sender
        /// @param e
        private void TimeFrameBox_ModifiedChanged(object sender, EventArgs e)
        {
            if (!updateGridButton.Enabled)
            {
                updateGridButton.Enabled = true;
                updateGridButton.Checked = true;
            }
        }

        /// @brief When the value is changed, enable Update button.
        /// @param sender
        /// @param e
        private void TickMarkBox_ModifiedChanged(object sender, EventArgs e)
        {
            if (!updateGridButton.Enabled)
            {
                updateGridButton.Enabled = true;
                updateGridButton.Checked = true;
            }
        }
    }
}
