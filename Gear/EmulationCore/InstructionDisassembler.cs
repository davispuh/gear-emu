/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * InstructionDisassembler.cs
 * Provides a method for creating the string equlivilent of a propeller operation
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

using System;
using System.Collections.Generic;
using System.Text;
using Gear.Disassembler;

namespace Gear.EmulationCore
{
    public static class InstructionDisassembler
    {
        private enum ArguementMode
        {
            None,
            Effect,
            SignedOffset,
            PackedLiteral,
            UnsignedOffset,
            UnsignedEffectedOffset,
            ByteLiteral,
            WordLiteral,
            NearLongLiteral,
            LongLiteral,
            ObjCallPair,
            MemoryOpCode
        }

        static private string[] InterpretedOps;
        static private Dictionary<byte, string> EffectCodes;
        static private string[] BriefInterpretedOps;
        static private Dictionary<byte, string> BriefEffectCodes;
        static private string[] BigInterpretedOps;
        static private Dictionary<byte, string> BigEffectCodes;
        static private ArguementMode[] ArguementModes;

        static InstructionDisassembler()
        {
            BigInterpretedOps = new string[] {
              // 0x00
	            "FRAME_CALL_RETURN",
	            "FRAME_CALL_NORETURN",
	            "FRAME_CALL_ABORT",
	            "FRAME_CALL_TRASHABORT",
	            "BRANCH",
	            "CALL",
	            "OBJCALL",
	            "OBJCALL_INDEXED",
              // 0x08
	            "LOOP_START",
	            "LOOP_CONTINUE",
	            "JUMP_IF_FALSE",
	            "JUMP_IF_TRUE",
	            "JUMP_FROM_STACK",
	            "COMPARE_CASE",
	            "COMPARE_CASE_RANGE",
	            "LOOK_ABORT",
	            // 0x10
              "LOOKUP_COMPARE",
	            "LOOKDOWN_COMPARE",
	            "LOOKUPRANGE_COMPARE",
	            "LOOKDOWNRANGE_COMPARE",
	            "QUIT",
	            "MARK_INTERPRETED",
	            "STRSIZE",
	            "STRCOMP",
	            // 0x18
              "BYTEFILL",
	            "WORDFILL",
	            "LONGFILL",
	            "WAITPEQ",
	            "BYTEMOVE",
	            "WORDMOVE",
	            "LONGMOVE",
	            "WAITPNE",
              // 0x20
	            "CLKSET",
	            "COGSTOP",
	            "LOCKRET",
	            "WAITCNT",
	            "READ_INDEXED_SPR",
	            "WRITE_INDEXED_SPR",
	            "EFFECT_INDEXED_SPR",
	            "WAITVID",
              // 0x28
	            "COGINIT_RETURNS",
	            "LOCKNEW_RETURNS",
	            "LOCKSET_RETURNS",
	            "LOCKCLR_RETURNS",
	            "COGINIT",
	            "LOCKNEW",
	            "LOCKSET",
	            "LOCKCLR",
              // 0x30
	            "ABORT",
	            "ABORT_WITH_RETURN",
	            "RETURN",
	            "POP_RETURN",
	            "PUSH_NEG1",
	            "PUSH_0",
	            "PUSH_1",
	            "PUSH_PACKED_LIT",
              // 0x38
	            "PUSH_BYTE_LIT",
	            "PUSH_WORD_LIT",
	            "PUSH_MID_LIT",
	            "PUSH_LONG_LIT",
	            "UNKNOWN OP $3C",
	            "INDEXED_MEM_OP",
	            "INDEXED_RANGE_MEM_OP",
	            "MEMORY_OP",
              // 0x40
	            "PUSH_VARMEM_LONG_0",
	            "POP_VARMEM_LONG_0",
	            "EFFECT_VARMEM_LONG_0",
	            "REFERENCE_VARMEM_LONG_0",
	            "PUSH_VARMEM_LONG_1",
	            "POP_VARMEM_LONG_1",
	            "EFFECT_VARMEM_LONG_1",
	            "REFERENCE_VARMEM_LONG_1",
	            // 0x48
              "PUSH_VARMEM_LONG_2",
	            "POP_VARMEM_LONG_2",
	            "EFFECT_VARMEM_LONG_2",
	            "REFERENCE_VARMEM_LONG_2",
	            "PUSH_VARMEM_LONG_3",
	            "POP_VARMEM_LONG_3",
	            "EFFECT_VARMEM_LONG_3",
	            "REFERENCE_VARMEM_LONG_3",
              // 0x50
	            "PUSH_VARMEM_LONG_4",
	            "POP_VARMEM_LONG_4",
	            "EFFECT_VARMEM_LONG_4",
	            "REFERENCE_VARMEM_LONG_4",
	            "PUSH_VARMEM_LONG_5",
	            "POP_VARMEM_LONG_5",
	            "EFFECT_VARMEM_LONG_5",
	            "REFERENCE_VARMEM_LONG_5",
              // 0x58
	            "PUSH_VARMEM_LONG_6",
	            "POP_VARMEM_LONG_6",
	            "EFFECT_VARMEM_LONG_6",
	            "REFERENCE_VARMEM_LONG_6",
	            "PUSH_VARMEM_LONG_7",
	            "POP_VARMEM_LONG_7",
	            "EFFECT_VARMEM_LONG_7",
	            "REFERENCE_VARMEM_LONG_7",
              // 0x60
	            "PUSH_LOCALMEM_LONG_0",
	            "POP_LOCALMEM_LONG_0",
	            "EFFECT_LOCALMEM_LONG_0",
	            "REFERENCE_LOCALMEM_LONG_0",
	            "PUSH_LOCALMEM_LONG_1",
	            "POP_LOCALMEM_LONG_1",
	            "EFFECT_LOCALMEM_LONG_1",
	            "REFERENCE_LOCALMEM_LONG_1",
              // 0x68
	            "PUSH_LOCALMEM_LONG_2",
	            "POP_LOCALMEM_LONG_2",
	            "EFFECT_LOCALMEM_LONG_2",
	            "REFERENCE_LOCALMEM_LONG_2",
	            "PUSH_LOCALMEM_LONG_3",
	            "POP_LOCALMEM_LONG_3",
	            "EFFECT_LOCALMEM_LONG_3",
	            "REFERENCE_LOCALMEM_LONG_3",
              // 0x70
	            "PUSH_LOCALMEM_LONG_4",
	            "POP_LOCALMEM_LONG_4",
	            "EFFECT_LOCALMEM_LONG_4",
	            "REFERENCE_LOCALMEM_LONG_4",
	            "PUSH_LOCALMEM_LONG_5",
	            "POP_LOCALMEM_LONG_5",
	            "EFFECT_LOCALMEM_LONG_5",
	            "REFERENCE_LOCALMEM_LONG_5",
              // 0x78
	            "PUSH_LOCALMEM_LONG_6",
	            "POP_LOCALMEM_LONG_6",
	            "EFFECT_LOCALMEM_LONG_6",
	            "REFERENCE_LOCALMEM_LONG_6",
	            "PUSH_LOCALMEM_LONG_7",
	            "POP_LOCALMEM_LONG_7",
	            "EFFECT_LOCALMEM_LONG_7",
	            "REFERENCE_LOCALMEM_LONG_7",
              // 0x80
	            "PUSH_MAINMEM_BYTE",
	            "POP_MAINMEM_BYTE",
	            "EFFECT_MAINMEM_BYTE",
	            "REFERENCE_MAINMEM_BYTE",
	            "PUSH_OBJECTMEM_BYTE",
	            "POP_OBJECTMEM_BYTE",
	            "EFFECT_OBJECTMEM_BYTE",
	            "REFERENCE_OBJECTMEM_BYTE",
              // 0x88
	            "PUSH_VARIABLEMEM_BYTE",
	            "POP_VARIABLEMEM_BYTE",
	            "EFFECT_VARIABLEMEM_BYTE",
	            "REFERENCE_VARIABLEMEM_BYTE",
	            "PUSH_LOCALMEM_BYTE",
	            "POP_LOCALMEM_BYTE",
	            "EFFECT_LOCALMEM_BYTE",
	            "REFERENCE_LOCALMEM_BYTE",
              // 0x90
	            "PUSH_INDEXED_MAINMEM_BYTE",
	            "POP_INDEXED_MAINMEM_BYTE",
	            "EFFECT_INDEXED_MAINMEM_BYTE",
	            "REFERENCE_INDEXED_MAINMEM_BYTE",
	            "PUSH_INDEXED_OBJECTMEM_BYTE",
	            "POP_INDEXED_OBJECTMEM_BYTE",
	            "EFFECT_INDEXED_OBJECTMEM_BYTE",
	            "REFERENCE_INDEXED_OBJECTMEM_BYTE",
              // 0x98
	            "PUSH_INDEXED_VARIABLEMEM_BYTE",
	            "POP_INDEXED_VARIABLEMEM_BYTE",
	            "EFFECT_INDEXED_VARIABLEMEM_BYTE",
	            "REFERENCE_INDEXED_VARIABLEMEM_BYTE",
	            "PUSH_INDEXED_LOCALMEM_BYTE",
	            "POP_INDEXED_LOCALMEM_BYTE",
	            "EFFECT_INDEXED_LOCALMEM_BYTE",
	            "REFERENCE_INDEXED_LOCALMEM_BYTE",
              // 0xA0
	            "PUSH_MAINMEM_WORD",
	            "POP_MAINMEM_WORD",
	            "EFFECT_MAINMEM_WORD",
	            "REFERENCE_MAINMEM_WORD",
	            "PUSH_OBJECTMEM_WORD",
	            "POP_OBJECTMEM_WORD",
	            "EFFECT_OBJECTMEM_WORD",
	            "REFERENCE_OBJECTMEM_WORD",
              // 0xA8
	            "PUSH_VARIABLEMEM_WORD",
	            "POP_VARIABLEMEM_WORD",
	            "EFFECT_VARIABLEMEM_WORD",
	            "REFERENCE_VARIABLEMEM_WORD",
	            "PUSH_LOCALMEM_WORD",
	            "POP_LOCALMEM_WORD",
	            "EFFECT_LOCALMEM_WORD",
	            "REFERENCE_LOCALMEM_WORD",
              // 0xB0
	            "PUSH_INDEXED_MAINMEM_WORD",
	            "POP_INDEXED_MAINMEM_WORD",
	            "EFFECT_INDEXED_MAINMEM_WORD",
	            "REFERENCE_INDEXED_MAINMEM_WORD",
	            "PUSH_INDEXED_OBJECTMEM_WORD",
	            "POP_INDEXED_OBJECTMEM_WORD",
	            "EFFECT_INDEXED_OBJECTMEM_WORD",
	            "REFERENCE_INDEXED_OBJECTMEM_WORD",
              // 0xB8
	            "PUSH_INDEXED_VARIABLEMEM_WORD",
	            "POP_INDEXED_VARIABLEMEM_WORD",
	            "EFFECT_INDEXED_VARIABLEMEM_WORD",
	            "REFERENCE_INDEXED_VARIABLEMEM_WORD",
	            "PUSH_INDEXED_LOCALMEM_WORD",
	            "POP_INDEXED_LOCALMEM_WORD",
	            "EFFECT_INDEXED_LOCALMEM_WORD",
	            "REFERENCE_INDEXED_LOCALMEM_WORD",
              // 0xC0
	            "PUSH_MAINMEM_LONG",
	            "POP_MAINMEM_LONG",
	            "EFFECT_MAINMEM_LONG",
	            "REFERENCE_MAINMEM_LONG",
	            "PUSH_OBJECTMEM_LONG",
	            "POP_OBJECTMEM_LONG",
	            "EFFECT_OBJECTMEM_LONG",
	            "REFERENCE_OBJECTMEM_LONG",
              // 0xC8
	            "PUSH_VARIABLEMEM_LONG",
	            "POP_VARIABLEMEM_LONG",
	            "EFFECT_VARIABLEMEM_LONG",
	            "REFERENCE_VARIABLEMEM_LONG",
	            "PUSH_LOCALMEM_LONG",
	            "POP_LOCALMEM_LONG",
	            "EFFECT_LOCALMEM_LONG",
	            "REFERENCE_LOCALMEM_LONG",
              // 0xD0
	            "PUSH_INDEXED_MAINMEM_LONG",
	            "POP_INDEXED_MAINMEM_LONG",
	            "EFFECT_INDEXED_MAINMEM_LONG",
	            "REFERENCE_INDEXED_MAINMEM_LONG",
	            "PUSH_INDEXED_OBJECTMEM_LONG",
	            "POP_INDEXED_OBJECTMEM_LONG",
	            "EFFECT_INDEXED_OBJECTMEM_LONG",
	            "REFERENCE_INDEXED_OBJECTMEM_LONG",
              // 0xD8
	            "PUSH_INDEXED_VARIABLEMEM_LONG",
	            "POP_INDEXED_VARIABLEMEM_LONG",
	            "EFFECT_INDEXED_VARIABLEMEM_LONG",
	            "REFERENCE_INDEXED_VARIABLEMEM_LONG",
	            "PUSH_INDEXED_LOCALMEM_LONG",
	            "POP_INDEXED_LOCALMEM_LONG",
	            "EFFECT_INDEXED_LOCALMEM_LONG",
	            "REFERENCE_INDEXED_LOCALMEM_LONG",
              // 0xE0
	            "ROTATE_RIGHT",
	            "ROTATE_LEFT",
	            "SHIFT_RIGHT",
	            "SHIFT_LEFT",
	            "LIMIT_MIN",
	            "LIMIT_MAX",
	            "NEGATE",
	            "COMPLEMENT",
              // 0xE8
	            "BIT_AND",
	            "ABSOLUTE_VALUE",
	            "BIT_OR",
	            "BIT_XOR",
	            "ADD",
	            "SUBTRACT",
	            "ARITH_SHIFT_RIGHT",
	            "BIT_REVERSE",
              // 0xF0
	            "LOGICAL_AND",
	            "ENCODE",
	            "LOGICAL_OR",
	            "DECODE",
	            "MULTIPLY",
	            "MULTIPLY_HI",
	            "DIVIDE",
	            "MODULO",
              // 0xF8
	            "SQUARE_ROOT",
	            "LESS",
	            "GREATER",
	            "NOT_EQUAL",
	            "EQUAL",
	            "LESS_EQUAL",
	            "GREATER_EQUAL",
	            "LOGICAL_NOT"
            };

            BriefInterpretedOps = new string[] {
	            "FrameReturn",
	            "FrameNoReturn",
	            "FrameAbort",
	            "FrameTrashAbort",
	            "Branch",
	            "Call",
	            "ObjCall",
	            "ObjCall[]",
	            "LoopStart",
	            "LoopContinue",
	            "JumpIfFalse",
	            "JumpIfTrue",
	            "JumpFromStack",
	            "Case ==",
	            "CaseRange",
	            "LookAbort",
	            "LookUpOne",
	            "LookDownOne",
	            "LookUpRange",
	            "LookDownRange",
	            "Quit",
	            "MarkInterpreted",
	            "StrSize",
	            "StrComp",
	            "ByteFill",
	            "WordFill",
	            "LongFill",
	            "WaitPEQ",
	            "ByteMove",
	            "WordMove",
	            "LongMove",
	            "WaitPNE",
	            "CLKSET",
	            "CogStop",
	            "LockRet",
	            "WaitCNT",
	            "RdLong SPR[]",
	            "WrLong SPR[]",
	            "RdWrLong SPR[]",
	            "WaitVID",
	            "CogInitReturns",
	            "LockNewReturns",
	            "LockSetReturns",
	            "LockClearReturns",
	            "CogInit",
	            "LockNew",
	            "LockSet",
	            "LockClear",
	            "Abort",
	            "AbortWithResult",
	            "Return",
	            "ReturnWithResult",
	            "# -1",
	            "# 0",
	            "# 1",
	            "#",
	            "#",
	            "#",
	            "#",
	            "#",
	            "UNKNOWN OP $3C",
	            "INDEXED_MEM_OP",
	            "INDEXED_RANGE_MEM_OP",
	            "MEMORY_OP",
	            "Var[0]@",
	            "Var[0]!",
	            "Var[0] with",
	            "&Var[0]",
	            "Var[1]@",
	            "Var[1]!",
	            "Var[1] with",
	            "&Var[1]",
	            "Var[2]@",
	            "Var[2]!",
	            "Var[2] with",
	            "&Var[2]",
	            "Var[3]@",
	            "Var[3]!",
	            "Var[3] with",
	            "&Var[3]",
	            "Var[4]@",
	            "Var[4]!",
	            "Var[4] with",
	            "&Var[4]",
	            "Var[5]@",
	            "Var[5]!",
	            "Var[5] with",
	            "&Var[5]",
	            "Var[6]@",
	            "Var[6]!",
	            "Var[6] with",
	            "&Var[6]",
	            "Var[7]@",
	            "Var[7]!",
	            "Var[7] with",
	            "&Var[7]",
	            "Loc[0]@",
	            "Loc[0]!",
	            "Loc[0] with",
	            "&Loc[0]",
	            "Loc[1]@",
	            "Loc[1]!",
	            "Loc[1] with",
	            "&Loc[1]",
	            "Loc[2]@",
	            "Loc[2]!",
	            "Loc[2] with",
	            "&Loc[2]",
	            "Loc[3]@",
	            "Loc[3]!",
	            "Loc[3] with",
	            "&Loc[3]",
	            "Loc[4]@",
	            "Loc[4]!",
	            "Loc[4] with",
	            "&Loc[4]",
	            "Loc[5]@",
	            "Loc[5]!",
	            "Loc[5] with",
	            "&Loc[5]",
	            "Loc[6]@",
	            "Loc[6]!",
	            "Loc[6] with",
	            "&Loc[6]",
	            "Loc[7]@",
	            "Loc[7]!",
	            "Loc[7] with",
	            "&Loc[7]",
	            "byte Mem[]@",
	            "byte Mem[]!",
	            "byte Mem[] with",
	            "byte &Mem[]",
	            "byte Obj[]@",
	            "byte Obj[]!",
	            "byte Obj[] with",
	            "byte &Obj[]",
	            "byte Var[]@",
	            "byte Var[]!",
	            "byte Var[] with",
	            "byte &Var[]",
	            "byte Loc[]@",
	            "byte Loc[]!",
	            "byte Loc[] then",
	            "byte &Loc[]",
	            "byte Mem[idx]@",
	            "byte Mem[idx]!",
	            "byte Mem[idx] with",
	            "byte &Mem[idx]",
	            "byte Obj[idx]@",
	            "byte Obj[idx]!",
	            "byte Obj[idx] with",
	            "byte &Obj[idx]",
	            "byte Var[idx]@",
	            "byte Var[idx]!",
	            "byte Var[idx] with",
	            "byte &Var[idx]",
	            "byte Loc[idx]@",
	            "byte Loc[idx]!",
	            "byte Loc[idx] with",
	            "byte &Loc[idx]",
	            "word Mem[]@",
	            "word Mem[]!",
	            "word Mem[] with",
	            "word &Mem[]",
	            "word Obj[]@",
	            "word Obj[]!",
	            "word Obj[] with",
	            "word &Obj[]",
	            "word Var[]@",
	            "word Var[]!",
	            "word Var[] with",
	            "word &Var[]",
	            "word Loc[]@",
	            "word Loc[]!",
	            "word Loc[] with",
	            "word &Loc[]",
	            "word Mem[idx]@",
	            "word Mem[idx]!",
	            "word Mem[idx] with",
	            "word &Mem[idx]",
	            "word Obj[idx]@",
	            "word Obj[idx]!",
	            "word Obj[idx] with",
	            "word &Obj[idx]",
	            "word Var[idx]@",
	            "word Var[idx]!",
	            "word Var[idx] with",
	            "word &Var[idx]",
	            "word Loc[idx]@",
	            "word Loc[idx]!",
	            "word Loc[idx] with",
	            "word &Loc[idx]",
	            "Mem[]@",
	            "Mem[]!",
	            "Mem[] with",
	            "&Mem[]",
	            "Obj[]@",
	            "Obj[]!",
	            "Obj[] with",
	            "&Obj[]",
	            "Var[]@",
	            "Var[]!",
	            "Var[] with",
	            "&Var[]",
	            "Loc[]@",
	            "Loc[]!",
	            "Loc[] with",
	            "&Loc[]",
	            "Mem[idx]@",
	            "Mem[idx]!",
	            "Mem[idx] with",
	            "&Mem[idx]",
	            "Obj[idx]@",
	            "Obj[idx]!",
	            "Obj[idx] with",
	            "&Obj[idx]",
	            "Var[idx]@",
	            "Var[idx]!",
	            "Var[idx] with",
	            "&Var[idx]",
	            "Loc[idx]@",
	            "Loc[idx]!",
	            "Loc[idx] with",
	            "&Loc[idx]",
	            "a->b",
	            "a<-b",
	            "a>>b",
	            "a<<b",
	            "a#>b",
	            "a#<b",
	            "-a",
	            "~a",
	            "a&b",
	            "||a",
	            "a|b",
	            "a^b",
	            "a+b",
	            "a-b",
	            "a~>b",
	            "a><b",
	            "a and b",
	            ">|a",
	            "a or b",
	            "|<a",
	            "a*b",
	            "(a*b)>>32",
	            "a/b",
	            "a mod b",
	            "sqrt(a)",
	            "a<b",
	            "a>b",
	            "a<>b",
	            "a==b",
	            "a=<b",
	            "a=>b",
	            "not a"
            };

            InterpretedOps = BigInterpretedOps;

            ArguementModes = new ArguementMode[] {
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.SignedOffset,
	            ArguementMode.ByteLiteral,
	            ArguementMode.ObjCallPair,
	            ArguementMode.ObjCallPair,
	            ArguementMode.SignedOffset,
	            ArguementMode.SignedOffset,
	            ArguementMode.SignedOffset,
	            ArguementMode.SignedOffset,
	            ArguementMode.None,
	            ArguementMode.SignedOffset,
	            ArguementMode.SignedOffset,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.PackedLiteral,
	            ArguementMode.ByteLiteral,
	            ArguementMode.WordLiteral,
	            ArguementMode.NearLongLiteral,
	            ArguementMode.LongLiteral,
	            ArguementMode.None,
	            ArguementMode.MemoryOpCode,
	            ArguementMode.MemoryOpCode,
	            ArguementMode.MemoryOpCode,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.Effect,
	            ArguementMode.None,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.UnsignedEffectedOffset,
	            ArguementMode.UnsignedOffset,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None,
	            ArguementMode.None
            };

            BigEffectCodes = new Dictionary<byte, string>();

            BigEffectCodes[0x0] = "COPY";
            BigEffectCodes[0x8] = "FORWARD_RANDOM";
            BigEffectCodes[0xC] = "REVERSE_RANDOM";
            BigEffectCodes[0x10] = "EXTEND_8";
            BigEffectCodes[0x14] = "EXTEND_16";
            BigEffectCodes[0x18] = "BIT_CLEAR";
            BigEffectCodes[0x1C] = "BIT_SET";
            BigEffectCodes[0x20] = "PRE_INCREMENT_BITS";
            BigEffectCodes[0x22] = "PRE_INCREMENT_BYTE";
            BigEffectCodes[0x24] = "PRE_INCREMENT_WORD";
            BigEffectCodes[0x26] = "PRE_INCREMENT_LONG";
            BigEffectCodes[0x28] = "POST_INCREMENT_BITS";
            BigEffectCodes[0x2A] = "POST_INCREMENT_BYTE";
            BigEffectCodes[0x2C] = "POST_INCREMENT_WORD";
            BigEffectCodes[0x2E] = "POST_INCREMENT_LONG";
            BigEffectCodes[0x30] = "PRE_DECREMENT_BITS";
            BigEffectCodes[0x32] = "PRE_DECREMENT_BYTE";
            BigEffectCodes[0x34] = "PRE_DECREMENT_WORD";
            BigEffectCodes[0x36] = "PRE_DECREMENT_LONG";
            BigEffectCodes[0x38] = "POST_DECREMENT_BITS";
            BigEffectCodes[0x3A] = "POST_DECREMENT_BYTE";
            BigEffectCodes[0x3C] = "POST_DECREMENT_WORD";
            BigEffectCodes[0x3E] = "POST_DECREMENT_LONG";
            BigEffectCodes[0x40] = "ROTATE_RIGHT";
            BigEffectCodes[0x41] = "ROTATE_LEFT";
            BigEffectCodes[0x42] = "SHIFT_RIGHT";
            BigEffectCodes[0x43] = "SHIFT_LEFT";
            BigEffectCodes[0x44] = "MINIMUM";
            BigEffectCodes[0x45] = "MAXIMUM";
            BigEffectCodes[0x46] = "NEGATE";
            BigEffectCodes[0x47] = "COMPLEMENT";
            BigEffectCodes[0x48] = "BIT_AND";
            BigEffectCodes[0x49] = "ABSOLUTE_VALUE";
            BigEffectCodes[0x4A] = "BIT_OR";
            BigEffectCodes[0x4B] = "BIT_XOR";
            BigEffectCodes[0x4C] = "ADD";
            BigEffectCodes[0x4D] = "SUBTRACT";
            BigEffectCodes[0x4E] = "ARITH_SHIFT_RIGHT";
            BigEffectCodes[0x4F] = "BIT_REVERSE";
            BigEffectCodes[0x50] = "LOGICAL_AND";
            BigEffectCodes[0x51] = "ENCODE";
            BigEffectCodes[0x52] = "LOGICAL_OR";
            BigEffectCodes[0x53] = "DECODE";
            BigEffectCodes[0x54] = "MULTIPLY";
            BigEffectCodes[0x55] = "MULTIPLY_HI";
            BigEffectCodes[0x56] = "DIVIDE";
            BigEffectCodes[0x57] = "MODULO";
            BigEffectCodes[0x58] = "SQUARE_ROOT";
            BigEffectCodes[0x59] = "LESS";
            BigEffectCodes[0x5A] = "GREATER";
            BigEffectCodes[0x5B] = "NOT_EQUAL";
            BigEffectCodes[0x5C] = "EQUAL";
            BigEffectCodes[0x5D] = "LESS_EQUAL";
            BigEffectCodes[0x5E] = "GREATER_EQUAL";
            BigEffectCodes[0x5F] = "NOT";

            BriefEffectCodes = new Dictionary<byte, string>();

            BriefEffectCodes[0x0] = "(dup)";
            BriefEffectCodes[0x8] = "(a=?a)";
            BriefEffectCodes[0xC] = "(a=a?)";
            BriefEffectCodes[0x10] = "(a=8<<a)";
            BriefEffectCodes[0x14] = "(a=16<<a)";
            BriefEffectCodes[0x18] = "(a=~)";
            BriefEffectCodes[0x1C] = "(a=~~)";
            BriefEffectCodes[0x20] = "(++bits)";
            BriefEffectCodes[0x22] = "(++byte)";
            BriefEffectCodes[0x24] = "(++word)";
            BriefEffectCodes[0x26] = "(++long)";
            BriefEffectCodes[0x28] = "(bits++)";
            BriefEffectCodes[0x2A] = "(byte++)";
            BriefEffectCodes[0x2C] = "(word++)";
            BriefEffectCodes[0x2E] = "(long++)";
            BriefEffectCodes[0x30] = "(--bits)";
            BriefEffectCodes[0x32] = "(--byte)";
            BriefEffectCodes[0x34] = "(--word)";
            BriefEffectCodes[0x36] = "(--long)";
            BriefEffectCodes[0x38] = "(bits--)";
            BriefEffectCodes[0x3A] = "(byte--)";
            BriefEffectCodes[0x3C] = "(word--)";
            BriefEffectCodes[0x3E] = "(long--)";
            BriefEffectCodes[0x40] = "(b=b->a)";
            BriefEffectCodes[0x41] = "(b=b<-a)";
            BriefEffectCodes[0x42] = "(b=b>>a)";
            BriefEffectCodes[0x43] = "(b=b<<a)";
            BriefEffectCodes[0x44] = "(b=min(b,a))";
            BriefEffectCodes[0x45] = "(b=max(b,a))";
            BriefEffectCodes[0x46] = "(b=-b)";
            BriefEffectCodes[0x47] = "(b=inv(b))";
            BriefEffectCodes[0x48] = "(b&=a)";
            BriefEffectCodes[0x49] = "(a=||a)";
            BriefEffectCodes[0x4A] = "(b|=a)";
            BriefEffectCodes[0x4B] = "(b^=a)";
            BriefEffectCodes[0x4C] = "(b+=a)";
            BriefEffectCodes[0x4D] = "(b-=a)";
            BriefEffectCodes[0x4E] = "(b=b~>a)";
            BriefEffectCodes[0x4F] = "(b=b><a)";
            BriefEffectCodes[0x50] = "(b=b AND a)";
            BriefEffectCodes[0x51] = "(a=>|a)";
            BriefEffectCodes[0x52] = "(b=b OR a)";
            BriefEffectCodes[0x53] = "(a=|<a)";
            BriefEffectCodes[0x54] = "(b*=a)";
            BriefEffectCodes[0x55] = "(b=(b*a)>>32";
            BriefEffectCodes[0x56] = "(b/=a)";
            BriefEffectCodes[0x57] = "(b=b mod a)";
            BriefEffectCodes[0x58] = "(a=^^a)";
            BriefEffectCodes[0x59] = "(b<=a)";
            BriefEffectCodes[0x5A] = "(b>=a)";
            BriefEffectCodes[0x5B] = "(b<>=a)";
            BriefEffectCodes[0x5C] = "(b===a)";
            BriefEffectCodes[0x5D] = "(b=<=a)";
            BriefEffectCodes[0x5E] = "(b=>=a)";
            BriefEffectCodes[0x5F] = "(a=!a)";

            EffectCodes = BigEffectCodes;
        }

