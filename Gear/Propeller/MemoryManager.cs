/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * MemoryManager.cs
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

using Gear.EmulationCore;

namespace Gear.Propeller
{
    /// <summary></summary>
    public class DirectMemory
    {
        /// <summary></summary>
        protected byte[] Memory;

        /// <summary></summary>
        public byte this[int offset] =>
            offset >= Memory.Length ? (byte)0x55 : Memory[offset];

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte DirectReadByte(uint address) =>
            Memory[address & PropellerCPU.MaxRAMAddress];

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        /// @version v22.04.02 - Removed redundant increment.
        public ushort DirectReadWord(uint address)
        {
            address &= 0xFFFFFFFE;
            return (ushort)(Memory[address++ & PropellerCPU.MaxRAMAddress] |
                (Memory[address & PropellerCPU.MaxRAMAddress] << 8));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        /// @version v22.04.02 - Removed redundant increment.
        public uint DirectReadLong(uint address)
        {
            address &= 0xFFFFFFFC;
            return (uint)Memory[address++ & PropellerCPU.MaxRAMAddress]
                | (uint)(Memory[address++ & PropellerCPU.MaxRAMAddress] << 8)
                | (uint)(Memory[address++ & PropellerCPU.MaxRAMAddress] << 16)
                | (uint)(Memory[address & PropellerCPU.MaxRAMAddress] << 24);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// @version v22.04.02 - Removed redundant increment.
        public void DirectWriteByte(uint address, byte value)
        {
            if ((address & 0x8000) != 0)
                return;
            Memory[address & 0x7FFF] = value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// @version v22.04.02 - Removed redundant increment.
        public void DirectWriteWord(uint address, ushort value)
        {
            address &= 0xFFFFFFFE;
            DirectWriteByte(address++, (byte) value);
            DirectWriteByte(address, (byte)(value >> 8));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
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

    /// <summary>
    ///
    /// </summary>
    public class MemoryManager
    {
        /// <summary></summary>
        private readonly DirectMemory _memory;
        /// <summary></summary>
        public uint Address { get; protected set; }

        /// <summary>Default constructor.</summary>
        /// <param name="memory"></param>
        /// <param name="address"></param>
        public MemoryManager(DirectMemory memory, uint address)
        {
            _memory = memory;
            Address = address;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public byte ReadByte() =>
            _memory.DirectReadByte(Address++);

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public ushort ReadWord()
        {
            Address &= 0xFFFFFFFE;
            ushort read = _memory.DirectReadWord(Address);
            Address += 2;
            return read;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public uint ReadLong()
        {
            Address &= 0xFFFFFFFC;
            uint read = _memory.DirectReadLong(Address);
            Address += 4;
            return read;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void WriteByte(byte value)
        {
            if ((Address & 0x8000) != 0)
                return;
            _memory.DirectWriteByte(Address++, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void WriteWord(ushort value)
        {
            Address &= 0xFFFFFFFE;
            _memory.DirectWriteWord(Address, value);
            Address += 2;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void WriteLong(uint value)
        {
            Address &= 0xFFFFFFFC;
            _memory.DirectWriteLong(Address, value);
            Address += 4;
        }
    }
}
