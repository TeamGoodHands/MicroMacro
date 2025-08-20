using UnityEngine;

namespace Module.Scaling
{
    /// <summary>
    /// Vector2に関連する汎用処理を書くユーティリティクラス
    /// </summary>
    public static class Vector2Util
    {
        /// <summary>
        ///  Vector2の各要素同士を除算する
        /// </summary>
        public static Vector2 Divide(Vector2 a, Vector2 b)
        {
            // ゼロ除算回避
            if (Mathf.Approximately(b.x, 0f) ||
                Mathf.Approximately(b.y, 0f))
            {
                Debug.LogError("ゼロ除算が発生しています");
                return new Vector2(0f, 0f);
            }
            
            return new Vector2(a.x / b.x, a.y / b.y);
        }
    }
}