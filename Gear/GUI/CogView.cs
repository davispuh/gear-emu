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
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief Generic real-time cog information viewer.
    public partial class CogView : Gear.PluginSupport.PluginBase
    {
        private readonly int HostID;
        private readonly Font MonoFont;
        private readonly Font MonoFontBold;
        private Bitmap BackBuffer;
        private readonly uint[] InterpAddress;
        private readonly int StackMargin = 180;
        private uint LastLine    = 0;       //!< @brief Last line in NativeCog view.
        private uint StringX;
        private uint StringY;
        private Brush StringBrush;
        private bool displayAsHexadecimal;
        private bool useShortOpcodes;
        private FrameState breakVideo;

        /// @brief Title of the tab window.
        public override string Title
        {
            get { return "Cog " + HostID.ToString(); }
        }

        /// @brief Attribute to allow the window to be closed (default) or not (like cog windows).
        public override bool IsClosable
        {
            get { return false; }
        }

        /// @brief Identify a plugin as user (=true) or system (=false).
        /// @version v15.03.26 - Added.
        public override bool IsUserPlugin
        {
            get { return false; }
        }

        public CogView(int hostId, PropellerCPU chip) : base (chip)
        {
            HostID = hostId;

            InterpAddress = new uint[80];   // Allow for up to 80 lines of displayed interpreted text

            displayAsHexadecimal = false;
            useShortOpcodes = true;

            MonoFont = new Font(FontFamily.GenericMonospace, 10);
            if (MonoFont == null)
                MonoFont = this.Font;

            MonoFontBold = new Font(MonoFont, FontStyle.Bold);

            InitializeComponent();

            breakNone.Checked = true;
            breakMiss.Checked = false;
            breakAll.Checked = false;
            breakVideo = FrameState.FrameMiss;
        }

        public Cog GetViewCog()
        {
            return Chip.GetCog(HostID);
        }

        private void DrawString(Graphics g, string s)
        {
            g.DrawString(s, MonoFont, StringBrush, StringX, StringY);
            StringY += (uint)MonoFont.Height;
        }

        private void Repaint(bool tick, NativeCog host)
        {
            Graphics g = Graphics.FromImage((Image)BackBuffer);
            g.Clear(SystemColors.Control);
            Brush brush;

            OpcodeSize.Visible = false;
            DisplayUnits.Visible = false;
            zeroFlagLabel.Text = "Zero: " + (host.ZeroFlag ? "True" : "False");
            carryFlagLabel.Text = "Carry: " + (host.CarryFlag ? "True" : "False");

            string display;
            uint topLine, bottomLine;
            topLine = 5;
            bottomLine = (uint)((ClientRectangle.Height / MonoFont.Height) - 5);

            for (uint i = (uint)positionScroll.Value, y = 0, line = 1;
                y < ClientRectangle.Height;
                y += (uint)MonoFont.Height, i++, line++)
            {
                if (i >= Cog.TotalCogMemory)
                    continue;

                uint mem = host[(int)i];

                if (memoryViewButton.Checked)
                {
                    string binary = Convert.ToString((long)mem, 2);

                    while (binary.Length < 32)
                        binary = "0" + binary;

                    display = string.Format("{0:X4}:  {1:X8}   {2}   {1}",
                        i,
                        mem,
                        binary);
                }
                else
                {
                    display = string.Format("{0:X3}:  {2:X8}   {1}",
                        i,
                        InstructionDisassembler.AssemblyText(mem),
                        mem);
                }

                if ((uint)positionScroll.Value + line - 1 == host.BreakPointCogCursor)
                    brush = System.Drawing.Brushes.Pink;
                else if ((!followPCButton.Checked) || (line <= topLine) || (line >= bottomLine))
                    brush = SystemBrushes.Control;
                else
                    brush = SystemBrushes.Window;
                g.FillRectangle(brush, 0, y, assemblyPanel.Width, y + MonoFont.Height);


                g.DrawString(
                    display,
                    (host.ProgramCursor == i) ? MonoFontBold : MonoFont,
                    SystemBrushes.ControlText, 0, y);
            }
        }

        private void Repaint(bool tick, InterpretedCog host)
        {
            Graphics g = Graphics.FromImage((Image)BackBuffer);
            Brush brush;

            g.Clear(SystemColors.Control);

            string display;
            uint topLine, bottomLine;
            topLine = 5;
            bottomLine = (uint)((ClientRectangle.Height / MonoFont.Height) - 5);

            zeroFlagLabel.Text = string.Empty;
            carryFlagLabel.Text = string.Empty;
            OpcodeSize.Visible = true;
            DisplayUnits.Visible = true;

            if (memoryViewButton.Checked)
            {
                for (uint i = (uint)positionScroll.Value, y = 0;
                    y < ClientRectangle.Height;
                    y += (uint)MonoFont.Height, i++)
                {
                    if (i > PropellerCPU.MaxRAMAddress)
                        continue;

                    uint mem = host[(int)i];

                    string binary = Convert.ToString((long)mem, 2);

                    while (binary.Length < 32)
                        binary = "0" + binary;

                    display = string.Format("{0:X4}:  {1:X8}   {2}   ",
                              i, mem, binary);
                    if (displayAsHexadecimal)
                        display += string.Format("{0:X8}", mem);
                    else
                        display += string.Format("{0}", mem);

                    g.FillRectangle(SystemBrushes.Control, 0, y, assemblyPanel.Width, y + MonoFont.Height);

                    g.DrawString(
                        display,
                        (host.ProgramCursor == i) ? MonoFontBold : MonoFont,
                        SystemBrushes.ControlText, 0, y);
                }
            }
            else
            {
                uint y = 0;

                for (uint i = (uint)positionScroll.Value, line = 1;
                    y < ClientRectangle.Height;
                    y += (uint)MonoFont.Height, line++)
                {
                    if (i > PropellerCPU.MaxRAMAddress)
                        continue;

                    uint start = i;

                    Propeller.MemoryManager mem = new Propeller.MemoryManager(Chip, i);
                    string inst = InstructionDisassembler.InterpreterText(mem, displayAsHexadecimal, useShortOpcodes);
                    i = mem.Address;
                    display = string.Format("{0:X4}: ", start);
                    InterpAddress[line] = start;

                    for (uint q = start; q < start + 4; q++)
                    {
                        if (q < i)
                        {
                            byte b = Chip.DirectReadByte(q);
                            display += string.Format(" {0:X2}", b);
                        }
                        else
                            display += "   ";
                    }


                    display += "  " + inst;

                    if (InterpAddress[line] == host.BreakPointCogCursor)
                        brush = System.Drawing.Brushes.Pink;
                    else if ((!followPCButton.Checked) || (line <= topLine) || (line >= bottomLine))
                        brush = SystemBrushes.Control;
                    else
                        brush = SystemBrushes.Window;
                    g.FillRectangle(brush, 0, y, assemblyPanel.Width, y + MonoFont.Height);

                    g.DrawString(
                        display,
                        (host.ProgramCursor == start) ? MonoFontBold : MonoFont,
                        SystemBrushes.ControlText, 0, y);
                }

                StringBrush = SystemBrushes.ControlText;
                StringY = 0;
                StringX = (uint)(assemblyPanel.Width - StackMargin);

                DrawString(g, string.Format("@Stk[0] = ${0:X4} {0}", host.StackFrame));
                DrawString(g, string.Format("@Obj[0] = ${0:X4} {0}", host.ObjectFrame));
                DrawString(g, string.Format("@Loc[0] = ${0:X4} {0}", host.LocalFrame));
                DrawString(g, string.Format("@Var[0] = ${0:X4} {0}", host.VariableFrame));
                g.DrawLine(Pens.Black, assemblyPanel.Width - StackMargin, StringY, assemblyPanel.Width, StringY);
                DrawString(g, string.Format("Caller& = ${0:X4} {0}", Chip.DirectReadWord(host.LocalFrame - 8)));
                DrawString(g, string.Format("          ${0:X4} {0}", Chip.DirectReadWord(host.LocalFrame - 6)));
                DrawString(g, string.Format("          ${0:X4} {0}", Chip.DirectReadWord(host.LocalFrame - 4)));
                DrawString(g, string.Format("Return& = ${0:X4}", Chip.DirectReadWord(host.LocalFrame - 2)));
                g.DrawLine(Pens.Black, assemblyPanel.Width - StackMargin, StringY, assemblyPanel.Width, StringY);

                for (uint i = host.LocalFrame; i < host.StackFrame && StringY < ClientRectangle.Height; i += 4)
                {
                    DrawString(g, string.Format("${0:X8}  {0}", (int)Chip.DirectReadLong(i)));
                }
            }
        }

        /// @brief Repaint the Cog state and data.
        /// @param force
        public override void Repaint(bool force)
        {
            if (Chip == null)
                return;

            Cog Host = Chip.GetCog(HostID);

            if (Host == null)
            {
                processorStateLabel.Text = "CPU State: Cog is stopped.";
                programCounterLabel.Text = string.Empty;
                zeroFlagLabel.Text = string.Empty;
                carryFlagLabel.Text = string.Empty;
                return;
            }

            positionScroll.Minimum = 0;

            if (Host is InterpretedCog)
                positionScroll.Maximum = PropellerCPU.MaxRAMAddress;
            else if (Host is NativeCog)
                positionScroll.Maximum = Cog.TotalCogMemory;

            positionScroll.LargeChange = 10;
            positionScroll.SmallChange = 1;

            if (positionScroll.Maximum < positionScroll.Value)
                positionScroll.Value = positionScroll.Maximum;

            if (followPCButton.Checked)
            {
                uint topLine, bottomLine;
                topLine = 5;
                bottomLine = (uint)((ClientRectangle.Height / MonoFont.Height) - 5);
                if (Host is NativeCog)
                {
                    if (Host.ProgramCursor < topLine)
                        positionScroll.Value = 0;
                    else if (Host.ProgramCursor - positionScroll.Value >= bottomLine - 1)
                        positionScroll.Value = (int)Host.ProgramCursor - (int)topLine;
                    else if (Host.ProgramCursor - positionScroll.Value < topLine)
                        positionScroll.Value = (int)Host.ProgramCursor - (int)topLine;
                }
                else
                    positionScroll.Value = (int)Host.ProgramCursor;
            }

            if (Host is NativeCog cog)
                Repaint(force, cog);
            else if (Host is InterpretedCog _cog)
                Repaint(force, _cog);

            programCounterLabel.Text = "PC: " + string.Format("{0:X8}", Host.ProgramCursor);
            processorStateLabel.Text = "CPU State: " + Host.CogStateString; // + Host.VideoStateString;
            frameCountLabel.Text = "Frames: " + Host.VideoFrameString;
            // frameCountLabel.Text = String.Format("Frames: {0}", Host.VideoFrames);

            assemblyPanel.CreateGraphics().DrawImageUnscaled(BackBuffer, 0, 0);
        }

        private void UpdateOnScroll(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        private void AssemblyView_Paint(object sender, PaintEventArgs e)
        {
            assemblyPanel.CreateGraphics().DrawImageUnscaled(BackBuffer, 0, 0);
        }

        private void AsmSized(object sender, EventArgs e)
        {
            if (assemblyPanel.Width > 0 && assemblyPanel.Height > 0)
                BackBuffer = new Bitmap(
                    assemblyPanel.Width,
                    assemblyPanel.Height);
            else
                BackBuffer = new Bitmap(1, 1);
            Repaint(false);
        }

        private void MemoryViewButton_Click(object sender, EventArgs e)
        {
            Repaint(false);
        }

        private void AssemblyPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //Make sure it's a valid breakpoint environment
                if (Chip == null) return;
                Cog Host = Chip.GetCog(HostID);
                if (Host == null) return;
                //Find the line that was clicked on
                int bp = (assemblyPanel.PointToClient(MousePosition).Y / MonoFont.Height);
                //What type of cog?
                if (Host is NativeCog) bp += positionScroll.Value;
                else if (Host is InterpretedCog) bp = (int)InterpAddress[bp + 1];
                //Toggle/move the breakpoint
                if (bp == Host.BreakPointCogCursor) Host.BreakPointCogCursor = -1;
                else Host.BreakPointCogCursor = bp;
                //Show the user what happened
                Repaint(false);
            }
        }

        private void FollowPCButton_Click(object sender, EventArgs e)
        {
            Repaint(false);
        }

        private void HexadecimalUnits_Click(object sender, EventArgs e)
        {
            displayAsHexadecimal = true;
            hexadecimalUnits.Checked = true;
            decimalUnits.Checked = false;
            Repaint(false);
        }

        private void DecimalUnits_Click(object sender, EventArgs e)
        {
            displayAsHexadecimal = false;
            hexadecimalUnits.Checked = false;
            decimalUnits.Checked = true;
            Repaint(false);
        }

        private void LongOpcodes_Click(object sender, EventArgs e)
        {
            useShortOpcodes = false;
            longOpcodes.Checked = true;
            shortOpcodes.Checked = false;
            Repaint(false);
        }

        private void ShortOpcodes_Click(object sender, EventArgs e)
        {
            useShortOpcodes = true;
            longOpcodes.Checked = false;
            shortOpcodes.Checked = true;
            Repaint(false);
        }

        private void AssemblyPanel_MouseClick(object sender, MouseEventArgs e)
        {
            positionScroll.Focus();
        }

        private void AssemblyPanel_MouseHover(object sender, EventArgs e)
        {

        }

        private void AssemblyPanel_MouseMove(object sender, MouseEventArgs e)
        {
            uint line;
            uint mem;
            if (!(Chip.GetCog(HostID) is NativeCog))
                return;
            NativeCog host = (NativeCog)Chip.GetCog(HostID);
            line = (uint)positionScroll.Value + (uint)(e.Y / MonoFont.Height);
            if (line >= Cog.TotalCogMemory)
                return;

            //Update tooltip only if line has change to prevent flickering
            if (line != LastLine)
            {
                mem = host.ReadLong(line);
                toolTip1.SetToolTip(assemblyPanel, string.Format(
                    "${0:x3}= ${1:x8}, {1}\n${2:x3}= ${3:x8}, {3}",
                    mem >> 9 & 0x1ff, host.ReadLong(mem >> 9 & 0x1ff),
                    mem      & 0x1ff, host.ReadLong(mem      & 0x1ff))
                );
                LastLine = line;
            }
        }

        public void VideoBreak_Click(object sender, EventArgs e)
        {
            if (sender == breakNone)
            {
                breakNone.Checked = true;
                breakMiss.Checked = false;
                breakAll.Checked = false;
                breakVideo = FrameState.FrameMiss;
            }
            if (sender == breakMiss)
            {
                breakNone.Checked = false;
                breakMiss.Checked = true;
                breakAll.Checked = false;
                breakVideo = FrameState.FrameMiss;
            }
            if (sender == breakAll)
            {
                breakNone.Checked = true;
                breakMiss.Checked = false;
                breakAll.Checked = false;
                breakVideo = FrameState.FrameNone;
            }
            Cog cog = Chip.GetCog(HostID);
            cog.SetVideoBreak(breakVideo);
        }

    }
}
