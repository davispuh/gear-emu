/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * PluginEditor.cs
 * Editor window for plugins class
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
using System.Xml;
using System.IO;
using System.CodeDom.Compiler;

using Gear.PluginSupport;

namespace Gear.GUI
{
    public partial class PluginEditor : Form
    {
        private string m_SaveFileName;

        public PluginEditor()
        {
            InitializeComponent();
            m_SaveFileName = null;

            codeEditorView.Font = new Font(FontFamily.GenericMonospace, 10);
            if (codeEditorView.Font == null)
                codeEditorView.Font = this.Font;

            errorListView.FullRowSelect = true;
            errorListView.GridLines = true;

            errorListView.Columns.Add("Name", -2, HorizontalAlignment.Left);
            errorListView.Columns.Add("Line", -2, HorizontalAlignment.Left);
            errorListView.Columns.Add("Column", -2, HorizontalAlignment.Left);
            errorListView.Columns.Add("Message", -2, HorizontalAlignment.Left);
        }

        public bool OpenFile(string FileName, bool displayErrors)
        {
            XmlTextReader tr = new XmlTextReader(FileName);
            bool ReadText = false;

            try
            {

                while (tr.Read())
                {
                    if (tr.NodeType == XmlNodeType.Text && ReadText )
                    {
                        codeEditorView.Text = tr.Value;
                        ReadText = false;
                    }

                    switch (tr.Name.ToLower())
                    {
                        case "reference":
                            referencesList.Items.Add(tr.GetAttribute("name"));
                            break;
                        case "instance":
                            instanceName.Text = tr.GetAttribute("class");
                            break;
                        case "code":
                            ReadText = true;
                            break;
                    }
                }
                m_SaveFileName = FileName;

                if (displayErrors)
                {
                    errorListView.Items.Clear();
                    ModuleLoader.EnumerateErrors(EnumErrors);
                }

                return true;
            }
            catch (IOException ioe)
            {
                MessageBox.Show(this,
                    ioe.Message,
                    "Failed to load plug-in",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return false;
            }
            catch (XmlException xmle)
            {
                MessageBox.Show(this,
                    xmle.Message,
                    "Failed to load plug-in",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return false;
            }
            finally
            {
                tr.Close();
            }
        }

        public void SaveFile(string FileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement root = xmlDoc.CreateElement("plugin");
            xmlDoc.AppendChild(root);

            XmlElement instance = xmlDoc.CreateElement("instance");
            instance.SetAttribute("class", instanceName.Text);
            root.AppendChild(instance);

            foreach (string s in referencesList.Items)
            {
                instance = xmlDoc.CreateElement("reference");
                instance.SetAttribute("name", s);
                root.AppendChild(instance);
            }

            instance = xmlDoc.CreateElement("code");
            instance.AppendChild(xmlDoc.CreateTextNode(codeEditorView.Text));
            root.AppendChild(instance);

            xmlDoc.Save(FileName);

            m_SaveFileName = FileName;
        }

        private void CheckSource_Click(object sender, EventArgs e)
        {
            string[] refs = new string[referencesList.Items.Count];
            int i = 0;

            errorListView.Items.Clear();
            foreach (string s in referencesList.Items)
                refs[i++] = s;

            if (ModuleLoader.LoadModule(codeEditorView.Text, instanceName.Text, refs) != null)
                MessageBox.Show("Script compiled without errors.");
            else
            {
                ModuleLoader.EnumerateErrors(EnumErrors);
            }
        }

        private void EnumErrors(CompilerError e)
        {
            ListViewItem item = new ListViewItem(e.ErrorNumber, 0);

            item.SubItems.Add(e.Line.ToString());
            item.SubItems.Add(e.Column.ToString());
            item.SubItems.Add(e.ErrorText);

            errorListView.Items.Add(item);
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog.Title = "Open Gear Plug-in...";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                OpenFile(openFileDialog.FileName,false);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (m_SaveFileName != null)
                SaveFile(m_SaveFileName);
            else
                SaveAsButton_Click(sender, e);
        }

        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*";
            saveFileDialog.Title = "Save Gear Plug-in...";

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                SaveFile(saveFileDialog.FileName);
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (referencesList.SelectedIndex != -1)
            {
                referencesList.Items.RemoveAt(referencesList.SelectedIndex);
            }
        }

        private void ErrorView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (errorListView.SelectedIndices.Count < 1)
                return;

            int index = errorListView.SelectedIndices[0];

            ListViewItem lvi = errorListView.Items[index];

            int line = Convert.ToInt32(lvi.SubItems[1].Text)-1;
            int column = Convert.ToInt32(lvi.SubItems[2].Text)-1;

            int i = 0;

            while (line != codeEditorView.GetLineFromCharIndex(i++)) ;

            i += column;

            codeEditorView.SelectionStart = i;
            codeEditorView.ScrollToCaret();
            codeEditorView.Select();

            return;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            referencesList.Items.Add(referenceName.Text);
            referenceName.Text = "";
        }
    }
}