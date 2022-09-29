/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * SpinView.Designer.cs
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
                _monoFont.Dispose();
                _bufferedGraphicsContext.Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpinView));
            this.objectTreeView = new System.Windows.Forms.TreeView();
            this.imageListForTreeView = new System.Windows.Forms.ImageList(this.components);
            this.memoryView = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.analyzeButton = new System.Windows.Forms.ToolStripButton();
            this.alignmentSplitButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.freeAlignmentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fixedByteAlignmentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fixedWordAlignmentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PositionScrollBar = new System.Windows.Forms.VScrollBar();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // objectTreeView
            //
            this.objectTreeView.Dock = System.Windows.Forms.DockStyle.Left;
            this.objectTreeView.FullRowSelect = true;
            this.objectTreeView.ImageIndex = 0;
            this.objectTreeView.ImageList = this.imageListForTreeView;
            this.objectTreeView.Indent = 15;
            this.objectTreeView.Location = new System.Drawing.Point(0, 31);
            this.objectTreeView.Name = "objectTreeView";
            this.objectTreeView.SelectedImageIndex = 0;
            this.objectTreeView.Size = new System.Drawing.Size(260, 422);
            this.objectTreeView.TabIndex = 0;
            this.objectTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ObjectTreeView_SelectChanged);
            //
            // imageListForTreeView
            //
            this.imageListForTreeView.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageListForTreeView.ImageSize = new System.Drawing.Size(16, 17);
            this.imageListForTreeView.TransparentColor = System.Drawing.Color.Transparent;
            //
            // memoryView
            //
            this.memoryView.AutoScroll = true;
            this.memoryView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.memoryView.Location = new System.Drawing.Point(260, 31);
            this.memoryView.MinimumSize = new System.Drawing.Size(350, 100);
            this.memoryView.Name = "memoryView";
            this.memoryView.Size = new System.Drawing.Size(367, 422);
            this.memoryView.TabIndex = 1;
            this.memoryView.SizeChanged += new System.EventHandler(this.MemoryView_SizeChange);
            this.memoryView.Paint += new System.Windows.Forms.PaintEventHandler(this.MemoryView_Paint);
            this.memoryView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MemoryView_MouseClick);
            //
            // toolStrip1
            //
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.analyzeButton,
            this.alignmentSplitButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(644, 31);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            //
            // analyzeButton
            //
            this.analyzeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.analyzeButton.Image = ((System.Drawing.Image)(resources.GetObject("analyzeButton.Image")));
            this.analyzeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.analyzeButton.Name = "analyzeButton";
            this.analyzeButton.Size = new System.Drawing.Size(52, 28);
            this.analyzeButton.Text = "Analyze";
            this.analyzeButton.Click += new System.EventHandler(this.AnalyzeButton_Click);
            //
            // alignmentSplitButton
            //
            this.alignmentSplitButton.AutoToolTip = false;
            this.alignmentSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.freeAlignmentMenuItem,
            this.fixedByteAlignmentMenuItem,
            this.fixedWordAlignmentMenuItem});
            this.alignmentSplitButton.Image = ((System.Drawing.Image)(resources.GetObject("AlignmentNone")));
            this.alignmentSplitButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.alignmentSplitButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.alignmentSplitButton.Name = "alignmentSplitButton";
            this.alignmentSplitButton.Size = new System.Drawing.Size(110, 28);
            this.alignmentSplitButton.Text = "Alignment";
            this.alignmentSplitButton.ToolTipText = "Fixed or Free Alignment";
            //
            // freeAlignmentMenuItem
            //
            this.freeAlignmentMenuItem.CheckOnClick = true;
            this.freeAlignmentMenuItem.Name = "freeAlignmentMenuItem";
            this.freeAlignmentMenuItem.Size = new System.Drawing.Size(161, 22);
            this.freeAlignmentMenuItem.Text = "Free";
            this.freeAlignmentMenuItem.ToolTipText = "Free advance in steps of 1 Byte.";
            this.freeAlignmentMenuItem.Click += new System.EventHandler(this.FreeAlignmentToolStripMenuItem_Click);
            //
            // fixedByteAlignmentMenuItem
            //
            this.fixedByteAlignmentMenuItem.CheckOnClick = true;
            this.fixedByteAlignmentMenuItem.Name = "fixedByteAlignmentMenuItem";
            this.fixedByteAlignmentMenuItem.Size = new System.Drawing.Size(161, 22);
            this.fixedByteAlignmentMenuItem.Text = "Fixed at 8 Bytes";
            this.fixedByteAlignmentMenuItem.ToolTipText = "Advance fixed at steps of 8 Bytes.";
            this.fixedByteAlignmentMenuItem.Click += new System.EventHandler(this.FixedByteAlignmentToolStripMenuItem_Click);
            //
            // fixedWordAlignmentMenuItem
            //
            this.fixedWordAlignmentMenuItem.CheckOnClick = true;
            this.fixedWordAlignmentMenuItem.Name = "fixedWordAlignmentMenuItem";
            this.fixedWordAlignmentMenuItem.Size = new System.Drawing.Size(161, 22);
            this.fixedWordAlignmentMenuItem.Text = "Fixed at 16 Bytes";
            this.fixedWordAlignmentMenuItem.ToolTipText = "Advance fixed at steps of 16 Bytes.";
            this.fixedWordAlignmentMenuItem.Click += new System.EventHandler(this.FixedWordAlignmentToolStripMenuItem_Click);
            //
            // PositionScrollBar
            //
            this.PositionScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.PositionScrollBar.LargeChange = 16;
            this.PositionScrollBar.Location = new System.Drawing.Point(627, 31);
            this.PositionScrollBar.Maximum = 32767;
            this.PositionScrollBar.Name = "PositionScrollBar";
            this.PositionScrollBar.Size = new System.Drawing.Size(17, 422);
            this.PositionScrollBar.TabIndex = 0;
            this.PositionScrollBar.TabStop = true;
            this.PositionScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.PositionScrollBar_PositionChanged);
            //
            // splitter1
            //
            this.splitter1.Location = new System.Drawing.Point(260, 31);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 422);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            //
            // SpinView
            //
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.memoryView);
            this.Controls.Add(this.objectTreeView);
            this.Controls.Add(this.PositionScrollBar);
            this.Controls.Add(this.toolStrip1);
            this.Name = "SpinView";
            this.Size = new System.Drawing.Size(644, 453);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView objectTreeView;
        private System.Windows.Forms.Panel memoryView;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton analyzeButton;
        // @version v22.07.01 - Name changed.
        private System.Windows.Forms.VScrollBar PositionScrollBar;
        private System.Windows.Forms.Splitter splitter1;
        // @version v22.07.01 - Added.
        private ImageList imageListForTreeView;
        // @version v22.07.01 - Added.
        private ToolStripDropDownButton alignmentSplitButton;
        // @version v22.07.01 - Added.
        private ToolStripMenuItem freeAlignmentMenuItem;
        // @version v22.07.01 - Added.
        private ToolStripMenuItem fixedByteAlignmentMenuItem;
        // @version v22.07.01 - Added.
        private ToolStripMenuItem fixedWordAlignmentMenuItem;
    }
}
