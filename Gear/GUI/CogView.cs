/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Gear.EmulationCore;
using Gear.PluginSupport;

namespace Gear.GUI
{
    public partial class CogView : PluginBase
    {
        private Propeller Chip;
        private int HostID;
        private Font MonoFont;
        private Font MonoFontBold;
        private Bitmap BackBuffer;

        public override string Title
        {
            get
            {
                return "Cog " + HostID.ToString();
            }
        }

        public CogView(int host)
        {
            HostID = host;

            MonoFont = new Font(FontFamily.GenericMonospace, 10);
            if (MonoFont == null)
                MonoFont = this.Font;

            MonoFontBold = new Font(MonoFont, FontStyle.Bold);

            InitializeComponent();
        }

        public Cog GetViewCog()
        {
            return Chip.GetCog(HostID);
        }

        public override void PresentChip(Propeller host)
        {
            Chip = host;
        }

        private void Repaint(bool tick, NativeCog host)
        {
            Graphics g = Graphics.FromImage((Image)BackBuffer);
            g.Clear(SystemColors.Control);

            zeroFlagLable.Text = "Zero: " + (host.ZeroFlag ? "True" : "False");
            carryFlagLable.Text = "Carry: " + (host.CarryFlag ? "True" : "False");

            String display;

            for (uint i = (uint)positionScroll.Value, y = 0;
                y < ClientRectangle.Height;
                y += (uint)MonoFont.Height, i++)
            {
                if (i > 0x1FF)
                    continue;

                uint mem = host[(int)i];

                if (memoryViewButton.Checked)
                {
                    string binary = Convert.ToString((long)mem, 2);

                    while (binary.Length < 32)
                        binary = "0" + binary;

                    display = String.Format("{0:X4}:  {1:X8}   {2}   {1}",
                        i,
                        mem,
                        binary);
                }
                else
                {
                    display = String.Format("{0:X4}:  {2:X8}   {1}",
                        i,
                        InstructionDisassembler.AssemblyText(mem),
                        mem);
                }

                g.FillRectangle(SystemBrushes.Control, 0, y, assemblyPanel.Width, y + MonoFont.Height);

                g.DrawString(
                    display,
                    (host.ProgramCursor == i) ? MonoFontBold : MonoFont,
                    SystemBrushes.ControlText, 0, y);
            }
        }

        private void Repaint(bool tick, InterpretedCog host)
        {
            Graphics g = Graphics.FromImage((Image)BackBuffer);
            g.Clear(SystemColors.Control);

            String display;

            zeroFlagLable.Text = "";
            carryFlagLable.Text = "";

            if (memoryViewButton.Checked)
            {
                for (uint i = (uint)positionScroll.Value, y = 0;
                    y < ClientRectangle.Height;
                    y += (uint)MonoFont.Height, i++)
                {
                    if (i > 0x1FF)
                        continue;

                    uint mem = host[(int)i];

                    string binary = Convert.ToString((long)mem, 2);

                    while (binary.Length < 32)
                        binary = "0" + binary;

                    display = String.Format("{0:X4}:  {1:X8}   {2}   {1}",
                        i,
                        mem,
                        binary);

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

                for (uint i = (uint)positionScroll.Value;
                    y < ClientRectangle.Height;
                    y += (uint)MonoFont.Height)
                {
                    if (i > 0x7FFF)
                        continue;

                    uint start = i;

                    string inst = InstructionDisassembler.InterpreterText(Chip, ref i);
                    display = String.Format("{0:X4}: ", start);

                    for (uint q = start; q < start + 4; q++)
                    {
                        if (q < i)
                        {
                            byte b = Chip.ReadByte(q);
                            display += String.Format(" {0:X2}", b);


                        }
                        else
                            display += "   ";
                    }


                    display += "  " + inst;

                    g.FillRectangle(SystemBrushes.Control, 0, y, assemblyPanel.Width, y + MonoFont.Height);

                    g.DrawString(
                        display,
                        (host.ProgramCursor == start) ? MonoFontBold : MonoFont,
                        SystemBrushes.ControlText, 0, y);
                }

                y = 0;

                g.DrawString(Chip.ReadWord(host.Local - 8).ToString(),
                    MonoFont,
                    SystemBrushes.ControlText,
                    assemblyPanel.Width - 128, y);
                y += (uint)MonoFont.Height;

                g.DrawString(Chip.ReadWord(host.Local - 6).ToString(),
                    MonoFont,
                    SystemBrushes.ControlText,
                    assemblyPanel.Width - 128, y);
                y += (uint)MonoFont.Height;

                g.DrawString(Chip.ReadWord(host.Local - 4).ToString(),
                    MonoFont,
                    SystemBrushes.ControlText,
                    assemblyPanel.Width - 128, y);
                y += (uint)MonoFont.Height;

                g.DrawString(Chip.ReadWord(host.Local - 2).ToString(),
                    MonoFont,
                    SystemBrushes.ControlText,
                    assemblyPanel.Width - 128, y);
                y += (uint)MonoFont.Height;

                g.DrawLine(Pens.Black, assemblyPanel.Width - 128, y, assemblyPanel.Width, y);

                for (uint i = host.Local;
                    i < host.Stack && y < ClientRectangle.Height;
                    y += (uint)MonoFont.Height, i += 4)
                {
                    g.DrawString(((int)Chip.ReadLong(i)).ToString(),
                        MonoFont,
                        SystemBrushes.ControlText,
                        assemblyPanel.Width - 128, y);
                }
            }
        }

        public override void Repaint(bool tick)
        {
            if (Chip == null)
            {
                return;
            }

            Cog Host = Chip.GetCog(HostID);

            if (Host == null)
            {
                processorStateLable.Text = "CPU State: Cog is stopped.";
                programCounterLable.Text = "";
                zeroFlagLable.Text = "";
                carryFlagLable.Text = "";
                return;
            }

            positionScroll.Minimum = 0;

            if (Host is InterpretedCog)
                positionScroll.Maximum = 0x8000;
            else if (Host is NativeCog)
                positionScroll.Maximum = 0x200;

            positionScroll.LargeChange = 1;
            if (positionScroll.Maximum < positionScroll.Value)
                positionScroll.Value = positionScroll.Maximum;

            if (followPCButton.Checked)
                positionScroll.Value = (int)Host.ProgramCursor;

            programCounterLable.Text = "PC: " + String.Format("{0:X8}", Host.ProgramCursor);
            processorStateLable.Text = "CPU State: " + Host.CogState;

            if (Host is NativeCog)
                Repaint(tick, (NativeCog)Host);
            else if (Host is InterpretedCog)
                Repaint(tick, (InterpretedCog)Host);

            programCounterLable.Text = "PC: " + String.Format("{0:X8}", Host.ProgramCursor);
            processorStateLable.Text = "CPU State: " + Host.CogState;

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
    }
}
