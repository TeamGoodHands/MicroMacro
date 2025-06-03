using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace CoreModule.Serialization
{
    [Serializable]
    public class SerializableBitArray
    {
        /// <summary>
        /// バイト配列の長さを取得します。
        /// </summary>
        public int ByteLength => bytes.Length;

        /// <summary>
        /// ビット配列の長さを取得します。
        /// </summary>
        public int BitLength => length;

        /// <summary>
        /// 長さを指定して、ビット配列を初期化します。
        /// </summary>
        public SerializableBitArray(int length)
        {
            this.bytes = new byte[(length + 7) >> 3];
            this.length = length;
        }

        /// <summary>
        /// バイト配列のコピーを取得します。
        /// </summary>
        public byte[] GetBytes()
        {
            byte[] array = new byte[this.bytes.Length];
            this.GetBytes(array);
            return array;
        }

        /// <summary>
        /// 指定したバイト配列にビットデータをコピーします。
        /// </summary>
        public void GetBytes(byte[] dest)
        {
            Buffer.BlockCopy(this.bytes, 0, dest, 0, this.bytes.Length);
        }

        // ビットアクセス用インデクサ
        public bool this[int index]
        {
            get
            {
                int byteIndex = index >> 3;
                int bitIndex = index & 7;
                return (bytes[byteIndex] & (1 << bitIndex)) != 0;
            }
            set
            {
                int byteIndex = index >> 3;
                int bitIndex = index & 7;
                int mask = 1 << bitIndex;

                // Unsafe で bool を byte に変換（true:1, false:0）
                byte bit = UnsafeUtility.As<bool, byte>(ref value);

                // ビットを一度クリアして、必要なら立てる
                bytes[byteIndex] = (byte)((bytes[byteIndex] & ~mask) | (bit << bitIndex));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountTrueBits()
        {
            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    BitArrayUtils.CountTrueBitsSimd(ptr, bytes.Length, out int count);
                    return count;
                }
            }
        }

        [SerializeField, HideInInspector]
        protected byte[] bytes;

        [SerializeField, HideInInspector]
        public int length;
    }
}