/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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
using System.Drawing;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief Rotating gear hub view object.
    public partial class RingMeter : UserControl
    {
        /// <summary></summary>
        private uint _ringPosition;

        /// <summary></summary>
        private static readonly StringFormat DrawFormat = new StringFormat();

        /// <summary>Number of tooth for gear representation.</summary>
        private const int ToothNumber = 8;

        /// <summary></summary>
        public uint Value
        {
            get => _ringPosition;
            set
            {
                uint old = _ringPosition;
                _ringPosition = (value & 0xF);
                if (old != _ringPosition)
                    Invalidate();
            }
        }

        /// <summary>Default constructor</summary>
        public RingMeter()
        {
            InitializeComponent();
            DrawFormat.LineAlignment = StringAlignment.Center;
            DrawFormat.Alignment = StringAlignment.Center;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            Height = Width;
            base.OnSizeChanged(e);
            Refresh();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (e is null)
                throw new ArgumentNullException(nameof(e));
            double scale = this.Width;
            double rotation = 14.50 - _ringPosition * 2;
            double baseLocationX = scale / 2;
            double baseLocationY = scale / 2;
            double innerSize = scale / 2 - 16;
            double outerSize = innerSize * 0.75;
            double topShift = Math.PI / 24;

            Point[] points = new Point[ToothNumber * 4];

            for (int i = 0; i < points.Length; i += 4)
            {
                points[i + 0] = new Point(
                    (int)(-Math.Sin((i + topShift + rotation) * Math.PI / 16.0) * outerSize + baseLocationX),
                    (int)(Math.Cos((i + topShift + rotation) * Math.PI / 16.0) * outerSize + baseLocationY)
                );
                points[i + 1] = new Point(
                    (int)(-Math.Sin((i + 1 + rotation) * Math.PI / 16.0) * innerSize + baseLocationX),
                    (int)(Math.Cos((i + 1 + rotation) * Math.PI / 16.0) * innerSize + baseLocationY)
                );
                points[i + 2] = new Point(
                    (int)(-Math.Sin((i + 2 + rotation) * Math.PI / 16.0) * innerSize + baseLocationX),
                    (int)(Math.Cos((i + 2 + rotation) * Math.PI / 16.0) * innerSize + baseLocationY)
                );
                points[i + 3] = new Point(
                    (int)(-Math.Sin((i + 3 - topShift + rotation) * Math.PI / 16.0) * outerSize + baseLocationX),
                    (int)(Math.Cos((i + 3 - topShift + rotation) * Math.PI / 16.0) * outerSize + baseLocationY)
                );
            }

            e.Graphics.FillPolygon(Brushes.White, points);
            e.Graphics.DrawPolygon(Pens.Black, points);
            e.Graphics.DrawEllipse(Pens.Black,
                new Rectangle(
                    (int)(baseLocationX - outerSize / 5),
                    (int)(baseLocationY - outerSize / 5),
                    (int)(outerSize / 2.5),
                    (int)(outerSize / 2.5)
                )
            );

            for (int i = 0; i < points.Length; i += 4)
            {
                float x = points[i].X + points[i + 1].X + points[i + 2].X + points[i + 3].X;
                float y = points[i].Y + points[i + 1].Y + points[i + 2].Y + points[i + 3].Y;

                e.Graphics.DrawString((i / 4).ToString(), this.Font, Brushes.Black, x / 4, y / 4, DrawFormat);
            }

            points = new Point[3];

            points[0] = new Point((int)(baseLocationX), (int)(baseLocationY - outerSize + scale * 0.02));
            points[1] = new Point((int)(baseLocationX - scale * 0.06), (int)(baseLocationY - outerSize + scale * 0.06));
            points[2] = new Point((int)(baseLocationX + scale * 0.06), (int)(baseLocationY - outerSize + scale * 0.06));

            e.Graphics.DrawPolygon(Pens.Black, points);
            base.OnPaint(e);
        }
    }
}
