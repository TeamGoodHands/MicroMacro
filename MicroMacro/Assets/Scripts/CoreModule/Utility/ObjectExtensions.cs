using UnityEngine;

namespace CoreModule.Utility
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 親を遡ってTryGetComponent（最初に見つかったコンポーネントを返す）
        /// </summary>
        public static bool TryGetComponentInParent<T>(this Transform start, out T component) where T : class
        {
            component = null;
            Transform current = start;

            while (current != null)
            {
                if (current.TryGetComponent(out component))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }
    }
}