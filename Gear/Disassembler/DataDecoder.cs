/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * DataDecoder.cs
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

using Gear.Propeller;
using System;

namespace Gear.Disassembler
{
    /// <summary>Methods to decoder SPIN Data from main memory.</summary>
    /// @version v22.08.01 - Changed class name from former `DataUnpacker`.
    public static class DataDecoder
    {
        /// <summary></summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory segment.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.08.01 - Changed parameter name to clarify meaning of it.
        public static int GetSignedOffset(MemorySegment memorySegment)
        {
            if (memorySegment == null)
                throw new ArgumentNullException(nameof(memorySegment));
            uint result = memorySegment.ReadByte();
            if ((result & 0x80) == 0)
            {
                if ((result & 0x40) != 0)
                    result |= 0xFFFFFF80;
            }
            else
            {
                result = (result << 8) | memorySegment.ReadByte();
                if ((result & 0x4000) != 0)
                    result |= 0xFFFF8000;
            }
            return (int)result;
        }

        /// <summary></summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory segment.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.08.01 - Changed parameter name to clarify meaning of it.
        public static short GetSignedPackedOffset(MemorySegment memorySegment)
        {
            if (memorySegment == null)
                throw new ArgumentNullException(nameof(memorySegment));
            short operation = memorySegment.ReadByte();
            bool extended = (operation & 0x80) != 0;
            operation = (short)((operation & 0x7F) | ((operation << 1) & 0x80));
            if (!extended)
                return (sbyte)operation;
            return (short)((operation << 8) | memorySegment.ReadByte());
        }

        /// <summary></summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory segment.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.08.01 - Changed parameter name to clarify meaning of it.
        /// @todo [Legacy] COMPLAIN if (op >= 0x80)
        public static uint GetPackedLiteral(MemorySegment memorySegment)
        {
            if (memorySegment == null)
                throw new ArgumentNullException(nameof(memorySegment));
            byte operation = memorySegment.ReadByte();
            if (operation >= 0x80)
                // TODO: COMPLAIN!
                return 0x55555555;
            uint data = (uint)2 << (operation & 0x1F);
            if ((operation & 0x20) != 0)
                data--;
            if ((operation & 0x40) != 0)
                data = ~data;
            return data;
        }

        /// <summary></summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory segment.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.08.01 - Changed parameter name to clarify meaning of it.
        public static int GetPackedOffset(MemorySegment memorySegment)
        {
            if (memorySegment == null)
                throw new ArgumentNullException(nameof(memorySegment));
            ushort operation = memorySegment.ReadByte();
            if ((operation & 0x80) == 0)
                return operation & 0x7F;
            operation &= 0x7F;
            return (operation << 8) | memorySegment.ReadByte();
        }

        /// <summary></summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory segment.</param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.08.01 - Changed parameter name to clarify meaning of
        /// it. Changed visibility to `private`.
        private static int GetLiteral(MemorySegment memorySegment, int count)
        {
            if (memorySegment == null)
                throw new ArgumentNullException(nameof(memorySegment));
            int result = 0;
            for (int i = 0; i < count; i++)
            {
                result <<= 8;
                result |= memorySegment.ReadByte();
            }
            return result;
        }

        /// <summary></summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory segment.</param>
        /// <returns></returns>
        /// @version v22.08.01 - Changed parameter name to clarify meaning of it.
        public static int GetWordLiteral(MemorySegment memorySegment) =>
            GetLiteral(memorySegment, 2);

        /// <summary></summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory segment.</param>
        /// <returns></returns>
        /// @version v22.08.01 - Changed parameter name to clarify meaning of it.
        public static int GetNearLongLiteral(MemorySegment memorySegment) =>
            GetLiteral(memorySegment, 3);

        /// <summary></summary>
        /// <param name="memorySegment">SPIN bytecode value(s), contained in a
        /// memory segment.</param>
        /// <returns></returns>
        /// @version v22.08.01 - Changed parameter name to clarify meaning of it.
        public static int GetLongLiteral(MemorySegment memorySegment) =>
            GetLiteral(memorySegment, 4);
    }
}
