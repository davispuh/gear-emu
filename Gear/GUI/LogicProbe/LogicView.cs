/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Gear.EmulationCore;
using Gear.PluginSupport;

namespace Gear.GUI.LogicProbe
{
    public partial class LogicView : PluginBase
    {
        private Propeller Host;
        private Font MonoFont;

        private List<LogicRow> Pins;
        private LogicDigital[] DigitalPins;
        private double TimeScale;
        private double Marker;

        private Bitmap BackBuffer;

        public override string Title
        {
            get
            {
                return "Logic Probe";
            }
        }

        public override Boolean IsClosable
        {
            get
            {
                return false;
            }
        }

        public LogicView()
        {
            InitializeComponent();

            MonoFont = new Font(FontFamily.GenericMonospace, 10);
            if (MonoFont == null)
            {
                MonoFont = this.Font;
            }

            viewOffset.LargeChange = 1;

            // ASB: set international localized text for timeFrame & tickMark text boxes
            double timeFrame = 0.0001,
                   tickMark  = 0.0000032;

            timeFrameBox.Text = timeFrame.ToString("0.0000");
            tickMarkBox.Text = tickMark.ToString("0.0000000") ;

            TimeScale = 0.0001;
            Marker = 256.0 / 80000000.0;

            Pins = new List<LogicRow>();
            DigitalPins = new LogicDigital[64];

            for (int i = 0; i < DigitalPins.Length; i++)
            {
                DigitalPins[i] = new LogicDigital(i);
            }

            for (int i = 0; i < 32; i++)
            {
                Pins.Add(DigitalPins[i]);
            }
        }

        public override void PresentChip(Propeller host)
        {
            Host = host;
            Host.NotifyOnPins(this);
        }

        // ASB: added method to clean samples of LogicRow
        public override void OnReset()
        {
            for (int i = 0; i < DigitalPins.Length; i++)
            {
                DigitalPins[i].Reset();
            }
        }

        public override void OnPinChange(double time, PinState[] states)
        {
            for (int i = 0; i < states.Length; i++)
            {
                DigitalPins[i].Update(states[i], time);
            }
        }

        public override void Repaint(bool tick)
        {
            if (Host == null)
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
                maxTime = Host.EmulatorTime;
                minTime = maxTime - TimeScale;
            }
            else
            {
                double minimum = Pins[0].MinTime;

                for (int i = 1; i < Pins.Count; i++)
                    if (minimum < Pins[i].MinTime)
                        minimum = Pins[i].MinTime;

                double range = (Host.EmulatorTime - minimum) - TimeScale;  // Only allow it to scale to the minimum time

                if (range > 0)
                {
                    minTime = range * (timeAdjustBar.Value + timeAdjustBar.LargeChange) /
                              timeAdjustBar.Maximum + minimum;
                    maxTime = minTime + TimeScale;
                }
                else
                {
                    maxTime = Host.EmulatorTime;
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

        private void ScrollChanged(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        private void WaveView_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }

        private void updateGridButton_Click(object sender, EventArgs e)
        {
            try
            {
                TimeScale = Convert.ToDouble(timeFrameBox.Text);
            }
            catch (FormatException)
            {
                timeFrameBox.Text = TimeScale.ToString();
            }

            try
            {
                Marker = Convert.ToDouble(tickMarkBox.Text);
            }
            catch (FormatException)
            {
                tickMarkBox.Text = Marker.ToString();
            }

            Repaint(true);
        }

        private void TimeChanged(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

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

        private void digitalButton_Click(object sender, EventArgs e)
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
                            MessageBox.Show("Invalid range value");
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
                pinsTextBox.Text = "";
            }
            catch (FormatException)
            {
                MessageBox.Show("Value needs to be a valid number.");
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("You must specify a pin between 0 and 63");
            }
            catch (OverflowException)
            {
                MessageBox.Show("You must specify a pin between 0 and 63");
            }

            Repaint(true);
        }

        private void analogButton_Click(object sender, EventArgs e)
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
                            MessageBox.Show("Invalid range value");
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
                    MessageBox.Show("Value needs to be a valid number.");
                    return;
                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show("You must specify a pin between 0 and 63");
                    return;
                }
                catch (OverflowException)
                {
                    MessageBox.Show("You must specify a pin between 0 and 63");
                    return;
                }
            }

            pinsTextBox.Text = "";
            Pins.Add(new LogicAnalog(pins.ToArray()));
            Repaint(true);
        }
    }
}
