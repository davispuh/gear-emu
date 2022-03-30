/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * AppPropertiesEditor.cs
 * Form to edit program properties.
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

using Gear.Properties;
using Gear.Utils;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief Form to edit program properties.
    /// @since v15.03.26 - Added. 
    public partial class AppPropertiesEditor : Form
    {
        /// @brief How many instances are created?
        /// @version v20.10.01 - Added.
        private static int instanceNumber = 0;

        /// @brief Default constructor.
        /// @throws SingleInstanceException If an intent to open a new instance
        ///  of the same class.
        /// @version v20.10.01 - Manage only one instance of AppPropertiesEditor.
        // PropertyGrid Control tutorial - https://msdn.microsoft.com/en-us/library/aa302326.aspx
        // also in https://msdn.microsoft.com/en-us/library/aa302334.aspx
        // and https://www.codeproject.com/Articles/22717/Using-PropertyGrid
        public AppPropertiesEditor()
        {
            if (instanceNumber > 0)
                throw new SingleInstanceException();
            InitializeComponent();
            GearPropertyGrid.SelectedObject = Settings.Default;
            instanceNumber++;
        }

        /// @brief Close the window, updating the related values in each formm.
        /// @param sender Sender object to this event.
        /// @param e Arguments to this event.
        private void OKButton_Click(object sender, EventArgs e)
        {
            //save to disk
            Properties.Settings.Default.Save();
            //redraw forms
            foreach(Form f in ParentForm.OwnedForms)
                f.Refresh();
            this.Close();
        }

        /// @brief Reset the property to its default value.
        /// @param sender Sender object to this event.
        /// @param e Arguments to this event.
        private void ResetButton_Click(object sender, EventArgs e)
        {
            PropertyDescriptor prop;    //to get the underlying property
            //check if a property is selected and if it is writeable
            if (GearPropertyGrid.SelectedGridItem.GridItemType == GridItemType.Property && 
                !(prop = GearPropertyGrid.SelectedGridItem.PropertyDescriptor).IsReadOnly)
            {
                //try to get the default value of the property
                if (prop.Attributes[typeof(DefaultSettingValueAttribute)] is DefaultSettingValueAttribute attr)  //if exist
                {
                    //remember old value
                    object oldValue = prop.GetValue(Settings.Default);
                    //set the new value
                    if (prop.CanResetValue(Settings.Default))
                        prop.ResetValue(Settings.Default);
                    else
                        prop.SetValue(
                            Settings.Default,
                            Convert.ChangeType(attr.Value, prop.PropertyType));
                    //call the notification event
                    GearPropertyGrid_PropertyValueChanged(sender,
                        new PropertyValueChangedEventArgs(
                            GearPropertyGrid.SelectedGridItem, oldValue));
                }
                GearPropertyGrid.Refresh();
            }
        }

        /// @brief Event when a property had changed its value, used to update copies of the 
        /// property values used in other forms.
        /// @param s Sender object to this event.
        /// @param e Arguments to this event, including the old value.
        /// @version v20.09.01 - Modified to include new properties.
        private void GearPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            PropertyDescriptor prop;    //to get the underlying property
            if (e.ChangedItem.GridItemType == GridItemType.Property &&
                !(prop = e.ChangedItem.PropertyDescriptor).IsReadOnly)
            {
                switch (prop.Name)
                {
                    case "LastTickMarkGrid":
                    case "LastTimeFrame":
                    case "LogicViewTimeUnit":
                    case "FreqFormat":
                    case "HubTimeUnit":
                    case "UpdateEachSteps":
                        foreach (Form form in ParentForm.MdiChildren)
                            if (form is Emulator emu)
                                emu.UpdateVarValue(prop.Name);
                        break;
                    case "TabSize":
                        foreach (Form form in ParentForm.MdiChildren)
                            if (form is PluginEditor pluginEditor)
                                pluginEditor.UpdateTabs(reloadText: true);
                        break;
                    case "LastPlugin":
                        foreach (Form form in ParentForm.MdiChildren)
                            if (form is PluginEditor pluginEditor)
                                pluginEditor.UpdateLastPlugin();
                        break;
                    default:
                        break;
                }
            }
        }

        /// @brief Manage event of closed window.
        /// @param sender
        /// @param e
        /// @version v20.10.01 - Added to manage only one instance of 
        /// AppPropertiesEditor.
        private void AppPropertiesEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            instanceNumber--;
        }

        /// @brief Refresh form's Icon 
        /// @param sender
        /// @param e
        /// @version v20.10.01 - Added.
        private void AppPropertiesEditor_Load(object sender, EventArgs e)
        {
            //workaround of bug on MDI Form (https://stackoverflow.com/a/6701490/10200101)
            Icon = Icon.Clone() as Icon;
        }
    }
}
