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

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief Combobox specialization for time unit management.
    /// @since @version 20.06.01 - Added.
    public partial class TimeUnitComboBox : ComboBox
    {
        /// @brief Managed Time units enumeration.
        public enum TimeUnitsEnum : int
        {
            ns,     //!< @brief nano seconds (10^-9 s)
            us,     //!< @brief micro seconds (10^-6 s)
            ms,     //!< @brief mili seconds (10^-3 s)
            s,      //!< @brief seconds (1 s)
            Min_s   //!< @brief minutes and seconds
        }

        /// @brief Default constructor
        public TimeUnitComboBox()
        {
            InitializeComponent();
            Collection<string> TimeUnits =
                new Collection<string>(CreateArray((TimeUnitsEnum[])Enum.GetValues(typeof(TimeUnitsEnum))));
            Items.AddRange(TimeUnits.ToArray());
        }

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
            if (thisVal != TimeUnitsEnum.Min_s)
                return thisVal.ToString();
            else
                return "m:s";
        }

        public static string[] CreateArray(params TimeUnitComboBox.TimeUnitsEnum[] values)
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
                    retVal = TimeUnitsEnum.Min_s;
                else
                    throw;
            }
            catch (OverflowException)
            {
                throw;
            }
            return retVal;
        }

    }

}