        public static string AssemblyText(uint Operation)
        {
            Assembly.ParsedInstruction instr = new Assembly.ParsedInstruction(Operation);
            string text;
            if (instr.CON > 0x00)
            {
                Assembly.SubInstruction ActualInstruction = instr.GetSubInstruction();

                string SrcString = "";
                string DestString = "";

                if (ActualInstruction.Source)
                {
                    if (instr.SRC >= Assembly.RegisterBaseAddress)
                    {
                        SrcString = String.Format("{0}{1}",
                                instr.ImmediateValue() ? "#" : "",
                                Assembly.Registers[instr.SRC - Assembly.RegisterBaseAddress].Name);
                    }
                    else
                    {
                        SrcString = String.Format("{0}${1:X3}",
                                instr.ImmediateValue() ? "#" : "",
                                instr.SRC);
                    }
                }

                if (ActualInstruction.Destination)
                {
                    if (instr.DEST >= Assembly.RegisterBaseAddress)
                    {
                        DestString = String.Format("{0}", Assembly.Registers[instr.DEST - Assembly.RegisterBaseAddress].Name);
                    }
                    else
                    {
                        DestString = String.Format("${0:X3}", instr.DEST);
                    }
                }

                text = String.Format("{0} {1} {2}{3}{4}", new object[] {
                    Assembly.Conditions[instr.CON][0],
                    ActualInstruction.Name,
                    DestString,
                    (ActualInstruction.Source && ActualInstruction.Destination) ? ", " : "",
                    SrcString }
                );

                if (instr.WriteResult())
                {
                    text += " WR";
                }

                if (instr.NoResult())
                {
                    text += " NR";
                }

                if (instr.WriteZero())
                {
                    text += " WZ";
                }

                if (instr.WriteCarry())
                {
                    text += " WC";
                }
            }
            else
            {
                text = String.Format("{0} {1}", new object[] { Assembly.Conditions[0][1], Assembly.Conditions[0][2] });
            }
            return text;
        }

