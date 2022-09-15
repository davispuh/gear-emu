/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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

using Gear.PluginSupport;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;

// ReSharper disable InconsistentNaming
// ReSharper disable LocalizableElement
// ReSharper disable StringLiteralTypo

namespace Gear.GUI
{
    /// <summary>%Form to edit or create GEAR plugins.</summary>
    public partial class PluginEditor : Form
    {
        /// <summary>Flag if the plugin definition has changed.</summary>
        /// <remarks>To determine changes, it includes not only the C# code,
        /// but also class name and reference list.</remarks>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private bool _codeChanged;

        /// <summary>Enable or not change detection event.</summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private bool _changeDetectEnabled;

        /// <summary>Regex to looking for class name inside the code of
        /// plugin.</summary>
        /// @version v15.03.26 - Added.
        private static readonly Regex ClassNameExpressionRegex = new Regex(
            @"\bclass\s+" +
            @"(?<classname>[@]?[_]*[A-Z|a-z|0-9]+[A-Z|a-z|0-9|_]*)" +
            @"\s*\:\s*PluginBase\b",
            RegexOptions.Compiled);

        /// <summary>Regex for syntax highlight.</summary>
        /// @version v15.03.26 - Added.
        private static readonly Regex LineExpressionRegex = new Regex(
            @"\n",
            RegexOptions.Compiled);

        /// <summary>Regex for parse token in lines for syntax
        /// highlight.</summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private readonly Regex _codeLineRegex = new Regex(
            @"([ \t{}();:])",
            RegexOptions.Compiled);

        /// <summary>keywords to highlight in editor code</summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
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

        /// <summary>Tabulator size for code editor.</summary>
        /// @version v22.06.02 - Added as member to establish data binding
        /// to program properties.
        private uint _tabSize;

        /// <summary>Tabulation array for editor.</summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private readonly int[] _tabs = new int[32];

        /// <summary>Flag to signal if embed the code of a plugin into XML,
        /// or to have it on a separated file.</summary>
        /// @version v22.06.02 - Added as member to establish data binding
        /// to program properties.
        private bool _embeddedCode;

        /// <summary>File name of the plugin.</summary>
        /// <remarks>If it is not saved, its value is empty.</remarks>
        /// @version v22.06.02 - Added.
        private string _pluginFileName;

        /// <summary>Detection of separated file for code.</summary>
        /// @version v20.08.01 - Added.
        private bool SeparatedFileExist { get; set; }

        /// <summary>Return last plugin successfully loaded o saved.</summary>
        /// <remarks>In order this property could be bind with the
        /// corresponding property setting, it must be public.</remarks>
        /// @note Include full path and name to the file.
        /// @version v22.06.02 - Modified visibility to enable data binding
        /// to program properties.
        // ReSharper disable once MemberCanBePrivate.Global
        public string LastPlugin{ get; set; }

        /// <summary>Complete Name for plugin, including path, for
        /// presentation purposes.</summary>
        /// @version v15.03.26 - Added.
        private string PluginFileName
        {
            get => string.IsNullOrEmpty(_pluginFileName) ?
                "<New plugin>" :
                _pluginFileName;
            set
            {
                if (_pluginFileName == value)
                    return;
                _pluginFileName = value;
                UpdateTitles();
            }
        }

        /// <summary>Attribute for changed plugin detection.</summary>
        /// @version v22.04.02 - Changed to private property.
        private bool CodeChanged
        {
            get => _codeChanged;
            set
            {
                if (_codeChanged == value)
                    return;
                _codeChanged = value;
                UpdateTitles();
            }
        }

        /// <summary>Property to establish tabulator size for code editor.</summary>
        /// <remarks>In order this property could be bind with the
        /// corresponding property setting, it must be public.</remarks>
        /// @version v22.06.02 - Added as property to establish data binding
        /// to program properties and reformat editor text with new values.
        // ReSharper disable once MemberCanBePrivate.Global
        public uint TabSize
        {
            get => _tabSize;
            set
            {
                _tabSize = value;
                UpdateTabPositions();
                if (!DesignMode && !_tabs.SequenceEqual(codeEditorView.SelectionTabs))
                    ReformatTextOnTabsUpdated();
            }
        }

