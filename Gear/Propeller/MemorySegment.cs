/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * MemorySegment.cs
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

namespace Gear.Propeller
{
    /// <summary>Management class for a memory segment with address
    /// auto advance.</summary>
    /// @version v22.08.01 - Changed class name to clarify meaning of it from former `MemoryManager`, moving to its own file.
    public class MemorySegment
    {
        /// <summary>Reference to main memory.</summary>
        private readonly DirectMemory _memory;

        /// <summary>Address pointer property.</summary>
        /// @version v22.08.01 - Changed visibility to a private setter.
        public uint Address { get; private set; }

        /// <summary>Default constructor.</summary>
        /// <param name="memory">Memory segment to manage.</param>
        /// <param name="address">Starting address.</param>
        /// @version v22.08.01 - Renamed class constructor to follow class
        /// name was changed.
        public MemorySegment(DirectMemory memory, uint address)
        {
            _memory = memory;
            Address = address;
        }

        /// <summary>Advance the address like was reading a byte from the
        /// segment.</summary>
        /// @version v22.09.01 - Added.
        public void AdvanceByte()
        {
            Address++;
        }

        /// <summary>Advance the address like was reading a word from the
        /// segment.</summary>
        /// @version v22.09.01 - Added.
        public void AdvanceWord()
        {
            Address += 2;
        }

        /// <summary>Advance the address like was reading a long from the
        /// segment.</summary>
        /// @version v22.09.01 - Added.
        public void AdvanceLong()
        {
            Address += 4;
        }

        /// <summary>Read a byte (8 bits) from managed segment memory, with
        /// auto advance address.</summary>
        /// <returns>Value of memory at address as Byte.</returns>
        public byte ReadByte() =>
            _memory.DirectReadByte(Address++);

        /// <summary>Read a word (16 bits) from managed segment memory, with
        /// auto advance address and sanitized to be multiple of 2 bytes.</summary>
        /// <returns>Value of memory at address as Word.</returns>
        public ushort ReadWord()
        {
            Address &= 0xFFFFFFFE;
            ushort read = _memory.DirectReadWord(Address);
            Address += 2;
            return read;
        }

        /// <summary>Read a long (32 bits) from managed segment memory, with
        /// auto advance address and sanitized to be multiple of 4 bytes.</summary>
        /// <returns>Value of memory at address as Long.</returns>
        public uint ReadLong()
        {
            Address &= 0xFFFFFFFC;
            uint read = _memory.DirectReadLong(Address);
            Address += 4;
            return read;
        }

        /// <summary>Write a byte (8 bits) to managed segment memory, with
        /// auto advance address.</summary>
        /// <param name="value">Value to set.</param>
        public void WriteByte(byte value)
        {
            if ((Address & 0x8000) != 0)
                return;
            _memory.DirectWriteByte(Address++, value);
        }

        /// <summary>Write a word (16 bits) to managed segment memory, with
        /// auto advance address and sanitized to be multiple of 2 bytes.</summary>
        /// <param name="value">Value to set.</param>
        public void WriteWord(ushort value)
        {
            Address &= 0xFFFFFFFE;
            _memory.DirectWriteWord(Address, value);
            Address += 2;
        }

        /// <summary>Write a long (32 bits) to managed segment memory, with
        /// auto advance address and sanitized to be multiple of 4 bytes.</summary>
        /// <param name="value">Value to set.</param>
        public void WriteLong(uint value)
        {
            Address &= 0xFFFFFFFC;
            _memory.DirectWriteLong(Address, value);
            Address += 4;
        }
    }
}
