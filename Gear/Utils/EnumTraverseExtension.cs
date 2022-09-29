/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * EnumTraverseExtension.cs
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
using System.Diagnostics;

// ReSharper disable ExceptionNotDocumented

namespace Gear.Utils
{
    /// <summary>Global extension to traverse Enums.</summary>
    /// <remarks>To help on detection of duplicates values, both methods try
    /// assertions of this on debug, showing an alert message.<br/>
    /// Based on
    /// https://stackoverflow.com/questions/642542/how-to-get-next-or-previous-enum-value-in-c-sharp </remarks>
    /// @version v22.09.02 - Added.
    public static class EnumTraverseExtension
    {
        /// <summary>Get the next value of enum.</summary>
        /// <remarks>Requirements for Enum values:<br/>
        /// - A sub range of values of the enum can be used for first and last
        /// values.<br/>
        /// - Duplicated values should be avoided.
        /// - Negatives values produce unexpected order changes. Avoid it.<br/>
        /// Allowed characteristics for Enum values:<br/>
        /// - Values don't have to be consecutive: flag enum style is
        /// correctly managed.</remarks>
        /// <typeparam name="TEnum">Enum type to be traverse.</typeparam>
        /// <param name="startValue">Enum value of <typeparamref name="TEnum"/>
        /// to start.</param>
        /// <param name="rollOver">TRUE signals the <paramref name="startValue"/>
        /// is the last in Enum, so the first one is returned, rolling over
        /// the values; FALSE indicates no rolling over.</param>
        /// <returns>The next value, if <paramref name="startValue"/> parameter
        /// is not the last. If it would be the last value of enum, this method
        /// returns the first, signaling this situation in
        /// <paramref name="rollOver"/> parameter this situation.</returns>
        /// <exception cref="ArgumentException">When <typeparamref name="TEnum"/>
        /// type is not an Enum type.</exception>
        public static TEnum EnumNext<TEnum>(this TEnum startValue, out bool rollOver)
            where TEnum : Enum
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException(
                    $"Argument {typeof(TEnum).FullName} is not an Enum.");
            TEnum[] arr = (TEnum[])Enum.GetValues(startValue.GetType());
            int lastIndex = Array.LastIndexOf(arr, startValue) + 1;
#if DEBUG
            int firstIndex = Array.IndexOf(arr, startValue) + 1;
            if (firstIndex != lastIndex)
            {
                string msg = $"First position: {firstIndex - 1:D}\n" +
                             $"Last position : {lastIndex - 1:D}\n";
                Debug.Assert(false,
                    $"Duplicated enum values detected on {typeof(TEnum)} Enum:\n",
                    msg);
            }
#endif
            if (arr.Length == lastIndex)
            {
                rollOver = true;
                return arr[0];
            }
            else
            {
                rollOver = false;
                return arr[lastIndex];
            }
        }

        /// <summary>Get the previous value of enum.</summary>
        /// <remarks>Requirements for Enum values:<br/>
        /// - A sub range of values of the enum can be used for first and last
        /// values.<br/>
        /// - Duplicated values should be avoided.
        /// - Negatives values produce unexpected order changes. Avoid it.<br/>
        /// Allowed characteristics for Enum values:<br/>
        /// - Values don't have to be consecutive: flag enum style is
        /// correctly managed.</remarks>
        /// <typeparam name="TEnum">Enum type to be traverse.</typeparam>
        /// <param name="startValue">Enum value of <typeparamref name="TEnum"/>
        /// to start.</param>
        /// <param name="rollOver">TRUE signals the <paramref name="startValue"/>
        /// is the first in Enum, so the last one is returned, rolling over
        /// the values; FALSE indicates no rolling over.</param>
        /// <returns>The previous value, if <paramref name="startValue"/>
        /// parameter is not the first. If it would be the first value of enum,
        /// this method returns the last, signaling this situation in
        /// <paramref name="rollOver"/> parameter.</returns>
        /// <exception cref="ArgumentException">When <typeparamref name="TEnum"/>
        /// type is not an Enum type.</exception>
        public static TEnum EnumPrev<TEnum>(this TEnum startValue, out bool rollOver)
            where TEnum : Enum
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException(
                    $"Argument {typeof(TEnum).FullName} is not an Enum.");
            TEnum[] arr = (TEnum[])Enum.GetValues(startValue.GetType());
            int firstIndex = Array.IndexOf(arr, startValue) - 1;
#if DEBUG
            int lastIndex = Array.LastIndexOf(arr, startValue) + 1;
            if (firstIndex != lastIndex)
            {
                string msg = $"First position: {firstIndex - 1:D}\n" +
                             $"Last position : {lastIndex - 1:D}\n";
                Debug.Assert(false,
                    $"Duplicated enum values detected on {typeof(TEnum)} Enum:\n",
                    msg);
            }
#endif
            if (firstIndex < 0)
            {
                rollOver = true;
                return arr[arr.Length - 1];
            }
            else
            {
                rollOver = false;
                return arr[firstIndex];
            }
        }
    }
}
