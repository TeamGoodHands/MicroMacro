using ChameleonOutline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PostProcessing.HandwriteOutline
{
    public class HandwriteOutlineFeature : ScriptableRendererFeature
    {
        [SerializeField] private HandwriteOutlineSettings featureSettings = new HandwriteOutlineSettings();
        [SerializeField] private Shader compositeShader;

        private Material compositeMaterial;
        private HandwriteOutlinePrepass prepass;
        private HandwriteOutlineCompositePass compositePass;
        private OutlineSharedData outlineSharedData;


        public override void Create()
        {
            outlineSharedData = new OutlineSharedData();
            compositeMaterial = CoreUtils.CreateEngineMaterial(compositeShader);

            prepass = new HandwriteOutlinePrepass(featureSettings, outlineSharedData);
            compositePass = new HandwriteOutlineCompositePass(featureSettings, compositeMaterial, outlineSharedData);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(prepass);
            renderer.EnqueuePass(compositePass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(compositeMaterial);
        }
    }

    public class OutlineSharedData
    {
        public TextureHandle PrepassTexture;
    }
}