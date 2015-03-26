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
            this.DisplayUnits = new System.Windows.Forms.ToolStripDropDownButton();
            this.decimalUnits = new System.Windows.Forms.ToolStripMenuItem();
            this.hexadecimalUnits = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.OpcodeSize = new System.Windows.Forms.ToolStripDropDownButton();
            this.longOpcodes = new System.Windows.Forms.ToolStripMenuItem();
            this.shortOpcodes = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
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
            this.assemblyPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.assemblyPanel_MouseDown);
            this.assemblyPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.assemblyPanel_MouseMove);
            this.assemblyPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.assemblyPanel_MouseClick);
            this.assemblyPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.AssemblyView_Paint);
            this.assemblyPanel.MouseHover += new System.EventHandler(this.assemblyPanel_MouseHover);
            this.assemblyPanel.SizeChanged += new System.EventHandler(this.AsmSized);
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
            this.DisplayUnits,
            this.toolStripSeparator6,
            this.OpcodeSize});
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
            this.memoryViewButton.Size = new System.Drawing.Size(78, 22);
            this.memoryViewButton.Text = "Show Memory";
            this.memoryViewButton.Click += new System.EventHandler(this.memoryViewButton_Click);
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
            this.followPCButton.Size = new System.Drawing.Size(57, 22);
            this.followPCButton.Text = "Follow PC";
            this.followPCButton.Click += new System.EventHandler(this.followPCButton_Click);
            //
            // programCounterLabel
            //
            this.programCounterLabel.Name = "programCounterLabel";
            this.programCounterLabel.Size = new System.Drawing.Size(19, 22);
            this.programCounterLabel.Text = "---";
            //
            // zeroFlagLabel
            //
            this.zeroFlagLabel.Name = "zeroFlagLabel";
            this.zeroFlagLabel.Size = new System.Drawing.Size(19, 22);
            this.zeroFlagLabel.Text = "---";
            //
            // carryFlagLabel
            //
            this.carryFlagLabel.Name = "carryFlagLabel";
            this.carryFlagLabel.Size = new System.Drawing.Size(19, 22);
            this.carryFlagLabel.Text = "---";
            //
            // processorStateLabel
            //
            this.processorStateLabel.Name = "processorStateLabel";
            this.processorStateLabel.Size = new System.Drawing.Size(19, 22);
            this.processorStateLabel.Text = "---";
            //
            // toolStripSeparator5
            //
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
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
            this.DisplayUnits.Size = new System.Drawing.Size(44, 22);
            this.DisplayUnits.Text = "Units";
            //
            // decimalUnits
            //
            this.decimalUnits.Checked = true;
            this.decimalUnits.CheckState = System.Windows.Forms.CheckState.Checked;
            this.decimalUnits.Name = "decimalUnits";
            this.decimalUnits.Size = new System.Drawing.Size(145, 22);
            this.decimalUnits.Text = "Decimal";
            this.decimalUnits.Click += new System.EventHandler(this.decimalUnits_Click);
            //
            // hexadecimalUnits
            //
            this.hexadecimalUnits.Name = "hexadecimalUnits";
            this.hexadecimalUnits.Size = new System.Drawing.Size(145, 22);
            this.hexadecimalUnits.Text = "Hexadecimal";
            this.hexadecimalUnits.Click += new System.EventHandler(this.hexadecimalUnits_Click);
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
            this.OpcodeSize.Size = new System.Drawing.Size(62, 22);
            this.OpcodeSize.Text = "Opcodes";
            //
            // longOpcodes
            //
            this.longOpcodes.Name = "longOpcodes";
            this.longOpcodes.Size = new System.Drawing.Size(156, 22);
            this.longOpcodes.Text = "Long Opcodes";
            this.longOpcodes.Click += new System.EventHandler(this.longOpcodes_Click);
            //
            // shortOpcodes
            //
            this.shortOpcodes.Checked = true;
            this.shortOpcodes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.shortOpcodes.Name = "shortOpcodes";
            this.shortOpcodes.Size = new System.Drawing.Size(156, 22);
            this.shortOpcodes.Text = "Short Opcodes";
            this.shortOpcodes.Click += new System.EventHandler(this.shortOpcodes_Click);
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

    }
}
