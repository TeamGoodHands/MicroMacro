using ChameleonOutline;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PostProcessing.HandwriteOutline
{
    public class HandwriteOutlineCompositePass : ScriptableRenderPass
    {
        private HandwriteOutlineSettings settings;
        private Material material;
        private readonly OutlineSharedData outlineSharedData;

        private static readonly int outlinePrepassTextureId = Shader.PropertyToID("_OutlinePrepassTexture");

        private static readonly int crayonIntensityPixelsId = Shader.PropertyToID("_CrayonIntensityPixels");
        private static readonly int crayonScaleId = Shader.PropertyToID("_CrayonScale");
        private static readonly int crayonTimeSpeedId = Shader.PropertyToID("_CrayonTimeSpeed");
        private static readonly int crayonTimeStepSize = Shader.PropertyToID("_CrayonTimeStepSize");

        private static readonly int stroke1NoiseOffsetId = Shader.PropertyToID("_Stroke1NoiseOffset");
        private static readonly int stroke2NoiseOffsetId = Shader.PropertyToID("_Stroke2NoiseOffset");
        private static readonly int stroke2PerpendicularOffsetPixelsId = Shader.PropertyToID("_Stroke2PerpendicularOffsetPixels");

        private static readonly int jitterIntensityPixelsId = Shader.PropertyToID("_JitterIntensityPixels");
        private static readonly int jitterFrequencyId = Shader.PropertyToID("_JitterFrequency");

        private static readonly int stroke2WeightId = Shader.PropertyToID("_Stroke2Weight");
        private static readonly int stroke2ColorTintId = Shader.PropertyToID("_Stroke2ColorTint");

        private static readonly int coreThresholdId = Shader.PropertyToID("_CoreThreshold");
        private static readonly int outerAlphaMultiplierId = Shader.PropertyToID("_OuterAlphaMultiplier");

        private class PassData
        {
            public TextureHandle Source;
            public TextureHandle Destination;
            public TextureHandle PrepassTexture;
            public Material Material;
        }

        public HandwriteOutlineCompositePass(HandwriteOutlineSettings settings, Material material, OutlineSharedData outlineSharedData)
        {
            this.settings = settings;
            this.material = material;
            this.outlineSharedData = outlineSharedData;

            renderPassEvent = settings.CompositeEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // マテリアルがnullだったら終了
            if (material == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = (int)DepthBits.None;

            TextureHandle commitTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_HandwriteOutlineCommit", false);
            TextureHandle prepassTexture = outlineSharedData.PrepassTexture;

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("HandwriteOutline: Composite", out PassData passData))
            {
                passData.PrepassTexture = prepassTexture;
                passData.Source = resourceData.activeColorTexture;
                passData.Destination = commitTarget;
                passData.Material = material;

                builder.UseTexture(passData.PrepassTexture, AccessFlags.Read);
                builder.UseTexture(passData.Source, AccessFlags.Read);

                builder.SetRenderAttachment(passData.Destination, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Read);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.Material.SetTexture(outlinePrepassTextureId, data.PrepassTexture);

                    data.Material.SetFloat(crayonIntensityPixelsId, settings.CrayonIntensityPixels);
                    data.Material.SetFloat(crayonScaleId, settings.CrayonScale);
                    data.Material.SetFloat(crayonTimeSpeedId, settings.CrayonTimeSpeed);
                    data.Material.SetFloat(crayonTimeStepSize, settings.CrayonTimeStepSize);

                    data.Material.SetVector(stroke1NoiseOffsetId, settings.Stroke1NoiseOffset);
                    data.Material.SetVector(stroke2NoiseOffsetId, settings.Stroke2NoiseOffset);
                    data.Material.SetFloat(stroke2PerpendicularOffsetPixelsId, settings.Stroke2PerpendicularOffsetPixels);

                    data.Material.SetFloat(jitterIntensityPixelsId, settings.JitterIntensityPixels);
                    data.Material.SetFloat(jitterFrequencyId, settings.JitterFrequency);

                    data.Material.SetFloat(stroke2WeightId, settings.Stroke2Weight);
                    data.Material.SetColor(stroke2ColorTintId, settings.Stroke2ColorTint);
                    
                    data.Material.SetFloat(coreThresholdId, settings.CoreThreshold);
                    data.Material.SetFloat(outerAlphaMultiplierId, settings.OuterAlphaMultiplier);

                    Blitter.BlitTexture(context.cmd, data.Source, Vector2.one, data.Material, 0);
                });
            }

            resourceData.cameraColor = commitTarget;
        }
    }
}