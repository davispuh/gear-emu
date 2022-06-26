/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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

using Gear.EmulationCore;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief Main memory viewer.
    public partial class MemoryView : PluginSupport.PluginBase
    {
        /// <summary>Bitmap buffer to draw the memory lines.</summary>
        /// @version v22.06.02 - Name changed to follow naming conventions.
        private Bitmap _backBuffer;

        /// <summary>Graphic style to draw text.</summary>
        /// @version v22.06.02 - Added.
        private Graphics _mainGraphics;

        /// <summary>Pen style to draw lines.</summary>
        /// @version v22.06.02 - Added.
        private readonly Pen _defaultPen;

        /// <summary>Bitmap buffer property to draw the memory lines.</summary>
        /// @version v22.06.02 - Added as property to hold the relationship
        /// with MainGraphics property.
        private Bitmap BackBuffer
        {
            get => _backBuffer;
            set
            {
                if (_backBuffer == value | value == null)
                    return;
                _backBuffer = value;
                MainGraphics = Graphics.FromImage(_backBuffer);
            }
        }

        /// <summary>Graphic style property to draw text.</summary>
        /// @version v22.06.02 - Added as property to set the font
        /// aliasing style for text of the control.
        private Graphics MainGraphics
        {
            get => _mainGraphics;
            set
            {
                _mainGraphics = value;
                _mainGraphics.SmoothingMode = SmoothingMode.HighQuality;
            }
        }

        /// <summary>Title of the tab window.</summary>
        public override string Title => "Main Memory";

        /// <summary>Attribute to allow the window to be closed (default) or
        /// not (like cog windows).</summary>
        public override bool IsClosable => false;

        /// <summary>Identify a plugin as user (=true) or system (=false).</summary>
        public override bool IsUserPlugin => false;

        /// <summary>Default Constructor.</summary>
        /// <param name="chip"></param>
        /// @issue{30} Linux-Mono: Version 22.06.02 crashes directly after loading a binary.
        /// @version v22.06.03 - Hotfix for issue #30.
        public MemoryView(PropellerCPU chip) : base(chip)
        {
            _defaultPen = new Pen(Color.Black, 1);
            InitializeComponent();
        }

        /// <summary>Register the events to be notified to this plugin.</summary>
        public override void PresentChip() { }

        /// <summary>Event to repaint the plugin screen (if used).</summary>
        /// <param name="force">Flag to indicate the intention to force the repaint.</param>
        /// @version v22.06.01 - Modified to minimize the rectangle paints,
        /// changed local variable names to clarify their meaning and to use
        /// constant `PropellerCPU.TotalMemory`. Added header to clarify the
        /// meaning of each column.
        public override void Repaint(bool force)
        {
            if (Chip == null)
                return;
            byte[] valueAsByte = new byte[4];
            //clear all panel
            MainGraphics.FillRectangle(SystemBrushes.Control, 0, 0,
                memoryPanel.Width, memoryPanel.Height);
            //draw the header
            MainGraphics.DrawLine(_defaultPen, 0, Font.Height + 1,
                memoryPanel.Width, Font.Height + 1);
            MainGraphics.DrawString(
                "Address     :   +0  +1  +2  +3  :\t WORD0\t WORD1 :\tSWORD0\tSWORD1\t:\tSIGNED LONG",
                Font, SystemBrushes.ControlText, 0, 0);
            //draw all other lines
            for (int idx = positionScrollBar.Value, linePosition = Font.Height + 2;
                 idx < PropellerCPU.TotalMemory && linePosition < memoryPanel.Height;
                 idx += 4, linePosition += Font.Height)
            {
                for (int byteGroupIndex = 0; byteGroupIndex < 4; byteGroupIndex++)
                    valueAsByte[byteGroupIndex] = Chip[idx + byteGroupIndex];
                ushort valueAsUnsignedWord1 = (ushort)(valueAsByte[0] | (valueAsByte[1] << 8));
                ushort valueAsUnsignedWord2 = (ushort)(valueAsByte[2] | (valueAsByte[3] << 8));
                int valueAsSignedLong = valueAsUnsignedWord1 | (valueAsUnsignedWord2 << 16);
                MainGraphics.DrawString(
                    $"${idx:X4} {idx,5:D} :  ${valueAsByte[0]:X2} ${valueAsByte[1]:X2} ${valueAsByte[2]:X2} ${valueAsByte[3]:X2}  :" +
                    $"\t{valueAsUnsignedWord1,6}\t{valueAsUnsignedWord2,6} :\t" +
                    $"{(short)valueAsUnsignedWord1,6}\t{(short)valueAsUnsignedWord2,6} :\t" +
                    $"{valueAsSignedLong,11:d}",
                    Font, SystemBrushes.ControlText, 0, linePosition);
            }
            memoryPanel.CreateGraphics().DrawImageUnscaled(BackBuffer, 0, 0);
        }

        /// <summary>Event handler to paint memory panel.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Paint event data arguments.</param>
        /// @version v22.06.01 - Method name changed to clarify its meaning.
        private void MemoryPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }

        /// <summary>Event Handler on size changed of Memory panel.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.01 - Method name changed to clarify its meaning and
        /// modified to use constant `PropellerCPU.TotalMemory`.
        private void MemoryPanel_SizeChange(object sender, EventArgs e)
        {
            positionScrollBar.Maximum = PropellerCPU.TotalMemory - memoryPanel.Height / Font.Height;

            if (memoryPanel.Width > 0 && memoryPanel.Height > 0)
                BackBuffer = new Bitmap(
                    memoryPanel.Width,
                    memoryPanel.Height);
            else
                BackBuffer = new Bitmap(1, 1);
            Repaint(false);
        }

        /// <summary>Event Handler on position changed of scroll bar.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Scroll event data arguments.</param>
        /// @version v22.06.01 - Method name changed to clarify its meaning.
        private void PositionScrollBar_PositionChanged(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        /// <summary>Event Handler on mouse click on Memory panel.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Mouse event data arguments.</param>
        private void MemoryPanel_MouseClick(object sender, MouseEventArgs e)
        {
            positionScrollBar.Focus();
        }
    }
}
