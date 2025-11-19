using ChameleonOutline;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PostProcessing.ChameleonOutline
{
    public class ChameleonOutlinePass : ScriptableRenderPass
    {
        private ChameleonOutlineSettings settings;
        private Material material;

        private static readonly ShaderTagId[] shaderTagIds =
        {
            new ShaderTagId("ChameleonOutlinePrepass"),
        };

        private static readonly int outlinePrepassTextureId = Shader.PropertyToID("_OutlinePrepassTexture");
        private static readonly int nearestTextureId = Shader.PropertyToID("_NearestTexture");
        
        private static readonly int stepId = Shader.PropertyToID("_Step");

        private class PrePassData
        {
            public TextureHandle PrepassTexture;
            public RendererListHandle RendererList;
        }

        private class PassData
        {
            public TextureHandle Source;
            public TextureHandle Destination;
            public TextureHandle PrepassTexture;
            public TextureHandle NearestTexture;
            public Material Material;
        }

        public ChameleonOutlinePass(ChameleonOutlineSettings settings, Material material)
        {
            this.settings = settings;
            this.material = material;

            renderPassEvent = settings.PrepassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // マテリアルがnullだったら終了
            if (material == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = (int)DepthBits.None;

            TextureHandle commitTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SSHatchingResult", false);

            desc.msaaSamples = (int)MSAASamples.None;
            desc.graphicsFormat = GraphicsFormat.B8G8R8A8_UNorm; // RGBチャンネル: アウトラインの色, Aチャンネル: アウトラインの太さ

            TextureHandle prepassTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_ChameleonPrepass", true);

            desc.graphicsFormat = GraphicsFormat.R16G16_UNorm;
            TextureHandle nearestTextureA = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_ChameleonNearestA", true);
            TextureHandle nearestTextureB = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_ChameleonNearestB", true);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("ChameleonOutline: Prepass", out PrePassData passData))
            {
                passData.PrepassTexture = prepassTexture;

                // RendererList を作る
                RendererListDesc rendererListDesc = new RendererListDesc(shaderTagIds, renderingData.cullResults, cameraData.camera)
                {
                    overrideMaterial = null,
                    renderQueueRange = RenderQueueRange.all,
                    layerMask = cameraData.camera.cullingMask,
                };

                passData.RendererList = renderGraph.CreateRendererList(rendererListDesc);
                builder.UseRendererList(passData.RendererList);

                builder.SetRenderAttachment(passData.PrepassTexture, 0);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PrePassData data, RasterGraphContext context) => { context.cmd.DrawRendererList(data.RendererList); });
            }


            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("ChameleonOutline: NearestInit", out PassData passData))
            {
                passData.Source = prepassTexture;
                passData.Destination = nearestTextureA;
                passData.Material = material;

                builder.UseTexture(passData.Source, AccessFlags.Read);
                builder.AllowPassCulling(false);

                builder.SetRenderAttachment(passData.Destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.Source, Vector2.one, data.Material, 0);
                });
            }


            float step = Mathf.Max(desc.width, desc.height) / 2f;

            for (int i = 0; i < settings.JumpIterations; i++)
            {
                bool even = (i % 2 == 0);

                TextureHandle nearestInput = even ? nearestTextureA : nearestTextureB;
                TextureHandle nearestOutput = even ? nearestTextureB : nearestTextureA;

                using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("ChameleonOutline: NearestJump", out PassData passData))
                {
                    passData.Source = nearestInput;
                    passData.Destination = nearestOutput;
                    passData.Material = material;

                    builder.UseTexture(passData.Source, AccessFlags.Read);
                    builder.AllowPassCulling(false);

                    builder.SetRenderAttachment(passData.Destination, 0);

                    Vector2 uvStep = new Vector2(step / desc.width, step / desc.height);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        data.Material.SetVector(stepId, uvStep);
                        Blitter.BlitTexture(context.cmd, data.Source, Vector2.one, data.Material, 1);
                    });
                }

                step *= 0.5f;
            }

            TextureHandle finalNearestTex = settings.JumpIterations % 2 == 0 ? nearestTextureA : nearestTextureB;

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("ChameleonOutline: Composite", out PassData passData))
            {
                passData.PrepassTexture = prepassTexture;
                passData.NearestTexture = finalNearestTex;
                passData.Source = resourceData.activeColorTexture;
                passData.Destination = commitTarget;
                passData.Material = material;

                builder.UseTexture(passData.PrepassTexture, AccessFlags.Read);
                builder.UseTexture(passData.NearestTexture, AccessFlags.Read);
                builder.UseTexture(passData.Source, AccessFlags.Read);

                builder.SetRenderAttachment(passData.Destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.Material.SetTexture(outlinePrepassTextureId, data.PrepassTexture);
                    data.Material.SetTexture(nearestTextureId, data.NearestTexture);

                    Blitter.BlitTexture(context.cmd, data.Source, Vector2.one, data.Material, 2);
                });
            }

            resourceData.cameraColor = commitTarget;
        }
    }
}