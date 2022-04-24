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
    partial class HubView
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
            this.coreFrequency = new System.Windows.Forms.Label();
            this.xtalFrequency = new System.Windows.Forms.Label();
            this.clockMode = new System.Windows.Forms.Label();
            this.systemCounter = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.elapsedTime = new System.Windows.Forms.Label();
            this.timeUnitSelector = new Gear.GUI.TimeUnitComboBox();
            this.timeLabel = new System.Windows.Forms.Label();
            this.pinIN = new Gear.GUI.BitView();
            this.pinDIR = new Gear.GUI.BitView();
            this.pinFloating = new Gear.GUI.BitView();
            this.pinLocksFree = new Gear.GUI.BitView();
            this.pinLocks = new Gear.GUI.BitView();
            this.ringMeter = new Gear.GUI.RingMeter();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // label8
            //
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 299);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(49, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Core [hz]";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label7
            //
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 280);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Xtal [hz]";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 261);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Clock Mode";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label13
            //
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(3, 217);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(44, 13);
            this.label13.TabIndex = 12;
            this.label13.Text = "Counter";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label12
            //
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 368);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(22, 13);
            this.label12.TabIndex = 11;
            this.label12.Text = "IN*";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label11
            //
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 406);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(30, 13);
            this.label11.TabIndex = 10;
            this.label11.Text = "DIR*";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label5
            //
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 343);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(36, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Locks";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 324);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Lock Free";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 444);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Floating";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // coreFrequency
            //
            this.coreFrequency.AutoSize = true;
            this.coreFrequency.Cursor = System.Windows.Forms.Cursors.Hand;
            this.coreFrequency.Location = new System.Drawing.Point(76, 299);
            this.coreFrequency.Name = "coreFrequency";
            this.coreFrequency.Size = new System.Drawing.Size(78, 13);
            this.coreFrequency.TabIndex = 21;
            this.coreFrequency.Text = "coreFrequency";
            this.coreFrequency.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.coreFrequency.Click += new System.EventHandler(this.FrequencyLabels_Click);
            //
            // xtalFrequency
            //
            this.xtalFrequency.AutoSize = true;
            this.xtalFrequency.Cursor = System.Windows.Forms.Cursors.Hand;
            this.xtalFrequency.Location = new System.Drawing.Point(76, 280);
            this.xtalFrequency.Name = "xtalFrequency";
            this.xtalFrequency.Size = new System.Drawing.Size(73, 13);
            this.xtalFrequency.TabIndex = 20;
            this.xtalFrequency.Text = "xtalFrequency";
            this.xtalFrequency.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.xtalFrequency.Click += new System.EventHandler(this.FrequencyLabels_Click);
            //
            // clockMode
            //
            this.clockMode.AutoSize = true;
            this.clockMode.Location = new System.Drawing.Point(76, 261);
            this.clockMode.Name = "clockMode";
            this.clockMode.Size = new System.Drawing.Size(60, 13);
            this.clockMode.TabIndex = 19;
            this.clockMode.Text = "clockMode";
            this.clockMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // systemCounter
            //
            this.systemCounter.AutoSize = true;
            this.systemCounter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.systemCounter.Location = new System.Drawing.Point(76, 217);
            this.systemCounter.Name = "systemCounter";
            this.systemCounter.Size = new System.Drawing.Size(78, 13);
            this.systemCounter.TabIndex = 13;
            this.systemCounter.Text = "SystemCounter";
            this.systemCounter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.systemCounter.Click += new System.EventHandler(this.FrequencyLabels_Click);
            //
            // elapsedTime
            //
            this.elapsedTime.AutoSize = true;
            this.elapsedTime.Cursor = System.Windows.Forms.Cursors.Hand;
            this.elapsedTime.Location = new System.Drawing.Point(76, 236);
            this.elapsedTime.Name = "elapsedTime";
            this.elapsedTime.Size = new System.Drawing.Size(68, 13);
            this.elapsedTime.TabIndex = 33;
            this.elapsedTime.Text = "ElapsedTime";
            this.toolTip1.SetToolTip(this.elapsedTime, "Elapsed time from begining of emulation.\r\n(Click to change to next time unit.)");
            this.elapsedTime.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ElapsedTime_MouseClick);
            //
            // timeUnitSelector
            //
            this.timeUnitSelector.BaseUnit = Gear.Utils.TimeUnitsEnum.s;
            this.timeUnitSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.timeUnitSelector.ExcludedUnits.Add(Gear.Utils.TimeUnitsEnum.None);
            this.timeUnitSelector.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.timeUnitSelector.FormattingEnabled = true;
            this.timeUnitSelector.Location = new System.Drawing.Point(32, 232);
            this.timeUnitSelector.Name = "timeUnitSelector";
            this.timeUnitSelector.Size = new System.Drawing.Size(40, 21);
            this.timeUnitSelector.TabIndex = 34;
            this.timeUnitSelector.TabStop = false;
            this.timeUnitSelector.TimeUnitSelected = Gear.Utils.TimeUnitsEnum.None;
            this.toolTip1.SetToolTip(this.timeUnitSelector, "Time unit of emulation time.");
            this.timeUnitSelector.SelectedIndexChanged += new System.EventHandler(this.TimeUnitSelector_SelectedIndexChanged);
            //
            // timeLabel
            //
            this.timeLabel.AutoSize = true;
            this.timeLabel.Location = new System.Drawing.Point(3, 236);
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(30, 13);
            this.timeLabel.TabIndex = 32;
            this.timeLabel.Text = "Time";
            //
            // pinIN
            //
            this.pinIN.Bits = 64;
            this.pinIN.Location = new System.Drawing.Point(78, 368);
            this.pinIN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pinIN.Name = "pinIN";
            this.pinIN.Postfix = "";
            this.pinIN.Prefix = "P";
            this.pinIN.Size = new System.Drawing.Size(128, 32);
            this.pinIN.TabIndex = 22;
            this.pinIN.Value = ((ulong)(0ul));
            //
            // pinDIR
            //
            this.pinDIR.Bits = 64;
            this.pinDIR.Location = new System.Drawing.Point(78, 406);
            this.pinDIR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pinDIR.Name = "pinDIR";
            this.pinDIR.Postfix = "";
            this.pinDIR.Prefix = "P";
            this.pinDIR.Size = new System.Drawing.Size(128, 32);
            this.pinDIR.TabIndex = 23;
            this.pinDIR.Value = ((ulong)(0ul));
            //
            // pinFloating
            //
            this.pinFloating.Bits = 64;
            this.pinFloating.Location = new System.Drawing.Point(78, 444);
            this.pinFloating.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pinFloating.Name = "pinFloating";
            this.pinFloating.Postfix = "";
            this.pinFloating.Prefix = "P";
            this.pinFloating.Size = new System.Drawing.Size(128, 32);
            this.pinFloating.TabIndex = 26;
            this.pinFloating.Value = ((ulong)(0ul));
            //
            // pinLocksFree
            //
            this.pinLocksFree.Bits = 8;
            this.pinLocksFree.Location = new System.Drawing.Point(78, 327);
            this.pinLocksFree.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pinLocksFree.Name = "pinLocksFree";
            this.pinLocksFree.Postfix = "";
            this.pinLocksFree.Prefix = "Lock";
            this.pinLocksFree.Size = new System.Drawing.Size(110, 10);
            this.pinLocksFree.TabIndex = 27;
            this.pinLocksFree.Value = ((ulong)(0ul));
            //
            // pinLocks
            //
            this.pinLocks.AutoSize = true;
            this.pinLocks.Bits = 8;
            this.pinLocks.Location = new System.Drawing.Point(78, 346);
            this.pinLocks.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pinLocks.Name = "pinLocks";
            this.pinLocks.Postfix = "";
            this.pinLocks.Prefix = "Lock";
            this.pinLocks.Size = new System.Drawing.Size(107, 10);
            this.pinLocks.TabIndex = 28;
            this.pinLocks.Value = ((ulong)(0ul));
            //
            // ringMeter
            //
            this.ringMeter.Location = new System.Drawing.Point(0, 0);
            this.ringMeter.MaximumSize = new System.Drawing.Size(214, 214);
            this.ringMeter.MinimumSize = new System.Drawing.Size(214, 214);
            this.ringMeter.Name = "ringMeter";
            this.ringMeter.Size = new System.Drawing.Size(214, 214);
            this.ringMeter.TabIndex = 0;
            this.ringMeter.Value = ((uint)(0u));
            //
            // HubView
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.timeUnitSelector);
            this.Controls.Add(this.elapsedTime);
            this.Controls.Add(this.timeLabel);
            this.Controls.Add(this.coreFrequency);
            this.Controls.Add(this.xtalFrequency);
            this.Controls.Add(this.clockMode);
            this.Controls.Add(this.systemCounter);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.pinIN);
            this.Controls.Add(this.pinDIR);
            this.Controls.Add(this.pinFloating);
            this.Controls.Add(this.pinLocksFree);
            this.Controls.Add(this.pinLocks);
            this.Controls.Add(this.ringMeter);
            this.DoubleBuffered = true;
            this.Name = "HubView";
            this.Size = new System.Drawing.Size(214, 546);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private RingMeter ringMeter;
        private System.Windows.Forms.Label coreFrequency;
        private System.Windows.Forms.Label xtalFrequency;
        private System.Windows.Forms.Label clockMode;
        private System.Windows.Forms.Label systemCounter;
        private BitView pinIN;
        private BitView pinDIR;
        private BitView pinFloating;
        private BitView pinLocksFree;
        private BitView pinLocks;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label timeLabel;
        private System.Windows.Forms.Label elapsedTime;
        private TimeUnitComboBox timeUnitSelector;
    }
}
