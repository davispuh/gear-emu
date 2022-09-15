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

    /// <summary>Interface for Time Unit Management.</summary>
    public interface ITimeUnitMgmt
    {
        /// <summary>Excluded time units.</summary>
        TimeUnitCollection ExcludedUnits { get; set; }

        /// <summary>Base unit used to transform multiply factors values.</summary>
        TimeUnitsEnum BaseUnit { get; set; }

        /// <summary>Time unit selected on this combobox.</summary>
        TimeUnitsEnum TimeUnitSelected { get; set; }

        /// <summary>Factor of the time unit selected on this combobox.</summary>
        double FactorSelected { get; }

        /// <summary>Determine if Factor has to multiplied, or divided.</summary>
        bool IsMultiplyFactor { get; }

        /// <summary>Synchronize values dependent of excludedUnits.</summary>
        void SyncValues();

        /// <summary>Select the next value of enabled values of ComboBox.</summary>
        void SelectNext();

        /// <summary>Select the previous value of enabled values of
        /// ComboBox.</summary>
        void SelectPrev();

        /// <summary>Assign the list of TextFormats methods to corresponding
        /// enum values.</summary>
        void AssignTextFormats(DelegatesPerTimeUnitsList assignments);

        /// <summary>Get the formatted text representation of parameter,
        /// using the assigned delegate method.</summary>
        ///@version v22.06.01 - Method name changed to correct misspelling.
        string GetFormattedText(double val);
    }

    /// <summary>Management of Time Units.</summary>
    public class TimeUnitMgmt : ITimeUnitMgmt
    {
        /// <summary>Reference to callback of ComboBox.</summary>
        private readonly ComboBox _owner;

        /// <summary>Internal member for excluded time units.</summary>
        private TimeUnitCollection _excludedUnits;

        /// <summary>Excluded time units.</summary>
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

        /// <summary>Enabled values extension list.</summary>
        public TimeUnitsList EnabledValuesList { get; private set; } =
            new TimeUnitsList();

        /// <summary>Base unit used to transform multiply factors values.</summary>
        /// @version v22.06.01 - Changed designer category to default.
        [Category("Default"), Description("Base Unit of Time for this control."),
         DisplayName("Base Time Unit"), Browsable(true)]
        public TimeUnitsEnum BaseUnit { get; set; } = TimeUnitsEnum.None;

        /// <summary>Time unit selected on this combobox.</summary>
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

        /// <summary>Factor of the time unit selected on this combobox.</summary>
        /// <returns>Number factor.</returns>
        public double FactorSelected =>
            _owner.SelectedIndex >= 0 && EnabledValuesList.Count > 0 ?
                EnabledValuesList.Values[_owner.SelectedIndex].Factor :
                0.0;

        /// <summary>Determine if Factor has to multiplied, or divided.</summary>
        /// <returns>If Factor has to multiply (=true), or divide (=false).</returns>
        public bool IsMultiplyFactor =>
            _owner.SelectedIndex < 0 || EnabledValuesList.Count <= 0 ||
            EnabledValuesList.Values[_owner.SelectedIndex].IsMultiplyFactor;

        /// <summary>Default constructor.</summary>
        /// @param baseObj ComboBox for callback.
        /// @exception ArgumentNullException Thrown if baseObj is null.
        public TimeUnitMgmt(ComboBox baseObj)
        {
            _owner = baseObj ?? throw new ArgumentNullException(nameof(baseObj));
            ExcludedUnits = new TimeUnitCollection { TimeUnitsEnum.None };
            TimeUnitSelected = EnabledValuesList.Keys.FirstOrDefault();
        }

        /// <summary>Synchronize values dependent of excludedUnits:
        /// EnabledValuesList and comboBox list values.</summary>
        public void SyncValues()
        {
            EnabledValuesList = new TimeUnitsList(_excludedUnits);
            _owner.Items.Clear();
            _owner.Items.AddRange(EnabledValuesList.GetNames());
        }

        /// <summary>Select the next value of enabled values of ComboBox. If it
        /// is the last one, starts from the beginning.</summary>
        public void SelectNext()
        {
            if (_owner.SelectedIndex == -1)
                return;
            int next = _owner.SelectedIndex + 1;
            _owner.SelectedIndex = next < EnabledValuesList.Count ?
                next :
                0;
        }

        /// <summary>Select the previous value of enabled values of ComboBox. If it
        /// is the first one, starts from the end.</summary>
        public void SelectPrev()
        {
            if (_owner.SelectedIndex == -1)
                return;
            int prev = _owner.SelectedIndex - 1;
            _owner.SelectedIndex = prev >= 0 ?
                prev :
                EnabledValuesList.Count - 1;
        }

        /// <summary>Assign the list of TextFormats methods to corresponding
        /// enum values.</summary>
        /// @param assignments List of assignments.
        public void AssignTextFormats(DelegatesPerTimeUnitsList assignments)
        {
            EnabledValuesList.AssignTextFormats(assignments);
        }

        /// <summary>Get the formatted text representation of parameter,
        /// using the assigned delegate method.</summary>
        /// @param val Numeric value to format.
        /// <returns>Formatted text.</returns>
        /// <exception cref="KeyNotFoundException">If selected time unit is
        /// not on enabled values list or Delegate method not set for
        /// TimeUnitsEnum.</exception>
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