        private static uint GetPackedLiteral(Propeller chip, ref uint address)
        {
            byte op = chip.ReadByte(address++);

            if (op >= 0x80)
                // TODO: COMPLAIN!
                return 0x55555555;

            uint data = (uint)2 << (op & 0x1F);

            if ((op & 0x20) != 0)
                data--;
            if ((op & 0x40) != 0)
                data = ~data;

            return data;
        }

        private static int GetPackedOffset(Propeller chip, ref uint address)
        {
            ushort op = chip.ReadByte(address++);

            if ((op & 0x80) == 0)
                return (op & 0x7F);

            op &= 0x7F;

            return (op << 8) | (chip.ReadByte(address++));
        }

        private static short GetSignedPackedOffset(Propeller chip, ref uint address)
        {
            short op = chip.ReadByte(address++);
            bool extended = (op & 0x80) != 0;

            op = (short)((op & 0x7F) | ((op << 1) & 0x80));

            if (!extended)
                return (short)(sbyte)op;

            return (short)((op << 8) | chip.ReadByte(address++));
        }

        private static string GetEffectCode(Propeller chip, ref uint address)
        {
            byte op = chip.ReadByte(address++);
            string effect = ((op & 0x80) == 0) ? "POP " : "";
            op &= 0x7F;

            if (op == 0x02)
            {
                effect += String.Format("REPEAT_COMPARE {0}", GetSignedPackedOffset(chip, ref address));
            }
            else if (op == 0x06)
            {
                effect += String.Format("REPEAT_COMPARE_STEP {0}", GetSignedPackedOffset(chip, ref address));
            }
            else if (EffectCodes.ContainsKey(op))
            {
                effect += EffectCodes[op];
            }
            else
            {
                effect += String.Format("UNKNOWN_EFFECT_{0:X}", op);
            }

            return effect;
        }

