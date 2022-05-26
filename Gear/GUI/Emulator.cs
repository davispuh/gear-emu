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

namespace Gear.GUI
{
    /// @brief View class for PropellerCPU emulator instance.
    /// @details This class implements a view over a propeller emulator, with interface to control
    /// the chip, like start, go through steps, reset or reload.
    public partial class Emulator : Form
    {
        private readonly PropellerCPU Chip;          //!< @brief Reference to PropellerCPU running instance.
        private string source;                       //!< @brief Name of Binary program loaded.
        private uint stepInterval;                   //!< @brief How many steps to update screen.
        private readonly List<Control> FloatControls;//!< @brief List of floating controls.

        /// @brief Stopwatch to periodically rerun a step of the emulation
        private readonly Timer runTimer;

        /// @brief Get if emulator is in running state.
        /// @version v20.10.01 - Added.
        private bool IsRunningState { get => runTimer.Enabled; }

        /// @brief Get the last binary opened successfully.
        public string LastBinary { get => source; }

        /// @brief Default Constructor.
        /// @param _source Binary program loaded (path & name)
        public Emulator(string _source)
        {
            Chip = new PropellerCPU(this);
            source = _source;
            FloatControls = new List<Control>();

            InitializeComponent();

            hubView.SetFontSpecialLabels();

            Text = "Propeller: " + source;

            // Create default layout
            for (int i = 0; i < PropellerCPU.TotalCogs; i++)  //using constant TOTAL_COGS
                AttachPlugin(new CogView(i, Chip));

            AttachPlugin(new MemoryView(Chip));
            AttachPlugin(new SpinView(Chip));
            AttachPlugin(new LogicProbe.LogicView(Chip));   //changed to LogicProbe be the last tab
            documentsTab.SelectedIndex = 0;

            // TEMPORARY RUN FUNCTION
            runTimer = new Timer
            {
                Interval = 10
            };
            runTimer.Tick += new EventHandler(RunEmulatorStep);
            UpdateRunningButtons();

            hubView.SetHost(Chip);
            UpdateStepInterval();
        }

