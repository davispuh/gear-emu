/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
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

using System.Drawing;
using System.Windows.Forms;

namespace Gear.GUI
{
    partial class SpinView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                MonoSpace.Dispose();
                BackBuffer.Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

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
            this.objectView.FullRowSelect = true;
            this.objectView.Indent = 10;
            this.objectView.Location = new System.Drawing.Point(0, 34);
            this.objectView.Name = "objectView";
            this.objectView.Size = new System.Drawing.Size(260, 419);
            this.objectView.TabIndex = 0;
            this.objectView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SelectChanged);
            // 
            // hexView
            // 
            this.hexView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexView.Location = new System.Drawing.Point(260, 34);
            this.hexView.Name = "hexView";
            this.hexView.Size = new System.Drawing.Size(367, 419);
            this.hexView.TabIndex = 1;
            this.hexView.SizeChanged += new System.EventHandler(this.OnSize);
            this.hexView.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.hexView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HexView_MouseClick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.analizeButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(644, 34);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // analizeButton
            // 
            this.analizeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.analizeButton.Image = ((System.Drawing.Image)(resources.GetObject("analizeButton.Image")));
            this.analizeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.analizeButton.Name = "analizeButton";
            this.analizeButton.Size = new System.Drawing.Size(88, 29);
            this.analizeButton.Text = "Reanalize";
            this.analizeButton.Click += new System.EventHandler(this.AnalizeButton_Click);
            // 
            // scrollPosition
            // 
            this.scrollPosition.Dock = System.Windows.Forms.DockStyle.Right;
            this.scrollPosition.LargeChange = 16;
            this.scrollPosition.Location = new System.Drawing.Point(627, 34);
            this.scrollPosition.Maximum = 65535;
            this.scrollPosition.Name = "scrollPosition";
            this.scrollPosition.Size = new System.Drawing.Size(17, 419);
            this.scrollPosition.TabIndex = 0;
            this.scrollPosition.TabStop = true;
            this.scrollPosition.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(260, 34);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 419);
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
            this.Size = new System.Drawing.Size(644, 453);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private readonly Font MonoSpace;
        private TreeView objectView;
        private Panel hexView;
        private ToolStrip toolStrip1;
        private ToolStripButton analizeButton;
        private VScrollBar scrollPosition;
        private readonly System.Drawing.Brush[] Colorize;
        private Splitter splitter1;
        private Bitmap BackBuffer;

    }
}