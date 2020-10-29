/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2020 - Gear Developers
 * --------------------------------------------------------------------------------
 * NumberFormatExt.cs
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

/// @brief Utilities classes.
/// @since v20.09.01 - Added.
namespace Gear.Utils
{
    /// @brief Available Number Formats.
    /// @since v20.09.01 - Added.
    public enum NumberFormatEnum
    {
        /// @brief No format (old default).
        None = 0,
        /// @brief Format given from system default.
        System_Default,
        /// @brief Using '_' as separator.
        Parallax_SPIN
    }

    /// @brief Provides number separators formats for clock and counters.
    /// @since v20.09.01 - Added.
    public static class NumberFormatEnumExtension
    {
        public static NumberFormatInfo GetFormatInfo(NumberFormatEnum value)
        {
           NumberFormatInfo retVal = 
                (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            switch (value)
            {
                case NumberFormatEnum.None:
                    retVal.NumberGroupSeparator = string.Empty;
                    break;
                case NumberFormatEnum.System_Default:
                    break;
                case NumberFormatEnum.Parallax_SPIN:
                    retVal.NumberGroupSeparator = "_";
                    break;
            }
            return retVal;
        }
    }
}
