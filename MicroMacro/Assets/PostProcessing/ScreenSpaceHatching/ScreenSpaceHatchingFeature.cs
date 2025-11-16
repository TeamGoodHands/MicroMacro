using NaughtyAttributes;
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
            [Range(-1f, 1f)] public float FrontHatchOffset = 1f;
            [Range(-1f, 1f)] public float BackHatchOffset = 1f;
            [Range(0f, 0.1f)] public float HatchOffsetBorder = 0.5f;
            [Range(2, 32)] public int BlurKernelRadius = 6;
            public Texture2D CrossPatternTexture;

            public float BlurStandardDeviation => Mathf.Floor((float)BlurKernelRadius * 0.5f);

            public Shader ssaoShader;
            public bool generateSamplingPoint = true;

            private const int SamplingCount = 12;

            [SerializeField, HideInInspector] private float[] samplingRotations = new float[SamplingCount];

            [SerializeField, HideInInspector] private float[] samplingLength = new float[SamplingCount];

            public (float[] rotations, float[] length) GetSamplingData()
            {
                if (generateSamplingPoint)
                {
                    for (int i = 0; i < SamplingCount; i++)
                    {
                        // 任意の角度. できるだけ均等にバラけていた方がよい
                        float pieceRad = (Mathf.PI * 2) / SamplingCount;
                        float rad = UnityEngine.Random.Range(
                            pieceRad * i,
                            pieceRad * (i + 1)
                        );

                        samplingRotations[i] = rad;

                        // 任意の長さの範囲. できるだけ均等にバラけていた方がよい
                        float baseLen = 0.1f;
                        float pieceLen = (1f - baseLen) / SamplingCount;
                        float len = UnityEngine.Random.Range(
                            baseLen + pieceLen * i,
                            baseLen + pieceLen * (i + 1)
                        );

                        samplingLength[i] = len;
                    }
                }

                return (samplingRotations, samplingLength);
            }
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
                pass.ConfigureInput(ScriptableRenderPassInput.Depth);
                renderer.EnqueuePass(pass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(material);
        }
    }
}