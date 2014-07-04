/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * RingMeter.cs
 * Rotating gear hub view object
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
    public partial class RingMeter : UserControl
    {
        private uint m_RingPosition;

        public uint Value
        {
            get
            {
                return m_RingPosition;
            }
            set
            {
                uint old = m_RingPosition;
                m_RingPosition = (value & 0xF);

                if (old != m_RingPosition)
                    Invalidate();
            }
        }

        public RingMeter()
        {
            InitializeComponent();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.Height = this.Width;
            base.OnSizeChanged(e);
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            double scale = this.Width;
            double rotation = 14.50 - m_RingPosition * 2;
            double BaseLocationX = scale / 2;
            double BaseLocationY = scale / 2;
            double InnerSize = scale / 2 - 16;
            double OuterSize = InnerSize * 0.75;
            double TopShift = Math.PI / 24;

            Point[] points = new Point[8 * 4];
            StringFormat drawFormat = new StringFormat();
            drawFormat.LineAlignment = StringAlignment.Center;
            drawFormat.Alignment = StringAlignment.Center;

            for (int i = 0; i < points.Length; i += 4)
            {
                points[i + 0] = new Point(
                    (int)(-Math.Sin((i + TopShift + rotation) * Math.PI / 16.0) * OuterSize + BaseLocationX),
                    (int)(Math.Cos((i + TopShift + rotation) * Math.PI / 16.0) * OuterSize + BaseLocationY)
                );
                points[i + 1] = new Point(
                    (int)(-Math.Sin((i + 1 + rotation) * Math.PI / 16.0) * InnerSize + BaseLocationX),
                    (int)(Math.Cos((i + 1 + rotation) * Math.PI / 16.0) * InnerSize + BaseLocationY)
                );
                points[i + 2] = new Point(
                    (int)(-Math.Sin((i + 2 + rotation) * Math.PI / 16.0) * InnerSize + BaseLocationX),
                    (int)(Math.Cos((i + 2 + rotation) * Math.PI / 16.0) * InnerSize + BaseLocationY)
                );
                points[i + 3] = new Point(
                    (int)(-Math.Sin((i + 3 - TopShift + rotation) * Math.PI / 16.0) * OuterSize + BaseLocationX),
                    (int)(Math.Cos((i + 3 - TopShift + rotation) * Math.PI / 16.0) * OuterSize + BaseLocationY)
                );
            }

            e.Graphics.FillPolygon(Brushes.White, points);
            e.Graphics.DrawPolygon(Pens.Black, points);
            e.Graphics.DrawEllipse(Pens.Black,
                new Rectangle(
                    (int)(BaseLocationX - OuterSize / 5),
                    (int)(BaseLocationY - OuterSize / 5),
                    (int)(OuterSize / 2.5),
                    (int)(OuterSize / 2.5)
                )
            );

            for (int i = 0; i < points.Length; i += 4)
            {
                float x = points[i].X + points[i + 1].X + points[i + 2].X + points[i + 3].X;
                float y = points[i].Y + points[i + 1].Y + points[i + 2].Y + points[i + 3].Y;

                e.Graphics.DrawString((i / 4).ToString(), this.Font, Brushes.Black, x / 4, y / 4, drawFormat);
            }

            points = new Point[3];

            points[0] = new Point((int)(BaseLocationX), (int)(BaseLocationY - OuterSize + scale * 0.02));
            points[1] = new Point((int)(BaseLocationX - scale * 0.06), (int)(BaseLocationY - OuterSize + scale * 0.06));
            points[2] = new Point((int)(BaseLocationX + scale * 0.06), (int)(BaseLocationY - OuterSize + scale * 0.06));

            e.Graphics.DrawPolygon(Pens.Black, points);
            base.OnPaint(e);
        }
    }
}
