﻿/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * NumberFormatEnumExtension.cs
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

using System.Globalization;

// ReSharper disable InvalidXmlDocComment

/// <summary>Utilities classes.</summary>
/// @version v20.09.01 - Added.
namespace Gear.Utils
{
    /// <summary>Available Number Formats.</summary>
    /// @version v20.09.01 - Added.
    public enum NumberFormatEnum : byte
    {
        /// <summary>No format (old default).</summary>
        None = 0,
        /// <summary>Format given from system default.</summary>
        SystemDefault,
        /// <summary>Using '_' as separator.</summary>
        ParallaxSpin
    }

    /// <summary>Provides number separators formats for clock and counters.</summary>
    /// @version v20.09.01 - Added.
    public static class NumberFormatEnumExtension
    {
        /// <summary></summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NumberFormatInfo GetFormatInfo(NumberFormatEnum value)
        {
           NumberFormatInfo retVal =
                (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            switch (value)
            {
                default:
                case NumberFormatEnum.None:
                    retVal.NumberGroupSeparator = string.Empty;
                    break;
                case NumberFormatEnum.SystemDefault:
                    break;
                case NumberFormatEnum.ParallaxSpin:
                    retVal.NumberGroupSeparator = "_";
                    break;
            }
            return retVal;
        }
    }
}
