using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CoreModule.ObjectPool
{
    public static class ObjectPool
    {
        public static Transform Root => poolRoot ??= new GameObject("Pool").transform;
        private static Transform poolRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            poolRoot = null;
        }
    }

    /// <summary>
    /// オブジェクトプール (マルチスレッド非対応)
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> objects;
        private readonly Func<T> createFunc;
        private readonly Action<T> onReset;

        public ObjectPool(Func<T> createFunc, Action<T> onReset = null, int initialCapacity = 16)
        {
            if (createFunc == null)
                throw new ArgumentNullException(nameof(createFunc));

            this.objects = new Stack<T>(initialCapacity);
            this.createFunc = createFunc;
            this.onReset = onReset;

            // プールの事前確保
            for (int i = 0; i < initialCapacity; i++)
            {
                objects.Push(createFunc());
            }
        }

        /// <summary>
        /// オブジェクトをプールから取得します。空の場合は新規生成します。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetOrCreate()
        {
            return objects.Count > 0 ? objects.Pop() : createFunc();
        }

        /// <summary>
        /// プール内にオブジェクトがあれば取得します。
        /// </summary>
        public bool TryGet(out T item)
        {
            if (objects.Count > 0)
            {
                item = objects.Pop();
                return true;
            }

            item = null;
            return false;
        }

        /// <summary>
        /// オブジェクトをプールに返却します。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            // 必要に応じてリセット
            onReset?.Invoke(item);
            objects.Push(item);
        }

        /// <summary>
        /// プール内のオブジェクト数を取得します。
        /// </summary>
        public int Count => objects.Count;
    }
}