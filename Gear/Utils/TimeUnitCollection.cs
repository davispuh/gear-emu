/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2020 - Gear Developers
 * --------------------------------------------------------------------------------
 * TimeUnitCollection.cs
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;

namespace Gear.Utils
{
    /// @brief Time units Collection.
    [Serializable]
    public class TimeUnitCollection : Collection<TimeUnitsEnum>, IEquatable<TimeUnitCollection>
    {
        /// @brief Default constructor.
        public TimeUnitCollection() : base() { }

        /// @brief Constructor with a list interface compliant class. Each 
        /// value is unique and the list are sorted by enum value.
        /// @param values Time unit enumeration values.
        public TimeUnitCollection(IList<TimeUnitsEnum> values) : base(values)
        {
            //get distinct and sorted list of values
            SortedSet<TimeUnitsEnum> tmp = new SortedSet<TimeUnitsEnum>(this.Distinct());
            this.Clear();
            foreach (TimeUnitsEnum item in tmp)
                base.Add(item);
        }

        /// @brief Add the item if it isn't already in the list, maintaining 
        /// it ordered by TimeUnitsEnum value.
        /// @param newItem Item to add to collection.
        public new void Add(TimeUnitsEnum newItem)
        {
            if (this.IndexOf(newItem) == -1)
            {
                if (this.Count == 0)
                    base.Add(newItem);
                else
                {
                    TimeUnitsEnum carry = TimeUnitsEnum.None;
                    bool inserted = false;
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (inserted)
                        {
                            TimeUnitsEnum tmp = this[i];
                            base.InsertItem(i, carry);
                            carry = tmp;
                        }
                        else if (this[i] > newItem)
                        {
                            carry = this[i];
                            base.InsertItem(i, newItem);
                            inserted = true;
                        }
                    }
                    if (!inserted)
                        carry = newItem;
                    if (carry != TimeUnitsEnum.None)
                        base.Add(carry);
                }
            }
        }

        /// @brief Equality operator
        /// @param other Other object to compare.
        /// @returns If both are the same object or have the same 
        /// items (=true), else (=false).
        public bool Equals(TimeUnitCollection other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (this.Count != other.Count)
                return false;
            else
            {
                for (int i = 0; i < Count; i++)
                {
                    if (this[i] != other[i])
                        return false;
                }
                return true;
            }
        }

    }

    /// @brief Converter class for TimeUnitCollection.
    /// @details This is necessary to expose the TimeUnitCollection on design 
    /// time in HubView.
    class TimeUnitCollectionConverter : TypeConverter
    {
        public override PropertyDescriptorCollection GetProperties(
            ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(TimeUnitCollection));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, 
            Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, 
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                System.Reflection.ConstructorInfo ci =
                    typeof(TimeUnitCollection).GetConstructor(System.Type.EmptyTypes);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

}

