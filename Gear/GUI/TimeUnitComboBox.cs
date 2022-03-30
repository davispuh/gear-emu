/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * TimeUnitComboBox.cs
 * Combobox specialization for time unit management.
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

using Gear.Utils;
using System.ComponentModel;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief Combobox specialization for time unit management.
    /// @since v20.09.01 - Added.
    public partial class TimeUnitComboBox : ComboBox, ITimeUnitMgmt
    {
        /// @brief Time Units Management Instance
        public TimeUnitMgmt Mgmt;

        /// @brief Excluded time units.
        /// @details Implements ITimeUnitMgmt interface.
        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Excluded Time Units for this control.")]
        [DisplayNameAttribute("Excluded Time Units")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TimeUnitCollection ExcludedUnits
        {
            get => Mgmt.ExcludedUnits;
            set => Mgmt.ExcludedUnits = value;
        }

        /// @brief Base unit used to transform multiply factors values.
        /// @details Implements ITimeUnitMgmt interface.
        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Base Unit of Time for this control.")]
        [DisplayNameAttribute("Base Time Unit")]
        [BrowsableAttribute(true)]
        public TimeUnitsEnum BaseUnit
        {
            get => Mgmt.BaseUnit;
            set => Mgmt.BaseUnit = value;
        }

        /// @brief Time unit selected on this combobox.
        /// @details Implements ITimeUnitMgmt interface.
        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Initial selected unit of time for this control.")]
        [DisplayNameAttribute("Selected Time Unit")]
        [BrowsableAttribute(true)]
        public TimeUnitsEnum TimeUnitSelected
        {
            get => Mgmt.TimeUnitSelected;
            set => Mgmt.TimeUnitSelected = value;
        }

        /// @brief Factor value of selected index of combo box.
        /// @details Implements ITimeUnitMgmt interface.
        [BrowsableAttribute(false)]
        public double FactorSelected => Mgmt.FactorSelected;

        /// @brief @todo Document class TimeUnitsComboBox.IsMultiplyFactor
        /// @details Implements ITimeUnitMgmt interface.
        [BrowsableAttribute(false)]
        public bool IsMultiplyFactor => Mgmt.IsMultiplyFactor;

        /// @brief Default constructor.
        public TimeUnitComboBox() : base()
        {
            Mgmt = new TimeUnitMgmt(this);
            InitializeComponent();
        }

        /// @brief Syncronize values dependent of excludedUnits
        public void SyncValues() => Mgmt.SyncValues();

        /// @brief Select the next valid value, rolling over if it is necesary.
        /// @details Implements ITimeUnitMgmt interface.
        public void SelectNext() => Mgmt.SelectNext();

        /// @brief Select the previous valid value, rolling over if it is necesary.
        /// @details Implements ITimeUnitMgmt interface.
        public void SelectPrev() => Mgmt.SelectPrev();

        /// @brief Assign the list of TextFormats methods to corresponding enum values.
        /// @details Implements ITimeUnitMgmt interface.
        /// @param assigments List of assigments.
        public void AssignTextFormats(DelegatesPerTimeUnitsList assigments) =>
            Mgmt.AssignTextFormats(assigments);

        /// @brief Get formated text for the value.
        /// @details Implements ITimeUnitMgmt interface.
        /// @param val Value to convert to text.
        /// @return Formated text of the value.
        public string GetFormatedText(double val) => 
            Mgmt.GetFormatedText(val);

    }
}
