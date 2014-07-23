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
using System.Text.RegularExpressions;

/// @copydoc Gear.GUI
/// 
namespace Gear.GUI
{
    /// @todo Document Gear.GUI.PluginEditor
    public partial class PluginEditor : Form
    {
        /// @brief File name of current plugin on editor window.
        /// @note Include full path and name to the file.
        private string m_SaveFileName;
        /// @brief Default font for editor code.
        /// @version V14.07.03 - Added.
        private Font defaultFont;    
        /// @brief Flag if the plugin definition has changed.
        /// To determine changes, it includes not only the C# code, but also class name and reference list.
        /// @version V14.07.17 - Added.
        private bool m_CodeChanged;
        /// @brief Enable or not change detection event.
        /// @version V14.07.17 - Added.
        private bool changeDetectEnabled;

        /// @brief Default constructor.
        /// Init class, defines columns for error grid, setting changes detection initially.
        public PluginEditor()
        {
            InitializeComponent();
            m_SaveFileName = null;
            changeDetectEnabled = true;
            CodeChanged = false;

            // setting default font
            defaultFont = new Font(FontFamily.GenericMonospace, 10, FontStyle.Regular);
            codeEditorView.Font = defaultFont;
            if (codeEditorView.Font == null)
                codeEditorView.Font = this.Font;

            //Setup error grid
            errorListView.FullRowSelect = true;
            errorListView.GridLines = true;

            errorListView.Columns.Add("Name", -2, HorizontalAlignment.Left);
            errorListView.Columns.Add("Line", -2, HorizontalAlignment.Left);
            errorListView.Columns.Add("Column", -2, HorizontalAlignment.Left);
            errorListView.Columns.Add("Message", -2, HorizontalAlignment.Left);
        }

        /// @brief Return last plugin succesfully loaded o saved.
        /// Handy to remember last plugin directory.
        /// @version V14.07.17 - Added.
        public string GetLastPlugin
        {
            get { return m_SaveFileName; }
        }

        /// @brief Attribute for changed plugin detection.
        /// @version V14.07.17 - Added.
        private bool CodeChanged
        {
            get { return m_CodeChanged; }
            set  
            {
                m_CodeChanged = value;
                UpdateTitle();
            }
        }

        /// @brief Complete Name for plugin, including path.
        /// @version V14.07.17 - Added.
        private string SaveFileName
        {
            get
            {
                if (m_SaveFileName != null)
                    return new FileInfo(m_SaveFileName).Name;
                else return "<New plugin>";
            }
            set
            {
                m_SaveFileName = value;
                UpdateTitle();
            }
        }

        /// @brief Update title window, considering modified state.
        /// Considering name of the plugin and showing modified state, to tell the user if need to save.
        private void UpdateTitle()
        {
            this.Text = ("Plugin Editor: " + SaveFileName +  (CodeChanged ? " *" : ""));
        }

        /// @brief Load a plugin from File.
        /// @todo Correct method to implement new plugin system
        /// 
        /// @note This method take care of update change state of the window. No need to do it 
        /// in callings to it.
        public bool OpenFile(string FileName, bool displayErrors)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            XmlReader tr = XmlReader.Create(FileName, settings);
            bool ReadText = false;

