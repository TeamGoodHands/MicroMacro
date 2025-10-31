using System;

namespace CoreModule.Utility
{
    public class UniqueRandom
    {
        private readonly int minimumInclusive;
        private readonly int maximumInclusive;
        private readonly int[] pool;
        private readonly bool autoReset;
        private readonly Random random;
        private readonly int length;
        
        private int remainingCount;

        public int Length => length;
        public int CountRemaining => remainingCount;

        public UniqueRandom(int minimumInclusive, int maximumInclusive, bool autoReset = true, int? seed = null)
        {
            if (minimumInclusive > maximumInclusive)
            {
                throw new ArgumentException("minimumInclusive must be less than or equal to maximumInclusive.");
            }

            this.minimumInclusive = minimumInclusive;
            this.maximumInclusive = maximumInclusive;
            this.autoReset = autoReset;
            this.random = seed.HasValue ? new Random(seed.Value) : new Random();

            length = maximumInclusive - minimumInclusive + 1;
            pool = new int[length];

            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < length; i++)
            {
                pool[i] = minimumInclusive + i;
            }

            remainingCount = length;
        }

        /// <summary>
        /// 重複なしの次の値を返す。尽きたら autoReset に従う。
        /// </summary>
        public int Next()
        {
            if (remainingCount == 0)
            {
                if (autoReset)
                {
                    Reset();
                }
                else
                {
                    throw new InvalidOperationException("All values have been consumed.");
                }
            }

            int index = random.Next(remainingCount);
            int value = pool[index];

            // 取り出した要素を末尾とスワップして残数をデクリメント（O(1)）
            pool[index] = pool[remainingCount - 1];
            pool[remainingCount - 1] = value;
            remainingCount--;

            return value;
        }

        /// <summary>
        /// 値が残っている間は true を返しつつ out に値を入れる。
        /// while で一巡だけ使いたい時に便利。
        /// </summary>
        public bool TryNext(out int value)
        {
            if (remainingCount == 0)
            {
                value = default;

                return false;
            }

            int index = random.Next(remainingCount);
            value = pool[index];
            pool[index] = pool[remainingCount - 1];
            pool[remainingCount - 1] = value;
            remainingCount--;

            return true;
        }
    }
}