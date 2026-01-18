using System;
using Constants;
using Module.Scaling;
using UGizmo;
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class WaterFlow
    {
        private readonly Scaler scaler;
        private readonly Transform waterPivot;
        private readonly Transform rotatePivot;
        private readonly WaterFlowParameter parameter;
        private RaycastHit hitInfo;

        public WaterFlow(Scaler scaler, Transform waterPivot, Transform rotatePivot, WaterFlowParameter parameter)
        {
            this.scaler = scaler;
            this.waterPivot = waterPivot;
            this.rotatePivot = rotatePivot;
            this.parameter = parameter;
        }

        public bool TryAddWaterForce(float radius, float maxDistance, out float actualDistance)
        {
            const int layerMask = ~(Layer.Mask.PlayerOnly |
                                    Layer.Mask.Player |
                                    Layer.Mask.Enemy |
                                    Layer.Mask.Bullet |
                                    Layer.Mask.ThroughPlatform |
                                    Layer.Mask.IgnoreRaycast |
                                    Layer.Mask.WaterOnly);

            actualDistance = maxDistance; // デフォルトは最大距離

            Vector3 position = rotatePivot.position;
            Vector3 halfExtents = new Vector3(radius, radius, radius);

            // 水が飛ぶ方向に球体キャストを行う
            bool isObjectHit = Physics.BoxCast(position, halfExtents, rotatePivot.up, out hitInfo, rotatePivot.rotation, maxDistance, layerMask);

            if (isObjectHit)
            {
                Vector2 hitPoint = hitInfo.point;
                actualDistance = hitInfo.distance + radius; // ヒットした場合はその距離を採用

                if (hitInfo.rigidbody != null)
                {
                    // 水の力を加える
                    ApplyWaterForceAtPosition(hitInfo.rigidbody, hitPoint);
                }
            }
            else
            {
                actualDistance = maxDistance + radius;
            }

#if UNITY_EDITOR
            // デバッグ表示
            UGizmos.DrawBoxCast(position, halfExtents, rotatePivot.up, rotatePivot.rotation, actualDistance, isObjectHit, hitInfo);
#endif

            return isObjectHit;
        }

        public void ApplyWaterForceAtPosition(Rigidbody rigidbody, Vector3 hitPoint, bool isPlayerHit = false)
        {
            // 縦方向と横方向の力を計算する
            Vector3 verticalForce = CalculateVerticalForce(isPlayerHit);
            Vector3 horizontalForce = CalculateHorizontalForce(isPlayerHit, rigidbody.position);
            Vector3 force = verticalForce + horizontalForce;

            // 衝突したポイントに力を加える
            rigidbody.AddForceAtPosition(force, hitPoint);
        }

        public void ApplyWaterForce(Rigidbody rigidbody, bool isPlayerHit, ForceMode forceMode)
        {
            // 縦方向と横方向の力を計算する
            Vector3 verticalForce = CalculateVerticalForce(isPlayerHit);
            Vector3 horizontalForce = CalculateHorizontalForce(isPlayerHit, rigidbody.position);
            Vector3 force = verticalForce + horizontalForce;

            // 衝突したポイントに力を加える
            rigidbody.AddForce(force, forceMode);
        }

        public Vector3 CalculateVerticalForce(bool isPlayerHit)
        {
            float scaleMultiplier = parameter.ScaleMultiplier * (scaler.CurrentStep - scaler.MinStep);
            float playerMultiplier = isPlayerHit ? parameter.PlayerMultiplier : 1f;

            // 水流方向に吹き飛ばす力を求める
            Vector3 verticalForce = rotatePivot.up * parameter.WaterPower * (scaleMultiplier * playerMultiplier);

            return verticalForce;
        }

        public Vector3 CalculateHorizontalForce(bool isPlayerHit, Vector3 hitPosition)
        {
            Vector2 relativePosition = hitPosition - waterPivot.position;
            float playerMultiplier = isPlayerHit ? parameter.PlayerMultiplier : 1f;

            // 水流に対して垂直なベクトルを求める
            Vector2 sideDirection = GetPerpendicularTowardPoint(rotatePivot.up, relativePosition);

            // サイドに吹き飛ばす力を求める
            Vector3 horizontalForce = sideDirection * parameter.WaterPower * (parameter.SideForceMultiplier * playerMultiplier);

            return horizontalForce;
        }

        /// <summary>
        /// ベクトルaに対して、点pに垂直なベクトルを求める関数
        /// </summary>
        private Vector2 GetPerpendicularTowardPoint(Vector2 a, Vector2 p)
        {
            Vector2 n = new Vector2(-a.y, a.x).normalized;
            float s = Mathf.Sign(Vector2.Dot(n, p));
            return n * s;
        }
    }
}