        /// <summary>Property to set flag to embed the code of a plugin into XML,
        /// or to have it on a separated file.</summary>
        /// <remarks>In order this property could be bind with the
        /// corresponding property setting, it must be public.</remarks>
        /// @version v22.06.02 - Added as property to establish data binding
        /// to program properties.
        // ReSharper disable once MemberCanBePrivate.Global
        public bool EmbeddedCode
        {
            get => _embeddedCode;
            set
            {
                _embeddedCode = value;
                EmbeddedCodeButton.Checked = _embeddedCode;
                UpdateTextEmbeddedCodeButton();
            }
        }

        /// <summary>Property to get/set the Form font.</summary>
        /// @version v22.06.01 - Added to prevent warning 'Virtual member call
        /// in constructor'.
        public sealed override Font Font
        {
            get => base.Font;
            set => base.Font = value;
        }

        /// <summary>Default constructor.</summary>
        /// <remarks>Initialize the class, defines columns for error grid, setting changes
        /// detection, and trying to load the default template for plugin.</remarks>
        /// <param name="loadDefaultTemplate">Indicate to load default
        /// template (=true) or no template at all(=false).</param>
        /// @version v22.06.02 - Added data bindings of program settings
        /// `EmbeddedCode`, `TabSize`, `LastPlugin` and `UseAnimations` properties.
        public PluginEditor(bool loadDefaultTemplate)
        {
            InitializeComponent();
            //init values
            _pluginFileName = string.Empty;
            _changeDetectEnabled = false;
            CodeChanged = false;
            SeparatedFileExist = false;
            //bonded properties
            TabSize = Properties.Settings.Default.TabSize;
            DataBindings.Add(new Binding("TabSize",
                Properties.Settings.Default, "TabSize",
                false, DataSourceUpdateMode.OnPropertyChanged));
            LastPlugin = Properties.Settings.Default.LastPlugin;
            DataBindings.Add(new Binding("LastPlugin",
                Properties.Settings.Default, "LastPlugin",
                false, DataSourceUpdateMode.OnPropertyChanged));
            EmbeddedCode = Properties.Settings.Default.EmbeddedCode;
            DataBindings.Add(new Binding("EmbeddedCode",
                Properties.Settings.Default, "EmbeddedCode",
                false, DataSourceUpdateMode.OnPropertyChanged));
            // setting default font
            defaultFont = new Font(FontFamily.GenericMonospace, 10, FontStyle.Regular);
            fontBold = new Font(defaultFont, FontStyle.Bold);
            codeEditorView.Font = defaultFont ?? Font;
            //load default plugin template
            if (loadDefaultTemplate)
            {
                try
                {
                    codeEditorView.LoadFile(@"Resources\PluginTemplate.cs",
                        RichTextBoxStreamType.PlainText);
                    CodeChanged = true;
                }
                catch (IOException) { }       //do nothing, maintaining empty the code text box
                catch (ArgumentException) { } //
            }
            _changeDetectEnabled = true;
            CodeChanged = false;
            SeparatedFileExist = false;
            //Setup error grid
            errorListView.FullRowSelect = true;
            errorListView.GridLines = true;
            errorListView.Columns.Add("Code   ", -2, HorizontalAlignment.Left);
            errorListView.Columns.Add("Line", -2, HorizontalAlignment.Right);
            errorListView.Columns.Add("Column", -2, HorizontalAlignment.Right);
            errorListView.Columns.Add("Message", -2, HorizontalAlignment.Left);
            //additional UI init
            progressHighlight.Visible = false;
        }

