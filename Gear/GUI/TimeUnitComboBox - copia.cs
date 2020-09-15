/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * TimeUnitComboBox.cs
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
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief Combobox specialization for time unit management.
    /// @since 20.06.01 - Added.
    public partial class TimeUnitComboBox : ComboBox
    {
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
            get
            {
                return excludedUnits;
            }
            set
            {
                if (excludedUnits != value)
                {
                    excludedUnits = value;
                    EnabledValuesList = new TimeUnitsList(excludedUnits);
                    Items.Clear();
                    Items.AddRange(EnabledValuesList.GetNames());
                }
            }
        }

        /// @brief Enabled values extension list.
        [BrowsableAttribute(false)]
        public TimeUnitsList EnabledValuesList { get; set; } = new TimeUnitsList();

        /// @brief Base unit used to transform multiply factors values.
        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Base Unit of Time for this control.")]
        [DisplayNameAttribute("Base Time Unit")]
        [BrowsableAttribute(true)]
        public TimeUnitsEnum BaseUnit { get; set; } = TimeUnitsEnum.None;

        /// @brief Time unit selected on this combobox.
        [BrowsableAttribute(false)]
        public TimeUnitsEnum TimeUnitSelected
        {
            get
            {
                if (SelectedIndex >= 0)
                    return EnabledValuesList.Values[SelectedIndex].Id;
                else
                    return TimeUnitsEnum.None;
            }
            set
            {
                SelectedIndex = EnabledValuesList.IndexOfKey(value);
            }
        }

        /// @brief Factor of the time unit selected on this combobox.
        /// @return Number factor.
        public double FactorSelected
        {
            get
            {
                return EnabledValuesList.Values[SelectedIndex].Factor;
            }
        }

        /// @brief Determine if Factor has to multiplyed, or divided.
        /// @return If Factor has to multiply (=true), or divide (=false).
        public bool IsMultiplyFactor
        {
            get
            {
                return EnabledValuesList.Values[SelectedIndex].IsMultiplyFactor;
            }
        }

        /// @brief Default constructor
        public TimeUnitComboBox()
        {
            ExcludedUnits = new TimeUnitCollection { TimeUnitsEnum.None };
            InitializeComponent();
            TimeUnitSelected = TimeUnitsEnum.None;
        }

        /// @brief Select the next value of enabled values of ComboBox. If it 
        /// is the last one, starts from the begining.
        public void SelectNext()
        {
            if (SelectedIndex != -1)
            {
                int next = SelectedIndex + 1;
                SelectedIndex = (next < EnabledValuesList.Count) ? next : 0;
            }
        }

        /// @brief Select the previus value of enabled values of ComboBox. If it 
        /// is the first one, starts from the end.
        public void SelectPrev()
        {
            if (SelectedIndex != -1)
            {
                int prev = SelectedIndex - 1;
                SelectedIndex = (prev > 0) ? prev : EnabledValuesList.Count - 1;
            }
        }

/*
        public static TimeUnitsEnum Next(TimeUnitsEnum thisVal)
        {
            TimeUnitsEnum next = thisVal + 1;
            return (next <= Enum.GetValues(typeof(TimeUnitsEnum)).Cast<TimeUnitsEnum>().Last()) ?
                next : 
                Enum.GetValues(typeof(TimeUnitsEnum)).Cast<TimeUnitsEnum>().First();
        }

        public static TimeUnitsEnum Prev(TimeUnitsEnum thisVal)
        {
            TimeUnitsEnum prev = thisVal - 1;
            return (prev >= Enum.GetValues(typeof(TimeUnitsEnum)).Cast<TimeUnitsEnum>().First()) ?
                prev :
                Enum.GetValues(typeof(TimeUnitsEnum)).Cast<TimeUnitsEnum>().Last();
        }

        public static string GetText(TimeUnitsEnum thisVal)
        {
            if (thisVal != TimeUnitsEnum.min_s)
                return thisVal.ToString();
            else
                return "m:s";
        }

        public static string[] CreateArray(params TimeUnitsEnum[] values)
        {
            if (values == null)
                return null;
            string[] retVal = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
                retVal[i] = GetText(values[i]);
            return retVal;
        }

        public static TimeUnitsEnum ParseText(string text)
        {
            TimeUnitsEnum retVal;
            try
            {
                retVal = (TimeUnitsEnum)Enum.Parse(typeof(TimeUnitsEnum), text, ignoreCase: true);

            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                if (text.Equals("m:s", StringComparison.InvariantCultureIgnoreCase))
                    retVal = TimeUnitsEnum.min_s;
                else
                    throw;
            }
            catch (OverflowException)
            {
                throw;
            }
            return retVal;
        }
*/

    }

}
