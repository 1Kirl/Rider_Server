using System;
using System.Collections.Generic;

namespace Shared.Bits
{
    public class BitWriter
    {
        private readonly List<byte> buffer = new();
        private int bitPosition = 0;

        public void WriteBits(int value, int bitCount)
        {
            for (int i = bitCount - 1; i >= 0; i--)
            {
                bool bit = ((value >> i) & 1) != 0;

                if (bitPosition % 8 == 0)
                    buffer.Add(0);

                int byteIndex = bitPosition / 8;
                int bitOffset = 7 - (bitPosition % 8); // MSB-first

                if (bit)
                    buffer[byteIndex] |= (byte)(1 << bitOffset);

                bitPosition++;
            }
        }

        public byte[] ToArray()
        {
            return buffer.ToArray();
        }
    }
}