        public static string GetMemoryOp(Propeller chip, ref uint address)
        {
            Spin.ParsedMemoryOperation OP = new Spin.ParsedMemoryOperation(chip.ReadByte(address++));

            string Name = OP.GetRegister().Name;

            switch (OP.Action)
            {
                case Spin.MemoryAction.PUSH:
                    return String.Format("PUSH {0}", Name);
                case Spin.MemoryAction.POP:
                    return String.Format("POP {0}", Name);
                case Spin.MemoryAction.EFFECT:
                    return String.Format("EFFECT {0} {1}", Name, GetEffectCode(chip, ref address));
                default:
                    return String.Format("UNKNOWN_{0} {1}", OP.Action, Name);
            }
        }

        public static string InterpreterText(Propeller chip, ref uint address, bool displayAsHexadecimal, bool useShortOpcodes)
        {
            byte op = chip.ReadByte(address++);
            string format;
            if (displayAsHexadecimal)
                format = "{0} ${1:X}";
            else
                format = "{0} {1}";
            if (useShortOpcodes)
            {
                EffectCodes = BriefEffectCodes;
                InterpretedOps = BriefInterpretedOps;
            }
            else
            {
                EffectCodes = BigEffectCodes;
                InterpretedOps = BigInterpretedOps;
            }

            switch (ArguementModes[op])
            {
                case ArguementMode.UnsignedOffset:
                    return String.Format(format, InterpretedOps[op], GetPackedOffset(chip, ref address));
                case ArguementMode.UnsignedEffectedOffset:
                    {
                        int arg = GetPackedOffset(chip, ref address);
                        format = "{0} {1} {2}";
                        return String.Format(format, InterpretedOps[op], arg, GetEffectCode(chip, ref address));
                    }
                case ArguementMode.Effect:
                    return String.Format(format, InterpretedOps[op], GetEffectCode(chip, ref address));
                case ArguementMode.SignedOffset:
                    {
                        uint result = chip.ReadByte(address++);

                        if ((result & 0x80) == 0)
                        {
                            if ((result & 0x40) != 0)
                                result |= 0xFFFFFF80;
                        }
                        else
                        {
                            result = (result << 8) | chip.ReadByte(address++);

                            if ((result & 0x4000) != 0)
                                result |= 0xFFFF8000;
                        }
                        return String.Format(format, InterpretedOps[op], (int)result);
                    }
                case ArguementMode.PackedLiteral:
                    return String.Format(format, InterpretedOps[op], GetPackedLiteral(chip, ref address));
                case ArguementMode.ByteLiteral:
                    return String.Format(format, InterpretedOps[op], chip.ReadByte(address++));
                case ArguementMode.WordLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 2; i++)
                        {
                            result <<= 8;
                            result |= chip.ReadByte(address++);
                        }
                        return String.Format(format, InterpretedOps[op], result);
                    }
                case ArguementMode.NearLongLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            result <<= 8;
                            result |= chip.ReadByte(address++);
                        }
                        return String.Format(format, InterpretedOps[op], result);
                    }
                case ArguementMode.LongLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            result <<= 8;
                            result |= chip.ReadByte(address++);
                        }
                        return String.Format(format, InterpretedOps[op], result);
                    }
                case ArguementMode.ObjCallPair:
                    {
                        byte obj = chip.ReadByte(address++);
                        byte funct = chip.ReadByte(address++);
                        return String.Format("{0} {1}.{2}", InterpretedOps[op], obj, funct);
                    }
                case ArguementMode.MemoryOpCode:
                    return String.Format("{0} {1}", InterpretedOps[op], GetMemoryOp(chip, ref address));
            }

            return InterpretedOps[op];
        }
    }
}
