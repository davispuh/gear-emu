/*

    Windows Forms Collapsible Splitter Control for .Net
    (c)Copyright 2002-2003 NJF (furty74@yahoo.com). All rights reserved.

    Assembly Build Dependencies:
    CollapsibleSplitter.bmp

    Version 1.1 Changes:
    OnPaint is now overridden instead of being a handled event, and the entire splitter is now painted rather than just the collpaser control
    The splitter rectangle is now correctly defined
    The Collapsed property was renamed to IsCollapsed, and the code changed so that no value needs to be set
    New visual styles added: Win9x, XP, DoubleDots and Lines

    Version 1.11 Changes:
    The OnMouseMove event handler was updated to address a flickering issue discovered by John O'Byrne

    Version 1.2 Changes:
    Added support for horizontal splitters

    Version 1.21 Changes:
    Added support for inclusion as a VS.Net ToolBox control
    Added a ToolBox bitmap
    Removed extraneous overrides
    Added summaries

    Version 1.22 Changes:
    Removed the ParentFolder from public properties - this is now set automatically in the OnHandleCreated event
    *Added expand/collapse animation code

    Version 1.3 Changes:
    Added an optional 3D border
    General code and comment cleaning
    Flagged assembly with the CLSCompliant attribute
    Added a simple designer class to filter unwanted properties

*/

