/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// <summary>Interface to request full re-paint of a control.</summary>
    /// @version v22.06.01 - Added.
    public interface IRequestRepaintable
    {
        /// <summary>Determine if this control is visible on screen.</summary>
        /// <returns>TRUE if all the control area is visible on screen,
        /// else FALSE.</returns>
        bool IsThisFullyVisible();
        /// <summary>Request full paint on next Repaint event.</summary>
        void RequestFullOnNextRepaint();
    }

    /// @brief Simple highlighted bit viewer.
    [DefaultProperty("Name"), DebuggerDisplay("{TextForDebugger,nq}")]
    public partial class BitView : UserControl, IRequestRepaintable
    {
        /// <summary>Size of a Bit in pixels.</summary>
        /// @version v22.06.01 - Name changed to clarify its meaning.
        private const int BitSizePx = 8;
        /// <summary>Internal margin in pixels, between each side of BitSizePx.</summary>
        /// @version v22.06.01 - Added.
        private const int BitMarginPx = 1;
        /// <summary>How many bits will be shown per row.</summary>
        private const int BitsPerRow = 16;
        /// <summary>Constant for none value selected.</summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private const int Nil = -1;

        /// <summary>Hold the values of each bit, to show in control.</summary>
        /// @version v22.06.01 - Name changed to clarify its meaning.
        private ulong _bitValues;
        /// <summary>Old value of each bit, used to compare to new changes
        /// and redraw only differences.</summary>
        /// @version v22.06.01 - Added.
        private ulong _oldBitValues;
        /// <summary>How many bits the control will show.</summary>
        /// @version v22.06.01 - Name changed to clarify its meaning from old
        /// name `shown`.
        private uint _bitWidth;
        /// <summary>Full redraw requested indicator.</summary>
        /// @version v22.06.01 - Added.
        private bool _fullRedrawRequested;

        /// <summary>Number of bit detected at mouse hovering.</summary>
        /// @version v22.06.01 - Name and type changed to clarify its meaning.
        private int _bitNumDetected;
        /// <summary>Index of last bit detected to be hover by the mouse.</summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private int _lastBitNumDetected;

        /// <summary>Pre calculated outer rectangle coordinates.</summary>
        /// @version v22.06.01 - Added to accelerate the drawing on screen.
        private readonly List<Rectangle> _outerRectangleList;
        /// <summary>Pre calculated inner rectangle coordinates.</summary>
        /// @version v22.06.01 - Added to accelerate the drawing on screen.
        private readonly List<Rectangle> _innerRectangleList;

        /// <summary>Private field for background color of a bit.</summary>
        /// @version v22.06.01 - Added to support custom colors in Designer mode.
        private Color _bitBackgroundColor;
        /// <summary>Private field for foreground color of a bit.</summary>
        /// @version v22.06.01 - Added to support custom colors in Designer mode.
        private Color _bitForegroundColor;

        /// <summary>Brush color to represent bit in OFF state.</summary>
        /// @version v22.06.01 - Added to accelerate the drawing on screen.
        private readonly SolidBrush _brushOff;
        /// <summary>Brush color to represent bit in ON state.</summary>
        /// @version v22.06.01 - Added to accelerate the drawing on screen.
        private readonly SolidBrush _brushOn;

        /// <summary>Default size of the control.</summary>
        /// @version v22.06.01 - Added to set minimum size of the control.
        private Size _defaultSize;

        /// <summary></summary>
        /// @version v22.06.01 - Added to set minimum size of the control.
        protected sealed override Size DefaultSize => _defaultSize;
        /// <summary></summary>
        /// @version v22.06.01 - Added to set minimum size of the control.
        protected sealed override Size DefaultMinimumSize => _defaultSize;
        /// <summary> </summary>
        /// @version v22.06.01 - Added to set minimum size of the control.
        public sealed override Size MinimumSize
        {
            get => base.MinimumSize;
            set => base.MinimumSize = value;
        }

        /// <summary>Prefix for tooltip shown.</summary>
        /// @version v22.06.01 - Convert to auto property removing old
        /// private member `prefix`.
        [Category("Default"),
         Description("Text to add to begining of string for tooltip."),
         DisplayName("Prefix text for tooltip"), Browsable(true)]
        public string Prefix { get; set; }
        /// <summary>Postfix for tooltip shown.</summary>
        /// @version v22.06.01 - Convert to auto property removing old
        /// private member `postfix`.
        [Category("Default"), Description("Text to add to end of string for tooltip."),
         DisplayName("Postfix text for tooltip"), Browsable(true)]
        public string Postfix { set; get; }
        /// <summary>Text for tooltip when mouse is hovering a bit on screen.</summary>
        /// @version v22.06.01 - Text generation changed to interpolated string.
        private string ToolTipText => $"{Prefix}{_bitNumDetected:D}{Postfix}";

        /// <summary>Background color of all bits, editable in Designer mode.</summary>
        /// @version v22.06.01 - Added to support custom colors.
        [Category("Appearance"), Description("Background color of a bit."),
         Browsable(true), DefaultValue(typeof(Color), "Black")]
        public Color BitBackgroundColor
        {
            get => _bitBackgroundColor;
            set
            {
                if (_bitBackgroundColor == value)
                    return;
                _bitBackgroundColor = value;
                _brushOff.Color = _bitBackgroundColor;
                Invalidate();
            }
        }

        /// <summary>Foreground color of all bits, editable in Designer mode.</summary>
        /// @version v22.06.01 - Added to support custom colors.
        [Category("Appearance"), Description("Foreground color of a bit."),
         Browsable(true), DefaultValue(typeof(Color), "Yellow")]
        public Color BitForegroundColor
        {
            get => _bitForegroundColor;
            set
            {
                if (_bitForegroundColor == value)
                    return;
                _bitForegroundColor = value;
                _brushOn.Color = _bitForegroundColor;
                Invalidate();
            }
        }

        /// <summary>How many bits the control will show.</summary>
        /// @version v22.06.01 - Added validations to modify only on changes
        /// and truncate on size limit, also added pre-calculate coordinates
        /// of bits to accelerate the drawing of control on screen.
        /// @todo Add MsgDialog to inform the truncate (value > MaxBitWidth)
        [Category("Default"), Description("How many bits the control will show."),
         DisplayName("Bit width"), Browsable(true)]
        public uint BitWidth
        {
            get => _bitWidth;
            set
            {
                if (_bitWidth == value)
                    return;
                //check limits in bits
                if (value <= sizeof(ulong) * 8)
                    _bitWidth = value;
                else
                    _bitWidth = sizeof(ulong) * 8; //TODO Add MsgDialog to inform the truncate (value > MaxBitWidth)
                _defaultSize = CalcMinimumSize();
                Size = MinimumSize;
                //re calculate coordinates for each bit on screen
                CalcRectanglesCoords();
                if (!DesignMode)
                    Invalidate();
            }
        }

        /// <summary>Value of each bit of the control.</summary>
        /// @version v22.06.01 - Added validation to modify only on changes
        /// and to store values to accelerate the drawing of control on screen.
        [Category("Default"), Description("Value of the bits."),
         DisplayName("Value"), Browsable(true)]
        public ulong Value
        {
            get => _bitValues;
            set
            {
                if (_bitValues != value)
                {
                    _bitValues = value;
                    ConditionalPaint(CreateGraphics());
                    _oldBitValues = _bitValues;
                }
                else
                    if (_fullRedrawRequested)
                        ConditionalPaint(CreateGraphics());
            }
        }

        /// <summary>Returns a summary text of this class, to be used in
        /// debugger view.</summary>
        /// @version v22.06.01 - Added.
        private string TextForDebugger =>
            $"{{{GetType().FullName}, Name: {Name}, BitWidth: " +
            $"{_bitWidth:D}, FullRedraws: {_fullRedrawRequested} }}";

        /// @brief Default constructor.
        /// @version v22.06.01 - Modified to accelerate the drawing of control
        /// on screen, support editable background and foreground colors in
        /// Designer mode, and to set minimum size.
        public BitView()
        {
            _bitValues = 0UL;
            _oldBitValues = 0UL;
            _bitWidth = 1;
            _fullRedrawRequested = false;
            _lastBitNumDetected = Nil;  // none selected initially
            _outerRectangleList = new List<Rectangle>();
            _innerRectangleList = new List<Rectangle>();
            _bitBackgroundColor = Color.Black;
            _bitForegroundColor = Color.Yellow;
            _brushOff = new SolidBrush(_bitBackgroundColor);
            _brushOn = new SolidBrush(_bitForegroundColor);
            _defaultSize = CalcMinimumSize();
            Size = MinimumSize;
            InitializeComponent();
            CalcRectanglesCoords();
        }

        /// <summary>Determine the minimum size to assure all the pins
        /// are shown on screen.</summary>
        /// @version v22.06.01 - Added.
        private Size CalcMinimumSize()
        {
            Size retSize = new Size();
            //determine the size arrangement of control
            if (_bitWidth <= BitsPerRow)
            {
                retSize.Width = (int)_bitWidth * BitSizePx;
                retSize.Height = BitSizePx;
            }
            else
            {
                retSize.Width = BitsPerRow * BitSizePx;
                retSize.Height = BitSizePx * ((int)_bitWidth / BitsPerRow);
                if (_bitWidth % BitsPerRow > 0)
                    retSize.Height += BitSizePx;
            }
            return retSize;
        }

        /// <summary>Calculate the coordinates of inner and outer rectangles
        /// for each bit on screen.</summary>
        /// @version v22.06.01 - Added to accelerate the drawing of control
        /// on screen. Also the layout of the boxes is centered.
        private void CalcRectanglesCoords()
        {
            Rectangle outerRect = new Rectangle();
            Rectangle innerRect = new Rectangle();
            _outerRectangleList.Clear();
            _innerRectangleList.Clear();
            for (int i = 0; i < _bitWidth; i++)
            {
                outerRect.X = i % BitsPerRow * BitSizePx + BitMarginPx;
                outerRect.Y = i / BitsPerRow * BitSizePx + BitMarginPx;
                outerRect.Width = BitSizePx - 2 * BitMarginPx;
                outerRect.Height = BitSizePx - 2 * BitMarginPx;
                innerRect.X = outerRect.X + 1;  //right: +1
                innerRect.Y = outerRect.Y + 1;  //down: +1
                innerRect.Width = outerRect.Width - 2;  //left: -2
                innerRect.Height = outerRect.Height - 2;  //up: -2
                _outerRectangleList.Add(outerRect);
                _innerRectangleList.Add(innerRect);
            }
        }

        /// <summary>Determine if this control is visible on screen.</summary>
        /// <returns>TRUE if all the control area is visible on screen,
        /// else FALSE.</returns>
        /// @version v22.06.01 - Added to implement conditional painting.
        public bool IsThisFullyVisible()
        {
            Rectangle userRectangle = RectangleToScreen(ClientRectangle);
            Rectangle parentRectangle = Parent.RectangleToScreen(Parent.DisplayRectangle);
            return parentRectangle.Contains(userRectangle);
        }

        /// <summary>Request full paint on next Repaint event.</summary>
        /// @version v22.06.01 - Added to implement conditional painting.
        public void RequestFullOnNextRepaint()
        {
            _fullRedrawRequested = true;
        }

        /// <summary>Paint fully the control.</summary>
        /// <param name="graph">Graphics context.</param>
        /// @version v22.06.01 - Added, merging code from old methods
        /// `OnPaint()` and `Redraw()`, but optimized for speed using
        /// pre-calculated coordinates and to paint two rectangles instead of
        /// three.
        /// @todo Parallelism [complex:medium, cycles:8 or 64] point in loop FillRectangle()
        private void PaintFull(Graphics graph)
        {
            for (int i = 0; i < _bitWidth; i++) //TODO Parallelism [complex:medium, cycles:8 or 64] point in loop FillRectangle()
            {
                ulong mask = 1UL << i;
                graph.FillRectangle(_brushOff, _outerRectangleList[i]);
                if ((_bitValues & mask) != 0UL)
                    graph.FillRectangle(_brushOn, _innerRectangleList[i]);
            }
        }

        /// <summary>Paint only pins that are changed.</summary>
        /// <param name="graph">Graphics context.</param>
        /// @version v22.06.01 - Added to implement conditional painting, with
        /// optimizations as using pre-calculated coordinates and to paint two
        /// rectangles instead of three.
        /// @todo Parallelism [complex:medium, cycles: max 8 or 64] point in loop FillRectangle()
        private void PaintOnlyChanged(Graphics graph)
        {
            ulong changedBits = _bitValues ^ _oldBitValues; //XOR: get only changed bits
            if (changedBits == 0UL)
                return;
            for (int i = 0; i < _bitWidth; i++) //TODO Parallelism [complex:medium, cycles:8 or 64] point in loop FillRectangle()
            {
                ulong mask = 1UL << i;
                if ((changedBits & mask) == 0UL)
                    continue;
                graph.FillRectangle(_brushOff, _outerRectangleList[i]);
                if ((_bitValues & mask) != 0UL)
                    graph.FillRectangle(_brushOn, _innerRectangleList[i]);
            }
        }

        /// <summary>Execute conditional painting of the control, drawing
        /// only the changed pins, except for Design mode or if full redraw
        /// was requested already.</summary>
        /// <param name="graph">Graphics context.</param>
        private void ConditionalPaint(Graphics graph)
        {
            if (DesignMode)
                PaintFull(graph);
            else
            {
                if (!_fullRedrawRequested)
                    PaintOnlyChanged(graph);
                else
                {
                    //draw all the bits
                    PaintFull(graph);
                    if (_fullRedrawRequested && IsThisFullyVisible())
                        _fullRedrawRequested = false;
                }
            }
        }

        /// <summary>Event handler to paint this control.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Paint event data arguments.</param>
        private void BitView_Paint(object sender, PaintEventArgs e)
        {
            ConditionalPaint(e.Graphics);
        }

        /// <summary>Event handler for set/clear tooltip, depending on mouse
        /// position.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Mouse event data arguments.</param>
        /// <exception cref="ArgumentNullException"></exception>
        private void BitView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            int x = e.X, y = e.Y, valX = Nil, valY = Nil;     // vars for column & row of boxes
            int temp;
            if (x < BitsPerRow * BitSizePx)   // test Max X range
            {
                temp = x / BitSizePx;       // Integer division
                if (temp * BitSizePx <= x && x <= (temp + 1) * BitSizePx - 2)
                    // test inside box
                    valX = temp;
            }

            // Test max Y range
            if (y < ((int)(_bitWidth - 1) % BitsPerRow + 1) * BitSizePx - 2 && valX != Nil)
            {
                temp = y / BitSizePx;       // Integer division
                if (temp * BitSizePx <= y && y <= (temp + 1) * BitSizePx - 2)
                    // test inside box
                    valY = temp;
            }

            if (valX != Nil && valY != Nil)
            {
                temp = valY * BitsPerRow + valX;
                // update tooltip only if box has change, to prevent flickering
                if (temp == _lastBitNumDetected)
                    return;
                _bitNumDetected = temp;
                toolTip1.SetToolTip(this, ToolTipText);
                _lastBitNumDetected = temp;
            }
            else
            {
                _lastBitNumDetected = Nil;
                toolTip1.SetToolTip(this, string.Empty);
            }
        }

        /// <summary>Event handler to keep minimum size to assure all the pins
        /// are shown on screen.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void BitView_Resize(object sender, EventArgs e)
        {
            SuspendLayout();
            if (AutoSize)
            {
                MinimumSize = DefaultMinimumSize;
                if (Size.Width > MinimumSize.Width | Size.Height > MinimumSize.Height)
                    ClientSize = MinimumSize;
            }
            ResumeLayout(false);
        }
    }
}
