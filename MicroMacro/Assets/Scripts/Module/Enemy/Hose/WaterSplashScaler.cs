using System;
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
        [SerializeField] private bool splashByScale;

        private static readonly int ScaleId = Shader.PropertyToID("Scale");
        private IWaterFlow waterFlow;

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
        }

        private void UpdateWaterSplashState(WaterState state)
        {
            Debug.Log(state);
            if (state == WaterState.Pushing)
            {
                headObject.SetActive(true);
                bodyObject.SetActive(true);
                waterSplash.Play();
                waterParticle.Play();
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