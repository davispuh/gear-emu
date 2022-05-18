/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * BinarySizeException.cs
 * Binary size helper exception class.
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
    /// <summary>
    /// Binary size helper exception class.
    /// </summary>
    /// @issue{20} Added exception class to fix it.
    /// @version v22.03.01 - Moved to its own file.
    [Serializable]
    public class BinarySizeException : Exception
    {
        /// <summary>Default constructor.</summary>
        public BinarySizeException() { }

        /// <summary>Constructor with a message.</summary>
        /// <param name="message">Message string.</param>
        public BinarySizeException(string message) :
            base(message)
        { }

        /// <summary>Constructor with a message and an inner exception.</summary>
        /// <param name="message">Message string.</param>
        /// <param name="innerException">Inner exception which throw this one.</param>
        public BinarySizeException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>Constructor in a serialization context.</summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        protected BinarySizeException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        { }
    }
}
