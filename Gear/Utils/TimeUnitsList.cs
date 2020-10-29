/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2020 - Gear Developers
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
using System.Diagnostics;

namespace Gear.Utils
{
    /// @brief List of TimeUnitsEnum with related TimeUnitsEnumExtension.
    public class TimeUnitsList : SortedList<TimeUnitsEnum, TimeUnitsEnumExtension>
    {
        /// @brief Default constructor.
        public TimeUnitsList() : base() { }

        /// @brief Constructor with excluded units.
        /// @param excludedUnits Time units collection to exclude values 
        /// on creation.
        public TimeUnitsList(TimeUnitCollection excludedUnits) : base()
        {
            var values = (TimeUnitsEnum[])Enum.GetValues(typeof(TimeUnitsEnum));
            if ((excludedUnits == null) | (excludedUnits?.Count == 0))
                foreach (var val in values)
                    this.Add(val, new TimeUnitsEnumExtension(val));
            else
                foreach (var val in values)
                    if (!excludedUnits.Contains(val))
                        this.Add(val, new TimeUnitsEnumExtension(val));
        }

        /// @brief Returns a array of names of each element on list.
        /// @return Array of names of each element on list.
        public string[] GetNames()
        {
            int len = this.Count;
            string[] retVal = new string[len];
            int idx = 0;
            foreach (var pair in this)
                retVal[idx++] = pair.Value.Name;
            return retVal;
        }

        /// @brief Assign the list of TextFormats methods to corresponding enum values.
        /// @param assigments List of assigments.
        public void AssignTextFormats(DelegatesPerTimeUnitsList assigments)
        {
            foreach (var item in this)
                if (assigments.TryGetValue(item.Key, out FormatToTextDelegate _delegate))
                    item.Value.FormatToTextDel = _delegate;
                else
                {
                    string msg = $"TimeUnit TimeUnitsEnum.{item.Key} not " +
                        $"found in parameter assigments List.";
                    Debug.Assert(false, msg);
                    item.Value.FormatToTextDel = null;
                }
        }

        /// @brief Determine if a text format method had been assigned to 
        /// the key.
        /// @param key Time unit to inquire.
        /// @return If a valid delegate method is assigned (=true); if it is 
        /// null reference or the key isn't in the list (=false).
        public bool HaveAssignedTextFormat(TimeUnitsEnum key)
        {
            if (this.TryGetValue(key, out TimeUnitsEnumExtension enumExtension))
                return enumExtension != null;
            else
                return false;
        }

    } //end class TimeUnitsList 

    /// @brief List of format text delegates assigned to each time unit.
    public class DelegatesPerTimeUnitsList : 
        SortedList<TimeUnitsEnum, FormatToTextDelegate>
    {
        private readonly TimeUnitCollection excludedItems;

        /// @brief Default constructor
        /// @param excluded Excluded items.
        /// @param listaInicial List of assigments
        public DelegatesPerTimeUnitsList(TimeUnitCollection excluded, 
            SortedList<TimeUnitsEnum, FormatToTextDelegate> listaInicial) : 
                base()
        {
            excludedItems = excluded;
            foreach (var pair in listaInicial)
                AddOrReplace(pair.Key, pair.Value);
        }

        /// @brief Add or replace the delegate member method associated with 
        /// the time unit.
        /// @param value Time unit.
        /// @param _delegate Method to format text for this time unit.
        private void AddOrReplace(TimeUnitsEnum value, 
            FormatToTextDelegate _delegate)
        {
            if ((excludedItems != null) | (excludedItems.Count > 0))
            {
                bool valid = !excludedItems.Contains(value);
                if (valid)
                {
                    if (this.ContainsKey(value))
                        Remove(value);
                    base.Add(value, _delegate);
                }
                else
                {
                    string msg = $"Time unit \"{value}\" is in excluded " +
                        $"Time units list!";
                    Debug.Assert(valid, msg);
                }
            }
            else
            {
                if (this.ContainsKey(value))
                    Remove(value);
                base.Add(value, _delegate);
            }
        }

        /// @brief Inheritated Add method, marked as obsolete, to force 
        /// to use AddOrReplace(.) method.
        /// @param value Time unit.
        /// @param _delegate Method to format text for this time unit.
        [ObsoleteAttribute("Use AddOrReplace(.) method instead", error: true)]
        public new void Add(TimeUnitsEnum value, 
            FormatToTextDelegate _delegate)
        {
            base.Add(value, _delegate);
        }

    } //end class DelegatesPerTimeUnitsList

} //end namespace Gear.Utils