            if (referencesList.Items.Count > 0) 
                referencesList.Items.Clear();   //clear out the reference list
            try
            {

                while (tr.Read())
                {
                    if (tr.NodeType == XmlNodeType.Text && ReadText)
                    {
                        //set or reset font and color
                        codeEditorView.SelectAll();
                        codeEditorView.SelectionFont = this.defaultFont;
                        codeEditorView.SelectionColor = Color.Black;
                        codeEditorView.Text = tr.Value;
                        ReadText = false;
                    }

                    switch (tr.Name.ToLower())
                    {
                        case "reference":
                            if (!tr.IsEmptyElement)     //prevent empty element generates error
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
                CodeChanged = false;

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

        /// @todo Document method PluginEditor.SaveFile()
        /// @todo Correct method to implement new plugin system.
        /// 
        /// Take care of update change state of the window. No need to do it in methods who call this.
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
            CodeChanged = false;    //update modified state for the plugin
        }

        /// @brief Method to compile C# source code to check errors on it.
        /// Actually call a C# compiler to determine errors, using references.
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `EventArgs` class with a list of argument to the event call.
        private void CheckSource_Click(object sender, EventArgs e)
        {
            string[] refs = new string[referencesList.Items.Count];
            int i = 0;

            errorListView.Items.Clear();
            foreach (string s in referencesList.Items)
                refs[i++] = s;

            if (ModuleLoader.LoadModule(codeEditorView.Text, instanceName.Text, refs) != null)
                MessageBox.Show("Script compiled without errors.","Plugin Editor - Check source.");
            else
            {
                ModuleLoader.EnumerateErrors(EnumErrors);
            }
        }

        /// @brief Add error details on screen list.
        /// @param[in] e `CompileError` object.
        private void EnumErrors(CompilerError e)
        {
            ListViewItem item = new ListViewItem(e.ErrorNumber, 0);

            item.SubItems.Add(e.Line.ToString());
            item.SubItems.Add(e.Column.ToString());
            item.SubItems.Add(e.ErrorText);

            errorListView.Items.Add(item);
        }

        /// @brief Show dialog to load a file with plugin information.
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `EventArgs` class with a list of argument to the event call.
        private void OpenButton_Click(object sender, EventArgs e)
        {
            bool continueAnyway = true;
            if (CodeChanged)
            {
                continueAnyway = CloseAnyway(SaveFileName); //ask the user to not lost changes
            }
            if (continueAnyway)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*";
                dialog.Title = "Open Gear Plug-in...";
                if (m_SaveFileName != null)
                    dialog.InitialDirectory = Path.GetDirectoryName(m_SaveFileName);   //retrieve from last plugin edited
                else
                    dialog.InitialDirectory = Path.GetDirectoryName(GearDesktop.LastPlugin);   //retrieve from global last plugin

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    OpenFile(dialog.FileName, false);
                }
            }
        }

        /// @brief Show dialog to save a plugin information into file, using GEAR plugin format.
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `EventArgs` class with a list of argument to the event call.
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (m_SaveFileName != null)
                SaveFile(m_SaveFileName);
            else
                SaveAsButton_Click(sender, e);

            UpdateTitle();   //update title window
        }

