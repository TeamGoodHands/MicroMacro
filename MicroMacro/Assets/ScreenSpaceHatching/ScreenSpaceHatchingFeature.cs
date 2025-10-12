using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Contents.ScreenSpaceHatching
{
    public class ScreenSpaceHatchingFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class ScreenSpaceHatchingSettings
        {
            [Range(0f, 1f)] public float Blend = 0.5f;
            [Range(0.01f, 5f)] public float OcclusionSampleLength = 1f;
            [Range(0f, 5f)] public float OcclusionMinDistance = 0f;
            [Range(0f, 150f)] public float OcclusionMaxDistance = 5f;
            [Range(0f, 1f)] public float OcclusionBias = 0.001f;
            [Range(0f, 4f)] public float OcclusionStrength = 1f;
            [Range(0.1f, 4f)] public float OcclusionPower = 1f;
            [Range(0.1f, 100f)] public float OcclusionDifferenceThreshold = 20f;
            [Range(0.1f, 1f)] public float BlendStep = 0.5f;
            [Range(0.1f, 5f)] public float BlendPower = 1f;
            [Range(0.1f, 10f)] public float HatchScale = 1f;
            [Range(-10f, 10f)] public float HatchOffset = 1f;
            [Range(2, 32)] public int BlurKernelRadius = 6;
            public Texture2D CrossPatternTexture;

            public float BlurStandardDeviation => Mathf.Floor((float)BlurKernelRadius * 0.5f);

            public Color OcclusionColor = Color.black;
            public Shader ssaoShader;
        }

        public ScreenSpaceHatchingSettings settings = new ScreenSpaceHatchingSettings();
        private Material material;
        private ScreenSpaceHatchingPass pass;

        public override void Create()
        {
            if (settings.ssaoShader != null)
            {
                material = CoreUtils.CreateEngineMaterial(settings.ssaoShader);
            }
            else
            {
                Debug.LogWarning("ScreenSpaceHatching shader not assigned in ScreenSpaceHatchingFeature.");
            }

            pass = new ScreenSpaceHatchingPass(material, settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (pass != null)
            {
                renderer.EnqueuePass(pass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(material);
        }
    }
}