using ChameleonOutline;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace SketchOutline
{
    public class SketchOutlineTransparentPrepass : ScriptableRenderPass
    {
         private readonly OutlineSharedData outlineSharedData;

        private static readonly ShaderTagId[] writeTagIds =
        {
            new ShaderTagId("SketchOutlineTransparentPrepass"),
        };

        private class PrePassData
        {
            public TextureHandle PrepassTexture;
            public RendererListHandle RendererList;
        }

        public SketchOutlineTransparentPrepass( OutlineSharedData outlineSharedData)
        {
            this.outlineSharedData = outlineSharedData;
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = (int)DepthBits.None;
            desc.msaaSamples = (int)MSAASamples.None;
            desc.graphicsFormat = GraphicsFormat.R8G8B8A8_SNorm; // RGBチャンネル: アウトラインの色, Aチャンネル: アウトラインの太さ

            outlineSharedData.PrepassTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SketchOutlineTransparentPrepass", true);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("SketchOutlineTransparent: Prepass", out PrePassData passData))
            {
                passData.PrepassTexture = outlineSharedData.PrepassTexture;
                
                builder.AllowPassCulling(false);

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
        }
    }
}