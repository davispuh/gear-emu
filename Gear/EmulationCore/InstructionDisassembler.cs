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

        static private string[] Conditions;
        static private string[] Instructions;
        static private string[] InterpretedOps;
        static private ArguementMode[] ArguementModes;
        static private Dictionary<byte, string> EffectCodes;
        static private string[] CogRegister;

        static InstructionDisassembler()
        {
            Conditions = new string[] {
                "IF_NEVER    ","IF_NZ_AND_NC","IF_NC_AND_Z ","IF_NC       ",
                "IF_C_AND_NZ ","IF_NZ       ","IF_C_NE_Z   ","IF_NC_OR_NZ ",
                "IF_C_AND_Z  ","IF_C_EQ_Z   ","IF_Z        ","IF_NC_OR_Z  ",
                "IF_C        ","IF_C_OR_NZ  ","IF_Z_OR_C   ","            "
            };

            Instructions = new string[] {
                "RWBYTE ","RWWORD ","RWLONG ","HUBOP  ",
                "MUL    ","MULS   ","ENC    ","ONES   ",
                "ROR    ","ROL    ","SHR    ","SHL    ",
                "RCR    ","RCL    ","SAR    ","REV    ",
                "MINS   ","MAXS   ","MIN    ","MAX    ",
                "MOVS   ","MOVD   ","MOVI   ","JMPRET ",
                "AND    ","ANDN   ","OR     ","XOR    ",
                "MUXC   ","MUXNC  ","MUXZ   ","MUXNZ  ",
                "ADD    ","SUB    ","ADDABS ","SUBABS ",
                "SUMC   ","SUMNC  ","SUMZ   ","SUMNZ  ",
                "MOV    ","NEG    ","ABS    ","ABSNEG ",
                "NEGC   ","NEGNC  ","NEGZ   ","NEGNZ  ",
                "CMPS   ","CMPSX  ","ADDX   ","SUBX   ",
                "ADDS   ","SUBS   ","ADDSX  ","SUBSX  ",
                "CMPSUB ","DJNZ   ","TJNZ   ","TJZ    ",
                "WAITPEQ","WAITPNE","WAITCNT","WAITVID"
            };

            InterpretedOps = new string[] {
	            "FRAME_CALL_RETURN",
	            "FRAME_CALL_NORETURN",
	            "FRAME_CALL_ABORT",
	            "FRAME_CALL_TRASHABORT",
	            "BRANCH",
	            "CALL",
	            "OBJCALL",
	            "OBJCALL_INDEXED",
	            "LOOP_START",
	            "LOOP_CONTINUE",
	            "JUMP_IF_FALSE",
	            "JUMP_IF_TRUE",
	            "JUMP_FROM_STACK",
	            "COMPARE_CASE",
	            "COMPARE_CASE_RANGE",
	            "LOOK_ABORT",
	            "LOOKUP_COMPARE",
	            "LOOKDOWN_COMPARE",
	            "LOOKUPRANGE_COMPARE",
	            "LOOKDOWNRANGE_COMPARE",
	            "QUIT",
	            "MARK_INTERPRETED",
	            "STRSIZE",
	            "STRCOMP",
	            "BYTEFILL",
	            "WORDFILL",
	            "LONGFILL",
	            "WAITPEQ",
	            "BYTEMOVE",
	            "WORDMOVE",
	            "LONGMOVE",
	            "WAITPNE",
	            "CLKSET",
	            "COGSTOP",
	            "LOCKRET",
	            "WAITCNT",
	            "READ_INDEXED_SPR",
	            "WRITE_INDEXED_SPR",
	            "EFFECT_INDEXED_SPR",
	            "WAITVID",
	            "COGINIT_RETURNS",
	            "LOCKNEW_RETURNS",
	            "LOCKSET_RETURNS",
	            "LOCKCLR_RETURNS",
	            "COGINIT",
	            "LOCKNEW",
	            "LOCKSET",
	            "LOCKCLR",
	            "ABORT",
	            "ABORT_WITH_RETURN",
	            "RETURN",
	            "POP_RETURN",
	            "PUSH_NEG1",
	            "PUSH_0",
	            "PUSH_1",
	            "PUSH_PACKED_LIT",
	            "PUSH_BYTE_LIT",
	            "PUSH_WORD_LIT",
	            "PUSH_MID_LIT",
	            "PUSH_LONG_LIT",
	            "UNKNOWN OP $3C",
	            "INDEXED_MEM_OP",
	            "INDEXED_RANGE_MEM_OP",
	            "MEMORY_OP",
	            "PUSH_VARMEM_LONG_0",
	            "POP_VARMEM_LONG_0",
	            "EFFECT_VARMEM_LONG_0",
	            "REFERENCE_VARMEM_LONG_0",
	            "PUSH_VARMEM_LONG_1",
	            "POP_VARMEM_LONG_1",
	            "EFFECT_VARMEM_LONG_1",
	            "REFERENCE_VARMEM_LONG_1",
	            "PUSH_VARMEM_LONG_2",
	            "POP_VARMEM_LONG_2",
	            "EFFECT_VARMEM_LONG_2",
	            "REFERENCE_VARMEM_LONG_2",
	            "PUSH_VARMEM_LONG_3",
	            "POP_VARMEM_LONG_3",
	            "EFFECT_VARMEM_LONG_3",
	            "REFERENCE_VARMEM_LONG_3",
	            "PUSH_VARMEM_LONG_4",
	            "POP_VARMEM_LONG_4",
	            "EFFECT_VARMEM_LONG_4",
	            "REFERENCE_VARMEM_LONG_4",
	            "PUSH_VARMEM_LONG_5",
	            "POP_VARMEM_LONG_5",
	            "EFFECT_VARMEM_LONG_5",
	            "REFERENCE_VARMEM_LONG_5",
	            "PUSH_VARMEM_LONG_6",
	            "POP_VARMEM_LONG_6",
	            "EFFECT_VARMEM_LONG_6",
	            "REFERENCE_VARMEM_LONG_6",
	            "PUSH_VARMEM_LONG_7",
	            "POP_VARMEM_LONG_7",
	            "EFFECT_VARMEM_LONG_7",
	            "REFERENCE_VARMEM_LONG_7",
	            "PUSH_LOCALMEM_LONG_0",
	            "POP_LOCALMEM_LONG_0",
	            "EFFECT_LOCALMEM_LONG_0",
	            "REFERENCE_LOCALMEM_LONG_0",
	            "PUSH_LOCALMEM_LONG_1",
	            "POP_LOCALMEM_LONG_1",
	            "EFFECT_LOCALMEM_LONG_1",
	            "REFERENCE_LOCALMEM_LONG_1",
	            "PUSH_LOCALMEM_LONG_2",
	            "POP_LOCALMEM_LONG_2",
	            "EFFECT_LOCALMEM_LONG_2",
	            "REFERENCE_LOCALMEM_LONG_2",
	            "PUSH_LOCALMEM_LONG_3",
	            "POP_LOCALMEM_LONG_3",
	            "EFFECT_LOCALMEM_LONG_3",
	            "REFERENCE_LOCALMEM_LONG_3",
	            "PUSH_LOCALMEM_LONG_4",
	            "POP_LOCALMEM_LONG_4",
	            "EFFECT_LOCALMEM_LONG_4",
	            "REFERENCE_LOCALMEM_LONG_4",
	            "PUSH_LOCALMEM_LONG_5",
	            "POP_LOCALMEM_LONG_5",
	            "EFFECT_LOCALMEM_LONG_5",
	            "REFERENCE_LOCALMEM_LONG_5",
	            "PUSH_LOCALMEM_LONG_6",
	            "POP_LOCALMEM_LONG_6",
	            "EFFECT_LOCALMEM_LONG_6",
	            "REFERENCE_LOCALMEM_LONG_6",
	            "PUSH_LOCALMEM_LONG_7",
	            "POP_LOCALMEM_LONG_7",
	            "EFFECT_LOCALMEM_LONG_7",
	            "REFERENCE_LOCALMEM_LONG_7",
	            "PUSH_MAINMEM_BYTE",
	            "POP_MAINMEM_BYTE",
	            "EFFECT_MAINMEM_BYTE",
	            "REFERENCE_MAINMEM_BYTE",
	            "PUSH_OBJECTMEM_BYTE",
	            "POP_OBJECTMEM_BYTE",
	            "EFFECT_OBJECTMEM_BYTE",
	            "REFERENCE_OBJECTMEM_BYTE",
	            "PUSH_VARIABLEMEM_BYTE",
	            "POP_VARIABLEMEM_BYTE",
	            "EFFECT_VARIABLEMEM_BYTE",
	            "REFERENCE_VARIABLEMEM_BYTE",
	            "PUSH_INDEXED_LOCALMEM_BYTE",
	            "POP_INDEXED_LOCALMEM_BYTE",
	            "EFFECT_INDEXED_LOCALMEM_BYTE",
	            "REFERENCE_INDEXED_LOCALMEM_BYTE",
	            "PUSH_INDEXED_MAINMEM_BYTE",
	            "POP_INDEXED_MAINMEM_BYTE",
	            "EFFECT_INDEXED_MAINMEM_BYTE",
	            "REFERENCE_INDEXED_MAINMEM_BYTE",
	            "PUSH_INDEXED_OBJECTMEM_BYTE",
	            "POP_INDEXED_OBJECTMEM_BYTE",
	            "EFFECT_INDEXED_OBJECTMEM_BYTE",
	            "REFERENCE_INDEXED_OBJECTMEM_BYTE",
	            "PUSH_INDEXED_VARIABLEMEM_BYTE",
	            "POP_INDEXED_VARIABLEMEM_BYTE",
	            "EFFECT_INDEXED_VARIABLEMEM_BYTE",
	            "REFERENCE_INDEXED_VARIABLEMEM_BYTE",
	            "PUSH_INDEXED_LOCALMEM_BYTE",
	            "POP_INDEXED_LOCALMEM_BYTE",
	            "EFFECT_INDEXED_LOCALMEM_BYTE",
	            "REFERENCE_INDEXED_LOCALMEM_BYTE",
	            "PUSH_MAINMEM_WORD",
	            "POP_MAINMEM_WORD",
	            "EFFECT_MAINMEM_WORD",
	            "REFERENCE_MAINMEM_WORD",
	            "PUSH_OBJECTMEM_WORD",
	            "POP_OBJECTMEM_WORD",
	            "EFFECT_OBJECTMEM_WORD",
	            "REFERENCE_OBJECTMEM_WORD",
	            "PUSH_VARIABLEMEM_WORD",
	            "POP_VARIABLEMEM_WORD",
	            "EFFECT_VARIABLEMEM_WORD",
	            "REFERENCE_VARIABLEMEM_WORD",
	            "PUSH_LOCALMEM_WORD",
	            "POP_LOCALMEM_WORD",
	            "EFFECT_LOCALMEM_WORD",
	            "REFERENCE_LOCALMEM_WORD",
	            "PUSH_INDEXED_MAINMEM_WORD",
	            "POP_INDEXED_MAINMEM_WORD",
	            "EFFECT_INDEXED_MAINMEM_WORD",
	            "REFERENCE_INDEXED_MAINMEM_WORD",
	            "PUSH_INDEXED_OBJECTMEM_WORD",
	            "POP_INDEXED_OBJECTMEM_WORD",
	            "EFFECT_INDEXED_OBJECTMEM_WORD",
	            "REFERENCE_INDEXED_OBJECTMEM_WORD",
	            "PUSH_INDEXED_VARIABLEMEM_WORD",
	            "POP_INDEXED_VARIABLEMEM_WORD",
	            "EFFECT_INDEXED_VARIABLEMEM_WORD",
	            "REFERENCE_INDEXED_VARIABLEMEM_WORD",
	            "PUSH_INDEXED_LOCALMEM_WORD",
	            "POP_INDEXED_LOCALMEM_WORD",
	            "EFFECT_INDEXED_LOCALMEM_WORD",
	            "REFERENCE_INDEXED_LOCALMEM_WORD",
	            "PUSH_MAINMEM_LONG",
	            "POP_MAINMEM_LONG",
	            "EFFECT_MAINMEM_LONG",
	            "REFERENCE_MAINMEM_LONG",
	            "PUSH_OBJECTMEM_LONG",
	            "POP_OBJECTMEM_LONG",
	            "EFFECT_OBJECTMEM_LONG",
	            "REFERENCE_OBJECTMEM_LONG",
	            "PUSH_VARIABLEMEM_LONG",
	            "POP_VARIABLEMEM_LONG",
	            "EFFECT_VARIABLEMEM_LONG",
	            "REFERENCE_VARIABLEMEM_LONG",
	            "PUSH_LOCALMEM_LONG",
	            "POP_LOCALMEM_LONG",
	            "EFFECT_LOCALMEM_LONG",
	            "REFERENCE_LOCALMEM_LONG",
	            "PUSH_INDEXED_MAINMEM_LONG",
	            "POP_INDEXED_MAINMEM_LONG",
	            "EFFECT_INDEXED_MAINMEM_LONG",
	            "REFERENCE_INDEXED_MAINMEM_LONG",
	            "PUSH_INDEXED_OBJECTMEM_LONG",
	            "POP_INDEXED_OBJECTMEM_LONG",
	            "EFFECT_INDEXED_OBJECTMEM_LONG",
	            "REFERENCE_INDEXED_OBJECTMEM_LONG",
	            "PUSH_INDEXED_VARIABLEMEM_LONG",
	            "POP_INDEXED_VARIABLEMEM_LONG",
	            "EFFECT_INDEXED_VARIABLEMEM_LONG",
	            "REFERENCE_INDEXED_VARIABLEMEM_LONG",
	            "PUSH_INDEXED_LOCALMEM_LONG",
	            "POP_INDEXED_LOCALMEM_LONG",
	            "EFFECT_INDEXED_LOCALMEM_LONG",
	            "REFERENCE_INDEXED_LOCALMEM_LONG",
	            "ROTATE_RIGHT",
	            "ROTATE_LEFT",
	            "SHIFT_RIGHT",
	            "SHIFT_LEFT",
	            "LIMIT_MIN",
	            "LIMIT_MAX",
	            "NEGATE",
	            "COMPLEMENT",
	            "BIT_AND",
	            "ABSOLUTE_VALUE",
	            "BIT_OR",
	            "BIT_XOR",
	            "ADD",
	            "SUBTRACT",
	            "ARITH_SHIFT_RIGHT",
	            "BIT_REVERSE",
	            "LOGICAL_AND",
	            "ENCODE",
	            "LOGICAL_OR",
	            "DECODE",
	            "MULTIPLY",
	            "MULTIPLY_HI",
	            "DIVIDE",
	            "MODULO",
	            "SQUARE_ROOT",
	            "LESS",
	            "GREATER",
	            "NOT_EQUAL",
	            "EQUAL",
	            "LESS_EQUAL",
	            "GREATER_EQUAL",
	            "LOGICAL_NOT"
            };

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

            EffectCodes = new Dictionary<byte, string>();

            EffectCodes[0x0] = "COPY";
            EffectCodes[0x8] = "FORWARD_RANDOM";
            EffectCodes[0xC] = "REVERSE_RANDOM";
            EffectCodes[0x10] = "EXTEND_8";
            EffectCodes[0x14] = "EXTEND_16";
            EffectCodes[0x18] = "BIT_CLEAR";
            EffectCodes[0x1C] = "BIT_SET";
            EffectCodes[0x20] = "PRE_INCREMENT_BITS";
            EffectCodes[0x22] = "PRE_INCREMENT_BYTE";
            EffectCodes[0x24] = "PRE_INCREMENT_WORD";
            EffectCodes[0x26] = "PRE_INCREMENT_LONG";
            EffectCodes[0x28] = "POST_INCREMENT_BITS";
            EffectCodes[0x2A] = "POST_INCREMENT_BYTE";
            EffectCodes[0x2C] = "POST_INCREMENT_WORD";
            EffectCodes[0x2E] = "POST_INCREMENT_LONG";
            EffectCodes[0x30] = "PRE_DECREMENT_BITS";
            EffectCodes[0x32] = "PRE_DECREMENT_BYTE";
            EffectCodes[0x34] = "PRE_DECREMENT_WORD";
            EffectCodes[0x36] = "PRE_DECREMENT_LONG";
            EffectCodes[0x38] = "POST_DECREMENT_BITS";
            EffectCodes[0x3A] = "POST_DECREMENT_BYTE";
            EffectCodes[0x3C] = "POST_DECREMENT_WORD";
            EffectCodes[0x3E] = "POST_DECREMENT_LONG";
            EffectCodes[0x40] = "ROTATE_RIGHT";
            EffectCodes[0x41] = "ROTATE_LEFT";
            EffectCodes[0x42] = "SHIFT_RIGHT";
            EffectCodes[0x43] = "SHIFT_LEFT";
            EffectCodes[0x44] = "MINIMUM";
            EffectCodes[0x45] = "MAXIMUM";
            EffectCodes[0x46] = "NEGATE";
            EffectCodes[0x47] = "COMPLEMENT";
            EffectCodes[0x48] = "BIT_AND";
            EffectCodes[0x49] = "ABSOLUTE_VALUE";
            EffectCodes[0x4A] = "BIT_OR";
            EffectCodes[0x4B] = "BIT_XOR";
            EffectCodes[0x4C] = "ADD";
            EffectCodes[0x4D] = "SUBTRACT";
            EffectCodes[0x4E] = "ARITH_SHIFT_RIGHT";
            EffectCodes[0x4F] = "BIT_REVERSE";
            EffectCodes[0x50] = "LOGICAL_AND";
            EffectCodes[0x51] = "ENCODE";
            EffectCodes[0x52] = "LOGICAL_OR";
            EffectCodes[0x53] = "DECODE";
            EffectCodes[0x54] = "MULTIPLY";
            EffectCodes[0x55] = "MULTIPLY_HI";
            EffectCodes[0x56] = "DIVIDE";
            EffectCodes[0x57] = "MODULO";
            EffectCodes[0x58] = "SQUARE_ROOT";
            EffectCodes[0x59] = "LESS";
            EffectCodes[0x5A] = "GREATER";
            EffectCodes[0x5B] = "NOT_EQUAL";
            EffectCodes[0x5C] = "EQUAL";
            EffectCodes[0x5D] = "LESS_EQUAL";
            EffectCodes[0x5E] = "GREATER_EQUAL";
            EffectCodes[0x5F] = "NOT";

            CogRegister = new string[] {
                "MEM_0","MEM_1","MEM_2","MEM_3",
                "MEM_4","MEM_5","MEM_6","MEM_7",
                "MEM_8","MEM_9","MEM_A","MEM_B",
                "MEM_C","MEM_D","MEM_E","MEM_F",
                "PAR",  "CNT",  "INA",  "INB",
                "OUTA", "OUTB", "DIRA", "DIRB",
                "CTRA", "CTRB", "FRQA", "FRQB",
                "PHSA", "PHSB", "VCFG", "VSCL"
            };
        }

        public static string AssemblyText(uint Operation)
        {
            uint InstructionCode = (Operation & 0xFC000000) >> 26;
            uint ConditionCode = (Operation & 0x003C0000) >> 18;

            bool WriteZero = (Operation & 0x02000000) != 0;
            bool WriteCarry = (Operation & 0x01000000) != 0;
            bool WriteResult = (Operation & 0x00800000) != 0;
            bool ImmediateValue = (Operation & 0x00400000) != 0;

            uint Source = Operation & 0x1FF;
            uint Destination = (Operation >> 9) & 0x1FF;

            string instruction =

            String.Format("{0} {1} {2:X3}, {3}{4:X3}", new object[] { Conditions[ConditionCode],
                Instructions[InstructionCode],
                Destination,
                ImmediateValue ? "#" : " ",
                Source } );

            if (WriteResult)
                instruction += " WR";
            else
                instruction += " NR";
            if (WriteZero)
                instruction += " WZ";
            if (WriteCarry)
                instruction += " WC";

            return instruction;
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

            if( (op & 0x80) == 0 )
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
                effect += String.Format("REPEAT_COMPARE {0}", GetSignedPackedOffset(chip, ref address));
            else if (op == 0x06)
                effect += String.Format("REPEAT_COMPARE_STEP {0}", GetSignedPackedOffset(chip, ref address));
            else if (EffectCodes.ContainsKey(op))
                effect += EffectCodes[op];
            else
                effect += String.Format("UNKNOWN_EFFECT_{0:X}", op);

            return effect;
        }

        public static string GetMemoryOp(Propeller chip, ref uint address)
        {
            byte op = chip.ReadByte(address++);

            switch (op & 0xE0)
            {
                case 0x00:
                    return String.Format("UNKNOWN_0 {0}", CogRegister[op&0x1F]);
                case 0x20:
                    return String.Format("UNKNOWN_2 {0}", CogRegister[op & 0x1F]);
                case 0x40:
                    return String.Format("UNKNOWN_4 {0}", CogRegister[op & 0x1F]);
                case 0x60:
                    return String.Format("UNKNOWN_6 {0}", CogRegister[op & 0x1F]);
                case 0x80:
                    return String.Format("PUSH {0}", CogRegister[op & 0x1F]);
                case 0xA0:
                    return String.Format("POP {0}", CogRegister[op & 0x1F]);
                case 0xC0:
                    return String.Format("EFFECT {0}", CogRegister[op & 0x1F], GetEffectCode(chip,ref address));
                default:
                    return String.Format("UNKNOWN_E {0}", CogRegister[op & 0x1F]);
            }
        }

        public static string InterpreterText(Propeller chip, ref uint address)
        {
            byte op = chip.ReadByte(address++);

            switch (ArguementModes[op])
            {
                case ArguementMode.UnsignedOffset:
                    {
                        return String.Format("{0} {1}", InterpretedOps[op], GetPackedOffset(chip, ref address));
                    }
                case ArguementMode.UnsignedEffectedOffset:
                    {
                        int arg = GetPackedOffset(chip, ref address);
                        return String.Format("{0} {1} {2}", InterpretedOps[op], arg, GetEffectCode(chip, ref address));
                    }
                case ArguementMode.Effect:
                    return String.Format("{0} {1}", InterpretedOps[op], GetEffectCode(chip, ref address));
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

                        return String.Format("{0} {1}", InterpretedOps[op], (int)result);
                    }
                case ArguementMode.PackedLiteral:
                    return String.Format("{0} ${1:x}", InterpretedOps[op], GetPackedLiteral(chip, ref address));
                case ArguementMode.ByteLiteral:
                    return String.Format("{0} {1}", InterpretedOps[op], chip.ReadByte(address++));
                case ArguementMode.WordLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 2; i++)
                        {
                            result <<= 8;
                            result |= chip.ReadByte(address++);
                        }
                        return String.Format("{0} {1}", InterpretedOps[op], result);
                    }
                case ArguementMode.NearLongLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            result <<= 8;
                            result |= chip.ReadByte(address++);
                        }
                        return String.Format("{0} {1}", InterpretedOps[op], result);
                    }
                case ArguementMode.LongLiteral:
                    {
                        int result = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            result <<= 8;
                            result |= chip.ReadByte(address++);
                        }
                        return String.Format("{0} {1}", InterpretedOps[op], result);
                    }
                case ArguementMode.ObjCallPair:
                    {
                        byte obj = chip.ReadByte(address++);
                        byte funct = chip.ReadByte(address++);
                        return String.Format("{0} {1}.{2}", InterpretedOps[op], obj,funct );
                    }
                case ArguementMode.MemoryOpCode:
                    {
                        return String.Format("{0} {1}", InterpretedOps[op], GetMemoryOp(chip, ref address));
                    }
            }

            return InterpretedOps[op];
        }
    }
}
