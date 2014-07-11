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
            System.Windows.Forms.Label label8;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label13;
            System.Windows.Forms.Label label12;
            System.Windows.Forms.Label label11;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label9;
            this.coreFrequency = new System.Windows.Forms.Label();
            this.xtalFrequency = new System.Windows.Forms.Label();
            this.clockMode = new System.Windows.Forms.Label();
            this.systemCounter = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pinIN = new Gear.GUI.BitView();
            this.pinDIR = new Gear.GUI.BitView();
            this.pinFloating = new Gear.GUI.BitView();
            this.pinLocksFree = new Gear.GUI.BitView();
            this.pinLocks = new Gear.GUI.BitView();
            this.ringMeter = new Gear.GUI.RingMeter();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            label12 = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // label8
            //
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(3, 282);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(29, 13);
            label8.TabIndex = 18;
            label8.Text = "Core";
            label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label7
            //
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(3, 262);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(25, 13);
            label7.TabIndex = 17;
            label7.Text = "Xtal";
            label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label6
            //
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(3, 242);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(64, 13);
            label6.TabIndex = 16;
            label6.Text = "Clock Mode";
            label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label13
            //
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(3, 217);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(44, 13);
            label13.TabIndex = 12;
            label13.Text = "Counter";
            label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label12
            //
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(3, 361);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(22, 13);
            label12.TabIndex = 11;
            label12.Text = "IN*";
            label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label11
            //
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(3, 399);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(30, 13);
            label11.TabIndex = 10;
            label11.Text = "DIR*";
            label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            label11.Click += new System.EventHandler(this.label11_Click);
            //
            // label5
            //
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(3, 332);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(36, 13);
            label5.TabIndex = 4;
            label5.Text = "Locks";
            label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label4
            //
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(3, 312);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(55, 13);
            label4.TabIndex = 3;
            label4.Text = "Lock Free";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label3
            //
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 437);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(44, 13);
            label3.TabIndex = 2;
            label3.Text = "Floating";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label9
            //
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(3, 481);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(61, 13);
            label9.TabIndex = 29;
            label9.Text = "Quick Keys";
            label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // coreFrequency
            //
            this.coreFrequency.AutoSize = true;
            this.coreFrequency.Location = new System.Drawing.Point(73, 282);
            this.coreFrequency.Name = "coreFrequency";
            this.coreFrequency.Size = new System.Drawing.Size(41, 13);
            this.coreFrequency.TabIndex = 21;
            this.coreFrequency.Text = "label14";
            this.coreFrequency.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // xtalFrequency
            //
            this.xtalFrequency.AutoSize = true;
            this.xtalFrequency.Location = new System.Drawing.Point(73, 262);
            this.xtalFrequency.Name = "xtalFrequency";
            this.xtalFrequency.Size = new System.Drawing.Size(41, 13);
            this.xtalFrequency.TabIndex = 20;
            this.xtalFrequency.Text = "label10";
            this.xtalFrequency.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // clockMode
            //
            this.clockMode.AutoSize = true;
            this.clockMode.Location = new System.Drawing.Point(73, 242);
            this.clockMode.Name = "clockMode";
            this.clockMode.Size = new System.Drawing.Size(35, 13);
            this.clockMode.TabIndex = 19;
            this.clockMode.Text = "label9";
            this.clockMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // systemCounter
            //
            this.systemCounter.AutoSize = true;
            this.systemCounter.Location = new System.Drawing.Point(73, 217);
            this.systemCounter.Name = "systemCounter";
            this.systemCounter.Size = new System.Drawing.Size(61, 13);
            this.systemCounter.TabIndex = 13;
            this.systemCounter.Text = "SysCounter";
            this.systemCounter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(73, 501);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 31;
            this.label1.Text = "S - Stop or Step";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(73, 481);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 30;
            this.label2.Text = "R - Run";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // pinIN
            //
            this.pinIN.Bits = 64;
            this.pinIN.Location = new System.Drawing.Point(73, 361);
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
            this.pinDIR.Location = new System.Drawing.Point(73, 399);
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
            this.pinFloating.Location = new System.Drawing.Point(73, 437);
            this.pinFloating.Name = "pinFloating";
            this.pinFloating.Postfix = "";
            this.pinFloating.Prefix = "P";
            this.pinFloating.Size = new System.Drawing.Size(138, 41);
            this.pinFloating.TabIndex = 26;
            this.pinFloating.Value = ((ulong)(0ul));
            //
            // pinLocksFree
            //
            this.pinLocksFree.Bits = 8;
            this.pinLocksFree.Location = new System.Drawing.Point(73, 315);
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
            this.pinLocks.Location = new System.Drawing.Point(73, 335);
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
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(label9);
            this.Controls.Add(this.coreFrequency);
            this.Controls.Add(this.xtalFrequency);
            this.Controls.Add(this.clockMode);
            this.Controls.Add(this.systemCounter);
            this.Controls.Add(label8);
            this.Controls.Add(label7);
            this.Controls.Add(label6);
            this.Controls.Add(label5);
            this.Controls.Add(label4);
            this.Controls.Add(label3);
            this.Controls.Add(label11);
            this.Controls.Add(label13);
            this.Controls.Add(label12);
            this.Controls.Add(this.pinIN);
            this.Controls.Add(this.pinDIR);
            this.Controls.Add(this.pinFloating);
            this.Controls.Add(this.pinLocksFree);
            this.Controls.Add(this.pinLocks);
            this.Controls.Add(this.ringMeter);
            this.Name = "HubView";
            this.Size = new System.Drawing.Size(214, 546);
            this.Load += new System.EventHandler(this.HubView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
