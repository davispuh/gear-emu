/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * SpinRegisters.cs
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
    public partial class Spin
    {
        static public readonly Register[] Registers = new Register[] {
            new Register("MEM_0"),
            new Register("MEM_1"),
            new Register("MEM_2"),
            new Register("MEM_3"),
            new Register("MEM_4"),
            new Register("MEM_5"),
            new Register("MEM_6"),
            new Register("MEM_7"),
            new Register("MEM_8"),
            new Register("MEM_9"),
            new Register("MEM_A"),
            new Register("MEM_B"),
            new Register("MEM_C"),
            new Register("MEM_D"),
            new Register("MEM_E"),
            new Register("MEM_F")
        };
    }
}
