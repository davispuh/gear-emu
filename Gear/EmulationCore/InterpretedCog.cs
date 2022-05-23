/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * InterpretedCog.cs
 * Class for an interpreted (SPIN) cog.
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

using Gear.Propeller;
using System;
using System.Collections.Generic;

namespace Gear.EmulationCore
{
    /// @brief Derived class from Cog, to emulate running SPIN code.
    public class InterpretedCog : Cog
    {
        /// <summary>Definition of how many cycles takes to boot.</summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        // 48 cycles to boot (fake)
        private const int InterpreterBootTime = 48;

        // Interpreter Specific Values

        /// @brief Flag for determining if a <c>COGINIT</c> statement is
        /// loading an interpreter.
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private bool _interpreterFlag;
        /// <summary></summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private uint _targetValue;
        /// <summary></summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private uint _maskValue;
        /// <summary></summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private uint _pixelsValue;
        /// <summary></summary>
        /// @version v22.05.04 - Name changed to follow naming conventions.
        private uint _colorsValue;

        /// @brief Internal stack, used for storing return call stuff.
        private readonly Stack<uint> _callStack;

        /// <summary>Flag of using port A or B.</summary>
        /// @version v22.05.04 - Name changed to clarify meaning of it.
        private bool _isPortB;

        /// <summary></summary>
        /// @version v22.05.04 - Merge between property and private member.
        public uint StackFrame { get; private set; }
        /// <summary></summary>
        /// @version v22.05.04 - Merge between property and private member.
        public uint LocalFrame { get; private set; }
        /// <summary></summary>
        /// @version v22.05.04 - Merge between property and private member.
        public uint ObjectFrame { get; private set; }
        /// <summary></summary>
        /// @version v22.05.04 - Merge between property and private member.
        public uint VariableFrame { get; private set; }

        /// <summary>Default constructor for a SPIN interpreted cog.</summary>
        /// <param name="cpuHost">PropellerCPU instance where this object belongs.</param>
        /// <param name="cogNum"></param>
        /// <param name="paramAddress">PARAM value given to the Cog.</param>
        /// <param name="frequency">Frequency running the cog (the same as the propeller cpu).</param>
        /// <param name="pllGroup"></param>
        /// @version v22.05.03 - Parameter names changed to use the same
        /// convention for a PropellerCPU instance reference, or changed to
        /// follow naming conventions.
        public InterpretedCog(PropellerCPU cpuHost, int cogNum, uint paramAddress,
            uint frequency, PLLGroup pllGroup)
            : base(cpuHost, cogNum, 0xF004, paramAddress, frequency, pllGroup)
        {
            _callStack = new Stack<uint>();
        }

        /// @brief Insert a LONG into stack.
        /// @param value LONG value.
        /// @version v22.05.04 - Changed visibility to private method.
        private void PushStack(uint value)
        {
            CpuHost.DirectWriteLong(StackFrame, value);
            StackFrame += 4;
        }

        /// @brief Retrieve a LONG from stack.
        /// @return LONG value.
        /// @version v22.05.04 - Changed visibility to private method.
        private uint PopStack()
        {
            StackFrame -= 4;
            return CpuHost.DirectReadLong(StackFrame);
        }

        /// @brief Insert a WORD into stack.
        /// @param value WORD value.
        private void PushStackWord(ushort value)
        {
            CpuHost.DirectWriteWord(StackFrame, value);
            StackFrame += 2;
        }

        /// @brief Retrieve a WORD from stack.
        /// @return WORD value.
        private ushort PopStackWord()
        {
            StackFrame -= 2;
            return CpuHost.DirectReadWord(StackFrame);
        }

        /// <summary>Setup the cog to a initial state after boot it.</summary>
        /// @version v22.05.04 - Changed visibility of method.
        private protected override void Boot()
        {
            State = CogRunState.BootInterpreter;
            StateCount = InterpreterBootTime; // how many cycles takes to boot
            uint initFrame = this[(int)Assembly.RegisterAddress.PAR];
            this[(int)Assembly.RegisterAddress.COGID] = CpuHost.CogID(this);
            initFrame &= PropellerCPU.MAX_RAM_ADDR;
            ObjectFrame = CpuHost.DirectReadWord(initFrame - 8);
            VariableFrame = CpuHost.DirectReadWord(initFrame - 6);
            ProgramCursor = CpuHost.DirectReadWord(initFrame - 4);
            StackFrame = (uint)CpuHost.DirectReadWord(initFrame - 2) - 4;
            LocalFrame = initFrame - 4;
            // Clear CogID
            this[(int)Assembly.RegisterAddress.INITCOGID] = initFrame - 4;
            CpuHost.DirectWriteLong(initFrame - 8, 0xFFFFFFFF);
            CpuHost.DirectWriteLong(initFrame - 4, 0x0);
        }

        /// <summary></summary>
        /// <returns></returns>
        /// @version v22.05.04 - Changed local variable names to follow naming
        /// conventions or to clarify meaning of it.
        private uint CogInit()
        {
            uint cogId;
            uint code;
            if (_interpreterFlag)
            {
                _interpreterFlag = false;  // Clear interpreter flag
                uint stackPointer = PopStack() + 8;
                uint functionCode = PopStack();
                uint functionArgs = functionCode >> 8;
                functionCode &= 0xFF;
                // Long align our stack (wasteful)
                while ((stackPointer & 0x3) != 0)
                    stackPointer++;
                // SETUP INTERPRETER HERE
                uint functionOffset = CpuHost.DirectReadWord(ObjectFrame + functionCode * 4) + ObjectFrame;
                uint argumentStack = functionArgs * 4 + stackPointer - 4;
                uint functionStack = argumentStack + CpuHost.DirectReadWord(ObjectFrame + functionCode * 4 + 2) + 4;
                // Object Memory (Same object)
                CpuHost.DirectWriteWord(stackPointer - 8, (ushort)ObjectFrame);
                // Variable Memory (Same variables)
                CpuHost.DirectWriteWord(stackPointer - 6, (ushort)VariableFrame);
                // ProgramCursor
                CpuHost.DirectWriteWord(stackPointer - 4, (ushort)functionOffset);
                // Stack Pointer
                CpuHost.DirectWriteWord(stackPointer - 2, (ushort)(functionStack + 4));
                // Pre-initialize boot function
                for (uint i = 0; i < functionArgs * 4; i += 4, argumentStack -= 4)
                    CpuHost.DirectWriteLong(argumentStack, PopStack());
                // Setup cog boot op-codes
                code = ((0xF004 & 0xFFFC) << 2) | (stackPointer << 16);
                // Find which cog we will be booting
                cogId = CpuHost.DirectReadLong(this[(int)Assembly.RegisterAddress.INITCOGID] - 4);
                // Clear CogID
                CpuHost.DirectWriteLong(this[(int)Assembly.RegisterAddress.INITCOGID] - 4, 0xFFFFFFFF);
            }
            else
            {
                // We will be using the value for the boot parameter
                // located inside of stack itself.  This is only valid because
                // SPIN instructions take longer than 14 cycles to execute.
                // Otherwise there is the possibility that the value could be clobbered.
                uint bootParam = PopStack();
                uint entryPoint = PopStack();
                cogId = PopStack();
                code = ((entryPoint & 0xFFFC) << 2) |
                       ((bootParam & 0xFFFC) << 16);
            }
            // New cog if ID is not valid
            code |= cogId < 8 ?
                cogId :
                0x8;
            bool carry = false, zero = false;  // Temporal provided to hub operation method
            return CpuHost.HubOp(this, (uint)HubOperationCodes.HUBOP_COGINIT, code, ref carry, ref zero);
        }

