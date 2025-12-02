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

            passOpaque = new SketchOutlinePassOpaque(material, settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (outlineShader == null)
                return;

            passOpaque.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
            renderer.EnqueuePass(passOpaque);
        }
    }
}