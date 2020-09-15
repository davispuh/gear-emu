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
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/// @brief Contains the definitions of %GUI objects (controlling objects).
namespace Gear.GUI
{
    /// @brief Implements the graphical Desktop to the emulator, plugin editor and 
    /// related windows.
    public partial class GearDesktop : Form
    {
        /// @brief Default constructor.
        public GearDesktop()
        {
            InitializeComponent();
        }

        /// @brief Load a new emulator from file.
        /// Load an binary image into a new emulator, from user selected file, remembering last
        /// binary directory, independently from last plugin directory.
        private void OpenBinaryButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Propeller Runtime Image (*.binary;*.eeprom)|*.binary;" +
                "*.eeprom|All Files (*.*)|*.*",
                Title = "Open Propeller Binary..."
            };
            if (!String.IsNullOrEmpty(Properties.Settings.Default.LastBinary))
                //retrieve last binary location
                openFileDialog.InitialDirectory = 
                    Path.GetDirectoryName(Properties.Settings.Default.LastBinary);   

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Emulator emul = new Emulator(Path.GetFileName(openFileDialog.FileName));
                //if successfully load binary in emulator
                if (emul.OpenFile(openFileDialog.FileName))
                {
                    //show emulator window
                    emul.MdiParent = this;
                    emul.WindowState = FormWindowState.Maximized;
                    emul.Show();
                    //remember last binary successfully opened
                    Properties.Settings.Default.LastBinary = emul.LastBinary;
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

        /// @brief Arrange the emulator windows in Vertical Tiles.
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
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutGear about = new AboutGear();
            about.ShowDialog(this);
        }

        /// @brief Load plugin editor from file.
        /// @details Load a plugin definition into a new editor window, from user selected file, 
        /// remembering independently from last binary directory.
        /// @param[in] sender Reference to object where event was raised.
        /// @param[in] e Event data arguments.
        private void OpenPluginButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Open Gear Plug-in..."
            };
            if (!String.IsNullOrEmpty(Properties.Settings.Default.LastPlugin))
                openFileDialog.InitialDirectory = 
                    Path.GetDirectoryName(Properties.Settings.Default.LastPlugin);

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                PluginEditor plugin = new PluginEditor(false);

                if (plugin.OpenFile(openFileDialog.FileName, false))
                {
                    //remember plugin successfully loaded
                    plugin.UpdateDefaultLastPluginOpened();
                    //show plugin editor loaded with selected one
                    plugin.MdiParent = this;
                    plugin.ShowErrorGrid(false);    //hide it by default
                    plugin.Show();
                }
            }
        }

        /// @brief Open a window with the plugin editor to create a new plugin.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        private void NewPluginButton_Click(object sender, EventArgs e)
        {
            //load default plugin template
            PluginEditor plugin = new PluginEditor(true)
            {
                MdiParent = this
            };
            plugin.Show();
        }

        /// @brief Open Gear properties editor.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @since 20-05-00 - Added.
        private void OptionsButton_Click(object sender, EventArgs e)
        {
            var options = new AppPropertiesEditor
            {
                MdiParent = this
            };
            options.Show();
        }

        /// @brief Open Gear properties editor
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @since 20-05-00 - Added.
        private void OptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsButton_Click(sender, e);
        }

    }
}