        /// @brief Execute a SPIN bytecode instruction in this cog.
        /// @returns TRUE if it is the opportunity to trigger a breakpoint,
        /// or FALSE if not.
        /// @pullreq{18} Correct video frame load timing, added video break,
        /// fix tab refresh.
        /// @issue{23} Inaccuracy in Spin literal decoding.
        /// @version v22.05.04 - Changed local variable names to follow naming
        /// conventions or to clarify meaning of it.
        /// @todo [Legacy] RAISE AN EXCEPTION on Executed undefined, OpCode 0x14
        /// @todo [Legacy] Should complain on invalid op if MSB is set, OpCode 0x37
        /// @todo [Legacy] ALERT ON BAD OP on Executed undefined, OpCode 0x3C
        ///
        /// Spin ByteCodes Summary
        /// ======================
        /// |OpCode set|Byte      |Description                                               |Extra bytes              ||
        /// |:--------:|---------:|:---------------------------------------------------------|:------------|-----------:|
        /// |0x00..0x3F|`00xxxxxx`|[Special purpose ops](@ref SpecialOps)                    |                         ||
        /// |0x40..0x7F|`01bxxxqq`|[Variable ops, Fast access VAR and LOC](@ref VariableOps) |             |+1 if assign|
        /// |0x80..0xDF|`1ssibbqq`|[Memory ops, access MEM, OBJ, VAR and LOC](@ref MemoryOps)|+1..2 if base|+1 if assign|
        /// |0xE0..0xFF|`111xxxxx`|[Math ops, Unary and Binary operators](@ref MathOpCodes)  |                         ||
        /// Source: [Cluso99's SPIN bytecode document revision RR20080721,
        /// on %Propeller 1 Forum](https://forums.parallax.com/discussion/comment/796018/#Comment_796018)
        ///
        /// 0x00..0x3F Special purpose OpCodes {#SpecialOps}
        /// ==================================
        /// Bytecode Table: 48 operations
        /// -----------------------------
        /// |POP method used|Set| Op |Byte      |Description                  |Pops|Push|Extra bytes       ||
        /// |:--------------|:-:|:--:|:--------:|:----------------------------|:--:|:--:|:-------|:--------:|
        /// |               | 0 |`00`|`000000tp`|`drop anchor                `|    |    |(`t=try, !p=push`)||
        /// |               | ^ |`01`|`000000tp`|`drop anchor                `|    |    |(`t=try, !p=push`)||
        /// |               | ^ |`02`|`000000tp`|`drop anchor                `|    |    |(`t=try, !p=push`)||
        /// |               | ^ |`03`|`000000tp`|`drop anchor                `|    |    |(`t=try, !p=push`)||
        /// |               | 1 |`04`|`00000100`|`jmp                        `|    |    |+1..2 address (see [Note 1](#N1SpecialOpcodes))||
        /// |               | ^ |`05`|`00000101`|`call sub                   `|    |    |+1 sub            ||
        /// |               | ^ |`06`|`00000110`|`call obj.sub               `|    |    |+2 obj+sub        ||
        /// |popx           | ^ |`07`|`00000111`|`call obj[].sub             `|1   |    |+2 obj+sub        ||
        /// |popx           | 2 |`08`|`00001000`|`tjz                        `|1   | 0/1|+1..2 address (see [Note 1](#N1SpecialOpcodes))||
        /// |popx           | ^ |`09`|`00001001`|`djnz                       `|1   | 0/1|     ^            ||
        /// |popx           | ^ |`0A`|`00001010`|`jz                         `|1   |    |     ^            ||
        /// |popx           | ^ |`0B`|`00001011`|`jnz                        `|1   |    |     ^            ||
        /// |popyx          | 3 |`0C`|`00001100`|`casedone                   `|2   |    |     ^            ||
        /// |popx           | ^ |`0D`|`00001101`|`value case                 `|1   |    |     ^            ||
        /// |popyx          | ^ |`0E`|`00001110`|`range case                 `|2   |    |     ^            ||
        /// |popayx         | ^ |`0F`|`00001111`|`lookdone                   `|3   |   1|                  ||
        /// |popx           | 4 |`10`|`00010000`|`value lookup               `|1   |    |                  ||
        /// |popx           | ^ |`11`|`00010001`|`value lookdown             `|1   |    |                  ||
        /// |popyx          | ^ |`12`|`00010010`|`range lookup               `|2   |    |                  ||
        /// |popyx          | ^ |`13`|`00010011`|`range lookdown             `|2   |    |                  ||
        /// |popx           | 5 |`14`|`00010100`|`pop                        `|1+  |    |    ???1+         ||
        /// |               | ^ |`15`|`00010101`|`run                        `|    |    |                  ||
        /// |popx           | ^ |`16`|`00010110`|`STRSIZE(string)            `|1   |   1|                  ||
        /// |popyx          | ^ |`17`|`00010111`|`STRCOMP(stringa,stringb)   `|2   |   1|                  ||
        /// |popayx         | 6 |`18`|`00011000`|`BYTEFILL(start,value,count)`|3   |    |                  ||
        /// |popayx         | ^ |`19`|`00011001`|`WORDFILL(start,value,count)`|3   |    |                  ||
        /// |popayx         | ^ |`1A`|`00011010`|`LONGFILL(start,value,count)`|3   |    |                  ||
        /// |popayx         | ^ |`1B`|`00011011`|`WAITPEQ(data,mask,port)    `|3   |    |                  ||
        /// |popayx         | 7 |`1C`|`00011100`|`BYTEMOVE(to,from,count)    `|3   |    |                  ||
        /// |popayx         | ^ |`1D`|`00011101`|`WORDMOVE(to,from,count)    `|3   |    |                  ||
        /// |popayx         | ^ |`1E`|`00011110`|`LONGMOVE(to,from,count)    `|3   |    |                  ||
        /// |popayx         | ^ |`1F`|`00011111`|`WAITPNE(data,mask,port)    `|3   |    |                  ||
        /// |popyx          | 8 |`20`|`00100000`|`CLKSET(mode,freq)          `|2   |    |                  ||
        /// |popx           | ^ |`21`|`00100001`|`COGSTOP(id)                `|1   |    |                  ||
        /// |popx           | ^ |`22`|`00100010`|`LOCKRET(id)                `|1   |    |                  ||
        /// |popx           | ^ |`23`|`00100011`|`WAITCNT(count)             `|1   |    |                  ||
        /// |popx           | 9 |`24`|`001001qq`|`SPR[nibble] op   push      `|1   |    |      |+1 if assign|
        /// |popx           | ^ |`25`|`001001qq`|`SPR[nibble] op   pop       `|1   |    |      |+1 if assign|
        /// |popx           | ^ |`26`|`001001qq`|`SPR[nibble] op   using     `|1   |    |      |+1 if assign|
        /// |popyx          | ^ |`27`|`00100111`|`WAITVID(colors,pixels)     `|2   |    |                  ||
        /// |popayx         | A |`28`|`00101p00`|`COGINIT(id,adr,ptr)        `|3   |   1|(`!p=push`)       ||
        /// |               | ^ |`29`|`00101p01`|`LOCKNEW                    `|    |   1|(`!p=push`)       ||
        /// |popx           | ^ |`2A`|`00101p10`|`LOCKSET(id)                `|1   |   1|(`!p=push`)       ||
        /// |popx           | ^ |`2B`|`00101p11`|`LOCKCLR(id)                `|1   |   1|(`!p=push`)       ||
        /// |popayx         | B |`2C`|`00101p00`|`COGINIT(id,adr,ptr)        `|3   |   0|(`no push`)       ||
        /// |               | ^ |`2D`|`00101p01`|`LOCKNEW                    `|    |   0|(`no push`)       ||
        /// |popx           | ^ |`2E`|`00101p10`|`LOCKSET(id)                `|1   |   0|(`no push`)       ||
        /// |popx           | ^ |`2F`|`00101p11`|`LOCKCLR(id)                `|1   |   0|(`no push`)       ||
        /// |               | C |`30`|`00110000`|`ABORT                      `|    |    |                  ||
        /// |popx           | ^ |`31`|`00110001`|`ABORT value                `|1   |    |                  ||
        /// |               | ^ |`32`|`00110010`|`RETURN                     `|    |    |                  ||
        /// |popx           | ^ |`33`|`00110011`|`RETURN value               `|1   |    |                  ||
        /// |               | D |`34`|`001101cc`|`PUSH #-1                   `|    |   1|                  ||
        /// |               | ^ |`35`|`001101cc`|`PUSH #0                    `|    |   1|                  ||
        /// |               | ^ |`36`|`001101cc`|`PUSH #1                    `|    |   1|                  ||
        /// |               | ^ |`37`|`00110111`|`PUSH #kp                   `|    |   1|+1 maskdata (see [Note 2](#N2SpecialOpcodes))||
        /// |               | E |`38`|`001110bb`|`PUSH #k1 (1 byte)          `|    |   1|+1 constant                                  ||
        /// |               | ^ |`39`|`001110bb`|`PUSH #k2 (2 bytes)         `|    |   1|+2 constant  (@#k1<<8 + @#k2)                ||
        /// |               | ^ |`3A`|`001110bb`|`PUSH #k3 (3 bytes)         `|    |   1|+3 constant  (@#k1<<16 + @#k2<<8 + @#k3)     ||
        /// |               | ^ |`3B`|`001110bb`|`PUSH #k4 (4 bytes)         `|    |   1|+4 constant  (@#k1<<24 + @#k2<<16 + @#k3<<8 + @#k4)||
        /// |               | F |`3C`|`00111100`|`<unused>                   `|    |    |                                                   ||
        /// |popx           | ^ |`3D`|`00111101`|`register[bit]      op      `|1   |    |+1 reg+op                                      |+1 if assign|
        /// |popyx          | ^ |`3E`|`00111110`|`register[bit..bit] op      `|2   |    |+1 reg+op                                      |+1 if assign|
        /// |               | ^ |`3F`|`00111111`|`register           op      `|    |    |+1 reg+op (see [Note 3](@ref N3SpecialOpcodes))|+1 if assign|
        /// Note 1: Address bytecodes (sign or zero extended to a long address) {#N1SpecialOpcodes}
        /// -------------------------------------------------------------------
        /// These (1 or 2) bytecodes are used to form a long address for the CASE, CASER and memory operations.
        /// |Structure                                 |||||
        /// |:--:|:--:|:------------:|--|:----------------:|
        /// |` x`|` s`|` a a a a a a`|  |` a a a a a a a a`|
        /// |`bF`|`bE`|`bDbCbBbAb9b8`|  |`b7b6b5b4b3b2b1b0`|
        /// where
        /// |Bit(s)     |Description     |
        /// |:---------:|:---------------|
        /// |`    x    `|`0`= 1 byte only|
        /// |     ^     |`1`= 2 bytes    |
        /// |Note: if the optional second byte is used then the first byte will be shifted left by 8 bits.||
        /// |`    s    `|`0`= higher order bits will be zero.|
        /// |     ^     |`1`= if sign-extend then 1's will be propagated to the left, otherwise 0's will be propagated to the left.|
        ///
        /// Note 2: Details of 0x37 operation {#N2SpecialOpcodes}
        /// ---------------------------------
        /// |Structure: Additional byte used as a constant||||
        /// |:--:|:--:|:--:|:---------:|
        /// |`-` |`n` |`d` |` r r r r r`|
        /// |`b7`|`b6`|`b5`|`b4b3b2b1b0`|
        /// where
        /// |Bit(s)     |Description                                    |
        /// |:---------:|:----------------------------------------------|
        /// |`    n    `|`0: nothing                                   `|
        /// |     ^     |`1: X := !X (invert/not)                      `|
        /// |`    d    `|`0: nothing                                   `|
        /// |     ^     |`1: X := X - 1 (decrement)                    `|
        /// |     ^     |`   then...                                   `|
        /// |`r r r r r`|`X := 2 << rrrrr (#2 rotate left "rrrrr" bits)`|
        /// |     ^     |`   then...                                   `|
        ///
        public override bool DoInstruction()
        {
            switch (State)
            {
                case CogRunState.BootInterpreter:
                case CogRunState.WaitInterpreter:
                    if (--StateCount == 0)
                        State = CogRunState.ExecInterpreter;
                    return true;
                case CogRunState.ExecInterpreter:
                    State = CogRunState.WaitInterpreter;
                    StateCount = 32;  // 32 cycles per instruction (faked)
                    break;
                case CogRunState.WaitPinsNotEqual:
                {
                    uint maskedIn = (_isPortB ? CpuHost.INB : CpuHost.INA) & _maskValue;
                    if (maskedIn != _targetValue)
                        State = CogRunState.ExecInterpreter;
                    return true;
                }
                case CogRunState.WaitPinsEqual:
                {
                    uint maskedIn = (_isPortB ? CpuHost.INB : CpuHost.INA) & _maskValue;
                    if (maskedIn == _targetValue)
                        State = CogRunState.ExecInterpreter;
                    return true;
                }
                case CogRunState.WaitCount:
                {
                    long target = CpuHost.Counter;
                    if (_targetValue == target)
                        State = CogRunState.ExecInterpreter;
                    return true;
                }
                case CogRunState.WaitVideo:
                    // Logic Changed by pull request #18: GetVideoData() now clears VAIT_VID state
                    return true;
                // Non-execute states are ignored
                default:
                    return true;
            }
            byte operation = CpuHost.DirectReadByte(ProgramCursor++);
            FrameFlag = FrameState.FrameNone;
            // Math Operations (0xE0..0xFF)
            if (operation >= 0xE0)
                PushStack(BaseMathOp((byte)(operation - 0xE0), true, PopStack()));
            // Masked Memory Operations (0x80..0xDF)
            else if (operation >= 0x80)
            {
                StepMaskedMemoryOp(operation);
                return true;
            }
            // Implicit Location Memory Ops (0x40..0x7F)
            else if (operation >= 0x40)
                StepImplicitMemoryOp(operation);
            // Special Purpose Ops (0x00..03F)
            else
            {
                switch (operation)
                {
                    case 0x00:  // FRAME CALL WITH RETURN
                    case 0x01:  // FRAME CALL WITH NO RETURN
                    case 0x02:  // FRAME CALL WITH TRAP AND RETURN
                    case 0x03:  // FRAME CALL WITH TRAP AND NO RETURN
                        //Use short stack for return masks
                        _callStack.Push((uint)(operation & 0x3));
                        PushStackWord((ushort)ObjectFrame);
                        PushStackWord((ushort)VariableFrame);
                        PushStackWord((ushort)LocalFrame);
                        // Enqueue pointer to return ProgramCursor
                        _callStack.Push(StackFrame);
                        // ProgramCursor is not valid yet
                        PushStackWord(0);
                        // Defaults to a zero value
                        PushStack(0);
                        break;
                    case 0x04: // UNCONDITIONAL RELATIVE BRANCH
                    {
                        uint branch = (uint)ReadPackedSignedWord();
                        ProgramCursor += branch;
                    }
                        break;
                    case 0x05:  // Call function
                    case 0x06:  // Call object function
                    case 0x07:  // Call indexed object function
                    {
                        // Pop our index from the stack
                        if (operation == 0x06 || operation == 0x07)
                        {
                            uint objectCode = (uint)(CpuHost.DirectReadByte(ProgramCursor++) * 4) + ObjectFrame;
                            if (operation == 0x07)
                                objectCode += PopStack() * 4;
                            ObjectFrame += CpuHost.DirectReadWord(objectCode);
                            VariableFrame += CpuHost.DirectReadWord(objectCode + 2);
                        }
                        uint functionCode = (uint)(CpuHost.DirectReadByte(ProgramCursor++) * 4) + ObjectFrame;
                        // De-CallStack the local frame
                        uint returnPointer = _callStack.Pop();
                        LocalFrame = returnPointer + 2;
                        // Preserve ProgramCursor
                        CpuHost.DirectWriteWord(returnPointer, (ushort)ProgramCursor);
                        // Branch, and change local and stack pointers to match what they need to be
                        ProgramCursor = ObjectFrame + CpuHost.DirectReadWord(functionCode);
                        StackFrame += CpuHost.DirectReadWord(functionCode + 2);
                    }
                        break;
                    case 0x08:  // LOOP START
                    {
                        int val = (int)PopStack();
                        uint branch = ReadPackedSignedWord();
                        if (val <= 0)
                            ProgramCursor = (uint)((int)ProgramCursor + branch);
                        else
                            PushStack((uint)val);
                    }
                        break;
                    case 0x09:  // LOOP CONTINUE
                    {
                        int val = (int)PopStack() - 1;
                        uint branch = ReadPackedSignedWord();
                        if (val > 0)
                        {
                            ProgramCursor = (uint)((int)ProgramCursor + branch);
                            PushStack((uint)val);
                        }
                    }
                        break;
                    case 0x0A:  // JUMP ON ZERO
                    {
                        uint val = PopStack();
                        uint branch = ReadPackedSignedWord();
                        if (val == 0)
                            ProgramCursor = (uint)((int)ProgramCursor + branch);
                    }
                        break;
                    case 0x0B:  // JUMP ON NOT ZERO
                    {
                        uint val = PopStack();
                        uint branch = ReadPackedSignedWord();
                        if (val != 0)
                            ProgramCursor = (uint)((int)ProgramCursor + branch);
                    }
                        break;
                    case 0x0C:  // JUMP FROM STACK
                        PopStack(); // Clears out the comparison case
                        ProgramCursor = PopStack() + ObjectFrame;
                        break;
                    case 0x0D:  // COMPARE CASE
                    {
                        uint equal = PopStack();
                        uint value = PopStack();
                        uint branch = ReadPackedSignedWord();
                        if (equal == value)
                            ProgramCursor = (uint)((int)ProgramCursor + branch);
                        PushStack(value);
                    }
                        break;
                    case 0x0E:  // COMPARE CASE RANGE
                    {
                        uint max = PopStack();
                        uint min = PopStack();
                        uint value = PopStack();
                        uint branch = ReadPackedSignedWord();
                        if (min > max)
                            (min, max) = (max, min);  //swap values
                        if (min <= value && value <= max)
                            ProgramCursor = (uint)((int)ProgramCursor + branch);
                        PushStack(value);
                    }
                        break;
                    case 0x0F:  // Look abort
                        PopStack(); // Key
                        PopStack(); // Jump
                        PopStack(); // Base
                        PushStack(0xFFFFFFFF);
                        break;
                    case 0x10:  // Look-up compare (get value at index)
                    {
                        int value = (int)PopStack();
                        int key = (int)PopStack();
                        uint jump = PopStack();
                        int @base = (int)PopStack();
                        if (key == @base)
                        {
                            ProgramCursor = jump + ObjectFrame;
                            PushStack((uint)value);
                        }
                        else
                        {
                            PushStack((uint)@base);
                            PushStack(jump);
                            PushStack((uint)--key);
                        }
                    }
                        break;
                    case 0x11:  // Look-down compare (get index of value)
                    {
                        int value = (int)PopStack();
                        int key = (int)PopStack();
                        uint jump = PopStack();
                        int @base = (int)PopStack();
                        if (key == value)
                        {
                            ProgramCursor = jump + ObjectFrame;
                            PushStack((uint)@base);
                        }
                        else
                        {
                            PushStack((uint)++@base);
                            PushStack(jump);
                            PushStack((uint)key);
                        }
                    }
                        break;
                    case 0x12:  // Look-up range compare
                    {
                        int top = (int)PopStack();
                        int bottom = (int)PopStack();
                        int key = (int)PopStack();
                        uint jump = PopStack();
                        int @base = (int)PopStack();
                        int range;
                        if (bottom < top)   // 0..12
                        {
                            range = top - bottom + 1;
                            // Are we in range?
                            if (key < range)
                            {
                                ProgramCursor = jump + ObjectFrame;
                                PushStack((uint)(bottom + key - @base));
                                break;
                            }
                        }
                        else                // 0..12
                        {
                            range = bottom - top + 1;
                            // Are we in range?
                            if (key < range)
                            {
                                ProgramCursor = jump + ObjectFrame;
                                PushStack((uint)(bottom - key - @base));
                                break;
                            }
                        }
                        PushStack((uint)@base);
                        PushStack(jump);
                        PushStack((uint)(key - range));
                    }
                        break;
                    case 0x13:  // Look-down range compare
                    {
                        int top = (int)PopStack();
                        int bottom = (int)PopStack();
                        int key = (int)PopStack();
                        uint jump = PopStack();
                        int @base = (int)PopStack();
                        int range;
                        if (bottom < top) // 0..12
                        {
                            range = top - bottom + 1;
                            // Value Found?
                            if (key >= bottom && key <= top)
                            {
                                ProgramCursor = jump + ObjectFrame;
                                PushStack((uint)(key - bottom + @base));
                                break;
                            }
                        }
                        else // 12..0
                        {
                            range = bottom - top + 1;
                            if (key <= bottom && key >= top)
                            {
                                ProgramCursor = jump + ObjectFrame;
                                PushStack((uint)(key - top + @base));
                                break;
                            }
                        }
                        PushStack((uint)(@base + range));
                        PushStack(jump);
                        PushStack((uint)key);
                    }
                        break;
                    case 0x14:  // Quit
                        // TODO: RAISE AN EXCEPTION HERE!
                        System.Windows.Forms.MessageBox.Show("Executed undefined OpCode $14");
                        break;
                    case 0x15:  // Mark Interpreted
                        _interpreterFlag = true;
                        break;
                    case 0x16:  // string size
                    {
                        uint i = 0;
                        for (uint b = PopStack(); CpuHost.DirectReadByte(b) != 0 && b < 0x10000; b++)
                            i++;
                        PushStack(i);
                    }
                        break;
                    case 0x17:  // string compare
                    {
                        uint firstAddress = PopStack();
                        uint secondAddress = PopStack();
                        while (true)
                        {
                            byte firstStr = CpuHost.DirectReadByte(firstAddress++);
                            byte secondStr = CpuHost.DirectReadByte(secondAddress++);
                            if (firstStr != secondStr)
                            {
                                PushStack(0);
                                break;
                            }
                            if (firstStr == 0)
                            {
                                PushStack(0xFFFFFFFF);
                                break;
                            }
                        }
                    }
                        break;
                    case 0x18:  // Byte fill
                    {
                        int count = (int)PopStack();
                        uint src = PopStack();
                        uint dest = PopStack();
                        while (count-- > 0)
                            CpuHost.DirectWriteByte(dest++, (byte)src);
                    }
                        break;
                    case 0x19:  // Word fill
                    {
                        int count = (int)PopStack();
                        uint src = PopStack();
                        uint dest = PopStack();
                        while (count-- > 0)
                        {
                            CpuHost.DirectWriteWord(dest, (ushort)src);
                            dest += 2;
                        }
                    }
                        break;
                    case 0x1A:  // Long fill
                    {
                        int count = (int)PopStack();
                        uint src = PopStack();
                        uint dest = PopStack();
                        while (count-- > 0)
                        {
                            CpuHost.DirectWriteLong(dest, src);
                            dest += 4;
                        }
                    }
                        break;
                    case 0x1B:  // Wait PEQ
                        _isPortB = PopStack() != 0;
                        _maskValue = PopStack();
                        _targetValue = PopStack();
                        State = CogRunState.WaitPinsEqual;
                        break;
                    case 0x1C:  // Byte move
                    {
                        int count = (int)PopStack();
                        uint src = PopStack();
                        uint dest = PopStack();
                        while (count-- > 0)
                            CpuHost.DirectWriteByte(dest++, CpuHost.DirectReadByte(src++));
                    }
                        break;
                    case 0x1D:  // Word move
                    {
                        int count = (int)PopStack();
                        uint src = PopStack();
                        uint dest = PopStack();
                        while (count-- > 0)
                        {
                            CpuHost.DirectWriteWord(dest, CpuHost.DirectReadWord(src));
                            dest += 2;
                            src += 2;
                        }
                    }
                        break;
                    case 0x1E:  // Long move
                    {
                        int count = (int)PopStack();
                        uint src = PopStack();
                        uint dest = PopStack();
                        while (count-- > 0)
                        {
                            CpuHost.DirectWriteLong(dest, CpuHost.DirectReadLong(src));
                            dest += 4;
                            src += 4;
                        }
                    }
                        break;
                    case 0x1F:  // Wait PNE
                        _isPortB = PopStack() != 0;
                        _maskValue = PopStack();
                        _targetValue = PopStack();
                        State = CogRunState.WaitPinsNotEqual;
                        break;
                    case 0x20:  // Clock Set
                    {
                        CpuHost.DirectWriteLong(0, PopStack());
                        byte mode = (byte)PopStack();
                        CpuHost.DirectWriteByte(4, mode);
                        CpuHost.SetClockMode(mode);
                    }
                        break;
                    case 0x21:  // Cog Stop
                    {
                        int cog = (int)PopStack();
                        CpuHost.Stop(cog);
                    }
                        break;
                    case 0x22:  // Lock Return
                        CpuHost.LockReturn(PopStack());
                        break;
                    case 0x23:  // Wait Cnt
                        _targetValue = PopStack();
                        State = CogRunState.WaitCount;
                        break;
                    case 0x24:  // Read Spr
                    {
                        uint address = PopStack();
                        if (address >= 16)
                            break;
                        address += 0x1F0;
                        PushStack(ReadLong(address));
                    }
                        break;
                    case 0x25:  // Write Spr
                    {
                        uint address = PopStack();
                        if (address >= 16)
                            break;
                        address += 0x1F0;
                        WriteLong(address, PopStack());
                    }
                        break;
                    case 0x26:  // USING Spr
                    {
                        uint address = PopStack();
                        if (address >= 16)
                            break;
                        address += 0x1F0;
                        WriteLong(address, InplaceUsingOp(ReadLong(address)));
                    }
                        break;
                    case 0x27:  // Wait Video
                        _pixelsValue = PopStack(); // pixels
                        _colorsValue = PopStack(); // Colors
                        State = CogRunState.WaitVideo;
                        break;
                    case 0x28:  // Cog Init w/Return
                        PushStack(CogInit());
                        break;
                    case 0x29:  // Lock New w/Return
                        PushStack(CpuHost.NewLock());
                        break;
                    case 0x2A:  // Lock Set w/Return
                        PushStack(CpuHost.LockSet(PopStack(), true));
                        break;
                    case 0x2B:  // Lock Clear w/Return
                        PushStack(CpuHost.LockSet(PopStack(), false));
                        break;
                    case 0x2C:  // Cog Init
                        CogInit();
                        break;
                    case 0x2D:  // Lock New
                        CpuHost.NewLock();
                        break;
                    case 0x2E:  // Lock Set
                        CpuHost.LockSet(PopStack(), true);
                        break;
                    case 0x2F:  // Lock Clear
                        CpuHost.LockSet(PopStack(), false);
                        break;
                    case 0x30:  // Abort w/o return
                        ReturnFromSub(CpuHost.DirectReadLong(LocalFrame), true);
                        break;
                    case 0x31:  // Abort w/return
                        ReturnFromSub(PopStack(), true);
                        break;
                    case 0x32:  // Return
                        ReturnFromSub(CpuHost.DirectReadLong(LocalFrame), false);
                        break;
                    case 0x33:  // Pop return value (same as 0x61)
                        ReturnFromSub(PopStack(), false);
                        break;
                    case 0x34:  // Push -1
                    case 0x35:  // Push 0
                    case 0x36:  // Push 1
                        PushStack((uint)(operation - 0x35));
                        break;
                    case 0x37:  // Push Packed Literal
                    {
                        uint value = CpuHost.DirectReadByte(ProgramCursor++);
                        //Bugfix issue #23: implement like ROL pasm instruction, not SHL.
                        uint result = ((uint)2 << (int)(value & 0x1F)) | (uint)2 >> (int)(0x20 - (value & 0x1F));
                        if ((value & 0x20) != 0)
                            result--;
                        if ((value & 0x40) != 0)
                            result = ~result;
                        // TODO Should complain on invalid op if MSB is set
                        PushStack(result);
                    }
                        break;
                    case 0x38:  // Push Byte
                    case 0x39:  // Push Word
                    case 0x3A:  // Push Three Byte
                    case 0x3B:  // Push Long
                    {
                        uint result = 0;
                        for (int i = 0; i < operation - 0x37; i++)
                        {
                            result <<= 8;
                            result |= CpuHost.DirectReadByte(ProgramCursor++);
                        }
                        PushStack(result);
                    }
                        break;
                    case 0x3C:  // UNDEFINED
                        // TODO: ALERT ON BAD OP
                        System.Windows.Forms.MessageBox.Show("Executed undefined OpCode $3C");
                        break;
                    case 0x3D:  // Cog Indexed Memory Op
                    {
                        int bit = (int)PopStack();
                        CogMemoryOp((uint)(1 << bit), bit);
                    }
                        break;
                    case 0x3E:  // Cog Ranged Memory Op
                    {
                        int min = (int)PopStack();
                        int max = (int)PopStack();
                        if (min > max)
                            (min, max) = (max, min);  //swap values
                        uint mask = 0;
                        for (int i = min; i <= max; i++)
                            mask |= (uint)(1 << i);
                        CogMemoryOp(mask, min);
                    }
                        break;
                    case 0x3F:  // Cog Memory op
                        CogMemoryOp(0xFFFFFFFF, 0);
                        break;
                }
            }
            return ProgramCursor != BreakPointCogCursor;
        }

