/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * Propeller.cs
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

// ReSharper disable InvalidXmlDocComment

/// <summary>%Propeller P1 Definitions.</summary>
namespace Gear.Propeller
{
    /// <summary>%Register base class.</summary>
    public class Register
    {
        /// <summary>Name of register</summary>
        public string Name { get; private protected set; }
    }

    /// <summary>Basic instruction base class.</summary>
    public class BasicInstruction
    {
        /// <summary>Full name of instruction.</summary>
        public string Name { get; private protected set; }
        /// <summary>Brief name of instruction.</summary>
        public string NameBrief { get; private protected set; }
    }
}
