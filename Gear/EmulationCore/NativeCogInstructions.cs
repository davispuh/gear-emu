/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. PropellerCPU Debugger
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * NativeCogInstructions.cs
 * Instruction methods of native assembly (machine code) cog
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

namespace Gear.EmulationCore
{
    partial class NativeCog
    {
        /// <summary>
        ///
        /// </summary>
        private void InstructionRCR()
        {
            int bits = (int)SourceValue & 31;
            DataResult = DestinationValue >> bits;
            if (Carry)
                DataResult |= ~(0xFFFFFFFF >> bits);

            CarryResult = (DestinationValue & 0x00000001) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionRCL()
        {
            int bits = (int)SourceValue & 31;
            DataResult = DestinationValue << bits;
            if (Carry)
                DataResult |= ~(0xFFFFFFFF << bits);

            CarryResult = (DestinationValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionROR()
        {
            int bits = (int)SourceValue & 31;
            ulong mask = DestinationValue | ((ulong)DestinationValue << 32);

            DataResult = (uint)(mask >> bits);
            CarryResult = (DestinationValue & 0x00000001) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionROL()
        {
            int bits = (int)SourceValue & 31;
            ulong mask = DestinationValue | ((ulong)DestinationValue << 32);

            DataResult = (uint)(mask << bits >> 32);
            CarryResult = (DestinationValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSHR()
        {
            int bits = (int)SourceValue & 31;

            DataResult = DestinationValue >> bits;
            CarryResult = (DestinationValue & 0x00000001) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSHL()
        {
            int bits = (int)SourceValue & 31;

            DataResult = DestinationValue << bits;
            CarryResult = (DestinationValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSAR()
        {
            int bits = (int)SourceValue & 31;

            DataResult = DestinationValue >> bits;
            if ((DestinationValue & 0x80000000) != 0)
                DataResult |= ~(0xFFFFFFFF >> bits);

            CarryResult = (DestinationValue & 0x00000001) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionOR()
        {
            DataResult = SourceValue | DestinationValue;
            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionXOR()
        {
            DataResult = SourceValue ^ DestinationValue;
            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionAND()
        {
            DataResult = SourceValue & DestinationValue;
            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionANDN()
        {
            DataResult = DestinationValue & ~SourceValue;
            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionMUXC()
        {
            if (Carry)
                DataResult = DestinationValue | SourceValue;
            else
                DataResult = DestinationValue & ~SourceValue;

            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionMUXNC()
        {
            if (!Carry)
                DataResult = DestinationValue | SourceValue;
            else
                DataResult = DestinationValue & ~SourceValue;

            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionMUXZ()
        {
            if (Zero)
                DataResult = DestinationValue | SourceValue;
            else
                DataResult = DestinationValue & ~SourceValue;

            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionMUXNZ()
        {
            if (!Zero)
                DataResult = DestinationValue | SourceValue;
            else
                DataResult = DestinationValue & ~SourceValue;

            ZeroResult = DataResult == 0;

            uint parity = DataResult;
            parity ^= parity >> 16;
            parity ^= parity >> 8;
            parity ^= parity >> 4;
            parity ^= parity >> 2;
            parity ^= parity >> 1;
            CarryResult = (parity & 1) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionREV()
        {
            int shift = 0;

            DataResult = 0;
            // if (SourceValue < 32)
            // {
            for (int i = 31 - ((int)SourceValue & 31); i >= 0; i--)
                DataResult |= ((DestinationValue >> i) & 1) << (shift++);
            ZeroResult = DataResult == 0;
            // }
            // else
            // {
            //    ZeroResult = true;
            // }
            CarryResult = (DestinationValue & 1) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionABS()
        {
            if ((SourceValue & 0x80000000) != 0)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionABSNEG()
        {
            if ((SourceValue & 0x80000000) == 0)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionNEG()
        {
            DataResult = (uint)-(int)SourceValue;
            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionNEGC()
        {
            if (Carry)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionNEGNC()
        {
            if (!Carry)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionNEGZ()
        {
            if (Zero)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionNEGNZ()
        {
            if (!Zero)
                DataResult = (uint)-(int)SourceValue;
            else
                DataResult = SourceValue;

            CarryResult = (SourceValue & 0x80000000) != 0;
            ZeroResult = DataResult == 0;
        }

        /// @brief Execute instruction MOV: Set a register to a value.
        /// @details Effects: Set D to S
        private void InstructionMOV()
        {
            DataResult = SourceValue;
            ZeroResult = (DataResult == 0);
            CarryResult = ((SourceValue & 0x80000000) != 0);
        }

        /// @brief Execute instruction MOVS: Set a register's source field to a value.
        /// @details Effects: Insert S[8..0] into D[8..0]
        private void InstructionMOVS()
        {
            DataResult = (DestinationValue & 0xFFFFFE00) | (SourceValue & 0x000001FF);
            ZeroResult = (DataResult == 0);
            /// @todo InstructionMOVS - Find out what carry REALLY does in hardware.
            CarryResult = Carry;
        }

        /// @brief Execute instruction MOVD: Set a register's destination field to a value.
        /// @details Effects: Insert S[8..0] into D[17..9]
        private void InstructionMOVD()
        {
            DataResult = (DestinationValue & 0xFFFC01FF) | ((SourceValue & 0x000001FF) << 9);
            ZeroResult = (DataResult == 0);
            /// @todo InstructionMOVD - Find out what carry REALLY does in hardware.
            CarryResult = Carry;
        }

        /// @brief Execute instruction MOVI: Set a register's instruction and effects fields
        /// to a value.
        /// @details Effects: Insert S[8..0] into D[31..23]
        private void InstructionMOVI()
        {
            DataResult = (DestinationValue & 0x007FFFFF) | ((SourceValue & 0x000001FF) << 23);
            ZeroResult = (DataResult == 0);
            /// @todo InstructionMOVI - Find out what carry REALLY does in hardware.
            CarryResult = Carry;
        }

        /// @brief Execute instruction JMPRET: Jump to address with intention to "return"
        /// to another address.
        /// @details Effects: Insert PC+1 into D[8..0] and set PC to S[8..0].
        /// @version V15.03.26 - corrected Carry flag according to Propeller Manual v1.2.
        private void InstructionJMPRET()
        {
            DataResult = (DestinationValue & 0xFFFFFE00) | (PC & 0x000001FF);
            PC = SourceValue & 0x1FF;
            ZeroResult = (DataResult == 0);
            //Note from Propeller Manual v1.2: "The C flag is set (1) unless PC+1 equals 0; very
            // unlikely since it would require the JMPRET to be executed from the top of
            // cog RAM ($1FF; special purpose register VSCL)."
            CarryResult = (PC != 0);
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionMINS()
        {
            if ((int)DestinationValue < (int)SourceValue)
            {
                DataResult = SourceValue;
                CarryResult = true;
            }
            else
            {
                DataResult = DestinationValue;
                CarryResult = false;
            }

            ZeroResult = DestinationValue == SourceValue;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionMAXS()
        {
            if ((int)DestinationValue < (int)SourceValue)
            {
                DataResult = DestinationValue;
                CarryResult = true;
            }
            else
            {
                DataResult = SourceValue;
                CarryResult = false;
            }

            ZeroResult = DestinationValue == SourceValue;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionMIN()
        {
            if (DestinationValue < SourceValue)
            {
                DataResult = SourceValue;
                CarryResult = true;
            }
            else
            {
                DataResult = DestinationValue;
                CarryResult = false;
            }

            ZeroResult = DestinationValue == SourceValue;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionMAX()
        {
            if (DestinationValue < SourceValue)
            {
                DataResult = DestinationValue;
                CarryResult = true;
            }
            else
            {
                DataResult = SourceValue;
                CarryResult = false;
            }

            ZeroResult = DestinationValue == SourceValue;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionADD()
        {
            ulong result = (ulong)SourceValue + (ulong)DestinationValue;
            DataResult = (uint)result;
            ZeroResult = DataResult == 0;
            CarryResult = (result & 0x100000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionADDABS()
        {
            ulong result = (ulong)DestinationValue;

            if ((SourceValue & 0x80000000) != 0)
                result += (ulong)-(int)SourceValue;
            else
                result += (ulong)SourceValue;

            DataResult = (uint)result;
            ZeroResult = DataResult == 0;
            CarryResult = (result & 0x100000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionADDX()
        {
            ulong result = (ulong)SourceValue + (ulong)DestinationValue + (ulong)(Carry ? 1 : 0);

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = (result & 0x100000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionADDS()
        {
            long result = (int)SourceValue + (int)DestinationValue;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionADDSX()
        {
            long result = (int)SourceValue + (int)DestinationValue + (Carry ? 1 : 0);

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSUB()
        {
            ulong result = (ulong)DestinationValue - (ulong)SourceValue;
            DataResult = (uint)result;
            ZeroResult = DataResult == 0;
            CarryResult = (result & 0x100000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSUBABS()
        {
            long result = DestinationValue;

            if ((SourceValue & 0x80000000) != 0)
                result -= (long)-(int)SourceValue;
            else
                result -= (long)SourceValue;

            DataResult = (uint)result;
            ZeroResult = DataResult == 0;
            CarryResult = (result & 0x100000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSUBX()
        {
            ulong result = (ulong)DestinationValue - (ulong)SourceValue;

            if (Carry)
                result--;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = (result & 0x100000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSUBS()
        {
            long result = (int)DestinationValue - (int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) != 0
                && ((SourceValue ^ DataResult) & 0x80000000) == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSUBSX()
        {
            long result = (int)DestinationValue - (int)SourceValue;

            if (Carry)
                result--;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) != 0
                && ((SourceValue ^ DataResult) & 0x80000000) == 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSUMC()
        {
            long result = (int)DestinationValue;

            if (Carry)
                result -= (int)SourceValue;
            else
                result += (int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSUMNC()
        {
            long result = (int)DestinationValue;

            if (!Carry)
                result -= (int)SourceValue;
            else
                result += (int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSUMZ()
        {
            long result = (int)DestinationValue;

            if (Zero)
                result -= (int)SourceValue;
            else
                result += (int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionSUMNZ()
        {
            long result = (int)DestinationValue;

            if (!Zero)
                result -= (int)SourceValue;
            else
                result += (int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = ((SourceValue ^ DestinationValue) & 0x80000000) == 0
                && ((SourceValue ^ DataResult) & 0x80000000) != 0;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionCMPS()
        {
            long result = (long)(int)DestinationValue - (long)(int)SourceValue;

            DataResult = (uint)result;
            ZeroResult = (DataResult == 0);
            CarryResult = (DataResult & 0x80000000) == 0x80000000;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionCMPSX()
        {
            long result = (long)(int)DestinationValue - (long)(int)SourceValue;

            if (Carry)
                result--;

            DataResult = (uint)result;
            ZeroResult = Zero && (DataResult == 0);
            CarryResult = (DataResult & 0x80000000) == 0x80000000;
            if (((SourceValue ^ DestinationValue) & 0x80000000) != 0)
                CarryResult = !CarryResult;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionCMPSUB()
        {
            if (DestinationValue >= SourceValue)
            {
                DataResult = DestinationValue - SourceValue;
                ZeroResult = (DataResult == 0);
            }
            else
            {
                DataResult = DestinationValue;
                ZeroResult = false;
            }
            CarryResult = DestinationValue >= SourceValue;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private bool InstructionDJNZ()
        {
            DataResult = DestinationValue - 1;
            CarryResult = DestinationValue == 0;
            ZeroResult = DataResult == 0;

            if (!ZeroResult)
                PC = SourceValue & 0x1FF;

            return !ZeroResult;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private bool InstructionTJNZ()
        {
            DataResult = DestinationValue;
            CarryResult = false;
            ZeroResult = DataResult == 0;

            if (!ZeroResult)
                PC = SourceValue & 0x1FF;

            return !ZeroResult;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private bool InstructionTJZ()
        {
            DataResult = DestinationValue;
            CarryResult = false;
            ZeroResult = DataResult == 0;

            if (ZeroResult)
                PC = SourceValue & 0x1FF;

            return ZeroResult;
        }

        /// <summary>
        ///
        /// </summary>
        private void InstructionWAITVID()
        {
            ulong result = (ulong)SourceValue + (ulong)DestinationValue;
            DataResult = (uint)result;
            ZeroResult = DataResult == 0;
            CarryResult = (result & 0x100000000) != 0;
        }

    }
}
