/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using Gear.PluginSupport;
using Gear.EmulationCore;

namespace Gear.GUI
{
    class SpinView : PluginBase
    {
        private Propeller Host;
        private Font MonoSpace;
        private TreeView objectView;
        private Panel hexView;
        private ToolStrip toolStrip1;
        private ToolStripButton analizeButton;
        private VScrollBar scrollPosition;
        private Brush[] Colorize;
        private Splitter splitter1;
        //private System.ComponentModel.IContainer components;
        private Bitmap BackBuffer;

        public override string Title
        {
            get
            {
                return "Spin Map";
            }
        }

        public override Boolean IsClosable
        {
            get
            {
                return false;
            }
        }

        public SpinView()
        {
            MonoSpace = new Font(FontFamily.GenericMonospace, 8);
            if (MonoSpace == null)
                MonoSpace = Font;
            InitializeComponent();
            Colorize = new Brush[0x10000];

            for (int i = 0; i < Colorize.Length; i++)
            {
                Colorize[i] = Brushes.Gray;
            }
        }

        public override void PresentChip(Propeller host)
        {
            Host = host;
        }

        private void ColorCode()
        {
            int i;

            objectView.Nodes.Clear();
            TreeNode root = objectView.Nodes.Add("Spin");
            TreeNode node;

            node = root.Nodes.Add(String.Format("System Frequency: {0}mhz", Host.ReadLong(0)));
            node.Tag = (int)0;
            node = root.Nodes.Add(String.Format("Clock Mode: {0:X2}", Host.ReadByte(4)));
            node.Tag = (int)4;
            node = root.Nodes.Add(String.Format("Check Sum: {0:X2}", Host.ReadByte(5)));
            node.Tag = (int)5;
            node = root.Nodes.Add(String.Format("Root Object: {0:X4}", Host.ReadWord(6)));
            node.Tag = (int)6;
            node = root.Nodes.Add(String.Format("Variable Base: {0:X4}", Host.ReadWord(8)));
            node.Tag = (int)8;
            node = root.Nodes.Add(String.Format("Local Frame: {0:X4}", Host.ReadWord(10)));
            node.Tag = (int)10;
            node = root.Nodes.Add(String.Format("Entry PC: {0:X4}", Host.ReadWord(12)));
            node.Tag = (int)12;
            node = root.Nodes.Add(String.Format("Starting Stack: {0:X4}", Host.ReadWord(14)));
            node.Tag = (int)14;

            for (i = 0; i < 16; i++)
                Colorize[i] = Brushes.White;

            for (i = Host.ReadWord(0x8); i < Host.ReadWord(0xA); i++)
                Colorize[i] = Brushes.LightYellow;

            for (; i < 0x8000; i++)
                Colorize[i] = Brushes.LightGray;

            ColorObject(Host.ReadWord(0x6), Host.ReadWord(0x8), root);
        }

        private void ColorObject(uint objFrame, uint varFrame, TreeNode root)
        {
            uint i, addr, addrnext;

            root = root.Nodes.Add(String.Format("Object {0:X}", objFrame));
            root.Tag = (int)objFrame;

            root.Nodes.Add(String.Format("Variable Space {0:X4}", varFrame)).Tag = (int)varFrame;
            Colorize[varFrame] = Brushes.LightBlue;

            ushort size = Host.ReadWord(objFrame);
            byte longs = Host.ReadByte(objFrame + 2);
            byte objects = Host.ReadByte(objFrame + 3);

            for (i = 0; i < longs * 4; i++)
                Colorize[i + objFrame] = Brushes.LightPink;
            for (; i < (longs + objects) * 4; i++)
                Colorize[i + objFrame] = Brushes.LavenderBlush;
            for (; i < size; i++)
                Colorize[i + objFrame] = Brushes.LightGreen;

            addrnext = Host.ReadWord(1 * 4 + objFrame) + objFrame;
            for (i = 1; i < longs; i++)
            {
                addr = addrnext;
                addrnext = Host.ReadWord((i + 1) * 4 + objFrame) + objFrame;
                if (i == longs - 1)
                {
                    addrnext = addr + 1;
                    while (Colorize[addrnext] == Brushes.LightGreen)
                        addrnext++;
                }
                ColorFunction(addr, addrnext, root);
            }

            for (i = 0; i < objects; i++)
                ColorObject(Host.ReadWord((longs + i) * 4 + objFrame) + objFrame,
                    Host.ReadWord((longs + i) * 4 + 2 + objFrame) + varFrame, root);
        }

        private void ColorFunction(uint functFrame, uint functFrameEnd, TreeNode root)
        {
            root = root.Nodes.Add(String.Format("Function {0:X} ({1:d})", functFrame, functFrameEnd - functFrame));
            root.Tag = (int)functFrame;

            Colorize[functFrame] = Brushes.Yellow;
        }