        /// <summary>Update tab positions, considering default tab size.</summary>
        /// @version v22.06.02 - Changed method name to clarify its meaning,
        /// modified to use new property TabSize, changed method visibility,
        /// and separate logic of reformat in new method
        /// `ReformatTextOnTabsUpdated()`.
        private void UpdateTabPositions()
        {
            // tab width
            for (int i = 0; i < _tabs.Length; i++)
            {
                int size = (i + 1) * (int)TabSize - 1;
                _tabs[i] = TextRenderer.MeasureText(
                    new string(' ', size),
                    defaultFont).Width;
            }
        }

        /// <summary>Reformat editor text with new positions of tab stops.</summary>
        /// @version v22.06.02 - Added to implement reformat text after tab
        /// size changed on program properties.
        private void ReformatTextOnTabsUpdated()
        {
            if (codeEditorView.Text.Length > 0)
            {
                //don't alter code changed status
                _changeDetectEnabled = false;
                //remember text cursor position
                RememberRTBoxPosition checkpoint =
                    new RememberRTBoxPosition(codeEditorView);
                //using temporary file to store contents of editor text
                string tempFile = Path.GetTempFileName();
                codeEditorView.SaveFile(tempFile, RichTextBoxStreamType.PlainText);
                codeEditorView.Clear();
                codeEditorView.SelectionTabs = _tabs;
                //reload to reformat
                codeEditorView.LoadFile(tempFile, RichTextBoxStreamType.PlainText);
                File.Delete(tempFile);
                //restore text cursor position
                checkpoint.RestorePosition();
                _changeDetectEnabled = true;
            }
            else
                codeEditorView.SelectionTabs = _tabs;
        }

        /// <summary>Shows or hide the error grid.</summary>
        /// <param name="enable">Enable (=true) or disable (=False) the error
        /// grid.</param>
        public void ShowErrorGrid(bool enable)
        {
            if (enable)
                errorListView.Show();
            else
                errorListView.Hide();
        }

        /// <summary>Update titles of window and metadata, considering
        /// modified state.</summary>
        /// <remarks>Considering name of the plugin and showing modified state,
        /// to tell the user if need to save.</remarks>
        /// @version v22.06.01 - Using string interpolation.
        private void UpdateTitles()
        {
            Text = $"Plugin Editor: {PluginFileName}{(CodeChanged ? " *" : string.Empty)}";
        }

