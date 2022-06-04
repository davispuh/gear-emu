/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * Emulator.cs
 * View class for PropellerCPU emulator instance
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

using Gear.EmulationCore;
using Gear.PluginSupport;
using Gear.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

// ReSharper disable LocalizableElement
namespace Gear.GUI
{
    /// @brief View class for PropellerCPU emulator instance.
    /// @details This class implements a view over a propeller emulator, with interface to control
    /// the chip, like start, go through steps, reset or reload.
    public partial class Emulator : Form
    {
        /// <summary>Reference to PropellerCPU running instance.</summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private readonly PropellerCPU _cpuHost;
        /// <summary>Name of Binary program loaded.</summary>
        /// @version v22.06.01 - Name changed to follow naming conventions and to clarify its meaning.
        private string _sourceFileName;
        /// <summary>How many steps to update screen.</summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private uint _stepInterval;
        /// <summary>List of floating controls.</summary>
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private readonly List<Control> _floatControls;
        /// <summary>Manager to maintain documents tabs ordered.</summary>
        /// @version v22.06.01 - Added.
        private readonly TabManager _docsManager;

        /// @brief Stopwatch to periodically rerun a step of the emulation
        /// @version v22.06.01 - Name changed to follow naming conventions.
        private readonly Timer _runTimer;

        /// @brief Get if emulator is in running state.
        /// @version v20.10.01 - Added.
        private bool IsRunningState => _runTimer.Enabled;

        /// @brief Get the last binary opened successfully.
        public string LastBinary => _sourceFileName;

        /// @brief Text of the base %Form.
        /// @version v22.06.01 - Added to prevent warning 'Virtual member call in constructor'.
        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = $"Propeller: {value}";
        }

        /// @brief Default Constructor.
        /// @param sourceFileName Binary program loaded (path & name)
        /// @version v22.06.01 Maintain tabs order: Added criteria for documents tab name generation for system plugins.
        /// @todo [Enhancement] Remove run timer and replace with a new runner in a different thread.
        public Emulator(string sourceFileName)
        {
            _cpuHost = new PropellerCPU(this);
            _sourceFileName = sourceFileName;
            _floatControls = new List<Control>();
            InitializeComponent();
            _docsManager = new TabManager(documentsTab);
            hubView.SetHost(_cpuHost);
            hubView.SetFontSpecialLabels();
            Text = _sourceFileName;
            // Create default layout
            for (int i = 0; i < PropellerCPU.TotalCogs; i++)
                AttachPlugin(new CogView(i, _cpuHost), TabManager.NumericZeroBased);
            AttachPlugin(new MemoryView(_cpuHost), TabManager.OnlyRepetitionNumberedFromOne);
            AttachPlugin(new SpinView(_cpuHost), TabManager.OnlyRepetitionNumberedFromOne);
            AttachPlugin(new LogicProbe.LogicView(_cpuHost), TabManager.OnlyRepetitionNumberedFromOne);
            documentsTab.SelectedIndex = 0;
            // TODO REMOVE TEMPORARY RUN FUNCTION
            _runTimer = new Timer
            {
                Interval = 10
            };
            _runTimer.Tick += RunEmulatorStep;
            UpdateRunningButtons();
            UpdateStepInterval();
        }

