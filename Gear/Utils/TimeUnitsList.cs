/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * TimeUnitsList.cs
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

namespace Gear.Utils
{
    /// <summary>List of TimeUnitsEnum with related TimeUnitsEnumExtension.</summary>
    public class TimeUnitsList : SortedList<TimeUnitsEnum, TimeUnitsEnumExtension>
    {
        /// <summary>Default constructor.</summary>
        public TimeUnitsList() { }

        /// <summary>Constructor with excluded units.</summary>
        /// <param name="excludedUnits">Time units collection to
        /// exclude values on creation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TimeUnitsList(TimeUnitCollection excludedUnits)
        {
            if (excludedUnits == null)
                throw new ArgumentNullException(nameof(excludedUnits));
            TimeUnitsEnum[] values = (TimeUnitsEnum[])Enum.GetValues(typeof(TimeUnitsEnum));
            if (excludedUnits.Count == 0)
                foreach (var val in values)
                    Add(val, new TimeUnitsEnumExtension(val));
            else
                foreach (var val in values)
                    if (!excludedUnits.Contains(val))
                        Add(val, new TimeUnitsEnumExtension(val));
        }

        /// <summary>Returns a array of names of each element on list.</summary>
        /// <returns>Array of names of each element on list.</returns>
        public string[] GetNames()
        {
            int len = Count;
            string[] retVal = new string[len];
            int idx = 0;
            foreach (var pair in this)
                retVal[idx++] = pair.Value.Name;
            return retVal;
        }

        /// <summary>Assign the list of TextFormats methods to corresponding
        /// enum values.</summary>
        /// @param assignments List of assignments.
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public void AssignTextFormats(DelegatesPerTimeUnitsList assignments)
        {
            if (assignments == null)
                throw new ArgumentNullException(nameof(assignments));
            foreach (var item in this)
                if (assignments.TryGetValue(item.Key, out FormatToTextDelegate @delegate))
                    item.Value.FormatToTextDel = @delegate;
                else
                {
                    string msg = $"TimeUnit TimeUnitsEnum.{item.Key} not found in parameter assignments List.";
                    throw new KeyNotFoundException(msg);
                }
        }

        /// <summary>Determine if a text format method had been assigned to
        /// the key.</summary>
        /// @param key Time unit to inquire.
        /// <returns>If a valid delegate method is assigned (=true); if it is
        /// null reference or the key isn't in the list (=false).</returns>
        public bool HaveAssignedTextFormat(TimeUnitsEnum key)
        {
            if (TryGetValue(key, out TimeUnitsEnumExtension enumExtension))
                return enumExtension != null;
            return false;
        }

    }

    /// <summary>List of format text delegates assigned to each time unit.</summary>
    public class DelegatesPerTimeUnitsList :
        SortedList<TimeUnitsEnum, FormatToTextDelegate>
    {
        /// <summary></summary>
        private readonly TimeUnitCollection _excludedItems;

        /// <summary>Default constructor.</summary>
        /// <param name="excluded">Excluded items.</param>
        /// <param name="initialList">List of assignments.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DelegatesPerTimeUnitsList(TimeUnitCollection excluded,
            SortedList<TimeUnitsEnum, FormatToTextDelegate> initialList)
        {
            _excludedItems = excluded;
            if (initialList == null)
                throw new ArgumentNullException(nameof(initialList));
            foreach (var pair in initialList)
                AddOrReplace(pair.Key, pair.Value);
        }

        /// <summary>Add or replace the delegate member method associated with
        /// the time unit.</summary>
        /// @param value Time unit.
        /// @param delegate Method to format text for this time unit.
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void AddOrReplace(TimeUnitsEnum value,
            FormatToTextDelegate @delegate)
        {
            if (_excludedItems != null && _excludedItems.Count > 0)
            {
                if (!_excludedItems.Contains(value))
                {
                    if (ContainsKey(value))
                        Remove(value);
                    base.Add(value, @delegate);
                }
                else
                {
                    string msg = $"Time unit \"{value}\" is in excluded Time units list!";
                    throw new ArgumentOutOfRangeException(nameof(value), value, msg);
                }
            }
            else
            {
                if (ContainsKey(value))
                    Remove(value);
                base.Add(value, @delegate);
            }
        }

        /// <summary>Inherited Add method, marked as obsolete, to force
        /// to use AddOrReplace(.) method.</summary>
        /// @param value Time unit.
        /// @param delegate Method to format text for this time unit.
        [Obsolete("Use AddOrReplace(.) method instead", error: true)]
        public new void Add(TimeUnitsEnum value,
            FormatToTextDelegate @delegate)
        {
            base.Add(value, @delegate);
        }

    } //end class DelegatesPerTimeUnitsList

} //end namespace Gear.Utils
