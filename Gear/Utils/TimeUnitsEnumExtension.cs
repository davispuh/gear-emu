/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

namespace Gear.Utils
{
    /// @brief Managed Time units enumeration expansion.
    [Serializable]
    public enum TimeUnitsEnum : byte
    {
        /// <summary>None selected</summary>
        None = 0,
        /// <summary>nano seconds (10^-9 s)</summary>
        ns,
        /// <summary>micro seconds (10^-6 s)</summary>
        us,
        /// <summary>mili seconds (10^-3 s)</summary>
        ms,
        /// <summary>seconds (1 s)</summary>
        s,
        /// <summary>minutes and seconds</summary>
        min_s
    }

    /// @brief Delegate to Format text a numeric value considering its
    /// associated time unit value.
    public delegate string FormatToTextDelegate(TimeUnitsEnum unit, double val);

    /// @brief Expanded time unit Enumeration class.
    [Serializable]
    public class TimeUnitsEnumExtension : IComparable
    {
        /// @brief Time Unit value.
        public readonly TimeUnitsEnum Id;

        /// @brief Name to display property.
        /// @return Name to display.
        public string Name =>
            Id != TimeUnitsEnum.min_s ?
                Id.ToString() :
                "m:s";

        /// @brief External defined method to format to text a value using
        /// time unit support.
        public FormatToTextDelegate FormatToTextDel { get; set; }

        /// @brief Factor of this time unit. If this is defined to multiply
        /// or divide, is given by IsMultiplyFactor property.
        /// @return Number factor.
        public double Factor =>
            GetFactor(Id);

        /// @brief Determine if Factor has to multiplied, or divided.
        /// @return If Factor has to multiply (=true), or divide (=false).
        public bool IsMultiplyFactor =>
            Id != TimeUnitsEnum.min_s;

        /// @brief Default constructor.
        /// @param id Time unit with extended attributes.
        public TimeUnitsEnumExtension(TimeUnitsEnum id)
        {
            Id = id;
        }

        /// @brief Get the factor associated to the unit.
        /// @param unit The time unit.
        /// @return Factor.
        /// <exception cref="InvalidEnumArgumentException"></exception>
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
                    throw new InvalidEnumArgumentException(msg);
                }
            }
        }

        /// <summary></summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool GetIsMultiplyFactor(TimeUnitsEnum unit)
        {
            return unit != TimeUnitsEnum.min_s;
        }

        /// @brief Factor referenced to base unit, to transform between
        /// TimeUnitsEnum values.
        /// @param unitToTransform Time unit to transform.
        /// @param isMultiplyFactor
        /// @param baseUnit Base time unit.
        /// @return Multiply factor relative to base unit.
        public static double TransformUnitsFactor(TimeUnitsEnum unitToTransform,
            out bool isMultiplyFactor,
            TimeUnitsEnum baseUnit)
        {
            isMultiplyFactor = GetIsMultiplyFactor(baseUnit);
            if (baseUnit == TimeUnitsEnum.s)
                return GetFactor(unitToTransform);
            if (GetIsMultiplyFactor(unitToTransform) ==
                GetIsMultiplyFactor(baseUnit))
                return GetFactor(unitToTransform) / GetFactor(baseUnit);
            return GetFactor(unitToTransform) * GetFactor(baseUnit);
        }

        /// <summary></summary>
        /// <returns></returns>
        public override string ToString() =>
            $"{{{Id}, \"{Name}\", {(FormatToTextDel == null ? "Delegate: null" : FormatToTextDel.ToString())}, \"{(IsMultiplyFactor ? "*" : "/")} {Factor}\"}}";

        /// <summary></summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetAll<T>() where T : TimeUnitsEnumExtension
        {
            var fields = typeof(T).GetFields(BindingFlags.Public |
                                             BindingFlags.Static |
                                             BindingFlags.DeclaredOnly);
            return fields.Select(f => f.GetValue(null)).Cast<T>();
        }

        /// <summary></summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is TimeUnitsEnumExtension otherValue))
                return false;
            var typeMatches = GetType() == obj.GetType();
            var valueMatches = Id.Equals(otherValue.Id);
            return typeMatches && valueMatches;
        }

        /// <summary></summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Tuple.Create(Id, Name).GetHashCode();
        }

        /// @brief Comparison between current instance and the parameter object.
        /// @details Implements IComparable interface.
        /// @param obj Other object to compare.
        /// @return Comparison result, this instance precedes the other (< 0), both are
        /// in the same position (= 0), or the other instance precedes this (> 0).
        public int CompareTo(object obj) =>
            Id.CompareTo(((TimeUnitsEnumExtension)obj).Id);

        /// <summary></summary>
        /// @details Implements IComparable interface.
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary></summary>
        /// @details Implements IComparable interface.
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            return !(left == right);
        }

        /// <summary></summary>
        /// @details Implements IComparable interface.
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            return left is null ?
                right is object :
                left.CompareTo(right) < 0;
        }

        /// <summary></summary>
        /// @details Implements IComparable interface.
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <=(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        /// <summary></summary>
        /// @details Implements IComparable interface.
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        /// <summary></summary>
        /// @details Implements IComparable interface.
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(TimeUnitsEnumExtension left, TimeUnitsEnumExtension right)
        {
            return left is null ?
                right is null :
                left.CompareTo(right) >= 0;
        }
    }
}
