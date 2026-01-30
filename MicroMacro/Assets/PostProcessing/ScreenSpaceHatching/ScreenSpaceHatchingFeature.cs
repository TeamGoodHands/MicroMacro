using PostProcessing.ScreenSpaceHatching;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Contents.ScreenSpaceHatching
{
    public class SSHatchingFeature : ScriptableRendererFeature
    {
        public Shader SSHatchingShader;
        public bool generateSamplingPoints;
        private Material hatchingMaterial;
        private ScreenSpaceHatchingPrepass hatchingPrepass;
        private ScreenSpaceHatchingPass hatchingPass;
        private ScreenSpaceHatchingPrepass.OutlineSharedData outlineSharedData;

        private const int SamplingCount = 12;
        [SerializeField, HideInInspector] private float[] samplingRotations = new float[SamplingCount];
        [SerializeField, HideInInspector] private float[] samplingLength = new float[SamplingCount];

        public override void Create()
        {
            if (SSHatchingShader != null)
            {
                hatchingMaterial = CoreUtils.CreateEngineMaterial(SSHatchingShader);
            }
            else
            {
                Debug.LogWarning("SSHatching shader is missing.");
            }
            
            outlineSharedData = new ScreenSpaceHatchingPrepass.OutlineSharedData();

            hatchingPrepass = new ScreenSpaceHatchingPrepass(outlineSharedData);
            hatchingPass = new ScreenSpaceHatchingPass(hatchingMaterial, outlineSharedData);
        }

        private void GenerateSamplingData()
        {
            if (!generateSamplingPoints)
                return;
            
            for (int i = 0; i < SamplingCount; i++)
            {
                float pieceRad = (Mathf.PI * 2) / SamplingCount;
                samplingRotations[i] = Random.Range(pieceRad * i, pieceRad * (i + 1));

                float baseLen = 0.1f;
                float pieceLen = (1f - baseLen) / SamplingCount;
                samplingLength[i] = Random.Range(baseLen + pieceLen * i, baseLen + pieceLen * (i + 1));
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (hatchingMaterial == null)
                return;

            VolumeStack stack = VolumeManager.instance.stack;
            ScreenSpaceHatchingVolume volume = stack.GetComponent<ScreenSpaceHatchingVolume>();

            if (volume == null || volume.IsActive() == false)
                return;
            
            renderer.EnqueuePass(hatchingPrepass);

            GenerateSamplingData();
            hatchingPass.Setup(volume,samplingRotations, samplingLength);
            hatchingPass.ConfigureInput(ScriptableRenderPassInput.Depth);
            renderer.EnqueuePass(hatchingPass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(hatchingMaterial);
        }
    }
}