        /// <summary>Load a plugin from File in Plugin Editor, updating the
        /// screen.</summary>
        /// <param name="fileName">Name of the file to open.</param>
        /// <param name="displayErrors">Flag to show errors in the error
        /// grid.</param>
        /// <returns>Success on load the file on the editor (=true) or
        /// fail (=false).</returns>
        /// @version v22.06.02 - Modified method name to clarify its meaning,
        /// and modified to use new EmbeddedCode property.
        public bool OpenPluginFromFile(string fileName, bool displayErrors)
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Parse,
                ValidationType = ValidationType.DTD
            };
            settings.ValidationEventHandler += DTDValidationErrHandler;
            XmlReader reader = XmlReader.Create(fileName, settings);

            bool readText = false;
            string codeFileName = string.Empty;
            string pluginVersion = "0.1";

            if (referencesList.Items.Count > 0)
                referencesList.Items.Clear();   //clear out the reference list
            try
            {
                while (reader.Read())
                {
                    if (readText)
                    {
                        //set or reset font and color
                        codeEditorView.SelectAll();
                        codeEditorView.SelectionFont = defaultFont;
                        codeEditorView.SelectionColor = Color.Black;
                        if (string.IsNullOrEmpty(codeFileName))
                        {
                            //Maintain compatibility with old plugins (using Text section)
                            if (reader.NodeType == XmlNodeType.Text ||
                                    reader.NodeType == XmlNodeType.CDATA)
                                codeEditorView.Text = reader.Value;
                            EmbeddedCode = true;
                            SeparatedFileExist = false;
                        }
                        else
                        {
                            codeFileName = Path.Combine(Path.GetDirectoryName(fileName) ??
                                                        string.Empty, codeFileName);
                            codeEditorView.Text = File.ReadAllText(codeFileName);
                            EmbeddedCode = false;
                            SeparatedFileExist = true;
                        }
                        codeEditorView.DeselectAll();
                        CodeChanged = false;
                        readText = false;
                    }
                    switch (reader.Name.ToLower())
                    {
                        case "plugin":
                            pluginVersion =
                                string.IsNullOrEmpty(reader.GetAttribute("version")) ?
                                pluginVersion :
                                reader.GetAttribute("version");
                            break;
                        case "reference":
                            if (!reader.IsEmptyElement)
                            {
                                string refer = reader.GetAttribute("name");
                                if (! string.IsNullOrEmpty(refer))
                                    referencesList.Items.Add(refer);
                            }
                            break;
                        case "instance":
                            instanceName.Text = reader.GetAttribute("class");
                            break;
                        case "code":
                            readText = true;
                            codeFileName = reader.GetAttribute("codeFileName");
                            break;
                    }
                }
                //end loading
                reader.Close();
                //refresh & store the plugin name
                PluginFileName = fileName;
                LastPlugin = fileName;
                UpdateDefaultLastPluginOpened();
                //update modified state for the plugin
                CodeChanged = false;
                if (displayErrors)
                {
                    errorListView.Items.Clear();
                    ModuleCompiler.EnumerateErrors(EnumErrors);
                }
                return true;
            }
            catch (IOException ioException)
            {
                MessageBox.Show(this,
                    ioException.Message,
                    "Failed to load plug-in",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return false;
            }
            catch (XmlException xmlException)
            {
                MessageBox.Show(this,
                    xmlException.Message,
                    "Failed to load plug-in",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return false;
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>Show message on DTD validation error.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Validation event data arguments.</param>
        /// @version v22.06.01 - Using string interpolation.
        private static void DTDValidationErrHandler(object sender, ValidationEventArgs e)
        {
            Console.WriteLine($"DTD Validation Error on plugin file: {e.Message}");
        }

        /// <summary>Save a XML file with the plugin information.</summary>
        /// <param name="fileName">Name of the file to save.</param>
        /// @version v22.06.02 - Modified to use new EmbeddedCode property and
        /// changed method visibility.
        private void SavePluginToFile(string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //declaration section
            XmlNode declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(declaration);
            //DTD section
            string internalDTD = File.ReadAllText(@"Resources\Plugin.dtd");
            XmlDocumentType docType = xmlDoc.CreateDocumentType("plugin", null, null,
                internalDTD);
            xmlDoc.AppendChild(docType);
            //plugin section
            XmlElement root = xmlDoc.CreateElement("plugin");
            root.SetAttribute("version", "1.0");
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
            string newName = Path.ChangeExtension(fileName, "cs");
            string codeFileName = Path.GetFileName(newName);
            if (EmbeddedCode)
                instance.AppendChild(
                    xmlDoc.CreateCDataSection(codeEditorView.Text));
            else
                instance.SetAttribute("codeFileName", codeFileName);
            root.AppendChild(instance);
            xmlDoc.Save(fileName);
            if (!EmbeddedCode)
            {
                File.WriteAllText(newName, codeEditorView.Text,
                    new UTF8Encoding(true));
                SeparatedFileExist = true;
            }
            else if (SeparatedFileExist)
            {
                var result = MessageBox.Show(
                    $"An old separate file for code exist: {codeFileName}\r\nIt will be left as orphan. Do you want to remove it?",
                    "Remove old separated code file?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    File.Delete(newName);
                SeparatedFileExist = false;
            }
            //refresh & store the plugin name
            PluginFileName = fileName;
            LastPlugin = fileName;
            UpdateDefaultLastPluginOpened();
            //update modified state for the plugin
            CodeChanged = false;
        }

        /// <summary>Method to compile C# source code to check errors on it.</summary>
        /// <remarks>Actually, call a C# compiler to determine errors,
        /// using references.</remarks>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.01 - Using string interpolation.
        private void CheckSource_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(codeEditorView.Text))
            {
                MessageBox.Show("No source code to check. Please add code.",
                    "Plugin Editor - Check source.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
            else
            {
                if (DetectClassName(codeEditorView.Text, out string className))
                {
                    int i = 0;
                    //show the name found in the screen field
                    instanceName.Text = className;
                    errorListView.Items.Clear();    //clear error list, if any
                    //prepare reference list
                    string[] refs = new string[referencesList.Items.Count];
                    foreach (string s in referencesList.Items)
                        refs[i++] = s;
                    try
                    {
                        PluginBase plugin = ModuleCompiler.LoadModule(
                            codeEditorView.Text,
                            className,
                            refs,
                            null);
                        if (plugin != null)
                        {
                            ShowErrorGrid(false);    //hide the error list
                            MessageBox.Show("Plugin compiled without errors.",
                                "Plugin Editor - Check source.",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            plugin.Dispose();
                        }
                        else
                        {
                            ModuleCompiler.EnumerateErrors(EnumErrors);
                            ShowErrorGrid(true);    //show the error list
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Compile Error: {ex}",
                            "Plugin Editor - Check source.",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                else   //not detected class name
                {
                    instanceName.Text = "Not found!";
                    MessageBox.Show("Cannot detect main plugin class name. Please use \"class <YourPluginName> : PluginBase {...\" declaration on your source code.",
                        "Plugin Editor - Check source.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Add error details on screen list.</summary>
        /// <param name="compilerError">CompilerError object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.09.01 - Changed access from `public`.
        private void EnumErrors(CompilerError compilerError)
        {
            if (compilerError == null)
                throw new ArgumentNullException(nameof(compilerError));
            ListViewItem item = new ListViewItem(compilerError.ErrorNumber, 0);
            item.SubItems.Add(compilerError.Line.ToString());
            item.SubItems.Add(compilerError.Column.ToString());
            item.SubItems.Add(compilerError.ErrorText);
            errorListView.Items.Add(item);
        }

        /// <summary>Show a dialog to load a file with plugin information.</summary>
        /// <remarks>This method checks if the previous plugin data was
        /// modified and not saved.</remarks>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.02 - Corrected errors if no file name was selected
        /// on dialog, but pressed open button, and to show dialog to save
        /// before lose changes in all circumstances.
        private void OpenButton_Click(object sender, EventArgs e)
        {
            //ask the user to not lost changes
            if (CodeChanged && !CloseAnyway(PluginFileName))
                return;
            using (OpenFileDialog dialog = new OpenFileDialog
                   {
                       Filter = "Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*",
                       Title = "Open Gear Plug-in..."
                   })
            {
                //retrieve from last plugin edited
                if (!string.IsNullOrEmpty(LastPlugin))
                    dialog.InitialDirectory = Path.GetDirectoryName(LastPlugin);
                //retrieve from global last plugin
                else if (!string.IsNullOrEmpty(Properties.Settings.Default.LastPlugin))
                    dialog.InitialDirectory =
                        Path.GetDirectoryName(Properties.Settings.Default.LastPlugin);
                //invoke Dialog and open plugin file
                if (dialog.ShowDialog(this) != DialogResult.OK ||
                    string.IsNullOrEmpty(dialog.FileName))
                    return;
                OpenPluginFromFile(dialog.FileName, false);
            }
        }

        /// <summary>Update the default name for last plugin opened or saved in
        /// the editor.</summary>
        /// @version v22.06.02 - Changed method visibility.
        private void UpdateDefaultLastPluginOpened()
        {
            Properties.Settings.Default.LastPlugin = LastPlugin;
            Properties.Settings.Default.Save();
        }

        /// <summary>Show dialog to save a plugin information into file, using
        /// GEAR plugin format.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_pluginFileName))
                SavePluginToFile(PluginFileName);
            else
                SaveAsButton_Click(sender, e);
        }

        /// <summary>Show dialog to save a plugin information into file,
        /// using GEAR plugin format.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Gear plug-in component (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Save Gear Plug-in..."
            };
            //get directory name for init directory for dialog
            if (!string.IsNullOrEmpty(_pluginFileName))
                //retrieve from last plugin edited
                dialog.InitialDirectory = Path.GetDirectoryName(PluginFileName);
            else
                if (!string.IsNullOrEmpty(LastPlugin))
                    //retrieve from global last plugin
                    dialog.InitialDirectory = Path.GetDirectoryName(LastPlugin);
            //propose the detected class name as file name
            dialog.FileName = instanceName.Text;

            if (dialog.ShowDialog(this) != DialogResult.OK ||
                string.IsNullOrEmpty(dialog.FileName))
                return;
            PluginFileName = dialog.FileName;
            SavePluginToFile(PluginFileName);
        }

        /// <summary>Add a reference from the <c>ReferenceName</c> text box.</summary>
        /// <remarks>Also update change state for the plugin module, marking
        /// as changed.</remarks>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void AddReferenceButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(referenceName.Text))
                return;
            referencesList.Items.Add(referenceName.Text);
            referenceName.Text = string.Empty;
            CodeChanged = true;
        }

        /// <summary>Remove the selected reference of the list.</summary>
        /// <remarks>Also update change state for the plugin module, marking
        /// as changed.</remarks>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void RemoveReferenceButton_Click(object sender, EventArgs e)
        {
            if (referencesList.SelectedIndex == -1)
                return;
            referencesList.Items.RemoveAt(referencesList.SelectedIndex);
            CodeChanged = true;
        }

        /// <summary>Position the cursor in code window, corresponding
        /// to selected error row.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        private void ErrorView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (errorListView.SelectedIndices.Count < 1)
                return;
            int i = 0;
            int index = errorListView.SelectedIndices[0];   //determine the current row
            ListViewItem lvi = errorListView.Items[index];
            try
            {
                int line = Convert.ToInt32(lvi.SubItems[1].Text) - 1;
                if (line < 0)
                    line = 0;
                int column = Convert.ToInt32(lvi.SubItems[2].Text) - 1;
                if (column < 0)
                    column = 0;
                while (line != codeEditorView.GetLineFromCharIndex(i++))
                    i += column;
                codeEditorView.SelectionStart = i;
                codeEditorView.ScrollToCaret();
                codeEditorView.Select();
            }
            catch (FormatException) { } //on errors do nothing
        }

        /// <summary>Check syntax on the C# source code.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v14.07.03 - Added.
        private void SyntaxButton_Click(object sender, EventArgs e)
        {
            int pos = 0;
            RememberRTBoxPosition checkpoint = new RememberRTBoxPosition(codeEditorView);
            _changeDetectEnabled = false;    //not enable change detection
            bool commentMultiline = false;  //initially not in comment mode
            //For each line in input, identify key words and format them when
            // adding to the rich text box.
            string[] lines = LineExpressionRegex.Split(codeEditorView.Text);
            //update progress bar
            progressHighlight.Maximum = lines.Length;
            progressHighlight.Value = 0;
            progressHighlight.Visible = true;
            //update editor code
            codeEditorView.Visible = false;
            codeEditorView.SelectAll();
            codeEditorView.Enabled = false;
            foreach (string line in lines)
            {
                progressHighlight.Value = ++pos;
                ParseLine(line, ref commentMultiline);   //remember comment mode between lines
            }
            //update progress bar
            progressHighlight.Visible = false;
            //update editor code
            checkpoint.RestorePosition();
            codeEditorView.Visible = true;
            codeEditorView.Enabled = true;
            _changeDetectEnabled = true; //restore change detection
        }

        /// <summary>Auxiliary method to check syntax.</summary>
        /// <remarks>Examines line by line, parsing reserved C# words.</remarks>
        /// <param name="line">Text line from the source code.</param>
        /// <param name="commentMultiline">Flag to indicate if it is on comment mode
        /// between multi lines (=true) or normal mode (=false).</param>
        /// @version v22.06.01 - Changed from recursive to iterative.
        private void ParseLine(string line, ref bool commentMultiline)
        {
            while (true)
            {
                int index;
                if (commentMultiline)
                {
                    // Check for a c style end comment
                    index = line.IndexOf("*/", StringComparison.Ordinal);
                    if (index != -1) //found end comment in this line?
                    {
                        string comment = line.Substring(0, index += 2);
                        codeEditorView.SelectionColor = Color.Green;
                        codeEditorView.SelectionFont = defaultFont;
                        codeEditorView.SelectedText = comment;
                        //parse the rest of the line (if any)
                        commentMultiline = false;
                        //is there more text after the comment?
                        if (line.Length > index)
                        {
                            line = line.Substring(index);
                            continue;
                        }
                        //no more text, so ...
                        codeEditorView.SelectedText = "\n"; //..finalize the line
                    }
                    else //not end comment in this line..
                    {
                        //.. so all the line will be a comment
                        codeEditorView.SelectionColor = Color.Green;
                        codeEditorView.SelectionFont = defaultFont;
                        codeEditorView.SelectedText = line;
                        codeEditorView.SelectedText = "\n"; //finalize the line
                    }
                }
                else //we are not in comment multi line mode
                {
                    bool putEndLine = true;
                    string[] tokens = _codeLineRegex.Split(line);
                    //parse the line searching tokens
                    foreach (string token in tokens)
                    {
                        // Check for a c style comment opening
                        if (token == "/*" || token.StartsWith("/*"))
                        {
                            index = line.IndexOf("/*", StringComparison.Ordinal);
                            int indexEnd = line.IndexOf("*/", StringComparison.Ordinal);
                            //end comment found in the rest of the line?
                            if (indexEnd != -1 && indexEnd > index)
                            {
                                //extract the comment text
                                string comment = line.Substring(index, (indexEnd += 2) - index);
                                codeEditorView.SelectionColor = Color.Green;
                                codeEditorView.SelectionFont = defaultFont;
                                codeEditorView.SelectedText = comment;
                                //is there more text after the comment?
                                if (line.Length > indexEnd)
                                {
                                    ParseLine(line.Substring(indexEnd), ref commentMultiline);
                                    putEndLine = false;
                                }
                                break;
                            }
                            else //as there is no end comment in the line..
                            {
                                //..entering comment multi line mode
                                commentMultiline = true;
                                string comment = line.Substring(index, line.Length - index);
                                codeEditorView.SelectionColor = Color.Green;
                                codeEditorView.SelectionFont = defaultFont;
                                codeEditorView.SelectedText = comment;
                                break;
                            }
                        }
                        // Check for a c++ style comment.
                        if (token == "//" || token.StartsWith("//"))
                        {
                            // Find the start of the comment and then extract the whole comment.
                            index = line.IndexOf("//", StringComparison.Ordinal);
                            string comment = line.Substring(index, line.Length - index);
                            codeEditorView.SelectionColor = Color.Green;
                            codeEditorView.SelectionFont = defaultFont;
                            codeEditorView.SelectedText = comment;
                            break;
                        }
                        // Set the token's default color and font.
                        codeEditorView.SelectionColor = Color.Black;
                        codeEditorView.SelectionFont = defaultFont;
                        // Check whether the token is a keyword.
                        if (Keywords.Contains(token))
                        {
                            // Apply alternative color and font to highlight keyword.
                            codeEditorView.SelectionColor = Color.Blue;
                            codeEditorView.SelectionFont = fontBold;
                        }
                        codeEditorView.SelectedText = token;
                    }
                    if (putEndLine)
                        codeEditorView.SelectedText = "\n";
                }
                break;
            }
        }

        /// <summary>Update change state for code text box.</summary>
        /// <remarks>It marks as changed, to prevent not averted loses at
        /// closure of the window.</remarks>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v15.03.26 - Added.
        private void CodeEditorView_TextChanged(object sender, EventArgs e)
        {
            if (_changeDetectEnabled)
                CodeChanged = true;
        }

        /// <summary>Detect the plugin class name from the code text given as
        /// parameter.</summary>
        /// <param name="code">Text of the source code of plugin to look for
        /// the class name declaration.</param>
        /// <param name="match">Name of the plugin class found. If not,
        /// it will be null.</param>
        /// <returns>If a match had found =True, else =False.</returns>
        /// @version v22.06.01 - Changed to static access and refactored.
        private static bool DetectClassName(string code, out string match)
        {
            match = null;
            //Look for a 'suspect' for class definition to show it to user later.
            Match suspect = ClassNameExpressionRegex.Match(code);
            if (!suspect.Success)
                return false;
            //detect class name from the detected groups
            string name = suspect.Groups["classname"].Value;
            if (string.IsNullOrEmpty(name))
                return false;
            match = name;
            return true;
        }

        /// <summary>Event handler for closing plugin window.</summary>
        /// <remarks>If code, references or class name have changed and them
        /// are not saved, a dialog is presented to the user to proceed or
        /// abort the closing.</remarks>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v15.03.26 - Added.
        private void PluginEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CodeChanged && !CloseAnyway(PluginFileName)) //ask the user to not lose changes
                e.Cancel = true;    //cancel the closing event
        }

        /// <summary>Ask the user to not loose changes.</summary>
        /// <param name="fileName">Filename to show in dialog.</param>
        /// <returns>True to close or False to stay open.</returns>
        /// @version v22.06.01 - Changed to static access and interpolate strings.
        private static bool CloseAnyway(string fileName)
        {
            //dialog to not lost changes
            DialogResult confirm = MessageBox.Show(
                $"Are you sure to close plugin \"{fileName}\" without saving?\nYour changes will lost.",
                "Save.",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button2
            );
            return confirm == DialogResult.OK;
        }

        /// <summary>Toggle the button state, updating the name & tooltip text.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.02 - Modified to use new EmbeddedCode property and
        /// changed method visibility.
        private void EmbeddedCode_Click(object sender, EventArgs e)
        {
            EmbeddedCode = !EmbeddedCode;
        }

        /// <summary>Update the name & tooltip text depending on state.</summary>
        /// @version v22.06.02 - modified to use new name of Embedded code button.
        private void UpdateTextEmbeddedCodeButton()
        {
            if (EmbeddedCodeButton.Checked)
            {
                EmbeddedCodeButton.Text = "Embedded";
                EmbeddedCodeButton.ToolTipText = "Embedded code in XML plugin file.";
                EmbeddedCodeButton.Image = Properties.Resources.Image_embedded;
            }
            else
            {
                EmbeddedCodeButton.Text = "Separated";
                EmbeddedCodeButton.ToolTipText = "Code in separated file from XML plugin file.";
                EmbeddedCodeButton.Image = Properties.Resources.Image_separated;
            }
        }

        /// <summary>On visible property changed, perform layout on tool strip.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v20.08.01 - Added.
        private void ProgressHighlight_VisibleChanged(object sender, EventArgs e)
        {
            toolStripMain.PerformLayout();
            toolStripMain.Refresh();
        }

        /// <summary>Refresh form's Icon.</summary>
        /// <param name="sender">Reference to object where event was raised.</param>
        /// <param name="e">Event data arguments.</param>
        /// @version v20.10.01 - Added.
        private void PluginEditor_Load(object sender, EventArgs e)
        {
            //workaround of bug on MDI Form (https://stackoverflow.com/a/6701490/10200101)
            Icon = Icon.Clone() as Icon;
        }
    }
}
