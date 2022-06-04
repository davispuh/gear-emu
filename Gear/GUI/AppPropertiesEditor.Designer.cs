/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * AppPropertiesEditor.Designer.cs
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
    partial class AppPropertiesEditor
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.GearPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.OKButton = new System.Windows.Forms.Button();
            this.ButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ResetButton = new System.Windows.Forms.Button();
            this.ButtonsPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // GearPropertyGrid
            //
            this.GearPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GearPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.GearPropertyGrid.MinimumSize = new System.Drawing.Size(250, 210);
            this.GearPropertyGrid.Name = "GearPropertyGrid";
            this.GearPropertyGrid.Size = new System.Drawing.Size(375, 279);
            this.GearPropertyGrid.TabIndex = 0;
            this.GearPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.GearPropertyGrid_PropertyValueChanged);
            //
            // OKButton
            //
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(297, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 3;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            //
            // ButtonsPanel
            //
            this.ButtonsPanel.Controls.Add(this.OKButton);
            this.ButtonsPanel.Controls.Add(this.ResetButton);
            this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.ButtonsPanel.Location = new System.Drawing.Point(0, 279);
            this.ButtonsPanel.MaximumSize = new System.Drawing.Size(0, 29);
            this.ButtonsPanel.MinimumSize = new System.Drawing.Size(250, 29);
            this.ButtonsPanel.Name = "ButtonsPanel";
            this.ButtonsPanel.Size = new System.Drawing.Size(375, 29);
            this.ButtonsPanel.TabIndex = 5;
            //
            // ResetButton
            //
            this.ResetButton.AutoSize = true;
            this.ResetButton.Location = new System.Drawing.Point(170, 3);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(121, 23);
            this.ResetButton.TabIndex = 4;
            this.ResetButton.Text = "Reset to default value";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            //
            // AppPropertiesEditor
            //
            this.AcceptButton = this.OKButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(375, 308);
            this.Controls.Add(this.GearPropertyGrid);
            this.Controls.Add(this.ButtonsPanel);
            this.DoubleBuffered = true;
            this.Icon = global::Gear.Properties.Resources.Icon_PropertiesEditor;
            this.MinimumSize = new System.Drawing.Size(270, 290);
            this.Name = "AppPropertiesEditor";
            this.Text = "Gear Properties";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AppPropertiesEditor_FormClosed);
            this.Load += new System.EventHandler(this.AppPropertiesEditor_Load);
            this.ButtonsPanel.ResumeLayout(false);
            this.ButtonsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid GearPropertyGrid;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.FlowLayoutPanel ButtonsPanel;
        private System.Windows.Forms.Button ResetButton;
    }
}