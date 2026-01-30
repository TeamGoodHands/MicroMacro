using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PostProcessing.ScreenSpaceHatching
{
    public class ScreenSpaceHatchingPrepass : ScriptableRenderPass
    {
        private readonly OutlineSharedData outlineSharedData;

        private static readonly ShaderTagId[] cutoutTagIds =
        {
            new ShaderTagId("ScreenSpaceHatchingCutoutPrepass"),
        };

        private class PrePassData
        {
            public TextureHandle PrepassTexture;
            public RendererListHandle RendererList;
        }

        public ScreenSpaceHatchingPrepass(OutlineSharedData outlineSharedData)
        {
            this.outlineSharedData = outlineSharedData;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = (int)DepthBits.None;
            desc.msaaSamples = (int)MSAASamples.None;
            desc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm; // RGBチャンネル: アウトラインの色, Aチャンネル: アウトラインの太さ

            outlineSharedData.PrepassTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_ScreenSpaceHatchingPrepass", true);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("SSHatching: Cutout", out PrePassData passData))
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

        public class OutlineSharedData
        {
            public TextureHandle PrepassTexture;
        }
    }
}