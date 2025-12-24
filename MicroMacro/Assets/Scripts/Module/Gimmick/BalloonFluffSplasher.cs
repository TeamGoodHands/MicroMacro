using Module.Scaling;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Gimmick
{
    public class BalloonFluffSplasher : MonoBehaviour
    {
        [SerializeField] private float scaleAmount = 0.1f;
        [SerializeField] private Scaler targetScaler;
        [SerializeField] private VisualEffect splashEffect;

        private static readonly int ScaleId = Shader.PropertyToID("Scale");

        private void Start()
        {
            targetScaler.OnScaleStarted += OnScaleStarted;
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            float amount = splashEffect.GetFloat(ScaleId) + (args.CurrentStep - args.PreviousStep) * scaleAmount;
            splashEffect.SetFloat(ScaleId, amount);
        }
    }
}