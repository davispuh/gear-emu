/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * AppPropertiesEditor.cs
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
using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief Form to edit program properties.
    /// @since v15.03.26 - Added. 
    public partial class AppPropertiesEditor : Form
    {
        // PropertyGrid Control tutorial - https://msdn.microsoft.com/en-us/library/aa302326.aspx
        // also in https://msdn.microsoft.com/en-us/library/aa302334.aspx
        // and http://www.codeproject.com/Articles/22717/Using-PropertyGrid
        public AppPropertiesEditor()
        {
            InitializeComponent();
            GearPropertyGrid.SelectedObject = Settings.Default;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            //save to disk
            Properties.Settings.Default.Save();
            foreach(Form f in ParentForm.OwnedForms)
            {

            }
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
                DefaultSettingValueAttribute attr =
                    prop.Attributes[typeof(DefaultSettingValueAttribute)] 
                    as DefaultSettingValueAttribute;
                if (attr != null)  //if exist
                {
                    //remember old value
                    object oldValue = prop.GetValue(Settings.Default);
                    //set the new value
                    if (prop.CanResetValue(Settings.Default))
                        prop.ResetValue(Settings.Default);
                    else
                        prop.SetValue(
                            Settings.Default, 
                            Convert.ChangeType(attr.Value, prop.PropertyType) );
                    //call the notification event
                    GearPropertyGrid_PropertyValueChanged(sender, new PropertyValueChangedEventArgs(
                        GearPropertyGrid.SelectedGridItem, oldValue));
                }
                GearPropertyGrid.Refresh();
            }
        }

        /// @brief Event when a property had changed its value, used to update copies of the 
        /// property values used in other forms.
        /// @param s Sender object to this event.
        /// @param e Arguments to this event, including the old value.
        private void GearPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            PropertyDescriptor prop;    //to get the underlying property
            if (e.ChangedItem.GridItemType == GridItemType.Property &&
                !(prop = e.ChangedItem.PropertyDescriptor).IsReadOnly)
            {
                switch (prop.Name)
                {
                    case "UpdateEachSteps":
                        if (this.ParentForm.IsMdiContainer & this.ParentForm.HasChildren)
                            foreach(Form f in this.ParentForm.MdiChildren)
                            {
                                var emu = f as Emulator;
                                if (emu != null)
                                    emu.stepInterval = (uint)prop.GetValue(Settings.Default);
                            }
                        break;
                    case "EmbeddedCode":
                        if (this.ParentForm.IsMdiContainer & this.ParentForm.HasChildren)
                            foreach (Form f in this.ParentForm.MdiChildren)
                            {
                                var pluginEditor = f as PluginEditor;
                                if (pluginEditor != null)
                                {
                                    pluginEditor.UpdateTextEmbeddedCodeButton();
                                    pluginEditor.CodeChanged = true;
                                }
                            }
                        break;
                    case "FreqFormat":
                        if (this.ParentForm.IsMdiContainer & this.ParentForm.HasChildren)
                            foreach (Form f in this.ParentForm.MdiChildren)
                            {
                                var emu = f as Emulator;
                                if (emu != null)
                                    emu.hubView.UpdateCounterFreqTexts();
                            }
                        break;
                    case "HubTimeUnit":
                        if (this.ParentForm.IsMdiContainer & this.ParentForm.HasChildren)
                            foreach (Form f in this.ParentForm.MdiChildren)
                            {
                                var emu = f as Emulator;
                                if (emu != null)
                                    emu.hubView.UpdateTimeText();
                            }
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