        /// @brief Update value of system properties inside of
        /// contained controls.
        /// @version v22.06.01 - Added updating UseAnimations property on
        /// CollapsibleSplitter controls.
        public void UpdateVarValue(string variableName)
        {
            switch (variableName)
            {
                case "LastTickMarkGrid":
                    foreach (TabPage tabCtl in documentsTab.TabPages)
                        foreach (Control ctl in tabCtl.Controls)
                            if (ctl is LogicProbe.LogicView logic)
                            {
                                logic.UpdateLastTickMarkGrid();
                                logic.UpdateFrameAndTickText();
                            }
                    break;
                case "LastTimeFrame":
                    foreach (TabPage tabCtl in documentsTab.TabPages)
                        foreach (Control ctl in tabCtl.Controls)
                            if (ctl is LogicProbe.LogicView logic)
                            {
                                logic.UpdateLastTimeFrame();
                                logic.UpdateFrameAndTickText();
                            }
                    break;
                case "LogicViewTimeUnit":
                    foreach (TabPage tabCtl in documentsTab.TabPages)
                        foreach (Control ctl in tabCtl.Controls)
                            if (ctl is LogicProbe.LogicView logic)
                                logic.UpdateTimeUnit();
                    break;
                case "FreqFormat":
                    //update hubView
                    hubView.UpdateFreqFormat();
                    hubView.UpdateCounterText();
                    hubView.UpdateTimeText();
                    hubView.UpdateFrequenciesTexts();
                    //update SpinView and logicView
                    foreach (TabPage tabCtl in documentsTab.TabPages)
                        foreach (Control ctl in tabCtl.Controls)
                            if (ctl is SpinView viewCtl)
                                viewCtl.UpdateFreqFormat();
                    break;
                case "HubTimeUnit":
                    hubView.UpdateTimeText();
                    hubView.UpdateHubTimeUnit();
                    break;
                case "UpdateEachSteps":
                    UpdateStepInterval();
                    break;
                case "UseAnimations":
                    UpdateUseAnimation();
                    break;
            }
        }

        /// @brief Update step interval from default value.
        /// @version v20.09.01 - Added.
        private void UpdateStepInterval()
        {
            _stepInterval = Properties.Settings.Default.UpdateEachSteps;
        }

        /// <summary>Update attribute use animation of each
        /// CollapsibleSplitter used in this form.</summary>
        /// @version v22.06.01 - Added.
        public void UpdateUseAnimation()
        {
            hubViewSplitter.UseAnimations = Properties.Settings.Default.UseAnimations;
            pinnedSplitter.UseAnimations = Properties.Settings.Default.UseAnimations;
        }

        /// <summary>Include a plugin to a propeller chip instance and insert
        /// into documents tab.</summary>
        /// <remarks>Attach a plugin, linking the propeller instance to the plugin,
        /// opening a new tab window and enabling the close button by plugin isClosable
        /// property.</remarks>
        /// <param name="newPlugin">Instance of a Gear.PluginSupport.PluginBase
        /// class to be attached.</param>
        /// <param name="generator"></param>
        /// @version v22.06.01 - Maintain tabs order: insert tab just created in document tabs. Changed parameter name to clarify its meaning. Added new parameter to generate numbering on duplicated tabs.
        private void AttachPlugin(PluginBase newPlugin, TabManager.QuantityPostFixGenerator generator)
        {
            //include into plugin lists of a PropellerCPU instance
            _cpuHost.IncludePlugin(newPlugin);
            //invoke initial setup of plugin
            newPlugin.PresentChip();
            //add to documents tabs
            _docsManager.AddToTabControl(newPlugin, generator);
            //Maintain the close button availability
            closeButton.Enabled = newPlugin.IsClosable;
        }

        /// @brief Delete a plugin from a propeller chip instance.
        /// @details Delete a plugin from the actives plugins of the propeller instance,
        /// effectively stopping the plugin. Remove also from pins and clock watch list.
        /// @param plugin Instance of a Gear.PluginSupport.PluginCommon class to be detached.
        /// @version v15.03.26 - Added.
        private void DetachPlugin(PluginBase plugin)
        {
            if (plugin.IsClosable)      //check if the plugin is able to close, then remove...
            {
                _cpuHost.RemoveOnPins(plugin);  //from pins watch list
                _cpuHost.RemoveOnClock(plugin); //from clock watch list
                _cpuHost.RemovePlugin(plugin);  //from the plugins registered to the propeller emulator
            }
        }

        /// @brief Run the emulator updating the screen between a number of steps.
        /// @details The program property "UpdateEachSteps" gives the number of steps before
        /// screen repaint.
        ///
        /// Adjusting this number in configuration (like increasing the number) enable to obtain
        /// faster execution at expense of less screen responsiveness.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v22.06.01 - Fixed problem of enable/disable buttons get incoherent when the emulator arrives to a breakpoint.
        private void RunEmulatorStep(object sender, EventArgs e)
        {
            for (uint i = 0; i < _stepInterval; i++)
                if (!_cpuHost.Step())
                {
                    _runTimer.Stop();
                    break;
                }
            UpdateRunningButtons();  //bugfix correction
            RepaintViews();
        }

