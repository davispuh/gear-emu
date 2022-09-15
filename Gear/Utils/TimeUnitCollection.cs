/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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
    /// <summary>Time units Collection.</summary>
    [Serializable]
    public class TimeUnitCollection : Collection<TimeUnitsEnum>, IEquatable<TimeUnitCollection>
    {
        /// <summary>Default constructor.</summary>
        public TimeUnitCollection() { }

        /// <summary>Constructor with a list interface compliant class. Each
        /// value is unique and the list are sorted by enum value.</summary>
        /// @param values Time unit enumeration values.
        public TimeUnitCollection(IList<TimeUnitsEnum> values) : base(values)
        {
            //get distinct and sorted list of values
            SortedSet<TimeUnitsEnum> tmp = new SortedSet<TimeUnitsEnum>(this.Distinct());
            Clear();
            foreach (TimeUnitsEnum item in tmp)
                base.Add(item);
        }

        /// <summary>Add the item if it isn't already in the list, maintaining
        /// it ordered by TimeUnitsEnum value.</summary>
        /// @param newItem Item to add to collection.
        public new void Add(TimeUnitsEnum newItem)
        {
            if (IndexOf(newItem) != -1)
                return;
            if (Count == 0)
                base.Add(newItem);
            else
            {
                TimeUnitsEnum carry = TimeUnitsEnum.None;
                bool inserted = false;
                for (int i = 0; i < Count; i++)
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

        /// <summary>Equality operator.</summary>
        /// @param other Other object to compare.
        /// <returns>If both are the same object or have the same
        /// items (=true), else (=false).</returns>
        public bool Equals(TimeUnitCollection other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (Count != other.Count)
                return false;
            for (int i = 0; i < Count; i++)
                if (this[i] != other[i])
                    return false;
            return true;
        }

    }

    /// <summary>Converter class for TimeUnitCollection.</summary>
    /// <remarks>This is necessary to expose the TimeUnitCollection on design
    /// time in HubView.</remarks>
    public class TimeUnitCollectionConverter : TypeConverter
    {
        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public override PropertyDescriptorCollection GetProperties(
            ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(TimeUnitCollection));
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override bool CanConvertTo(ITypeDescriptorContext context,
            Type destinationType)
        {
            return destinationType == typeof(InstanceDescriptor) ||
                   base.CanConvertTo(context, destinationType);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                System.Reflection.ConstructorInfo ci =
                    typeof(TimeUnitCollection).GetConstructor(Type.EmptyTypes);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