namespace Gear.GUI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    #region Enums
    /// <summary>Enumeration to specify the visual style to be applied to the
    /// CollapsibleSplitter control.</summary>
    /// @version v22.06.01 - Changed enum name to clarify its meaning.
    public enum VisualStylesEnum
    {
        Mozilla = 0,
        XP,
        Win9x,
        DoubleDots,
        Lines
    }

    /// <summary>
    /// Enumeration to specify the current animation state of the control.
    /// </summary>
    public enum SplitterState
    {
        Collapsed = 0,
        Expanding,
        Expanded,
        Collapsing
    }

    #endregion

    /// <summary>
    /// A custom collapsible splitter that can resize, hide and show associated form controls
    /// </summary>
    [ToolboxBitmap(typeof(CollapsibleSplitter))]
    [Designer(typeof(CollapsibleSplitterDesigner))]
    public class CollapsibleSplitter : Splitter
    {
        #region Private Properties

        // declare and define some base properties

        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private bool _hot;
        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private readonly Color _hotColor = CalculateColor(SystemColors.Highlight, SystemColors.Window, 70);
        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private Rectangle _rr;
        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private Form _parentForm;
        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private VisualStylesEnum _visualStyle;

        // Border added in version 1.3
        /// <summary></summary>
        private Border3DStyle _borderStyle = Border3DStyle.Flat;

        // animation controls introduced in version 1.22

        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private readonly Timer _animationTimer;
        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private int _controlWidth;
        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private int _controlHeight;
        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private int _parentFormWidth;
        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private int _parentFormHeight;
        /// <summary></summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private SplitterState _currentState;

        #endregion

        #region Public Properties

        /// <summary>
        /// The initial state of the Splitter. Set to True if the control to hide is not visible by default
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue("False"),
        Description("The initial state of the Splitter. Set to True if the control to hide is not visible by default")]
        public bool IsCollapsed =>
            ControlToHide == null || !ControlToHide.Visible;

        /// <summary>
        /// The System.Windows.Forms.Control that the splitter will collapse
        /// </summary>
        /// @version v22.06.01 - Changed to a property with auto value, absorbing removed private member `controlToHide`.
        [Bindable(true), Category("Collapsing Options"), DefaultValue(""),
        Description("The System.Windows.Forms.Control that the splitter will collapse")]
        public Control ControlToHide { get; set; }

        /// <summary>
        /// Determines if the collapse and expanding actions will be animated
        /// </summary>
        /// @version v22.06.01 - Changed to a property with auto value, absorbing removed private member `useAnimations`.
        [Bindable(true), Category("Collapsing Options"), DefaultValue("True"),
         Description("Determines if the collapse and expanding actions will be animated")]
        public bool UseAnimations { get; set; }

        /// <summary>
        /// The delay in milliseconds between animation steps
        /// </summary>
        /// @version v22.06.01 - Changed to a property with auto value, absorbing removed private member `animationDelay`.
        [Bindable(true), Category("Collapsing Options"), DefaultValue("20"),
         Description("The delay in millisenconds between animation steps")]
        public int AnimationDelay { get; set; } = 20;

        /// <summary>
        /// The amount of pixels moved in each animation step
        /// </summary>
        /// @version v22.06.01 - Changed to a property with auto value, absorbing removed private member `animationStep`.
        [Bindable(true), Category("Collapsing Options"), DefaultValue("20"),
         Description("The amount of pixels moved in each animation step")]
        public int AnimationStep { get; set; } = 20;

        /// <summary>
        /// When true the entire parent form will be expanded and collapsed, otherwise just the control to expand will be changed
        /// </summary>
        /// @version v22.06.01 - Changed to a property with auto value, absorbing removed private member `expandParentForm`.
        [Bindable(true), Category("Collapsing Options"), DefaultValue("False"),
         Description("When true the entire parent form will be expanded and collapsed, otherwise just the contol to expand will be changed")]
        public bool ExpandParentForm { get; set; }

        /// <summary>
        /// The visual style that will be painted on the control
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue("VisualStyles.XP"),
         Description("The visual style that will be painted on the control")]
        public VisualStylesEnum VisualStyle
        {
            get => _visualStyle;
            set
            {
                if (_visualStyle == value)
                    return;
                _visualStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// An optional border style to paint on the control. Set to Flat for no border
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue("System.Windows.Forms.Border3DStyle.Flat"),
         Description("An optional border style to paint on the control. Set to Flat for no border")]
        public Border3DStyle BorderStyle3D
        {
            get => _borderStyle;
            set
            {
                if (_borderStyle == value)
                    return;
                _borderStyle = value;
                Invalidate();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///
        /// </summary>
        public void ToggleState()
        {
            ToggleSplitter();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CollapsibleSplitter()
        {
            // Register mouse events
            Click += OnClick;
            Resize += OnResize;
            MouseLeave += OnMouseLeave;
            MouseMove += OnMouseMove;

            // eliminate flicker (from comment of 'Member 1430126' 17-Aug-11)
            DoubleBuffered = true;

            // Setup the animation timer control
            _animationTimer = new Timer
            {
                Interval = AnimationDelay
            };
            _animationTimer.Tick += AnimationTimerTick;
        }

        #endregion

        #region Overrides

        /// <summary>
        ///
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _animationTimer.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _parentForm = FindForm();

            // set the current state
            if (ControlToHide != null)
                _currentState = ControlToHide.Visible ?
                    SplitterState.Expanded :
                    SplitterState.Collapsed;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        /// <summary>
        ///
        /// </summary>
        /// @version v22.06.01 - Added to prevent warning 'Virtual member call in constructor'.
        protected sealed override bool DoubleBuffered
        {
            get => base.DoubleBuffered;
            set => base.DoubleBuffered = value;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        ///
        /// </summary>
        /// <param name="e">Mouse event data arguments.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // if the hider control isn't hot, let the base resize action occur
            if (ControlToHide == null || _hot || !ControlToHide.Visible)
                return;
            base.OnMouseDown(e);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void OnResize(object sender, EventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        ///
        /// </summary>
        /// This method was updated in version 1.11 to fix a flickering problem
        /// discovered by John O'Byrne
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Mouse event data arguments.</param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // check to see if the mouse cursor position is within the bounds of our control
            if (e.X >= _rr.X && e.X <= _rr.X + _rr.Width && e.Y >= _rr.Y && e.Y <= _rr.Y + _rr.Height)
            {
                if (!_hot)
                {
                    _hot = true;
                    Cursor = Cursors.Hand;
                    Invalidate();
                }
            }
            else
            {
                if (_hot)
                {
                    _hot = false;
                    Invalidate();
                }

                Cursor = Cursors.Default;

                if (ControlToHide == null)
                    return;
                if (!ControlToHide.Visible)
                    Cursor = Cursors.Default;
                else // Changed in v1.2 to support Horizontal Splitters
                {
                    Cursor = (Dock == DockStyle.Left || Dock == DockStyle.Right) ?
                        Cursors.VSplit :
                        Cursors.HSplit;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void OnMouseLeave(object sender, EventArgs e)
        {
            // ensure that the hot state is removed
            _hot = false;
            Invalidate();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void OnClick(object sender, EventArgs e)
        {
            if (ControlToHide != null && _hot &&
                _currentState != SplitterState.Collapsing &&
                _currentState != SplitterState.Expanding)
            {
                ToggleSplitter();
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void ToggleSplitter()
        {

            // if an animation is currently in progress for this control, drop out
            if (_currentState == SplitterState.Collapsing || _currentState == SplitterState.Expanding)
                return;

            _controlWidth = ControlToHide.Width;
            _controlHeight = ControlToHide.Height;

            if (ControlToHide.Visible)
            {
                if (UseAnimations)
                {
                    _currentState = SplitterState.Collapsing;

                    if (_parentForm != null)
                    {
                        if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                            _parentFormWidth = _parentForm.Width - _controlWidth;
                        else
                            _parentFormHeight = _parentForm.Height - _controlHeight;
                    }
                    _animationTimer.Enabled = true;
                }
                else
                {
                    // no animations, so just toggle the visible state
                    _currentState = SplitterState.Collapsed;
                    ControlToHide.Visible = false;
                    if (ExpandParentForm && _parentForm != null)
                    {
                        if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                            _parentForm.Width -= ControlToHide.Width;
                        else
                            _parentForm.Height -= ControlToHide.Height;
                    }
                }
            }
            else
            {
                // control to hide is collapsed
                if (UseAnimations)
                {
                    _currentState = SplitterState.Expanding;

                    if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                    {
                        if (_parentForm != null)
                            _parentFormWidth = _parentForm.Width + _controlWidth;
                        ControlToHide.Width = 0;
                    }
                    else
                    {
                        if (_parentForm != null)
                            _parentFormHeight = _parentForm.Height + _controlHeight;
                        ControlToHide.Height = 0;
                    }
                    ControlToHide.Visible = true;
                    _animationTimer.Enabled = true;
                }
                else
                {
                    // no animations, so just toggle the visible state
                    _currentState = SplitterState.Expanded;
                    ControlToHide.Visible = true;
                    if (ExpandParentForm && _parentForm != null)
                    {
                        if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                            _parentForm.Width += ControlToHide.Width;
                        else
                            _parentForm.Height += ControlToHide.Height;
                    }
                }
            }

        }

        #endregion

        #region Implementation

        #region Animation Timer Tick

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void AnimationTimerTick(object sender, EventArgs e)
        {
            switch (_currentState)
            {
                case SplitterState.Collapsing:

                    if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                    {
                        // vertical splitter
                        if (ControlToHide.Width > AnimationStep)
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                                _parentForm.Width -= AnimationStep;
                            ControlToHide.Width -= AnimationStep;
                        }
                        else
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                            {
                                _parentForm.Width = _parentFormWidth;
                            }
                            ControlToHide.Visible = false;
                            _animationTimer.Enabled = false;
                            ControlToHide.Width = _controlWidth;
                            _currentState = SplitterState.Collapsed;
                            Invalidate();
                        }
                    }
                    else
                    {
                        // horizontal splitter
                        if (ControlToHide.Height > AnimationStep)
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                                _parentForm.Height -= AnimationStep;
                            ControlToHide.Height -= AnimationStep;
                        }
                        else
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                                _parentForm.Height = _parentFormHeight;
                            ControlToHide.Visible = false;
                            _animationTimer.Enabled = false;
                            ControlToHide.Height = _controlHeight;
                            _currentState = SplitterState.Collapsed;
                            Invalidate();
                        }
                    }
                    break;

                case SplitterState.Expanding:

                    if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                    {
                        // vertical splitter
                        if (ControlToHide.Width < (_controlWidth - AnimationStep))
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized &&
                                _parentForm != null)
                                _parentForm.Width += AnimationStep;
                            ControlToHide.Width += AnimationStep;
                        }
                        else
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized &&
                                _parentForm != null)
                                _parentForm.Width = _parentFormWidth;
                            ControlToHide.Width = _controlWidth;
                            ControlToHide.Visible = true;
                            _animationTimer.Enabled = false;
                            _currentState = SplitterState.Expanded;
                            Invalidate();
                        }
                    }
                    else
                    {
                        // horizontal splitter
                        if (ControlToHide.Height < (_controlHeight - AnimationStep))
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized &&
                                _parentForm != null)
                                _parentForm.Height += AnimationStep;
                            ControlToHide.Height += AnimationStep;
                        }
                        else
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized &&
                                _parentForm != null)
                                _parentForm.Height = _parentFormHeight;
                            ControlToHide.Height = _controlHeight;
                            ControlToHide.Visible = true;
                            _animationTimer.Enabled = false;
                            _currentState = SplitterState.Expanded;
                            Invalidate();
                        }

                    }
                    break;
            }
        }

        #endregion

        #region Paint the control

        /// <summary>
        /// OnPaint is now an override rather than an event in version 1.1
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        /// @version v22.06.01 - Local variable names changed to clarify meaning of it. Also throw new and more specific exceptions.
        protected override void OnPaint(PaintEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            // create a Graphics object
            Graphics graph = e.Graphics;

            // find the rectangle for the splitter and paint it
            Rectangle rectangle = ClientRectangle; // fixed in version 1.1
            graph.FillRectangle(new SolidBrush(BackColor), rectangle);

            #region Vertical Splitter
            // Check the docking style and create the control rectangle accordingly
            if (Dock == DockStyle.Left || Dock == DockStyle.Right)
            {
                // create a new rectangle in the vertical center of the splitter for our collapse control button
                _rr = new Rectangle(rectangle.X, rectangle.Y + ((rectangle.Height - 115) / 2), 8, 115);
                // force the width to 8px so that everything always draws correctly
                Width = 8;

                // draw the background color for our control image
                graph.FillRectangle(_hot ? new SolidBrush(_hotColor) : new SolidBrush(BackColor),
                    new Rectangle(_rr.X + 1, _rr.Y, 6, 115));

                // draw the top & bottom lines for our control image
                graph.DrawLine(new Pen(SystemColors.ControlDark, 1), _rr.X + 1, _rr.Y, _rr.X + _rr.Width - 2, _rr.Y);
                graph.DrawLine(new Pen(SystemColors.ControlDark, 1), _rr.X + 1, _rr.Y + _rr.Height, _rr.X + _rr.Width - 2, _rr.Y + _rr.Height);

                if (Enabled)
                {
                    // draw the arrows for our control image
                    // the ArrowPointArray is a point array that defines an arrow shaped polygon
                    graph.FillPolygon(new SolidBrush(SystemColors.ControlDarkDark), ArrowPointArray(_rr.X + 2, _rr.Y + 3));
                    graph.FillPolygon(new SolidBrush(SystemColors.ControlDarkDark), ArrowPointArray(_rr.X + 2, _rr.Y + _rr.Height - 9));
                }

                // draw the dots for our control image using a loop
                int x = _rr.X + 3;
                int y = _rr.Y + 14;

                // Visual Styles added in version 1.1
                switch (_visualStyle)
                {
                    case VisualStylesEnum.Mozilla:

                        for (int i = 0; i < 30; i++)
                        {
                            // light dot
                            graph.DrawLine(new Pen(SystemColors.ControlLightLight),
                                x, y + (i * 3), x + 1, y + 1 + (i * 3));
                            // dark dot
                            graph.DrawLine(new Pen(SystemColors.ControlDarkDark),
                                x + 1, y + 1 + (i * 3), x + 2, y + 2 + (i * 3));
                            // overdraw the background color as we actually drew 2px diagonal lines, not just dots
                            graph.DrawLine(_hot ? new Pen(_hotColor) : new Pen(BackColor),
                                x + 2, y + 1 + (i * 3), x + 2, y + 2 + (i * 3));
                        }
                        break;

                    case VisualStylesEnum.DoubleDots:
                        for (int i = 0; i < 30; i++)
                        {
                            // light dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlLightLight), x, y + 1 + (i * 3), 1, 1);
                            // dark dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlDark), x - 1, y + (i * 3), 1, 1);
                            i++;
                            // light dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlLightLight), x + 2, y + 1 + (i * 3), 1, 1);
                            // dark dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlDark), x + 1, y + (i * 3), 1, 1);
                        }
                        break;

                    case VisualStylesEnum.Win9x:

                        graph.DrawLine(new Pen(SystemColors.ControlLightLight), x, y, x + 2, y);
                        graph.DrawLine(new Pen(SystemColors.ControlLightLight), x, y, x, y + 90);
                        graph.DrawLine(new Pen(SystemColors.ControlDark), x + 2, y, x + 2, y + 90);
                        graph.DrawLine(new Pen(SystemColors.ControlDark), x, y + 90, x + 2, y + 90);
                        break;

                    case VisualStylesEnum.XP:

                        for (int i = 0; i < 18; i++)
                        {
                            // light dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlLight), x, y + (i * 5), 2, 2);
                            // light light dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlLightLight), x + 1, y + 1 + (i * 5), 1, 1);
                            // dark dark dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlDarkDark), x, y + (i * 5), 1, 1);
                            // dark fill
                            graph.DrawLine(new Pen(SystemColors.ControlDark), x, y + (i * 5), x, y + (i * 5) + 1);
                            graph.DrawLine(new Pen(SystemColors.ControlDark), x, y + (i * 5), x + 1, y + (i * 5));
                        }
                        break;

                    case VisualStylesEnum.Lines:

                        for (int i = 0; i < 44; i++)
                            graph.DrawLine(new Pen(SystemColors.ControlDark), x, y + (i * 2), x + 2, y + (i * 2));

                        break;
                }

                // Added in version 1.3
                if (_borderStyle != Border3DStyle.Flat)
                {
                    // Paint the control border
                    ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, _borderStyle, Border3DSide.Left);
                    ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, _borderStyle, Border3DSide.Right);
                }
            }

            #endregion

            // Horizontal Splitter support added in v1.2

            #region Horizontal Splitter

            else if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
            {
                // create a new rectangle in the horizontal center of the splitter for our collapse control button
                _rr = new Rectangle(rectangle.X + ((rectangle.Width - 115) / 2), rectangle.Y, 115, 8);
                // force the height to 8px
                Height = 8;

                // draw the background color for our control image
                graph.FillRectangle(_hot ? new SolidBrush(_hotColor) : new SolidBrush(BackColor),
                    new Rectangle(_rr.X, _rr.Y + 1, 115, 6));

                // draw the left & right lines for our control image
                graph.DrawLine(new Pen(SystemColors.ControlDark, 1), _rr.X, _rr.Y + 1, _rr.X, _rr.Y + _rr.Height - 2);
                graph.DrawLine(new Pen(SystemColors.ControlDark, 1), _rr.X + _rr.Width, _rr.Y + 1, _rr.X + _rr.Width, _rr.Y + _rr.Height - 2);

                if (Enabled)
                {
                    // draw the arrows for our control image
                    // the ArrowPointArray is a point array that defines an arrow shaped polygon
                    graph.FillPolygon(new SolidBrush(SystemColors.ControlDarkDark), ArrowPointArray(_rr.X + 3, _rr.Y + 2));
                    graph.FillPolygon(new SolidBrush(SystemColors.ControlDarkDark), ArrowPointArray(_rr.X + _rr.Width - 9, _rr.Y + 2));
                }

                // draw the dots for our control image using a loop
                int x = _rr.X + 14;
                int y = _rr.Y + 3;

                // Visual Styles added in version 1.1
                switch (_visualStyle)
                {
                    case VisualStylesEnum.Mozilla:
                        for (int i = 0; i < 30; i++)
                        {
                            // light dot
                            graph.DrawLine(new Pen(SystemColors.ControlLightLight), x + (i * 3), y, x + 1 + (i * 3), y + 1);
                            // dark dot
                            graph.DrawLine(new Pen(SystemColors.ControlDarkDark), x + 1 + (i * 3), y + 1, x + 2 + (i * 3), y + 2);
                            // overdraw the background color as we actually drew 2px diagonal lines, not just dots
                            graph.DrawLine(_hot ? new Pen(_hotColor) : new Pen(BackColor), x + 1 + (i * 3), y + 2,
                                x + 2 + (i * 3), y + 2);
                        }
                        break;

                    case VisualStylesEnum.DoubleDots:
                        for (int i = 0; i < 30; i++)
                        {
                            // light dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlLightLight), x + 1 + (i * 3), y, 1, 1);
                            // dark dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlDark), x + (i * 3), y - 1, 1, 1);
                            i++;
                            // light dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlLightLight), x + 1 + (i * 3), y + 2, 1, 1);
                            // dark dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlDark), x + (i * 3), y + 1, 1, 1);
                        }
                        break;

                    case VisualStylesEnum.Win9x:
                        graph.DrawLine(new Pen(SystemColors.ControlLightLight), x, y, x, y + 2);
                        graph.DrawLine(new Pen(SystemColors.ControlLightLight), x, y, x + 88, y);
                        graph.DrawLine(new Pen(SystemColors.ControlDark), x, y + 2, x + 88, y + 2);
                        graph.DrawLine(new Pen(SystemColors.ControlDark), x + 88, y, x + 88, y + 2);
                        break;

                    case VisualStylesEnum.XP:
                        for (int i = 0; i < 18; i++)
                        {
                            // light dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlLight), x + (i * 5), y, 2, 2);
                            // light light dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlLightLight), x + 1 + (i * 5), y + 1, 1, 1);
                            // dark dark dot
                            graph.DrawRectangle(new Pen(SystemColors.ControlDarkDark), x + (i * 5), y, 1, 1);
                            // dark fill
                            graph.DrawLine(new Pen(SystemColors.ControlDark), x + (i * 5), y, x + (i * 5) + 1, y);
                            graph.DrawLine(new Pen(SystemColors.ControlDark), x + (i * 5), y, x + (i * 5), y + 1);
                        }
                        break;

                    case VisualStylesEnum.Lines:
                        for (int i = 0; i < 44; i++)
                            graph.DrawLine(new Pen(SystemColors.ControlDark), x + (i * 2), y, x + (i * 2), y + 2);
                        break;
                }

                // Added in version 1.3
                if (_borderStyle != Border3DStyle.Flat)
                {
                    // Paint the control border
                    ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, _borderStyle, Border3DSide.Top);
                    ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, _borderStyle, Border3DSide.Bottom);
                }
            }

            #endregion

            else
            {
                throw new InvalidEnumArgumentException(
                    "The Collapsible Splitter control cannot have the Filled " +
                    "or None Dockstyle property");
            }

            // dispose the Graphics object
            // eliminate flicker (from comment of 'Member 1430126' 17-Aug-11)
            //g.Dispose();
        }
        #endregion

        #region Arrow Polygon Array

        /// <summary>
        /// This creates a point array to draw a arrow-like polygon
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Point[] ArrowPointArray(int x, int y)
        {
            Point[] point = new Point[3];

            if (ControlToHide != null)
            {
                // decide which direction the arrow will point
                if ((Dock == DockStyle.Right && ControlToHide.Visible) ||
                    (Dock == DockStyle.Left && !ControlToHide.Visible))
                {
                    // right arrow
                    point[0] = new Point(x, y);
                    point[1] = new Point(x + 3, y + 3);
                    point[2] = new Point(x, y + 6);
                }
                else if ((Dock == DockStyle.Right && !ControlToHide.Visible) ||
                    (Dock == DockStyle.Left && ControlToHide.Visible))
                {
                    // left arrow
                    point[0] = new Point(x + 3, y);
                    point[1] = new Point(x, y + 3);
                    point[2] = new Point(x + 3, y + 6);
                }

                    // Up/Down arrows added in v1.2

                else if ((Dock == DockStyle.Top && ControlToHide.Visible) ||
                    (Dock == DockStyle.Bottom && !ControlToHide.Visible))
                {
                    // up arrow
                    point[0] = new Point(x + 3, y);
                    point[1] = new Point(x + 6, y + 4);
                    point[2] = new Point(x, y + 4);
                }
                else if ((Dock == DockStyle.Top && !ControlToHide.Visible) ||
                    (Dock == DockStyle.Bottom && ControlToHide.Visible))
                {
                    // down arrow
                    point[0] = new Point(x, y);
                    point[1] = new Point(x + 6, y);
                    point[2] = new Point(x + 3, y + 3);
                }
            }

            return point;
        }

        #endregion

        #region Color Calculator

        /// <summary>
        /// This method was borrowed from the RichUI Control library by Sajith M
        /// </summary>
        /// <param name="front"></param>
        /// <param name="back"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        private static Color CalculateColor(Color front, Color back, int alpha)
        {
            // solid color obtained as a result of alpha-blending

            Color frontColor = Color.FromArgb(255, front);
            Color backColor = Color.FromArgb(255, back);

            float frontRed = frontColor.R;
            float frontGreen = frontColor.G;
            float frontBlue = frontColor.B;
            float backRed = backColor.R;
            float backGreen = backColor.G;
            float backBlue = backColor.B;

            float fRed = frontRed * alpha / 255 + backRed * ((float)(255 - alpha) / 255);
            byte newRed = (byte)fRed;
            float fGreen = frontGreen * alpha / 255 + backGreen * ((float)(255 - alpha) / 255);
            byte newGreen = (byte)fGreen;
            float fBlue = frontBlue * alpha / 255 + backBlue * ((float)(255 - alpha) / 255);
            byte newBlue = (byte)fBlue;

            return Color.FromArgb(255, newRed, newGreen, newBlue);
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// A simple designer class for the CollapsibleSplitter control to remove
    /// unwanted properties at design time.
    /// </summary>
    public class CollapsibleSplitterDesigner : System.Windows.Forms.Design.ControlDesigner
    {
        /// <summary>
        ///
        /// </summary>
        public CollapsibleSplitterDesigner() { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="properties"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.06.01 - Throw new exception.
        protected override void PreFilterProperties(System.Collections.IDictionary properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            properties.Remove("IsCollapsed");
            properties.Remove("BorderStyle");
            properties.Remove("Size");
        }
    }
}