        /// @brief Update Text and Images of buttons involved on running and
        /// stop state of emulator.
        /// @version v22.04.02 - Incorporated step Instruction and step Clock
        /// buttons to be controlled.
        private void UpdateRunningButtons()
        {
            if (IsRunningState == runEmulatorButton.Checked)
                return;
            runEmulatorButton.Checked = IsRunningState;
            if (IsRunningState)
            {
                runEmulatorButton.Text = "Run";
                runEmulatorButton.Image = Properties.Resources.Image_runStatus;
                stopEmulatorButton.Text = "&Stop";
                stopEmulatorButton.Image = Properties.Resources.Image_stopStill;
                stepInstructionButton.Text = "Step Instruction";
                stepInstructionButton.Enabled = false;
                stepClockButton.Enabled = false;
            }
            else
            {
                runEmulatorButton.Text = "&Run";
                runEmulatorButton.Image = Properties.Resources.Image_runStill;
                stopEmulatorButton.Text = "Stop";
                stopEmulatorButton.Image = Properties.Resources.Image_stopStatus;
                stepInstructionButton.Text = "&Step Instruction";
                stepInstructionButton.Enabled = true;
                stepClockButton.Enabled = true;
            }
        }

        /// @brief Load a binary image from file.
        /// @details Generate a new instance of a `PropellerCPU` and load
        /// the program from the binary.
        /// @version v22.06.01 - Parameter name changed to follow naming conventions.
        public bool OpenFile(string fileName)
        {
            try
            {
                _cpuHost.Initialize(File.ReadAllBytes(fileName));
                _sourceFileName = fileName;
                Text = fileName;
                Properties.Settings.Default.LastBinary = _sourceFileName;
                Properties.Settings.Default.Save();
                UpdateRunningButtons();
                RepaintViews();
                return true;
            }
            catch (IOException ioe)
            {
                MessageBox.Show(this,
                    ioe.Message,
                    "IO Error - Failed to load program binary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            catch (BinarySizeException e)
            {
                MessageBox.Show(this,
                    e.Message,
                    "Failed to load program binary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        /// @brief Load a plugin from XML file.
        /// @details Try to open the XML definition for the plugin from the file name given as
        /// parameter. Then extract information from the XML (class name, auxiliary references
        /// and source code to compile), trying to compile the C# source code (based on
        /// Gear.PluginSupport.PluginBase class) and returning the new class instance. If the
        /// compilation fails, then it opens the plugin editor to show errors and source code.
        /// @param fileName Name and path to the XML plugin file to open
        /// @returns Reference to the new plugin instance (on success) or NULL (on fail).
        /// @version v22.06.01 - Parameter name and local variable name changed to follow naming conventions.
        /// @todo [Enhance] show dialog to inform user the compilation error and plugin open in editor instead of loading it
        /// @todo [Bug] Manage all possible exceptions thrown at plugin load in Emulator
        public PluginBase LoadPlugin(string fileName)
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Parse,
                ValidationType = ValidationType.DTD
            };
            XmlReader reader = XmlReader.Create(fileName, settings);
            bool readText = false;
            List<string> references = new List<string>();
            string instanceName = string.Empty;
            string code = string.Empty;
            string codeFileName = string.Empty;
            string pluginVersion = "0.1";
            try
            {
                while (reader.Read())
                {
                    if (readText)
                    {
                        if (string.IsNullOrEmpty(codeFileName))
                        {
                            //Maintain compatibility with old plugins (using Text section)
                            if (reader.NodeType == XmlNodeType.Text ||
                                    reader.NodeType == XmlNodeType.CDATA)
                                code = reader.Value;
                        }
                        else
                        {
                            codeFileName = Path.Combine(
                                Path.GetDirectoryName(fileName) ?? string.Empty,
                                codeFileName);
                            code = File.ReadAllText(codeFileName);
                        }
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
                                references.Add(reader.GetAttribute("name"));
                            break;
                        case "instance":
                            instanceName = reader.GetAttribute("class");
                            break;
                        case "code":
                            readText = true;
                            codeFileName = reader.GetAttribute("codeFileName");
                            break;
                    }
                }
                //Dynamic load and compile the plugin module as a class, giving the chip
                // instance as a parameter.
                PluginBase plugin = ModuleCompiler.LoadModule(
                    code,
                    instanceName,
                    references.ToArray(),
                    _cpuHost
                );
                if (plugin == null)  //if it fails...
                {
                    // ...open plugin editor in other window
                    PluginEditor pe = new PluginEditor(false);
                    pe.OpenFile(fileName, true);
                    pe.MdiParent = MdiParent;
                    pe.Show();
                    //the compilation errors are displayed in the error grid
                    ModuleCompiler.EnumerateErrors(pe.EnumErrors);
                    //show the error list
                    pe.ShowErrorGrid(true);
                    //TODO show dialog to inform user the compilation error and plugin open in editor instead of loading it
                }
                else  //successful compiling & instantiating of the new class
                {
                    //update location of last plugin
                    Properties.Settings.Default.LastPlugin = fileName;
                    Properties.Settings.Default.Save();
                }
                return plugin;
            }
            // TODO  Analyze all possible exceptions thrown inside and manage them appropriately
            catch (Exception ex)
            {
                if (ex is IOException | ex is XmlException)
                {
                    MessageBox.Show(this,
                        ex.Message,
                        "Failed to load program binary",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    return null;
                }
                else throw;
            }
            finally
            {
                reader.Close();
            }
        }

        /// @brief Select binary propeller image to load.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v22.03.02 - Remember last binary open from Settings.
        private void OpenBinary_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog
                   {
                       Filter = "Propeller Runtime Image (*.binary;*.eeprom)|*.binary;" +
                                "*.eeprom|All Files (*.*)|*.*",
                       Title = "Open Propeller Binary...",
                       FileName = _sourceFileName
                   })
            {
                //retrieve last binary location
                if (!string.IsNullOrEmpty(Properties.Settings.Default.LastBinary))
                    openFileDialog.InitialDirectory =
                        Path.GetDirectoryName(Properties.Settings.Default.LastBinary);
                //invoke Dialog
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                    OpenFile(openFileDialog.FileName);
            }
        }

        /// @brief Event to reload the whole %Propeller program from binary file.
        /// @details It also reset the %Propeller Chip and all the plugins.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v20.10.01 - UpdateRunningButtons.
        private void ReloadBinary_Click(object sender, EventArgs e)
        {
            OpenFile(_sourceFileName);
            _cpuHost.Reset();
            UpdateRunningButtons();
            RepaintViews();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e">Event data arguments.</param>
        /// @version v22.06.01 - Local variable name changed to clarify it meaning.
        protected override void OnClosed(EventArgs e)
        {
            foreach (Control control in _floatControls)
                control.Parent = null;
            _floatControls.Clear();
            base.OnClosed(e);
        }

        /// @brief Repaint the Views, including float windows.
        /// @version v22.06.01 - Corrected bug on float windows not were updated.
        /// @todo Parallelism [complex:medium, cycles:typ1] point in loop of list of _floatControls
        private void RepaintViews()
        {
            foreach (Control floatControl in _floatControls)  // TODO Parallelism [complex:medium, cycles:typ1] point in loop _floatControls
                foreach (Control control in floatControl.Controls)
                    if (control is PluginBase pluginControl)
                        pluginControl.Repaint(true);
                    else
                        control.Refresh();
            Control pinnedControl = pinnedPanel.GetNextControl(pinnedPanel, true);
            ((PluginBase)pinnedControl)?.Repaint(true);
            if (documentsTab.SelectedTab != null &&
                (pinnedControl = documentsTab.SelectedTab.GetNextControl(documentsTab.SelectedTab, true)) != null)
                ((PluginBase)pinnedControl).Repaint(true);
            hubView.DataChanged();
        }

        /// @brief Event to reset the whole %Propeller Chip.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v20.10.01 - Update running buttons.
        private void ResetEmulator_Click(object sender, EventArgs e)
        {
            _cpuHost.Reset();
            UpdateRunningButtons();
            RepaintViews();
        }

        /// @brief Send the active tab to a floating window.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        /// @version v22.06.01 - Refactored to maintain tabs ordered.
        private void FloatActiveTab_Click(object sender, EventArgs e)
        {
            TabPage selectedTab = documentsTab.SelectedTab;
            //get plugin control
            Control controlToFloat = null;
            foreach (Control control in selectedTab.Controls)
                if (control is PluginBase)
                    controlToFloat = control;
            if (controlToFloat == null)
                return;
            if (documentsTab.SelectedIndex > 0)
            {
                //select the previous tab
                documentsTab.SelectedIndex -= 1;
                //tab changing housekeeping for plugin close button
                DocumentsTab_Click(this, e);
            }
            selectedTab.Parent = null;
            FloatedWindow floatedWindow = new FloatedWindow(this, controlToFloat);
            controlToFloat.Parent = floatedWindow;
            controlToFloat.Dock = DockStyle.Fill;
            controlToFloat.Text = selectedTab.Text;
            floatedWindow.MdiParent = MdiParent;
            floatedWindow.Text = $"{selectedTab.Text}: {_sourceFileName}";
            floatedWindow.Show();
            _floatControls.Add(floatedWindow);
            _docsManager.FloatingTabsQuantity = _floatControls.Count;
        }

        /// @brief Unfloat the control, opening in a new tab.
        /// @param control Control object to move.
        /// @throws ArgumentNullException
        /// @version v22.06.01 - Changed method name from `Unfloat` and Refactored to maintain documents tabs order.
        public void UnFloatCtrl(Control control)
        {
            if (control is null)
                throw new ArgumentNullException(nameof(control));
            Control lastParent = control.Parent;
            //reinsert tab in saved position
            _docsManager.RestoreToTabControl(control);
            //tab changing housekeeping for plugin close button
            DocumentsTab_Click(this, EventArgs.Empty);
            _floatControls.Remove(lastParent);
            _docsManager.FloatingTabsQuantity = _floatControls.Count;
        }

        /// @brief Send the active tab to pin panel.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        /// @version v22.06.01 - Refactored to maintain tabs ordered.
        private void PinActiveTab_Click(object sender, EventArgs e)
        {
            Control oldPinnedControl = pinnedPanel.GetNextControl(pinnedPanel, true);
            TabPage selectedTab = documentsTab.SelectedTab;
            Control newPinControl = selectedTab?.GetNextControl(selectedTab, true);
            if (newPinControl == null)
                return;
            if (documentsTab.SelectedIndex > 0)
            {
                //select the previous tab
                documentsTab.SelectedIndex -= 1;
                //tab changing housekeeping for plugin close button
                DocumentsTab_Click(this, e);
            }
            selectedTab.Parent = null;
            if (pinnedSplitter.IsCollapsed)
                pinnedSplitter.ToggleState();
            newPinControl.Parent = pinnedPanel;
            newPinControl.Dock = DockStyle.Fill;
            newPinControl.Text = selectedTab.Text;
            _docsManager.TabPinnedExist = true;
            //restore old pinned control to tab pages, if any
            if (oldPinnedControl == null)
                return;
            //reinsert tab in saved position
            _docsManager.RestoreToTabControl(oldPinnedControl);
            //tab changing housekeeping for plugin close button
            DocumentsTab_Click(this, EventArgs.Empty);
        }

        /// @brief Unpin a view, attaching to a tab.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        /// @version v22.06.01 - Refactored to maintain documents tabs order.
        private void UnpinButton_Click(object sender, EventArgs e)
        {
            Control oldPinnedControl = pinnedPanel.GetNextControl(pinnedPanel, true);
            if (oldPinnedControl == null)
                return;
            //reinsert tab in saved position
            _docsManager.RestoreToTabControl(oldPinnedControl);
            //tab changing housekeeping for plugin close button
            DocumentsTab_Click(this, EventArgs.Empty);
            if (!pinnedSplitter.IsCollapsed)
                pinnedSplitter.ToggleState();
            _docsManager.TabPinnedExist = false;
        }

        /// @brief Event to run the emulator freely.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        /// @version v20.10.01 - UpdateRunningButtons.
        private void RunEmulator_Click(object sender, EventArgs e)
        {
            _runTimer.Start();
            UpdateRunningButtons();
        }

        /// @brief Stop the emulation.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        /// @version v20.10.01 - UpdateRunningButtons.
        private void StopEmulator_Click(object sender, EventArgs e)
        {
            _runTimer.Stop();
            UpdateRunningButtons();
            RepaintViews(); //added the repaint, to refresh the views
        }

        /// @brief Run one clock tick of the active cog, stopping after executed.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v22.04.02 - Corrected method name to clarify its meaning.
        private void StepClock_Click(object sender, EventArgs e)
        {
            _cpuHost.Step();
            UpdateRunningButtons();
            RepaintViews();
        }

        /// @brief Event to run one instruction in emulator, stopping after executed.
        /// @details Only makes sense to run a step  if a cog is selected.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        /// @version v22.06.01 - Refactored to improve detection.
        /// @todo Review visibility of step button when not a CogView is active
        private void StepInstruction_Click(object sender, EventArgs e)
        {
            Control control = documentsTab.SelectedTab?.GetNextControl(documentsTab.SelectedTab, true);
            switch (control)
            {
                case null:
                    return;
                case CogView view:
                {
                    Cog cog = view.GetViewCog();
                    cog?.StepInstruction();
                    break;
                }
            }
            UpdateRunningButtons();
            RepaintViews();
        }

        /// @brief Make a stop on the emulation, when a breakpoint is requested by a plugin.
        /// @details This method would be called when a plugin want to stop, for example
        /// when a breakpoint condition is satisfied.
        /// @version v20.10.01 - UpdateRunningButtons.
        public void BreakPoint()
        {
            _runTimer.Stop();
            UpdateRunningButtons();
            RepaintViews();
        }

        /// @brief Try to open a plugin, compiling it and attaching to the active
        /// emulator instance.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        /// @version v22.06.01 - Code for attach plugin to emulator moved here.
        private void OpenPlugin_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Gear Plug-in (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Open Gear Plug-in..."
            };
            //get the path of last plugin like a hint to open a new one
            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastPlugin))
                dialog.InitialDirectory =
                    Path.GetDirectoryName(Properties.Settings.Default.LastPlugin);
            //ask the user what plugin file to open
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                PluginBase newPlugin = LoadPlugin(dialog.FileName);
                if (newPlugin != null)
                    AttachPlugin(newPlugin, TabManager.OnlyRepetitionNumberedFromOne);
            }
        }

        /// @brief Event when the Emulator windows begin to close.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        private void Emulator_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cpuHost.OnClose(sender, e);
        }

        /// @brief Event when emulator goes out of focus.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        /// @version v20.10.01 - UpdateRunningButtons.
        private void OnDeactivate(object sender, EventArgs e)
        {
            _runTimer.Stop();
            UpdateRunningButtons();
        }

        /// @brief Close the plugin window and terminate the plugin instance.
        /// @details Not only close the tab window, also detach the plugin
        /// from the PropellerCPU what uses it.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v22.06.01 - Refactored to maintain tabs order: remove tab associated to plugin from documents tabs.
        private void CloseActiveTab_Click(object sender, EventArgs e)
        {
            //get selected tab object
            TabPage selectedTab = documentsTab.SelectedTab;
            if (selectedTab == null)
                return;
            //get plugin
            PluginBase pluginAssociated = null;
            foreach (Control ctl in selectedTab.Controls)
            {
                pluginAssociated = ctl as PluginBase;
                if (pluginAssociated == null)
                    continue;
                if (!pluginAssociated.IsClosable)
                    return;
                break;
            }
            if (pluginAssociated == null) //if none of controls is a PluginBase
                return;
            if (documentsTab.SelectedIndex > 0)
            {
                //select the previous tab
                documentsTab.SelectedIndex -= 1;
                //tab changing housekeeping for plugin close button
                DocumentsTab_Click(this, e);
            }
            //remove objects from UI
            _docsManager.RemoveFromTabControl(selectedTab, pluginAssociated);
            selectedTab.Dispose();
            //detach the plugin from the emulator
            DetachPlugin(pluginAssociated);
            pluginAssociated.Dispose();
        }

        /// @brief Refresh tab page.
        /// @details Enable close plugin button based on if active tab is subclass of
        /// Gear.PluginSupport.PluginBase and if that class permit close the window. Typically
        /// the user plugins enabled it; but the cog window, main memory, logic probe, etc,
        /// don't allow to close.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v22.06.01 - Refactored to improve logic.
        private void DocumentsTab_Click(object sender, EventArgs e)
        {
            TabPage selectedTab = documentsTab.SelectedTab;
            if (selectedTab == null)
                return;
            foreach (Control control in selectedTab.Controls)
                if (control is PluginBase pluginControl)
                {
                    closeButton.Enabled = pluginControl.IsClosable;
                    pluginControl.Repaint(false);
                }
                else
                    closeButton.Enabled = false;
            selectedTab.Invalidate();
        }

        /// @brief Process key press to manage the run state of emulator.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        /// @version v22.06.01 - Refactored to improve logic.
        /// @todo [enhance:] More feedback: Add feedback of hot key ignored.
        private void DocumentsTab_KeyPress(object sender, KeyPressEventArgs e)
        {
            TabPage tp = documentsTab.SelectedTab;
            if (tp == null)
                return;
            foreach (Control control in tp.Controls)
                if (control is PluginBase pluginControl && !pluginControl.AllowHotKeys)
                    return;
            switch (e.KeyChar.ToString().ToLowerInvariant())
            {
                case "s":
                    if (IsRunningState)
                    {
                        _runTimer.Stop();
                        UpdateRunningButtons();
                    }
                    else
                        StepInstruction_Click(this, EventArgs.Empty);
                    break;
                case "r":
                    if (!IsRunningState)
                    {
                        _runTimer.Start();
                        UpdateRunningButtons();
                    }
                    //else { }  //TODO [enhance:] More feedback: Add feedback of hot key ignored
                    break;
                case "c":
                    if (!IsRunningState)
                        StepClock_Click(this, EventArgs.Empty);
                    //else { }  //TODO [enhance:] More feedback: Add feedback of hot key ignored
                    break;
            }
        }

        /// @brief Refresh form's Icon.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the event details.
        /// @version v20.10.01 - Added.
        private void Emulator_Load(object sender, EventArgs e)
        {
            //workaround of bug on MDI Form (https://stackoverflow.com/a/6701490/10200101)
            Icon = Icon.Clone() as Icon;
        }
    }

}

