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

namespace Gear.GUI
{
    public partial class GearDesktop : Form
    {
        public GearDesktop()
        {
            InitializeComponent();
        }

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

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutGear about = new AboutGear();
            about.ShowDialog(this);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
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

        private void newPlugin_Click(object sender, EventArgs e)
        {
            PluginEditor plugin = new PluginEditor();
            plugin.MdiParent = this;
            plugin.Show();
        }
    }
}
