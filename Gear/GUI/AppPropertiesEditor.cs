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
    /// <summary>Form to edit program properties.</summary>
    /// @version v15.03.26 - Added.
    public partial class AppPropertiesEditor : Form
    {
        /// <summary>How many instances are created?</summary>
        /// @version v20.10.01 - Added.
        private static int _instanceNumber = 0;

        /// <summary>Default constructor.</summary>
        /// <remarks>References:
        ///   - PropertyGrid Control tutorial - https://msdn.microsoft.com/en-us/library/aa302326.aspx
        /// also in https://msdn.microsoft.com/en-us/library/aa302334.aspx
        /// and https://www.codeproject.com/Articles/22717/Using-PropertyGrid
        /// </remarks>
        /// <exception cref="SingleInstanceException">If an intent to open a
        /// new instance of the same class.</exception>
        /// @version v20.10.01 - Manage only one instance of AppPropertiesEditor.
        public AppPropertiesEditor()
        {
            if (_instanceNumber > 0)
                throw new SingleInstanceException();
            InitializeComponent();
            GearPropertyGrid.SelectedObject = Settings.Default;
            _instanceNumber++;
        }

        /// <summary>Close the window, updating the related values in each form.</summary>
        /// @param sender Sender object to this event.
        /// @param e Event arguments to this event.
        private void OKButton_Click(object sender, EventArgs e)
        {
            //save to disk
            Settings.Default.Save();
            //redraw forms
            if (ParentForm != null)
                foreach (Form form in ParentForm.OwnedForms)
                    form.Refresh();
            Close();
        }

        /// <summary>Event handler to refresh the data grid contents.</summary>
        /// <param name="sender">Sender object to this event.</param>
        /// <param name="e">Event arguments to this event.</param>
        /// @version v22.06.02 - Added.
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            GearPropertyGrid.Refresh();
        }

        /// <summary>Reset the property to its default value.</summary>
        /// @param sender Sender object to this event.
        /// @param e Event arguments to this event.
        private void ResetButton_Click(object sender, EventArgs e)
        {
            //to get the underlying property
            PropertyDescriptor prop = GearPropertyGrid.SelectedGridItem.PropertyDescriptor;
            //check if a property is selected and if it is writable
            if (prop is null || prop.IsReadOnly ||
                GearPropertyGrid.SelectedGridItem.GridItemType != GridItemType.Property)
                return;
            //try to get the default value of the property
            if (prop.Attributes[typeof(DefaultSettingValueAttribute)] is DefaultSettingValueAttribute attr)  //if exist
            {
                //remember old value
                object oldValue = prop.GetValue(Settings.Default);
                //set the new value
                if (prop.CanResetValue(Settings.Default))
                    prop.ResetValue(Settings.Default);
                else
                    prop.SetValue(Settings.Default,
                        Convert.ChangeType(attr.Value, prop.PropertyType));
                //call the notification event
                GearPropertyGrid_PropertyValueChanged(sender,
                    new PropertyValueChangedEventArgs(
                        GearPropertyGrid.SelectedGridItem, oldValue));
            }
            GearPropertyGrid.Refresh();
        }

        /// <summary>Event when a property had changed its value, used to update
        /// copies of the property values used in other forms.</summary>
        /// @param s Sender object to this event.
        /// @param e Arguments to this event, including the old value.
        /// @version v22.06.02 - Removes updating of `TabSize`, `LastPlugin`
        /// and `UseAnimations` properties because now they are using
        /// DataBindings as update mechanism.
        private void GearPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //to get the underlying property
            PropertyDescriptor prop = e.ChangedItem.PropertyDescriptor;
            if (prop is null || prop.IsReadOnly ||
                e.ChangedItem.GridItemType != GridItemType.Property ||
                ParentForm == null)
                return;
            switch (prop.Name)
            {
                case "LastTickMarkGrid":
                case "LastTimeFrame":
                case "LogicViewTimeUnit":
                case "FreqFormat":
                case "HubTimeUnit":
                case "UpdateEachSteps":
                    foreach (Form form in ParentForm.MdiChildren)
                        if (form is Emulator emulator)
                            emulator.UpdateVarValue(prop.Name);
                    break;
            }
        }

        /// <summary>Manage event of closed window.</summary>
        /// @param sender Reference to object where event was raised.
        /// @param e Form closed event data arguments.
        /// @version v20.10.01 - Added to manage only one instance of
        /// AppPropertiesEditor.
        private void AppPropertiesEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            _instanceNumber--;
        }

        /// <summary>Refresh form's Icon.</summary>
        /// @param sender Reference to object where event was raised.
        /// @param e Event data arguments.
        /// @version v20.10.01 - Added.
        private void AppPropertiesEditor_Load(object sender, EventArgs e)
        {
            //workaround of bug on MDI Form (https://stackoverflow.com/a/6701490/10200101)
            Icon = Icon.Clone() as Icon;
        }

        /// <summary>Event handler when the grid lost the focus.</summary>
        /// <param name="sender">Sender object to this event.</param>
        /// <param name="e">Event arguments to this event.</param>
        /// @version v22.06.02 - Added to manage visibility of Reset button.
        private void GearPropertyGrid_Leave(object sender, EventArgs e)
        {
            ResetButton.Enabled = false;
        }

        /// <summary>Event handler when the property grid get the focus.</summary>
        /// <param name="sender">Sender object to this event.</param>
        /// <param name="e">Event arguments to this event.</param>
        /// @version v22.06.02 - Added to manage visibility of Reset button.
        private void GearPropertyGrid_Enter(object sender, EventArgs e)
        {
            if (GearPropertyGrid.SelectedGridItem != null)
                ResetButton.Enabled = true;
        }

        /// <summary>Event handler when the selected grid item is changed.</summary>
        /// <param name="sender">Sender object to this event.</param>
        /// <param name="e">Event arguments to this event.</param>
        /// @version v22.06.02 - Added to manage visibility of Reset button.
        private void GearPropertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            ResetButton.Enabled = GearPropertyGrid.SelectedGridItem != null;
        }
    }
}
