/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * AssemblyInstructions.cs
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

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Gear.Propeller
{
    public partial class Assembly
    {
        /// <summary>P1 PASM Instruction codes.</summary>
        public enum InstructionCodes : uint
        {
            RWBYTE  = 0x00000000,
            RWWORD  = 0x04000000,
            RWLONG  = 0x08000000,
            HUBOP   = 0x0C000000,
            MUL     = 0x10000000,
            MULS    = 0x14000000,
            ENC     = 0x18000000,
            ONES    = 0x1C000000,
            ROR     = 0x20000000,
            ROL     = 0x24000000,
            SHR     = 0x28000000,
            SHL     = 0x2C000000,
            RCR     = 0x30000000,
            RCL     = 0x34000000,
            SAR     = 0x38000000,
            REV     = 0x3C000000,
            MINS    = 0x40000000,
            MAXS    = 0x44000000,
            MIN     = 0x48000000,
            MAX     = 0x4C000000,
            MOVS    = 0x50000000,
            MOVD    = 0x54000000,
            MOVI    = 0x58000000,
            JMPRET  = 0x5C000000,
            AND     = 0x60000000,
            ANDN    = 0x64000000,
            OR      = 0x68000000,
            XOR     = 0x6C000000,
            MUXC    = 0x70000000,
            MUXNC   = 0x74000000,
            MUXZ    = 0x78000000,
            MUXNZ   = 0x7C000000,
            ADD     = 0x80000000,
            SUB     = 0x84000000,
            ADDABS  = 0x88000000,
            SUBABS  = 0x8C000000,
            SUMC    = 0x90000000,
            SUMNC   = 0x94000000,
            SUMZ    = 0x98000000,
            SUMNZ   = 0x9C000000,
            MOV     = 0xA0000000,
            NEG     = 0xA4000000,
            ABS     = 0xA8000000,
            ABSNEG  = 0xAC000000,
            NEGC    = 0xB0000000,
            NEGNC   = 0xB4000000,
            NEGZ    = 0xB8000000,
            NEGNZ   = 0xBC000000,
            CMPS    = 0xC0000000,
            CMPSX   = 0xC4000000,
            ADDX    = 0xC8000000,
            SUBX    = 0xCC000000,
            ADDS    = 0xD0000000,
            SUBS    = 0xD4000000,
            ADDSX   = 0xD8000000,
            SUBSX   = 0xDC000000,
            CMPSUB  = 0xE0000000,
            DJNZ    = 0xE4000000,
            TJNZ    = 0xE8000000,
            TJZ     = 0xEC000000,
            WAITPEQ = 0xF0000000,
            WAITPNE = 0xF4000000,
            WAITCNT = 0xF8000000,
            WAITVID = 0xFC000000
        }

        /// <summary>Declaration of P1 PASM %Instructions, with its
        /// instructions variants.</summary>
        /// @version v22.08.01 - Changed class name for instruction variants,
        /// formerly `SubInstructions`.
        public static readonly ReadOnlyCollection<Instruction> Instructions =
            new ReadOnlyCollection<Instruction>(new[]
            {
                new Instruction(InstructionTypeEnum.ReadWrite,     new[] {       //       0
                    new InstructionVariant("RDBYTE ", true, true, true, true, false, false, "000000 001i 1111 ddddddddd sssssssss"),
                    new InstructionVariant("WRBYTE ", true, true, true, true, false, false, "000000 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.ReadWrite,     new[] {       //       1
                    new InstructionVariant("RDWORD ", true, true, true, true, false, false, "000001 001i 1111 ddddddddd sssssssss"),
                    new InstructionVariant("WRWORD ", true, true, true, true, false, false, "000001 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.ReadWrite,     new[] {       //       2
                    new InstructionVariant("RDLONG ", true, true, true, true, false, false, "000010 001i 1111 ddddddddd sssssssss"),
                    new InstructionVariant("WRLONG ", true, true, true, true, false, false, "000010 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Hub,    new[] {              //       3
                    new InstructionVariant("CLKSET ", true, false, true, true, true,  false, "000011 0001 1111 ddddddddd ------000"),
                    new InstructionVariant("COGID  ", true, false, true, true, false, false, "000011 0011 1111 ddddddddd ------001"),
                    new InstructionVariant("COGINIT", true, false, true, true, true,  false, "000011 0001 1111 ddddddddd ------010"),
                    new InstructionVariant("COGSTOP", true, false, true, true, true,  false, "000011 0001 1111 ddddddddd ------011"),
                    new InstructionVariant("LOCKNEW", true, false, true, true, false, false, "000011 0011 1111 ddddddddd ------100"),
                    new InstructionVariant("LOCKRET", true, false, true, true, true,  false, "000011 0001 1111 ddddddddd ------101"),
                    new InstructionVariant("LOCKSET", true, false, true, true, true,  false, "000011 0001 1111 ddddddddd ------110"),
                    new InstructionVariant("LOCKCLR", true, false, true, true, true,  false, "000011 0001 1111 ddddddddd ------111"),
                    new InstructionVariant("HUBOP  ", true, true,  true, true, false, false, "000011 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //       4
                    new InstructionVariant("MUL    ", true, true, true, true, false, false, "000100 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //       5
                    new InstructionVariant("MULS   ", true, true, true, true, false, false, "000101 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //       6
                    new InstructionVariant("ENC    ", true, true, true, true, false, false, "000110 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //       7
                    new InstructionVariant("ONES   ", true, true, true, true, false, false, "000111 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //       8
                    new InstructionVariant("ROR    ", true, true, true, true, false, false, "001000 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //       9
                    new InstructionVariant("ROL    ", true, true, true, true, false, false, "001001 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      10
                    new InstructionVariant("SHR    ", true, true, true, true, false, false, "001010 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      11
                    new InstructionVariant("SHL    ", true, true, true, true, false, false, "001011 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      12
                    new InstructionVariant("RCR    ", true, true, true, true, false, false, "001100 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      13
                    new InstructionVariant("RCL    ", true, true, true, true, false, false, "001101 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      14
                    new InstructionVariant("SAR    ", true, true, true, true, false, false, "001110 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      15
                    new InstructionVariant("REV    ", true, true, true, true, false, false, "001111 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      16
                    new InstructionVariant("MINS   ", true, true, true, true, false, false, "010000 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      17
                    new InstructionVariant("MAXS   ", true, true, true, true, false, false, "010001 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      18
                    new InstructionVariant("MIN    ", true, true, true, true, false, false, "010010 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      19
                    new InstructionVariant("MAX    ", true, true, true, true, false, false, "010011 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      20
                    new InstructionVariant("MOVS   ", true, true, true, true, false, false, "010100 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      21
                    new InstructionVariant("MOVD   ", true, true, true, true, false, false, "010101 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      22
                    new InstructionVariant("MOVI   ", true, true, true, true, false, false, "010110 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Jump,   new[] {              //      23
                    new InstructionVariant("JMP    ", false, true,  true, true, true,  false, "010111 000i 1111 --------- sssssssss"),
                    new InstructionVariant("RET    ", false, false, true, true, false, false, "010111 0001 1111 --------- ---------"),
                    new InstructionVariant("JMPRET ", true,  true,  true, true, false, false, "010111 001i 1111 ddddddddd sssssssss"),
                    new InstructionVariant("CALL   ", false, true,  true, true, false, false, "010111 0011 1111 ????????? sssssssss")
                }),
                new Instruction(InstructionTypeEnum.ReadWrite,     new[] {       //      24
                    new InstructionVariant("AND    ", true, true, true, true, false, false, "011000 001i 1111 ddddddddd sssssssss"),
                    new InstructionVariant("TEST   ", true, true, true, true, true,  false, "011000 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.ReadWrite,     new[] {       //      25
                    new InstructionVariant("ANDN   ", true, true, true, true, false, false, "011001 001i 1111 ddddddddd sssssssss"),
                    new InstructionVariant("TESTN  ", true, true, true, true, true,  false, "011001 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      26
                    new InstructionVariant("OR     ", true, true, true, true, false, false, "011010 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      27
                    new InstructionVariant("XOR    ", true, true, true, true, false, false, "011011 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      28
                    new InstructionVariant("MUXC   ", true, true, true, true, false, false, "011100 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      29
                    new InstructionVariant("MUXNC  ", true, true, true, true, false, false, "011101 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      30
                    new InstructionVariant("MUXZ   ", true, true, true, true, false, false, "011110 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      31
                    new InstructionVariant("MUXNZ  ", true, true, true, true, false, false, "011111 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      32
                    new InstructionVariant("ADD    ", true, true, true, true, false, false, "100000 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.ReadWrite,     new[] {       //      33
                    new InstructionVariant("SUB    ", true, true, true, true, false, false, "100001 001i 1111 ddddddddd sssssssss"),
                    new InstructionVariant("CMP    ", true, true, true, true, true,  false, "100001 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      34
                    new InstructionVariant("ADDABS ", true, true, true, true, false, false, "100010 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      35
                    new InstructionVariant("SUBABS ", true, true, true, true, false, false, "100011 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      36
                    new InstructionVariant("SUMC   ", true, true, true, true, false, false, "100100 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      37
                    new InstructionVariant("SUMNC  ", true, true, true, true, false, false, "100101 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      38
                    new InstructionVariant("SUMZ   ", true, true, true, true, false, false, "100110 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      39
                    new InstructionVariant("SUMNZ  ", true, true, true, true, false, false, "100111 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      40
                    new InstructionVariant("MOV    ", true, true, true, true, false, false, "101000 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      41
                    new InstructionVariant("NEG    ", true, true, true, true, false, false, "101001 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      42
                    new InstructionVariant("ABS    ", true, true, true, true, false, false, "101010 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      43
                    new InstructionVariant("ABSNEG ", true, true, true, true, false, false, "101011 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      44
                    new InstructionVariant("NEGC   ", true, true, true, true, false, false, "101100 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      45
                    new InstructionVariant("NEGNC  ", true, true, true, true, false, false, "101101 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      46
                    new InstructionVariant("NEGZ   ", true, true, true, true, false, false, "101110 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      47
                    new InstructionVariant("NEGNZ  ", true, true, true, true, false, false, "101111 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      48
                    new InstructionVariant("CMPS   ", true, true, true, true, true,  false, "110000 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      49
                    new InstructionVariant("CMPSX  ", true, true, true, true, true,  false, "110001 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      50
                    new InstructionVariant("ADDX   ", true, true, true, true, false, false, "110010 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.ReadWrite,     new[] {       //      51
                    new InstructionVariant("SUBX   ", true, true, true, true, false, false, "110011 001i 1111 ddddddddd sssssssss"),
                    new InstructionVariant("CMPX   ", true, true, true, true, true,  false, "110011 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      52
                    new InstructionVariant("ADDS   ", true, true, true, true, false, false, "110100 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      53
                    new InstructionVariant("SUBS   ", true, true, true, true, false, false, "110101 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      54
                    new InstructionVariant("ADDSX  ", true, true, true, true, false, false, "110110 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      55
                    new InstructionVariant("SUBSX  ", true, true, true, true, false, false, "110111 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      56
                    new InstructionVariant("CMPSUB ", true, true, true, true, false, false, "111000 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      57
                    new InstructionVariant("DJNZ   ", true, true, true, true, false, false, "111001 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      58
                    new InstructionVariant("TJNZ   ", true, true, true, true, true,  false, "111010 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      59
                    new InstructionVariant("TJZ    ", true, true, true, true, true,  false, "111011 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      60
                    new InstructionVariant("WAITPEQ", true, true, true, true, true,  false, "111100 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      61
                    new InstructionVariant("WAITPNE", true, true, true, true, true,  false, "111101 000i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      62
                    new InstructionVariant("WAITCNT", true, true, true, true, false, false, "111110 001i 1111 ddddddddd sssssssss")
                }),
                new Instruction(InstructionTypeEnum.Normal, new[] {              //      63
                    new InstructionVariant("WAITVID", true, true, true, true, true,  false, "111111 000i 1111 ddddddddd sssssssss")
                })
            });
    }
}
