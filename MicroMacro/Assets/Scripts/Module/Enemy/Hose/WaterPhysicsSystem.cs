using Constants;
using DG.Tweening;
using Module.Scaling;
using UGizmo;
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class WaterPhysicsSystem : MonoBehaviour
    {
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private Transform firePoint;   // WaterPivot
        [SerializeField] private WaterFlowParameter parameter;
        [SerializeField] private Scaler scaler;
        
        private float currentHitDistance;
        private RaycastHit hitInfo;
        
        // 初期値保持
        private float waterDefaultScaleX;
        private float scalerDefaultScaleY;
        private Quaternion defaultRotation;

        public float CurrentHitDistance => currentHitDistance;

        public void Start()
        {
            // 初期スケールを記録
            if (firePoint != null)
            {
                waterDefaultScaleX = firePoint.localScale.x;
                // 安全策: 初期値が0なら1とみなす
                if (waterDefaultScaleX < 0.01f) waterDefaultScaleX = 1.0f;
            }
            
            if (scaler != null) scalerDefaultScaleY = scaler.transform.localScale.y;
            if (rotatePivot != null) defaultRotation = rotatePivot.localRotation;
        }

        public void RunPhysics(float intensity, Transform playerTransform)
        {
            if (intensity <= 0.001f)
            {
                currentHitDistance = 0f;
                return;
            }

            // 1. スケーラーの差分計算
            float lengthScaleDiff = scaler.transform.localScale.y - scalerDefaultScaleY;
            
            // 2. 水流の長さを計算（マイナスにならないようMaxで保護）
            float addedLength = Mathf.Max(0, lengthScaleDiff * parameter.LengthMultiplier);
            float targetLocalScaleX = waterDefaultScaleX + addedLength;
            
            // 3. 親のスケール影響（EffectiveScale）を計算
            float effectiveScale = 1f;
            if (firePoint.parent != null)
            {
                Vector3 worldScaleVec = firePoint.parent.TransformVector(firePoint.localRotation * Vector3.right);
                effectiveScale = worldScaleVec.magnitude;
            }

            // 4. ワールド空間での最大飛距離
            float maxDist = targetLocalScaleX * effectiveScale * intensity;



            // --- BoxCast処理 ---

            float radius = 0.5f; // 必要に応じて調整（0.2fくらい推奨）
            Vector3 halfExtents = new Vector3(radius, radius, radius);
            int layerMask = ~(Layer.Mask.PlayerOnly | Layer.Mask.Player | Layer.Mask.Enemy | 
                              Layer.Mask.Bullet | Layer.Mask.ThroughPlatform | 
                              Layer.Mask.IgnoreRaycast | Layer.Mask.WaterOnly);

            // ▼【修正】方向を up (Y軸) から right (X軸) に変更！
            bool isHitting = Physics.BoxCast(firePoint.position, halfExtents, firePoint.right, out hitInfo, firePoint.rotation, maxDist, layerMask, QueryTriggerInteraction.Ignore);

            if (isHitting)
            {
                currentHitDistance = hitInfo.distance;
                if (hitInfo.rigidbody != null)
                {
                    ApplyForce(hitInfo.rigidbody, hitInfo.point, playerTransform);
                }
            }
            else
            {
                currentHitDistance = maxDist;
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            // デバッグ描画も right に修正
            UGizmos.DrawBoxCast(firePoint.position, halfExtents, firePoint.right, firePoint.rotation, currentHitDistance, isHitting, hitInfo);
#endif
        }

        private void ApplyForce(Rigidbody targetRb, Vector3 hitPoint, Transform playerTransform)
        {
            bool isPlayer = playerTransform != null && targetRb.transform == playerTransform;
            float playerMult = isPlayer ? parameter.PlayerMultiplier : 1f;

            // ▼【修正】力の方向も right (X軸) に変更
            Vector3 pushForce = firePoint.right * (parameter.WaterPower.y * playerMult);
            
            // 横方向（拡散）
            Vector3 relativePos = hitPoint - firePoint.position;
            // ▼【修正】基準ベクトルを right に変更
            Vector3 sideDir = GetPerpendicularTowardPoint(firePoint.right, relativePos);
            Vector3 sideForce = sideDir * (parameter.WaterPower.x * parameter.SideForceMultiplier * playerMult);

            targetRb.AddForceAtPosition(pushForce + sideForce, hitPoint);
        }
        
        private Vector2 GetPerpendicularTowardPoint(Vector2 a, Vector2 p)
        {
            // ベクトルa(進行方向)に対して垂直なベクトルを返す
            // (1,0)なら(0,1)になるので、このロジックはそのまま使えます
            Vector2 n = new Vector2(-a.y, a.x).normalized;
            float s = Mathf.Sign(Vector2.Dot(n, p));
            return n * s;
        }

        public void LookAtTarget(Vector3 targetPos, float smoothSpeed)
        {
            Vector3 direction = (targetPos - rotatePivot.position).normalized;
            // 元のコードに合わせて -90f オフセットを入れていますが、
            // もし「見た目が90度ズレる」場合は、ここの -90f を削除してください。
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            
            float currentZ = rotatePivot.eulerAngles.z;
            float newZ = Mathf.LerpAngle(currentZ, targetAngle, smoothSpeed * Time.fixedDeltaTime);
            rotatePivot.rotation = Quaternion.Euler(0, 0, newZ);
        }
        
        public Vector3 CalculateForceForPusher(bool isPlayer)
        {
             float playerMult = isPlayer ? parameter.PlayerMultiplier : 1f;
             // ▼【修正】ここも right (X軸) に統一
             return firePoint.right * (parameter.WaterPower.y * playerMult);
        }

        public Tween ShakeBody(float duration)
        {
            return rotatePivot.DOShakePosition(duration, 0.002f, 30, 90, false, true);
        }

        public Tween ResetAngle(float time)
        {
            return rotatePivot.DOLocalRotateQuaternion(defaultRotation, time);
        }
    }
}