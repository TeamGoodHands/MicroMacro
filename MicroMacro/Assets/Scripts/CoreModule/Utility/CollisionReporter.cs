using UnityEngine;

namespace CoreModule.Utility
{
    /// <summary>
    /// 指定レイヤーとの衝突を監視し、フラグで通知する補助スクリプト
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class CollisionReporter : MonoBehaviour
    {
        [Tooltip("Ground / Player など衝突を検出したいレイヤー")]
        [SerializeField] private LayerMask targetMask;

        public bool HasHit { get; private set; }

        private void OnCollisionEnter(Collision other)
        {
            // ヒットした相手が対象レイヤーならフラグ ON
            if (((1 << other.gameObject.layer) & targetMask) != 0)
                HasHit = true;
        }

        /// <summary>外部から呼び出してヒットフラグをリセット</summary>
        public void ResetHit() => HasHit = false;
    }
}