        /// <summary></summary>
        /// <param name="colors"></param>
        /// <param name="pixels"></param>
        /// @pullreq{18} Method added.
        public override void GetVideoData(out uint colors, out uint pixels)
        {
            if (State == CogRunState.WaitVideo)
            {
                colors = _colorsValue;
                pixels = _pixelsValue;
                State = CogRunState.ExecInterpreter;
                FrameFlag = FrameState.FrameHit;
            }
            else
            {
                // Frame counter ran out while not in WAIT_VID
                // True results would depend upon Spin Interpreter operation
                colors = 0;
                pixels = 0;
                FrameFlag = FrameState.FrameMiss;
            }
        }

        /// <summary></summary>
        /// <param name="value"></param>
        /// <param name="abort"></param>
        private void ReturnFromSub(uint value, bool abort)
        {
            bool trapAbort;
            bool wantReturn;
            do
            {
                StackFrame = LocalFrame;
                // Stop on call underflow
                if (_callStack.Count <= 0)
                {
                    CpuHost.Stop((int)CpuHost.CogID(this));
                    return;
                }
                uint returnTypeMask = _callStack.Pop();
                trapAbort = (returnTypeMask & 0x02) != 0;
                wantReturn = (returnTypeMask & 0x01) == 0;
                ProgramCursor = PopStackWord();
                LocalFrame = PopStackWord();
                VariableFrame = PopStackWord();
                ObjectFrame = PopStackWord();
            } while (abort && !trapAbort);
            // Do we return a value?  If so, then push it to the stack
            if (wantReturn)
                PushStack(value);
        }

