/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * InterpretedCog.cs
 * Object class for an interpreted (SPIN) cog.
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
    class InterpretedCog : Cog
    {
        // Constants
        const int INTERPRETER_BOOT_TIME = 48;   // 48 cycles to boot (fake)

        // Interpreter Specific Values
        private uint StackFrame;
        private uint ObjectFrame;
        private uint VariableFrame;
        private uint LocalFrame;
        private bool InterpreterFlag; // Flag for determining if a COGINIT statement is to load an interpreter

        private uint TargetValue;
        private uint MaskValue;
        private uint PixelsValue;
        private uint ColorsValue;

        private Stack<uint> CallStack;  // Internal stack, used for storing return call stuff

        private bool Port;

        public uint Stack
        {
            get { return StackFrame; }
        }

        public uint Local
        {
            get { return LocalFrame; }
        }

        public uint Object
        {
            get { return ObjectFrame; }
        }

        public uint Variable
        {
            get { return VariableFrame; }
        }

        public InterpretedCog(PropellerCPU host,
            uint paramAddress, uint frequency,
            PLLGroup pll)
            : base(host, 0xF004, paramAddress, frequency, pll)
        {
            CallStack = new Stack<uint>();
        }

        public void PushStack(uint value)
        {
            Hub.WriteLong(StackFrame, value);
            StackFrame += 4;
        }

        public uint PopStack()
        {
            StackFrame -= 4;
            return Hub.ReadLong(StackFrame);
        }

        private void PushStackWord(ushort value)
        {
            Hub.WriteWord(StackFrame, value);
            StackFrame += 2;
        }

        private ushort PopStackWord()
        {
            StackFrame -= 2;
            return Hub.ReadWord(StackFrame);
        }

        public override void Boot()
        {
            State = CogRunState.BOOT_INTERPRETER;
            StateCount = INTERPRETER_BOOT_TIME;

            uint InitFrame = this[(int)CogSpecialAddress.PAR];

            this[(int)CogSpecialAddress.COGID] = Hub.CogID(this);

            InitFrame &= 0xFFFF;

            ObjectFrame = Hub.ReadWord(InitFrame - 8);
            VariableFrame = Hub.ReadWord(InitFrame - 6);
            PC = Hub.ReadWord(InitFrame - 4);
            StackFrame = Hub.ReadWord(InitFrame - 2) - (uint)4;
            LocalFrame = InitFrame - (uint)4;

            // Clear CogID
            this[(int)CogSpecialAddress.INITCOGID] = InitFrame - 4;
            Hub.WriteLong(InitFrame - 8, 0xFFFFFFFF);
            Hub.WriteLong(InitFrame - 4, 0);
        }

        private uint CogInit()
        {
            uint CogID;
            uint code;

            if (InterpreterFlag)
            {
                InterpreterFlag = false;  // Clear interpreter flag

                uint StackPointer = PopStack() + 8;
                uint FunctionCode = PopStack();
                uint FunctionArgs = FunctionCode >> 8;
                FunctionCode &= 0xFF;

                // Long align our stack (wasteful)
                while ((StackPointer & 0x3) != 0)
                    StackPointer++;

                // SETUP INTERPRETER HERE

                uint FunctionOffset = Hub.ReadWord(ObjectFrame + FunctionCode * 4) + ObjectFrame;
                uint ArguementStack = FunctionArgs * 4 + StackPointer - 4;
                uint FunctStack = (uint)Hub.ReadWord(ObjectFrame + FunctionCode * 4 + 2) + ArguementStack + 4;

                Hub.WriteWord(StackPointer - 8, ObjectFrame);               // Object Memory (Same object)
                Hub.WriteWord(StackPointer - 6, VariableFrame);             // Variable Memory (Same variables)
                Hub.WriteWord(StackPointer - 4, FunctionOffset);            // PC
                Hub.WriteWord(StackPointer - 2, FunctStack + (uint)4);      // Stack Pointer

                // Preinitialize boot function
                for (uint i = 0; i < FunctionArgs * 4; i += 4, ArguementStack -= 4)
                    Hub.WriteLong(ArguementStack, PopStack());

                // Setup cog boot op-codes
                code = ((0xF004 & 0xFFFC) << 2) | (StackPointer << 16);

                // Find which cog we will be booting
                CogID = Hub.ReadLong(this[(int)CogSpecialAddress.INITCOGID] - 4);
                Hub.WriteLong(this[(int)CogSpecialAddress.INITCOGID] - 4, 0xFFFFFFFF);   // Clear CogID
            }
            else
            {
                // We will be using the value for the boot parameter
                // located inside of stack itself.  This is only valid because
                // SPIN instructions take longer than 14 cycles to execute.
                // Otherwise there is the possibility that the value could be clobbered.

                uint BootParam = PopStack();
                uint EntryPoint = PopStack();
                CogID = PopStack();

                code = ((EntryPoint & 0xFFFC) << 2) |
                       ((BootParam & 0xFFFC) << 16);
            }

            // New cog if ID is not valid
            code |= ((CogID < 8) ? CogID : 0x8);

            bool temp = false, temp2 = false;  // Provided for hub op carry
            return Hub.HubOp(this, (uint)HubOperationCodes.HUBOP_COGINIT, code, ref temp, ref temp2);
        }

        /// @todo document Gear.EmulationCore.DoInstruction()
        /// 
        override public bool DoInstruction()
        {
            switch (State)
            {
                case CogRunState.BOOT_INTERPRETER:
                case CogRunState.WAIT_INTERPRETER:
                    if (--StateCount == 0)
                        State = CogRunState.EXEC_INTERPRETER;
                    return true;
                case CogRunState.EXEC_INTERPRETER:
                    State = CogRunState.WAIT_INTERPRETER;
                    StateCount = 32;  // 32 cycles per instruction (faked)
                    break;

                case CogRunState.WAIT_PNE:
                    {

                        uint maskedIn = (Port ? Hub.INB : Hub.INA) & MaskValue;
                        if (maskedIn != TargetValue)
                            State = CogRunState.EXEC_INTERPRETER;

                        return true;
                    }
                case CogRunState.WAIT_PEQ:
                    {
                        uint maskedIn = (Port ? Hub.INB : Hub.INA) & MaskValue;
                        if (maskedIn == TargetValue)
                            State = CogRunState.EXEC_INTERPRETER;

                        return true;
                    }
                case CogRunState.WAIT_CNT:
                    {
                        long target = Hub.Counter;

                        if (TargetValue == target)
                            State = CogRunState.EXEC_INTERPRETER;

                        return true;
                    }
                case CogRunState.WAIT_VID:
                    if (Video.Ready)
                    {
                        State = CogRunState.EXEC_INTERPRETER;
                        Video.Feed(ColorsValue, PixelsValue);
                    }
                    return true;

                // Non-execute states are ignored
                default:
                    return true;
            }

            byte op = Hub.ReadByte(PC++);

            // Masked Memory Operations
            if (op >= 0xE0)
            {
                PushStack(BaseMathOp((byte)(op - 0xE0), false, PopStack()));
            }
            // Masked Memory Operations
            else if (op >= 0x80)
            {
                StepMaskedMemoryOp(op);
                return true;
            }
            // Inplicit Location Memory Ops
            else if (op >= 0x40)
            {
                StepImplicitMemoryOp(op);
            }
            else
            {
                switch (op)
                {
                    case 0x00:  // FRAME CALL WITH RETURN
                    case 0x01:  // FRAME CALL WITH NO RETURN
                    case 0x02:  // FRAME CALL WITH TRAP AND RETURN
                    case 0x03:  // FRAME CALL WITH TRAP AND NO RETURN
                        {
                            //Use short stack for return masks
                            CallStack.Push((uint)(op & 0x3));

                            PushStackWord((ushort)ObjectFrame);
                            PushStackWord((ushort)VariableFrame);
                            PushStackWord((ushort)LocalFrame);
                            CallStack.Push(StackFrame);     // Enqueue pointer to return PC
                            PushStackWord(0);               // PC is not valid yet

                            // Defaults to a zero value
                            PushStack(0);
                        }
                        break;
                    case 0x04:  // UNCONDITIONAL RELATIVE BRANCH
                        {
                            uint offset = (uint)ReadPackedSignedWord();
                            PC += offset;
                        }
                        break;
                    case 0x05:  // Call function
                    case 0x06:  // Call object function
                    case 0x07:  // Call indexed object function
                        {
                            // Pop our index from the stack
                            if (op == 0x06 || op == 0x07)
                            {
                                uint objectCode = (uint)(Hub.ReadByte(PC++) * 4) + ObjectFrame;

                                if (op == 0x07)
                                    objectCode += PopStack() * 4;

                                ObjectFrame += Hub.ReadWord(objectCode);
                                VariableFrame += Hub.ReadWord(objectCode + 2);
                            }

                            uint functionCode = (uint)(Hub.ReadByte(PC++) * 4) + ObjectFrame;

                            // De-CallStack the local frame
                            uint ReturnPointer = CallStack.Pop();
                            LocalFrame = ReturnPointer + 2;
                            Hub.WriteWord(ReturnPointer, PC); // Preserve PC

                            // Branch, and change local and stack pointers to match what they need to be
                            PC = ObjectFrame + Hub.ReadWord(functionCode);
                            StackFrame += Hub.ReadWord(functionCode + 2);
                        }
                        break;
                    case 0x08:  // LOOP START
                        {
                            int val = (int)PopStack();
                            uint branch = ReadPackedSignedWord();

                            if (val <= 0)
                                PC = (uint)((int)PC + branch);
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
                                PC = (uint)((int)PC + branch);
                                PushStack((uint)val);
                            }
                        }
                        break;
                    case 0x0A:  // JUMP ON ZERO
                        {
                            uint val = PopStack();
                            uint branch = ReadPackedSignedWord();

                            if (val == 0)
                                PC = (uint)((int)PC + branch);
                        }
                        break;
                    case 0x0B:  // JUMP ON NOT ZERO
                        {
                            uint val = PopStack();
                            uint branch = ReadPackedSignedWord();

                            if (val != 0)
                                PC = (uint)((int)PC + branch);
                        }
                        break;
                    case 0x0C:  // JUMP FROM STACK
                        PopStack(); // Clears out the comparison case
                        PC = PopStack() + ObjectFrame;
                        break;
                    case 0x0D:  // COMPARE CASE
                        {
                            uint equal = PopStack();
                            uint value = PopStack();
                            uint branch = ReadPackedSignedWord();

                            if (equal == value)
                                PC = (uint)((int)PC + branch);

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
                            {
                                uint t = min;
                                min = max;
                                max = t;
                            }

                            if (min <= value && value <= max)
                                PC = (uint)((int)PC + branch);

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
                            int Value = (int)PopStack();
                            int Key = (int)PopStack();
                            uint Jump = PopStack();
                            int Base = (int)PopStack();

                            if (Key == Base)
                            {
                                PC = Jump + ObjectFrame;
                                PushStack((uint)Value);
                            }
                            else
                            {
                                PushStack((uint)Base);
                                PushStack(Jump);
                                PushStack((uint)--Key);
                            }
                        }
                        break;
                    case 0x11:  // Look-down compare (get index of value)
                        {
                            int Value = (int)PopStack();
                            int Key = (int)PopStack();
                            uint Jump = PopStack();
                            int Base = (int)PopStack();

                            if (Key == Value)
                            {
                                PC = Jump + ObjectFrame;
                                PushStack((uint)Base);
                            }
                            else
                            {
                                PushStack((uint)(++Base));
                                PushStack(Jump);
                                PushStack((uint)Key);
                            }
                        }
                        break;
                    case 0x12:  // Look-up range compare
                        {
                            int Top = (int)PopStack();
                            int Bottom = (int)PopStack();
                            int Key = (int)PopStack();
                            uint Jump = PopStack();
                            int Base = (int)PopStack();
                            int Range;

                            if (Bottom < Top)   // 0..12
                            {
                                Range = Top - Bottom + 1;

                                // Are we inrange?
                                if (Key < Range)
                                {
                                    PC = Jump + ObjectFrame;
                                    PushStack((uint)(Bottom + Key - Base));
                                    break;
                                }
                            }
                            else                // 0..12
                            {
                                Range = Bottom - Top + 1;

                                // Are we in range?
                                if (Key < Range)
                                {
                                    PC = Jump + ObjectFrame;
                                    PushStack((uint)(Bottom - Key - Base));
                                    break;
                                }
                            }

                            PushStack((uint)Base);
                            PushStack(Jump);
                            PushStack((uint)(Key - Range));
                        }
                        break;
                    case 0x13:  // Look-down range compare
                        {
                            int Top = (int)PopStack();
                            int Bottom = (int)PopStack();
                            int Key = (int)PopStack();
                            uint Jump = PopStack();
                            int Base = (int)PopStack();
                            int Range;

                            if (Bottom < Top) // 0..12
                            {
                                Range = Top - Bottom + 1;

                                // Value Found?
                                if (Key >= Bottom && Key <= Top)
                                {
                                    PC = Jump + ObjectFrame;
                                    PushStack((uint)(Key - Bottom + Base));
                                    break;
                                }
                            }
                            else // 12..0
                            {
                                Range = Bottom - Top + 1;

                                if (Key <= Bottom && Key >= Top)
                                {
                                    PC = Jump + ObjectFrame;
                                    PushStack((uint)(Key - Top + Base));
                                    break;
                                }
                            }

                            PushStack((uint)(Base + Range));
                            PushStack(Jump);
                            PushStack((uint)Key);
                        }
                        break;
                    case 0x14:  // Quit
                        // TODO: RAISE AN EXCEPTION HERE!
                        System.Windows.Forms.MessageBox.Show("Executed undefined opcode $14");
                        break;
                    case 0x15:  // Mark Interpreted
                        InterpreterFlag = true;
                        break;
                    case 0x16:  // string size
                        {
                            uint i = 0;

                            for (uint b = PopStack(); Hub.ReadByte(b) != 0 && b < 0x10000; b++)
                                i++;

                            PushStack(i);
                        }
                        break;
                    case 0x17:  // string compare
                        {
                            uint a = PopStack();
                            uint b = PopStack();

                            while (true)
                            {
                                byte _a = Hub.ReadByte(a++);
                                byte _b = Hub.ReadByte(b++);

                                if (_a != _b)
                                {
                                    PushStack(0);
                                    break;
                                }
                                else if (_a == 0)
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
                                Hub.WriteByte(dest++, src);
                        }
                        break;
                    case 0x19:  // Word fill
                        {
                            int count = (int)PopStack();
                            uint src = PopStack();
                            uint dest = PopStack();

                            while (count-- > 0)
                            {
                                Hub.WriteWord(dest, src);
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
                                Hub.WriteLong(dest, src);
                                dest += 4;
                            }
                        }
                        break;
                    case 0x1B:  // Wait PEQ
                        Port = PopStack() != 0;
                        MaskValue = PopStack();
                        TargetValue = PopStack();
                        State = CogRunState.WAIT_PEQ;
                        break;
                    case 0x1C:  // Byte move
                        {
                            int count = (int)PopStack();
                            uint src = PopStack();
                            uint dest = PopStack();

                            while (count-- > 0)
                                Hub.WriteByte(dest++, Hub.ReadByte(src++));
                        }
                        break;
                    case 0x1D:  // Word move
                        {
                            int count = (int)PopStack();
                            uint src = PopStack();
                            uint dest = PopStack();

                            while (count-- > 0)
                            {
                                Hub.WriteWord(dest, Hub.ReadWord(src));
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
                                Hub.WriteLong(dest, Hub.ReadLong(src));
                                dest += 4;
                                src += 4;
                            }
                        }
                        break;
                    case 0x1F:  // Wait PNE
                        Port = PopStack() != 0;
                        MaskValue = PopStack();
                        TargetValue = PopStack();
                        State = CogRunState.WAIT_PNE;
                        break;
                    case 0x20:  // Clock Set
                        {
                            Hub.WriteLong(0, PopStack());

                            byte mode = (byte)PopStack();
                            Hub.WriteByte(4, mode);
                            Hub.SetClockMode(mode);
                        }
                        break;

                    case 0x21:  // Cog Stop
                        {
                            int cog = (int)PopStack();

                            Hub.Stop(cog);
                        }
                        break;
                    case 0x22:  // Lock Return
                        Hub.LockReturn(PopStack());
                        break;
                    case 0x23:  // Wait Cnt
                        TargetValue = PopStack();
                        State = CogRunState.WAIT_CNT;
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
                    case 0x26:  // Effect Spr
                        {
                            uint address = PopStack();

                            if (address >= 16)
                                break;

                            address += 0x1F0;

                            WriteLong(address, InplaceEffect(ReadLong(address)));
                        }
                        break;
                    case 0x27:  // Wait Vid
                        PixelsValue = PopStack(); // pixels
                        ColorsValue = PopStack(); // Colors
                        State = CogRunState.WAIT_VID;
                        break;
                    case 0x28:  // Cog Init w/Return
                        PushStack(CogInit());
                        break;
                    case 0x29:  // Lock New w/Return
                        PushStack(Hub.NewLock());
                        break;
                    case 0x2A:  // Lock Set w/Return
                        PushStack(Hub.LockSet(PopStack(), true));
                        break;
                    case 0x2B:  // Lock Clear w/Return
                        PushStack(Hub.LockSet(PopStack(), false));
                        break;
                    case 0x2C:  // Cog Init
                        CogInit();
                        break;
                    case 0x2D:  // Lock New
                        Hub.NewLock();
                        break;
                    case 0x2E:  // Lock Set
                        Hub.LockSet(PopStack(), true);
                        break;
                    case 0x2F:  // Lock Clear
                        Hub.LockSet(PopStack(), false);
                        break;
                    case 0x30:  // Abort w/o return
                        ReturnFromSub(Hub.ReadLong(LocalFrame), true);
                        break;
                    case 0x31:  // Abort w/return
                        ReturnFromSub(PopStack(), true);
                        break;
                    case 0x32:  // Return
                        ReturnFromSub(Hub.ReadLong(LocalFrame), false);
                        break;
                    case 0x33:  // Pop return value (same as 0x61)
                        // Hub.WriteLong(LocalFrame, PopStack());
                        ReturnFromSub(PopStack(), false);
                        break;
                    case 0x34:  // Push -1
                    case 0x35:  // Push 0
                    case 0x36:  // Push 1
                        PushStack((uint)(op - 0x35));
                        break;
                    case 0x37:  // Push Packed Literal
                        {
                            uint result = 0;
                            uint value = Hub.ReadByte(PC++);

                            result = (uint)2 << (int)(value & 0x1F);

                            if ((value & 0x20) != 0)
                                result--;
                            if ((value & 0x40) != 0)
                                result = ~result;

                            // Should complain on invalid op if MSB is set

                            PushStack(result);
                        }
                        break;
                    case 0x38:  // Push Byte
                    case 0x39:  // Push Word
                    case 0x3A:  // Push Three Byte
                    case 0x3B:  // Push Long
                        {
                            uint result = 0;
                            for (int i = 0; i < op - 0x37; i++)
                            {
                                result <<= 8;
                                result |= Hub.ReadByte(PC++);
                            }
                            PushStack(result);
                        }
                        break;
                    case 0x3C:  // UNDEFINED
                        // TODO: ALERT ON BAD OP
                        System.Windows.Forms.MessageBox.Show("Executed undefined opcode $3C");
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
                            {
                                int temp = min;
                                min = max;
                                max = temp;
                            }

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
            return PC != BreakPointCogCursor;
        }

        private void ReturnFromSub(uint value, bool abort)
        {
            bool trapAbort;
            bool wantReturn;

            do
            {
                StackFrame = LocalFrame;

                // Stop on call underflow
                if (CallStack.Count <= 0)
                {
                    Hub.Stop((int)Hub.CogID(this));
                    return;
                }

                uint returnTypeMask = CallStack.Pop();

                trapAbort = (returnTypeMask & 0x02) != 0;
                wantReturn = (returnTypeMask & 0x01) == 0;

                PC = PopStackWord();
                LocalFrame = PopStackWord();
                VariableFrame = PopStackWord();
                ObjectFrame = PopStackWord();
            } while (abort && !trapAbort);

            // Do we return a value?  If so, then push it to the stack
            if (wantReturn)
                PushStack(value);
        }

        private void CogMemoryOp(uint mask, int lowestbit)
        {
            byte op = Hub.ReadByte(PC++);
            uint reg = (uint)((op & 0x1F) + 0x1E0);

            switch (op & 0xE0)
            {
                case 0x80:
                    PushStack((ReadLong(reg) & mask) >> lowestbit);
                    break;
                case 0xA0:
                    {
                        uint val = ReadLong(reg);
                        WriteLong(reg, (val & ~mask) | ((PopStack() << lowestbit) & mask));
                    }
                    break;
                case 0xC0:
                    {
                        uint val = ReadLong(reg);
                        WriteLong(reg, (val & ~mask) | ((InplaceEffect((val & mask) >> lowestbit) << lowestbit) & mask));
                    }
                    break;
                default:
                    // TODO: COMPLAIN ABOUT INVALID OP
                    break;
            }
        }

        private uint BaseMathOp(byte op, bool inplace, uint initial)
        {
            // --- Unary Operators ---
            switch (op)
            {
                case 0x06:  // Negate
                    {
                        return ((uint)(-(int)initial));
                    }
                case 0x07:  // Complement
                    return ~initial;
                case 0x09:  // Absolute Value
                    {
                        int mask = (int)initial;

                        return (mask < 0) ? ((uint)(-mask)) : ((uint)mask);
                    }
                case 0x11:  // Encode (MSB set)
                    {
                        uint mask = initial;
                        for (int i = 31; i >= 0; i--)
                        {
                            if (((mask >> i) & 1) != 0)
                            {
                                return ((uint)(i + 1));
                            }
                        }
                        return (0);
                    }
                case 0x13:  // Decode
                    return ((uint)1 << (int)(initial & 0x1F));
                case 0x18:  // Square Root
                    return (uint)Math.Sqrt(initial);
                case 0x1F:    // Logical Not
                    return (initial == 0) ? (uint)0xFFFFFFFF : (uint)0;
            }

            // --- Binary Operators ---
            uint left;
            uint right;

            if (inplace)
            {
                left = initial;
                right = PopStack();
            }
            else
            {
                left = PopStack();
                right = initial;
            }

            switch (op)
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
                    return (left << (int)right);
                case 0x04:  // Limit Min
                    return ((int)left < (int)right) ? right : left;
                case 0x05:  // Limit Max
                    return ((int)left > (int)right) ? right : left;
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
                case 0x0E:  // Arithmatic shift right
                    {
                        int shift = (int)right;
                        ulong mask = left;
                        if ((mask & 0x80000000) != 0)
                            mask |= 0xFFFFFFFF00000000;

                        mask >>= shift;

                        return ((uint)mask);
                    }
                case 0x0F:  // Bit reverse
                    {
                        uint mask = 0;
                        int bits = (int)right;
                        uint source = left;

                        for (int i = 0; bits > 0; bits--, i++)
                            mask |= ((source >> i) & 1) << (bits - 1);

                        return (mask);
                    }
                case 0x10:  // Logical AND
                    return ((right != 0) && (left != 0)) ? 0xFFFFFFFF : 0;
                case 0x12:  // Logical OR
                    return ((right != 0) || (left != 0)) ? 0xFFFFFFFF : 0;
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
                    return ((int)left < (int)right) ? (uint)0xFFFFFFFF : (uint)0;
                case 0x1A:  // Greater
                    return ((int)left > (int)right) ? (uint)0xFFFFFFFF : (uint)0;
                case 0x1B:  // Not Equal
                    return ((int)left != (int)right) ? (uint)0xFFFFFFFF : (uint)0;
                case 0x1C:  // Equal
                    return ((int)left == (int)right) ? (uint)0xFFFFFFFF : (uint)0;
                case 0x1D:  // Less than or Equal
                    return ((int)left <= (int)right) ? (uint)0xFFFFFFFF : (uint)0;
                case 0x1E:  // Greater than or equal
                    return ((int)left >= (int)right) ? (uint)0xFFFFFFFF : (uint)0;
            }

            // Emergency condition: Should never occur
            return 0;
        }

        private void StepMaskedMemoryOp(byte op)
        {
            byte Type = (byte)(op & 0x03);  // Bit[0..1] = POP, PUSH, EFFECT, REFERENCE
            byte Base = (byte)(op & 0x0C);  // Bit[2..3] = MAIN, OBJ, VAR, LOCAL
            byte Indexed = (byte)(op & 0x10);  // Bit[4]    = Indexed ?
            byte Size = (byte)(op & 0x60);  // Bit[5..6] = BYTE, WORD, LONG

            uint address;

            switch (Base)
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

            if (Indexed != 0)
            {
                if (Base == 0x00)
                    address = PopStack() + (address << (Size >> 5));
                else
                    address += PopStack() << (Size >> 5);
            }

            switch (Type)
            {
                case 0: // PUSH is already done
                    switch (Size)
                    {
                        case 0x00:  // byte
                            PushStack(Hub.ReadByte(address));
                            break;
                        case 0x20:
                            PushStack(Hub.ReadWord(address));
                            break;
                        case 0x40:
                            PushStack(Hub.ReadLong(address));
                            break;
                    }
                    break;
                case 1: // POP
                    switch (Size)
                    {
                        case 0x00:  // byte
                            Hub.WriteByte(address, PopStack());
                            break;
                        case 0x20:
                            Hub.WriteWord(address, PopStack());
                            break;
                        case 0x40:
                            Hub.WriteLong(address, PopStack());
                            break;
                    }
                    break;
                case 2: // EFFECT
                    switch (Size)
                    {
                        case 0x00:  // byte
                            Hub.WriteByte(address, InplaceEffect(Hub.ReadByte(address)));
                            break;
                        case 0x20:  // word
                            Hub.WriteWord(address, InplaceEffect(Hub.ReadWord(address)));
                            break;
                        case 0x40:  // long
                            Hub.WriteLong(address, InplaceEffect(Hub.ReadLong(address)));
                            break;
                    }
                    break;
                case 3: // REFERENCE
                    PushStack(address);
                    break;
            }
        }

        private void StepImplicitMemoryOp(byte op)
        {
            byte Type = (byte)(op & 0x03);  // Bit[0..1] = POP, PUSH, EFFECT, REFERENCE
            byte Base = (byte)(op & 0x20);  // Bit[5]    = VAR, LOCAL
            uint Index = (uint)(op & 0x1C); // Bit[2..4] = Index (*4)

            Index += ((Base == 0) ? VariableFrame : LocalFrame);

            switch (Type)
            {
                case 0: // PUSH
                    PushStack(Hub.ReadLong(Index));
                    break;
                case 1: // POP
                    Hub.WriteLong(Index, PopStack());
                    break;
                case 2: // EFFECT
                    Hub.WriteLong(Index, InplaceEffect(Hub.ReadLong(Index)));
                    break;
                case 3: // REFERENCE
                    PushStack(Index);
                    break;
            }
        }

        private uint ReadPackedUnsignedWord()
        {
            uint op = Hub.ReadByte(PC++);

            if ((op & 0x80) != 0)
                return ((op << 8) | Hub.ReadByte(PC++)) & 0x7FFF;
            return op;
        }

        private uint ReadPackedSignedWord()
        {
            uint op = Hub.ReadByte(PC++);

            if ((op & 0x80) == 0)
            {
                if ((op & 0x40) != 0)
                    op |= 0xFFFFFF80;

                return op;
            }
            else
            {
                op = ((op << 8) | Hub.ReadByte(PC++)) & 0x7FFF;

                if ((op & 0x4000) != 0)
                    op |= 0xFFFF8000;

                return op;
            }
        }

        private uint InplaceEffect(uint originalValue)
        {
            byte op = Hub.ReadByte(PC++);
            bool push = (op & 0x80) != 0;
            uint result;
            uint stored;

            op &= 0x7F;

            // Use Main opcode set
            if (op >= 0x40 && op <= 0x5F)
            {
                stored = result = BaseMathOp((byte)(op - 0x40), true, originalValue);
            }
            else
            {
                switch (op)
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
                                    PC += (uint)branch;
                            }
                            else
                            {
                                if (++value <= end)
                                    PC += (uint)branch;
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
                            {
                                int temp = start;
                                start = end;
                                end = temp;
                            }

                            value += step;

                            if (value >= start && value <= end)
                                PC += (uint)branch;

                            stored = result = (uint)value;
                        }
                        break;

                    case 0x08:  // Forward random
                        {
                            result = (originalValue == 0) ? 1 : originalValue;

                            for (int i = 0; i < 32; i++)
                            {
                                uint parity = (result) ^ (result >> 1) ^ (result >> 2) ^ (result >> 4);
                                result = (result >> 1);

                                if ((parity & 1) != 0)
                                    result |= 0x80000000;
                            }

                            stored = result;
                        }
                        break;
                    case 0x0C:  // Reverse random
                        {
                            result = (originalValue == 0) ? 1 : originalValue;

                            for (int i = 0; i < 32; i++)
                            {
                                uint parity = (result) ^ (result >> 1) ^ (result >> 3) ^ (result >> 31);
                                result = (result << 1);

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

                        if ((result & 0x8000) != 0)
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
