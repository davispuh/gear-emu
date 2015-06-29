/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * AssemblyRegisters.CS
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

namespace Gear.Propeller
{
    public partial class Assembly
    {
        /// @brief %Cog RAM Special Purpose Registers.
        /// 
        /// @remark Source: Table 15 - %Cog RAM Special Purpose Registers, %Propeller 
        /// P8X32A Datasheet V1.4.0.
        /// @version v15.03.26 - added NONE to enum, to follow best practices and do not
        /// have an illegal value when is instantiated.
        public enum RegisterAddress : uint
        {
            COGID     = 0x1E9,    //!< Identification number of this cog.
            INITCOGID = 0x1EF,    //!< @todo Document enum value Assembly.RegisterAddress.INITCOGID.
            PAR       = 0x1F0,    //!< Boot Parameter
            CNT       = 0x1F1,    //!< System Counter
            INA       = 0x1F2,    //!< Input States for P31 - P0.
            INB       = 0x1F3,    //!< Input States for P63 - P32.
            OUTA      = 0x1F4,    //!< Output States for P31 - P0.
            OUTB      = 0x1F5,    //!< Output States for P63 - P32.
            DIRA      = 0x1F6,    //!< Direction States for P31 - P0.
            DIRB      = 0x1F7,    //!< Direction States for P63 - P32.
            CNTA      = 0x1F8,    //!< Counter A Control.
            CNTB      = 0x1F9,    //!< Counter B Control.
            FRQA      = 0x1FA,    //!< Counter A Frequency.
            FRQB      = 0x1FB,    //!< Counter B Frequency.
            PHSA      = 0x1FC,    //!< Counter A Phase.
            PHSB      = 0x1FD,    //!< Counter B Phase.
            VCFG      = 0x1FE,    //!< Video Configuration.
            VSCL      = 0x1FF,    //!< Video Scale.
            NONE      = 0x000     //!< None
        }

        public const int RegisterBaseAddress = 0x1F0;

        /// @brief %Cog RAM Special Purpose Registers.
        /// Specify the write and read status of special registers of cog RAM.
        /// Source: Table 15 - %Cog RAM Special Purpose Registers, %Propeller P8X32A Datasheet V1.4.0.
        /// @note PAR address is a special case, because unless Propeller Manual V1.4 
        /// specifications says it is a read-only register, there are claims that in reality it 
        /// is writable as explains 
        /// <a href="http://forums.parallax.com/showthread.php/115909-PASM-simulator-debugger)">
        /// Forum thread "PASM simulator / debugger?</a>.
        /// @par They claims that some parallax video drivers in PASM changes the PAR register, 
        /// and GEAR didn't emulate that.
        static public readonly Register[] Registers = new Register[] {
            //Constructor Register(string Name, bool Read, bool Write)
            new Register("PAR ", true, true),   //  $1F0 - Boot Parameter // PAR register changed to writable
            new Register("CNT ", true, false),  //  $1F1 - System Counter
            new Register("INA ", true, false),  //  $1F2 - Input States for P31-P0.
            new Register("INB ", true, false),  //  $1F3 - Input States for P63-P32.
            new Register("OUTA", true, true ),  //  $1F4 - Output States for P31-P0.
            new Register("OUTB", true, true ),  //  $1F5 - Output States for P63-P32.
            new Register("DIRA", true, true ),  //  $1F6 - Direction States for P31-P0.
            new Register("DIRB", true, true ),  //  $1F7 - Direction States for P63-P32.
            new Register("CTRA", true, true ),  //  $1F8 - Counter A Control.
            new Register("CTRB", true, true ),  //  $1F9 - Counter B Control.
            new Register("FRQA", true, true ),  //  $1FA - Counter A Frequency.
            new Register("FRQB", true, true ),  //  $1FB - Counter B Frequency.
            new Register("PHSA", true, true ),  //  $1FC - Counter A Phase.
            new Register("PHSB", true, true ),  //  $1FD - Counter B Phase.
            new Register("VCFG", true, true ),  //  $1FE - Video Configuration.
            new Register("VSCL", true, true ),  //  $1FF - Video Scale.
        };
    }
}
