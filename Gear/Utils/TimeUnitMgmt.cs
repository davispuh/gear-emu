/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
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

        /// @brief Determine if Factor has to multiplyed, or divided.
        bool IsMultiplyFactor { get; }

        /// @brief Syncronize values dependent of excludedUnits
        void SyncValues();

        /// @brief Select the next value of enabled values of ComboBox.
        void SelectNext();

        /// @brief Select the previous value of enabled values of ComboBox.
        void SelectPrev();

        /// @brief Assign the list of TextFormats methods to corresponding 
        /// enum values.
        void AssignTextFormats(DelegatesPerTimeUnitsList assigments);

        /// @brief Get the formated text representation of parameter,
        /// using the assigned delegate method.
        string GetFormatedText(double val);
    }

    /// @brief Management of Time Units.
    public class TimeUnitMgmt : ITimeUnitMgmt
    {
        /// @brief Reference to callback of ComboBox.
        private readonly ComboBox Owner;

        /// @brief Internal member for excluded time units.
        private TimeUnitCollection excludedUnits = null;

        /// @brief Excluded time units.
        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Excluded Time Units for this control.")]
        [DisplayNameAttribute("Excluded Time Units")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TimeUnitCollection ExcludedUnits
        {
            get => excludedUnits;
            set
            {
                if (value == null)
                {
                    excludedUnits = null;
                    SyncValues();
                }
                else if (!value.Equals(excludedUnits))
                {
                    excludedUnits = value;
                    SyncValues();
                }
            }
        }

        /// @brief Enabled values extension list.
        public TimeUnitsList EnabledValuesList { get; private set; } =
            new TimeUnitsList();

        /// @brief Base unit used to transform multiply factors values.
        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Base Unit of Time for this control.")]
        [DisplayNameAttribute("Base Time Unit")]
        [BrowsableAttribute(true)]
        public TimeUnitsEnum BaseUnit { get; set; } = TimeUnitsEnum.None;

        /// @brief Time unit selected on this combobox.
        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Initial selected unit of time for this control.")]
        [DisplayNameAttribute("Selected Time Unit")]
        [BrowsableAttribute(true)]
        public TimeUnitsEnum TimeUnitSelected
        {
            get
            {
                if ((Owner.SelectedIndex >= 0) && (EnabledValuesList.Count > 0))
                    return EnabledValuesList.Values[Owner.SelectedIndex].Id;
                else
                    return TimeUnitsEnum.None;
            }
            set => Owner.SelectedIndex = EnabledValuesList.IndexOfKey(value);
        }

        /// @brief Factor of the time unit selected on this combobox.
        /// @return Number factor.
        public double FactorSelected
        {
            get
            {
                if ((Owner.SelectedIndex >= 0) && (EnabledValuesList.Count > 0))
                    return EnabledValuesList.Values[Owner.SelectedIndex].Factor;
                else
                    return 0.0;
            }
        }

        /// @brief Determine if Factor has to multiplyed, or divided.
        /// @return If Factor has to multiply (=true), or divide (=false).
        public bool IsMultiplyFactor
        {
            get
            {
                if ((Owner.SelectedIndex >= 0) && (EnabledValuesList.Count > 0))
                    return EnabledValuesList.Values[Owner.SelectedIndex].IsMultiplyFactor;
                else
                    return true;
            }
        }

        /// @brief Default constructor
        /// @param baseObj ComboBox for callback.
        /// @exception ArgumentNullException Thrown if baseObj is null.
        public TimeUnitMgmt(ComboBox baseObj)
        {
            if (baseObj != null)
            {
                Owner = baseObj;
                ExcludedUnits = new TimeUnitCollection { TimeUnitsEnum.None };
                TimeUnitSelected = EnabledValuesList.Keys.FirstOrDefault();
            }
            else
                throw new ArgumentNullException(nameof(baseObj));
        }

        /// @brief Syncronize values dependent of excludedUnits: 
        /// EnabledValuesList and comboBox list values.
        public void SyncValues()
        {
            EnabledValuesList = new TimeUnitsList(excludedUnits);
            Owner.Items.Clear();
            Owner.Items.AddRange(EnabledValuesList.GetNames());
        }

        /// @brief Select the next value of enabled values of ComboBox. If it 
        /// is the last one, starts from the begining.
        public void SelectNext()
        {
            if (Owner.SelectedIndex != -1)
            {
                int next = Owner.SelectedIndex + 1;
                Owner.SelectedIndex = (next < EnabledValuesList.Count) ? next : 0;
            }
        }

        /// @brief Select the previous value of enabled values of ComboBox. If it 
        /// is the first one, starts from the end.
        public void SelectPrev()
        {
            if (Owner.SelectedIndex != -1)
            {
                int prev = Owner.SelectedIndex - 1;
                Owner.SelectedIndex = (prev >= 0) ? 
                    prev : 
                    EnabledValuesList.Count - 1;
            }
        }

        /// @brief Assign the list of TextFormats methods to corresponding 
        /// enum values.
        /// @param assigments List of assigments.
        public void AssignTextFormats(DelegatesPerTimeUnitsList assigments)
        {
            EnabledValuesList.AssignTextFormats(assigments);
        }

        /// @brief Get the formated text representation of parameter,
        /// using the assigned delegate method.
        /// @param val Numeric value to format.
        /// @return Formated text.
        public string GetFormatedText(double val)
        {
            TimeUnitsEnum sel = TimeUnitSelected;
            if (EnabledValuesList.TryGetValue(sel, 
                out TimeUnitsEnumExtension obj) && (obj != null))
            {
                if (EnabledValuesList.HaveAssignedTextFormat(sel))
                    return obj.FormatToTextDel(sel, val);
                else
                {
                    string msg = $"Selected time unit 'TimeUnitsEnum.{sel}' " +
                        $"isn't in enabled values list!";
                    throw new KeyNotFoundException(msg);
                    //Debug.Assert(false, msg);
                    //return string.Empty;
                }
            }
            else
            {
                string msg = $"Delegate method not set for TimeUnitsEnum.{sel}!";
                throw new KeyNotFoundException(msg);
                //Debug.Assert(false, msg);
                //return string.Empty;
            }
        }

    } //end class TimeUnitMgmt

} //end namespace Gear.Utils

