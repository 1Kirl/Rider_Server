using System;
using System.Collections.Generic;

namespace Shared.Bits
{
    public class BitReader
    {
        private readonly byte[] data;
        private int bitPosition = 0;

        public BitReader(byte[] data)
        {
            this.data = data;
        }

        public int ReadBits(int bitCount)
        {
            int result = 0;
            for (int i = 0; i < bitCount; i++)
            {
                int byteIndex = bitPosition / 8;

                if (byteIndex >= data.Length)
                    throw new InvalidOperationException("Trying to read beyond the end of data");

                int bitOffset = 7 - (bitPosition % 8); // MSB-first

                int bit = (data[byteIndex] >> bitOffset) & 1;
                result = (result << 1) | bit;

                bitPosition++;
            }
            return result;
        }
    }
}