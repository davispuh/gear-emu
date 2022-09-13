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
using Gear.Propeller;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Gear.GUI
{
    /// <summary>Generic real-time cog information viewer.</summary>
    /// @version v22.09.01 - Added custom debugger text.
    [DefaultProperty("Name"), DebuggerDisplay("{TextForDebugger,nq}")]
    public partial class CogView : PluginSupport.PluginBase
    {
        /// <summary>States for cog screen representation.</summary>
        /// @version v22.08.01 - Added.
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

        /// <summary>Types of highlight of active line.</summary>
        /// @version v22.09.01 - Added.
        [Flags]
        public enum HighlightedTypeEnum : byte
        {
            /// <summary>Only Bold on text line.</summary>
            OnlyBold = 1,
            /// <summary>Only Frame around text line.</summary>
            OnlyFrame = 2,
            /// <summary> Combination of Bold text and Frame around text
            /// line.</summary>
            BoldAndFrame = 3
        }

        /// <summary> Max number of characters for width of interpreted
        /// details pane.</summary>
        private const int CharWidthInterpreterDetails = 24;

        /// <summary>Cog number identifier.</summary>
        private readonly int _hostId;

        /// <summary>Mono spaced font for plain text.</summary>
        private readonly Font _monoFont;
        /// <summary>Bold mono spaced font for highlighted text.</summary>
        private readonly Font _monoFontBold;

        /// <summary>Line height of Mono font.</summary>
        /// @version v22.09.01 - Added
        private readonly int _lineHeight;

        /// <summary>Width on pixels of stack details for a SPIN cog.</summary>
        /// @version v22.09.01 - Added
        private readonly int _stackMargin;

        /// <summary></summary>
        /// @version v22.09.01 - Added
        private int _decodedEffectiveWidth;

        /// <summary></summary>
        /// @version v22.09.01 - Added
        private readonly BufferedGraphicsContext _bufferedGraphicsContext =
            new BufferedGraphicsContext();

        /// <summary>Array of starting address of each line of a interpreted
        /// cog.</summary>
        /// @version v22.09.01 - Changed member name to clarify its meaning,
        /// from former `_interpreterAddresses`.
        private uint[] _interpreterAddressLines;

        /// <summary>Array of length of memory bytes used by each line of a
        /// interpreted cog.</summary>
        /// @version v22.09.01 - Added.
        private byte[] _interpretedLengthLines;

        /// <summary>Memory address of last line hovered by mouse in NativeCog
        /// view.</summary>
        /// @version v22.09.01 - Changed name from `_lastLine`, to clarify the
        /// true concept of it.
        private uint _oldMemoryPosHovered;

        /// <summary>Backing field for state of buttons, related to running
        /// state and cog mode (Interpreted/PASM).</summary>
        /// @version v22.08.01 - Added.
        private PresentationCogStateEnum _presentationState;

        /// <summary>Backing field for Flag to display decoded program values
        /// as Hexadecimal, or Decimal.</summary>
        private bool _displayAsHexadecimal;
        /// <summary>Flag to display short decoded SPIN operations, or
        /// long ones.</summary>
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

        /// <summary>Double buffer property to draw on it.</summary>
        /// @version v22.09.01 - Modified to use double buffering in a
        /// independent way for each cog.
        private BufferedGraphics BackBuffer { get; set; }

        /// <summary>Graphic style property to draw text and graphics
        /// on buffer.</summary>
        /// @version v22.09.01 - Modified to use implicit value.
        private Graphics BufferGraphics { get; set; }

        /// <summary>State for buttons, related to running state and cog mode
        /// (Interpreted/PASM).</summary>
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

        /// <summary>Type of highlight for program cursor line.</summary>
        /// <remarks>In order this property could be bind with the
        /// corresponding property setting, it must be public and must have a
        /// setter.</remarks>
        /// @version v22.09.01 - Added.
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public HighlightedTypeEnum HighlightedType { get; set; }

        /// <summary>Flag to display decoded program values
        /// as Hexadecimal, or Decimal.</summary>
        /// @version v22.09.01 - Added.
        private bool DisplayAsHexadecimal
        {
            get => _displayAsHexadecimal;
            set
            {
                if (_displayAsHexadecimal == value)
                    return;
                _displayAsHexadecimal = value;
                SetControlsOfDisplayAsHexadecimal();
            }
        }

        /// <summary>Returns a summary text of this class, to be used in
        /// debugger view.</summary>
        /// @version v22.09.01 - Added.
        private string TextForDebugger =>
            $"{{{GetType().FullName}, Cog: {_hostId:D}, Mode: {_presentationState}}}";

        /// <summary>Default constructor.</summary>
        /// <param name="hostId">Id of related cog.</param>
        /// <param name="chip">Reference to Propeller instance.</param>
        /// @version v22.09.01 - Modified to use DisplayAsHexadecimal and
        /// HighlightedType properties.
        public CogView(int hostId, PropellerCPU chip) : base (chip)
        {
            _hostId = hostId;
            _monoFont = new Font(FontFamily.GenericMonospace, 10);
            _monoFontBold = new Font(_monoFont, FontStyle.Bold);
            _lineHeight = _monoFont.Height;
            _stackMargin = TextRenderer.MeasureText(
                new string('0', CharWidthInterpreterDetails), _monoFont).Width + 2;
            //bonded properties
            HighlightedType = Properties.Settings.Default.PCHighlightedType;
            DataBindings.Add(new Binding("HighlightedType",
                Properties.Settings.Default, "PCHighlightedType",
                false, DataSourceUpdateMode.OnPropertyChanged));
            _useShortOpCodes = true;
            _breakVideo = FrameState.None;
            InitializeComponent();
            ResetBufferGraphics();
            _decodedEffectiveWidth =
                decodedPanel.ClientSize.Width - _stackMargin - 1;
            //recover default value from user properties
            bool defaultValueHex = Properties.Settings.Default.ValuesShownAsHex;
            //assure the value is changed, using different values on backing
            // member and property
            _displayAsHexadecimal = !defaultValueHex;
            DisplayAsHexadecimal = defaultValueHex;
            positionScroll.Enabled = !followPCButton.Checked;
        }

        /// <summary>Assign double buffering and graphic style to decoded
        /// panel.</summary>
        /// <remarks>Used to set the font aliasing style for text of the
        /// control.</remarks>
        /// @version v22.09.01 - Added as method from former setter of
        /// BackBuffer property.
        private void ResetBufferGraphics()
        {
            BackBuffer = _bufferedGraphicsContext.Allocate(
                decodedPanel.CreateGraphics(),
                decodedPanel.DisplayRectangle);
            BufferGraphics = BackBuffer.Graphics;
            BufferGraphics.SmoothingMode = SmoothingMode.HighQuality;
            BufferGraphics.TextRenderingHint =
                TextRenderingHint.ClearTypeGridFit;
        }

        /// <summary>Check if required length is met for a array variable,
        /// else it will resize the array to new length.</summary>
        /// <remarks>Only increments the array size if it does not fit, so if
        /// the array is already bigger, it will be maintained.</remarks>
        /// <typeparam name="T">Type of array variable.</typeparam>
        /// <param name="variable">Variable to be resized if necessary.</param>
        /// <param name="requiredLength">Required length of array.</param>
        /// @version v22.09.01 - Added.
        private void CheckRequiredArrayLength<T>(ref T[] variable,
            int requiredLength)
        {
            if (variable == null || variable.Length < requiredLength)
                variable = new T[requiredLength];
        }

        /// <summary>Set the status of CogView controls according to the
        /// current PresentationCogState.</summary>
        /// @version v22.09.01 - Modified to enable the position scroll
        /// according of follow program button.
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
                    positionScroll.Enabled = !followPCButton.Checked;
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
                    positionScroll.Enabled = !followPCButton.Checked;
                    break;
                default:
                {
                    string msg = $"Value {PresentationCogState} not supported on {nameof(PresentationCogStateEnum)} enum.";
                    throw new ArgumentOutOfRangeException(nameof(PresentationCogState), PresentationCogState, msg);
                }
            }
        }

        /// <summary>Set the status of Display Units Button according to
        /// Flag of DisplayAsHexadecimal value.</summary>
        /// @version v22.09.01 - Added.
        private void SetControlsOfDisplayAsHexadecimal()
        {
            hexadecimalUnits.Checked = DisplayAsHexadecimal;
            decimalUnits.Checked = !DisplayAsHexadecimal;
            displayUnitsBtn.Text = DisplayAsHexadecimal ?
                @"Units: Hex" :
                @"Units: Dec";
        }

        /// <summary>Calculate the number of lines that fits on decoded panel.</summary>
        /// <returns>Number of lines.</returns>
        /// @version v22.09.01 - Added.
        private int GetLinesOfDecodedPanel() =>
            decodedPanel.ClientSize.Height / _lineHeight;

        /// <summary>Display PASM decoded text for a Native cog.</summary>
        /// <param name="force">Flag to indicate the intention to force the
        /// repaint.</param>
        /// <param name="cog">Native cog reference.</param>
        /// @version v22.09.01 - Refactored and changed to enable parallel
        /// drawing.
        private void DisplayNativeCog(bool force, NativeCog cog)
        {
            //change background when following program cursor
            BufferGraphics.Clear(followPCButton.Checked ?
                SystemColors.Window :
                SystemColors.Control);
            //update values on valuesToolStrip
            zeroFlagLabel.Text = $@"{cog.ZeroFlag}";
            carryFlagLabel.Text = $@"{cog.CarryFlag}";
            int memoryPos = positionScroll.Value;
            //get physical number of lines that fits on panel
            int totalLines = Math.Min(
                GetLinesOfDecodedPanel(),
                Cog.TotalCogMemory - memoryPos);
            string formatForValue = DisplayAsHexadecimal ?
                "${0:X8}" :
                "{0:D10}";
            //loop to draw lines
            for (int line = 0; line < totalLines; line++)
                DrawNativeLine(cog, memoryPos + line, line,
                    formatForValue);
        }

        /// <summary>Draw PASM decoded text line.</summary>
        /// <param name="cog">Native cog reference.</param>
        /// <param name="memoryPos">Memory position to print decoded value.</param>
        /// <param name="line">Line number where to draw.</param>
        /// <param name="formatForValue">String with format for memory
        /// contents, hex or decimal.</param>
        /// @version v22.09.01 - Added.
        private void DrawNativeLine(NativeCog cog, int memoryPos, int line,
            string formatForValue)
        {
            uint memoryValue = cog[memoryPos];
            string textToDisplay = memoryViewButton.Checked ?
                $"${memoryPos:X3}:  {string.Format(formatForValue, memoryValue)}   {InstructionDisassembler.BinaryRepresentationText(memoryValue)}" :
                $"${memoryPos:X3}:  ${memoryValue:X8}   {InstructionDisassembler.AssemblyText(memoryValue, DisplayAsHexadecimal)}";
            //change background on breakpoint line
            if (memoryPos == cog.BreakPointCogCursor)
                BufferGraphics.FillRectangle(Brushes.Pink, 0,
                    line * _lineHeight, decodedPanel.ClientSize.Width,
                    _lineHeight);
            //Draw frame on line if is the same of program cursor
            if (cog.ProgramCursor == memoryPos &&
                (HighlightedType & HighlightedTypeEnum.OnlyFrame) ==
                HighlightedTypeEnum.OnlyFrame)
                BufferGraphics.DrawRectangle(Pens.Black,
                    0, line * _lineHeight,
                    decodedPanel.ClientSize.Width - 1,
                    _lineHeight);
            //print text line
            BufferGraphics.DrawString(
                textToDisplay,
                cog.ProgramCursor == memoryPos &&
                (HighlightedType & HighlightedTypeEnum.OnlyBold) ==
                HighlightedTypeEnum.OnlyBold ?
                    _monoFontBold :
                    _monoFont,
                SystemBrushes.ControlText, 0, line * _lineHeight);
        }

        /// <summary>Display values of main memory for Interpreted cog.</summary>
        /// <param name="force">Flag to indicate the intention to force the
        /// repaint.</param>
        /// <param name="cog">Interpreted cog reference.</param>
        /// @version v22.09.01 - Refactored and changed to enable parallel
        /// drawing.
        private void DisplayMemoryForInterpretedCog(bool force, InterpretedCog cog)
        {
            //change background when following program cursor
            BufferGraphics.Clear(followPCButton.Checked ?
                SystemColors.Window :
                SystemColors.Control);
            uint memoryPos = (uint)positionScroll.Value;
            //get physical number of lines that fits on panel
            int totalLines = GetLinesOfDecodedPanel();
            //and assure array length sufficient for that physical number of lines
            CheckRequiredArrayLength(ref _interpreterAddressLines, totalLines);
            CheckRequiredArrayLength(ref _interpretedLengthLines, totalLines);
            //but if we are at the upper end of memory, the view could show
            // fewer lines without resizing the array, thus reducing memory
            // allocation events => faster redraw.
            totalLines = (int)Math.Min(
                totalLines,
                PropellerCPU.TotalRAM - memoryPos);
            MemorySegment memorySegment = new MemorySegment(Chip, memoryPos);
            string formatForValue = DisplayAsHexadecimal ? "${0:X2}" : "{0,3:D}";
            //parse lines and length
            for (uint line = 0; line < totalLines; line++)
            {
                _interpreterAddressLines[line] = memorySegment.Address;
                _interpretedLengthLines[line] =
                    (byte)InstructionDisassembler.InterpretedInstructionLength(memorySegment);
            }
            //draw nothing if there is no effective space for decoded lines
            if (_decodedEffectiveWidth <= 0)
                return;
            //loop to draw lines
            for (int line = 0; line < totalLines; line++)
                DrawInterpretedMemoryLine(cog,
                    _interpreterAddressLines[line],
                    _interpretedLengthLines[line], line, formatForValue);
        }

        /// <summary>Draw SPIN memory decoded text line.</summary>
        /// <remarks>Shows the main RAM memory range.</remarks>
        /// <param name="cog">Interpreted cog reference.</param>
        /// <param name="memoryPos">Memory position to print decoded value.</param>
        /// <param name="length">Length of decoded instruction, in bytes.</param>
        /// <param name="line">Line number where to draw.</param>
        /// <param name="formatForValue">String with format for memory
        /// contents, hex or decimal.</param>
        /// @version v22.09.01 - Added.
        private void DrawInterpretedMemoryLine(InterpretedCog cog,
            uint memoryPos, byte length, int line, string formatForValue)
        {
            //generate memory and binary values as text field: 1 to 5 bytes long
            string memoryText = string.Empty;
            string binaryText = string.Empty;
            for (uint i = 0; i < length; i++)
            {
                byte memoryValue = Chip.DirectReadByte(memoryPos + i);
                string interSpace = i > 0 ? " " : string.Empty;
                memoryText += $"{interSpace}{string.Format(formatForValue, memoryValue)}";
                binaryText += $"{interSpace}{InstructionDisassembler.NumberToBinary(memoryValue)}";
            }
            //padding of memory text
            //19 = 3 * 5 + 4 = <value length> * <max bytes> - (<max bytes> - 1)
            int fillQty = 19 - memoryText.Length;
            if (fillQty > 0)
                memoryText += new string(' ', fillQty);
            //change background on breakpoint line
            if (memoryPos == cog.BreakPointCogCursor)
                BufferGraphics.FillRectangle(Brushes.Pink, 0,
                    line * _lineHeight, decodedPanel.ClientSize.Width,
                    _lineHeight);
            //Draw frame on line if is the same of program cursor
            if (cog.ProgramCursor == memoryPos &&
                (HighlightedType & HighlightedTypeEnum.OnlyFrame) ==
                HighlightedTypeEnum.OnlyFrame)
                BufferGraphics.DrawRectangle(Pens.Black,
                    0, line * _lineHeight, _decodedEffectiveWidth,
                    _lineHeight);
            //print text line
            BufferGraphics.DrawString(
                $"${memoryPos:X4}:  {memoryText}  {binaryText}",
                cog.ProgramCursor == memoryPos &&
                (HighlightedType & HighlightedTypeEnum.OnlyBold) ==
                HighlightedTypeEnum.OnlyBold ?
                    _monoFontBold :
                    _monoFont,
                SystemBrushes.ControlText, 0, line * _lineHeight);
        }

        /// <summary>Display decoded text for SPIN Interpreted cog.</summary>
        /// <remarks>Shows the main RAM memory range.</remarks>
        /// <param name="force">Flag to indicate the intention to force the
        /// repaint.</param>
        /// <param name="cog">Interpreted cog reference.</param>
        /// @version v22.09.01 - Refactored.
        private void DisplayDecodedForInterpretedCog(bool force, InterpretedCog cog)
        {
            //change background when following program cursor
            BufferGraphics.Clear(followPCButton.Checked ?
                SystemColors.Window :
                SystemColors.Control);
            uint memoryPos = (uint)positionScroll.Value;
            //get physical number of lines that fits on panel
            int totalLines = GetLinesOfDecodedPanel();
            //and assure array length sufficient for that physical number of lines
            CheckRequiredArrayLength(ref _interpreterAddressLines, totalLines);
            CheckRequiredArrayLength(ref _interpretedLengthLines, totalLines);
            //but if we are at the upper end of memory, the view could show
            // fewer lines without resizing the array, thus reducing memory
            // allocation events => faster redraw.
            totalLines = (int)Math.Min(
                totalLines,
                PropellerCPU.TotalRAM - memoryPos);
            //pre-process the decoding to get instruction length of each line
            MemorySegment memorySegment = new MemorySegment(Chip, memoryPos);
            for (int line = 0; line < totalLines; line++)
            {
                _interpreterAddressLines[line] = memorySegment.Address;
                _interpretedLengthLines[line] =
                    (byte)InstructionDisassembler.InterpretedInstructionLength(memorySegment);
            }
            //draw nothing if there is no effective space for decoded lines
            if (_decodedEffectiveWidth <= 0)
                return;
            string formatForValue = DisplayAsHexadecimal ?
                "${0:X2}" :
                "{0,3:D}";
            //loop to draw lines
            MemorySegment commonMemorySegment =
                new MemorySegment(Chip, _interpreterAddressLines[0]);
            for (int line = 0; line < totalLines; line++)
                DrawInterpretedDecodedLine(cog, commonMemorySegment,
                    _interpreterAddressLines[line],
                    _interpretedLengthLines[line], line, formatForValue);
        }

        /// <summary>Draw SPIN decoded text line.</summary>
        /// <param name="cog">Interpreted cog reference.</param>
        /// <param name="memorySegment"></param>
        /// <param name="memoryPos">Memory position to print decoded value.</param>
        /// <param name="length">Length of decoded instruction, in bytes.</param>
        /// <param name="line">Line number where to draw.</param>
        /// <param name="formatForValue">String with format for memory
        /// contents, hex or decimal.</param>
        /// @version v22.09.01 - Added.
        private void DrawInterpretedDecodedLine(InterpretedCog cog,
            MemorySegment memorySegment, uint memoryPos, byte length, int line,
            string formatForValue)
        {
            //generate memory values as text field: 1 to 5 bytes long
            string memoryText = string.Empty;
            for (uint i = 0; i < length; i++)
            {
                byte memoryValue = Chip.DirectReadByte(memoryPos + i);
                string interSpace = i > 0 ? " " : string.Empty;
                memoryText += $"{interSpace}{string.Format(formatForValue, memoryValue)}";
            }
            //padding of memory text
            //19 = 3 * 5 + 4 = <value length> * <max bytes> - (<max bytes> - 1)
            int fillQty = 19 - memoryText.Length;
            if (fillQty > 0)
                memoryText += new string(' ', fillQty);
            //decode instruction as text field
            string decodedText = InstructionDisassembler.InterpreterText(
                memorySegment ?? new MemorySegment(Chip, memoryPos),
                DisplayAsHexadecimal, _useShortOpCodes);
            //change background on breakpoint line
            if (memoryPos == cog.BreakPointCogCursor)
                BufferGraphics.FillRectangle(Brushes.Pink, 0,
                    line * _lineHeight, decodedPanel.ClientSize.Width,
                    _lineHeight);
            //Draw frame on line if is the same of program cursor
            if (cog.ProgramCursor == memoryPos &&
                (HighlightedType & HighlightedTypeEnum.OnlyFrame) ==
                HighlightedTypeEnum.OnlyFrame)
                BufferGraphics.DrawRectangle(Pens.Black,
                    0, line * _lineHeight, _decodedEffectiveWidth,
                    _lineHeight);
            //print text line
            BufferGraphics.DrawString(
                $"${memoryPos:X4}:  {memoryText}  {decodedText}",
                cog.ProgramCursor == memoryPos &&
                (HighlightedType & HighlightedTypeEnum.OnlyBold) ==
                HighlightedTypeEnum.OnlyBold ?
                    _monoFontBold :
                    _monoFont,
                SystemBrushes.ControlText, 0, line * _lineHeight);
        }

        /// <summary>Display details pane for a SPIN cog.</summary>
        /// <param name="cog">Interpreted cog reference.</param>
        /// @version v22.09.01 - Added as separated method, because this logic
        /// was embedded into DisplayDecodedForInterpretedCog() method.
        private void DisplayInterpretedDetails(InterpretedCog cog)
        {
            //fill the pane area
            BufferGraphics.FillRectangle(SystemBrushes.Control,
                decodedPanel.ClientSize.Width - _stackMargin - 1, 0,
                _stackMargin + 1, decodedPanel.ClientSize.Height);
            //side separation line
            BufferGraphics.DrawLine(Pens.Black,
                decodedPanel.ClientSize.Width - _stackMargin - 1, 0,
                decodedPanel.ClientSize.Width - _stackMargin - 1, decodedPanel.ClientSize.Height);
            Brush standardBrush = SystemBrushes.ControlText;
            int line = 0;
            uint longValue = cog.StackFrame;
            string text = $"@Stk[0]  =  ${longValue:X4}, {longValue,5:D}";
            DrawLineOfInterpretedDetails(text, standardBrush, line++);
            longValue = cog.ObjectFrame;
            text = $"@Obj[0]  =  ${longValue:X4}, {longValue,5:D}";
            DrawLineOfInterpretedDetails(text, standardBrush, line++);
            longValue = cog.LocalFrame;
            text = $"@Loc[0]  =  ${longValue:X4}, {longValue,5:D}";
            DrawLineOfInterpretedDetails(text, standardBrush, line++);
            longValue = cog.VariableFrame;
            text = $"@Var[0]  =  ${longValue:X4}, {longValue,5:D}";
            DrawLineOfInterpretedDetails(text, standardBrush, line++);
            //1st separation line
            BufferGraphics.DrawLine(Pens.Black,
                decodedPanel.ClientSize.Width - _stackMargin,
                line * _lineHeight, decodedPanel.ClientSize.Width,
                line * _lineHeight);
            ushort wordValue = Chip.DirectReadWord(cog.LocalFrame - 8);
            text = $"Caller&  =  ${wordValue:X4}, {wordValue,5:D}";
            DrawLineOfInterpretedDetails(text, standardBrush, line++);
            wordValue = Chip.DirectReadWord(cog.LocalFrame - 6);
            text = $"         =  ${wordValue:X4}, {wordValue,5:D}";
            DrawLineOfInterpretedDetails(text, standardBrush, line++);
            wordValue = Chip.DirectReadWord(cog.LocalFrame - 4);
            text = $"         =  ${wordValue:X4}, {wordValue,5:D}";
            DrawLineOfInterpretedDetails(text, standardBrush, line++);
            wordValue = Chip.DirectReadWord(cog.LocalFrame - 2);
            text = $"Return&  =  ${wordValue:X4}, {wordValue,5:D}";
            DrawLineOfInterpretedDetails(text, standardBrush, line++);
            //draw header of stack
            BufferGraphics.FillRectangle(SystemBrushes.ControlLight,
                decodedPanel.ClientSize.Width - _stackMargin,
                line * _lineHeight, _stackMargin, _lineHeight);
            //2nd separation line
            BufferGraphics.DrawLine(Pens.Black,
                decodedPanel.ClientSize.Width - _stackMargin,
                line * _lineHeight, decodedPanel.ClientSize.Width,
                line * _lineHeight);
            int stackLength = (int)(cog.StackFrame - cog.LocalFrame) / 4;
            text = $"(Len {stackLength})";
            DrawLineOfInterpretedDetails($"Stack:{new string(' ', CharWidthInterpreterDetails - 6 - text.Length)}{text}",
                standardBrush, line++);
            if (stackLength <= 0)
                return;
            for (uint i = cog.LocalFrame; i < cog.StackFrame && line < GetLinesOfDecodedPanel(); i += 4)
            {
                int intValue = (int)Chip.DirectReadLong(i);
                text = $"${intValue:X}";
                DrawLineOfInterpretedDetails($"{(i - cog.LocalFrame) / 4,2:D}:{text,9},{intValue,11:D}",
                    standardBrush, line++);
            }
        }

        /// <summary>Draw one line of SPIN interpreter details</summary>
        /// <param name="text">Text for the printed line.</param>
        /// <param name="lineBrush">Brush used to print the text.</param>
        /// <param name="line">Line number.</param>
        /// @version v22.09.01 - Added.
        private void DrawLineOfInterpretedDetails(string text,
            Brush lineBrush, int line)
        {
            BufferGraphics.DrawString(text, _monoFont, lineBrush,
                decodedPanel.ClientSize.Width - _stackMargin,
                line * _lineHeight);
        }

        /// <summary>Repaint the Cog state and data.</summary>
        /// <param name="force">Flag to indicate the intention to force the
        /// repaint.</param>
        /// @version v22.09.01 - Refactored.
        public override void Repaint(bool force)
        {
            PresentationCogStateEnum oldPresentationValue = PresentationCogState;
            Cog cog = ReferencedCog;
            if (cog == null)
            {
                //show stopped cog view
                PresentationCogState = PresentationCogStateEnum.Stopped;
                if (oldPresentationValue == PresentationCogStateEnum.None)
                    return;
                BufferGraphics.Clear(SystemColors.Control);
                BackBuffer.Render();
                return;
            }
            //update values on valuesToolStrip
            cogStateLabel.Text = $@"{cog.CogStateString}";
            programCursorLabel.Text = $@"${cog.ProgramCursor:X4}";
            frameCountLabel.Text = $@"{cog.VideoFrameString}";
            //check for changed cog type
            switch (cog)
            {
                case InterpretedCog _:
                    if (oldPresentationValue !=
                        PresentationCogStateEnum.InterpretedRunning)
                    {
                        PresentationCogState =
                            PresentationCogStateEnum.InterpretedRunning;
                        //set advances in position scroll
                        positionScroll.Maximum = PropellerCPU.TotalRAM - 1;
                        positionScroll.LargeChange = GetLinesOfDecodedPanel();
                    }
                    break;
                case NativeCog _:
                    if (oldPresentationValue !=
                        PresentationCogStateEnum.NativeRunning)
                    {
                        PresentationCogState =
                            PresentationCogStateEnum.NativeRunning;
                        //set advances in position scroll
                        positionScroll.Maximum = Cog.TotalCogMemory - 1;
                        positionScroll.LargeChange = GetLinesOfDecodedPanel();
                    }
                    break;
            }
            //limit position of scroll at the end of memory of each case
            if (positionScroll.Value > positionScroll.Maximum)
                positionScroll.Value = positionScroll.Maximum;
            if (followPCButton.Checked)
            {
                //int lastHighlightedLine = GetLinesOfDecodedPanel() - HighlightedLines;
                if (cog.ProgramCursor < GetLinesOfDecodedPanel())
                    positionScroll.Value = 0;
                else if (cog.ProgramCursor - positionScroll.Value > GetLinesOfDecodedPanel())
                    positionScroll.Value = (int)cog.ProgramCursor;
                else if (cog.ProgramCursor < positionScroll.Value)
                    positionScroll.Value = (int)cog.ProgramCursor;
            }
            //draw specific data based on cog type
            switch (cog)
            {
                case NativeCog nativeCog:
                    DisplayNativeCog(force, nativeCog);
                    break;
                case InterpretedCog interpretedCog:
                    if (memoryViewButton.Checked)
                        DisplayMemoryForInterpretedCog(force, interpretedCog);
                    else
                        DisplayDecodedForInterpretedCog(force, interpretedCog);
                    DisplayInterpretedDetails(interpretedCog);
                    break;
            }
            BackBuffer.Render();
        }

        /// <summary>Event handler to manage the scroll of position bar.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Scroll event data arguments.</param>
        /// @version v22.09.01 - Changed method name to clarify its meaning..
        private void PositionScroll_Scroll(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        /// <summary>Event handler to manage the size changes of the decoded
        /// program panel.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.09.01 - Changed method name to clarify its meaning.
        private void DecodedPanel_SizeChanged(object sender, EventArgs e)
        {
            if (decodedPanel.ClientSize.Width > 0 && decodedPanel.ClientSize.Height > 0)
            {
                ResetBufferGraphics();
                _decodedEffectiveWidth =
                    decodedPanel.ClientSize.Width - _stackMargin - 1;
            }
            decodedPanel.Invalidate(true);
        }

        /// <summary>Event handler to paint the decoded program panel.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Paint event data arguments.</param>
        /// @version v22.09.01 - Changed method name to clarify its meaning..
        private void DecodedPanel_Paint(object sender, PaintEventArgs e)
        {
            //positionScroll.Refresh();
            Repaint(false);
        }

        /// <summary>Event handler to manage mouse click on the decoded program
        /// panel.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Mouse event data arguments.</param>
        /// @version v22.09.01 - Changed method name to clarify its meaning..
        private void DecodedPanel_MouseClick(object sender, MouseEventArgs e)
        {
            positionScroll.Focus();
        }

        /// <summary>Event handler to manage the mouse hovering over a decoded
        /// line on the decoded program panel.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Mouse event data arguments.</param>
        /// @version v22.09.01 - Corrected error of swapped source and
        /// destination registers on tooltip text. Refactored method to decode
        /// more information from instruction and enable to show also in memory
        /// view. Changed local variable names to clarify the true concept of
        /// them.
        private void DecodedPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(ReferencedCog is NativeCog nativeCog))
                return;
            uint memoryAddress =
                (uint)(positionScroll.Value +
                       decodedPanel.PointToClient(MousePosition).Y / _lineHeight);
            if (memoryAddress >= Cog.TotalCogMemory)
                return;
            //Update tooltip only if memory position has changed, to prevent flickering
            if (memoryAddress == _oldMemoryPosHovered)
                return;
            uint encodedInstruction = nativeCog.ReadLong(memoryAddress);
            Disassembler.Assembly.DecodedPASMInstruction instruction =
                new Disassembler.Assembly.DecodedPASMInstruction(encodedInstruction);
            Assembly.InstructionVariant actualInstructionVariant =
                instruction.GetInstructionVariant();
            string sourceText = actualInstructionVariant.UseSource &&
                                instruction.CON != 0x0 ?
                (instruction.ImmediateValue() ?
                    $"Source immediate value= ${instruction.SRC:x3}, {instruction.SRC}" :
                    $"Source ${instruction.SRC:x3}: Reg value= ${nativeCog.ReadLong(instruction.SRC):x3}, {nativeCog.ReadLong(instruction.SRC)}") :
                "Source not used by instruction.";
            string destString = actualInstructionVariant.UseDestination &&
                                instruction.CON != 0x0 ?
                $"Destin. ${instruction.DEST:x3}: Reg value = ${nativeCog.ReadLong(instruction.DEST):x3}, {nativeCog.ReadLong(instruction.DEST)}" :
                "Destination not used by instruction.";
            toolTip1.SetToolTip(decodedPanel, $"{sourceText}\n{destString}");
            _oldMemoryPosHovered = memoryAddress;
        }

        /// <summary></summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Mouse event data arguments.</param>
        /// @version v22.09.01 - Changed local variable name to clarify the
        /// true concept of them.
        private void DecodedPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left)
                return;
            //Make sure it's a valid breakpoint environment
            Cog cog = ReferencedCog;
            if (cog == null)
                return;
            //Find the line that was clicked on
            int memoryAddress =
                decodedPanel.PointToClient(MousePosition).Y / _lineHeight;
            switch (cog)
            {
                //What type of cog?
                case NativeCog _:
                    memoryAddress += positionScroll.Value;
                    break;
                case InterpretedCog _:
                    try
                    {
                        memoryAddress =
                            (int)_interpreterAddressLines[memoryAddress];
                    }
                    catch (NullReferenceException) { return; }
                    break;
            }
            //Toggle/move the breakpoint
            if (memoryAddress == cog.BreakPointCogCursor)
                cog.BreakPointCogCursor = -1;
            else
                cog.BreakPointCogCursor = memoryAddress;
            //Show the user what happened
            Repaint(false);
        }

        /// <summary>Event handler when the Memory button is clicked.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void MemoryViewButton_Click(object sender, EventArgs e)
        {
            Repaint(false);
        }

        /// <summary>Event handler when the follow program cursor button is
        /// clicked.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.09.01 - Modified to change position scroll bar
        /// visibility.
        private void FollowPCButton_Click(object sender, EventArgs e)
        {
            positionScroll.Enabled = !followPCButton.Checked;
            Repaint(false);
        }

        /// <summary>Event handler when the hexadecimal units menu option is
        /// selected.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.09.01 - Modified to use DisplayAsHexadecimal property.
        private void HexadecimalUnits_Click(object sender, EventArgs e)
        {
            if (!displayUnitsBtn.Enabled)
                return;
            DisplayAsHexadecimal = true;
            Repaint(false);
        }

        /// <summary>Event handler when the decimal units menu option is
        /// selected.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.09.01 - Modified to use DisplayAsHexadecimal property.
        private void DecimalUnits_Click(object sender, EventArgs e)
        {
            if (!displayUnitsBtn.Enabled)
                return;
            DisplayAsHexadecimal = false;
            Repaint(false);
        }

        /// <summary>Event handler when the long op-codes menu option is
        /// selected.</summary>
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

        /// <summary>Event handler when the short op-codes menu option is
        /// selected.</summary>
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

        /// <summary>Event handler when the video break button is clicked.</summary>
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
                frameBreakMode.Text = @"Video Break: None";
            }
            if (sender == breakMiss)
            {
                breakNone.Checked = false;
                breakMiss.Checked = true;
                breakAll.Checked = false;
                _breakVideo = FrameState.Miss;
                frameBreakMode.Text = @"Video Break: Miss";
            }
            if (sender == breakAll)
            {
                breakNone.Checked = false;
                breakMiss.Checked = false;
                breakAll.Checked = true;
                _breakVideo = FrameState.None;
                frameBreakMode.Text = @"Video Break: End";
            }
            ReferencedCog?.SetVideoBreak(_breakVideo);
        }
    }
}
