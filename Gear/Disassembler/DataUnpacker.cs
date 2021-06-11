/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2020 - Gear Developers
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

namespace Gear.Disassembler
{
    /// <summary>
    /// 
    /// </summary>
    class DataUnpacker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static int GetSignedOffset(MemoryManager memory)
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static short GetSignedPackedOffset(MemoryManager memory)
        {
            short op = memory.ReadByte();
            bool extended = (op & 0x80) != 0;

            op = (short)((op & 0x7F) | ((op << 1) & 0x80));

            if (!extended)
                return (short)(sbyte)op;

            return (short)((op << 8) | memory.ReadByte());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static uint GetPackedLiteral(MemoryManager memory)
        {
            byte op = memory.ReadByte();

            if (op >= 0x80)
                // TODO: COMPLAIN!
                return 0x55555555;

            uint data = (uint)2 << (op & 0x1F);

            if ((op & 0x20) != 0)
                data--;
            if ((op & 0x40) != 0)
                data = ~data;

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static int GetPackedOffset(MemoryManager memory)
        {
            ushort op = memory.ReadByte();

            if ((op & 0x80) == 0)
                return (op & 0x7F);

            op &= 0x7F;

            return (op << 8) | (memory.ReadByte());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int GetLiteral(MemoryManager memory, int count)
        {
            int result = 0;
            for (int i = 0; i < count; i++)
            {
                result <<= 8;
                result |= memory.ReadByte();
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static int GetWordLiteral(MemoryManager memory)
        {
            return GetLiteral(memory, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static int GetNearLongLiteral(MemoryManager memory)
        {
            return GetLiteral(memory, 3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static int GetLongLiteral(MemoryManager memory)
        {
            return GetLiteral(memory, 4);
        }

    }
}
