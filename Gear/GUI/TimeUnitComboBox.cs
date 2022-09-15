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

// ReSharper disable CommentTypo

namespace Gear.GUI
{
    /// <summary>Combobox specialization for time unit management.</summary>
    /// @version v20.09.01 - Added.
    public partial class TimeUnitComboBox : ComboBox, Utils.ITimeUnitMgmt
    {
        /// <summary>Time Units Management Instance.</summary>
        /// @version v22.06.01 - Changed name and visibility of member.
        private readonly TimeUnitMgmt _manager;

        /// <summary>Excluded time units.</summary>
        /// <remarks>Implements ITimeUnitMgmt interface.</remarks>
        /// @version v22.06.01 - Changed designer category to default.
        [Category("Default"), Description("Excluded Time Units for this control."),
         DisplayName("Excluded Time Units"), Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TimeUnitCollection ExcludedUnits
        {
            get => _manager.ExcludedUnits;
            set => _manager.ExcludedUnits = value;
        }

        /// <summary>Base unit used to transform multiply factors values.</summary>
        /// <remarks>Implements ITimeUnitMgmt interface.</remarks>
        /// @version v22.06.01 - Changed designer category to default.
        [Category("Default"), Description("Base Unit of Time for this control."),
         DisplayName("Base Time Unit"), Browsable(true)]
        public TimeUnitsEnum BaseUnit
        {
            get => _manager.BaseUnit;
            set => _manager.BaseUnit = value;
        }

        /// <summary>Time unit selected on this combobox.</summary>
        /// <remarks>Implements ITimeUnitMgmt interface.</remarks>
        /// @version v22.06.01 - Changed designer category to default.
        [Category("Default"), Description("Initial selected unit of time for this control."),
         DisplayName("Selected Time Unit"), Browsable(true)]
        public TimeUnitsEnum TimeUnitSelected
        {
            get => _manager.TimeUnitSelected;
            set => _manager.TimeUnitSelected = value;
        }

        /// <summary>Factor value of selected index of combo box.</summary>
        /// <remarks>Implements ITimeUnitMgmt interface.</remarks>
        [Browsable(false)]
        public double FactorSelected => _manager.FactorSelected;

        /// <summary>TRUE if factor should be multiplied, FALSE if it should
        /// be divided.</summary>
        /// <remarks>Implements ITimeUnitMgmt interface.</remarks>
        [Browsable(false)]
        public bool IsMultiplyFactor => _manager.IsMultiplyFactor;

        /// <summary>Default constructor.</summary>
        public TimeUnitComboBox()
        {
            _manager = new TimeUnitMgmt(this);
            InitializeComponent();
        }

        /// <summary>Synchronize values dependent of excludedUnits.</summary>
        public void SyncValues() => _manager.SyncValues();

        /// <summary>Select the next valid value, rolling over if it is
        /// necessary.</summary>
        /// <remarks>Implements ITimeUnitMgmt interface.</remarks>
        public void SelectNext() => _manager.SelectNext();

        /// <summary>Select the previous valid value, rolling over if it is
        /// necessary.</summary>
        /// <remarks>Implements ITimeUnitMgmt interface.</remarks>
        public void SelectPrev() => _manager.SelectPrev();

        /// <summary>Assign the list of TextFormats methods to corresponding
        /// enum values.</summary>
        /// <remarks>Implements ITimeUnitMgmt interface.</remarks>
        /// @param assignments List of assignments.
        public void AssignTextFormats(DelegatesPerTimeUnitsList assignments) =>
            _manager.AssignTextFormats(assignments);

        /// <summary>Get formatted text for the value.</summary>
        /// <remarks>Implements ITimeUnitMgmt interface.</remarks>
        /// @param val Value to convert to text.
        /// <returns>Formatted text of the value.</returns>
        public string GetFormattedText(double val) =>
            _manager.GetFormattedText(val);
    }
}
