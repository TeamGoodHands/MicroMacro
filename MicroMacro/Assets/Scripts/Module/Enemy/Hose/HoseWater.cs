using System;
using Constants;
using Module.Scaling;
using UGizmo;
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class HoseWater
    {
        private readonly Scaler scaler;
        private readonly Transform waterPivot;
        private readonly Transform rotatePivot;
        private readonly HoseParameter parameter;
        private RaycastHit hitInfo;

        public HoseWater(Scaler scaler, Transform waterPivot, Transform rotatePivot, HoseParameter parameter)
        {
            this.scaler = scaler;
            this.waterPivot = waterPivot;
            this.rotatePivot = rotatePivot;
            this.parameter = parameter;
        }

        public bool TryAddWaterForce(float radius, out Vector3 hitPoint)
        {
            const int layerMask = ~(Layer.Mask.PlayerOnly |
                                    Layer.Mask.Enemy |
                                    Layer.Mask.Bullet |
                                    Layer.Mask.ThroughPlatform);

            hitPoint = Vector2.zero;

            Vector3 position = rotatePivot.position;
            Vector3 halfExtents = new Vector3(radius, radius, radius);
            float maxDistance = Mathf.Max(0f, scaler.transform.localScale.x * waterPivot.localScale.x - radius);

            // 水が飛ぶ方向に球体キャストを行う
            bool isObjectHit = Physics.BoxCast(position, halfExtents, rotatePivot.up, out hitInfo, rotatePivot.rotation, maxDistance, layerMask);

            if (isObjectHit)
            {
                hitPoint = hitInfo.point;

                if (hitInfo.rigidbody != null)
                {
                    // 水の力を加える
                    ApplyWaterForce(hitPoint);
                }
            }

            // デバッグ表示
            UGizmos.DrawBoxCast(position, halfExtents, rotatePivot.up, rotatePivot.rotation, maxDistance, isObjectHit, hitInfo);

            return isObjectHit;
        }

        private void ApplyWaterForce(Vector3 hitPoint)
        {
            bool isPlayerHit = hitInfo.collider.CompareTag(Tag.Player);

            // 縦方向と横方向の力を計算する
            Vector3 verticalForce = CalculateVerticalForce(isPlayerHit);
            Vector3 horizontalForce = CalculateHorizontalForce(isPlayerHit);
            Vector3 force = verticalForce + horizontalForce;

            // 衝突したポイントに力を加える
            hitInfo.rigidbody.AddForceAtPosition(force, hitPoint);
        }

        private Vector3 CalculateVerticalForce(bool isPlayerHit)
        {
            float scaleMultiplier = parameter.ScaleMultiplier * (scaler.CurrentStep - scaler.MinStep);
            float playerMultiplier = isPlayerHit ? parameter.PlayerMultiplier : 1f;

            // 水流方向に吹き飛ばす力を求める
            Vector3 verticalForce = rotatePivot.up * parameter.WaterPower * (scaleMultiplier * playerMultiplier);

            return verticalForce;
        }

        private Vector3 CalculateHorizontalForce(bool isPlayerHit)
        {
            Vector2 relativePosition = hitInfo.rigidbody.position - waterPivot.position;
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