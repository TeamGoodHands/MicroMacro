using PostProcessing.ChameleonOutline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ChameleonOutline
{
    public class ChameleonOutlineFeature : ScriptableRendererFeature
    {
        [SerializeField] private ChameleonOutlineSettings featureSettings = new ChameleonOutlineSettings();

        private Material compositeMaterial;
        private Texture2D outlineLutTexture;

        private ChameleonOutlinePass passPass;

        public override void Create()
        {
            Shader compositeShader = Shader.Find("Hidden/ChameleonOutline/Composite");

            compositeMaterial = CoreUtils.CreateEngineMaterial(compositeShader);

            passPass = new ChameleonOutlinePass(featureSettings, compositeMaterial);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(passPass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(compositeMaterial);
        }
    }
}