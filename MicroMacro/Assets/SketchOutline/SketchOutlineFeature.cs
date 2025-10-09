using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace SketchOutline
{
    public class SketchOutlineFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader outlineShader;
        [SerializeField] private SketchOutlineSettings settings = new SketchOutlineSettings();
        private Material material;

        private static readonly int edgeColorID = Shader.PropertyToID("_EdgeColor");
        private static readonly int edgeWidthID = Shader.PropertyToID("_EdgeWidth");
        private static readonly int edgeThresholdID = Shader.PropertyToID("_EdgeThreshold");
        private static readonly int wobbleAmplitudeID = Shader.PropertyToID("_WobbleAmplitude");
        private static readonly int wobbleFrequencyID = Shader.PropertyToID("_WobbleFrequency");
        private static readonly int noiseScaleID = Shader.PropertyToID("_NoiseScale");
        private static readonly int quantizeStepSecondsID = Shader.PropertyToID("_QuantizeStepSeconds");
        private static readonly int paperTextureID = Shader.PropertyToID("_PaperTexture");

        [Serializable]
        private class SketchOutlineSettings
        {
            public Color EdgeColor = Color.black;
            [Range(0.0f, 3.0f)] public float EdgeWidth = 1f;
            [Range(0.0f, 2.0f)] public float EdgeThreshold = 0.5f;
            [Range(0.0f, 2.0f)] public float WobbleAmplitude = 0.6f;
            [Range(0.0f, 10.0f)] public float WobbleFrequency = 3.0f;
            [Range(0.5f, 10f)] public float NoiseScale = 3.0f;
            [Range(0.0f, 1.0f)] public float QuantizeStepSeconds = 0.25f;
            public Texture2D PaperTexture;
        }

        private class SketchOutlinePass : ScriptableRenderPass
        {
            private readonly Material material;
            private readonly SketchOutlineSettings settings;

            // RenderGraph に渡すデータ入れ物
            private class PassData
            {
                public Material Material;
                public TextureHandle Source;
                public TextureHandle Destination;
            }

            public SketchOutlinePass(Material material, SketchOutlineSettings settings)
            {
                this.material = material;
                this.settings = settings;

                // 他PPと干渉しにくい位置（線をハッキリ残したいならこのまま）
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null)
                    return;

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;

                // 入力（現在のカラーバッファ）
                TextureHandle source = resourceData.activeColorTexture;

                // 出力（同サイズの一時テクスチャ）
                desc.depthBufferBits = (int)DepthBits.None;
                TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SketchTemporary", false);

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("SketchOutlinePass", out var passData))
                {
                    passData.Material = material;
                    passData.Source = source;
                    passData.Destination = destination;

                    builder.UseTexture(passData.Source, AccessFlags.Read);

                    builder.SetRenderAttachment(passData.Destination, 0, AccessFlags.Write);

                    // グローバルは禁止。Blitter×Materialだけで完結させる。
                    builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    {
                        data.Material.SetTexture(paperTextureID, data.Source);
                        data.Material.SetColor(edgeColorID, settings.EdgeColor);
                        data.Material.SetFloat(edgeWidthID, settings.EdgeWidth);
                        data.Material.SetFloat(edgeThresholdID, settings.EdgeThreshold);
                        data.Material.SetFloat(wobbleAmplitudeID, settings.WobbleAmplitude);
                        data.Material.SetFloat(wobbleFrequencyID, settings.WobbleFrequency);
                        data.Material.SetFloat(noiseScaleID, settings.NoiseScale);
                        data.Material.SetFloat(quantizeStepSecondsID, settings.QuantizeStepSeconds);

                        // src -> dst へ 1パス描画（マテリアルのPass 0を使用）
                        Blitter.BlitTexture(ctx.cmd, data.Source, Vector4.one, data.Material, 0);
                    });
                }

                // 以降のパスが参照するアクティブカラーを差し替える
                resourceData.cameraColor = destination;
            }
        }

        private SketchOutlinePass pass;

        public override void Create()
        {
            if (outlineShader != null)
            {
                material = CoreUtils.CreateEngineMaterial(outlineShader);
            }
            else
            {
                Debug.LogWarning("ScreenSpaceHatching shader not assigned in ScreenSpaceHatchingFeature.");
            }

            pass = new SketchOutlinePass(material, settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (outlineShader == null)
                return;

            renderer.EnqueuePass(pass);
        }
    }
}