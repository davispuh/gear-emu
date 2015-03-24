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
    /// @brief Implements the graphical Desktop to the emulator, plugin editor and related windows.
    public partial class GearDesktop : Form
    {
        /// @brief Gear.GUI.GearDesktop Constructor.
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
            if (Properties.Settings.Default.LastBinary.Length > 0)
                //retrieve last binary location
                openFileDialog.InitialDirectory = 
                    Path.GetDirectoryName(Properties.Settings.Default.LastBinary);   

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
                    Properties.Settings.Default.LastBinary = emul.GetLastBinary;
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// @brief Close the application.
        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// @brief Arrange the emulator windows in cascade layout.
        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        /// @brief Arrange the emulator windows in Verticle Tiles.
        private void TileVerticleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        /// @brief Arrange the emulator windows in Horizontal Tiles. 
        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        /// @brief Arrange the emulator windows in icons layout.
        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        /// @brief Close all the Emulators windows.
        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        /// @brief Show the details about the GEAR Emulator.
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutGear about = new AboutGear();
            about.ShowDialog(this);
        }

        /// @brief Load plugin editor from file.
        /// @details Load a plugin definition into a new editor window, from user selected file, 
        /// remembering independently from last binary directory.
        private void OpenPluginButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog.Title = "Open Gear Plug-in...";
            if (Properties.Settings.Default.LastPlugin.Length > 0)
                openFileDialog.InitialDirectory = 
                    Path.GetDirectoryName(Properties.Settings.Default.LastPlugin);

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                PluginEditor plugin = new PluginEditor(false);

                if (plugin.OpenFile(openFileDialog.FileName, false))
                {
                    //show plugin editor loaded with selected one
                    plugin.MdiParent = this;
                    plugin.Show();
                    //remember plugin succesfully loaded
                    Properties.Settings.Default.LastPlugin = plugin.GetLastPlugin;
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// @brief Open a window with the plugin editor to create a new plugin.
        private void newPluginButton_Click(object sender, EventArgs e)
        {
            //load default plugin template
            PluginEditor plugin = new PluginEditor(! Properties.Settings.Default.UseNoTemplate);   
            plugin.MdiParent = this;
            plugin.Show();
        }

    }
}
