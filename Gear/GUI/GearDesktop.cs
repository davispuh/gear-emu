/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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

using Gear.Utils;
using System;
using System.IO;
using System.Windows.Forms;

// ReSharper disable InvalidXmlDocComment

/// <summary>Contains the definitions of %GUI objects (controlling
/// objects).</summary>
namespace Gear.GUI
{
    /// <summary>Implements the graphical Desktop to the emulator, plugin
    /// editor and related windows.</summary>
    public partial class GearDesktop : Form
    {
        /// <summary>Default constructor.</summary>
        public GearDesktop()
        {
            InitializeComponent();
        }

        /// <summary>Load a new emulator from file.</summary>
        /// <remarks>Load an binary image into a new emulator, from user
        /// selected file, remembering last binary directory, independently
        /// from last plugin directory.</remarks>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.02 - Corrected error if no file name was selected
        /// on dialog, but pressed open button. Also changed local variable
        /// name to clarify its meaning.
        private void OpenBinaryButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog
                   {
                       Filter = @"Propeller Runtime Image (*.binary;*.eeprom)|*.binary;" +
                                @"*.eeprom|All Files (*.*)|*.*",
                       Title = @"Open Propeller Binary..."
                   })
            {
                //retrieve last binary location
                if (!string.IsNullOrEmpty(Properties.Settings.Default.LastBinary))
                    dialog.InitialDirectory =
                        Path.GetDirectoryName(Properties.Settings.Default.LastBinary);
                //invoke Dialog
                if (dialog.ShowDialog(this) != DialogResult.OK ||
                    string.IsNullOrEmpty(dialog.FileName))
                    return;
                Emulator emulator = new Emulator(Path.GetFileName(dialog.FileName));
                //if successfully load binary in emulator
                if (emulator.OpenFile(dialog.FileName))
                {
                    //show emulator window
                    emulator.MdiParent = this;
                    emulator.WindowState = FormWindowState.Maximized;
                    emulator.Show();
                    //remember last binary successfully opened
                    Properties.Settings.Default.LastBinary = emulator.LastBinary;
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>Close the application.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>Arrange the emulator windows in cascade layout.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        /// <summary>Arrange the emulator windows in Vertical Tiles.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        /// <summary>Arrange the emulator windows in Horizontal Tiles.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        /// <summary>Arrange the emulator windows in icons layout.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        /// <summary>Close all the Emulators windows.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        /// <summary>Show the details about the GEAR Emulator.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutGear about = new AboutGear();
            about.ShowDialog(this);
        }

        /// <summary>Load plugin editor from file.</summary>
        /// <remarks>Load a plugin definition into a new editor window, from user selected file,
        /// remembering independently from last binary directory.</remarks>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.02 - Corrected error if no file name was selected
        /// on dialog, but pressed open button and modified local variables
        /// name to clarify its meaning.
        private void OpenPluginButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = @"Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = @"Open Gear Plug-in..."
            };
            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastPlugin))
                dialog.InitialDirectory =
                    Path.GetDirectoryName(Properties.Settings.Default.LastPlugin);

            if (dialog.ShowDialog(this) != DialogResult.OK ||
                string.IsNullOrEmpty(dialog.FileName))
                return;
            PluginEditor pluginEditor = new PluginEditor(false);

            if (!pluginEditor.OpenPluginFromFile(dialog.FileName, false))
                return;
            //show plugin editor loaded with selected one
            pluginEditor.MdiParent = this;
            pluginEditor.Show();
        }

        /// <summary>Load plugin editor from file.</summary>
        /// <remarks>Load a plugin definition into a new editor window, from
        /// user selected file, remembering independently from last binary
        /// directory.</remarks>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v20.09.03 - Added.
        private void EditPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenPluginButton_Click(sender, e);
        }

        /// <summary>Open a window with the plugin editor to create a new
        /// plugin.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.02 - Modified local variable name to clarify
        /// its meaning.
        private void NewPluginButton_Click(object sender, EventArgs e)
        {
            //load default plugin template
            PluginEditor pluginEditor = new PluginEditor(true)
            {
                MdiParent = this
            };
            pluginEditor.Show();
        }

        /// <summary>Open a window with the plugin editor to create a new
        /// plugin.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v20.09.03 - Added.
        private void NewPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewPluginButton_Click(sender, e);
        }

        /// <summary>Open Gear properties editor.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v20.10.01 - Edited to manage only one instance of
        /// AppPropertiesEditor.
        private void OptionsButton_Click(object sender, EventArgs e)
        {
            try
            {
                AppPropertiesEditor options = new AppPropertiesEditor
                {
                    MdiParent = this
                };
                options.Show();
            }
            catch (SingleInstanceException)
            {
                foreach(var form in MdiChildren)
                    if (form is AppPropertiesEditor)
                    {
                        form.Activate();
                        break;
                    }
            }
        }

        /// <summary>Open Gear properties editor.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v20.05.00 - Added.
        private void OptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsButton_Click(sender, e);
        }
    }
}
