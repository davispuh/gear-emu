using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gear.EmulationCore;

namespace Gear.Disassembler
{
    class DataUnpacker
    {
        public static int GetSignedOffset(MemoryManager memory)
        {
            uint result = memory.ReadByte();

            if ((result & 0x80) == 0)
            {
                if ((result & 0x40) != 0)
                    result |= 0xFFFFFF80;
            }
            else
            {
                result = (result << 8) | memory.ReadByte();

                if ((result & 0x4000) != 0)
                    result |= 0xFFFF8000;
            }
            return (int)result;
        }

        public static short GetSignedPackedOffset(MemoryManager memory)
        {
            short op = memory.ReadByte();
            bool extended = (op & 0x80) != 0;

            op = (short)((op & 0x7F) | ((op << 1) & 0x80));

            if (!extended)
                return (short)(sbyte)op;

            return (short)((op << 8) | memory.ReadByte());
        }

        public static uint GetPackedLiteral(MemoryManager memory)
        {
            byte op = memory.ReadByte();

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

        public static int GetPackedOffset(MemoryManager memory)
        {
            ushort op = memory.ReadByte();

            if ((op & 0x80) == 0)
                return (op & 0x7F);

            op &= 0x7F;

            return (op << 8) | (memory.ReadByte());
        }

        public static int GetLiteral(MemoryManager memory, int count)
        {
            int result = 0;
            for (int i = 0; i < count; i++)
            {
                result <<= 8;
                result |= memory.ReadByte();
            }
            return result;
        }

        public static int GetWordLiteral(MemoryManager memory)
        {
            return GetLiteral(memory, 2);
        }

        public static int GetNearLongLiteral(MemoryManager memory)
        {
            return GetLiteral(memory, 3);
        }

        public static int GetLongLiteral(MemoryManager memory)
        {
            return GetLiteral(memory, 4);
        }

    }
}
