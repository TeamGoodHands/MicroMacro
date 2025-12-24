using System;
using Module.Scaling;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Enemy.Hose
{
    public class WaterSplashScaler : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private float scaleAmountSplash;
        [SerializeField] private VisualEffect waterSplash;
        [SerializeField] private VisualEffect waterParticle;

        private static readonly int ScaleId = Shader.PropertyToID("Scale");

        private void Start()
        {
            scaler.OnScaleCompleted += UpdateWaterSplashScale;
        }

        private void UpdateWaterSplashScale(ScaleEventArgs args)
        {
            float amount = waterSplash.GetFloat(ScaleId) + (args.CurrentStep - args.PreviousStep) * scaleAmountSplash;
            waterSplash.SetFloat(ScaleId, amount);
            waterParticle.SetFloat(ScaleId, amount);
        }
    }
}