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
    /// @version v20.09.01 - Added.
    public partial class TimeUnitComboBox : ComboBox, Utils.ITimeUnitMgmt
    {
        /// @brief Time Units Management Instance.
        /// @version v22.06.01 - Changed name and visibility of member.
        private readonly TimeUnitMgmt _manager;

        /// @brief Excluded time units.
        /// @details Implements ITimeUnitMgmt interface.
        [Category("Misc")]
        [Description("Excluded Time Units for this control.")]
        [DisplayName("Excluded Time Units")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TimeUnitCollection ExcludedUnits
        {
            get => _manager.ExcludedUnits;
            set => _manager.ExcludedUnits = value;
        }

        /// @brief Base unit used to transform multiply factors values.
        /// @details Implements ITimeUnitMgmt interface.
        [Category("Misc")]
        [Description("Base Unit of Time for this control.")]
        [DisplayName("Base Time Unit")]
        [Browsable(true)]
        public TimeUnitsEnum BaseUnit
        {
            get => _manager.BaseUnit;
            set => _manager.BaseUnit = value;
        }

        /// @brief Time unit selected on this combobox.
        /// @details Implements ITimeUnitMgmt interface.
        [Category("Misc")]
        [Description("Initial selected unit of time for this control.")]
        [DisplayName("Selected Time Unit")]
        [Browsable(true)]
        public TimeUnitsEnum TimeUnitSelected
        {
            get => _manager.TimeUnitSelected;
            set => _manager.TimeUnitSelected = value;
        }

        /// @brief Factor value of selected index of combo box.
        /// @details Implements ITimeUnitMgmt interface.
        [Browsable(false)]
        public double FactorSelected => _manager.FactorSelected;

        /// @brief
        /// @details Implements ITimeUnitMgmt interface.
        [Browsable(false)]
        public bool IsMultiplyFactor => _manager.IsMultiplyFactor;

        /// @brief Default constructor.
        public TimeUnitComboBox()
        {
            _manager = new TimeUnitMgmt(this);
            InitializeComponent();
        }

        /// <summary>Synchronize values dependent of excludedUnits.</summary>
        public void SyncValues() => _manager.SyncValues();

        /// @brief Select the next valid value, rolling over if it is necessary.
        /// @details Implements ITimeUnitMgmt interface.
        public void SelectNext() => _manager.SelectNext();

        /// @brief Select the previous valid value, rolling over if it is necessary.
        /// @details Implements ITimeUnitMgmt interface.
        public void SelectPrev() => _manager.SelectPrev();

        /// @brief Assign the list of TextFormats methods to corresponding enum values.
        /// @details Implements ITimeUnitMgmt interface.
        /// @param assignments List of assignments.
        public void AssignTextFormats(DelegatesPerTimeUnitsList assignments) =>
            _manager.AssignTextFormats(assignments);

        /// @brief Get formatted text for the value.
        /// @details Implements ITimeUnitMgmt interface.
        /// @param val Value to convert to text.
        /// @return Formatted text of the value.
        public string GetFormattedText(double val) =>
            _manager.GetFormattedText(val);
    }
}
