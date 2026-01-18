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
        private ScreenSpaceHatchingVolume volumeSettings;

        private static readonly int occlusionLengthID = Shader.PropertyToID("_OcclusionSampleLength");
        private static readonly int minDistanceID = Shader.PropertyToID("_OcclusionMinDistance");
        private static readonly int maxDistanceID = Shader.PropertyToID("_OcclusionMaxDistance");
        private static readonly int occlusionBiasID = Shader.PropertyToID("_OcclusionBias");
        private static readonly int strengthID = Shader.PropertyToID("_OcclusionStrength");
        private static readonly int occlusionPowerID = Shader.PropertyToID("_OcclusionPower");
        private static readonly int thresholdID = Shader.PropertyToID("_OcclusionDifferenceThreshold");
        private static readonly int samplingRotationsID = Shader.PropertyToID("_SamplingRotations");
        private static readonly int samplingDistancesID = Shader.PropertyToID("_SamplingDistances");
        private static readonly int blurRadiusID = Shader.PropertyToID("_BlurKernelRadius");
        private static readonly int blurDevID = Shader.PropertyToID("_BlurStandardDeviation");
        private static readonly int blurResultTextureID = Shader.PropertyToID("_BlurResultTexture");
        private static readonly int blendStepID = Shader.PropertyToID("_BlendStep");
        private static readonly int blendPowerID = Shader.PropertyToID("_BlendPower");
        private static readonly int hatchScaleID = Shader.PropertyToID("_HatchScale");
        private static readonly int frontOffsetID = Shader.PropertyToID("_FrontHatchOffset");
        private static readonly int backOffsetID = Shader.PropertyToID("_BackHatchOffset");
        private static readonly int offsetBorderID = Shader.PropertyToID("_HatchOffsetBorder");
        private static readonly int crossPatternID = Shader.PropertyToID("_CrossHatchPatternTexture");
        
        private float[] samplingRotations;
        private float[] samplingLengths;

        public ScreenSpaceHatchingPass(Material material)
        {
            this.material = material;
            this.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }
        

        public void Setup(ScreenSpaceHatchingVolume volume, float[] samplingRotations, float[] samplingLength)
        {
            this.samplingRotations = samplingRotations;
            this.samplingLengths = samplingLength;
            this.volumeSettings = volume;
        }

        private class PassData
        {
            public Material Material;
            public ScreenSpaceHatchingVolume Volume;
            public TextureHandle Source;
            public TextureHandle Destination;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            if (material == null || volumeSettings == null)
                return;

            material.SetFloatArray(samplingRotationsID, samplingRotations);
            material.SetFloatArray(samplingDistancesID, samplingLengths);

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = (int)DepthBits.None;
            TextureHandle finalTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SSHatchingResult", false);

            desc.graphicsFormat = GraphicsFormat.R16_SFloat;
            TextureHandle ssaoTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SSHatchingCompute", false);

            desc.width = Mathf.Max(1, desc.width / 8);
            desc.height = Mathf.Max(1, desc.height / 8);
            TextureHandle downTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SSHatchingDown", false, FilterMode.Bilinear);
            TextureHandle hBlurTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SSHatchingHBlur", false);
            TextureHandle vBlurTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SSHatchingVBlur", false);

            // 1. Compute Pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SSHatching: Compute", out PassData passData))
            {
                passData.Material = material;
                passData.Volume = volumeSettings;
                passData.Destination = ssaoTarget;
                builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                builder.SetRenderAttachment(passData.Destination, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.Material.SetFloat(occlusionLengthID, data.Volume.OcclusionLength.value);
                    data.Material.SetFloat(minDistanceID, data.Volume.MinDistance.value);
                    data.Material.SetFloat(maxDistanceID, data.Volume.MaxDistance.value);
                    data.Material.SetFloat(occlusionBiasID, data.Volume.OcclusionBias.value);
                    data.Material.SetFloat(strengthID, data.Volume.Strength.value);
                    data.Material.SetFloat(occlusionPowerID, data.Volume.OcclusionPower.value);
                    data.Material.SetFloat(thresholdID, data.Volume.OcclusionThreshold.value);
                    Blitter.BlitTexture(ctx.cmd, Texture2D.whiteTexture, Vector2.one, data.Material, 0);
                });
            }

            // 2. Downsampling Pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SSHatching: Downsample", out PassData passData))
            {
                passData.Material = material;
                passData.Source = ssaoTarget;
                passData.Destination = downTarget;
                builder.UseTexture(passData.Source, AccessFlags.Read);
                builder.SetRenderAttachment(passData.Destination, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.Source, Vector2.one, data.Material, 1);
                });
            }

            // 3. Horizontal Blur Pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SSHatching: HBlur", out PassData passData))
            {
                passData.Material = material;
                passData.Volume = volumeSettings;
                passData.Source = downTarget;
                passData.Destination = hBlurTarget;
                builder.UseTexture(passData.Source, AccessFlags.Read);
                builder.SetRenderAttachment(passData.Destination, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    float deviation = Mathf.Floor((float)data.Volume.BlurRadius.value * 0.5f);
                    data.Material.SetFloat(blurRadiusID, data.Volume.BlurRadius.value);
                    data.Material.SetFloat(blurDevID, deviation);
                    Blitter.BlitTexture(ctx.cmd, data.Source, Vector2.one, data.Material, 2);
                });
            }

            // 4. Vertical Blur Pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SSHatching: VBlur", out PassData passData))
            {
                passData.Material = material;
                passData.Volume = volumeSettings;
                passData.Source = hBlurTarget;
                passData.Destination = vBlurTarget;
                builder.UseTexture(passData.Source, AccessFlags.Read);
                builder.SetRenderAttachment(passData.Destination, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    float deviation = Mathf.Floor((float)data.Volume.BlurRadius.value * 0.5f);
                    data.Material.SetFloat(blurRadiusID, data.Volume.BlurRadius.value);
                    data.Material.SetFloat(blurDevID, deviation);
                    Blitter.BlitTexture(ctx.cmd, data.Source, Vector2.one, data.Material, 3);
                });
            }

            // 5. Composite Pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SSHatching: Composite", out PassData passData))
            {
                passData.Material = material;
                passData.Volume = volumeSettings;
                passData.Source = resourceData.activeColorTexture;
                passData.Destination = finalTarget;
                builder.UseTexture(passData.Source, AccessFlags.Read);
                builder.UseTexture(vBlurTarget, AccessFlags.Read);
                builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                builder.SetRenderAttachment(passData.Destination, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.Material.SetTexture(blurResultTextureID, vBlurTarget);
                    data.Material.SetTexture(crossPatternID, data.Volume.CrossPattern.value);
                    data.Material.SetFloat(blendStepID, data.Volume.BlendStep.value);
                    data.Material.SetFloat(blendPowerID, data.Volume.BlendPower.value);
                    data.Material.SetFloat(hatchScaleID, data.Volume.HatchScale.value);
                    data.Material.SetFloat(frontOffsetID, data.Volume.FrontOffset.value);
                    data.Material.SetFloat(backOffsetID, data.Volume.BackOffset.value);
                    data.Material.SetFloat(offsetBorderID, data.Volume.OffsetBorder.value);
                    Blitter.BlitTexture(ctx.cmd, data.Source, Vector2.one, data.Material, 4);
                });
            }

            resourceData.cameraColor = finalTarget;
        }

    }
}