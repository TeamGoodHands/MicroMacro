using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CoreModule.Utility
{
    public readonly struct ListSegment<T> : IReadOnlyList<T>
    {
        private readonly List<T> list;
        private readonly int offset;
        private readonly int count;

        public ListSegment(List<T> list, int offset, int count)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if ((uint)offset > (uint)list.Count || (uint)count > (uint)(list.Count - offset))
            {
                throw new ArgumentOutOfRangeException();
            }

            this.list = list;
            this.offset = offset;
            this.count = count;
        }

        public int Count => count;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return list[offset + index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if ((uint)index >= (uint)count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                list[offset + index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(list, offset, count);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private readonly List<T> list;
            private readonly int end;     // offset + count を前計算
            private int index;            // 現在のインデックス（実配列インデックス）

            internal Enumerator(List<T> list, int offset, int count)
            {
                this.list = list;
                this.end = offset + count;
                this.index = offset - 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                index++;

                return index < end;
            }

            public T Current => list[index];
            object IEnumerator.Current => Current!;

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
            }
        }
    }
}
