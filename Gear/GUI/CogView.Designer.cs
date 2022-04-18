/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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

namespace Gear.GUI
{
    partial class CogView
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
                components.Dispose();
            }
            this.MonoFont.Dispose();
            this.MonoFontBold.Dispose();
            this.BackBuffer.Dispose();

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CogView));
            this.assemblyPanel = new System.Windows.Forms.Panel();
            this.positionScroll = new System.Windows.Forms.VScrollBar();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.memoryViewButton = new System.Windows.Forms.ToolStripButton();
            this.followPCButton = new System.Windows.Forms.ToolStripButton();
            this.programCounterLabel = new System.Windows.Forms.ToolStripLabel();
            this.zeroFlagLabel = new System.Windows.Forms.ToolStripLabel();
            this.carryFlagLabel = new System.Windows.Forms.ToolStripLabel();
            this.processorStateLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.frameCountLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.DisplayUnits = new System.Windows.Forms.ToolStripDropDownButton();
            this.decimalUnits = new System.Windows.Forms.ToolStripMenuItem();
            this.hexadecimalUnits = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.OpcodeSize = new System.Windows.Forms.ToolStripDropDownButton();
            this.longOpcodes = new System.Windows.Forms.ToolStripMenuItem();
            this.shortOpcodes = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.FrameBreakMode = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.breakNone = new System.Windows.Forms.ToolStripMenuItem();
            this.breakMiss = new System.Windows.Forms.ToolStripMenuItem();
            this.breakAll = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // toolStripSeparator4
            //
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            //
            // assemblyPanel
            //
            this.assemblyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assemblyPanel.Location = new System.Drawing.Point(0, 25);
            this.assemblyPanel.Name = "assemblyPanel";
            this.assemblyPanel.Size = new System.Drawing.Size(548, 421);
            this.assemblyPanel.TabIndex = 1;
            this.assemblyPanel.SizeChanged += new System.EventHandler(this.AsmSized);
            this.assemblyPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.AssemblyView_Paint);
            this.assemblyPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.AssemblyPanel_MouseClick);
            this.assemblyPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AssemblyPanel_MouseDown);
            this.assemblyPanel.MouseHover += new System.EventHandler(this.AssemblyPanel_MouseHover);
            this.assemblyPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.AssemblyPanel_MouseMove);
            //
            // positionScroll
            //
            this.positionScroll.Dock = System.Windows.Forms.DockStyle.Right;
            this.positionScroll.LargeChange = 16;
            this.positionScroll.Location = new System.Drawing.Point(548, 25);
            this.positionScroll.Name = "positionScroll";
            this.positionScroll.Size = new System.Drawing.Size(17, 421);
            this.positionScroll.TabIndex = 2;
            this.positionScroll.TabStop = true;
            this.positionScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.UpdateOnScroll);
            //
            // toolStrip
            //
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.memoryViewButton,
            this.followPCButton,
            toolStripSeparator4,
            this.programCounterLabel,
            toolStripSeparator1,
            this.zeroFlagLabel,
            toolStripSeparator2,
            this.carryFlagLabel,
            toolStripSeparator3,
            this.processorStateLabel,
            this.toolStripSeparator5,
            this.frameCountLabel,
            this.toolStripSeparator7,
            this.DisplayUnits,
            this.toolStripSeparator6,
            this.OpcodeSize,
            this.toolStripSeparator8,
            this.FrameBreakMode});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(565, 25);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip1";
            //
            // memoryViewButton
            //
            this.memoryViewButton.CheckOnClick = true;
            this.memoryViewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.memoryViewButton.Image = ((System.Drawing.Image)(resources.GetObject("memoryViewButton.Image")));
            this.memoryViewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.memoryViewButton.Name = "memoryViewButton";
            this.memoryViewButton.Size = new System.Drawing.Size(88, 22);
            this.memoryViewButton.Text = "Show Memory";
            this.memoryViewButton.Click += new System.EventHandler(this.MemoryViewButton_Click);
            //
            // followPCButton
            //
            this.followPCButton.Checked = true;
            this.followPCButton.CheckOnClick = true;
            this.followPCButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.followPCButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.followPCButton.Image = ((System.Drawing.Image)(resources.GetObject("followPCButton.Image")));
            this.followPCButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.followPCButton.Name = "followPCButton";
            this.followPCButton.Size = new System.Drawing.Size(64, 22);
            this.followPCButton.Text = "Follow PC";
            this.followPCButton.Click += new System.EventHandler(this.FollowPCButton_Click);
            //
            // programCounterLabel
            //
            this.programCounterLabel.Name = "programCounterLabel";
            this.programCounterLabel.Size = new System.Drawing.Size(22, 22);
            this.programCounterLabel.Text = "---";
            //
            // zeroFlagLabel
            //
            this.zeroFlagLabel.Name = "zeroFlagLabel";
            this.zeroFlagLabel.Size = new System.Drawing.Size(22, 22);
            this.zeroFlagLabel.Text = "---";
            //
            // carryFlagLabel
            //
            this.carryFlagLabel.Name = "carryFlagLabel";
            this.carryFlagLabel.Size = new System.Drawing.Size(22, 22);
            this.carryFlagLabel.Text = "---";
            //
            // processorStateLabel
            //
            this.processorStateLabel.Name = "processorStateLabel";
            this.processorStateLabel.Size = new System.Drawing.Size(22, 22);
            this.processorStateLabel.Text = "---";
            //
            // toolStripSeparator5
            //
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            //
            // frameCountLabel
            //
            this.frameCountLabel.AutoToolTip = true;
            this.frameCountLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.frameCountLabel.Name = "frameCountLabel";
            this.frameCountLabel.Size = new System.Drawing.Size(22, 22);
            this.frameCountLabel.Text = "---";
            this.frameCountLabel.ToolTipText = "Number of Video Frames";
            //
            // toolStripSeparator7
            //
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            //
            // DisplayUnits
            //
            this.DisplayUnits.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.DisplayUnits.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.decimalUnits,
            this.hexadecimalUnits});
            this.DisplayUnits.Image = ((System.Drawing.Image)(resources.GetObject("DisplayUnits.Image")));
            this.DisplayUnits.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DisplayUnits.Name = "DisplayUnits";
            this.DisplayUnits.Size = new System.Drawing.Size(47, 22);
            this.DisplayUnits.Text = "Units";
            //
            // decimalUnits
            //
            this.decimalUnits.Checked = true;
            this.decimalUnits.CheckState = System.Windows.Forms.CheckState.Checked;
            this.decimalUnits.Name = "decimalUnits";
            this.decimalUnits.Size = new System.Drawing.Size(143, 22);
            this.decimalUnits.Text = "Decimal";
            this.decimalUnits.Click += new System.EventHandler(this.DecimalUnits_Click);
            //
            // hexadecimalUnits
            //
            this.hexadecimalUnits.Name = "hexadecimalUnits";
            this.hexadecimalUnits.Size = new System.Drawing.Size(143, 22);
            this.hexadecimalUnits.Text = "Hexadecimal";
            this.hexadecimalUnits.Click += new System.EventHandler(this.HexadecimalUnits_Click);
            //
            // toolStripSeparator6
            //
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            //
            // OpcodeSize
            //
            this.OpcodeSize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.OpcodeSize.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.longOpcodes,
            this.shortOpcodes});
            this.OpcodeSize.Image = ((System.Drawing.Image)(resources.GetObject("OpcodeSize.Image")));
            this.OpcodeSize.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpcodeSize.Name = "OpcodeSize";
            this.OpcodeSize.Size = new System.Drawing.Size(67, 22);
            this.OpcodeSize.Text = "Opcodes";
            //
            // longOpcodes
            //
            this.longOpcodes.Name = "longOpcodes";
            this.longOpcodes.Size = new System.Drawing.Size(152, 22);
            this.longOpcodes.Text = "Long Opcodes";
            this.longOpcodes.Click += new System.EventHandler(this.LongOpcodes_Click);
            //
            // shortOpcodes
            //
            this.shortOpcodes.Checked = true;
            this.shortOpcodes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.shortOpcodes.Name = "shortOpcodes";
            this.shortOpcodes.Size = new System.Drawing.Size(152, 22);
            this.shortOpcodes.Text = "Short Opcodes";
            this.shortOpcodes.Click += new System.EventHandler(this.ShortOpcodes_Click);
            //
            // FrameBreakMode
            //
            this.FrameBreakMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.FrameBreakMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.breakNone,
            this.breakMiss,
            this.breakAll});
            this.FrameBreakMode.Image = ((System.Drawing.Image)(resources.GetObject("FrameBreakMode.Image")));
            this.FrameBreakMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FrameBreakMode.Name = "FrameBreakMode";
            this.FrameBreakMode.Size = new System.Drawing.Size(82, 22);
            this.FrameBreakMode.Text = "Video Break";
            this.FrameBreakMode.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.FrameBreakMode.ToolTipText = "Enable break on end of video frame";
            //
            // toolStripSeparator8
            //
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            //
            // breakNone
            //
            this.breakNone.AutoToolTip = true;
            this.breakNone.CheckOnClick = true;
            this.breakNone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.breakNone.Name = "breakNone";
            this.breakNone.Size = new System.Drawing.Size(180, 22);
            this.breakNone.Text = "None";
            this.breakNone.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.breakNone.Click += new System.EventHandler(this.VideoBreak_Click);
            //
            // breakMiss
            //
            this.breakMiss.CheckOnClick = true;
            this.breakMiss.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.breakMiss.Name = "breakMiss";
            this.breakMiss.Size = new System.Drawing.Size(180, 22);
            this.breakMiss.Text = "Frame Miss";
            this.breakMiss.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.breakMiss.ToolTipText = "Break if frame end misses WAIT_VID";
            this.breakMiss.Click += new System.EventHandler(this.VideoBreak_Click);
            //
            // breakAll
            //
            this.breakAll.AutoToolTip = true;
            this.breakAll.CheckOnClick = true;
            this.breakAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.breakAll.Name = "breakAll";
            this.breakAll.Size = new System.Drawing.Size(180, 22);
            this.breakAll.Text = "Frame End";
            this.breakAll.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.breakAll.ToolTipText = "Break on end of video frame";
            this.breakAll.Click += new System.EventHandler(this.VideoBreak_Click);
            //
            // CogView
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.assemblyPanel);
            this.Controls.Add(this.positionScroll);
            this.Controls.Add(this.toolStrip);
            this.Name = "CogView";
            this.Size = new System.Drawing.Size(565, 446);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel assemblyPanel;
        private System.Windows.Forms.VScrollBar positionScroll;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel programCounterLabel;
        private System.Windows.Forms.ToolStripLabel zeroFlagLabel;
        private System.Windows.Forms.ToolStripLabel carryFlagLabel;
        private System.Windows.Forms.ToolStripLabel processorStateLabel;
        private System.Windows.Forms.ToolStripButton memoryViewButton;
        private System.Windows.Forms.ToolStripButton followPCButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripDropDownButton DisplayUnits;
        private System.Windows.Forms.ToolStripMenuItem decimalUnits;
        private System.Windows.Forms.ToolStripMenuItem hexadecimalUnits;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripDropDownButton OpcodeSize;
        private System.Windows.Forms.ToolStripMenuItem longOpcodes;
        private System.Windows.Forms.ToolStripMenuItem shortOpcodes;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripLabel frameCountLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripDropDownButton FrameBreakMode;
        private System.Windows.Forms.ToolStripMenuItem breakNone;
        private System.Windows.Forms.ToolStripMenuItem breakMiss;
        private System.Windows.Forms.ToolStripMenuItem breakAll;
    }
}
