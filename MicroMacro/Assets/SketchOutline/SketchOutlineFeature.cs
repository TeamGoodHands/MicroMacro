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

        private static readonly int outlineColorID = Shader.PropertyToID("_OutlineColor");
        private static readonly int thicknessID = Shader.PropertyToID("_Thickness");
        private static readonly int depthThresholdID = Shader.PropertyToID("_DepthThreshold");
        private static readonly int blendID = Shader.PropertyToID("_Blend");
        private static readonly int edgeTextureID = Shader.PropertyToID("_EdgeTextureID");

        [Serializable]
        private class SketchOutlineSettings
        {
            public Color OutlineColor = Color.black;
            [Range(0.5f, 6f)] public float ThicknessPixels = 1.0f;
            [Range(0.0001f, 0.1f)] public float DepthThreshold = 0.01f;
            [Range(0f, 1f)] public float Blend = 0.0f;

            // 不透明物の後（透過も描かれた後）に走らせるのが扱いやすい
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;
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
                renderPassEvent = settings.Event;
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

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("SketchOutlinePass: Edge Detection", out var passData))
                {
                    passData.Material = material;
                    passData.Destination = destination;

                    builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);

                    builder.SetRenderAttachment(passData.Destination, 0, AccessFlags.Write);

                    // グローバルは禁止。Blitter×Materialだけで完結させる。
                    builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    {
                        data.Material.SetColor(outlineColorID, settings.OutlineColor);
                        data.Material.SetFloat(thicknessID, settings.ThicknessPixels);
                        data.Material.SetFloat(depthThresholdID, settings.DepthThreshold);
                        data.Material.SetFloat(blendID, settings.Blend);

                        // src -> dst へ 1パス描画（マテリアルのPass 0を使用）
                        Blitter.BlitTexture(ctx.cmd, Texture2D.blackTexture, Vector2.one, data.Material, 0);
                    });
                }
                
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("SketchOutlinePass: Edge Detection", out var passData))
                {
                    passData.Material = material;
                    passData.Source = destination;
                    passData.Destination = source;

                    builder.UseTexture(passData.Source, AccessFlags.Read);

                    builder.SetRenderAttachment(passData.Destination, 0, AccessFlags.Write);

                    // グローバルは禁止。Blitter×Materialだけで完結させる。
                    builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    {
                        data.Material.SetTexture(edgeTextureID, destination);
                        
                        // src -> dst へ 1パス描画（マテリアルのPass 0を使用）
                        Blitter.BlitTexture(ctx.cmd, data.Source, Vector2.one, data.Material, 1);
                    });
                }
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