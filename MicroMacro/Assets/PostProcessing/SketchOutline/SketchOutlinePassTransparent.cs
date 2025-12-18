using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace SketchOutline
{
    public class SketchOutlinePassTransparent : ScriptableRenderPass
    {
        private readonly Material material;
        private readonly SketchOutlineSettings settings;
        private readonly OutlineSharedData outlineSharedData;
        private TextureHandle originalNormalTexture;

        private static readonly int outlineColorID = Shader.PropertyToID("_OutlineColor");
        private static readonly int blendID = Shader.PropertyToID("_Blend");
        private static readonly int thicknessID = Shader.PropertyToID("_Thickness");
        private static readonly int edgeTextureID = Shader.PropertyToID("_EdgeTexture");
        private static readonly int jitterAmpPixelsID = Shader.PropertyToID("_JitterAmpPixels");
        private static readonly int jitterScaleID = Shader.PropertyToID("_JitterScale");
        private static readonly int jitterSpeedID = Shader.PropertyToID("_JitterSpeed");
        private static readonly int depthLoId = Shader.PropertyToID("_DepthLo");
        private static readonly int depthHiId = Shader.PropertyToID("_DepthHi");
        private static readonly int normalLoId = Shader.PropertyToID("_NormalLo");
        private static readonly int normalHiId = Shader.PropertyToID("_NormalHi");
        private static readonly int timeStepSizeId = Shader.PropertyToID("_TimeStepSize");

        private class PassData
        {
            public Material Material;
            public TextureHandle Source;
            public TextureHandle Destination;
        }

        public SketchOutlinePassTransparent(Material material, SketchOutlineSettings settings,
            OutlineSharedData outlineSharedData)
        {
            this.material = material;
            this.settings = settings;
            this.outlineSharedData = outlineSharedData;

            renderPassEvent = settings.injectEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;

            desc.depthBufferBits = (int)DepthBits.None;

            TextureHandle commitTarget =
                UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SketchedTarget", false);

            desc.graphicsFormat = GraphicsFormat.R16_SFloat;
            TextureHandle destination =
                UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SketchTemporary", false);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SketchOutlineTransparentPass: Edge Detection", out var passData))
            {
                passData.Material = material;
                passData.Destination = destination;

                builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                builder.UseTexture(outlineSharedData.PrepassTexture, AccessFlags.Read);

                if (resourceData.cameraNormalsTexture.IsValid())
                {
                    builder.UseTexture(resourceData.cameraNormalsTexture);
                }

                builder.SetRenderAttachment(passData.Destination, 0, AccessFlags.Write);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.Material.SetColor(outlineColorID, settings.outlineColor);
                    data.Material.SetFloat(thicknessID, settings.thickness);
                    ctx.cmd.SetGlobalTexture("_CameraNormalsTexture", outlineSharedData.PrepassTexture);

                    Blitter.BlitTexture(ctx.cmd, Texture2D.blackTexture, Vector2.one, data.Material, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SketchOutlineTransparentPass: Composite", out var passData))
            {
                passData.Material = material;
                passData.Source = resourceData.activeColorTexture;
                passData.Destination = commitTarget;

                // 元の法線マップのハンドルを取得して渡す
                if (resourceData.cameraNormalsTexture.IsValid())
                {
                    originalNormalTexture = resourceData.cameraNormalsTexture;
                    builder.UseTexture(originalNormalTexture);
                }
                
                builder.AllowGlobalStateModification(true);

                builder.UseTexture(passData.Source, AccessFlags.Read);
                builder.UseTexture(destination, AccessFlags.Read);

                builder.SetRenderAttachment(passData.Destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.Material.SetTexture(edgeTextureID, destination);
                    data.Material.SetFloat(jitterAmpPixelsID, settings.enableJitter ? settings.jitterAmpPixels : 0f);
                    data.Material.SetFloat(jitterScaleID, settings.jitterScale);
                    data.Material.SetFloat(jitterSpeedID, settings.jitterSpeed);
                    data.Material.SetFloat(depthLoId, settings.depthLow);
                    data.Material.SetFloat(depthHiId, settings.depthHigh);
                    data.Material.SetFloat(normalLoId, settings.normalLow);
                    data.Material.SetFloat(normalHiId, settings.normalHigh);
                    data.Material.SetFloat(timeStepSizeId, settings.timeStepSize);
                    data.Material.SetFloat(blendID, settings.blend);
                    
                    ctx.cmd.SetGlobalTexture("_CameraNormalsTexture", originalNormalTexture);

                    Blitter.BlitTexture(ctx.cmd, data.Source, Vector2.one, data.Material, 1);
                });
            }

            resourceData.cameraColor = commitTarget;
        }
    }
}