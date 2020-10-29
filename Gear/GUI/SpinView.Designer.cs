/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2020 - Gear Developers
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
            this.hexView = new Gear.GUI.DoubleBufferedPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.analyzeButton = new System.Windows.Forms.ToolStripButton();
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
            this.objectView.Location = new System.Drawing.Point(0, 25);
            this.objectView.Name = "objectView";
            this.objectView.Size = new System.Drawing.Size(260, 428);
            this.objectView.TabIndex = 0;
            this.objectView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SelectChanged);
            // 
            // hexView
            // 
            this.hexView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexView.Location = new System.Drawing.Point(260, 25);
            this.hexView.Name = "hexView";
            this.hexView.Size = new System.Drawing.Size(367, 428);
            this.hexView.TabIndex = 1;
            this.hexView.SizeChanged += new System.EventHandler(this.OnSize);
            this.hexView.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.hexView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HexView_MouseClick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.analyzeButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(644, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // analyzeButton
            // 
            this.analyzeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.analyzeButton.Image = ((System.Drawing.Image)(resources.GetObject("analyzeButton.Image")));
            this.analyzeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.analyzeButton.Name = "analyzeButton";
            this.analyzeButton.Size = new System.Drawing.Size(60, 22);
            this.analyzeButton.Text = "Reanalize";
            this.analyzeButton.Click += new System.EventHandler(this.AnalizeButton_Click);
            // 
            // scrollPosition
            // 
            this.scrollPosition.Dock = System.Windows.Forms.DockStyle.Right;
            this.scrollPosition.LargeChange = 16;
            this.scrollPosition.Location = new System.Drawing.Point(627, 25);
            this.scrollPosition.Maximum = 65535;
            this.scrollPosition.Name = "scrollPosition";
            this.scrollPosition.Size = new System.Drawing.Size(17, 428);
            this.scrollPosition.TabIndex = 0;
            this.scrollPosition.TabStop = true;
            this.scrollPosition.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(260, 25);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 428);
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

        private System.Windows.Forms.TreeView objectView;
        private Gear.GUI.DoubleBufferedPanel hexView;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton analyzeButton;
        private System.Windows.Forms.VScrollBar scrollPosition;
        private System.Windows.Forms.Splitter splitter1;
        private readonly Font MonoSpace;
        private readonly System.Drawing.Brush[] Colorize;
        private Bitmap BackBuffer;

    }
}