using System;
using Module.Scaling;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Enemy.Hose
{
    public class WaterSplashScaler : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private SnakeHoseController snakeHoseController;
        [SerializeField] private float scaleAmountSplash;
        [SerializeField] private VisualEffect waterSplash;
        [SerializeField] private VisualEffect waterParticle;

        private static readonly int ScaleId = Shader.PropertyToID("Scale");

        private void Start()
        {
            scaler.OnScaleCompleted += UpdateWaterSplashScale;
            snakeHoseController.OnWaterStateChanged += UpdateWaterSplashState;

            if (snakeHoseController.CurrentIntensity == 0f)
            {
                waterSplash.Stop();
                waterParticle.Stop();
            }
        }

        private void UpdateWaterSplashState(bool isWaterOn)
        {
            if (isWaterOn)
            {
                waterSplash.Play();
                waterParticle.Play();
            }
            else
            {
                waterSplash.Stop();
                waterParticle.Stop();
            }
        }

        private void UpdateWaterSplashScale(ScaleEventArgs args)
        {
            if (waterSplash == null)
                return;
            
            float amount = waterSplash.GetFloat(ScaleId) + (args.CurrentStep - args.PreviousStep) * scaleAmountSplash;
            waterSplash.SetFloat(ScaleId, amount);
            waterParticle.SetFloat(ScaleId, amount);
        }
    }
}