/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * BitView.cs
 * Simple highlighted bit viewer
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

namespace Gear.GUI
{
    public partial class BitView : UserControl
    {
        const int BitSize = 8;
        const int BitsPerRow = 16;

        private ulong bits;
        private int shown;

        public BitView()
        {
            bits = 0;
            shown = 32;
            InitializeComponent();
        }

        public int Bits
        {
            set
            {
                shown = value;

                if (value <= BitsPerRow)
                {
                    Width = value * BitSize;
                    Height = BitSize;
                }
                else
                {
                    Width = BitsPerRow * BitSize;
                    Height = BitSize * (value / BitsPerRow);

                    if ((value % BitsPerRow) > 0)
                    {
                        Height += BitSize;
                    }
                }
            }
            get
            {
                return shown;
            }
        }

        public ulong Value
        {
            set
            {
                bits = value;
                Redraw(CreateGraphics());
            }
            get
            {
                return bits;
            }
        }

        private void Redraw(Graphics g)
        {
            for (int i = 0; i < shown; i++)
            {
                g.FillRectangle(
                    ((bits >> i) & 1) != 0 ? Brushes.Yellow : Brushes.Black,
                    i % BitsPerRow * BitSize + 1,
                    i / BitsPerRow * BitSize + 1,
                    BitSize - 4,
                    BitSize - 4);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            for (int i = 0; i < shown; i++)
            {
                e.Graphics.FillRectangle(
                    Brushes.Black,
                    i % BitsPerRow * BitSize,
                    i / BitsPerRow * BitSize,
                    BitSize - 2,
                    BitSize - 2);
            }

            Redraw(e.Graphics);
        }
    }
}