        /// @brief Execute bytecode operation from cog memory.
        /// @param mask
        /// @param lowestBit
        /// @version v22.05.04 - Parameter name changed to follow naming
        /// conventions.
        /// @todo [Legacy] COMPLAIN ABOUT INVALID OP for 0x3F operations of SPIN bytecode
        ///
        /// Note 3: Details of 0x3F operations {#N3SpecialOpcodes}
        /// ==================================
        /// Read additional byte that follow a cog memory op (<c>0x3F</c>).
        /// |Op  |2nd op| Description |
        /// |:--:|:----:|-------------|
        /// |`3F`|`80+n`|`PUSH    spr`|
        /// |  ^ |`A0+n`|`POP     spr`|
        /// |  ^ |`C0+n`|`USING   spr`|
        /// Source: [Cluso99's SPIN bytecode document revision RR20080721,
        /// on %Propeller 1 Forum](https://forums.parallax.com/discussion/comment/796018/#Comment_796018)
        ///
        private void CogMemoryOp(uint mask, int lowestBit)
        {
            byte op = CpuHost.DirectReadByte(ProgramCursor++);
            uint reg = (uint)((op & 0x1F) + 0x1E0);
            switch (op & 0xE0)
            {
                case 0x80:  // PUSH
                    PushStack((ReadLong(reg) & mask) >> lowestBit);
                    break;
                case 0xA0:  // POP
                {
                    uint val = ReadLong(reg);
                    WriteLong(reg, (val & ~mask) | ((PopStack() << lowestBit) & mask));
                }
                    break;
                case 0xC0:  // USING
                {
                    uint val = ReadLong(reg);
                    WriteLong(reg, (val & ~mask) | ((InplaceUsingOp((val & mask) >> lowestBit) << lowestBit) & mask));
                }
                    break;
                default:
                    // TODO: COMPLAIN ABOUT INVALID OP
                    break;
            }
        }