        /// @brief Show dialog to save a plugin information into file, using GEAR plugin format.
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `EventArgs` class with a list of argument to the event call.
        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*";
            dialog.Title = "Save Gear Plug-in...";
            if (m_SaveFileName != null)
                dialog.InitialDirectory = Path.GetDirectoryName(m_SaveFileName);   //retrieve from last plugin edited
            else
                dialog.InitialDirectory = Path.GetDirectoryName(GearDesktop.LastPlugin);    //retrieve from global last plugin

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                SaveFile(dialog.FileName);
                UpdateTitle();   //update title window
            }
        }

        /// @brief Remove the selected reference of the list.
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `EventArgs` class with a list of argument to the event call.
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (referencesList.SelectedIndex != -1)
            {
                referencesList.Items.RemoveAt(referencesList.SelectedIndex);
                CodeChanged = true;
            }
        }

        /// @todo Document method PluginEditor.ErrorView_SelectedIndexChanged() 
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `EventArgs` class with a list of argument to the event call.
        private void ErrorView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (errorListView.SelectedIndices.Count < 1)
                return;

            int index = errorListView.SelectedIndices[0];

            ListViewItem lvi = errorListView.Items[index];

            int line = Convert.ToInt32(lvi.SubItems[1].Text) - 1;
            int column = Convert.ToInt32(lvi.SubItems[2].Text) - 1;

            int i = 0;

            while (line != codeEditorView.GetLineFromCharIndex(i++)) ;

            i += column;

            codeEditorView.SelectionStart = i;
            codeEditorView.ScrollToCaret();
            codeEditorView.Select();

            return;
        }

        /// @brief Add a reference from the `ReferenceName`text box.
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `EventArgs` class with a list of argument to the event call.
        private void addReferenceButton_Click(object sender, EventArgs e)
        {
            if (referenceName.Text != null)
            {
                referencesList.Items.Add(referenceName.Text);
                referenceName.Text = "";
                CodeChanged = true;
            }
        }

        /// @brief Check syntax on the C# source code
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `EventArgs` class with a list of argument to the event call.
        /// @since V14.07.03 - Added.
        /// @note Experimental highlighting. Probably changes in the future.
        // Sintax highlighting
        private void syntaxButton_Click(object sender, EventArgs e)
        {
            int restore_pos = codeEditorView.SelectionStart;    //remember last position
            changeDetectEnabled = false;    //not enable change detection
            // Foreach line in input,
            // identify key words and format them when adding to the rich text box.
            Regex r = new Regex("\\n", RegexOptions.Compiled);
            String[] lines = r.Split(codeEditorView.Text);
            codeEditorView.SelectAll();
            codeEditorView.Enabled = false;
            foreach (string l in lines)
            {
                ParseLine(l);
            }
            codeEditorView.SelectionStart = restore_pos;    //restore last position
            codeEditorView.ScrollToCaret();                 //and scroll to it
            codeEditorView.Enabled = true;
            changeDetectEnabled = true; //restore change detection
        }

        /// @brief Auxiliary method to check syntax.
        /// Examines line by line, parsing reserved C# words.
        /// @param[in] line Text line from the source code.
        /// @since V14.07.03 - Added.
        /// @note Experimental highlighting. Probably changes in the future.
        // Parse line for sintax highlighting.
        private void ParseLine(string line)
        {
            Regex r = new Regex("([ \\t{}();:])", RegexOptions.Compiled);
            String[] tokens = r.Split(line);
            System.Drawing.Font fontRegular = this.defaultFont;
            System.Drawing.Font fontBold = new Font(fontRegular, FontStyle.Bold);

            foreach (string token in tokens)
            {
                // Set the token's default color and font.
                codeEditorView.SelectionColor = Color.Black;
                codeEditorView.SelectionFont = fontRegular;

                // Check for a comment.
                if (token == "//" || token.StartsWith("//"))
                {
                    // Find the start of the comment and then extract the whole comment.
                    int index = line.IndexOf("//");
                    string comment = line.Substring(index, line.Length - index);
                    codeEditorView.SelectionColor = Color.Green;
                    codeEditorView.SelectionFont = fontRegular;
                    codeEditorView.SelectedText = comment;
                    break;
                }

                // Check whether the token is a keyword.
                String[] keywords = {
                                        "add", "abstract", "alias", "as", "ascending", "async", "await",
                                        "base", "bool", "break", "byte", "case", "catch", "char", "checked",
                                        "class", "const", "continue", "decimal", "default", "delegate",
                                        "descending", "do", "double", "dynamic", "else", "enum", "event",
                                        "explicit", "extern", "false", "finally", "fixed", "float",
                                        "for", "foreach", "from", "get", "global", "goto", "group", "if",
                                        "implicit", "in", "int", "interface", "internal", "into", "is",
                                        "join", "let", "lock", "long", "namespace", "new", "null", "object",
                                        "operator", "orderby", "out", "override", "params", "partial ",
                                        "private", "protected", "public", "readonly", "ref", "remove",
                                        "return", "sbyte", "sealed", "select", "set", "short", "sizeof",
                                        "stackalloc", "static", "string", "struct", "switch", "this",
                                        "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
                                        "unsafe", "ushort", "using", "value", "var", "virtual", "void",
                                        "volatile", "where", "while", "yield"
                };
                for (int i = 0; i < keywords.Length; i++)
                {
                    if (keywords[i] == token)
                    {
                        // Apply alternative color and font to highlight keyword.
                        codeEditorView.SelectionColor = Color.Blue;
                        codeEditorView.SelectionFont = fontBold;
                        break;
                    }
                }
                codeEditorView.SelectedText = token;
            }
            codeEditorView.SelectedText = "\n";
        }

        /// @brief Update changed state for plugin window.
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `EventArgs` class with a list of argument to the event call.
        /// @version V14.07.17 - Added.
        private void codeEditorView_TextChanged(object sender, EventArgs e)
        {
            if (changeDetectEnabled)
            {
                CodeChanged = true;
            }
        }

        /// @brief Update changed state for instance name.
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `EventArgs` class with a list of argument to the event call.
        /// @version V14.07.17 - Added.
        private void instanceName_TextChanged(object sender, EventArgs e)
        {
            CodeChanged = true;
        }

        /// @brief Event handler for closing plugin window.
        /// If code have changed and it is not saved, a Dialog is presented to the user to proceed or abort 
        /// the closing.
        /// @param[in] sender Object who called this on event.
        /// @param[in] e `FormClosingEventArgs` class with a list of argument to the event call.
        /// @version V14.07.17 - Added.
        private void PluginEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CodeChanged)
            {
                if (!CloseAnyway(SaveFileName)) //ask the user to not loose changes
                    e.Cancel = true;    //cancel the closing event
            }
            GearDesktop.LastPlugin = GetLastPlugin;
        }

        /// @brief Ask the user to not loose changes.
        /// @param fileName Filename to show in dialog
        /// @returns Boolean to close (true) or not (false)
        /// @version V14.07.17 - Added.
        private bool CloseAnyway(string fileName)
        {
            //dialog to not lost changes
            DialogResult confirm = MessageBox.Show(
                "Are you sure to close plugin \"" + fileName + "\" without saving?\nYour changes will lost.",
                "Save.",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Exclamation
            );
            if (confirm == DialogResult.OK)
                return true;
            else
                return false;
        }

    }
}