        /// @brief Update value of system properties inside of
        /// contained controls.
        /// @version v20.09.01 - Added.
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
                default:
                    break;
            }
        }

        /// @brief Update step interval from default value.
        /// @version v20.09.01 - Added.
        private void UpdateStepInterval()
        {
            stepInterval = Properties.Settings.Default.UpdateEachSteps;
        }

        /// @brief Include a plugin to a propeller chip instance.
        /// @details Attach a plugin, linking the propeller instance to the plugin, opening a new
        /// tab window and enabling the close button by plugin's isClosable property.
        /// @param plugin Instance of a Gear.PluginSupport.PluginBase class to be attached.
        private void AttachPlugin(PluginBase plugin)
        {
            Chip.IncludePlugin(plugin); //include into plugin lists of a PropellerCPU instance
            plugin.PresentChip();       //invoke initial setup of plugin.

            TabPage t = new TabPage(plugin.Title)
            {
                Parent = documentsTab
            };
            plugin.Dock = DockStyle.Fill;
            plugin.Parent = t;
            documentsTab.SelectedTab = t;
            //Maintain the close button availability
            closeButton.Enabled = plugin.IsClosable;
        }

        /// @brief Delete a plugin from a propeller chip instance.
        /// @details Delete a plugin from the actives plugins of the propeller instance,
        /// effectively stopping the plugin. Remove also from pins and clock watch list.
        /// @param plugin Instance of a Gear.PluginSupport.PluginCommon class to be detached.
        /// @version v15.03.26 - Added.
        //Added method to detach a plugin from the active plugin list of the propeller instance.
        private void DetachPlugin(PluginBase plugin)
        {
            if (plugin.IsClosable)      //check if the plugin is able to close, then remove...
            {
                Chip.RemoveOnPins(plugin);  //from pins watch list
                Chip.RemoveOnClock(plugin); //from clock watch list
                Chip.RemovePlugin(plugin);  //from the plugins registered to the propeller emulator
            };
        }

        /// @brief Run the emulator updating the screen between a number of steps.
        /// @details The program property "UpdateEachSteps" gives the number of steps before
        /// screen repaint.
        /// Adjusting this number in configuration (like increasing the number) enable to obtain
        /// faster execution at expense of less screen responsiveness.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        private void RunEmulatorStep(object sender, EventArgs e)
        {
            for (uint i = 0; i < stepInterval; i++)
                if (!Chip.Step())
                {
                    runTimer.Stop();
                    break;
                }
            RepaintViews();
        }

        /// @brief Update Text and Images of buttons involved on running and
        /// stop state of emulator.
        /// @version v22.04.02 - Incorporated step Instruction and step Clock
        /// buttons to be controlled.
        private void UpdateRunningButtons()
        {
            if (IsRunningState != runEmulatorButton.Checked)
            {
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
        }

        /// @brief Load a binary image from file.
        /// @details Generate a new instance of a `PropellerCPU` and load
        /// the program from the binary.
        /// @version v22.03.02 - Bugfix of Form name not updated when open
        /// another binary file.
        public bool OpenFile(string FileName)
        {
            try
            {
                Chip.Initialize(File.ReadAllBytes(FileName));
                source = FileName;
                Text = FileName;
                Properties.Settings.Default.LastBinary = source;
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
        /// @param FileName Name and path to the XML plugin file to open
        /// @returns Reference to the new plugin instance (on success) or NULL (on fail).
        public PluginBase LoadPlugin(string FileName)
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Parse,
                ValidationType = ValidationType.DTD
            };
            XmlReader tr = XmlReader.Create(FileName, settings);
            bool ReadText = false;

            List<string> references = new List<string>();
            string instanceName = string.Empty;
            string code = string.Empty;
            string codeFileName = string.Empty;
            string pluginVersion = "0.1";

            try
            {

                while (tr.Read())
                {
                    if (ReadText)
                    {
                        if (string.IsNullOrEmpty(codeFileName))
                        {
                            //Mantain compatibility with old plugins (using Text section)
                            if (tr.NodeType == XmlNodeType.Text ||
                                    tr.NodeType == XmlNodeType.CDATA)
                                code = tr.Value;
                        }
                        else
                        {
                            codeFileName =
                                Path.Combine(Path.GetDirectoryName(FileName), codeFileName);
                            code = File.ReadAllText(codeFileName);
                        }
                        ReadText = false;
                    }
                    switch (tr.Name.ToLower())
                    {
                        case "plugin":
                            pluginVersion =
                                string.IsNullOrEmpty(tr.GetAttribute("version")) ?
                                pluginVersion :
                                tr.GetAttribute("version");
                            break;
                        case "reference":
                            if (!tr.IsEmptyElement)
                                references.Add(tr.GetAttribute("name"));
                            break;
                        case "instance":
                            instanceName = tr.GetAttribute("class");
                            break;
                        case "code":
                            ReadText = true;
                            codeFileName = tr.GetAttribute("codeFileName");
                            break;
                    }
                }

                //Dynamic load and compile the plugin module as a class, giving the chip
                // instance as a parameter.
                PluginBase plugin = ModuleCompiler.LoadModule(
                    code,
                    instanceName,
                    references.ToArray(),
                    Chip
                );

                if (plugin == null)  //if it fails...
                {
                    // ...open plugin editor in other window
                    PluginEditor pe = new PluginEditor(false);
                    pe.OpenFile(FileName, true);
                    pe.MdiParent = this.MdiParent;
                    pe.Show();
                    //the compilation errors are displayed in the error grid
                    ModuleCompiler.EnumerateErrors(pe.EnumErrors);
                    //show the error list
                    pe.ShowErrorGrid(true);
                }
                else  //successful compiling & instantiating of the new class...
                {
                    //...add the reference to the plugin list of the emulator instance
                    AttachPlugin(plugin);
                    //update location of last plugin
                    Properties.Settings.Default.LastPlugin = FileName;
                    Properties.Settings.Default.Save();
                }

                return plugin;
            }
            catch (Exception ex)
            {
                if ((ex is IOException) | (ex is XmlException))
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
                tr.Close();
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
                       FileName = source
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
            OpenFile(source);
            Chip.Reset();
            UpdateRunningButtons();
            RepaintViews();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            foreach (Control c in FloatControls)
                c.Parent = null;

            FloatControls.Clear();

            base.OnClosed(e);
        }

        /// @brief Repaint the Views, including float windows.
        private void RepaintViews()
        {
            foreach (Control s in FloatControls)
                s.Refresh();

            Control c = pinnedPanel.GetNextControl(null, true);

            if (c != null)
                ((PluginBase)c).Repaint(true);

            if ((documentsTab.SelectedTab != null) &&
                 ((c = documentsTab.SelectedTab.GetNextControl(null, true)) != null))
                ((PluginBase)c).Repaint(true);

            hubView.DataChanged();
        }

        /// @brief Event to reset the whole %Propeller Chip.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v20.10.01 - UpdateRunningButtons.
        private void ResetEmulator_Click(object sender, EventArgs e)
        {
            Chip.Reset();
            UpdateRunningButtons();
            RepaintViews();
        }

        /// @brief Close the plugin window and terminate the plugin instance.
        /// @details Not only close the tab window, also detach the plugin
        /// from the PropellerCPU what uses it.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        private void CloseActiveTab_Click(object sender, EventArgs e)
        {
            TabPage tp = documentsTab.SelectedTab;
            PluginBase p = (PluginBase)tp.Controls[0];

            if (p != null)          //test if cast to PluginBase works...
            {
                if (p.IsClosable)   //... so, test if we can close the tab
                {
                    if (documentsTab.SelectedIndex > 0)
                    {
                        //select the previous tab
                        documentsTab.SelectedIndex -= 1;
                        //tab changing housekeeping for plugin close button
                        DocumentsTab_Click(this, e);
                        //detach the plugin from the emulator
                        this.DetachPlugin(p);
                        p.Dispose();
                    }
                    tp.Parent = null;   //delete the reference to plugin
                };
            }
        }

        /// @brief
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
        private void FloatActiveTab_Click(object sender, EventArgs e)
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
            fw.Text = tp.Text + ": " + source;

            FloatControls.Add(fw);
        }

        /// @brief Unfloat the tab object.
        /// @param c Tab object.
        public void Unfloat(Control c)
        {
            TabPage tp = new TabPage(c.Text)
            {
                Parent = documentsTab
            };
            c.Parent = tp;
            c.Dock = DockStyle.Fill;

            FloatControls.Remove(c.Parent);
        }

        /// @brief
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
        private void PinActiveTab_Click(object sender, EventArgs e)
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
                tp = new TabPage(oldPin.Text)
                {
                    Parent = documentsTab
                };
                oldPin.Parent = tp;
            }
        }

        /// @brief
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
        private void UnpinButton_Click(object sender, EventArgs e)
        {
            Control oldPin = pinnedPanel.GetNextControl(null, true);

            if (oldPin != null)
            {
                TabPage tp = new TabPage(oldPin.Text)
                {
                    Parent = documentsTab
                };
                oldPin.Parent = tp;

                if (!pinnedSplitter.IsCollapsed)
                    pinnedSplitter.ToggleState();
            }
        }

        /// @brief Event to run the emulator freely.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
        /// @version v20.10.01 - UpdateRunningButtons.
        private void RunEmulator_Click(object sender, EventArgs e)
        {
            runTimer.Start();
            UpdateRunningButtons();
        }

        /// @brief Stop the emulation.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
        /// @version v20.10.01 - UpdateRunningButtons.
        private void StopEmulator_Click(object sender, EventArgs e)
        {
            runTimer.Stop();
            UpdateRunningButtons();
            RepaintViews(); //added the repaint, to refresh the views
        }

        /// @brief Run one clock tick of the active cog, stopping after executed.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v22.04.02 - Corrected method name.
        private void StepClock_Click(object sender, EventArgs e)
        {
            Chip.Step();
            UpdateRunningButtons();
            RepaintViews();
        }

        /// @brief Event to run one instruction in emulator, stopping after executed.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
        /// @version v20.10.01 - UpdateRunningButtons.
        private void StepInstruction_Click(object sender, EventArgs e)
        {
            if (documentsTab.SelectedTab != null)
            {
                Control c = documentsTab.SelectedTab.GetNextControl(null, true);

                if (c != null && c is CogView view)
                {
                    Cog cog = view.GetViewCog();

                    if (cog != null)
                        cog.StepInstruction();
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
            runTimer.Stop();
            UpdateRunningButtons();
            RepaintViews();
        }

        /// @brief Try to open a plugin, compiling it and attaching to the active
        /// emulator instance.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
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
                LoadPlugin(dialog.FileName);
        }

        /// @brief Event when the Emulator windows begin to close.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
        private void Emulator_FormClosing(object sender, FormClosingEventArgs e)
        {
            Chip.OnClose(sender, e);
        }

        /// @brief
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
        /// @version v20.10.01 - UpdateRunningButtons.
        private void OnDeactivate(object sender, EventArgs e)
        {
            runTimer.Stop();
            UpdateRunningButtons();
        }

        /// @brief Determine availability of close plugin button when tab is changed.
        /// @details Enable close plugin button based on if active tab is subclass of
        /// Gear.PluginSupport.PluginBase and if that class permit close the window. Typically
        /// the user plugins enabled it; but the cog window, main memory, logic probe, etc,
        /// don't allow to close.
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version V14.07.03 - Added.
        private void DocumentsTab_Click(object sender, EventArgs e)
        {
            TabPage tp = documentsTab.SelectedTab;
            if (tp.Controls[0] is PluginBase)
            {
                PluginBase b = (tp.Controls[0]) as PluginBase;
                if (b.IsClosable)
                    closeButton.Enabled = true;
                else
                    closeButton.Enabled = false;
                b.Repaint(false);
            }
            else
            {
                closeButton.Enabled = false;
            }
            tp.Invalidate();
        }

        /// @brief Process key press to manage the run state of emulator.
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
        /// @version v22.04.02 - Added input key for clock step.
        private void DocumentsTab_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (ActiveControl is PluginBase)
            {
                PluginBase b = ActiveControl as PluginBase;
                if (!b.AllowHotKeys)
                    return;
            }

            string key = e.KeyChar.ToString().ToLowerInvariant();
            if (key == "s")
            {
                if (IsRunningState)
                {
                    runTimer.Stop();
                    UpdateRunningButtons();
                }
                else
                    StepInstruction_Click(this, EventArgs.Empty);
            }

            if ((key == "r") & !IsRunningState)
            {
                runTimer.Start();
                UpdateRunningButtons();
            }

            if ((key == "c") & !IsRunningState)
                StepClock_Click(this, EventArgs.Empty);
        }

        /// @brief Refresh form's Icon
        /// @param sender Reference to the object where this event was called.
        /// @param e Class with the details event.
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
