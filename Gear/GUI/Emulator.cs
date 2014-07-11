/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * Emulator.cs
 * Emulator window class
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
using System.Xml;

using Gear.EmulationCore;
using Gear.PluginSupport;

namespace Gear.GUI
{
    public partial class Emulator : Form
    {
        private Propeller Chip;
        private String Source;
        private String LastFileName;
        private List<Control> FloatControls;

        private Timer runTimer;

        public Emulator(string source)
        {
            Chip = new Propeller(this);
            Source = source;
            FloatControls = new List<Control>();

            InitializeComponent();

            this.Text = "Propeller: " + source;

            // Create default layout
            for (int i = 0; i < 8; i++)
                AttachPlugin(new CogView(i));

            AttachPlugin(new MemoryView());
            AttachPlugin(new LogicProbe.LogicView());
            AttachPlugin(new SpinView());
            documentsTab.SelectedIndex = 0;

            // TEMPORARY RUN FUNCTION
            runTimer = new Timer();
            runTimer.Interval = 10;
            runTimer.Tick += new EventHandler(RunEmulatorStep);

            hubView.Host = Chip;
        }

        public void BreakPoint()
        {
            runTimer.Stop();
            RepaintViews();
        }

        private void AttachPlugin(PluginBase bm)
        {
            Chip.IncludePlugin(bm);
            bm.PresentChip(Chip);

            TabPage t = new TabPage(bm.Title);
            t.Parent = documentsTab;
            bm.Dock = DockStyle.Fill;
            bm.Parent = t;
            documentsTab.SelectedTab = t;
            // ASB: mantain the close button avalaibility
            if (bm.IsClosable)
            {
                closeButton.Enabled = true;
            }
            else
            {
                closeButton.Enabled = false;
            }
        }

        private void RunEmulatorStep(object sender, EventArgs e)
        {
            for (int i = 0; i < 1024; i++)
                if (!Chip.Step())
                {
                    runTimer.Stop();
                    break;
                }
            RepaintViews();
        }

        public void Unfloat(Control c)
        {
            TabPage tp = new TabPage(c.Text);

            tp.Parent = documentsTab;
            c.Parent = tp;
            c.Dock = DockStyle.Fill;

            FloatControls.Remove(c.Parent);
        }

