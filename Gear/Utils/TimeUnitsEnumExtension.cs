/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * TimeUnitsEnumExtension.cs
 * Managed Time units enumeration expansion.
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
using System.Linq;
using System.Reflection;

namespace Gear.Utils
{
    /// @brief Managed Time units enumeration expansion.
    [Serializable]
    public enum TimeUnitsEnum : int
    {
        None = 0,   //!< @brief None selected
        ns,     //!< @brief nano seconds (10^-9 s)
        us,     //!< @brief micro seconds (10^-6 s)
        ms,     //!< @brief mili seconds (10^-3 s)
        s,      //!< @brief seconds (1 s)
        min_s   //!< @brief minutes and seconds
    }

    /// @brief Delegate to Format to text a numeric value considering its 
    /// associated time unit value.
    public delegate string FormatToTextDelegate(TimeUnitsEnum unit, double val);

    /// @brief Expanded time unit Enumeration class.
    [Serializable]
    public class TimeUnitsEnumExtension : IComparable
    {
        /// @brief Time Unit value.
        public TimeUnitsEnum Id { get; private set; }

        /// @brief Internal name store.
        private string name;

        /// @brief Name to display property.
        /// @return Name to display.
        public string Name 
        {
            get => name;
            private set {
                if (Id != TimeUnitsEnum.min_s)
                    name = Id.ToString();
                else
                    name = "m:s";
            }
        }

        /// @brief External defined method to format to text a value using 
        /// time unit support.
        public FormatToTextDelegate FormatToTextDel { get; set; }

        /// @brief Factor of this time unit. If this is defined to multiply
        /// or divide, is given by IsMultiplyFactor property.
        /// @return Number factor.
        public double Factor
        {
            get => GetFactor(Id);
        }

        /// @brief Determine if Factor has to multiplyed, or divided.
        /// @return If Factor has to multiply (=true), or divide (=false).
        public bool IsMultiplyFactor
        {
            get
            {
                if (Id == TimeUnitsEnum.min_s)
                    return false;
                else
                    return true;
            }
        }
    
        /// @brief Default constructor.
        /// @param id Time unit with extended atributes.
        public TimeUnitsEnumExtension(TimeUnitsEnum id)
        {
            Id = id;
            Name = string.Empty;
        }

        /// @brief Get the factor associated to the unit.
        /// @param unit The time unit.
        /// @return Factor.
        public static double GetFactor(TimeUnitsEnum unit)
        {
            switch (unit)
            {
                case TimeUnitsEnum.None:
                    return 0.0;
                case TimeUnitsEnum.ns:
                    return 1e9f;
                case TimeUnitsEnum.us:
                    return 1e6f;
                case TimeUnitsEnum.ms:
                    return 1e3f;
                case TimeUnitsEnum.s:
                    return 1.0;
                case TimeUnitsEnum.min_s:
                    return 60.0;
                default:
                    {
                        string msg = $" Value {unit} has not assigned conversion factor.";
                        Debug.Assert(false, msg);
                        throw new Exception(msg);
                    }
            }
        }

        /// @brief @todo Document method TimeUnitsEnumExtension.GetIsMultiplyFactor(.)
        /// @param unit
        /// @return
        public static bool GetIsMultiplyFactor(TimeUnitsEnum unit)
        {
            if (unit == TimeUnitsEnum.min_s)
                return false;
            else
                return true;
        }

        /// @brief Factor referenced to baseUnit, to transform between 
        /// TimeUnitsEnum values.
        /// @param unitToTransform Time unit to transform.
        /// @param isMultiplyFactor
        /// @param baseUnit Base time unit.
        /// @return Multiply factor relative to base unit.
        public static double TransformUnitsFactor(TimeUnitsEnum unitToTransform,
            out bool isMultiplyFactor,
            TimeUnitsEnum baseUnit = TimeUnitsEnum.s)
        {
            isMultiplyFactor = GetIsMultiplyFactor(baseUnit);
            if (baseUnit == TimeUnitsEnum.s)
                return GetFactor(unitToTransform);
            else
            {
                if (GetIsMultiplyFactor(unitToTransform) ==
                    GetIsMultiplyFactor(baseUnit))
                    return GetFactor(unitToTransform) / GetFactor(baseUnit);
                else
                    return GetFactor(unitToTransform) * GetFactor(baseUnit);
            }
        }

        /// @brief @todo Document method TimeUnitsEnumExtension.ToString()
        public override string ToString()
        {
            return string.Concat(
                $"{{{Id}, \"{Name}\", ",
                (FormatToTextDel is null) ?
                    "Delegate: null" :
                    FormatToTextDel.ToString(),
                ", ",
                string.Format("\"{0} {1}\"",
                    (IsMultiplyFactor) ? "*" : "/",
                    Factor),
                "}");
        }

        /// @brief @todo Document method TimeUnitsEnumExtension.GetAll<T>()
        /// @tparam T
        /// @return
        public static IEnumerable<T> GetAll<T>() where T : TimeUnitsEnumExtension
        {
            var fields = typeof(T).GetFields(BindingFlags.Public |
                                             BindingFlags.Static |
                                             BindingFlags.DeclaredOnly);

            return fields.Select(f => f.GetValue(null)).Cast<T>();
        }

        /// @brief @todo Document method TimeUnitsEnumExtension.Equals(.)
        public override bool Equals(object obj)
        {
            TimeUnitsEnumExtension otherValue = obj as TimeUnitsEnumExtension;
            if (otherValue == null)
                return false;
            var typeMatches = GetType().Equals(obj.GetType());
            var valueMatches = Id.Equals(otherValue.Id);
            return typeMatches && valueMatches;
        }

        /// @brief @todo Document method TimeUnitsEnumExtension.GetHashCode()
        public override int GetHashCode()
        {
            return Tuple.Create<TimeUnitsEnum, string>(Id, Name).GetHashCode();
        }

        /// @brief Comparison between current instance and the parameter object.
        /// @details Implements IComparable interface.
        /// @param other Other object to compare.
        /// @return Comparison result, this instance precedes the other (< 0), both are 
        /// in the same position (= 0), or the other instance precedes this (> 0).
        public int CompareTo(object other) => 
            Id.CompareTo(((TimeUnitsEnumExtension)other).Id);

        /// @brief @todo Document method TimeUnitsEnumExtension.
        /// @details Implements IComparable interface.
        /// @param left
        /// @param right
        /// @returns
        public static bool operator ==(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// @brief @todo Document method TimeUnitsEnumExtension.
        /// @details Implements IComparable interface.
        /// @param left
        /// @param right
        /// @returns
        public static bool operator !=(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            return !(left == right);
        }

        /// @brief @todo Document method TimeUnitsEnumExtension.
        /// @details Implements IComparable interface.
        /// @param left
        /// @param right
        /// @returns
        public static bool operator <(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            return left is null ? 
                right is object : 
                left.CompareTo(right) < 0;
        }

        /// @brief @todo Document method TimeUnitsEnumExtension.
        /// @details Implements IComparable interface.
        /// @param left
        /// @param right
        /// @returns
        public static bool operator <=(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        /// @brief @todo Document method TimeUnitsEnumExtension.
        /// @details Implements IComparable interface.
        /// @param left
        /// @param right
        /// @returns
        public static bool operator >(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        /// @brief @todo Document method TimeUnitsEnumExtension.
        /// @details Implements IComparable interface.
        /// @param left
        /// @param right
        /// @returns
        public static bool operator >=(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            return left is null ?
                right is null : 
                left.CompareTo(right) >= 0;
        }

    } //end class TimeUnitsEnumExtension

} //end namespace Gear.Utils
