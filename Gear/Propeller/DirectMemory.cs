/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * DirectMemory.cs
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

using System.Diagnostics;
using Gear.EmulationCore;

namespace Gear.Propeller
{
    /// <summary>Implements memory for Hub main RAM of a PropellerCPU.</summary>
    /// @version v22.09.01 - Added custom debugger text.
    [DebuggerDisplay("{TextForDebugger,nq}")]
    public class DirectMemory
    {
        /// <summary>Array for memory contents.</summary>
        protected byte[] Memory;

        /// <summary>Property to get or set main memory byte value.</summary>
        /// <param name="address">Main memory address.</param>
        /// <returns>Value of memory at address.</returns>
        /// @version v22.08.01 - Parameter name changed to be more meaningful.
        public byte this[int address] =>
            address >= Memory.Length ?
                (byte)0x55 :
                Memory[address];

        /// <summary>Returns a summary text of this class, to be used in
        /// debugger view.</summary>
        /// @version v22.09.01 - Added.
        private string TextForDebugger =>
            $"{{{GetType().FullName}, Length: {(Memory == null ? "[Not initialized]" : Memory.Length.ToString("D"))}}}";

        /// <summary>Read a byte (8 bits) from main memory.</summary>
        /// <param name="address">Main memory address.</param>
        /// <returns>Value of memory at address as Byte.</returns>
        public byte DirectReadByte(uint address) =>
            Memory[address & PropellerCPU.MaxMemoryAddress];

        /// <summary>Read a word (16 bits) from main memory.</summary>
        /// <param name="address">Main memory address, sanitized to be
        /// multiple of 2 bytes.</param>
        /// <returns>Value of memory at address as Word.</returns>
        /// @version v22.04.02 - Removed redundant increment.
        public ushort DirectReadWord(uint address)
        {
            address &= 0xFFFFFFFE;
            return (ushort)(Memory[address++ & PropellerCPU.MaxMemoryAddress] |
                (Memory[address & PropellerCPU.MaxMemoryAddress] << 8));
        }

        /// <summary>Read a long (32 bits) from main memory.</summary>
        /// <param name="address">Main memory address, sanitized to be
        /// multiple of 4 bytes.</param>
        /// <returns>Value of memory at address as Long.</returns>
        /// @version v22.04.02 - Removed redundant increment.
        public uint DirectReadLong(uint address)
        {
            address &= 0xFFFFFFFC;
            return (uint)Memory[address++ & PropellerCPU.MaxMemoryAddress]
                | (uint)(Memory[address++ & PropellerCPU.MaxMemoryAddress] << 8)
                | (uint)(Memory[address++ & PropellerCPU.MaxMemoryAddress] << 16)
                | (uint)(Memory[address & PropellerCPU.MaxMemoryAddress] << 24);
        }

        /// <summary>Write a byte (8 bits) to main memory.</summary>
        /// <param name="address">Main memory address.</param>
        /// <param name="value">Value to set.</param>
        /// @version v22.04.02 - Removed redundant increment.
        public void DirectWriteByte(uint address, byte value)
        {
            if ((address & 0x8000) != 0)
                return;
            Memory[address & 0x7FFF] = value;
        }

        /// <summary>Write a word (16 bits) to main memory.</summary>
        /// <param name="address">Main memory address, sanitized to be
        /// multiple of 2 bytes.</param>
        /// <param name="value">Value to set.</param>
        /// @version v22.04.02 - Removed redundant increment.
        public void DirectWriteWord(uint address, ushort value)
        {
            address &= 0xFFFFFFFE;
            DirectWriteByte(address++, (byte)value);
            DirectWriteByte(address, (byte)(value >> 8));
        }

        /// <summary>Write a long (32 bits) to main memory.</summary>
        /// <param name="address">Main memory address, sanitized to be
        /// multiple of 4 bytes.</param>
        /// <param name="value">Value to set.</param>
        /// @version v22.04.02 - Removed redundant increment.
        public void DirectWriteLong(uint address, uint value)
        {
            address &= 0xFFFFFFFC;
            DirectWriteByte(address++, (byte)value);
            DirectWriteByte(address++, (byte)(value >> 8));
            DirectWriteByte(address++, (byte)(value >> 16));
            DirectWriteByte(address, (byte)(value >> 24));
        }
    }
}
