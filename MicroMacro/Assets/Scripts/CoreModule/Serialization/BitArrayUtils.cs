using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace CoreModule.Serialization
{
    [BurstCompile]
    public static class BitArrayUtils
    {
        /// <summary>
        /// バイト配列内の真のビット数をカウントします。
        /// </summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CountTrueBitsSimd(byte* bytes, int length, out int count)
        {
            count = 0;

            int i = 0;

            // 16バイト = 128ビット単位で処理（v128）
            int simdLength = length - length & 15;

            for (; i < simdLength; i += 16)
            {
                v128 vec = UnsafeUtility.ReadArrayElement<v128>(bytes, i >> 4);

                // 1バイトずつPopCountを加算
                for (int j = 0; j < 16; j++)
                {
                    byte b = UnsafeUtility.ReadArrayElement<byte>(&vec, j);
                    count += math.countbits((uint)b);
                }
            }

            // 残りのバイトを個別にカウント
            for (; i < length; i++)
            {
                count += math.countbits((uint)bytes[i]);
            }
        }
    }
}