        public bool OpenFile(string FileName)
        {
            try
            {
                Chip.Initialize(File.ReadAllBytes(FileName));
                LastFileName = FileName;
                RepaintViews();
                return true;
            }
            catch (IOException ioe)
            {
                MessageBox.Show(this,
                    ioe.Message,
                    "Failed to load program binary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return false;
            }
        }

        public PluginBase LoadPlugin(string FileName)
        {
            XmlTextReader tr = new XmlTextReader(FileName);
            bool ReadText = false;

            List<string> references = new List<string>();
            string instanceName = "";
            string code = "";

            try
            {

                while (tr.Read())
                {
                    if (tr.NodeType == XmlNodeType.Text && ReadText)
                    {
                        code = tr.Value;
                        ReadText = false;
                    }

                    switch (tr.Name.ToLower())
                    {
                        case "reference":
                            references.Add(tr.GetAttribute("name"));
                            break;
                        case "instance":
                            instanceName = tr.GetAttribute("class");
                            break;
                        case "code":
                            ReadText = true;
                            break;
                    }
                }


                PluginBase bm = ModuleLoader.LoadModule(code, instanceName, references.ToArray());

                if (bm == null)
                {
                    PluginEditor pe = new PluginEditor();
                    pe.OpenFile(FileName, true);
                    pe.MdiParent = this.MdiParent;
                    pe.Show();
                }
                else
                {
                    AttachPlugin(bm);
                }

                return bm;
            }
            catch (IOException ioe)
            {
                MessageBox.Show(this,
                    ioe.Message,
                    "Failed to load program binary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return null;
            }
            catch (XmlException xmle)
            {
                MessageBox.Show(this,
                    xmle.Message,
                    "Failed to load program binary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return null;
            }
            finally
            {
                tr.Close();
            }
        }

        private void openBinary_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Propeller Runtime Image (*.binary;*.eeprom)|*.binary;*.eeprom|All Files (*.*)|*.*";
            openFileDialog.Title = "Open Propeller Binary...";
            openFileDialog.FileName = Source;

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                OpenFile(openFileDialog.FileName);
        }

        private void reloadBinary_Click(object sender, EventArgs e)
        {
            OpenFile(LastFileName);
            Chip.Reset();
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (Control c in FloatControls)
                c.Parent = null;

            FloatControls.Clear();

            base.OnClosed(e);
        }

        private void RepaintViews()
        {
            foreach (Control s in FloatControls)
                s.Refresh();

            Control c = pinnedPanel.GetNextControl(null, true);

            if (c != null)
                ((PluginBase)c).Repaint(true);

            if (documentsTab.SelectedTab != null && (c = documentsTab.SelectedTab.GetNextControl(null, true)) != null)
                ((PluginBase)c).Repaint(true);

            hubView.DataChanged();
        }

        private void resetEmulator_Click(object sender, EventArgs e)
        {
            Chip.Reset();
            RepaintViews();
        }

        private void stepEmulator_Click(object sender, EventArgs e)
        {
            Chip.Step();
            RepaintViews();
        }

        private void closeActiveTab_Click(object sender, EventArgs e)
        {
            TabPage tp = documentsTab.SelectedTab;
            PluginBase p = (PluginBase)tp.Controls[0];

            if (p.IsClosable)
            {
                if (documentsTab.SelectedIndex > 0)
                {
                    documentsTab.SelectedIndex = documentsTab.SelectedIndex - 1;
                }
                tp.Parent = null;
            }
        }

        private void floatActiveTab_Click(object sender, EventArgs e)
        {
            TabPage tp = documentsTab.SelectedTab;
            tp.Parent = null;

            FloatedWindow fw = new FloatedWindow(this);

            Control c = tp.GetNextControl(null, true);
            c.Dock = DockStyle.Fill;
            c.Parent = fw;
            c.Text = tp.Text;

            fw.MdiParent = this.MdiParent;
            fw.Show();
            fw.Text = tp.Text + ": " + Source;

            FloatControls.Add(fw);
        }

        private void pinActiveTab_Click(object sender, EventArgs e)
        {
            Control oldPin = pinnedPanel.GetNextControl(null, true);

            TabPage tp = documentsTab.SelectedTab;
            tp.Parent = null;

            Control newPin = tp.GetNextControl(null, true);
            newPin.Dock = DockStyle.Fill;
            newPin.Parent = pinnedPanel;
            newPin.Text = tp.Text;

            if (pinnedSplitter.IsCollapsed)
                pinnedSplitter.ToggleState();

            if (oldPin != null)
            {
                tp = new TabPage(oldPin.Text);
                tp.Parent = documentsTab;
                oldPin.Parent = tp;
            }
        }

        private void unpinButton_Click(object sender, EventArgs e)
        {
            Control oldPin = pinnedPanel.GetNextControl(null, true);

            if (oldPin != null)
            {
                TabPage tp = new TabPage(oldPin.Text);
                tp.Parent = documentsTab;
                oldPin.Parent = tp;

                if (!pinnedSplitter.IsCollapsed)
                    pinnedSplitter.ToggleState();
            }
        }

        private void runEmulator_Click(object sender, EventArgs e)
        {
            runTimer.Start();
        }

        private void stopEmulator_Click(object sender, EventArgs e)
        {
            runTimer.Stop();
        }

        private void stepInstruction_Click(object sender, EventArgs e)
        {
            if (documentsTab.SelectedTab != null)
            {
                Control c = documentsTab.SelectedTab.GetNextControl(null, true);

                if (c != null && c is CogView)
                {
                    Cog cog = ((CogView)c).GetViewCog();

                    if (cog != null)
                        cog.StepInstruction();
                }
            }

            RepaintViews();
        }

        private void OpenPlugin_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Gear Plug-in (*.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog.Title = "Open Gear Plug-in...";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                LoadPlugin(openFileDialog.FileName);
        }

        private void OnDeactivate(object sender, EventArgs e)
        {
            runTimer.Stop();
        }

        // ASB: Method to mantain close button avaibility when tab is changed
        private void documentsTab_Click(object sender, EventArgs e)
        {
            TabPage tp = documentsTab.SelectedTab;
            if (tp.Controls[0] is PluginBase)
            {
                PluginBase b = (tp.Controls[0]) as PluginBase;
                if (b.IsClosable)
                {
                    closeButton.Enabled = true;
                }
                else
                {
                    closeButton.Enabled = false;
                }
            }
            else
            {
                closeButton.Enabled = false;
            }
        }

        private void documentsTab_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (ActiveControl is PluginBase)
            {
                PluginBase b = ActiveControl as PluginBase;
                if (b.AllowHotKeys != true)
                    return;
            }
            if ((e.KeyChar == 's') | (e.KeyChar == 'S'))
            {
                if (runTimer.Enabled)
                    runTimer.Stop();
                else
                    stepInstruction_Click(sender, e);
            }
            if ((e.KeyChar == 'r') | (e.KeyChar == 'R'))
            {
                if (!runTimer.Enabled)
                    runTimer.Start();
            }
        }
    }
}
