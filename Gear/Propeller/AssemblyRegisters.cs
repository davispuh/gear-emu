/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * AssemblyRegisters.cs
 * Registers of a Propeller Cog.
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

using System.Collections.ObjectModel;

// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace Gear.Propeller
{
    /// @brief Registers of a Propeller Cog.
    public partial class Assembly
    {
        /// @brief %Cog RAM Special Purpose Registers.
        /// @details Source: Table 15 - %Cog RAM Special Purpose Registers,
        /// <c>%Propeller P8X32A Datasheet V1.4.0</c>.
        /// @version v15.03.26 - added NONE to enum, to follow best practices
        /// and do not have an illegal value when is instantiated.
        public enum RegisterAddress : uint
        {
            /// <summary>None</summary>
            NONE = 0x000,
            /// <summary>Identification number of this cog.</summary>
            COGID = 0x1E9,
            /// <summary></summary>
            INITCOGID = 0x1EF,
            /// <summary>Boot Parameter</summary>
            PAR = 0x1F0,
            /// <summary>System Counter</summary>
            CNT = 0x1F1,
            /// <summary>Input States for P31 - P0.</summary>
            INA = 0x1F2,
            /// <summary>Input States for P63 - P32.</summary>
            INB = 0x1F3,
            /// <summary>Output States for P31 - P0.</summary>
            OUTA = 0x1F4,
            /// <summary>Output States for P63 - P32.</summary>
            OUTB = 0x1F5,
            /// <summary>Direction States for P31 - P0.</summary>
            DIRA = 0x1F6,
            /// <summary>Direction States for P63 - P32.</summary>
            DIRB = 0x1F7,
            /// <summary>Counter A Control.</summary>
            CNTA = 0x1F8,
            /// <summary>Counter B Control.</summary>
            CNTB = 0x1F9,
            /// <summary>Counter A Frequency.</summary>
            FRQA = 0x1FA,
            /// <summary>Counter B Frequency.</summary>
            FRQB = 0x1FB,
            /// <summary>Counter A Phase.</summary>
            PHSA = 0x1FC,
            /// <summary>Counter B Phase.</summary>
            PHSB = 0x1FD,
            /// <summary>Video Configuration.</summary>
            VCFG = 0x1FE,
            /// <summary>Video Scale.</summary>
            VSCL = 0x1FF
        }

        /// <summary></summary>
        public const int RegisterBaseAddress = 0x1F0;

        /// @brief Declaration of P1 PASM %Cog RAM Special Purpose Registers.
        /// @details Specify the write and read status of special registers
        /// of cog RAM.
        ///
        /// Source: Table 15 - %Cog RAM Special Purpose Registers,
        /// <c>%Propeller P8X32A Datasheet V1.4.0</c>.
        /// |Name|Code|Description|
        /// |:--:|---:|:----------|
        /// |PAR |$1F0|Boot Parameter.|
        /// |CNT |$1F1|System Counter.|
        /// |INA |$1F2|Input States for P32-P1.|
        /// |INB |$1F3|Input States for P64-P33.|
        /// |OUTA|$1F4|Output States for P31-P1.|
        /// |OUTB|$1F5|Output States for P64-P33.|
        /// |DIRA|$1F6|Direction States for P32-P1.|
        /// |DIRB|$1F7|Direction States for P64-P32.|
        /// |CTRA|$1F8|Counter A Control.|
        /// |CTRB|$1F9|Counter B Control.|
        /// |FRQA|$1FA|Counter A Frequency.|
        /// |FRQB|$1FB|Counter B Frequency.|
        /// |PHSA|$1FC|Counter A Phase.|
        /// |PHSB|$1FD|Counter B Phase.|
        /// |VCFG|$1FE|Video Configuration.|
        /// |VSCL|$1FF|Video Scale.|
        ///
        /// @note <c>PAR</c> address is a special case, because unless
        /// <c>%Propeller Manual V1.4</c> specifications says it is a read-only
        /// register, there is evidence that in reality it is writable as explains
        /// <a href="https://forums.parallax.com/discussion/115909/PASM-simulator-debugger">
        /// Forum thread "PASM simulator / debugger?</a>.
        /// They show that some parallax video drivers in PASM changes the <c>PAR</c> register,
        /// and GEAR didn't emulate that.
        /// @version v22.05.01 - Fix security warning CA2105: 'Array fields
        /// should not be read only', adopting ReadOnlyCollection static
        /// object as storage.
        public static readonly ReadOnlyCollection<Register> Registers =
            new ReadOnlyCollection<Register>(new[]
            {
                //Constructor Register(string Name, bool Read, bool Write)
                new Register("PAR ", true, true), //  $1F0 - Boot Parameter // PAR register changed to writable
                new Register("CNT ", true, false), //  $1F1 - System Counter
                new Register("INA ", true, false), //  $1F2 - Input States for P31-P0.
                new Register("INB ", true, false), //  $1F3 - Input States for P63-P32.
                new Register("OUTA", true, true), //  $1F4 - Output States for P31-P0.
                new Register("OUTB", true, true), //  $1F5 - Output States for P63-P32.
                new Register("DIRA", true, true), //  $1F6 - Direction States for P31-P0.
                new Register("DIRB", true, true), //  $1F7 - Direction States for P63-P32.
                new Register("CTRA", true, true), //  $1F8 - Counter A Control.
                new Register("CTRB", true, true), //  $1F9 - Counter B Control.
                new Register("FRQA", true, true), //  $1FA - Counter A Frequency.
                new Register("FRQB", true, true), //  $1FB - Counter B Frequency.
                new Register("PHSA", true, true), //  $1FC - Counter A Phase.
                new Register("PHSB", true, true), //  $1FD - Counter B Phase.
                new Register("VCFG", true, true), //  $1FE - Video Configuration.
                new Register("VSCL", true, true), //  $1FF - Video Scale.
            });
    }
}
