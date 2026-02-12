using System;
using DG.Tweening;
using Module.Management;
using Module.Scaling;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Enemy.Hose
{
    public class WaterSplashScaler : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private GameObject headObject;
        [SerializeField] private GameObject bodyObject;
        [SerializeField] private GameObject waterFlowObject;
        [SerializeField] private float scaleAmountSplash;
        [SerializeField] private VisualEffect waterSplash;
        [SerializeField] private VisualEffect waterParticle;
        [SerializeField] private AreaSoundManager waterSound;
        [SerializeField] private bool splashByScale;

        private static readonly int ScaleId = Shader.PropertyToID("Scale");
        private IWaterFlow waterFlow;
        private Tween soundTween;

        private void Start()
        {
            waterFlow = waterFlowObject.GetComponent<IWaterFlow>();

            scaler.OnScaleStarted += UpdateWaterSplashScale;
            waterFlow.OnWaterStateChanged += UpdateWaterSplashState;

            if (waterFlow.CurrentIntensity == 0f)
            {
                waterSplash.Stop();
                waterParticle.Stop();
            }
            else if(waterSound != null)
            {
                waterSound.Volume = 1.0f;
            }
        }

        public void UpdateWaterSplashState(WaterState state)
        {
            if (state == WaterState.Pushing)
            {
                headObject.SetActive(true);
                bodyObject.SetActive(true);
                waterSplash.Play();
                waterParticle.Play();

                soundTween?.Kill();
                if (waterSound != null)
                {
                    soundTween = DOTween.To(() => waterSound.Volume, x => waterSound.Volume = x, 1.0f, 0.5f);
                }
            }
            else if (state == WaterState.Ending)
            {
                waterSplash.Stop();
                waterParticle.Stop();
            }
            else if (state == WaterState.End)
            {
                headObject.SetActive(false);
                bodyObject.SetActive(false);

                soundTween?.Kill();

                if (waterSound != null)
                {
                    soundTween = DOTween.To(() => waterSound.Volume, x => waterSound.Volume = x, 0.0f, 0.5f);
                }
            }
        }

        private void UpdateWaterSplashScale(ScaleEventArgs args)
        {
            if (waterSplash == null)
                return;

            if (splashByScale)
            {
                if (args.State == State.MinScale)
                {
                    UpdateWaterSplashState(WaterState.Ending);
                }
                else
                {
                    UpdateWaterSplashState(WaterState.Pushing);
                }
            }


            float amount = waterSplash.GetFloat(ScaleId) + (args.CurrentStep - args.PreviousStep) * scaleAmountSplash;
            waterSplash.SetFloat(ScaleId, amount);
            waterParticle.SetFloat(ScaleId, amount);
        }
    }
}