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
        /// @brief Last plugin succesfully opened or saved.
        /// Include complete path and name.
        /// @version V14.07.17 - Introduced.
        static public string LastPlugin;
        /// @brief Last bynary file succesfully opened.
        /// Include complete path and name.
        /// @version V14.07.17 - Introduced.
        static public string LastBinary;
        
        /// @todo Document Gear.GUI.GearDesktop()
        public GearDesktop()
        {
            InitializeComponent();
        }

        /// @brief Load a new emulator from file.
        /// Load an binary image into a new emulator, from user selected file, remembering last binary directory,
        /// independently from last plugin directory.
        private void OpenBinaryButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Propeller Runtime Image (*.binary;*.eeprom)|*.binary;*.eeprom|All Files (*.*)|*.*";
            openFileDialog.Title = "Open Propeller Binary...";
            if (LastBinary != null)
                openFileDialog.InitialDirectory = Path.GetDirectoryName(LastBinary);   //retrieve last binary location

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Emulator emul = new Emulator(Path.GetFileName(openFileDialog.FileName));
                if (emul.OpenFile(openFileDialog.FileName))   //if succesfully load binary in emulator
                {
                    //show emulator window
                    emul.MdiParent = this;
                    emul.WindowState = FormWindowState.Maximized;
                    emul.Show();
                    //remember last binary succesfully opened
                    LastBinary = emul.GetLastBinary;
                }
            }
        }

        /// @todo Document Gear.GUI.ExitToolsStripMenuItem_Click().
        /// 
        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// @todo Document Gear.GUI.CascadeToolStripMenuItem_Click()
        /// 
        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        /// @todo Document Gear.GUI.TileVerticleToolStripMenuItem_Click()
        /// 
        private void TileVerticleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        /// @todo Document Gear.GUI.TileHorizontalToolStripMenuItem_Click()
        /// 
        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        /// @todo Document Gear.GUI.ArrangeIconsToolStripMenuItem_Click()
        /// 
        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        /// @todo Document Gear.GUI.CloseAllToolStripMenuItem_Click()
        /// 
        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        /// @todo Document Gear.GUI.aboutToolStripMenuItem_Click()
        /// 
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutGear about = new AboutGear();
            about.ShowDialog(this);
        }

        /// @brief Load plugin editor from file.
        /// Load a plugin definition into a new editor window, from user selected file, remembering last plugin directory,
        /// independently from last binary directory.
        private void OpenPluginButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog.Title = "Open Gear Plug-in...";
            if (LastPlugin != null)
                openFileDialog.InitialDirectory = Path.GetDirectoryName(LastPlugin);

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                PluginEditor plugin = new PluginEditor();

                if (plugin.OpenFile(openFileDialog.FileName, false))
                {
                    //show plugin editor loaded with selected one
                    plugin.MdiParent = this;
                    plugin.Show();
                    //remember plugin succesfully loaded
                    LastPlugin = plugin.GetLastPlugin;
                }
            }
        }

        /// @todo Document Gear.GUI.newPluginButton_Click()
        /// 
        private void newPluginButton_Click(object sender, EventArgs e)
        {
            PluginEditor plugin = new PluginEditor();
            plugin.MdiParent = this;
            plugin.Show();
        }

    }
}
