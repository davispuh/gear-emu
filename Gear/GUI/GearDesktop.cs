/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * GearDesktop.cs
 * Main window class for gear
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

/// @brief Contains definitions related to GUI objects (controlling objects) namespace.
/// @namespace Gear.GUI
namespace Gear.GUI
{
    /// @todo Document Gear.GUI.GearDesktop class
    public partial class GearDesktop : Form
    {
        /// @todo Document Gear.GUI.GearDesktop()
        public GearDesktop()
        {
            InitializeComponent();
        }

        /// @todo Document Gear.GUI.OpenFile()
        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Propeller Runtime Image (*.binary;*.eeprom)|*.binary;*.eeprom|All Files (*.*)|*.*";
            openFileDialog.Title = "Open Propeller Binary...";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Emulator em = new Emulator(Path.GetFileName(openFileDialog.FileName));
                if (em.OpenFile(openFileDialog.FileName))
                {
                    em.MdiParent = this;
                    em.WindowState = FormWindowState.Maximized;
                    em.Show();
                }
            }
        }

        /// @todo Document Gear.GUI.ExitToolsStripMenuItem_Click()
        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// @todo Document Gear.GUI.CascadeToolStripMenuItem_Click()
        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        /// @todo Document Gear.GUI.TileVerticleToolStripMenuItem_Click()
        private void TileVerticleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        /// @todo Document Gear.GUI.TileHorizontalToolStripMenuItem_Click()
        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        /// @todo Document Gear.GUI.ArrangeIconsToolStripMenuItem_Click()
        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        /// @todo Document Gear.GUI.CloseAllToolStripMenuItem_Click()
        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        /// @todo Document Gear.GUI.aboutToolStripMenuItem_Click()
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutGear about = new AboutGear();
            about.ShowDialog(this);
        }

        /// @todo Document Gear.GUI.OpenPluginButton_Click()
        private void OpenPluginButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog.Title = "Open Gear Plug-in...";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                PluginEditor plugin = new PluginEditor();

                if (plugin.OpenFile(openFileDialog.FileName, false))
                {
                    plugin.MdiParent = this;
                    plugin.Show();
                }
            }
        }

        /// @todo Document Gear.GUI.newPlugin_Click()
        private void newPlugin_Click(object sender, EventArgs e)
        {
            PluginEditor plugin = new PluginEditor();
            plugin.MdiParent = this;
            plugin.Show();
        }


    }
}