        /// @brief Execute a bytecode operation from math group.
        /// @param operation Operation code to execute, from range: <c>0xE0..0xFF</c>.
        /// @param swapValues Must swap values from stack (=true ), or maintain order (=false).
        /// @param initial Initial value.
        /// @return Result from math operation.
        /// @version v22.05.04 - Parameter name changed to follow naming
        /// conventions.
        ///
        /// 0xE0..0xFF Math operations OpCodes {#MathOpCodes}
        /// ==================================
        /// 1. Regular math operation
        /// -------------------------
        /// |Structure: Math OpCodes||
        /// |:-----:|:-----------:|
        /// |` 1 1 1`|` q q q q q`|
        /// |`b7b6b5`|`b4b3b2b1b0`|
        /// where
        /// |bit(s)     |Description                    |
        /// |:---------:|:------------------------------|
        /// |`1 1 1`    |Fixed bits equivalent to `0xE0`|
        /// |`q q q q q`|Bits on table                  |
        ///
        /// 2. Math Assigment operation (USING)
        /// -----------------------------------
        /// When an operation in Memory or Variables ops groups define USING 2nd OpCode,
        /// the math asignment use a different format.
        /// |Structure: Maths OpCodes, "op2"||
        /// |:------:|:----------:|
        /// |` 1 p s`|` q q q q q`|
        /// |`b7b6b5`|`b4b3b2b1b0`|
        /// where
        /// |Bit(s)     |Description                   ||
        /// |:---------:|:----:|------------------------|
        /// |`1`        |Fixed bit equivalent to `0x80`||
        /// |`p`        |`(!p)`|`0`= push                |
        /// | ^         |  ^   |`1`= no push             |
        /// |`s`        |`(!s)`|`0`= swap binary args    |
        /// | ^         |  ^   |`1`= no swap             |
        /// |`q q q q q`|Bits on table                 ||
        /// Bytecode Table: 32 operations
        /// -----------------------------
        /// <table>
        ///     <tr>
        ///         <th align="center" colspan="2">Equiv instr</th>
        ///         <th align="center">Op</th>
        ///         <th align="center">Bits</th>
        ///         <th align="center" colspan="2">Using Operation</th>
        ///         <th align="center">Unitary/Binary</th>
        ///         <th align="center">Normal</th>
        ///         <th align="center">Assign</th>
        ///         <th align="left">Description</th>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>ROR</code></td>
        ///         <td align="center"><code>0x041</code></td>
        ///         <td align="center"><code>E0</code></td>
        ///         <td align="center"><code>00000</code></td>
        ///         <td align="center"><code>ROR</code></td>
        ///         <td align="center"><code>1st -&gt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>-&gt;</code></td>
        ///         <td align="center"><code>-&gt;=</code></td>
        ///         <td align="left">rotate right</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>ROL</code></td>
        ///         <td align="center"><code>0x049</code></td>
        ///         <td align="center"><code>E1</code></td>
        ///         <td align="center"><code>00001</code></td>
        ///         <td align="center"><code>ROL</code></td>
        ///         <td align="center"><code>1st &lt;- 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>&lt;-</code></td>
        ///         <td align="center"><code>&lt;-=</code></td>
        ///         <td align="left">rotate left</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>SHR</code></td>
        ///         <td align="center"><code>0x051</code></td>
        ///         <td align="center"><code>E2</code></td>
        ///         <td align="center"><code>00010</code></td>
        ///         <td align="center"><code>SHR</code></td>
        ///         <td align="center"><code>1st &gt;&gt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>&gt;&gt;</code></td>
        ///         <td align="center"><code>&gt;&gt;=</code></td>
        ///         <td align="left">shift right</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>SHL</code></td>
        ///         <td align="center"><code>0x059</code></td>
        ///         <td align="center"><code>E3</code></td>
        ///         <td align="center"><code>00011</code></td>
        ///         <td align="center"><code>SHL</code></td>
        ///         <td align="center"><code>1st &lt;&lt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>&lt;&lt;</code></td>
        ///         <td align="center"><code>&lt;&lt;=</code></td>
        ///         <td align="left">shift left</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>MINS</code></td>
        ///         <td align="center"><code>0x081</code></td>
        ///         <td align="center"><code>E4</code></td>
        ///         <td align="center"><code>00100</code></td>
        ///         <td align="center"><code>MINs</code></td>
        ///         <td align="center"><code>1st #&gt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>#&gt;</code></td>
        ///         <td align="center"><code>#&gt;=</code></td>
        ///         <td align="left">limit minimum (signed)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>MAXS</code></td>
        ///         <td align="center"><code>0x089</code></td>
        ///         <td align="center"><code>E5</code></td>
        ///         <td align="center"><code>00101</code></td>
        ///         <td align="center"><code>MAXs</code></td>
        ///         <td align="center"><code>1st &lt;# 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>&lt;#</code></td>
        ///         <td align="center"><code>&lt;#=</code></td>
        ///         <td align="left">limit maximum (signed)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>NEG</code></td>
        ///         <td align="center"><code>0x149</code></td>
        ///         <td align="center"><code>E6</code></td>
        ///         <td align="center"><code>00110</code></td>
        ///         <td align="center"><code>NEG</code></td>
        ///         <td align="center"><code>- 1st</code></td>
        ///         <td align="center">unitary</td>
        ///         <td align="center"><code>-</code></td>
        ///         <td align="center"><code>-</code></td>
        ///         <td align="left">negate</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>E7</code></td>
        ///         <td align="center"><code>00111</code></td>
        ///         <td align="center"><code>BIT_NOT</code></td>
        ///         <td align="center"><code>! 1st</code></td>
        ///         <td align="center">unitary</td>
        ///         <td align="center"><code>!</code></td>
        ///         <td align="center"><code>!</code></td>
        ///         <td align="left">bitwise not</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>AND</code></td>
        ///         <td align="center"><code>0x0C1</code></td>
        ///         <td align="center"><code>E8</code></td>
        ///         <td align="center"><code>01000</code></td>
        ///         <td align="center"><code>BIT_AND</code></td>
        ///         <td align="center"><code>1st &amp; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>&amp;</code></td>
        ///         <td align="center"><code>&amp;=</code></td>
        ///         <td align="left">bitwise and</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>ABS</code></td>
        ///         <td align="center"><code>0x151</code></td>
        ///         <td align="center"><code>E9</code></td>
        ///         <td align="center"><code>01001</code></td>
        ///         <td align="center"><code>ABS</code></td>
        ///         <td align="center"><code>ABS(1st)</code></td>
        ///         <td align="center">unitary</td>
        ///         <td align="center"><code>||</code></td>
        ///         <td align="center"><code>||</code></td>
        ///         <td align="left">absolute</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>OR</code></td>
        ///         <td align="center"><code>0x0D1</code></td>
        ///         <td align="center"><code>EA</code></td>
        ///         <td align="center"><code>01010</code></td>
        ///         <td align="center"><code>BIT_OR</code></td>
        ///         <td align="center"><code>1st | 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>|</code></td>
        ///         <td align="center"><code>|=</code></td>
        ///         <td align="left">bitwise or</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>XOR</code></td>
        ///         <td align="center"><code>0x0D9</code></td>
        ///         <td align="center"><code>EB</code></td>
        ///         <td align="center"><code>01011</code></td>
        ///         <td align="center"><code>BIT_XOR</code></td>
        ///         <td align="center"><code>1st ^ 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>^</code></td>
        ///         <td align="center"><code>^=</code></td>
        ///         <td align="left">bitwise xor</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>ADD</code></td>
        ///         <td align="center"><code>0x101</code></td>
        ///         <td align="center"><code>EC</code></td>
        ///         <td align="center"><code>01100</code></td>
        ///         <td align="center"><code>ADD</code></td>
        ///         <td align="center"><code>1st + 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>+</code></td>
        ///         <td align="center"><code>+=</code></td>
        ///         <td align="left">add</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>SUB</code></td>
        ///         <td align="center"><code>0x109</code></td>
        ///         <td align="center"><code>ED</code></td>
        ///         <td align="center"><code>01101</code></td>
        ///         <td align="center"><code>SUB</code></td>
        ///         <td align="center"><code>1st - 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>-</code></td>
        ///         <td align="center"><code>-=</code></td>
        ///         <td align="left">subtract</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>SAR</code></td>
        ///         <td align="center"><code>0x071</code></td>
        ///         <td align="center"><code>EE</code></td>
        ///         <td align="center"><code>01110</code></td>
        ///         <td align="center"><code>SAR</code></td>
        ///         <td align="center"><code>1st ~&gt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>~&gt;</code></td>
        ///         <td align="center"><code>~&gt;=</code></td>
        ///         <td align="left">shift arithmetic right</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>EF</code></td>
        ///         <td align="center"><code>01111</code></td>
        ///         <td align="center"><code>BIT_REV</code></td>
        ///         <td align="center"><code>1st &gt;&lt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>&gt;&lt;</code></td>
        ///         <td align="center"><code>&gt;&lt;=</code></td>
        ///         <td align="left">reverse bits (neg y first)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>AND</code></td>
        ///         <td align="center"><code>0x0C1</code></td>
        ///         <td align="center"><code>F0</code></td>
        ///         <td align="center"><code>10000</code></td>
        ///         <td align="center"><code>LOG_AND</code></td>
        ///         <td align="center"><code>1st AND 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>AND</code></td>
        ///         <td align="center">&nbsp;</td>
        ///         <td align="left">boolean and</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>F1</code></td>
        ///         <td align="center"><code>10001</code></td>
        ///         <td align="center"><code>ENCODE</code></td>
        ///         <td align="center"><code>&gt;| 1st</code></td>
        ///         <td align="center">unitary</td>
        ///         <td align="center"><code>&gt;|</code></td>
        ///         <td align="center"><code>&gt;|</code></td>
        ///         <td align="left">encode (0-32)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center"><code>OR</code></td>
        ///         <td align="center"><code>0x0D1</code></td>
        ///         <td align="center"><code>F2</code></td>
        ///         <td align="center"><code>10010</code></td>
        ///         <td align="center"><code>LOG_OR</code></td>
        ///         <td align="center"><code>1st OR 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>OR</code></td>
        ///         <td align="center">&nbsp;</td>
        ///         <td align="left">boolean or</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>F3</code></td>
        ///         <td align="center"><code>10011</code></td>
        ///         <td align="center"><code>DECODE</code></td>
        ///         <td align="center"><code>|&lt; 1st</code></td>
        ///         <td align="center">unitary</td>
        ///         <td align="center"><code>|&lt;</code></td>
        ///         <td align="center"><code>|&lt;</code></td>
        ///         <td align="left">decode</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>F4</code></td>
        ///         <td align="center"><code>10100</code></td>
        ///         <td align="center"><code>MPY</code></td>
        ///         <td align="center"><code>1st * 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>*</code></td>
        ///         <td align="center"><code>*=</code></td>
        ///         <td align="left">multiply, return lower half (signed)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>F5</code></td>
        ///         <td align="center"><code>10101</code></td>
        ///         <td align="center"><code>MPY_MSW</code></td>
        ///         <td align="center"><code>1st ** 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>**</code></td>
        ///         <td align="center"><code>**=</code></td>
        ///         <td align="left">multiply, return upper half (signed)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>F6</code></td>
        ///         <td align="center"><code>10110</code></td>
        ///         <td align="center"><code>DIV</code></td>
        ///         <td align="center"><code>1st / 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>/</code></td>
        ///         <td align="center"><code>/=</code></td>
        ///         <td align="left">divide, return quotient (signed)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>F7</code></td>
        ///         <td align="center"><code>10111</code></td>
        ///         <td align="center"><code>MOD</code></td>
        ///         <td align="center"><code>1st // 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>//</code></td>
        ///         <td align="center"><code>//=</code></td>
        ///         <td align="left">divide, return remainder (signed)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>F8</code></td>
        ///         <td align="center"><code>11000</code></td>
        ///         <td align="center"><code>SQRT</code></td>
        ///         <td align="center"><code>^^ 1st</code></td>
        ///         <td align="center">unitary</td>
        ///         <td align="center"><code>^^</code></td>
        ///         <td align="center"><code>^^</code></td>
        ///         <td align="left">square root</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>F9</code></td>
        ///         <td align="center"><code>11001</code></td>
        ///         <td align="center"><code>LT</code></td>
        ///         <td align="center"><code>1st &lt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>&lt;</code></td>
        ///         <td align="center">&nbsp;</td>
        ///         <td align="left">test below (signed)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>FA</code></td>
        ///         <td align="center"><code>11010</code></td>
        ///         <td align="center"><code>GT</code></td>
        ///         <td align="center"><code>1st &gt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>&gt;</code></td>
        ///         <td align="center">&nbsp;</td>
        ///         <td align="left">test above (signed)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>FB</code></td>
        ///         <td align="center"><code>11011</code></td>
        ///         <td align="center"><code>NE</code></td>
        ///         <td align="center"><code>1st &lt;&gt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>&lt;&gt;</code></td>
        ///         <td align="center">&nbsp;</td>
        ///         <td align="left">test not equal</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>FC</code></td>
        ///         <td align="center"><code>11100</code></td>
        ///         <td align="center"><code>EQ</code></td>
        ///         <td align="center"><code>1st == 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>==</code></td>
        ///         <td align="center">&nbsp;</td>
        ///         <td align="left">test equal</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>FD</code></td>
        ///         <td align="center"><code>11101</code></td>
        ///         <td align="center"><code>LE</code></td>
        ///         <td align="center"><code>1st =&lt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>=&lt;</code></td>
        ///         <td align="center">&nbsp;</td>
        ///         <td align="left">test below or equal (signed)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>FE</code></td>
        ///         <td align="center"><code>11110</code></td>
        ///         <td align="center"><code>GE</code></td>
        ///         <td align="center"><code>1st =&gt; 2nd</code></td>
        ///         <td align="center">binary</td>
        ///         <td align="center"><code>=&gt;</code></td>
        ///         <td align="center">&nbsp;</td>
        ///         <td align="left">test above or equal (signed)</td>
        ///     </tr>
        ///     <tr>
        ///         <td align="center" colspan="2">&nbsp;</td>
        ///         <td align="center"><code>FF</code></td>
        ///         <td align="center"><code>11111</code></td>
        ///         <td align="center"><code>LOG_NOT</code></td>
        ///         <td align="center"><code>NOT 1st</code></td>
        ///         <td align="center">unitary</td>
        ///         <td align="center"><code>NOT</code></td>
        ///         <td align="center"><code>NOT</code></td>
        ///         <td align="left">boolean not</td>
        ///     </tr>
        /// </table>
        /// Source: [Cluso99's SPIN bytecode document revision RR20080721,
        /// on %Propeller 1 Forum](https://forums.parallax.com/discussion/comment/796018/#Comment_796018)
        ///
        private uint BaseMathOp(byte operation, bool swapValues, uint initial)
        {
            // --- Unary Operators ---
            switch (operation)
            {
                case 0x06:  // Negate
                    return (uint)-(int)initial;
                case 0x07:  // Complement
                    return ~initial;
                case 0x09:  // Absolute Value
                {
                    int mask = (int)initial;
                    return mask < 0 ?
                        (uint)-mask :
                        (uint)mask;
                }
                case 0x11:  // Encode (MSB set)
                    for (int i = 31; i >= 0; i--)
                        if (((initial >> i) & 1) != 0)
                            return (uint)(i + 1);
                    return 0;
                case 0x13:  // Decode
                    return (uint)1 << (int)(initial & 0x1F);
                case 0x18:  // Square Root
                    return (uint)Math.Sqrt(initial);
                case 0x1F:    // Logical Not
                    return initial == 0 ?
                        0xFFFFFFFF :
                        0x0;
            }

            // --- Binary Operators ---
            uint left;
            uint right;
            if (swapValues)
            {
                left = PopStack();
                right = initial;
            }
            else
            {
                left = initial;
                right = PopStack();
            }

            switch (operation)
            {
                case 0x00:  // Rotate right
                {
                    int shift = (int)right;
                    ulong mask = left;
                    mask |= mask << 32;
                    mask >>= shift;
                    return (uint)mask;
                }
                case 0x01:  // Rotate left
                {
                    int shift = (int)right;
                    ulong mask = left;
                    mask |= mask << 32;
                    mask <<= shift;
                    return (uint)(mask >> 32);
                }
                case 0x02:  // Shift right
                    return left >> (int)right;
                case 0x03:  // Shift left
                    return left << (int)right;
                case 0x04:  // Limit Min
                    return (int)left < (int)right ?
                        right :
                        left;
                case 0x05:  // Limit Max
                    return (int)left > (int)right ?
                        right :
                        left;
                case 0x08:  // Bitwise AND
                    return right & left;
                case 0x0A:  // Bitwise OR
                    return right | left;
                case 0x0B:  // Bitwise Exclusive-Or
                    return right ^ left;
                case 0x0C:  // Add
                    return (uint)((int)left + (int)right);
                case 0x0D:  // Subtract
                    return (uint)((int)left - (int)right);
                case 0x0E:  // Arithmetic shift right
                {
                    int shift = (int)right;
                    ulong mask = left;
                    if ((mask & 0x80000000) != 0)
                        mask |= 0xFFFFFFFF00000000;
                    mask >>= shift;
                    return (uint)mask;
                }
                case 0x0F:  // Bit reverse
                {
                    uint mask = 0;
                    int bits = (int)right;
                    for (int i = 0; bits > 0; bits--, i++)
                        mask |= ((left >> i) & 1) << (bits - 1);
                    return mask;
                }
                case 0x10:  // Logical AND
                    return right != 0 && left != 0 ?
                        0xFFFFFFFF :
                        0x0;
                case 0x12:  // Logical OR
                    return right != 0 || left != 0 ?
                        0xFFFFFFFF :
                        0x0;
                case 0x14:  // Multiply
                    return (uint)((int)left * (int)right);
                case 0x15:  // Multiply HI
                    return (uint)(((long)(int)left * (long)(int)right) >> 32);
                case 0x16:  // Divide
                    if (right == 0)
                        return 0xFFFFFFFF;
                    return (uint)((int)left / (int)right);
                case 0x17:  // Modulo
                    if (right == 0)
                        return 0xFFFFFFFF;
                    return (uint)((int)left % (int)right);
                case 0x19:  // Less
                    return (int)left < (int)right ?
                        0xFFFFFFFF :
                        0x0;
                case 0x1A:  // Greater
                    return (int)left > (int)right ?
                        0xFFFFFFFF :
                        0x0;
                case 0x1B:  // Not Equal
                    return (int)left != (int)right ?
                        0xFFFFFFFF :
                        0x0;
                case 0x1C:  // Equal
                    return (int)left == (int)right ?
                        0xFFFFFFFF :
                        0x0;
                case 0x1D:  // Less than or Equal
                    return (int)left <= (int)right ?
                        0xFFFFFFFF :
                        0x0;
                case 0x1E:  // Greater than or equal
                    return (int)left >= (int)right ?
                        0xFFFFFFFF :
                        0x0;
            }
            // Emergency condition: Should never occur
            return 0;
        }