// Reference link to MSCGEN: http://www.mcternan.me.uk/mscgen/
// Reference link to DOXYGEN commands: https://www.doxygen.nl/manual/commands.html
//
/// @defgroup PluginDetails Plugin Loading Details
///

/// @ingroup PluginDetails
/// @page PluginLoadingSequencePage Loading Sequence for a Plugin.
/// @par Main Sequence.
/// Sequence of plugin loading, since the user presses the button in the emulator window (ideal
/// flow case).
/// @anchor PluginLoadingSequenceFig1
/// @par
/// @mscfile "Load plugin Callings-fig1.mcsgen" "Fig.1: Main sequence for a Plugin loading."
/// @par Detail for Registering OnPinChange & OnClock Methods.
/// This is a detail of main sequence of
/// @ref PluginLoadingSequenceFig1 "\"Fig.1: Main sequence for a Plugin loading.\"", to show
/// the possible flows of invocations when the program calls the Method `PresentChip()`, but not
/// from PluginBase; is the method defined in the plugin class derived by the loaded & compiled
/// plugin class. So the plugin programmer could choose to call or not either `OnClock()` and
/// `OnPinChange()` derived methods.
/// @anchor PluginLoadingSequenceFig2
/// @par
/// @mscfile "Load plugin Callings-fig2.mcsgen" "Fig.2: details of invocation for Plugin members."
///
