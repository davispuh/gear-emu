using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.Disassembler
{
    public partial class Assembly
    {
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
