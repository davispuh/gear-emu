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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// <summary>Rotating gear hub view object.</summary>
    public partial class RingMeter : UserControl
    {
        /// <summary>Number of tooth for gear representation.</summary>
        private const int ToothNumber = 8;

        /// <summary>Format for text on ring.</summary>
        private static readonly StringFormat DrawFormat =
            new StringFormat(StringFormatFlags.NoClip);

        /// <summary>Object to access Resources for this control, like
        /// rendered images of the Ring.</summary>
        /// @version v22.06.02 - Added to draw the Ring on screen.
        private static readonly ComponentResourceManager Resources =
            new ComponentResourceManager(typeof(RingMeter));

        /// <summary>Position of ring: 0..16.</summary>
        private uint _ringPosition;

        /// <summary>Graphic position of ring: 0..8, using symmetries of the
        /// gear design.</summary>
        /// @version v22.06.02 - Added to draw the Ring on screen.
        private uint _gearPosition;

        /// <summary>Brush color to Draw numbers.</summary>
        /// @version v22.06.02 - Added to accelerate the drawing on screen.
        private readonly SolidBrush _brushNumbers;

        /// <summary>Pre-calculated coordinates of the numbers on tooth.</summary>
        /// @version v22.06.02 - Added to accelerate the drawing on screen.
        private readonly Point[] _numberCoordinates;

        /// <summary>Rendered Image array of the Ring, without numbers.</summary>
        /// @version v22.06.02 - Added to accelerate the drawing on screen.
        private readonly Image[] _ringImages;

        /// <summary>Value of the position of the Ring.</summary>
        /// <remarks>Also update the background image corresponding with
        /// the position.</remarks>
        /// @version v22.06.02 - Modified to use image array and optimized
        /// for image symmetries to redraw only in changes.
        public uint Value
        {
            get => _ringPosition;
            set
            {
                //update only on changes of value
                if (_ringPosition == (value & 0xF))
                    return;
                _ringPosition = value & 0xF;
                //check for ring image symmetries
                if (_gearPosition == _ringPosition % 8)
                    return;
                _gearPosition = _ringPosition % 8;
                //update image of ring
                BackgroundImage = _ringImages[_gearPosition];
            }
        }

        /// <summary>Default constructor</summary>
        /// @version v22.06.02 - Modified to accelerate drawing on screen
        /// of this control, and parallel processing.
        public RingMeter()
        {
            DrawFormat.LineAlignment = StringAlignment.Center;
            DrawFormat.Alignment = StringAlignment.Center;
            _brushNumbers = new SolidBrush(Color.Black);
            InitializeComponent();
            _ringImages = new Image[ToothNumber];
            _numberCoordinates = new Point[ToothNumber * 2];
            Parallel.Invoke(
                CalculateNumberCoordinates,
                AssignImageFromResources);
        }

        /// <summary>Pre calculate coordinates of numbers on tooth.</summary>
        /// <remarks>Calculate in the first eight points, the positions of each
        /// teeth, not rotated. The second group of eight points, contains the
        /// rotated positions of the tooth.</remarks>
        /// @image html Gear_Control_Geometry.svg "Gear geometric design" width=75%
        /// @version v22.06.02 - Added to accelerate drawing on screen of this
        /// control and modified method visibility.
        private void CalculateNumberCoordinates()
        {
            const double toothSepAngle = Math.PI / 4.0; //45°
            const double oddShiftAngle = toothSepAngle / 2.0; //22.5°
            const double featureAngle = toothSepAngle / 8.0; //5.625°
            double scale = Width;
            double baseLocationX = scale / 2;
            double baseLocationY = scale / 2;
            double outerSize = scale / 2 - 16;
            double innerSize = outerSize * 0.75;
            double numRadius = (outerSize * Math.Cos(featureAngle) + innerSize * Math.Cos(3 * featureAngle)) / 2;

            //First group: even ring positions -> gear not rotated
            for (int i = 0; i < ToothNumber; i++)
            {
                _numberCoordinates[i].X = (int)Math.Round(baseLocationX + Math.Sin(i * toothSepAngle) * numRadius);
                _numberCoordinates[i].Y = (int)Math.Round(baseLocationY - Math.Cos(i * toothSepAngle) * numRadius);
            }

            //Second group: odd ring positions -> gear rotated 22.5°
            for (int i = ToothNumber; i < ToothNumber * 2; i++)
            {
                _numberCoordinates[i].X =
                    (int)Math.Round(baseLocationX + Math.Sin(i * toothSepAngle - oddShiftAngle) * numRadius);
                _numberCoordinates[i].Y =
                    (int)Math.Round(baseLocationY - Math.Cos(i * toothSepAngle - oddShiftAngle) * numRadius);
            }
        }

        /// <summary>Assign images of rotating Ring to array from resources.</summary>
        /// @version v22.06.02 - Added to accelerate drawing on screen
        /// of this control.
        private void AssignImageFromResources()
        {
            for (uint index = 0; index < ToothNumber; index++)
                _ringImages[index] =
                    (Image)Resources.GetObject($"Gear_Control{index:D}");
        }

        /// <summary>Event Handler on size changed.</summary>
        /// <remarks>Maintain aspect ratio of 1:1 between height and width.</remarks>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.02 - Modified to use preferred method `Control.Invalidate()`.
        protected override void OnSizeChanged(EventArgs e)
        {
            Height = Width;
            base.OnSizeChanged(e);
            Invalidate();
        }

        /// <summary>Event Handler to paint the control.</summary>
        /// <remarks>Draw only the numbers on teeth.</remarks>
        /// <param name="e">Paint event data arguments.</param>
        /// @version v22.06.02 - Refactored to accelerate drawing on screen
        /// of this control, and set the font aliasing style for text.
        protected override void OnPaint(PaintEventArgs e)
        {
            //use font aliasing style
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            base.OnPaint(e);
            int oddShift = _ringPosition % 2 == 0 ?
                0 :
                ToothNumber;
            int numAdvance = (int)_ringPosition / 2; //integer division
            //draw the number on each teeth
            for (int i = 0; i < ToothNumber; i++)
            {
                int number = (i + numAdvance) % 8;
                e.Graphics.DrawString($"{number:D}", Font, _brushNumbers,
                    _numberCoordinates[i + oddShift].X,
                    _numberCoordinates[i + oddShift].Y, DrawFormat);
            }
        }
    }
}
