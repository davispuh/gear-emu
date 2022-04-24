/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * SpinView.cs
 * Spin object viewer class
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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief Spin object viewer.
    partial class SpinView : Gear.PluginSupport.PluginBase
    {
        /// @brief Current Culture to modify its Number format.
        /// @version v20.09.01 - Added.
        private readonly CultureInfo currentCultureMod =
            (CultureInfo)CultureInfo.CurrentCulture.Clone();

        public override string Title
        {
            get { return "Spin Map"; }
        }

        public override bool IsClosable
        {
            get { return false; }
        }

        public override bool IsUserPlugin
        {
            get { return false; }
        }

        /// @brief Default constructor.
        /// @param chip Reference to Propeller instance.
        public SpinView(PropellerCPU chip) : base(chip)
        {
            MonoSpace = new Font(FontFamily.GenericMonospace, 8);
            if (MonoSpace == null)
                MonoSpace = Font;

            Colorize = new Brush[0x10000];
            for (int i = 0; i < Colorize.Length; i++)
            {
                Colorize[i] = Brushes.Gray;
            }
            //retrieve saved settings
            UpdateFreqFormat();

            InitializeComponent();
        }

        /// @brief Update frequency format to be displayed.
        /// @version v20.09.01 - Added.
        public void UpdateFreqFormat()
        {
            currentCultureMod.NumberFormat =
                NumberFormatEnumExtension.GetFormatInfo(
                    Properties.Settings.Default.FreqFormat);
        }

        public override void PresentChip() { }

        /// @brief Format the value to string, considering the value
        ///  of FreqFormatValue.
        /// @param val Value to format to string.
        /// @returns The text formatted.
        /// @version v20.09.01 - Added.
        private string FreqFormatText(uint val)
        {
            return string.Format(currentCultureMod, "{0:#,##0}", val);
        }

        private void ColorCode()
        {
            int i;

            objectView.Nodes.Clear();
            TreeNode root = objectView.Nodes.Add("Spin");
            TreeNode node;

            node = root.Nodes.Add(string.Format("System Frequency: {0} Mhz",
                FreqFormatText(Chip.DirectReadLong(0))));
            node.Tag = (int)0;
            node = root.Nodes.Add(string.Format("Clock Mode: {0:X2}", Chip.DirectReadByte(4)));
            node.Tag = (int)4;
            node = root.Nodes.Add(string.Format("Check Sum: {0:X2}", Chip.DirectReadByte(5)));
            node.Tag = (int)5;
            node = root.Nodes.Add(string.Format("Root Object: {0:X4}", Chip.DirectReadWord(6)));
            node.Tag = (int)6;
            node = root.Nodes.Add(string.Format("Variable Base: {0:X4}", Chip.DirectReadWord(8)));
            node.Tag = (int)8;
            node = root.Nodes.Add(string.Format("Local Frame: {0:X4}", Chip.DirectReadWord(10)));
            node.Tag = (int)10;
            node = root.Nodes.Add(string.Format("Entry PC: {0:X4}", Chip.DirectReadWord(12)));
            node.Tag = (int)12;
            node = root.Nodes.Add(string.Format("Starting Stack: {0:X4}", Chip.DirectReadWord(14)));
            node.Tag = (int)14;

            for (i = 0; i < 16; i++)
                Colorize[i] = Brushes.White;

            for (i = Chip.DirectReadWord(0x8); i < Chip.DirectReadWord(0xA); i++)
                Colorize[i] = Brushes.LightYellow;

            for (; i < 0x8000; i++)
                Colorize[i] = Brushes.LightGray;

            ColorObject(Chip.DirectReadWord(0x6), Chip.DirectReadWord(0x8), root);
        }

        private void ColorObject(uint objFrame, uint varFrame, TreeNode root)
        {
            uint i, addr, addrnext;

            root = root.Nodes.Add(string.Format("Object {0:X}", objFrame));
            root.Tag = (int)objFrame;

            root.Nodes.Add(string.Format("Variable Space {0:X4}", varFrame)).Tag = (int)varFrame;
            Colorize[varFrame] = Brushes.LightBlue;

            ushort size = Chip.DirectReadWord(objFrame);
            byte longs = Chip.DirectReadByte(objFrame + 2);
            byte objects = Chip.DirectReadByte(objFrame + 3);

            for (i = 0; i < longs * 4; i++)
                Colorize[i + objFrame] = Brushes.LightPink;
            for (; i < (longs + objects) * 4; i++)
                Colorize[i + objFrame] = Brushes.LavenderBlush;
            for (; i < size; i++)
                Colorize[i + objFrame] = Brushes.LightGreen;

            addrnext = Chip.DirectReadWord(1 * 4 + objFrame) + objFrame;
            for (i = 1; i < longs; i++)
            {
                addr = addrnext;
                addrnext = Chip.DirectReadWord((i + 1) * 4 + objFrame) + objFrame;
                if (i == longs - 1)
                {
                    addrnext = addr + 1;
                    while (Colorize[addrnext] == Brushes.LightGreen)
                        addrnext++;
                }
                ColorFunction(addr, addrnext, root);
            }

            for (i = 0; i < objects; i++)
                ColorObject(Chip.DirectReadWord((longs + i) * 4 + objFrame) + objFrame,
                    Chip.DirectReadWord((longs + i) * 4 + 2 + objFrame) + varFrame, root);
        }

        private void ColorFunction(uint functFrame, uint functFrameEnd, TreeNode root)
        {
            root = root.Nodes.Add(string.Format("Function {0:X} ({1:d})", functFrame, functFrameEnd - functFrame));
            root.Tag = (int)functFrame;

            Colorize[functFrame] = Brushes.Yellow;
        }

        public override void Repaint(bool force)
        {
            if (Chip == null)
                return;

            Graphics g = Graphics.FromImage((Image)BackBuffer);

            g.Clear(SystemColors.Control);

            Size s = TextRenderer.MeasureText("00", MonoSpace);
            Size a = TextRenderer.MeasureText("0000:", MonoSpace);

            for (int y = scrollPosition.Value, dy = 0; y < 0x10000 && dy < hexView.ClientRectangle.Height; dy += s.Height)
            {
                // Draw the address
                g.FillRectangle(Brushes.White, new Rectangle(0, dy, a.Width, s.Height));
                g.DrawString(string.Format("{0:X4}:", y), MonoSpace, SystemBrushes.ControlText, 0, dy);
                // Draw the line of data
                for (int x = 0, dx = a.Width; y < 0x10000 && x < 16; x++, dx += s.Width, y++)
                {
                    byte data = Chip.DirectReadByte((uint)y);
                    g.FillRectangle(Colorize[y], new Rectangle(dx, dy, s.Width, s.Height));
                    g.DrawString(string.Format("{0:X2}", data), MonoSpace, SystemBrushes.ControlText, dx, dy);
                }
            }

            hexView.CreateGraphics().DrawImageUnscaled(BackBuffer, 0, 0);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }

        private void SelectChanged(object sender, TreeViewEventArgs e)
        {
            object o = objectView.SelectedNode.Tag;

            if (o != null)
            {
                scrollPosition.Value = (int)o;
                Repaint(false);
            }
        }

        private void AnalizeButton_Click(object sender, EventArgs e)
        {
            ColorCode();
            objectView.ExpandAll();
            Repaint(false);
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        private void OnSize(object sender, EventArgs e)
        {
            if (hexView.Width > 0 && hexView.Height > 0)
            {
                BackBuffer = new Bitmap(
                    hexView.Width,
                    hexView.Height
                );
            }
            else
            {
                BackBuffer = new Bitmap(1, 1);
            }
            Repaint(false);
        }

        private void HexView_MouseClick(object sender, MouseEventArgs e)
        {
            scrollPosition.Focus();
        }

    }
}
