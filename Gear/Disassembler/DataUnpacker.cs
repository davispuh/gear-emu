/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * DataUnpacker.cs
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
    /// <summary></summary>
    public static class DataUnpacker
    {
        /// <summary></summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.05.03 - Added exception.
        public static int GetSignedOffset(MemoryManager memory)
        {
            if (memory == null)
                throw new ArgumentNullException(nameof(memory));
            uint result = memory.ReadByte();
            if ((result & 0x80) == 0)
            {
                if ((result & 0x40) != 0)
                    result |= 0xFFFFFF80;
            }
            else
            {
                result = (result << 8) | memory.ReadByte();
                if ((result & 0x4000) != 0)
                    result |= 0xFFFF8000;
            }
            return (int)result;
        }

        /// <summary></summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.05.03 - Added exception and changed local variable
        /// name to clarify meaning of it.
        public static short GetSignedPackedOffset(MemoryManager memory)
        {
            if (memory == null)
                throw new ArgumentNullException(nameof(memory));
            short operation = memory.ReadByte();
            bool extended = (operation & 0x80) != 0;
            operation = (short)((operation & 0x7F) | ((operation << 1) & 0x80));
            if (!extended)
                return (sbyte)operation;
            return (short)((operation << 8) | memory.ReadByte());
        }

        /// <summary></summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.05.03 - Added exception and changed local variable
        /// name to clarify meaning of it.
        /// @todo [Legacy] COMPLAIN if (op >= 0x80)
        public static uint GetPackedLiteral(MemoryManager memory)
        {
            if (memory == null)
                throw new ArgumentNullException(nameof(memory));
            byte operation = memory.ReadByte();
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
        /// <param name="memory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.05.03 - Added exception and changed local variable
        /// name to clarify meaning of it.
        public static int GetPackedOffset(MemoryManager memory)
        {
            if (memory == null)
                throw new ArgumentNullException(nameof(memory));
            ushort operation = memory.ReadByte();
            if ((operation & 0x80) == 0)
                return operation & 0x7F;
            operation &= 0x7F;
            return (operation << 8) | memory.ReadByte();
        }

        /// <summary></summary>
        /// <param name="memory"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.05.03 - Added exception.
        public static int GetLiteral(MemoryManager memory, int count)
        {
            if (memory == null)
                throw new ArgumentNullException(nameof(memory));
            int result = 0;
            for (int i = 0; i < count; i++)
            {
                result <<= 8;
                result |= memory.ReadByte();
            }
            return result;
        }

        /// <summary></summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static int GetWordLiteral(MemoryManager memory) =>
            GetLiteral(memory, 2);

        /// <summary></summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static int GetNearLongLiteral(MemoryManager memory) =>
            GetLiteral(memory, 3);

        /// <summary></summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static int GetLongLiteral(MemoryManager memory) =>
            GetLiteral(memory, 4);
    }
}
