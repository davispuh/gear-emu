/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * SpinView.cs
 * Spin object viewer class
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
using Gear.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Gear.GUI
{
    /// <summary>Spin object viewer.</summary>
    public partial class SpinView : PluginSupport.PluginBase
    {
        /// <summary>Indicates the different type of byte alignment for the
        /// Spin map.</summary>
        /// @version v22.07.01 - Added.
        public enum AlignmentEnum : byte
        {
            /// <summary>No alignment.</summary>
            None = 0,
            /// <summary>Each 8 Bytes.</summary>
            Byte = 8,
            /// <summary>Each 16 Bytes.</summary>
            Word = 16
        }

        /// <summary>Styles to draw headers.</summary>
        /// <remarks>The values can be combined because is a flag
        /// enumeration.</remarks>
        /// @version v22.09.02 - Added.
        [Flags]
        public enum HeaderStyleEnum : byte
        {
            /// <summary>Draw only top header.</summary>
            OnlyTop = 1,
            /// <summary>Draw only bottom header.</summary>
            OnlyBottom = 2,
            /// <summary>Draw both headers.</summary>
            TopAndBottom = 3
        }

        /// <summary>Sections of drawing areas.</summary>
        /// @version v22.09.02 - Added
        private enum SectionEnum : byte
        {
            /// <summary>Top header.</summary>
            TopHeader = 0,
            /// <summary>Bottom header.</summary>
            BottomHeader,
            /// <summary>Address column.</summary>
            AddressColumn,
            /// <summary>Memory data.</summary>
            MemoryData
        }

        /// <summary>Indicates the different type of nodes of the Spin map.</summary>
        /// @version v22.09.02 - Delete `Begin` enum value.
        private enum NodeTypeEnum : byte
        {
            /// <summary>Spin header section.</summary>
            SpinHeader = 0,
            /// <summary>Object start node.</summary>
            Object,
            /// <summary>Object header section.</summary>
            ObjectHeader,
            /// <summary>Object body section.</summary>
            ObjectBody,
            /// <summary>Inner objects list section.</summary>
            InnerObjectsList,
            /// <summary>Function start node.</summary>
            Function,
            /// <summary>Function body section.</summary>
            FunctionBody,
            /// <summary>Variables start node.</summary>
            Variable,
            /// <summary>Variables body section.</summary>
            VariableBody,
            /// <summary>Local variables start node.</summary>
            Local,
            /// <summary>Start of Stack section.</summary>
            Stack,
            /// <summary>End of enumeration.</summary>
            /// <remarks>Used to mark the end in a `for` loop.</remarks>
            End
        }

        /// <summary>How many memory values will show in a line.</summary>
        /// @version v22.09.02 - Added.
        const int ValuesQtyToShow = 16;

        /// <summary>List of Colors to paint each byte of memory.</summary>
        /// @version v22.07.01 - Added.
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private static readonly SortedDictionary<NodeTypeEnum, Color> ColorList;

        /// <summary>List of Brushes to paint each byte of memory.</summary>
        /// @version v22.07.01 - Added.
        private static readonly SortedDictionary<NodeTypeEnum, Brush> BrushesList;

        /// <summary>Object to access Resources for this control</summary>
        /// @version v22.07.01 - Added.
        private static readonly ComponentResourceManager Resources =
            new ComponentResourceManager(typeof(SpinView));

        /// <summary>Array for colorize every memory position according
        /// to map location.</summary>
        /// @version v22.07.01 - Name changed to follow naming conventions.
        /// Changed declaring location from `SpinView.Designer.cs` to here, to
        /// better documentation.
        private readonly Brush[] _memoryColorBrush;

        ///<summary>Mono spaced font for memory map.</summary>
        /// @version v22.07.01 - Name changed to follow naming conventions.
        /// Changed declaring location from `SpinView.Designer.cs` to here, to
        /// better documentation.
        private readonly Font _monoFont;

        /// <summary>Width and height of memory data on Mono font.</summary>
        /// @version v22.09.02 - Added
        private readonly Size _dataFontSize;

        /// <summary>Width and height of address for memory on Mono font.</summary>
        /// @version v22.09.02 - Added
        private readonly Size _addressFontSize;

        /// <summary>Current Culture to modify its Number format.</summary>
        /// @version v22.07.01 - Name changed to follow naming conventions.
        private readonly CultureInfo _currentCultureMod;

        /// <summary>Section drawing manager instance.</summary>
        /// @version v22.09.02 - Added
        private readonly DrawingSectionManager<SectionEnum> _sectionManager;

        /// <summary>Buffer graphic context.</summary>
        /// @version v22.09.02 - Added
        private readonly BufferedGraphicsContext _bufferedGraphicsContext =
            new BufferedGraphicsContext();
/*
        /// <summary>Backing field for Graphic style to draw text on buffer.</summary>
        /// @version v22.09.01 - Changed to enable parallel drawing.
        private ThreadLocal<Graphics> _bufferGraphics =
            new ThreadLocal<Graphics>(true);
*/
        /// <summary>Backing field for Graphic style to draw text on buffer.</summary>
        /// @version v22.09.01 - Changed to enable parallel drawing.
        private Graphics _bufferGraphics;

        /// <summary>Image for Icon on tree view for each node.</summary>
        /// @version v22.07.01 - Added.
        private readonly Image _frameIcon;

        /// <summary>Storage for frequency format.</summary>
        /// <remarks>Used to establish data binding to program properties.</remarks>
        /// @version v22.07.01 - Added.
        private NumberFormatEnum _reqFormatValue;

        /// <summary>Flag to declare if color decode of Spin byte code has
        /// been done or not.</summary>
        /// @version v22.07.01 - Added.
        private bool _colorDecodeDone;

        /// <summary>Baking field to HeaderStyle property.</summary>
        /// @version v22.09.02 - Added.
        private HeaderStyleEnum _headerStyle;

        /// <summary>Current byte alignment for the Spin map.</summary>
        /// @version v22.07.01 - Added.
        private AlignmentEnum _byteAlignment;

        /// <summary>Title of the tab window.</summary>
        public override string Title => "Spin Map";

        /// <summary>Attribute to allow the window to be closed (default) or
        /// not (like cog windows).</summary>
        public override bool IsClosable => false;

        /// <summary>Identify a plugin as user (=true) or system (=false).</summary>
        public override bool IsUserPlugin => false;

        /// <summary>Double buffer property to draw on it.</summary>
        /// @version v22.09.02 - Modified
        private BufferedGraphics BackBuffer { get; set; }

/*
        /// <summary>Graphic style property to draw text and graphics
        /// on buffer.</summary>
        /// <remarks>Used to set the font aliasing style for text of the
        /// control.</remarks>
        /// @version v22.09.02 - Modified
        private Graphics BufferGraphics
        {
            get
            {
                Graphics localCopyGraphics = null;
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(_bufferedGraphicsContext, ref lockTaken);
                    if (_bufferGraphics.Value == null)
                        _bufferGraphics.Value = GraphicsGenerator(memoryView);
                    localCopyGraphics = _bufferGraphics.Value;
                    if (_bufferGraphics.IsValueCreated)
                    {
                        int setSize = _bufferGraphics.Values.Count;
                        if (setSize > 1)
                            for (int i = 0; i < setSize - 1; i++)
                            for (int j = i + 1; j < setSize; j++)
                                Debug.Assert(_bufferGraphics.Values[i] != _bufferGraphics.Values[j], $"Buffers are the same:\nFirst: {i:D} - {_bufferGraphics.Values[i]}\nSecond: {j:D} - {_bufferGraphics.Values[j]}");
                    }
                }
                catch (ArgumentNullException e)
                {
                    MessageBox.Show(
                        e.Message,
                        $@"Error assigning Graphics in {nameof(BufferGraphics)}",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                catch (ArgumentException)
                {
                    return null;
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit(_bufferedGraphicsContext);
                }
                return localCopyGraphics;
            }
        }
*/

        /// <summary>Graphic style property to draw text and graphics
        /// on buffer.</summary>
        /// <remarks>Used to set the font aliasing style for text of the
        /// control.</remarks>
        /// @version v22.09.02 - Modified
        private Graphics BufferGraphics =>
            _bufferGraphics;

        /// <summary>Frequency format to be displayed.</summary>
        /// <remarks>Used to establish data binding to program properties.
        /// Needs to be public and have getter and setter to binding
        /// works.</remarks>
        /// @version v22.07.01 - Added.
        // ReSharper disable once MemberCanBePrivate.Global
        public NumberFormatEnum FreqFormatValue
        {
            get => _reqFormatValue;
            set
            {
                if (_reqFormatValue == value)
                    return;
                _reqFormatValue = value;
                _currentCultureMod.NumberFormat =
                    NumberFormatEnumExtension.GetFormatInfo(_reqFormatValue);
                _colorDecodeDone = false;
                UpdateSystemFreq();
            }
        }

        /// <summary>Style to draw headers.</summary>
        /// <remarks>Used to establish data binding to program properties.
        /// Needs to be public and have getter and setter to binding
        /// works.</remarks>
        /// @version v22.09.02 - Added.
        // ReSharper disable once MemberCanBePrivate.Global
        public HeaderStyleEnum HeaderStyle
        {
            get => _headerStyle;
            set
            {
                if (_headerStyle == value)
                    return;
                _headerStyle = value;
                _sectionManager?.Reset();
                memoryView.Invalidate();
            }
        }

        /// <summary>Current byte alignment for the Spin map.</summary>
        /// @version v22.07.01 - Added.
        private AlignmentEnum ByteAlignment
        {
            get => _byteAlignment;
            set
            {
                if (_byteAlignment == value)
                    return;
                AlignmentEnum oldValue = _byteAlignment;
                _byteAlignment = value;
                SetAdvancesOnScrollBar();
                SetMenuStatesOfAlignmentBtn(oldValue);
                UpdateAlignmentBtn();
            }
        }

        /// <summary>Default static constructor.</summary>
        /// <remarks>Initialize color and brush lists.</remarks>
        /// @version v22.07.01 - Added.
        static SpinView()
        {
            ColorList = new SortedDictionary<NodeTypeEnum, Color> {
                { NodeTypeEnum.SpinHeader, Color.White},
                {NodeTypeEnum.Object, Color.FromArgb(255, 247, 98, 145)},
                {NodeTypeEnum.ObjectHeader, Color.FromArgb(255, 255, 170, 170)},
                {NodeTypeEnum.ObjectBody, Color.LightGreen},
                {NodeTypeEnum.InnerObjectsList, Color.FromArgb(255, 255, 216, 216)},
                {NodeTypeEnum.Function, Color.FromArgb(255, 102, 178, 255)},
                {NodeTypeEnum.FunctionBody, Color.FromArgb(255, 191, 223, 255)},
                {NodeTypeEnum.Variable, Color.FromArgb(255, 252, 234, 11)},
                {NodeTypeEnum.VariableBody, Color.FromArgb(255, 255, 249, 209)},
                {NodeTypeEnum.Local, Color.FromArgb(255, 183, 156, 156)},
                {NodeTypeEnum.Stack, Color.LightGray},
                { NodeTypeEnum.End, Color.Gray}
            };
            BrushesList = new SortedDictionary<NodeTypeEnum, Brush>();
            //fill brushes values
            foreach (KeyValuePair<NodeTypeEnum, Color> pair in ColorList)
                BrushesList.Add(pair.Key, new SolidBrush(pair.Value));
        }

        /// <summary>Default Constructor.</summary>
        /// <param name="chip">Reference to Propeller instance.</param>
        /// @version v22.09.01 - Changed to use parallel loop to initialize
        /// _memoryColorBrush array and a separated task to generate icons for
        /// nodes.
        public SpinView(PropellerCPU chip) : base(chip)
        {
            //init objects
            _memoryColorBrush = new Brush[PropellerCPU.TotalRAM];
            _monoFont = new Font(FontFamily.GenericMonospace, 8);
            _dataFontSize = TextRenderer.MeasureText("00", _monoFont);
            _addressFontSize = TextRenderer.MeasureText("$0000:", _monoFont);
            _currentCultureMod = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            _sectionManager = new DrawingSectionManager<SectionEnum>(
                CalculateRectangleOfSection, SectionEnum.TopHeader,
                SectionEnum.MemoryData);
            // ReSharper disable once ExceptionNotDocumented
            _frameIcon = (Image)Resources.GetObject("FrameIcon");
            InitializeComponent();
            ResetBufferGraphics();
            //this goes here, because it isn't accepted on Designer!
            PositionScrollBar.Maximum = PropellerCPU.TotalRAM - 1;
            ByteAlignment = AlignmentEnum.Word;
            //bonded properties
            FreqFormatValue = Properties.Settings.Default.FreqFormat;
            DataBindings.Add(new Binding("FreqFormatValue",
                Properties.Settings.Default, "FreqFormat",
                false, DataSourceUpdateMode.OnPropertyChanged));
            HeaderStyle = Properties.Settings.Default.HeaderToDraw;
            DataBindings.Add(new Binding("HeaderStyle",
                Properties.Settings.Default, "HeaderToDraw",
                false, DataSourceUpdateMode.OnPropertyChanged));
            //generate icons for each node type
            bool rollOver = false;
            for (NodeTypeEnum nodeType = 0;
                 nodeType < NodeTypeEnum.End && !rollOver;
                 nodeType = nodeType.EnumNext(out rollOver))
            {
                try
                {
                    Image image = GenerateIconForNode(nodeType);
                    imageListForTreeView.Images.AddStrip(image);
                }
                catch (InvalidOperationException e)
                {
                    MessageBox.Show(this, e.Message, $@"Icon generation failed on '{nodeType}'.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            //fill values to format memory background
            // ReSharper disable once ExceptionNotDocumented
            Parallel.ForEach(Partitioner.Create(0, _memoryColorBrush.Length),
                range =>
                {
                    Brush newValue = BrushesList[NodeTypeEnum.End];
                    for (int i = range.Item1; i < range.Item2; i++)
                        _memoryColorBrush[i] = newValue;
                });
            _colorDecodeDone = false;
        }

        /// <summary>Register the events to be notified to this plugin.</summary>
        public override void PresentChip() { }

        /// <summary>Event when the chip is reset.</summary>
        public override void OnReset()
        {
            base.OnReset();
            _colorDecodeDone = false;
            Analyze();
        }

        /// <summary>Generate a new Graphics based on control object.</summary>
        /// <param name="control">Graphical object to be acceded by this
        /// Graphics object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="control"/>
        /// is <see langword="null"/>.</exception>
        /// <returns></returns>
        /// @version v22.09.02 - Added.
        private Graphics GraphicsGenerator(Control control)
        {
            if (control == null)
            {
                string msg = $"Argument null given as parameter {nameof(control)} in method {nameof(GraphicsGenerator)}({typeof(Control)})";
                throw new ArgumentNullException(nameof(control), msg);
            }
            Graphics graphics = control.CreateGraphics();
            //graphical settings to apply
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.CompositingQuality = CompositingQuality.AssumeLinear;
            return graphics;
        }

/*
        /// <summary></summary>
        /// <param name="sectionType"></param>
        /// <returns></returns>
        /// @version v22.09.02 - Added.
        private Graphics GetGraphics(SectionEnum sectionType)
        {
            if (!_sectionGraphics.ContainsKey(sectionType) ||
                _sectionGraphics[sectionType] == null)
            {
                Graphics graphics = memoryView.CreateGraphics();
                //graphical settings
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                _sectionGraphics[sectionType] = graphics;
                //select area to draw
                if (!_sectionRectangles.TryGetValue(sectionType, out Rectangle selectedRectangle))
                {
                    MessageBox.Show(this,
                        $@"Not found rectangle location for {sectionType}.",
                        @"Error Drawing memory header",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                _sectionRectangles[sectionType] = selectedRectangle;
                //select/create buffered graphics
                if (_sectionBufferedGraphics.ContainsKey(sectionType))
                    return graphics;
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(_bufferedGraphicsContext, ref lockTaken);
                    _sectionBufferedGraphics[sectionType] =
                        _bufferedGraphicsContext.Allocate(graphics,
                            selectedRectangle);
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit(_bufferedGraphicsContext);
                }
                return graphics;
            }
            return _sectionGraphics[sectionType];
        }
*/

/*
        /// <summary>Invalidate graphics underlying for double buffer of main
        /// panel.</summary>
        /// @version v22.09.02 - Changed
        private void ResetBufferGraphics()
        {
            bool lockTaken = false;
            try
            {
                Monitor.Enter(_bufferedGraphicsContext, ref lockTaken);
                _bufferGraphics.Value = GraphicsGenerator(memoryView);
                BackBuffer = _bufferedGraphicsContext.Allocate(
                    GraphicsGenerator(memoryView),
                    memoryView.DisplayRectangle);
            }
            catch (ArgumentNullException e)
            {
                MessageBox.Show(
                    e.Message,
                    $@"Error Resetting Graphics Buffer in {nameof(ResetBufferGraphics)}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_bufferedGraphicsContext);
            }
        }
*/

        /// <summary>Regenerate graphics access with double buffer of main
        /// panel.</summary>
        /// @version v22.09.02 - Changed to use GraphicsGenerator() method.
        private void ResetBufferGraphics()
        {
            BackBuffer = _bufferedGraphicsContext.Allocate(
                GraphicsGenerator(memoryView),
                memoryView.DisplayRectangle);
            _bufferGraphics = BackBuffer.Graphics;
        }

        /// <summary>Generate image for icon of tree view.</summary>
        /// <param name="nodeType">Type of node to generate.</param>
        /// <returns>Image generated.</returns>
        /// @version v22.07.01 - Added.
        private Image GenerateIconForNode(NodeTypeEnum nodeType)
        {
            //only margin on top
            Image generated =
                new Bitmap(_frameIcon.Width, _frameIcon.Height + 1,
                    PixelFormat.Format32bppArgb);
            Graphics graph = Graphics.FromImage(generated);
            graph.SmoothingMode = SmoothingMode.HighQuality;
            graph.CompositingQuality = CompositingQuality.AssumeLinear;
            //background color by type
            graph.FillRectangle(BrushesList[nodeType], 0, 1,
                _frameIcon.Width, _frameIcon.Height + 1);
            //create transparent lower margin
            graph.DrawLine(Pens.Transparent, 0, 0, _frameIcon.Width, 0);
            //overwrite icon image with translucent pixels (hole to fill
            //with background color)
            graph.DrawImage(_frameIcon, 0, 1);
            return generated;
        }

        /// <summary>Calculate the number of lines that fits on memory
        /// panel.</summary>
        /// <returns>Number of lines.</returns>
        /// @version v22.09.02 - Added.
        private int GetLinesOfMemoryPanel() =>
            memoryView.ClientSize.Height / _dataFontSize.Height;

        /// <summary>Method to calculate rectangle area for each drawing
        /// section.</summary>
        /// <param name="section">Drawing section enum value.</param>
        /// <returns>Rectangle with coordinates delimiting the related
        /// drawing area.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// @version v22.09.02 - Added.
        private Rectangle CalculateRectangleOfSection(SectionEnum section)
        {
            int totalLines = GetLinesOfMemoryPanel();
            bool haveTopHeader = HeaderStyle.HasFlag(HeaderStyleEnum.OnlyTop);
            bool haveBottomHeader = HeaderStyle.HasFlag(HeaderStyleEnum.OnlyBottom);
            switch (section)
            {
                case SectionEnum.TopHeader:
                    return haveTopHeader ?
                        new Rectangle(0, 0,
                            _addressFontSize.Width + _dataFontSize.Width * ValuesQtyToShow,
                            _dataFontSize.Height) :
                        Rectangle.Empty;
                case SectionEnum.BottomHeader:
                    return haveBottomHeader ?
                        new Rectangle(0,
                            (totalLines - 1) * _dataFontSize.Height,
                            _addressFontSize.Width + _dataFontSize.Width * ValuesQtyToShow,
                            _dataFontSize.Height) :
                        Rectangle.Empty;
                case SectionEnum.AddressColumn:
                    return new Rectangle(0,
                        haveTopHeader ? _addressFontSize.Height : 0,
                        _addressFontSize.Width,
                        _addressFontSize.Height *
                        (totalLines - (haveBottomHeader ? 1 : 0)
                                    - (haveTopHeader ? 1 : 0)));
                case SectionEnum.MemoryData:
                    return new Rectangle(
                        _addressFontSize.Width,
                        haveTopHeader ? _addressFontSize.Height : 0,
                        _dataFontSize.Width * ValuesQtyToShow,
                        _addressFontSize.Height *
                        (totalLines - (haveBottomHeader ? 1 : 0)
                                    - (haveTopHeader ? 1 : 0)));
                default:
                {
                    string msg = $"Value {section} not supported on {typeof(SectionEnum).FullName} enum.";
                        throw new ArgumentOutOfRangeException(nameof(section), section, msg);
                }
            }
        }

        /// <summary>Set quantity advance on Scroll Bar according to
        /// alignment type.</summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// @version v22.07.01 - Added.
        private void SetAdvancesOnScrollBar()
        {
            if (PositionScrollBar == null)
                return;
            switch (ByteAlignment)
            {
                case AlignmentEnum.None:
                    PositionScrollBar.SmallChange = 1;
                    PositionScrollBar.LargeChange = 16;
                    break;
                case AlignmentEnum.Byte:
                    PositionScrollBar.SmallChange = (int)ByteAlignment;
                    PositionScrollBar.LargeChange = 8 * 4;
                    break;
                case AlignmentEnum.Word:
                    PositionScrollBar.SmallChange = (int)ByteAlignment;
                    PositionScrollBar.LargeChange = 16 * 2;
                    break;
                default:
                {
                    string msg = $"Value {ByteAlignment} not supported on {nameof(AlignmentEnum)} enum.";
                        throw new ArgumentOutOfRangeException(nameof(ByteAlignment), ByteAlignment, msg);
                }
            }
        }

        /// <summary>Set starting position of byte map, according to the
        /// alignment type.</summary>
        /// <param name="position">Memory position requested to align.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// @version v22.07.01 - Added.
        private void SetAlignedPosition(int position)
        {
            if (position < 0 || position > PropellerCPU.MaxMemoryAddress)
                return;
            switch (ByteAlignment)
            {
                case AlignmentEnum.None:
                    PositionScrollBar.Value = position;
                    break;
                case AlignmentEnum.Byte:
                case AlignmentEnum.Word:
                    //integer division
                    PositionScrollBar.Value =
                        position / (int)ByteAlignment * (int)ByteAlignment;
                    break;
                default:
                {
                    string msg = $"Value {ByteAlignment} not supported on {nameof(AlignmentEnum)} enum.";
                    throw new ArgumentOutOfRangeException(nameof(ByteAlignment), ByteAlignment, msg);
                }
            }
        }

        /// <summary>Set position of selected node on the object tree view.</summary>
        /// @version v22.07.01 - Added.
        private void SetPositionOfSelectedNode()
        {
            object obj = objectTreeView.SelectedNode.Tag;
            if (obj == null)
                return;
            SetAlignedPosition((int)obj);
        }

        /// <summary>Set the checked status of Item Menu according to current
        /// and old alignment values.</summary>
        /// <param name="oldByteAlignment">Old alignment value.</param>
        /// @version v22.07.01 - Added.
        private void SetMenuStatesOfAlignmentBtn(AlignmentEnum oldByteAlignment)
        {
            if (freeAlignmentMenuItem == null ||
                fixedByteAlignmentMenuItem == null ||
                fixedWordAlignmentMenuItem == null)
                return;
            //update for old value
            SetItemMenuState(oldByteAlignment, false);
            //update for current value
            SetItemMenuState(_byteAlignment, true);
        }

        /// <summary>Set a state for a single Item Menu.</summary>
        /// <param name="alignment">Alignment value corresponding to a Item
        /// Menu to set status.</param>
        /// <param name="newValue">New value to set on Item Menu.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// @version v22.07.01 - Added.
        private void SetItemMenuState(AlignmentEnum alignment, bool newValue)
        {
            switch (alignment)
            {
                case AlignmentEnum.None:
                    freeAlignmentMenuItem.Checked = newValue;
                    break;
                case AlignmentEnum.Byte:
                    fixedByteAlignmentMenuItem.Checked = newValue;
                    break;
                case AlignmentEnum.Word:
                    fixedWordAlignmentMenuItem.Checked = newValue;
                    break;
                default:
                {
                    string msg = $"Value {alignment} not supported on {nameof(AlignmentEnum)} enum.";
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, msg);
                }
            }
        }

        /// <summary>Update name, Icon and tooltip of alignment button,
        /// according to selected alignment type.</summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// @version v22.07.01 - Added.
        private void UpdateAlignmentBtn()
        {
            if (alignmentSplitButton == null)
                return;
            int steps = ByteAlignment == AlignmentEnum.None ?
                1 :
                (int)ByteAlignment;
            alignmentSplitButton.ToolTipText = $@"Advances in steps of {steps:D} step{(steps > 1 ? string.Empty : "s")}";

            switch (ByteAlignment)
            {
                case AlignmentEnum.None:
                    alignmentSplitButton.Text = @"Free Alignment";
                    alignmentSplitButton.Image = (Image)Resources.GetObject("AlignmentNone");
                    break;
                case AlignmentEnum.Byte:
                    alignmentSplitButton.Text = @"Alignment: 8";
                    alignmentSplitButton.Image = (Image)Resources.GetObject("AlignmentByte"); break;
                case AlignmentEnum.Word:
                    alignmentSplitButton.Text = @"Alignment: 16";
                    alignmentSplitButton.Image = (Image)Resources.GetObject("AlignmentWord"); break;
                default:
                {
                    string msg = $"Value {ByteAlignment} not supported on {nameof(AlignmentEnum)} enum.";
                    throw new ArgumentOutOfRangeException(nameof(ByteAlignment), ByteAlignment, msg);
                }
            }
        }

        /// <summary>Analyze the Spin packet coding, filling the object tree
        /// view.</summary>
        /// @version v22.07.01 - Added.
        private void Analyze()
        {
            if (!_colorDecodeDone)
            {
                objectTreeView.SuspendLayout();
                ColorDecoding();
                objectTreeView.ExpandAll();
                objectTreeView.ResumeLayout(true);
                _colorDecodeDone = true;
            }
            memoryView.Invalidate(true);
        }

        /// <summary>Format a frequency value to string, considering the value
        /// of FreqFormatValue.</summary>
        /// <param name="value">Value to format to string.</param>
        /// <returns>The formatted text.</returns>
        /// @version v22.07.01 - Modified parameter name.
        private string FreqFormatText(uint value)
        {
            return string.Format(_currentCultureMod, "System Frequency: {0:#,##0}  Mhz", value);
        }

        /// <summary>Update system frequency node, according to the format
        /// selected.</summary>
        /// @version v22.09.01 - Corrected to manage null instances or zero values.
        private void UpdateSystemFreq()
        {
            if (objectTreeView == null || objectTreeView.Nodes.Count == 0)
                Analyze();
            TreeNode freqNode = objectTreeView?.Nodes.OfType<TreeNode>()
                .FirstOrDefault(node =>
                {
                    if (node.Tag == null)
                        return false;
                    return node.Tag.Equals(0);
                });
            if (freqNode == null)
                Analyze();
            else
                freqNode.Text = FreqFormatText(Chip.DirectReadLong(0));
        }

        /// <summary>Method to decoding the Spin byte coding, colorizing by
        /// type.</summary>
        /// @version v22.07.01 - Modified method name to clarify its meaning,
        /// changed to use string interpolation. Added nodes for local vars
        /// and stack.
        private void ColorDecoding()
        {
            int i;
            objectTreeView.Nodes.Clear();
            //Add nodes for spin header
            TreeNode root = objectTreeView.Nodes.Add("Spin Header");
            //clock frequency node
            TreeNode node = root.Nodes.Add(FreqFormatText(Chip.DirectReadLong(0)));
            node.Tag = 0;
            node = root.Nodes.Add($"Clock Mode: ${Chip.DirectReadByte(0x4):X2}");
            node.Tag = 4;
            node = root.Nodes.Add($"Check Sum: ${Chip.DirectReadByte(0x5):X2}");
            node.Tag = 5;
            node = root.Nodes.Add($"Root Object: ${Chip.DirectReadWord(0x6):X4}");
            node.Tag = 6;
            node = root.Nodes.Add($"Variable Base: ${Chip.DirectReadWord(0x8):X4}");
            node.Tag = 8;
            node = root.Nodes.Add($"Local Frame: ${Chip.DirectReadWord(0xA):X4}");
            node.Tag = 10;
            node = root.Nodes.Add($"Entry PC: ${Chip.DirectReadWord(0xC):X4}");
            node.Tag = 12;
            node = root.Nodes.Add($"Starting Stack: ${Chip.DirectReadWord(0xE):X4}");
            node.Tag = 14;
            //header 0..15 bytes
            for (i = 0; i < 16; i++)
                _memoryColorBrush[i] = BrushesList[NodeTypeEnum.SpinHeader];

            //variables frame, ends at beginning of local frame
            for (i = Chip.DirectReadWord(0x8); i < Chip.DirectReadWord(0xA); i++)
                _memoryColorBrush[i] = BrushesList[NodeTypeEnum.VariableBody];

            //local frame, up to stack start end of RAM memory
            for (; i < Chip.DirectReadWord(0xE); i++)
                _memoryColorBrush[i] = BrushesList[NodeTypeEnum.Local];

            //local frame, up to stack start end of RAM memory
            for (; i < PropellerCPU.TotalRAM; i++)
                _memoryColorBrush[i] = BrushesList[NodeTypeEnum.Stack];

            //add node for local vars
            node = root.Nodes.Add(
                $"Local Vars ${Chip.DirectReadWord(0xA):X4} " +
                    $"(Len {Chip.DirectReadWord(0xE) - Chip.DirectReadWord(0xA):D})");
            node.Tag = (int)Chip.DirectReadWord(0xA);
            node.SelectedImageIndex = node.ImageIndex = (int)NodeTypeEnum.Local;

            //add node for stack
            node = root.Nodes.Add($"Stack ${Chip.DirectReadWord(0xE):X4}");
            node.Tag = (int)Chip.DirectReadWord(0xE);
            node.SelectedImageIndex = node.ImageIndex = (int)NodeTypeEnum.Stack;

            //decode the objects chained list
            ColorDecodingOfObject(Chip.DirectReadWord(0x6), Chip.DirectReadWord(0x8), root);
        }

        /// <summary>Decoding of a object, colorizing by type.</summary>
        /// <param name="objFrame">Start address of objects.</param>
        /// <param name="varFrame">Start address of variables.</param>
        /// <param name="lastNode">Last node of tree view.</param>
        /// @version v22.07.01 - Method name and parameter name changed
        /// to clarify their meaning. Added colorization for Function body.
        /// Changed colors used, using declaration on static constructor
        /// instead of here.
        private void ColorDecodingOfObject(uint objFrame, uint varFrame, TreeNode lastNode)
        {
            uint i;
            //add node for object
            ushort size = Chip.DirectReadWord(objFrame);
            lastNode = lastNode.Nodes.Add($"Object ${objFrame:X4} (Len {size:D})");
            lastNode.Tag = (int)objFrame;
            lastNode.SelectedImageIndex = lastNode.ImageIndex = (int)NodeTypeEnum.Object;

            //decode colors for object header
            byte longs = Chip.DirectReadByte(objFrame + 2);
            for (i = 0; i < longs * 4; i++)
                _memoryColorBrush[i + objFrame] = i == 0 ?
                    BrushesList[NodeTypeEnum.Object] :
                    BrushesList[NodeTypeEnum.ObjectHeader];
            //add node for object header
            if (longs > 0)
            {
                TreeNode longsNode = lastNode.Nodes.Add($"Obj header ${objFrame:X4} (Len {longs * 4:D})");
                longsNode.Tag = (int)objFrame;
                longsNode.SelectedImageIndex = longsNode.ImageIndex = (int)NodeTypeEnum.ObjectHeader;
            }

            //decode colors for inner object list
            byte objects = Chip.DirectReadByte(objFrame + 3);
            for (; i < (longs + objects) * 4; i++)
                _memoryColorBrush[i + objFrame] = BrushesList[NodeTypeEnum.InnerObjectsList];
            //add node for inner object list
            if (objects > 0)
            {
                TreeNode objectsNode = lastNode.Nodes.Add($"Sub Obj list ${objFrame + longs * 4:X4} (Len {objects* 4:D})");
                objectsNode.Tag = (int)(objFrame + longs * 4);
                objectsNode.SelectedImageIndex = objectsNode.ImageIndex = (int)NodeTypeEnum.InnerObjectsList;
            }

            //decode colors for variables associated to object
            _memoryColorBrush[varFrame] = BrushesList[NodeTypeEnum.Variable];
            //add node for variables associated to object
            TreeNode varsNode = lastNode.Nodes.Add($"Variable Space ${varFrame:X4}");
            varsNode.Tag = (int)varFrame;
            varsNode.SelectedImageIndex = varsNode.ImageIndex = (int)NodeTypeEnum.Variable;

            //decode colors for body of object
            for (; i < size; i++)
                _memoryColorBrush[i + objFrame] = BrushesList[NodeTypeEnum.FunctionBody];

            //decode functions of this object
            uint addressNext = Chip.DirectReadWord(1 * 4 + objFrame) + objFrame;
            for (i = 1; i < longs; i++)
            {
                uint address = addressNext;
                addressNext = Chip.DirectReadWord((i + 1) * 4 + objFrame) + objFrame;
                if (i == longs - 1)
                {
                    addressNext = address + 1;
                    while (_memoryColorBrush[addressNext] == BrushesList[NodeTypeEnum.FunctionBody])
                        addressNext++;
                }
                ColorDecodingOfFunction(address, addressNext, lastNode);
            }
            //continue decoding of inner objects
            for (i = 0; i < objects; i++)
                ColorDecodingOfObject(
                    Chip.DirectReadWord((longs + i) * 4 + objFrame) + objFrame,
                    Chip.DirectReadWord((longs + i) * 4 + 2 + objFrame) + varFrame,
                    lastNode);
        }

        /// <summary>Decoding of a function, colorizing by type.</summary>
        /// <param name="functionFrame">Start address of function.</param>
        /// <param name="functionFrameEnd">End address of function.</param>
        /// <param name="lastNode">Last node of tree view.</param>
        /// @version v22.07.01 - Method name and parameters names changed
        /// to clarify its meaning. Added colorization for Function body
        /// Changed colors used, using declaration on static constructor
        /// instead of here.
        private void ColorDecodingOfFunction(uint functionFrame, uint functionFrameEnd, TreeNode lastNode)
        {
            //add node for function
            lastNode = lastNode.Nodes.Add($"Function ${functionFrame:X} (Len {functionFrameEnd - functionFrame:d})");
            lastNode.Tag = (int)functionFrame;
            lastNode.SelectedImageIndex = lastNode.ImageIndex = (int)NodeTypeEnum.Function;
            _memoryColorBrush[functionFrame] = BrushesList[NodeTypeEnum.Function];

            //decode colors for function body
            if (functionFrameEnd - functionFrame <= 1)
                return;
            for (uint i = functionFrame + 1; i < functionFrameEnd; i++)
                _memoryColorBrush[i] = BrushesList[NodeTypeEnum.FunctionBody];
        }

        /// <summary>Draw memory header, top or bottom.</summary>
        /// <param name="memoryPos">Initial address, to print shifts from it.</param>
        /// <param name="isTopHeader">TRUE to draw top header, FALSE to draw
        ///     bottom header.</param>
        /// @version v22.09.02 - Added.
        private void DrawMemoryHeader(int memoryPos, bool isTopHeader)
        {
            SectionEnum currentSection =
                isTopHeader ? SectionEnum.TopHeader : SectionEnum.BottomHeader;
            Rectangle headerRectangle = _sectionManager.GetRectangle(currentSection);
            if (headerRectangle.IsEmpty)
                return;
            Brush brush = SystemBrushes.ControlText;
            //fill with background color
            BufferGraphics.FillRectangle(
                SystemBrushes.ControlLight, headerRectangle);
            //draw address text
            // ReSharper disable once StringLiteralTypo
            BufferGraphics.DrawString(@" Addr:", _monoFont,
                brush, 2, headerRectangle.Y);
            //loop for each shift value
            for (int x = 0, shift = memoryPos % ValuesQtyToShow, dx = _addressFontSize.Width;
                 x < ValuesQtyToShow;
                 x++, shift = (shift + 1) % ValuesQtyToShow, dx += _dataFontSize.Width)
                BufferGraphics.DrawString($"+{shift:X1}", _monoFont,
                    brush, dx, headerRectangle.Y);
        }

        /// <summary>Draw address column.</summary>
        /// <param name="memoryPos">Initial address, to print start address
        /// for each row.</param>
        /// @version v22.09.02 - Added.
        private void DrawAddressColumn(int memoryPos)
        {
            Rectangle rectangle =
                _sectionManager.GetRectangle(SectionEnum.AddressColumn);
            if (rectangle.IsEmpty)
                return;
            Brush brush = SystemBrushes.ControlText;
            int endY = rectangle.Y + rectangle.Height;
            //fill entire column space
            BufferGraphics.FillRectangle(Brushes.White, rectangle);
            // Draw each row
            for (int posY = rectangle.Y, idx = memoryPos;
                 posY < endY;
                 posY += _addressFontSize.Height, idx += ValuesQtyToShow)
                BufferGraphics.DrawString($"${idx:X4}:", _monoFont,
                    brush, 2, posY);
        }

        /// <summary>Draw memory data.</summary>
        /// <param name="memoryPos">Initial address, to print memory values
        /// from it.</param>
        /// @version v22.09.02 - Added.
        private void DrawMemoryData(int memoryPos)
        {
            Rectangle rectangle =
                _sectionManager.GetRectangle(SectionEnum.MemoryData);
            if (rectangle.IsEmpty)
                return;
            Brush brush = SystemBrushes.ControlText;
            int endX = rectangle.X + rectangle.Width;
            int endY = rectangle.Y + rectangle.Height;
            //loop for lines
            for (int posY = rectangle.Y, idx = memoryPos;
                 posY < endY;
                 posY += _dataFontSize.Height)
            {
                // Draw one line of data
                for (int posX = rectangle.X;
                     posX < endX;
                     posX += _dataFontSize.Width, idx++)
                {
                    if (_memoryColorBrush[idx] != null)
                        BufferGraphics.FillRectangle(_memoryColorBrush[idx],
                            posX, posY, _dataFontSize.Width,
                            _dataFontSize.Height);
                    BufferGraphics.DrawString(
                        Chip.DirectReadByte((uint)idx).ToString("X2"),
                        _monoFont, brush, posX, posY);
                }
            }
        }

        /// <summary>Event to repaint the plugin screen (if used).</summary>
        /// <param name="force">Flag to indicate the intention to force the
        /// repaint.</param>
        /// @version v22.09.02 - Modified to avoid boxing a byte when printing
        /// memory values.
        public override void Repaint(bool force)
        {
            if (Chip == null)
                return;
            if (BufferGraphics == null)
                return;
            int memoryPos = PositionScrollBar.Value;
            int totalLines = Math.Min(
                GetLinesOfMemoryPanel(),
                PropellerCPU.TotalRAM - memoryPos);
            bool topHeaderRequested = HeaderStyle.HasFlag(HeaderStyleEnum.OnlyTop);
            bool bottomHeaderRequested = HeaderStyle.HasFlag(HeaderStyleEnum.OnlyBottom);
            //fill the panel area
            BufferGraphics.Clear(SystemColors.Control);
            if (totalLines <=
                (topHeaderRequested ? 1 : 0) + (bottomHeaderRequested ? 1 : 0))
                return;
/*
             Parallel.Invoke(
                //draw the headers
                () =>
                {
                    if (topHeaderRequested)
                        DrawMemoryHeader(memoryPos, isTopHeader: true);
                },
                () =>
                {
                    if (bottomHeaderRequested)
                        DrawMemoryHeader(memoryPos, isTopHeader: false);
                },
                //draw address column
                () =>
                {
                    DrawAddressColumn(memoryPos);
                },
                () =>
                {
                    DrawMemoryData(memoryPos);
                });
*/
/*
             Parallel.Invoke(
                //draw the headers
                () =>
                {
                    if (topHeaderRequested)
                    {
                        DrawMemoryHeader(memoryPos, isTopHeader: true);
                    }
                    if (bottomHeaderRequested)
                    {
                        DrawMemoryHeader(memoryPos, isTopHeader: false);
                    }
                    //draw address column
                    DrawAddressColumn(memoryPos);
                },
                () =>
                {
                    DrawMemoryData(memoryPos);
                });
*/
            if (topHeaderRequested)
                DrawMemoryHeader(memoryPos, isTopHeader: true);
            if (bottomHeaderRequested)
                DrawMemoryHeader(memoryPos, isTopHeader: false);
            //draw address column
            DrawAddressColumn(memoryPos);
            //draw memory data
            DrawMemoryData(memoryPos);
/*
            bool lockTaken = false;
            try
            {
                Monitor.Enter(_bufferedGraphicsContext, ref lockTaken);
                BackBuffer.Render();
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_bufferedGraphicsContext);
            }
*/
            BackBuffer.Render();
        }

        /// <summary>Event handler to analyze the Spin packet code.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.07.01 - Modified to use new method that encapsulated
        /// logic of analyze the Spin packet code.
        private void AnalyzeButton_Click(object sender, EventArgs e)
        {
            _colorDecodeDone = false;
            Analyze();
        }

        /// <summary>Event handler when the position of scroll bar has changed.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Scroll event data arguments.</param>
        /// @version v22.07.01 - Method name changed to clarify its meaning.
        private void PositionScrollBar_PositionChanged(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
                SetAlignedPosition(e.NewValue);
            Repaint(false);
        }

        /// <summary>Event handler when the size of memory panel has changed.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.07.01 - Method name changed to clarify its meaning.
        private void MemoryView_SizeChange(object sender, EventArgs e)
        {
            if (memoryView.ClientSize.Width > 0 && memoryView.ClientSize.Height > 0)
            {
                ResetBufferGraphics();
                _sectionManager.Reset();
            }
            memoryView.Invalidate(true);
        }

        /// <summary>Event handler to paint the memory panel.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Paint event data arguments.</param>
        /// @version v22.07.01 - Method name changed to clarify its meaning.
        private void MemoryView_Paint(object sender, PaintEventArgs e)
        {
            Repaint(false);
        }

        /// <summary>Event handler to manage a mouse click on memory panel.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Mouse event data arguments.</param>
        /// @version v22.07.01 - Method name changed to clarify its meaning.
        private void MemoryView_MouseClick(object sender, MouseEventArgs e)
        {
            PositionScrollBar.Focus();
        }

        /// <summary>Event handler when the selected node of tree view
        /// has changed.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">TreeView event data arguments.</param>
        /// @version v22.07.01 - Method name changed to clarify its meaning.
        private void ObjectTreeView_SelectChanged(object sender, TreeViewEventArgs e)
        {
            SetPositionOfSelectedNode();
            Repaint(false);
        }

        /// <summary>Event handler when Free alignment Menu item is selected.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.07.01 - Added.
        private void FreeAlignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ByteAlignment = AlignmentEnum.None;
            SetAlignedPosition(PositionScrollBar.Value);
            Repaint(false);
        }

        /// <summary>Event handler when Fixed Byte alignment Menu item
        /// is selected.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.07.01 - Added.
        private void FixedByteAlignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ByteAlignment = AlignmentEnum.Byte;
            SetAlignedPosition(PositionScrollBar.Value);
            Repaint(false);
        }

        /// <summary>Event handler when Fixed Word alignment Menu item
        /// is selected.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.07.01 - Added.
        private void FixedWordAlignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ByteAlignment = AlignmentEnum.Word;
            SetAlignedPosition(PositionScrollBar.Value);
            Repaint(false);
        }
    }
}
