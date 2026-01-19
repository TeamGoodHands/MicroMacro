using System;
using Constants;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class FallObjectEnemyChecker : MonoBehaviour
    {
        [SerializeField] private float  castDistanceMultiplier = 0.3f;
        public event Action<GameObject> OnHit;
        public event Action OnCheckStart;
        public event Action OnCheckStop;

        private bool isCheck;

        public void StartCheck()
        {
            isCheck = true;
            OnCheckStart?.Invoke();
        }

        public void StopCheck()
        {
            isCheck = false;
            OnCheckStop?.Invoke();
        }

        private void Update()
        {
            if (!isCheck)
                return;

            (bool isHit, GameObject target) = CheckGround();
            if (isHit)
            {
                OnHit?.Invoke(target);
            }
        }

        private (bool isHit, GameObject target) CheckGround()
        {
            // BoxCast の開始位置をわずかに持ち上げる
            const float startOffset = 0.01f;

            // BoxCast の距離
            float castDistance = transform.localScale.y * castDistanceMultiplier;

            // 半径（Half-Extents）を取得
            Vector3 halfExtents = transform.localScale * 0.5f;

            // BoxCast 実行
            Vector3 origin = transform.position + Vector3.up * startOffset;
            bool hit = Physics.BoxCast(
                origin,
                halfExtents,
                Vector3.down,
                out RaycastHit hitInfo,
                Quaternion.identity,
                castDistance,
                Layer.Mask.WaterOnly,
                QueryTriggerInteraction.Ignore);

            return (hit, hitInfo.collider?.gameObject);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.down * transform.localScale.y * castDistanceMultiplier, transform.localScale);
        }
    }
}