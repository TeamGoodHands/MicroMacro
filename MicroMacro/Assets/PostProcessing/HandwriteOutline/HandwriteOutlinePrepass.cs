using ChameleonOutline;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PostProcessing.HandwriteOutline
{
    public class HandwriteOutlinePrepass : ScriptableRenderPass
    {
        private readonly OutlineSharedData outlineSharedData;

        private static readonly ShaderTagId[] writeTagIds =
        {
            new ShaderTagId("HandwriteOutlinePrepass"),
        };

        private static readonly ShaderTagId[] cutoutTagIds =
        {
            new ShaderTagId("HandwriteOutlineCutoutPrepass"),
        };

        private class PrePassData
        {
            public TextureHandle PrepassTexture;
            public RendererListHandle RendererList;
        }

        public HandwriteOutlinePrepass(HandwriteOutlineSettings settings, OutlineSharedData outlineSharedData)
        {
            this.outlineSharedData = outlineSharedData;
            renderPassEvent = settings.PrepassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = (int)DepthBits.None;
            desc.msaaSamples = (int)MSAASamples.None;
            desc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm; // RGBチャンネル: アウトラインの色, Aチャンネル: アウトラインの太さ

            outlineSharedData.PrepassTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_HandwriteOutlinePrepass", true);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("HandwriteOutline: Prepass", out PrePassData passData))
            {
                passData.PrepassTexture = outlineSharedData.PrepassTexture;

                // RendererList を作る
                RendererListDesc rendererListDesc = new RendererListDesc(writeTagIds, renderingData.cullResults, cameraData.camera)
                {
                    overrideMaterial = null,
                    layerMask = cameraData.camera.cullingMask,
                    renderQueueRange = RenderQueueRange.all,
                };

                passData.RendererList = renderGraph.CreateRendererList(rendererListDesc);
                builder.UseRendererList(passData.RendererList);

                builder.SetRenderAttachment(passData.PrepassTexture, 0);

                builder.SetRenderFunc((PrePassData data, RasterGraphContext context) => { context.cmd.DrawRendererList(data.RendererList); });
            }

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("HandwriteOutline: Cutout", out PrePassData passData))
            {
                passData.PrepassTexture = outlineSharedData.PrepassTexture;

                // RendererList を作る
                RendererListDesc rendererListDesc = new RendererListDesc(cutoutTagIds, renderingData.cullResults, cameraData.camera)
                {
                    overrideMaterial = null,
                    renderQueueRange = RenderQueueRange.all,
                    layerMask = cameraData.camera.cullingMask,
                };

                passData.RendererList = renderGraph.CreateRendererList(rendererListDesc);
                builder.UseRendererList(passData.RendererList);

                builder.SetRenderAttachment(passData.PrepassTexture, 0);

                builder.SetRenderFunc((PrePassData data, RasterGraphContext context) => { context.cmd.DrawRendererList(data.RendererList); });
            }
        }
    }
}