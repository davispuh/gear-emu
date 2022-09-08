/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * CogView.cs
 * Generic real-time cog information viewer
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
using System.Drawing.Text;
using System.Windows.Forms;

// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Gear.GUI
{
    /// @brief Generic real-time cog information viewer.
    public partial class CogView : PluginSupport.PluginBase
    {
        /// <summary>States for cog screen representation.</summary>
        private enum PresentationCogStateEnum
        {
            /// <summary>State not set.</summary>
            None = 0,
            /// <summary>Cog is not running.</summary>
            Stopped,
            /// <summary>Running in PASM Mode.</summary>
            NativeRunning,
            /// <summary>Running in Spin interpreted Mode.</summary>
            InterpretedRunning
        }

        /// <summary></summary>
        private const int StackMargin = 180;
        /// <summary>Maximum displayed lines of interpreted text.</summary>
        private const int MaxDisplayedLines = 80;

        /// <summary>Cog number identifier.</summary>
        private readonly int _hostId;

        /// <summary>Mono spaced font for plain text.</summary>
        private readonly Font _monoFont;
        /// <summary>Bold mono spaced font for highlighted text.</summary>
        private readonly Font _monoFontBold;

        /// <summary>Backing field for Bitmap buffer to draw the memory lines.</summary>
        private Bitmap _backBuffer;

        /// <summary>Backing field for Graphic style to draw text on buffer.</summary>
        /// @version v22.08.01 - Added.
        private Graphics _bufferGraphics;

        /// <summary>Backing field for Graphic style to draw text on
        /// main Panel.</summary>
        /// @version v22.08.01 - Added.
        private Graphics _mainGraphics;

        /// <summary></summary>
        private Brush _stringBrush;
        /// <summary></summary>
        private uint _stringCoordinateX;
        /// <summary></summary>
        private uint _stringCoordinateY;

        /// <summary></summary>
        private readonly uint[] _interpreterAddresses;

        /// <summary>Number of last line in NativeCog view.</summary>
        private uint _lastLine;

        /// <summary>State for buttons, related to running state and cog mode
        /// (Interpreted/PASM).</summary>
        /// @version v22.08.01 - Added.
        private PresentationCogStateEnum _presentationState;

        /// <summary></summary>
        private bool _displayAsHexadecimal;
        /// <summary></summary>
        private bool _useShortOpCodes;

        /// <summary>State of Video break point.</summary>
        private FrameState _breakVideo;

        /// @brief Title of the tab window.
        public override string Title => $"Cog {_hostId}";

        /// @brief Attribute to allow the window to be closed (default) or not (like cog windows).
        public override bool IsClosable => false;

        /// @brief Identify a plugin as user (=true) or system (=false).
        /// @version V15.03.26 - Added.
        public override bool IsUserPlugin => false;

        /// <summary>Referenced cog of this viewer plugin.</summary>
        /// <returns>Reference to a cog, or null.</returns>
        public Cog ReferencedCog => Chip?.GetCog(_hostId);

        /// <summary>Bitmap buffer property to draw the memory lines.</summary>
        /// @version v22.08.01 - Added as property to hold the relationship
        /// with MainGraphics property.
        private Bitmap BackBuffer
        {
            get => _backBuffer;
            set
            {
                if (_backBuffer == value | value == null)
                    return;
                _backBuffer = value;
                BufferGraphics = Graphics.FromImage(_backBuffer);
            }
        }

        /// <summary>Graphic style property to draw text and graphics
        /// on buffer.</summary>
        /// <remarks>Used to set the font aliasing style for text of the
        /// control.</remarks>
        /// @version v22.08.01 - Added.
        private Graphics BufferGraphics
        {
            get => _bufferGraphics;
            set
            {
                _bufferGraphics = value;
                _bufferGraphics.SmoothingMode = SmoothingMode.HighQuality;
                _bufferGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            }
        }

        /// <summary>Graphic style property to draw text and graphics
        /// on main Panel.</summary>
        /// @version v22.08.01 - Added.
        private Graphics MainGraphics
        {
            get => _mainGraphics ?? (_mainGraphics = assemblyPanel.CreateGraphics());
            set => _mainGraphics = value;
        }

        /// <summary></summary>
        /// @version v22.08.01 - Added.
        private PresentationCogStateEnum PresentationCogState
        {
            get => _presentationState;
            set
            {
                if (_presentationState == value)
                    return;
                _presentationState = value;
                SetStateOfControls();
            }
        }

        /// <summary>Default constructor.</summary>
        /// <param name="hostId">Id of related cog.</param>
        /// <param name="chip">Reference to Propeller instance.</param>
        public CogView(int hostId, PropellerCPU chip) : base (chip)
        {
            _hostId = hostId;
            _monoFont = new Font(FontFamily.GenericMonospace, 10);
            _monoFontBold = new Font(_monoFont, FontStyle.Bold);
            _interpreterAddresses = new uint[MaxDisplayedLines];
            _displayAsHexadecimal = false;
            _useShortOpCodes = true;
            _breakVideo = FrameState.None;
            InitializeComponent();
        }

        /// <summary>Set the status of CogView controls according to the
        /// current PresentationCogState.</summary>
        /// @version v22.08.01 - Added.
        private void SetStateOfControls()
        {
            switch (PresentationCogState)
            {
                case PresentationCogStateEnum.None:
                    break;
                case PresentationCogStateEnum.Stopped:
                    //actions tool strip controls
                    actionsToolStrip.Enabled = false;
                    OpCodeSize.Enabled = false;
                    //values tool strip controls
                    cogStateLabel.Text = @"Cog is stopped";
                    programCursorLabel.Text = @"---";
                    toolStripLabel3.Visible = false;
                    zeroFlagLabel.Visible = false;
                    toolStripSeparator5.Visible = false;
                    toolStripLabel4.Visible = false;
                    carryFlagLabel.Visible = false;
                    toolStripSeparator6.Visible = false;
                    //other controls in main panel
                    positionScroll.Enabled = false;
                    break;
                case PresentationCogStateEnum.NativeRunning:
                    //actions tool strip controls
                    actionsToolStrip.Enabled = true;
                    OpCodeSize.Enabled = false;
                    //values tool strip controls
                    toolStripLabel3.Visible = true;
                    zeroFlagLabel.Visible = true;
                    toolStripSeparator5.Visible = true;
                    toolStripLabel4.Visible = true;
                    carryFlagLabel.Visible = true;
                    toolStripSeparator6.Visible = true;
                    //other controls in main panel
                    positionScroll.Enabled = true;
                    break;
                case PresentationCogStateEnum.InterpretedRunning:
                    //actions tool strip controls
                    actionsToolStrip.Enabled = true;
                    OpCodeSize.Enabled = true;
                    //values tool strip controls
                    toolStripLabel3.Visible = false;
                    zeroFlagLabel.Visible = false;
                    toolStripSeparator5.Visible = false;
                    toolStripLabel4.Visible = false;
                    carryFlagLabel.Visible = false;
                    toolStripSeparator6.Visible = false;
                    //other controls in main panel
                    positionScroll.Enabled = true;
                    break;
                default:
                {
                    string msg = $"Value {PresentationCogState} not supported on {nameof(PresentationCogStateEnum)} enum.";
                    throw new ArgumentOutOfRangeException(nameof(PresentationCogState), PresentationCogState, msg);
                }
            }
        }

        /// <summary></summary>
        /// <param name="graph"></param>
        /// <param name="text">Text to draw on control.</param>
        private void DrawString(Graphics graph, string text)
        {
            graph.DrawString(text, _monoFont, _stringBrush,
                _stringCoordinateX, _stringCoordinateY);
            _stringCoordinateY += (uint)_monoFont.Height;
        }

        /// <summary>Display PASM decoded text for a Native %Cog</summary>
        /// <param name="force">Flag to indicate the intention to force the
        /// repaint.</param>
        /// <param name="cog"></param>
        /// @version v22.08.01 - Added the option to show decimal or
        /// hexadecimal representation of values. Changed method name from
        /// former `Repaint(bool, NativeCog)`.
        private void DisplayNativeCog(bool force, NativeCog cog)
        {
            const uint topLine = 5;
            BufferGraphics.Clear(SystemColors.Control);
            zeroFlagLabel.Text = $@"{cog.ZeroFlag}";
            carryFlagLabel.Text = $@"{cog.CarryFlag}";
            uint bottomLine = (uint)(ClientRectangle.Height / _monoFont.Height - 5);

            for (uint i = (uint)positionScroll.Value, y = 0, line = 1;
                y < ClientRectangle.Height;
                y += (uint)_monoFont.Height, i++, line++)
            {
                if (i >= Cog.TotalCogMemory)
                    continue;
                uint memoryValue = cog[(int)i];
                string display;
                if (memoryViewButton.Checked)
                {
                    string binary = Convert.ToString(memoryValue, 2);
                    while (binary.Length < sizeof(uint) * 8)
                        binary = $"0{binary}";
                    display = $"${i:X3}:  {memoryValue:X8}   {binary}   {memoryValue}";
                }
                else
                    display = $"${i:X3}:  {memoryValue:X8}   {InstructionDisassembler.AssemblyText(memoryValue, _displayAsHexadecimal)}";
                Brush brush;
                if ((uint)positionScroll.Value + line - 1 == cog.BreakPointCogCursor)
                    brush = Brushes.Pink;
                else if (!followPCButton.Checked || line <= topLine || line >= bottomLine)
                    brush = SystemBrushes.Control;
                else
                    brush = SystemBrushes.Window;
                BufferGraphics.FillRectangle(brush, 0, y, assemblyPanel.Width, y + _monoFont.Height);
                BufferGraphics.DrawString(
                    display,
                    cog.ProgramCursor == i ? _monoFontBold : _monoFont,
                    SystemBrushes.ControlText, 0, y);
            }
        }

        /// <summary>Display values of main memory for Interpreted %Cog.</summary>
        /// <param name="force">Flag to indicate the intention to force the
        /// repaint.</param>
        /// <param name="cog"></param>
        /// @version v22.08.01 - Added from splitting of former method
        /// `Repaint(bool, InterpretedCog)`. Corrected problem of show cog
        /// memory contents instead of main memory values to be interpreted.
        private void DisplayMemoryForInterpretedCog(bool force, InterpretedCog cog)
        {
            BufferGraphics.Clear(SystemColors.Control);
            for (uint i = (uint)positionScroll.Value, y = 0;
                 y < ClientRectangle.Height;
                 y += (uint)_monoFont.Height, i++)
            {
                if (i > PropellerCPU.MaxMemoryAddress)
                    continue;
                byte mem = Chip.DirectReadByte(i);
                string binary = Convert.ToString(mem, 2);
                while (binary.Length < sizeof(byte) * 8)
                    binary = $"0{binary}";
                string display = $"${i:X4}:  ${mem:X2}   {binary}   " + (_displayAsHexadecimal ? $"{mem:X2}" : $"{mem,3:D}");
                BufferGraphics.FillRectangle(SystemBrushes.Control, 0, y, assemblyPanel.Width, y + _monoFont.Height);
                BufferGraphics.DrawString(
                    display,
                    cog.ProgramCursor == i ? _monoFontBold : _monoFont,
                    SystemBrushes.ControlText, 0, y);
            }
        }

        /// <summary>Display decoded text for Interpreted %Cog.</summary>
        /// <param name="force">Flag to indicate the intention to force the
        /// repaint.</param>
        /// <param name="cog"></param>
        /// @version v22.08.01 - Added from splitting of former method
        /// `Repaint(bool, InterpretedCog)`. Changed local variable name to
        /// clarify meaning of it.
        private void DisplayDecodedForInterpretedCog(bool force, InterpretedCog cog)
        {
            const uint topLine = 5;
            uint bottomLine = (uint)(ClientRectangle.Height / _monoFont.Height - 5);
            uint y = 0;
            BufferGraphics.Clear(SystemColors.Control);
            for (uint i = (uint)positionScroll.Value, line = 1;
                 y < ClientRectangle.Height;
                 y += (uint)_monoFont.Height, line++)
            {
                if (i > PropellerCPU.MaxMemoryAddress)
                    continue;
                uint start = i;
                Propeller.MemorySegment memorySegment = new Propeller.MemorySegment(Chip, i);
                string decoded = InstructionDisassembler.InterpreterText(memorySegment, _displayAsHexadecimal, _useShortOpCodes);
                i = memorySegment.Address;
                string display = $"${start:X4}: ";
                _interpreterAddresses[line] = start;
                for (uint q = start; q < start + 4; q++)
                {
                    if (q < i)
                    {
                        byte b = Chip.DirectReadByte(q);
                        display += $" ${b:X2}";
                    }
                    else
                        display += "    ";
                }
                display += $"  {decoded}";
                Brush brush;
                if (_interpreterAddresses[line] == cog.BreakPointCogCursor)
                    brush = Brushes.Pink;
                else if (!followPCButton.Checked || line <= topLine || line >= bottomLine)
                    brush = SystemBrushes.Control;
                else
                    brush = SystemBrushes.Window;
                BufferGraphics.FillRectangle(brush, 0, y, assemblyPanel.Width,
                    y + _monoFont.Height);
                BufferGraphics.DrawString(
                    display,
                    cog.ProgramCursor == start ? _monoFontBold : _monoFont,
                    SystemBrushes.ControlText, 0, y);
            }
            //draw object details
            _stringBrush = SystemBrushes.ControlText;
            _stringCoordinateY = 0;
            _stringCoordinateX = (uint)(assemblyPanel.Width - StackMargin);
            DrawString(BufferGraphics,
                $"@Stk[0] = ${cog.StackFrame:X4} {cog.StackFrame}");
            DrawString(BufferGraphics,
                $"@Obj[0] = ${cog.ObjectFrame:X4} {cog.ObjectFrame}");
            DrawString(BufferGraphics,
                $"@Loc[0] = ${cog.LocalFrame:X4} {cog.LocalFrame}");
            DrawString(BufferGraphics,
                $"@Var[0] = ${cog.VariableFrame:X4} {cog.VariableFrame}");
            BufferGraphics.DrawLine(Pens.Black, assemblyPanel.Width - StackMargin,
                _stringCoordinateY, assemblyPanel.Width, _stringCoordinateY);
            DrawString(BufferGraphics,
                $"Caller& = ${Chip.DirectReadWord(cog.LocalFrame - 8):X4} " +
                $"{Chip.DirectReadWord(cog.LocalFrame - 8)}");
            DrawString(BufferGraphics,
                $"          ${Chip.DirectReadWord(cog.LocalFrame - 6):X4} " +
                $"{Chip.DirectReadWord(cog.LocalFrame - 6)}");
            DrawString(BufferGraphics,
                $"          ${Chip.DirectReadWord(cog.LocalFrame - 4):X4} " +
                $"{Chip.DirectReadWord(cog.LocalFrame - 4)}");
            DrawString(BufferGraphics,
                $"Return& = ${Chip.DirectReadWord(cog.LocalFrame - 2):X4}");
            BufferGraphics.DrawLine(Pens.Black, assemblyPanel.Width - StackMargin,
                _stringCoordinateY, assemblyPanel.Width, _stringCoordinateY);
            for (uint i = cog.LocalFrame;
                 i < cog.StackFrame && _stringCoordinateY < ClientRectangle.Height;
                 i += 4)
                DrawString(BufferGraphics,
                    $"${(int)Chip.DirectReadLong(i):X8}  {(int)Chip.DirectReadLong(i)}");
        }

        /// <summary>Repaint the Cog state and data.</summary>
        /// <param name="force">Flag to indicate the intention to force the
        /// repaint.</param>
        public override void Repaint(bool force)
        {
            Cog cog = ReferencedCog;
            if (cog == null)
            {
                PresentationCogStateEnum oldValue = PresentationCogState;
                PresentationCogState = PresentationCogStateEnum.Stopped;
                if (oldValue != PresentationCogStateEnum.None)
                {
                    MainGraphics.Clear(SystemColors.Control);
                    BufferGraphics.Clear(SystemColors.Control);
                }
                return;
            }
            //update values
            cogStateLabel.Text = $@"{cog.CogStateString}";
            programCursorLabel.Text = $@"${cog.ProgramCursor:X4}";
            frameCountLabel.Text = $@"{cog.VideoFrameString}";
            //set position scroll
            switch (cog)
            {
                case InterpretedCog _:
                    positionScroll.Maximum = PropellerCPU.MaxMemoryAddress;
                    break;
                case NativeCog _:
                    positionScroll.Maximum = Cog.TotalCogMemory;
                    break;
            }
            positionScroll.LargeChange = 10;
            positionScroll.SmallChange = 1;
            if (positionScroll.Maximum < positionScroll.Value)
                positionScroll.Value = positionScroll.Maximum;
            if (followPCButton.Checked)
            {
                const uint topLine = 5;
                var bottomLine = (uint)(ClientRectangle.Height / _monoFont.Height - 5);
                if (cog is NativeCog hostNativeCog)
                {
                    if (hostNativeCog.ProgramCursor < topLine)
                        positionScroll.Value = 0;
                    else if (hostNativeCog.ProgramCursor - positionScroll.Value >= bottomLine - 1)
                        positionScroll.Value = (int)hostNativeCog.ProgramCursor - (int)topLine;
                    else if (hostNativeCog.ProgramCursor - positionScroll.Value < topLine)
                        positionScroll.Value = (int)hostNativeCog.ProgramCursor - (int)topLine;
                }
                else
                    positionScroll.Value = (int)cog.ProgramCursor;
            }
            //draw specific data based on cog type
            switch (cog)
            {
                case NativeCog nativeCog:
                    PresentationCogState = PresentationCogStateEnum.NativeRunning;
                    DisplayNativeCog(force, nativeCog);
                    break;
                case InterpretedCog interpretedCog:
                    PresentationCogState = PresentationCogStateEnum.InterpretedRunning;
                    if (memoryViewButton.Checked)
                        DisplayMemoryForInterpretedCog(force, interpretedCog);
                    else
                        DisplayDecodedForInterpretedCog(force, interpretedCog);
                    break;
            }
            MainGraphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Scroll event data arguments.</param>
        private void UpdateOnScroll(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.08.01 - Changed method name to clarify its meaning.
        private void AssemblyPanel_SizeChanged(object sender, EventArgs e)
        {
            if (assemblyPanel.Width > 0 && assemblyPanel.Height > 0)
                BackBuffer = new Bitmap(assemblyPanel.Width, assemblyPanel.Height);
            else
                BackBuffer = new Bitmap(1, 1);
            //force MainGraphics to recalculate on next get value
            MainGraphics = null;
            Repaint(false);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Paint event data arguments.</param>
        private void AssemblyView_Paint(object sender, PaintEventArgs e)
        {
            MainGraphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Mouse event data arguments.</param>
        private void AssemblyPanel_MouseClick(object sender, MouseEventArgs e)
        {
            positionScroll.Focus();
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Mouse event data arguments.</param>
        private void AssemblyPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(ReferencedCog is NativeCog nativeCog))
                return;
            uint line = (uint)positionScroll.Value + (uint)(e.Y / _monoFont.Height);
            if (line >= Cog.TotalCogMemory)
                return;
            //Update tooltip only if line has change to prevent flickering
            if (line == _lastLine)
                return;
            var mem = nativeCog.ReadLong(line);
            toolTip1.SetToolTip(assemblyPanel,
                $"${mem >> 9 & Cog.MaskCogMemory:x3}= " +
                $"${nativeCog.ReadLong(mem >> 9 & Cog.MaskCogMemory):x8}, " +
                $"{nativeCog.ReadLong(mem >> 9 & Cog.MaskCogMemory)}\n" +
                $"${mem & Cog.MaskCogMemory:x3}= " +
                $"${nativeCog.ReadLong(mem & Cog.MaskCogMemory):x8}, " +
                $"{nativeCog.ReadLong(mem & Cog.MaskCogMemory)}"
            );
            _lastLine = line;
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Mouse event data arguments.</param>
        private void AssemblyPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left)
                return;
            //Make sure it's a valid breakpoint environment
            Cog cog = ReferencedCog;
            if (cog == null)
                return;
            //Find the line that was clicked on
            int lineNumber = assemblyPanel.PointToClient(MousePosition).Y / _monoFont.Height;
            switch (cog)
            {
                //What type of cog?
                case NativeCog _:
                    lineNumber += positionScroll.Value;
                    break;
                case InterpretedCog _:
                    lineNumber = (int)_interpreterAddresses[lineNumber + 1];
                    break;
            }
            //Toggle/move the breakpoint
            if (lineNumber == cog.BreakPointCogCursor)
                cog.BreakPointCogCursor = -1;
            else
                cog.BreakPointCogCursor = lineNumber;
            //Show the user what happened
            Repaint(false);
        }

        /// <summary> </summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void MemoryViewButton_Click(object sender, EventArgs e)
        {
            Repaint(false);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void FollowPCButton_Click(object sender, EventArgs e)
        {
            Repaint(false);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void HexadecimalUnits_Click(object sender, EventArgs e)
        {
            if (!displayUnitsBtn.Enabled)
                return;
            _displayAsHexadecimal = true;
            hexadecimalUnits.Checked = true;
            decimalUnits.Checked = false;
            displayUnitsBtn.Text = @"Units: Hex";
            Repaint(false);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void DecimalUnits_Click(object sender, EventArgs e)
        {
            if (!displayUnitsBtn.Enabled)
                return;
            _displayAsHexadecimal = false;
            hexadecimalUnits.Checked = false;
            decimalUnits.Checked = true;
            displayUnitsBtn.Text = @"Units: Dec";
            Repaint(false);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void LongOpCodes_Click(object sender, EventArgs e)
        {
            if (!OpCodeSize.Enabled)
                return;
            _useShortOpCodes = false;
            longOpcodes.Checked = true;
            shortOpcodes.Checked = false;
            OpCodeSize.Text = @"Opcodes: Long";
            Repaint(false);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void ShortOpCodes_Click(object sender, EventArgs e)
        {
            if (!OpCodeSize.Enabled)
                return;
            _useShortOpCodes = true;
            longOpcodes.Checked = false;
            shortOpcodes.Checked = true;
            OpCodeSize.Text = @"Opcodes: Short";
            Repaint(false);
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.08.01 - Corrected inconsistency when breakAll is
        /// selected, but breakNone was marked. Removed exception thrown,
        /// because a null sender object is really permitted.
        private void VideoBreak_Click(object sender, EventArgs e)
        {
            if (sender == null)
                return;
            if (sender == breakNone)
            {
                breakNone.Checked = true;
                breakMiss.Checked = false;
                breakAll.Checked = false;
                _breakVideo = FrameState.Miss;
                FrameBreakMode.Text = @"Video Break: None";
            }
            if (sender == breakMiss)
            {
                breakNone.Checked = false;
                breakMiss.Checked = true;
                breakAll.Checked = false;
                _breakVideo = FrameState.Miss;
                FrameBreakMode.Text = @"Video Break: Miss";
            }
            if (sender == breakAll)
            {
                breakNone.Checked = false;
                breakMiss.Checked = false;
                breakAll.Checked = true;
                _breakVideo = FrameState.None;
                FrameBreakMode.Text = @"Video Break: End";
            }
            ReferencedCog?.SetVideoBreak(_breakVideo);
        }
    }
}
