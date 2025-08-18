using UnityEngine;

namespace Module.Scaling
{
    /// <summary>
    /// Vector3に関連する汎用処理を書くユーティリティクラス
    /// </summary>
    public static class Vector3Util
    {
        /// <summary>
        ///  Vector3の各要素同士を除算する
        /// </summary>
        public static Vector3 Divide(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
    }
}