/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * TimeUnitMgmt.cs
 * Management of Time Units.
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
using System.Linq;
using System.Windows.Forms;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable IdentifierTypo
namespace Gear.Utils
{

    /// @brief Interface for Time Unit Management.
    public interface ITimeUnitMgmt
    {
        /// @brief Excluded time units.
        TimeUnitCollection ExcludedUnits { get; set; }

        /// @brief Base unit used to transform multiply factors values.
        TimeUnitsEnum BaseUnit { get; set; }

        /// @brief Time unit selected on this combobox.
        TimeUnitsEnum TimeUnitSelected { get; set; }

        /// @brief Factor of the time unit selected on this combobox.
        double FactorSelected { get; }

        /// @brief Determine if Factor has to multiplied, or divided.
        bool IsMultiplyFactor { get; }

        /// @brief Synchronize values dependent of excludedUnits
        void SyncValues();

        /// @brief Select the next value of enabled values of ComboBox.
        void SelectNext();

        /// @brief Select the previous value of enabled values of ComboBox.
        void SelectPrev();

        /// @brief Assign the list of TextFormats methods to corresponding
        /// enum values.
        void AssignTextFormats(DelegatesPerTimeUnitsList assignments);

        /// @brief Get the formatted text representation of parameter,
        /// using the assigned delegate method.
        ///@version v22.06.01 - Method name changed to correct misspelling.
        string GetFormattedText(double val);
    }

    /// @brief Management of Time Units.
    public class TimeUnitMgmt : ITimeUnitMgmt
    {
        /// @brief Reference to callback of ComboBox.
        private readonly ComboBox _owner;

        /// @brief Internal member for excluded time units.
        private TimeUnitCollection _excludedUnits;

        /// @brief Excluded time units.
        /// @version v22.06.01 - Changed designer category to default.
        [Category("Default"), Description("Excluded Time Units for this control."),
         DisplayName("Excluded Time Units"), Browsable(true),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TimeUnitCollection ExcludedUnits
        {
            get => _excludedUnits;
            set
            {
                if (value == null)
                {
                    _excludedUnits = null;
                    SyncValues();
                }
                else if (!value.Equals(_excludedUnits))
                {
                    _excludedUnits = value;
                    SyncValues();
                }
            }
        }

        /// @brief Enabled values extension list.
        public TimeUnitsList EnabledValuesList { get; private set; } =
            new TimeUnitsList();

        /// @brief Base unit used to transform multiply factors values.
        /// @version v22.06.01 - Changed designer category to default.
        [Category("Default"), Description("Base Unit of Time for this control."),
         DisplayName("Base Time Unit"), Browsable(true)]
        public TimeUnitsEnum BaseUnit { get; set; } = TimeUnitsEnum.None;

        /// @brief Time unit selected on this combobox.
        /// @version v22.06.01 - Changed designer category to default.
        [Category("Default"), Description("Initial selected unit of time for this control."),
         DisplayName("Selected Time Unit"), Browsable(true)]
        public TimeUnitsEnum TimeUnitSelected
        {
            get =>
                _owner.SelectedIndex >= 0 && EnabledValuesList.Count > 0 ?
                    EnabledValuesList.Values[_owner.SelectedIndex].Id :
                    TimeUnitsEnum.None;
            set => _owner.SelectedIndex = EnabledValuesList.IndexOfKey(value);
        }

        /// @brief Factor of the time unit selected on this combobox.
        /// @return Number factor.
        public double FactorSelected =>
            _owner.SelectedIndex >= 0 && EnabledValuesList.Count > 0 ?
                EnabledValuesList.Values[_owner.SelectedIndex].Factor :
                0.0;

        /// @brief Determine if Factor has to multiplied, or divided.
        /// @return If Factor has to multiply (=true), or divide (=false).
        public bool IsMultiplyFactor =>
            _owner.SelectedIndex < 0 || EnabledValuesList.Count <= 0 ||
            EnabledValuesList.Values[_owner.SelectedIndex].IsMultiplyFactor;

        /// @brief Default constructor
        /// @param baseObj ComboBox for callback.
        /// @exception ArgumentNullException Thrown if baseObj is null.
        public TimeUnitMgmt(ComboBox baseObj)
        {
            _owner = baseObj ?? throw new ArgumentNullException(nameof(baseObj));
            ExcludedUnits = new TimeUnitCollection { TimeUnitsEnum.None };
            TimeUnitSelected = EnabledValuesList.Keys.FirstOrDefault();
        }

        /// @brief Synchronize values dependent of excludedUnits:
        /// EnabledValuesList and comboBox list values.
        public void SyncValues()
        {
            EnabledValuesList = new TimeUnitsList(_excludedUnits);
            _owner.Items.Clear();
            _owner.Items.AddRange(EnabledValuesList.GetNames());
        }

        /// @brief Select the next value of enabled values of ComboBox. If it
        /// is the last one, starts from the beginning.
        public void SelectNext()
        {
            if (_owner.SelectedIndex == -1)
                return;
            int next = _owner.SelectedIndex + 1;
            _owner.SelectedIndex = next < EnabledValuesList.Count ?
                next :
                0;
        }

        /// @brief Select the previous value of enabled values of ComboBox. If it
        /// is the first one, starts from the end.
        public void SelectPrev()
        {
            if (_owner.SelectedIndex == -1)
                return;
            int prev = _owner.SelectedIndex - 1;
            _owner.SelectedIndex = prev >= 0 ?
                prev :
                EnabledValuesList.Count - 1;
        }

        /// @brief Assign the list of TextFormats methods to corresponding
        /// enum values.
        /// @param assignments List of assignments.
        public void AssignTextFormats(DelegatesPerTimeUnitsList assignments)
        {
            EnabledValuesList.AssignTextFormats(assignments);
        }

        /// @brief Get the formatted text representation of parameter,
        /// using the assigned delegate method.
        /// @param val Numeric value to format.
        /// @return Formatted text.
        /// @throws KeyNotFoundException If selected time unit is not on
        /// enabled values list or Delegate method not set for TimeUnitsEnum.
        public string GetFormattedText(double val)
        {
            TimeUnitsEnum sel = TimeUnitSelected;
            if (EnabledValuesList.TryGetValue(sel,
                out TimeUnitsEnumExtension obj) && obj != null)
            {
                if (EnabledValuesList.HaveAssignedTextFormat(sel))
                    return obj.FormatToTextDel(sel, val);
                string msg = $"Selected time unit 'TimeUnitsEnum.{sel}' is not on enabled values list!";
                throw new KeyNotFoundException(msg);
            }
            else
            {
                string msg = $"Delegate method not set for TimeUnitsEnum.{sel}!";
                throw new KeyNotFoundException(msg);
            }
        }
    }
}
