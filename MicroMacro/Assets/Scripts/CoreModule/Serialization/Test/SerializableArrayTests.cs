using NUnit.Framework;

namespace CoreModule.Serialization
{
    public class SerializableBitArrayTests
    {
        [Test]
        public void BitArray_CanSetAndGetBitsCorrectly()
        {
            var bits = new SerializableBitArray(16); // 16ビットの配列

            // 全ビット false のはず
            for (int i = 0; i < 16; i++)
                Assert.IsFalse(bits[i]);

            // 任意のビットをセット
            bits[0] = true;
            bits[3] = true;
            bits[7] = true;
            bits[15] = true;

            Assert.IsTrue(bits[0]);
            Assert.IsTrue(bits[3]);
            Assert.IsTrue(bits[7]);
            Assert.IsTrue(bits[15]);

            // 他は false のまま
            Assert.IsFalse(bits[1]);
            Assert.IsFalse(bits[2]);
            Assert.IsFalse(bits[4]);
            Assert.IsFalse(bits[14]);
        }

        [Test]
        public void BitArray_GetBytes_ReturnsCorrectData()
        {
            var bitArray = new SerializableBitArray(8);
            bitArray[0] = true; // 00000001
            bitArray[3] = true; // 00001001
            bitArray[7] = true; // 10001001

            var bytes = bitArray.GetBytes();

            Assert.AreEqual(1, bytes.Length);
            Assert.AreEqual(0b10001001, bytes[0]);
        }

        [Test]
        public void BitArray_Length_MatchesConstructorInput()
        {
            var bitArray = new SerializableBitArray(13);
            Assert.AreEqual(13, bitArray.length);
        }

        [Test]
        public void CountTrueBitsSimd_BasicTest()
        {
            unsafe
            {
                // 全ビット0の配列（16バイト = 128ビット）
                byte[] allZeros = new byte[16];
                fixed (byte* ptr = allZeros)
                {
                    BitArrayUtils.CountTrueBitsSimd(ptr, allZeros.Length, out int count);
                    Assert.AreEqual(0, count);
                }

                // 全ビット1の配列
                byte[] allOnes = new byte[16];
                for (int i = 0; i < allOnes.Length; i++)
                {
                    allOnes[i] = 0xFF;
                }

                fixed (byte* ptr = allOnes)
                {
                    BitArrayUtils.CountTrueBitsSimd(ptr, allOnes.Length, out int count);
                    Assert.AreEqual(128, count);
                }

                // 交互にビット立てた配列（0xAA = 10101010b）
                byte[] alternating = new byte[16];
                for (int i = 0; i < alternating.Length; i++)
                {
                    alternating[i] = 0xAA;
                }

                // 0xAA は1バイトあたり4ビット立っている
                fixed (byte* ptr = alternating)
                {
                    BitArrayUtils.CountTrueBitsSimd(ptr, alternating.Length, out int count);
                    Assert.AreEqual(16 * 4, count);
                }

                // 不規則なビットパターン
                byte[] pattern = new byte[] { 0b_0000_0001, 0b_1000_0000, 0b_1111_0000 };
                int expectedCount = 1 + 1 + 4; // 各バイトの立っているビット数合計

                fixed (byte* ptr = pattern)
                {
                    BitArrayUtils.CountTrueBitsSimd(ptr, pattern.Length, out int count);
                    Assert.AreEqual(expectedCount, count);
                }
            }
        }

        [Test]
        public void CountTrueBitsSimd_EmptyArray_ReturnsZero()
        {
            byte[] empty = new byte[0];
            unsafe
            {
                fixed (byte* ptr = empty)
                {
                    BitArrayUtils.CountTrueBitsSimd(ptr, empty.Length, out int count);
                    Assert.AreEqual(0, count);
                }
            }
        }
    }
}