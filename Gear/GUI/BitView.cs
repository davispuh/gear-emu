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

        // ASB: tooltip helpers
        private String prefix;
        private String postfix;
        private String text;

        private int last_box;   // ASB: last value of index
        const int nil = -1;     // ASB: constant for none value selected

        public BitView()
        {
            bits = 0;
            shown = 32;
            last_box = nil;     // ASB: none selected initially
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

        // ASB: set property of prefix for tooltip
        public String Prefix
        {
            set
            {
                prefix = value;
            }
            get
            {
                return prefix;
            }
        }

        // ASB: set property of postfix for tooltip
        public String Postfix
        {
            set
            {
                postfix = value;
            }
            get
            {
                return postfix;
            }
        }

        // ASB: get text for tooltip
        private String ToolTipText
        {
            get
            {
                return (prefix + text + postfix);
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

        // ASB: event handler for set/clear tooltip, depending on mouse position
        private void BitView_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X, y = e.Y, temp = 0,
                valX = nil, valY = nil;     // vars for column & row of boxes

            if (x < BitsPerRow * BitSize)   // test Max X range
            {
                temp = (x / BitSize);       // Integer division
                if (((temp * BitSize) <= x) && (x <= ((temp + 1) * BitSize - 2)))
                {                           // test inside box
                    valX = temp;
                }
            }

            // Test max Y range
            if (y < (((int)(shown - 1) % BitsPerRow + 1) * BitSize - 2) && (valX != nil))
            {
                temp = (y / BitSize);       // Integer division
                if (((temp * BitSize) <= y) && (y <= ((temp + 1) * BitSize - 2)))
                {                           // test inside box
                    valY = temp;
                }
            }

            if (valX != nil && valY != nil)
            {
                temp = valY * BitsPerRow + valX;
                // update tooltip only if box has change, to prevent flickering
                if (temp != last_box)
                {
                    text = temp.ToString("0");
                    toolTip1.SetToolTip(this, ToolTipText);
                    last_box = temp;
                }
            }
            else
            {
                last_box = nil;
                toolTip1.SetToolTip(this, "");
            };
        }
    }
}
