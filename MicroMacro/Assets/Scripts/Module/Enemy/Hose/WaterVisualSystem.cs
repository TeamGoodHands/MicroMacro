using DG.Tweening;
using Module.Scaling;
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class WaterVisualSystem : MonoBehaviour
    {
        [SerializeField] private Transform waterPivot;
        [SerializeField] private Renderer screwRenderer;
        [SerializeField] private Renderer waterRenderer;
        [SerializeField] private float manualLengthOffset; 
        
        private WaterFlowParameter parameter;
        private WaterPhysicsSystem physicsSystem;
        private Vector3 defaultScale;
        
        private static readonly int WaterThresholdId = Shader.PropertyToID("_WaterThreshold");
        private static readonly int MainColorId = Shader.PropertyToID("_MainColor");

        public void Initialize(WaterFlowParameter param, Scaler scl)
        {
            parameter = param;
            physicsSystem = GetComponent<WaterPhysicsSystem>();
            defaultScale = waterPivot.localScale;
        }

        public void UpdateVisuals(float intensity)
        {
            if (intensity <= 0.001f)
            {
                waterPivot.localScale = Vector3.zero;
                return;
            }

            float actualDistance = physicsSystem.CurrentHitDistance;

            // ▼ 修正: 親のスケール補正を「EffectiveScale」に戻しました
            float effectiveScale = 1f;
            if (waterPivot.parent != null)
            {
                // 親の変換を通して、X軸方向の正しい倍率を取得
                Vector3 worldScaleVec = waterPivot.parent.TransformVector(waterPivot.localRotation * Vector3.right);
                effectiveScale = worldScaleVec.magnitude;
            }

            Vector3 newScale = defaultScale;

            // 0除算対策をしつつ、ワールド距離を実質倍率で割ってローカルスケールに変換
            if (effectiveScale > 0.0001f)
            {
                newScale.x = actualDistance / effectiveScale;
            }
            else
            {
                newScale.x = 0f;
            }
            
            waterPivot.localScale = newScale + Vector3.right * manualLengthOffset;
        }

        public void SetRapidsMode(bool isRapids)
        {
            if (isRapids)
            {
                screwRenderer.material.DOFloat(parameter.ScrewWidth, WaterThresholdId, 1f);
                waterRenderer.material.SetColor(MainColorId, parameter.RapidsWaterColor);
            }
            else
            {
                screwRenderer.material.DOFloat(1f, WaterThresholdId, 1f);
                waterRenderer.material.SetColor(MainColorId, parameter.DefaultWaterColor);
            }
        }
    }
}