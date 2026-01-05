using System;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Module.Enemy.Hose
{
    public class HoseHead : MonoBehaviour
    {
        [SerializeField] private SplineContainer splineContainer;

        [Button("Rebuild")]
        public void Rebuild()
        {
            SetPositionAtTime(1f);
        }

// モデルの向きを微調整するための設定（公式に準拠）
        [SerializeField] private SplineComponent.AlignAxis objectForwardAxis = SplineComponent.AlignAxis.ZAxis;
        [SerializeField] private SplineComponent.AlignAxis objectUpAxis = SplineComponent.AlignAxis.YAxis;

        public void SetPositionAtTime(float t)
        {
            if (splineContainer == null) return;

            // 1. ローカル座標系での評価
            // SplineAnimate.cs の EvaluatePositionAndRotation と同様のフロー
            float3 localPos = splineContainer.EvaluatePosition(t);
            float3 localTangent = splineContainer.EvaluateTangent(t);
            float3 localUp = splineContainer.EvaluateUpVector(t);

            // 2. 接線がゼロの場合の回避処理（公式コードの移植）
            if (math.lengthsq(localTangent) <= math.EPSILON)
            {
                float delta = 0.01f;
                localTangent = (t < 1.0f)
                    ? splineContainer.EvaluateTangent(t + delta)
                    : splineContainer.EvaluateTangent(t - delta);
            }

            // 3. ワールド座標への変換（ズレ防止の核心）
            // ContainerのTransformを使用して、見た目通りの場所に配置する
            Vector3 worldPos = splineContainer.transform.TransformPoint(localPos);
            Vector3 worldTangent = splineContainer.transform.TransformDirection(localTangent);
            Vector3 worldUp = splineContainer.transform.TransformDirection(localUp);

            // 4. 回転の補正（Axis Remap）
            // モデルの「前」がZ軸でない場合に、正しい向きに修正する
            var remappedForward = GetAxisVector(objectForwardAxis);
            var remappedUp = GetAxisVector(objectUpAxis);
            var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

            // 5. 反映
            transform.position = worldPos; // localPosition ではなく position を使用
            if (worldTangent != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(worldTangent, worldUp) * axisRemapRotation;
            }
        }

        // 公式コードにある軸ベクトル取得の簡易版
        private Vector3 GetAxisVector(SplineComponent.AlignAxis axis)
        {
            switch (axis)
            {
                case SplineComponent.AlignAxis.XAxis: return Vector3.right;
                case SplineComponent.AlignAxis.YAxis: return Vector3.up;
                case SplineComponent.AlignAxis.ZAxis: return Vector3.forward;
                case SplineComponent.AlignAxis.NegativeXAxis: return Vector3.left;
                case SplineComponent.AlignAxis.NegativeYAxis: return Vector3.down;
                case SplineComponent.AlignAxis.NegativeZAxis: return Vector3.back;
                default: return Vector3.forward;
            }
        }
    }
}