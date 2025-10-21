using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Contents.ScreenSpaceHatching
{
    public class ScreenSpaceHatchingPass : ScriptableRenderPass
    {
        private readonly Material material;
        private readonly ScreenSpaceHatchingFeature.ScreenSpaceHatchingSettings settings;

        // shader property IDs
        private static readonly int blendID = Shader.PropertyToID("_Blend");
        private static readonly int occlusionColorID = Shader.PropertyToID("_OcclusionColor");
        private static readonly int occlusionSampleLengthID = Shader.PropertyToID("_OcclusionSampleLength");
        private static readonly int occlusionMinDistanceID = Shader.PropertyToID("_OcclusionMinDistance");
        private static readonly int occlusionMaxDistanceID = Shader.PropertyToID("_OcclusionMaxDistance");
        private static readonly int occlusionBiasID = Shader.PropertyToID("_OcclusionBias");
        private static readonly int occlusionStrengthID = Shader.PropertyToID("_OcclusionStrength");
        private static readonly int occlusionPowerID = Shader.PropertyToID("_OcclusionPower");
        private static readonly int occlusionDifferenceThresholdID = Shader.PropertyToID("_OcclusionDifferenceThreshold");
        private static readonly int samplingRotationsID = Shader.PropertyToID("_SamplingRotations");
        private static readonly int samplingDistancesID = Shader.PropertyToID("_SamplingDistances");
        private static readonly int blurKernelRadiusID = Shader.PropertyToID("_BlurKernelRadius");
        private static readonly int blurStandardDeviationID = Shader.PropertyToID("_BlurStandardDeviation");
        private static readonly int blurResultTextureID = Shader.PropertyToID("_BlurResultTexture");
        private static readonly int blendStepID = Shader.PropertyToID("_BlendStep");
        private static readonly int blendPowerID = Shader.PropertyToID("_BlendPower");
        private static readonly int hatchScaleID = Shader.PropertyToID("_HatchScale");
        private static readonly int frontHatchOffsetID = Shader.PropertyToID("_FrontHatchOffset");
        private static readonly int backHatchOffsetID = Shader.PropertyToID("_BackHatchOffset");
        private static readonly int hatchOffsetBorderID = Shader.PropertyToID("_HatchOffsetBorder");
        private static readonly int crossPatternTextureID = Shader.PropertyToID("_CrossHatchPatternTexture");

        public ScreenSpaceHatchingPass(Material mat, ScreenSpaceHatchingFeature.ScreenSpaceHatchingSettings settings)
        {
            this.material = mat;
            (float[] rotations, float[] length) samplingData = settings.GetSamplingData();
            material.SetFloatArray(samplingRotationsID, samplingData.rotations);
            material.SetFloatArray(samplingDistancesID, samplingData.length);

            this.settings = settings;
            this.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        private class PassData
        {
            public Material Material;
            public ScreenSpaceHatchingFeature.ScreenSpaceHatchingSettings Settings;
            public TextureHandle Source; // カメラのカラーデータ
            public TextureHandle Destination; // 書き込み先 (temp)
        }


        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // フレームデータを取得
            var cameraData = frameData.Get<UniversalCameraData>();
            var resourceData = frameData.Get<UniversalResourceData>();

            // マテリアルがnullだったら終了
            if (material == null)
                return;

            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = (int)DepthBits.None;
            TextureHandle commitTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SSAOResult", false);

            desc.graphicsFormat = GraphicsFormat.R16_SFloat;

            // SSAOを書き込むための一時テクスチャを作成
            TextureHandle ssaoTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "SSAO_TempColor", false);

            desc.width = Mathf.Max(1, desc.width / 8);
            desc.height = Mathf.Max(1, desc.height / 8);
            TextureHandle downsampleTarget =
                UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SSAODownSample", false, FilterMode.Bilinear);
            TextureHandle horizontalBlurTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_OutlineHorizontalBlur", false);
            TextureHandle verticalBlurTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_OutlineVerticalBlur", false);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SSAO: Compute", out var passData))
            {
                passData.Material = material;
                passData.Settings = settings;
                passData.Destination = ssaoTarget;

                builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);

                // 描画先を登録
                builder.SetRenderAttachment(passData.Destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    // パラメータを設定
                    data.Material.SetFloat(blendID, data.Settings.Blend);
                    data.Material.SetColor(occlusionColorID, data.Settings.OcclusionColor);
                    data.Material.SetFloat(occlusionSampleLengthID, data.Settings.OcclusionSampleLength);
                    data.Material.SetFloat(occlusionMinDistanceID, data.Settings.OcclusionMinDistance);
                    data.Material.SetFloat(occlusionMaxDistanceID, data.Settings.OcclusionMaxDistance);
                    data.Material.SetFloat(occlusionBiasID, data.Settings.OcclusionBias);
                    data.Material.SetFloat(occlusionStrengthID, data.Settings.OcclusionStrength);
                    data.Material.SetFloat(occlusionPowerID, data.Settings.OcclusionPower);
                    data.Material.SetFloat(occlusionDifferenceThresholdID, data.Settings.OcclusionDifferenceThreshold);

                    // 描画
                    Blitter.BlitTexture(ctx.cmd, Texture2D.whiteTexture, Vector2.one, data.Material, 0);
                });
            }


            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SSAO: Downsampling", out var passData))
            {
                passData.Material = material;
                passData.Settings = settings;
                passData.Destination = downsampleTarget;
                passData.Source = ssaoTarget;

                builder.UseTexture(passData.Source, AccessFlags.Read);

                builder.SetRenderAttachment(passData.Destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    // 描画
                    Blitter.BlitTexture(ctx.cmd, passData.Source, Vector2.one, data.Material, 1);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SSAO: Horizontal Blur", out var passData))
            {
                passData.Material = material;
                passData.Settings = settings;
                passData.Destination = horizontalBlurTarget;
                passData.Source = downsampleTarget;

                builder.UseTexture(passData.Source, AccessFlags.Read);

                builder.SetRenderAttachment(passData.Destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    // パラメータを設定
                    data.Material.SetFloat(blurKernelRadiusID, data.Settings.BlurKernelRadius);
                    data.Material.SetFloat(blurStandardDeviationID, data.Settings.BlurStandardDeviation);

                    // 描画
                    Blitter.BlitTexture(ctx.cmd, passData.Source, Vector2.one, data.Material, 2);
                });
            }


            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SSAO: Vertical Blur", out var passData))
            {
                passData.Material = material;
                passData.Settings = settings;
                passData.Destination = verticalBlurTarget;
                passData.Source = horizontalBlurTarget;

                builder.UseTexture(passData.Source, AccessFlags.Read);

                builder.SetRenderAttachment(passData.Destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    // パラメータを設定
                    data.Material.SetFloat(blurKernelRadiusID, data.Settings.BlurKernelRadius);
                    data.Material.SetFloat(blurStandardDeviationID, data.Settings.BlurStandardDeviation);

                    // 描画
                    Blitter.BlitTexture(ctx.cmd, passData.Source, Vector2.one, data.Material, 3);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SSAO: Commit to Camera", out var passData2))
            {
                passData2.Material = material;
                passData2.Settings = settings;
                passData2.Destination = commitTarget;
                passData2.Source = resourceData.activeColorTexture;

                builder.UseTexture(passData2.Source, AccessFlags.Read);
                builder.UseTexture(verticalBlurTarget, AccessFlags.Read);
                builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);

                builder.SetRenderAttachment(passData2.Destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.Material.SetTexture(blurResultTextureID, verticalBlurTarget);
                    data.Material.SetTexture(crossPatternTextureID, data.Settings.CrossPatternTexture);
                    data.Material.SetFloat(blendStepID, data.Settings.BlendStep);
                    data.Material.SetFloat(blendPowerID, data.Settings.BlendPower);
                    data.Material.SetFloat(hatchScaleID, data.Settings.HatchScale);
                    data.Material.SetFloat(frontHatchOffsetID, data.Settings.FrontHatchOffset);
                    data.Material.SetFloat(backHatchOffsetID, data.Settings.BackHatchOffset);
                    ;
                    data.Material.SetFloat(hatchOffsetBorderID, data.Settings.HatchOffsetBorder);
                    ;

                    Blitter.BlitTexture(ctx.cmd, data.Source, Vector2.one, data.Material, 4);
                });
            }

            resourceData.cameraColor = commitTarget;
        }
    }
}