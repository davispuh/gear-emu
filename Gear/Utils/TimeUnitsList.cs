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
    /// @brief List of TimeUnitsEnum with related TimeUnitsEnumExtension.
    public class TimeUnitsList : SortedList<TimeUnitsEnum, TimeUnitsEnumExtension>
    {
        /// @brief Default constructor.
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

        /// @brief Returns a array of names of each element on list.
        /// @return Array of names of each element on list.
        public string[] GetNames()
        {
            int len = Count;
            string[] retVal = new string[len];
            int idx = 0;
            foreach (var pair in this)
                retVal[idx++] = pair.Value.Name;
            return retVal;
        }

        /// @brief Assign the list of TextFormats methods to corresponding enum values.
        /// @param assignments List of assignments.
        /// @throws ArgumentNullException
        /// @throws KeyNotFoundException
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

        /// @brief Determine if a text format method had been assigned to
        /// the key.
        /// @param key Time unit to inquire.
        /// @return If a valid delegate method is assigned (=true); if it is
        /// null reference or the key isn't in the list (=false).
        public bool HaveAssignedTextFormat(TimeUnitsEnum key)
        {
            if (TryGetValue(key, out TimeUnitsEnumExtension enumExtension))
                return enumExtension != null;
            return false;
        }

    } //end class TimeUnitsList

    /// @brief List of format text delegates assigned to each time unit.
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

        /// @brief Add or replace the delegate member method associated with
        /// the time unit.
        /// @param value Time unit.
        /// @param delegate Method to format text for this time unit.
        /// @throws ArgumentOutOfRangeException
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

        /// @brief Inherited Add method, marked as obsolete, to force
        /// to use AddOrReplace(.) method.
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
