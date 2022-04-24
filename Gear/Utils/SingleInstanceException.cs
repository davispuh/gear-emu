/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * SingleInstanceException.cs
 * Single instance helper exception class.
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

namespace Gear.Utils
{
    /// @brief Single instance helper exception class.
    /// @version v20.10.01 - Added exception class to control a single
    /// instance form.
    [Serializable]
    public class SingleInstanceException : Exception
    {
        /// <summary>
        ///
        /// </summary>
        public SingleInstanceException() :
            base()
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        public SingleInstanceException(string message) :
            base(message)
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public SingleInstanceException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        protected SingleInstanceException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        { }
    }

}
