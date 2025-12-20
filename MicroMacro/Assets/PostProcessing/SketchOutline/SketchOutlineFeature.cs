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
        private SketchOutlinePassOpaque passOpaque;
        private SketchOutlineTransparentPrepass prepassTransparent;
        private SketchOutlinePassTransparent passTransparent;
        private OutlineSharedData outlineSharedData;

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
            
            outlineSharedData = new OutlineSharedData();
            passOpaque = new SketchOutlinePassOpaque(material, settings, outlineSharedData);
            prepassTransparent = new SketchOutlineTransparentPrepass(outlineSharedData);
            passTransparent = new SketchOutlinePassTransparent(material, settings, outlineSharedData);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (outlineShader == null)
                return;

            passOpaque.ConfigureInput(ScriptableRenderPassInput.Normal);
            renderer.EnqueuePass(passOpaque);
            renderer.EnqueuePass(prepassTransparent);
            renderer.EnqueuePass(passTransparent);
        }
    }
    
    public class OutlineSharedData
    {
        public TextureHandle PrepassTexture;
        public TextureHandle SketchedTarget;
    }
}