using System;
using Constants;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class FallObjectEnemyChecker : MonoBehaviour
    {
        public event Action<GameObject> OnHit;

        private bool isCheck;

        public void StartCheck()
        {
            isCheck = true;
        }

        public void StopCheck()
        {
            isCheck = false;
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
            const float castDistance = 0.1f;

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
                Layer.Mask.Enemy,
                QueryTriggerInteraction.Ignore);

            return (hit, hitInfo.collider?.gameObject);
        }
    }
}