using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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

        [Serializable]
        private class SketchOutlineSettings
        {
            public Color outlineColor = Color.black;
            [Range(0.5f, 6f)] public float thickness = 1.0f;

            [Header("Jitter (Sketchy Outline)")]
            public bool enableJitter = true;

            [Range(0f, 1f)] public float blend = 1.0f;
            [Range(0f, 0.01f)] public float jitterAmpPixels = 0.75f; 
            [Range(8f, 512f)] public float jitterScale = 160f;
            [Range(0f, 2f)] public float jitterSpeed = 1.0f;
            [Range(0.01f, 1f)] public float timeStepSize = 1f;

            [Range(0f, 0.01f)] public float depthLow = 1f / 220f;
            [Range(0f, 0.01f)] public float depthHigh = 1f / 180f;
            [Range(0f, 1f)] public float normalLow = 1f / 4.5f;
            [Range(0f, 1f)] public float normalHigh = 1f / 3.5f;

            public RenderPassEvent injectEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        private class SketchOutlinePass : ScriptableRenderPass
        {
            private readonly Material material;
            private readonly SketchOutlineSettings settings;

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

                TextureHandle commitTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SketchedTarget", false);

                desc.graphicsFormat = GraphicsFormat.R16_SFloat;
                TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SketchTemporary", false);

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("SketchOutlinePass: Edge Detection", out var passData))
                {
                    passData.Material = material;
                    passData.Destination = destination;

                    builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                    builder.UseTexture(resourceData.cameraNormalsTexture, AccessFlags.Read);

                    builder.SetRenderAttachment(passData.Destination, 0, AccessFlags.Write);

                    builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    {
                        data.Material.SetColor(outlineColorID, settings.outlineColor);
                        data.Material.SetFloat(thicknessID, settings.thickness);

                        Blitter.BlitTexture(ctx.cmd, Texture2D.blackTexture, Vector2.one, data.Material, 0);
                    });
                }

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("SketchOutlinePass: Composite", out var passData))
                {
                    passData.Material = material;
                    passData.Source = resourceData.activeColorTexture;
                    passData.Destination = commitTarget;

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

                        Blitter.BlitTexture(ctx.cmd, data.Source, Vector2.one, data.Material, 1);
                    });
                }

                resourceData.cameraColor = commitTarget;
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

            pass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Motion);
            renderer.EnqueuePass(pass);
        }
    }
}