/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * LogicView.Designer.cs
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

namespace Gear.GUI.LogicProbe
{
    partial class LogicView
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
            this._monoFont.Dispose();
            this._backBuffer.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogicView));
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripCmbBoxUnit = new Gear.GUI.TimeUnitToolStripComboBox();
            this.timeFrameBox = new System.Windows.Forms.ToolStripTextBox();
            this.tickMarkBox = new System.Windows.Forms.ToolStripTextBox();
            this.updateGridButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.pinsTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.analogButton = new System.Windows.Forms.ToolStripButton();
            this.digitalButton = new System.Windows.Forms.ToolStripButton();
            this.viewOffset = new System.Windows.Forms.VScrollBar();
            this.waveView = new Gear.GUI.DoubleBufferedPanel();
            this.timeAdjustBar = new System.Windows.Forms.HScrollBar();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // toolStripLabel1
            //
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(72, 22);
            this.toolStripLabel1.Text = "Time Frame:";
            //
            // toolStripLabel2
            //
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(86, 22);
            this.toolStripLabel2.Text = "Tick Mark Grid:";
            //
            // toolStripSeparator2
            //
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            //
            // toolStrip1
            //
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripCmbBoxUnit,
            this.timeFrameBox,
            this.toolStripLabel2,
            this.tickMarkBox,
            this.updateGridButton,
            this.toolStripSeparator2,
            this.toolStripLabel3,
            this.pinsTextBox,
            this.analogButton,
            this.digitalButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(697, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            //
            // toolStripCmbBoxUnit
            //
            this.toolStripCmbBoxUnit.AutoSize = false;
            this.toolStripCmbBoxUnit.BaseUnit = Gear.Utils.TimeUnitsEnum.s;
            this.toolStripCmbBoxUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripCmbBoxUnit.ExcludedUnits.Add(Gear.Utils.TimeUnitsEnum.None);
            this.toolStripCmbBoxUnit.ExcludedUnits.Add(Gear.Utils.TimeUnitsEnum.min_s);
            this.toolStripCmbBoxUnit.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
            this.toolStripCmbBoxUnit.Name = "toolStripCmbBoxUnit";
            this.toolStripCmbBoxUnit.Size = new System.Drawing.Size(40, 23);
            this.toolStripCmbBoxUnit.TimeUnitSelected = Gear.Utils.TimeUnitsEnum.None;
            this.toolStripCmbBoxUnit.ToolTipText = "Time Unit for Time frame and Tick mark grid.";
            this.toolStripCmbBoxUnit.SelectedIndexChanged += new System.EventHandler(this.ToolStripCmbBoxUnit_SelectedIndexChanged);
            //
            // timeFrameBox
            //
            this.timeFrameBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.timeFrameBox.Name = "timeFrameBox";
            this.timeFrameBox.Size = new System.Drawing.Size(85, 25);
            this.timeFrameBox.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.timeFrameBox.ToolTipText = "Duration of Time frame in the time unit selected.\r\nTo apply changes, press Update" +
    " Grid button.";
            this.timeFrameBox.ModifiedChanged += new System.EventHandler(this.TimeFrameBox_ModifiedChanged);
            //
            // tickMarkBox
            //
            this.tickMarkBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tickMarkBox.Name = "tickMarkBox";
            this.tickMarkBox.Size = new System.Drawing.Size(85, 25);
            this.tickMarkBox.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tickMarkBox.ToolTipText = "Separation between marks, expressed in the time unit selected.\r\nTo apply changes," +
    " press Update Grid button.";
            this.tickMarkBox.ModifiedChanged += new System.EventHandler(this.TickMarkBox_ModifiedChanged);
            //
            // updateGridButton
            //
            this.updateGridButton.Enabled = false;
            this.updateGridButton.Image = global::Gear.Properties.Resources.Image_updateGrid;
            this.updateGridButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.updateGridButton.Name = "updateGridButton";
            this.updateGridButton.Size = new System.Drawing.Size(90, 22);
            this.updateGridButton.Text = "Update Grid";
            this.updateGridButton.ToolTipText = "Update grid with new values.";
            this.updateGridButton.Click += new System.EventHandler(this.UpdateGridButton_Click);
            //
            // toolStripLabel3
            //
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(52, 22);
            this.toolStripLabel3.Text = "Add pin:";
            //
            // pinsTextBox
            //
            this.pinsTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pinsTextBox.Name = "pinsTextBox";
            this.pinsTextBox.Size = new System.Drawing.Size(20, 25);
            this.pinsTextBox.ToolTipText = "Pin number or range (ex. 2..5) to add.";
            //
            // analogButton
            //
            this.analogButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.analogButton.Image = ((System.Drawing.Image)(resources.GetObject("analogButton.Image")));
            this.analogButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.analogButton.Name = "analogButton";
            this.analogButton.Size = new System.Drawing.Size(57, 22);
            this.analogButton.Text = "+Analog";
            this.analogButton.ToolTipText = "Add a Analog view on pin selected.";
            this.analogButton.Click += new System.EventHandler(this.AddAnalogButton_Click);
            //
            // digitalButton
            //
            this.digitalButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.digitalButton.Image = ((System.Drawing.Image)(resources.GetObject("digitalButton.Image")));
            this.digitalButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.digitalButton.Name = "digitalButton";
            this.digitalButton.Size = new System.Drawing.Size(53, 22);
            this.digitalButton.Text = "+Digital";
            this.digitalButton.ToolTipText = "Add a Digitalview on pin selected.";
            this.digitalButton.Click += new System.EventHandler(this.AddDigitalButton_Click);
            //
            // viewOffset
            //
            this.viewOffset.Dock = System.Windows.Forms.DockStyle.Right;
            this.viewOffset.LargeChange = 4;
            this.viewOffset.Location = new System.Drawing.Point(680, 25);
            this.viewOffset.Name = "viewOffset";
            this.viewOffset.Size = new System.Drawing.Size(17, 411);
            this.viewOffset.TabIndex = 2;
            this.viewOffset.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollChanged);
            //
            // waveView
            //
            this.waveView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waveView.Location = new System.Drawing.Point(0, 25);
            this.waveView.Name = "waveView";
            this.waveView.Size = new System.Drawing.Size(680, 411);
            this.waveView.TabIndex = 1;
            this.waveView.SizeChanged += new System.EventHandler(this.OnSized);
            this.waveView.Click += new System.EventHandler(this.OnClick);
            this.waveView.Paint += new System.Windows.Forms.PaintEventHandler(this.WaveView_Paint);
            this.waveView.DoubleClick += new System.EventHandler(this.OnDblClick);
            //
            // timeAdjustBar
            //
            this.timeAdjustBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.timeAdjustBar.Location = new System.Drawing.Point(0, 436);
            this.timeAdjustBar.Maximum = 1000;
            this.timeAdjustBar.Name = "timeAdjustBar";
            this.timeAdjustBar.Size = new System.Drawing.Size(697, 17);
            this.timeAdjustBar.TabIndex = 3;
            this.timeAdjustBar.Value = 1000;
            this.timeAdjustBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.TimeChanged);
            //
            // LogicView
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.waveView);
            this.Controls.Add(this.viewOffset);
            this.Controls.Add(this.timeAdjustBar);
            this.Controls.Add(this.toolStrip1);
            this.Name = "LogicView";
            this.Size = new System.Drawing.Size(697, 453);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.VScrollBar viewOffset;
        private Gear.GUI.DoubleBufferedPanel waveView;
        private System.Windows.Forms.ToolStripTextBox timeFrameBox;
        private System.Windows.Forms.ToolStripTextBox tickMarkBox;
        private System.Windows.Forms.HScrollBar timeAdjustBar;
        private System.Windows.Forms.ToolStripButton updateGridButton;
        private System.Windows.Forms.ToolStripButton digitalButton;
        private System.Windows.Forms.ToolStripButton analogButton;
        private System.Windows.Forms.ToolStripTextBox pinsTextBox;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private Gear.GUI.TimeUnitToolStripComboBox toolStripCmbBoxUnit;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}
