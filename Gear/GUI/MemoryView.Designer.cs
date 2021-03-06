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

namespace Gear.GUI
{
    partial class MemoryView
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
            this.positionScrollBar = new System.Windows.Forms.VScrollBar();
            this.memoryPanel = new Gear.GUI.DoubleBufferedPanel();
            this.SuspendLayout();
            // 
            // positionScrollBar
            // 
            this.positionScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.positionScrollBar.LargeChange = 16;
            this.positionScrollBar.Location = new System.Drawing.Point(208, 0);
            this.positionScrollBar.Name = "positionScrollBar";
            this.positionScrollBar.Size = new System.Drawing.Size(17, 231);
            this.positionScrollBar.TabIndex = 0;
            this.positionScrollBar.TabStop = true;
            this.positionScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.PositionChanged);
            // 
            // memoryPanel
            // 
            this.memoryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.memoryPanel.Location = new System.Drawing.Point(0, 0);
            this.memoryPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.memoryPanel.Name = "memoryPanel";
            this.memoryPanel.Size = new System.Drawing.Size(360, 231);
            this.memoryPanel.TabIndex = 1;
            this.memoryPanel.SizeChanged += new System.EventHandler(this.SizeChange);
            this.memoryPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintMemoryView);
            this.memoryPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MemoryPanel_MouseClick);
            // 
            // MemoryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.memoryPanel);
            this.Controls.Add(this.positionScrollBar);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MemoryView";
            this.Size = new System.Drawing.Size(225, 231);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.VScrollBar positionScrollBar;
        private Gear.GUI.DoubleBufferedPanel memoryPanel;
    }
}
