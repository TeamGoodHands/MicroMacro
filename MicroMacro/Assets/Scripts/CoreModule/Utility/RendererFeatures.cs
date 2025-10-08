using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CoreModule.Utility
{
    public static class RendererFeatures
    {
        /// <summary>
        /// 指定したRendererFeatureを取得します
        /// </summary>
        /// <param name="rendererFeature"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryGetRendererFeature<T>(out T rendererFeature) where T : ScriptableRendererFeature
        {
            rendererFeature = null;

            var pipelineAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;

            if (pipelineAsset == null)
                return false;

            var renderer = pipelineAsset.GetRenderer(0);
            var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

            if (property == null)
                return false;

            List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;

            if (features == null)
                return false;   

            foreach (ScriptableRendererFeature feature in features)
            {
                if (feature is T scriptableRendererFeature)
                {
                    rendererFeature = scriptableRendererFeature;
                    return true;
                }
            }

            return false;
        }
    }
}