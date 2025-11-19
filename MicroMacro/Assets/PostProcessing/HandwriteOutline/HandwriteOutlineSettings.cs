using PostProcessing.ChameleonOutline;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ChameleonOutline
{
    [System.Serializable]
    public class HandwriteOutlineSettings
    {
        public RenderPassEvent PrepassEvent = RenderPassEvent.BeforeRenderingTransparents;
        public RenderPassEvent CompositeEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        [Header("Crayon Base")]
        public float CrayonIntensityPixels = 1.0f;

        public float CrayonScale = 8.0f;
        public float CrayonTimeSpeed = 0.0f;
        public float CrayonTimeStepSize = 0.5f;

        [Header("Stroke Noise")]
        public Vector2 Stroke1NoiseOffset = Vector2.zero;

        public Vector2 Stroke2NoiseOffset = new Vector2(0.37f, 1.9f);
        public float Stroke2PerpendicularOffsetPixels = 2.0f;

        [Header("Jitter")]
        public float JitterIntensityPixels = 0.7f;

        public float JitterFrequency = 40.0f;

        [Header("Blend")]
        public float Stroke2Weight = 0.9f;

        public Color Stroke2ColorTint = new Color(0.95f, 0.98f, 1.0f, 1.0f);

        [Header("Alpha Shaping")]
        public float CoreThreshold = 0.85f;

        public float OuterAlphaMultiplier = 0.4f;
    }
}