        public override void Repaint(bool force)
        {
            if (Host == null)
                return;

            Graphics g = Graphics.FromImage((Image)BackBuffer);
            ASCIIEncoding ascii = new ASCIIEncoding();

            g.Clear(SystemColors.Control);

            Size s = TextRenderer.MeasureText("00", MonoSpace);
            Size a = TextRenderer.MeasureText("0000:", MonoSpace);

            for (int y = scrollPosition.Value, dy = 0; y < 0x10000 && dy < hexView.ClientRectangle.Height; dy += s.Height)
            {
                // Draw the address
                g.FillRectangle(Brushes.White, new Rectangle(0, dy, a.Width, s.Height));
                g.DrawString(String.Format("{0:X4}:", y), MonoSpace, SystemBrushes.ControlText, 0, dy);
                // Draw the line of data
                for (int x = 0, dx = a.Width; y < 0x10000 && x < 16; x++, dx += s.Width, y++)
                {
                    byte data = Host.ReadByte((uint)y);
                    g.FillRectangle(Colorize[y], new Rectangle(dx, dy, s.Width, s.Height));

                    // if (data > 32 && data < 127)
                    // {
                    //    g.DrawString(ascii.GetString(new byte[] { data }), MonoSpace, SystemBrushes.ControlText, dx, dy);
                    // }
                    // else
                    // {
                        g.DrawString(String.Format("{0:X2}", data), MonoSpace, SystemBrushes.ControlText, dx, dy);
                    // }
                }
            }

            hexView.CreateGraphics().DrawImageUnscaled(BackBuffer, 0, 0);
        }

        #region FormCode

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpinView));
            this.objectView = new System.Windows.Forms.TreeView();
            this.hexView = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.analizeButton = new System.Windows.Forms.ToolStripButton();
            this.scrollPosition = new System.Windows.Forms.VScrollBar();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // objectView
            //
            this.objectView.Dock = System.Windows.Forms.DockStyle.Left;
            this.objectView.Indent = 15;
            this.objectView.Location = new System.Drawing.Point(0, 25);
            this.objectView.Name = "objectView";
            this.objectView.Size = new System.Drawing.Size(193, 424);
            this.objectView.TabIndex = 0;
            this.objectView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SelectChanged);
            //
            // hexView
            //
            this.hexView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexView.Location = new System.Drawing.Point(193, 25);
            this.hexView.Name = "hexView";
            this.hexView.Size = new System.Drawing.Size(415, 424);
            this.hexView.TabIndex = 1;
            this.hexView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.hexView_MouseClick);
            this.hexView.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.hexView.SizeChanged += new System.EventHandler(this.OnSize);
            //
            // toolStrip1
            //
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.analizeButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(625, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            //
            // analizeButton
            //
            this.analizeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.analizeButton.Image = ((System.Drawing.Image)(resources.GetObject("analizeButton.Image")));
            this.analizeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.analizeButton.Name = "analizeButton";
            this.analizeButton.Size = new System.Drawing.Size(57, 22);
            this.analizeButton.Text = "Reanalize";
            this.analizeButton.Click += new System.EventHandler(this.analizeButton_Click);
            //
            // scrollPosition
            //
            this.scrollPosition.Dock = System.Windows.Forms.DockStyle.Right;
            this.scrollPosition.LargeChange = 16;
            this.scrollPosition.Location = new System.Drawing.Point(608, 25);
            this.scrollPosition.Maximum = 65535;
            this.scrollPosition.Name = "scrollPosition";
            this.scrollPosition.Size = new System.Drawing.Size(17, 424);
            this.scrollPosition.TabIndex = 0;
            this.scrollPosition.TabStop = true;
            this.scrollPosition.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            //
            // splitter1
            //
            this.splitter1.Location = new System.Drawing.Point(193, 25);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 424);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            //
            // SpinView
            //
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.hexView);
            this.Controls.Add(this.objectView);
            this.Controls.Add(this.scrollPosition);
            this.Controls.Add(this.toolStrip1);
            this.Name = "SpinView";
            this.Size = new System.Drawing.Size(625, 449);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }

        private void SelectChanged(object sender, TreeViewEventArgs e)
        {
            object o = objectView.SelectedNode.Tag;

            if (o != null)
            {
                scrollPosition.Value = (int)o;
                Repaint(false);
            }
        }

        private void analizeButton_Click(object sender, EventArgs e)
        {
            ColorCode();
            objectView.ExpandAll();
            Repaint(false);
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            Repaint(false);
        }

        private void OnSize(object sender, EventArgs e)
        {
            if (hexView.Width > 0 && hexView.Height > 0)
            {
                BackBuffer = new Bitmap(
                    hexView.Width,
                    hexView.Height
                );
            }
            else
            {
                BackBuffer = new Bitmap(1, 1);
            }
            Repaint(false);
        }

        private void hexView_MouseClick(object sender, MouseEventArgs e)
        {
            scrollPosition.Focus();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void openStimulusFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