        /// @brief Execute a bytecode operation from memory group.
        /// @param operation Operation code to execute, from range: <c>0x80..0xDF</c>.
        /// @version v22.05.04 - Changed parameter name to clarify meaning
        /// of it also changed local variable names to follow naming
        /// conventions or to clarify meaning of it.
        ///
        /// 0x80..0xDF Memory ops (Access MEM, OBJ, VAR and LOC) {#MemoryOps}
        /// ====================================================
        /// |Structure: Stack load/save OpCodes|||||
        /// |:--:|:----:|:--:|:----:|:----:|
        /// |` 1`|` s s`|` i`|` b b`|` q q`|
        /// |`b7`|`b6b5`|`b4`|`b3b2`|`b1b0`|
        /// where
        /// |bit(s)|Description                     ||
        /// |:----:|:---------------------|---------:|
        /// |`1`   |Fixed bit equivalent to `0x80`  ||
        /// |`s s` |` 00`= Byte                     ||
        /// |  ^   |` 01`= Word                     ||
        /// |  ^   |` 10`= Long                     ||
        /// |  ^   |(`11`= would convert in Math op)||
        /// |`i`   |`0`= no offset                  ||
        /// | ^    |`1`=[]= add offset (indexed)    ||
        /// |`b b` |`00`= MEM  base popped from stack|if i=1 add offset             |
        /// |  ^   |`01`= OBJ  base is object base   |if i=1 add offset             |
        /// |  ^   |`10`= VAR  base is variable base |if i=1 add offset             |
        /// |  ^   |`11`= LOC  base is stack base    |if i=1 add offset             |
        /// |`q q` |`00`= PUSH   Read  - push result in stack                      ||
        /// |  ^   |`01`= POP    Write - pop value from stack                      ||
        /// |  ^   |`10`= USING  2nd OpCode (assignment) executed, result in target||
        /// |  ^   |`11`= PUSH#  Push address of destination onto stack            ||
        /// Bytecode Table: 96 operations
        /// -----------------------------
        /// |Op  |Byte      |Size|Indexed|Description  ||
        /// |:--:|:--------:|:--:|:-----:|-------------||
        /// |`80`|`10000000`|Byte|       |`MEM  PUSH`  ||
        /// |`81`|`10000001`|Byte|       |`MEM  POP`   ||
        /// |`82`|`10000010`|Byte|       |`MEM  USING` ||
        /// |`83`|`10000011`|Byte|       |`MEM  PUSH #`||
        /// |`84`|`10000100`|Byte|       |`OBJ  PUSH`  ||
        /// |`85`|`10000101`|Byte|       |`OBJ  POP`   ||
        /// |`86`|`10000110`|Byte|       |`OBJ  USING` ||
        /// |`87`|`10000111`|Byte|       |`OBJ  PUSH #`||
        /// |`88`|`10001000`|Byte|       |`VAR  PUSH`  ||
        /// |`89`|`10001001`|Byte|       |`VAR  POP`   ||
        /// |`8A`|`10001010`|Byte|       |`VAR  USING` ||
        /// |`8B`|`10001011`|Byte|       |`VAR  PUSH #`||
        /// |`8C`|`10001100`|Byte|       |`LOC  PUSH`  ||
        /// |`8D`|`10001101`|Byte|       |`LOC  POP`   ||
        /// |`8E`|`10001110`|Byte|       |`LOC  USING` ||
        /// |`8F`|`10001111`|Byte|       |`LOC  PUSH #`||
        /// |`90`|`10010000`|Byte|`[]`   |`MEM  PUSH`  ||
        /// |`91`|`10010001`|Byte|`[]`   |`MEM  POP`   ||
        /// |`92`|`10010010`|Byte|`[]`   |`MEM  USING` ||
        /// |`93`|`10010011`|Byte|`[]`   |`MEM  PUSH #`||
        /// |`94`|`10010100`|Byte|`[]`   |`OBJ  PUSH`  ||
        /// |`95`|`10010101`|Byte|`[]`   |`OBJ  POP`   ||
        /// |`96`|`10010110`|Byte|`[]`   |`OBJ  USING` ||
        /// |`97`|`10010111`|Byte|`[]`   |`OBJ  PUSH #`||
        /// |`98`|`10011000`|Byte|`[]`   |`VAR  PUSH`  ||
        /// |`99`|`10011001`|Byte|`[]`   |`VAR  POP`   ||
        /// |`9A`|`10011010`|Byte|`[]`   |`VAR  USING` ||
        /// |`9B`|`10011011`|Byte|`[]`   |`VAR  PUSH #`||
        /// |`9C`|`10011100`|Byte|`[]`   |`LOC  PUSH`  ||
        /// |`9D`|`10011101`|Byte|`[]`   |`LOC  POP`   ||
        /// |`9E`|`10011110`|Byte|`[]`   |`LOC  USING` ||
        /// |`9F`|`10011111`|Byte|`[]`   |`LOC  PUSH #`||
        /// |`A0`|`10100000`|Word|       |`MEM  PUSH`  ||
        /// |`A1`|`10100001`|Word|       |`MEM  POP`   ||
        /// |`A2`|`10100010`|Word|       |`MEM  USING` ||
        /// |`A3`|`10100011`|Word|       |`MEM  PUSH #`||
        /// |`A4`|`10100100`|Word|       |`OBJ  PUSH`  ||
        /// |`A5`|`10100101`|Word|       |`OBJ  POP`   ||
        /// |`A6`|`10100110`|Word|       |`OBJ  USING` ||
        /// |`A7`|`10100111`|Word|       |`OBJ  PUSH #`||
        /// |`A8`|`10101000`|Word|       |`VAR  PUSH`  ||
        /// |`A9`|`10101001`|Word|       |`VAR  POP`   ||
        /// |`AA`|`10101010`|Word|       |`VAR  USING` ||
        /// |`AB`|`10101011`|Word|       |`VAR  PUSH #`||
        /// |`AC`|`10101100`|Word|       |`LOC  PUSH`  ||
        /// |`AD`|`10101101`|Word|       |`LOC  POP`   ||
        /// |`AE`|`10101110`|Word|       |`LOC  USING` ||
        /// |`AF`|`10101111`|Word|       |`LOC  PUSH #`||
        /// |`B0`|`10110000`|Word|`[]`   |`MEM  PUSH`  ||
        /// |`B1`|`10110001`|Word|`[]`   |`MEM  POP`   ||
        /// |`B2`|`10110010`|Word|`[]`   |`MEM  USING` ||
        /// |`B3`|`10110011`|Word|`[]`   |`MEM  PUSH #`||
        /// |`B4`|`10110100`|Word|`[]`   |`OBJ  PUSH`  ||
        /// |`B5`|`10110101`|Word|`[]`   |`OBJ  POP`   ||
        /// |`B6`|`10110110`|Word|`[]`   |`OBJ  USING` ||
        /// |`B7`|`10110111`|Word|`[]`   |`OBJ  PUSH #`||
        /// |`B8`|`10111000`|Word|`[]`   |`VAR  PUSH`  ||
        /// |`B9`|`10111001`|Word|`[]`   |`VAR  POP`   ||
        /// |`BA`|`10111010`|Word|`[]`   |`VAR  USING` ||
        /// |`BB`|`10111011`|Word|`[]`   |`VAR  PUSH #`||
        /// |`BC`|`10111100`|Word|`[]`   |`LOC  PUSH`  ||
        /// |`BD`|`10111101`|Word|`[]`   |`LOC  POP`   ||
        /// |`BE`|`10111110`|Word|`[]`   |`LOC  USING` ||
        /// |`BF`|`10111111`|Word|`[]`   |`LOC  PUSH #`||
        /// |`C0`|`11000000`|Long|       |`MEM  PUSH`  ||
        /// |`C1`|`11000001`|Long|       |`MEM  POP`   ||
        /// |`C2`|`11000010`|Long|       |`MEM  USING` ||
        /// |`C3`|`11000011`|Long|       |`MEM  PUSH #`||
        /// |`C4`|`11000100`|Long|       |`OBJ  PUSH`  ||
        /// |`C5`|`11000101`|Long|       |`OBJ  POP`   ||
        /// |`C6`|`11000110`|Long|       |`OBJ  USING` ||
        /// |`C7`|`11000111`|Long|       |`OBJ  PUSH #`||
        /// |`C8`|`11001000`|Long|       |`VAR  PUSH`  |`see also 0x40..0x7F bytecodes`|
        /// |`C9`|`11001001`|Long|       |`VAR  POP`   |           ^                   |
        /// |`CA`|`11001010`|Long|       |`VAR  USING` |           ^                   |
        /// |`CB`|`11001011`|Long|       |`VAR  PUSH #`|           ^                   |
        /// |`CC`|`11001100`|Long|       |`LOC  PUSH`  |           ^                   |
        /// |`CD`|`11001101`|Long|       |`LOC  POP`   |           ^                   |
        /// |`CE`|`11001110`|Long|       |`LOC  USING` |           ^                   |
        /// |`CF`|`11001111`|Long|       |`LOC  PUSH #`|           ^                   |
        /// |`D0`|`11010000`|Long|`[]`   |`MEM  PUSH`  ||
        /// |`D1`|`11010001`|Long|`[]`   |`MEM  POP`   ||
        /// |`D2`|`11010010`|Long|`[]`   |`MEM  USING` ||
        /// |`D3`|`11010011`|Long|`[]`   |`MEM  PUSH #`||
        /// |`D4`|`11010100`|Long|`[]`   |`OBJ  PUSH`  ||
        /// |`D5`|`11010101`|Long|`[]`   |`OBJ  POP`   ||
        /// |`D6`|`11010110`|Long|`[]`   |`OBJ  USING` ||
        /// |`D7`|`11010111`|Long|`[]`   |`OBJ  PUSH #`||
        /// |`D8`|`11011000`|Long|`[]`   |`VAR  PUSH`  ||
        /// |`D9`|`11011001`|Long|`[]`   |`VAR  POP`   ||
        /// |`DA`|`11011010`|Long|`[]`   |`VAR  USING` ||
        /// |`DB`|`11011011`|Long|`[]`   |`VAR  PUSH #`||
        /// |`DC`|`11011100`|Long|`[]`   |`LOC  PUSH`  ||
        /// |`DD`|`11011101`|Long|`[]`   |`LOC  POP`   ||
        /// |`DE`|`11011110`|Long|`[]`   |`LOC  USING` ||
        /// |`DF`|`11011111`|Long|`[]`   |`LOC  PUSH #`||
        /// Source: [Cluso99's SPIN bytecode document revision RR20080721,
        /// on %Propeller 1 Forum](https://forums.parallax.com/discussion/comment/796018/#Comment_796018)
        ///
        private void StepMaskedMemoryOp(byte operation)
        {
            byte type = (byte)(operation & 0x03);     // Bit[0..1] = POP, PUSH, USING, REFERENCE
            byte @base = (byte)(operation & 0x0C);    // Bit[2..3] = MAIN, OBJ, VAR, LOCAL
            byte indexed = (byte)(operation & 0x10);  // Bit[4] = Indexed ?
            byte size = (byte)(operation & 0x60);     // Bit[5..6] = BYTE, WORD, LONG
            uint address;
            switch (@base)
            {
                case 0x00:  // Main Memory
                    address = PopStack();
                    break;
                case 0x04:  // Object Memory
                    address = ObjectFrame + ReadPackedUnsignedWord();
                    break;
                case 0x08:  // Variable Memory
                    address = VariableFrame + ReadPackedUnsignedWord();
                    break;
                default:    // Local Memory
                    address = LocalFrame + ReadPackedUnsignedWord();
                    break;
            }

            if (indexed != 0)
            {
                if (@base == 0x00)
                    address = PopStack() + (address << (size >> 5));
                else
                    address += PopStack() << (size >> 5);
            }

            switch (type)
            {
                case 0: // PUSH is already done
                    switch (size)
                    {
                        case 0x00:  // byte
                            PushStack(CpuHost.DirectReadByte(address));
                            break;
                        case 0x20:  // word
                            PushStack(CpuHost.DirectReadWord(address));
                            break;
                        case 0x40:  // long
                            PushStack(CpuHost.DirectReadLong(address));
                            break;
                    }
                    break;
                case 1: // POP
                    switch (size)
                    {
                        case 0x00:  // byte
                            CpuHost.DirectWriteByte(address, (byte)PopStack());
                            break;
                        case 0x20:  // word
                            CpuHost.DirectWriteWord(address, (ushort)PopStack());
                            break;
                        case 0x40:  // long
                            CpuHost.DirectWriteLong(address, PopStack());
                            break;
                    }
                    break;
                case 2: // USING
                    switch (size)
                    {
                        case 0x00:  // byte
                            CpuHost.DirectWriteByte(address, (byte)InplaceUsingOp(CpuHost.DirectReadByte(address)));
                            break;
                        case 0x20:  // word
                            CpuHost.DirectWriteWord(address, (ushort)InplaceUsingOp(CpuHost.DirectReadWord(address)));
                            break;
                        case 0x40:  // long
                            CpuHost.DirectWriteLong(address, InplaceUsingOp(CpuHost.DirectReadLong(address)));
                            break;
                    }
                    break;
                case 3: // REFERENCE
                    PushStack(address);
                    break;
            }
        }

