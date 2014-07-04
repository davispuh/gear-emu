/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * MemoryView.cs
 * Main memory viewer class
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

namespace Gear.GUI
{
    public partial class MemoryView : PluginBase
    {
        private Propeller Host;
        private Font MonoFont;
        private Bitmap BackBuffer;

        public override string Title
        {
            get
            {
                return "Main Memory";
            }
        }

        public override Boolean IsClosable
        {
            get
            {
                return false;
            }
        }

        public MemoryView()
        {
            MonoFont = new Font(FontFamily.GenericMonospace, 10);
            if (MonoFont == null)
                MonoFont = this.Font;

            InitializeComponent();
            positionScrollBar.Minimum = 0;
        }

        public override void PresentChip(Propeller host)
        {
            Host = host;
        }

        public override void Repaint(bool tick)
        {
            Graphics g = Graphics.FromImage((Image)BackBuffer);
            byte[] b = new byte[4];

            if (Host == null)
                return;

            for (int i = positionScrollBar.Value, p = 0; p < memoryPanel.Height && i < 0x10000; i += 4, p += MonoFont.Height)
            {
                for (int bi = 0; bi < 4; bi++)
                    b[bi] = Host[i + bi];

                ushort s1 = (ushort)(b[0] | (b[1] << 8));
                ushort s2 = (ushort)(b[2] | (b[3] << 8));

                int i1 = (int)(s1 | (s2 << 16));

                g.FillRectangle(SystemBrushes.Control, 0, p, memoryPanel.Width, p + MonoFont.Height);
                g.DrawString(
                    String.Format("{0:X4}:  {1:X2} {2:X2} {3:X2} {4:X2}  :\t{5}\t{6}\t{7}\t{8}\t{9:d}",
                        i, b[0], b[1], b[2], b[3], s1, s2, (short)s1, (short)s2, i1), MonoFont, SystemBrushes.ControlText, 0, p);
            }

            memoryPanel.CreateGraphics().DrawImageUnscaled(BackBuffer, 0, 0);
        }

        private void PaintMemoryView(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }

        private void SizeChange(object sender, EventArgs e)
        {
            positionScrollBar.Maximum = 0x10000 - memoryPanel.Height / MonoFont.Height;

            if (memoryPanel.Width > 0 && memoryPanel.Height > 0)
                BackBuffer = new Bitmap(
                    memoryPanel.Width,
                    memoryPanel.Height);
            else
                BackBuffer = new Bitmap(1, 1);
            Repaint(false);
        }

        private void PositionChanged(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        private void memoryPanel_MouseClick(object sender, MouseEventArgs e)
        {
            positionScrollBar.Focus();
        }
    }
}
