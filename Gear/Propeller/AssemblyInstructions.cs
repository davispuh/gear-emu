using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Propeller
{
    public partial class Assembly
    {
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

        static public readonly Instruction[] Instructions = new Instruction[] {
            new Instruction(InstructionType.WR,     new SubInstruction[] {              //       0
                new SubInstruction("RDBYTE ", true, true, true, true, false, false),    //  000000 001i 1111 ddddddddd sssssssss
                new SubInstruction("WRBYTE ", true, true, true, true, false, false)     //  000000 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.WR,     new SubInstruction[] {              //       1
                new SubInstruction("RDWORD ", true, true, true, true, false, false),    //  000001 001i 1111 ddddddddd sssssssss
                new SubInstruction("WRWORD ", true, true, true, true, false, false)     //  000001 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.WR,     new SubInstruction[] {              //       2
                new SubInstruction("RDLONG ", true, true, true, true, false, false),    //  000010 001i 1111 ddddddddd sssssssss
                new SubInstruction("WRLONG ", true, true, true, true, false, false)     //  000010 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Hub,    new SubInstruction[] {              //       3
                new SubInstruction("CLKSET ", true, false, true, true, true,  false),   //  000011 0001 1111 ddddddddd ------000
                new SubInstruction("COGID  ", true, false, true, true, false, false),   //  000011 0011 1111 ddddddddd ------001
                new SubInstruction("COGINIT", true, false, true, true, true,  false),   //  000011 0001 1111 ddddddddd ------010
                new SubInstruction("COGSTOP", true, false, true, true, true,  false),   //  000011 0001 1111 ddddddddd ------011
                new SubInstruction("LOCKNEW", true, false, true, true, false, false),   //  000011 0011 1111 ddddddddd ------100
                new SubInstruction("LOCKRET", true, false, true, true, true,  false),   //  000011 0001 1111 ddddddddd ------101
                new SubInstruction("LOCKSET", true, false, true, true, true,  false),   //  000011 0001 1111 ddddddddd ------110
                new SubInstruction("LOCKCLR", true, false, true, true, true,  false),   //  000011 0001 1111 ddddddddd ------111
                new SubInstruction("HUBOP  ", true, true,  true, true, false, false)    //  000011 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //       4
                new SubInstruction("MUL    ", true, true, true, true, false, false)     //  000100 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //       5
                new SubInstruction("MULS   ", true, true, true, true, false, false)     //  000101 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //       6
                new SubInstruction("ENC    ", true, true, true, true, false, false)     //  000110 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //       7
                new SubInstruction("ONES   ", true, true, true, true, false, false)     //  000111 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //       8
                new SubInstruction("ROR    ", true, true, true, true, false, false)     //  001000 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //       9
                new SubInstruction("ROL    ", true, true, true, true, false, false)     //  001001 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      10
                new SubInstruction("SHR    ", true, true, true, true, false, false)     //  001010 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      11
                new SubInstruction("SHL    ", true, true, true, true, false, false)     //  001011 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      12
                new SubInstruction("RCR    ", true, true, true, true, false, false)     //  001100 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      13
                new SubInstruction("RCL    ", true, true, true, true, false, false)     //  001101 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      14
                new SubInstruction("SAR    ", true, true, true, true, false, false)     //  001110 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      15
                new SubInstruction("REV    ", true, true, true, true, false, false)     //  001111 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      16
                new SubInstruction("MINS   ", true, true, true, true, false, false)     //  010000 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      17
                new SubInstruction("MAXS   ", true, true, true, true, false, false)     //  010001 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      18
                new SubInstruction("MIN    ", true, true, true, true, false, false)     //  010010 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      19
                new SubInstruction("MAX    ", true, true, true, true, false, false)     //  010011 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      20
                new SubInstruction("MOVS   ", true, true, true, true, false, false)     //  010100 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      21
                new SubInstruction("MOVD   ", true, true, true, true, false, false)     //  010101 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      22
                new SubInstruction("MOVI   ", true, true, true, true, false, false)     //  010110 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Jump,   new SubInstruction[] {              //      23
                new SubInstruction("JMP    ", false, true,  true, true, true,  false),  //  010111 000i 1111 --------- sssssssss
                new SubInstruction("RET    ", false, false, true, true, false, false),  //  010111 0001 1111 --------- ---------
                new SubInstruction("JMPRET ", true,  true,  true, true, false, false),  //  010111 001i 1111 ddddddddd sssssssss
                new SubInstruction("CALL   ", false, true,  true, true, false, false)   //  010111 0011 1111 ????????? sssssssss
            }),
            new Instruction(InstructionType.WR,     new SubInstruction[] {              //      24
                new SubInstruction("AND    ", true, true, true, true, false, false),    //  011000 001i 1111 ddddddddd sssssssss
                new SubInstruction("TEST   ", true, true, true, true, true,  false)     //  011000 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.WR,     new SubInstruction[] {              //      25
                new SubInstruction("ANDN   ", true, true, true, true, false, false),    //  011001 001i 1111 ddddddddd sssssssss
                new SubInstruction("TESTN  ", true, true, true, true, true,  false)     //  011001 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      26
                new SubInstruction("OR     ", true, true, true, true, false, false)     //  011010 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      27
                new SubInstruction("XOR    ", true, true, true, true, false, false)     //  011011 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      28
                new SubInstruction("MUXC   ", true, true, true, true, false, false)     //  011100 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      29
                new SubInstruction("MUXNC  ", true, true, true, true, false, false)     //  011101 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      30
                new SubInstruction("MUXZ   ", true, true, true, true, false, false)     //  011110 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      31
                new SubInstruction("MUXNZ  ", true, true, true, true, false, false)     //  011111 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      32
                new SubInstruction("ADD    ", true, true, true, true, false, false)     //  100000 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.WR,     new SubInstruction[] {              //      33
                new SubInstruction("SUB    ", true, true, true, true, false, false),    //  100001 001i 1111 ddddddddd sssssssss
                new SubInstruction("CMP    ", true, true, true, true, true,  false)     //  100001 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      34
                new SubInstruction("ADDABS ", true, true, true, true, false, false)     //  100010 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      35
                new SubInstruction("SUBABS ", true, true, true, true, false, false)     //  100011 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      36
                new SubInstruction("SUMC   ", true, true, true, true, false, false)     //  100100 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      37
                new SubInstruction("SUMNC  ", true, true, true, true, false, false)     //  100101 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      38
                new SubInstruction("SUMZ   ", true, true, true, true, false, false)     //  100110 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      39
                new SubInstruction("SUMNZ  ", true, true, true, true, false, false)     //  100111 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      40
                new SubInstruction("MOV    ", true, true, true, true, false, false)     //  101000 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      41
                new SubInstruction("NEG    ", true, true, true, true, false, false)     //  101001 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      42
                new SubInstruction("ABS    ", true, true, true, true, false, false)     //  101010 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      43
                new SubInstruction("ABSNEG ", true, true, true, true, false, false)     //  101011 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      44
                new SubInstruction("NEGC   ", true, true, true, true, false, false)     //  101100 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      45
                new SubInstruction("NEGNC  ", true, true, true, true, false, false)     //  101101 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      46
                new SubInstruction("NEGZ   ", true, true, true, true, false, false)     //  101110 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      47
                new SubInstruction("NEGNZ  ", true, true, true, true, false, false)     //  101111 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      48
                new SubInstruction("CMPS   ", true, true, true, true, true,  false)     //  110000 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      49
                new SubInstruction("CMPSX  ", true, true, true, true, true,  false)     //  110001 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      50
                new SubInstruction("ADDX   ", true, true, true, true, false, false)     //  110010 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.WR,     new SubInstruction[] {              //      51
                new SubInstruction("SUBX   ", true, true, true, true, false, false),    //  110011 001i 1111 ddddddddd sssssssss
                new SubInstruction("CMPX   ", true, true, true, true, true,  false)     //  110011 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      52
                new SubInstruction("ADDS   ", true, true, true, true, false, false)     //  110100 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      53
                new SubInstruction("SUBS   ", true, true, true, true, false, false)     //  110101 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      54
                new SubInstruction("ADDSX  ", true, true, true, true, false, false)     //  110110 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      55
                new SubInstruction("SUBSX  ", true, true, true, true, false, false)     //  110111 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      56
                new SubInstruction("CMPSUB ", true, true, true, true, false, false)     //  111000 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      57
                new SubInstruction("DJNZ   ", true, true, true, true, false, false)     //  111001 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      58
                new SubInstruction("TJNZ   ", true, true, true, true, true,  false)     //  111010 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      59
                new SubInstruction("TJZ    ", true, true, true, true, true,  false)     //  111011 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      60
                new SubInstruction("WAITPEQ", true, true, true, true, true,  false)     //  111100 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      61
                new SubInstruction("WAITPNE", true, true, true, true, true,  false)     //  111101 000i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      62
                new SubInstruction("WAITCNT", true, true, true, true, false, false)     //  111110 001i 1111 ddddddddd sssssssss
            }),
            new Instruction(InstructionType.Normal, new SubInstruction[] {              //      63
                new SubInstruction("WAITVID", true, true, true, true, true,  false)     //  111111 000i 1111 ddddddddd sssssssss
            })
        };
    }
}