        /// @brief Execute a bytecode operation from variable group.
        /// @param operation Operation code to execute, from range: <c>0x40..0x7F</c>.
        /// @version v22.05.04 - Changed parameter name to clarify meaning
        /// of it also changed local variable names to follow naming
        /// conventions or to clarify meaning of it.
        ///
        /// 0x40..0x7F Variables ops (Fast access VAR, LOC) {#VariableOps}
        /// ===============================================
        /// These OpCodes allow fast access by making long access to the first
        /// few long entries in the variable space or stack a single byte OpCode.
        /// The single byte OpCodes are effectively expanded within the interpreter.
        /// |Structure: Variables OpCodes||||
        /// |:----:|:--:|:------:|:----:|
        /// |` 0 1`|` b`|` v v v`|` q q`|
        /// |`b7b6`|`b5`|`b4b3b2`|`b1b0`|
        /// is expanded to:
        /// |Word                                            |||||||
        /// |:------------:|:--:|:----:|--|:------:|:------:|:----:|
        /// |` 1 ?1? 0 0 1`|` b`|` q q`|  |` 0 0 0`|` v v v`|` 0 0`|
        /// |`bF bE bDbCbB`|`bA`|`b9b8`|  |`b7b6b5`|`b4b3b2`|`b1b0`|
        /// where
        /// |Bit(s)  |Description                    ||
        /// |:------:|:------------------------------||
        /// |` 0 1`  |Fixed bits equivalent to `0x40`||
        /// |` b`    |`0`= VAR                       ||
        /// |  ^     |`1`= LOC                       ||
        /// |` v v v`|Address (adr = v*4)            ||
        /// |` q q`  |`00`= PUSH |Read  - push result in stack                      |
        /// |   ^    |`01`= POP  |Write - pop value from stack                      |
        /// |   ^    |`10`= USING|2nd OpCode (assignment) executed, result in target|
        /// |   ^    |`11`= PUSH#|Push address of destination onto stack            |
        /// Bytecode Table: 64 operations
        /// -----------------------------
        /// | Op |Byte      |Description                |
        /// |:--:|:--------:|:--------------------------|
        /// |`40`|`01000000`|`VAR  PUSH    addr=0*4= 00`|
        /// |`41`|`01000001`|`VAR  POP     addr=0*4= 00`|
        /// |`42`|`01000010`|`VAR  USING   addr=0*4= 00`|
        /// |`43`|`01000011`|`VAR  PUSH #  addr=0*4= 00`|
        /// |`44`|`01000100`|`VAR  PUSH    addr=1*4= 04`|
        /// |`45`|`01000101`|`VAR  POP     addr=1*4= 04`|
        /// |`46`|`01000110`|`VAR  USING   addr=1*4= 04`|
        /// |`47`|`01000111`|`VAR  PUSH #  addr=1*4= 04`|
        /// |`48`|`01001000`|`VAR  PUSH    addr=2*4= 08`|
        /// |`49`|`01001001`|`VAR  POP     addr=2*4= 08`|
        /// |`4A`|`01001010`|`VAR  USING   addr=2*4= 08`|
        /// |`4B`|`01001011`|`VAR  PUSH #  addr=2*4= 08`|
        /// |`4C`|`01001100`|`VAR  PUSH    addr=3*4= 0C`|
        /// |`4D`|`01001101`|`VAR  POP     addr=3*4= 0C`|
        /// |`4E`|`01001110`|`VAR  USING   addr=3*4= 0C`|
        /// |`4F`|`01001111`|`VAR  PUSH #  addr=3*4= 0C`|
        /// |`50`|`01010000`|`VAR  PUSH    addr=4*4= 10`|
        /// |`51`|`01010001`|`VAR  POP     addr=4*4= 10`|
        /// |`52`|`01010010`|`VAR  USING   addr=4*4= 10`|
        /// |`53`|`01010011`|`VAR  PUSH #  addr=4*4= 10`|
        /// |`54`|`01010100`|`VAR  PUSH    addr=5*4= 14`|
        /// |`55`|`01010101`|`VAR  POP     addr=5*4= 14`|
        /// |`56`|`01010110`|`VAR  USING   addr=5*4= 14`|
        /// |`57`|`01010111`|`VAR  PUSH #  addr=5*4= 14`|
        /// |`58`|`01011000`|`VAR  PUSH    addr=6*4= 18`|
        /// |`59`|`01011001`|`VAR  POP     addr=6*4= 18`|
        /// |`5A`|`01011010`|`VAR  USING   addr=6*4= 18`|
        /// |`5B`|`01011011`|`VAR  PUSH #  addr=6*4= 18`|
        /// |`5C`|`01011100`|`VAR  PUSH    addr=7*4= 1C`|
        /// |`5D`|`01011101`|`VAR  POP     addr=7*4= 1C`|
        /// |`5E`|`01011110`|`VAR  USING   addr=7*4= 1C`|
        /// |`5F`|`01011111`|`VAR  PUSH #  addr=7*4= 1C`|
        /// |`60`|`01100000`|`LOC  PUSH    addr=0*4= 00`|
        /// |`61`|`01100001`|`LOC  POP     addr=0*4= 00`|
        /// |`62`|`01100010`|`LOC  USING   addr=0*4= 00`|
        /// |`63`|`01100011`|`LOC  PUSH #  addr=0*4= 00`|
        /// |`64`|`01100100`|`LOC  PUSH    addr=1*4= 04`|
        /// |`65`|`01100101`|`LOC  POP     addr=1*4= 04`|
        /// |`66`|`01100110`|`LOC  USING   addr=1*4= 04`|
        /// |`67`|`01100111`|`LOC  PUSH #  addr=1*4= 04`|
        /// |`68`|`01101000`|`LOC  PUSH    addr=2*4= 08`|
        /// |`69`|`01101001`|`LOC  POP     addr=2*4= 08`|
        /// |`6A`|`01101010`|`LOC  USING   addr=2*4= 08`|
        /// |`6B`|`01101011`|`LOC  PUSH #  addr=2*4= 08`|
        /// |`6C`|`01101100`|`LOC  PUSH    addr=3*4= 0C`|
        /// |`6D`|`01101101`|`LOC  POP     addr=3*4= 0C`|
        /// |`6E`|`01101110`|`LOC  USING   addr=3*4= 0C`|
        /// |`6F`|`01101111`|`LOC  PUSH #  addr=3*4= 0C`|
        /// |`70`|`01110000`|`LOC  PUSH    addr=4*4= 10`|
        /// |`71`|`01110001`|`LOC  POP     addr=4*4= 10`|
        /// |`72`|`01110010`|`LOC  USING   addr=4*4= 10`|
        /// |`73`|`01110011`|`LOC  PUSH #  addr=4*4= 10`|
        /// |`74`|`01110100`|`LOC  PUSH    addr=5*4= 14`|
        /// |`75`|`01110101`|`LOC  POP     addr=5*4= 14`|
        /// |`76`|`01110110`|`LOC  USING   addr=5*4= 14`|
        /// |`77`|`01110111`|`LOC  PUSH #  addr=5*4= 14`|
        /// |`78`|`01111000`|`LOC  PUSH    addr=6*4= 18`|
        /// |`79`|`01111001`|`LOC  POP     addr=6*4= 18`|
        /// |`7A`|`01111010`|`LOC  USING   addr=6*4= 18`|
        /// |`7B`|`01111011`|`LOC  PUSH #  addr=6*4= 18`|
        /// |`7C`|`01111100`|`LOC  PUSH    addr=7*4= 1C`|
        /// |`7D`|`01111101`|`LOC  POP     addr=7*4= 1C`|
        /// |`7E`|`01111110`|`LOC  USING   addr=7*4= 1C`|
        /// |`7F`|`01111111`|`LOC  PUSH #  addr=7*4= 1C`|
        /// Source: [Cluso99's SPIN bytecode document revision RR20080721,
        /// on %Propeller 1 Forum](https://forums.parallax.com/discussion/comment/796018/#Comment_796018)
        ///
        private void StepImplicitMemoryOp(byte operation)
        {
            byte type = (byte)(operation & 0x03);  // Bit[0..1] = POP, PUSH, USING, REFERENCE
            uint index = (uint)(operation & 0x1C); // Bit[2..4] = Index (*4)
            byte @base = (byte)(operation & 0x20); // Bit[5]    = VAR, LOCAL
            index += @base == 0 ?
                VariableFrame :
                LocalFrame;
            switch (type)
            {
                case 0: // PUSH
                    PushStack(CpuHost.DirectReadLong(index));
                    break;
                case 1: // POP
                    CpuHost.DirectWriteLong(index, PopStack());
                    break;
                case 2: // USING
                    CpuHost.DirectWriteLong(index, InplaceUsingOp(CpuHost.DirectReadLong(index)));
                    break;
                case 3: // REFERENCE
                    PushStack(index);
                    break;
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        /// @version v22.05.04 - Changed local variable name to clarify
        /// meaning of it.
        private uint ReadPackedUnsignedWord()
        {
            uint operation = CpuHost.DirectReadByte(ProgramCursor++);
            if ((operation & 0x80) != 0)
                return ((operation << 8) | CpuHost.DirectReadByte(ProgramCursor++)) & 0x7FFF;
            return operation;
        }

        /// <summary></summary>
        /// <returns></returns>
        /// @version v22.05.04 - Changed local variable name to clarify
        /// meaning of it.
        private uint ReadPackedSignedWord()
        {
            uint operation = CpuHost.DirectReadByte(ProgramCursor++);
            if ((operation & 0x80) == 0)
            {
                if ((operation & 0x40) != 0)
                    operation |= 0xFFFFFF80;
                return operation;
            }
            operation = ((operation << 8) | CpuHost.DirectReadByte(ProgramCursor++)) & 0x7FFF;
            if ((operation & 0x4000) != 0)
                operation |= 0xFFFF8000;
            return operation;
        }

        /// @brief Execute bytecode operation for a additional bytecode that
        /// follows a USING bytecode.
        /// @param originalValue Initial value of MEM, OBJ, VAR or LOC.
        /// @return Stored value from operation.
        /// @issue{24} reversed assignment ops.
        /// @version v22.05.04 - Changed local variable name to clarify
        /// meaning of it.
        /// @todo [Legacy] RAISE AN EXCEPTION on (op > 0x7F)
        /// @todo [Legacy] SEND MESSAGE ON UNDEFINED OP on switch(op) Assignments operators
        ///
        /// Assignment Operators OpCodes
        /// ============================
        /// This is an additional bytecode and follows a USING bytecode.
        /// |Byte      |Description                   |||
        /// |:--------:|:-----:|:-----------|:----------|
        /// |With (p=push)                           ||||
        /// |`p000000-`|       |`write`                ||
        /// |`-000001-`|       |`repeat-var loop`         |+1..2 address (see [Note 1](#N1SpecialOpcodes))|
        /// |`-000011-`|       |`repeat-var loop pop step`|      ^                                        |
        /// |`p00010--`|`?var` |`random forward (long)`||
        /// |`p00011--`|`var?` |`random reverse (long)`||
        /// |`p00100--`|`~var` |`sign-extend byte`     ||
        /// |`p00101--`|`~~var`|`sign-extend word`     ||
        /// |`p00110--`|`var~` |`post-clear`           ||
        /// |`p00111--`|`var~~`|`post-set`             ||
        /// |`p010000-`|`++var`|`pre-inc bits`         ||
        /// |`p010001-`|`++var`|`pre-inc byte`         ||
        /// |`p010010-`|`++var`|`pre-inc word`         ||
        /// |`p010011-`|`++var`|`pre-inc long`         ||
        /// |`p010100-`|`var++`|`post-inc bits`        ||
        /// |`p010101-`|`var++`|`post-inc byte`        ||
        /// |`p010110-`|`var++`|`post-inc word`        ||
        /// |`p010111-`|`var++`|`post-inc long`        ||
        /// |`p011000-`|`--var`|`pre-dec bits`         ||
        /// |`p011001-`|`--var`|`pre-dec byte`         ||
        /// |`p011010-`|`--var`|`pre-dec word`         ||
        /// |`p011011-`|`--var`|`pre-dec long`         ||
        /// |`p011100-`|`var--`|`post-dec bits`        ||
        /// |`p011101-`|`var--`|`post-dec byte`        ||
        /// |`p011110-`|`var--`|`post-dec word`        ||
        /// |`p011111-`|`var--`|`post-dec long`        ||
        /// Source: [Cluso99's SPIN bytecode document revision RR20080721,
        /// on %Propeller 1 Forum](https://forums.parallax.com/discussion/comment/796018/#Comment_796018)
        ///
        private uint InplaceUsingOp(uint originalValue)
        {
            byte operation = CpuHost.DirectReadByte(ProgramCursor++);
            bool push = (operation & 0x80) != 0;
            uint result;
            uint stored;
            operation &= 0x7F;
            // Use Variable OpCode set
            // Issue #24 reversed assignment ops
            if (operation >= 0x40)
            {
                if (operation <= 0x5F)
                    // VAR OpCode set
                    stored = result = BaseMathOp((byte)(operation - 0x40), false, originalValue);
                else if (operation <= 0x7F)
                    // LOC OpCode set
                    stored = result = BaseMathOp((byte)(operation - 0x60), true, originalValue);
                else
                {
                    //TODO Raise exception here
                    System.Windows.Forms.MessageBox.Show($"Unkown OpCode {operation}");
                    stored = result = 0;
                }
            }
            // Assignments operators
            else
            {
                switch (operation)
                {
                    case 0x00:  // COPY
                        stored = result = PopStack();   // Copy the top of the stack
                        break;
                    case 0x02:  // REPEAT_COMPARE
                    {
                        int value = (int)originalValue;
                        int end = (int)PopStack();
                        int start = (int)PopStack();
                        uint branch = ReadPackedSignedWord();
                        if (end < start)
                        {
                            if (--value >= end)
                                ProgramCursor += (uint)branch;
                        }
                        else
                        {
                            if (++value <= end)
                                ProgramCursor += (uint)branch;
                        }
                        stored = result = (uint)value;
                    }
                        break;
                    case 0x06:  // REPEAT_COMPARE_STEP
                    {
                        int value = (int)originalValue;
                        int end = (int)PopStack();
                        int start = (int)PopStack();
                        int step = (int)PopStack();
                        uint branch = ReadPackedSignedWord();
                        if (end < start)
                            (start, end) = (end, start);  // swap values
                        value += step;
                        if (value >= start && value <= end)
                            ProgramCursor += (uint)branch;
                        stored = result = (uint)value;
                    }
                        break;
                    case 0x08:  // Forward random
                    {
                        result = originalValue == 0x0 ?
                            0x1 :
                            originalValue;
                        for (int i = 0; i < 32; i++)
                        {
                            uint parity = result ^ (result >> 1) ^ (result >> 2) ^ (result >> 4);
                            result >>= 1;
                            if ((parity & 1) != 0)
                                result |= 0x80000000;
                        }
                        stored = result;
                    }
                        break;
                    case 0x0C:  // Reverse random
                    {
                        result = originalValue == 0x0 ?
                            0x1 :
                            originalValue;
                        for (int i = 0; i < 32; i++)
                        {
                            uint parity = result ^ (result >> 1) ^ (result >> 3) ^ (result >> 31);
                            result <<= 1;
                            if ((parity & 1) != 0)
                                result |= 0x00000001;
                        }
                        stored = result;
                    }
                        break;
                    case 0x10:  // PRE-EXTEND 8
                        result = originalValue;
                        if ((result & 0x80) != 0)
                            result |= 0xFFFFFF00;
                        else
                            result &= 0xFFFFFF00;
                        stored = result;
                        break;
                    case 0x14:  // PRE-EXTEND 16
                        result = originalValue;
                        if ((result & PropellerCPU.TOTAL_RAM) != 0)
                            result |= 0xFFFF0000;
                        else
                            result &= 0xFFFF0000;
                        stored = result;
                        break;
                    case 0x18:  // POST-RESET
                        result = originalValue;
                        stored = 0;
                        break;
                    case 0x1C:  // POST-SET
                        result = originalValue;
                        stored = 0xFFFFFFFF;
                        break;
                    case 0x20:  // PRE-INCREMENT BITS
                        stored = result = originalValue + 1;
                        break;
                    case 0x22:  // PRE-INCREMENT BYTE
                        stored = result = (originalValue + 1) & 0xFF;
                        break;
                    case 0x24:  // PRE-INCREMENT WORD
                        stored = result = (originalValue + 1) & 0xFFFF;
                        break;
                    case 0x26:  // PRE-INCREMENT LONG
                        stored = result = originalValue + 1;
                        break;
                    case 0x28:  // POST-INCREMENT BITS
                        result = originalValue;
                        stored = result + 1;
                        break;
                    case 0x2A:  // POST-INCREMENT BYTE
                        result = originalValue;
                        stored = (result + 1) & 0xFF;
                        break;
                    case 0x2C:  // POST-INCREMENT WORD
                        result = originalValue;
                        stored = (result + 1) & 0xFFFF;
                        break;
                    case 0x2E:  // POST-INCREMENT LONG
                        result = originalValue;
                        stored = result + 1;
                        break;
                    case 0x30:  // PRE-DECREMENT BITS
                        stored = result = originalValue - 1;
                        break;
                    case 0x32:  // PRE-DECREMENT BYTE
                        stored = result = (originalValue - 1) & 0xFF;
                        break;
                    case 0x34:  // PRE-DECREMENT WORD
                        stored = result = (originalValue - 1) & 0xFFFF;
                        break;
                    case 0x36:  // PRE-DECREMENT LONG
                        stored = result = originalValue - 1;
                        break;
                    case 0x38:  // POST-DECREMENT BITS
                        result = originalValue;
                        stored = result - 1;
                        break;
                    case 0x3A:  // POST-DECREMENT BYTE
                        result = originalValue;
                        stored = (result - 1) & 0xFF;
                        break;
                    case 0x3C:  // POST-DECREMENT WORD
                        result = originalValue;
                        stored = (result - 1) & 0xFFFF;
                        break;
                    case 0x3E:  // POST-DECREMENT LONG
                        result = originalValue;
                        stored = result - 1;
                        break;
                    default:
                        // TODO: SEND MESSAGE ON UNDEFINED OP
                        stored = result = 0;
                        break;
                }
            }

            if (push)
                PushStack(result);
            return stored;
        }
    }
}
