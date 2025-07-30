using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.LevelEditor
{
    /// <summary>
    /// GameObjectのプレビューテクスチャを生成するクラス
    /// </summary>
    public class PreviewTextureCreator
    {
        private PreviewRenderUtility[] previewRenderUtilities;
        private GameObject[] instances;

        public List<RenderTexture> CreatePreviewTextures(GameObject[] prefabs)
        {
            if (prefabs.Length == 0)
                return new List<RenderTexture>();

            previewRenderUtilities = new PreviewRenderUtility[prefabs.Length];
            instances = new GameObject[prefabs.Length];

            for (int i = 0; i < prefabs.Length; i++)
            {
                var renderUtility = new PreviewRenderUtility(true);
                previewRenderUtilities[i] = renderUtility;
                instances[i] = Object.Instantiate(prefabs[i]);

                // 複数Rendererのバウンディングを結合して扱う
                Renderer[] renderers = instances[i].GetComponentsInChildren<Renderer>();
                SetupUtility(renderUtility, renderers);

                renderUtility.AddSingleGO(instances[i]);
            }

            var textures = new List<RenderTexture>();

            foreach (PreviewRenderUtility utility in previewRenderUtilities)
            {
                utility.BeginPreview(new Rect(0, 0, 128, 128), GUIStyle.none);
                utility.camera.Render();
                RenderTexture texture = (RenderTexture)utility.EndPreview();
                textures.Add(texture);
            }

            return textures;
        }

        // 複数RendererのBoundsをまとめてカメラやライトの調整を実施
        private void SetupUtility(PreviewRenderUtility previewRenderUtility, Renderer[] targetRenderers)
        {
            Camera targetCamera = previewRenderUtility.camera;
            float padding = 2f;

            if (targetCamera == null || targetRenderers == null || targetRenderers.Length == 0)
                return;

            targetCamera.orthographic = true;

            // 複数RendererのBoundsをまとめて算出
            Bounds bounds = targetRenderers[0].bounds;
            for (int i = 1; i < targetRenderers.Length; i++)
            {
                bounds.Encapsulate(targetRenderers[i].bounds);
            }

            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            float verticalSize = extents.y;
            float horizontalSize = extents.x / targetCamera.aspect;
            targetCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize) * padding;

            Vector3 viewDirection = -Vector3.forward;
            Vector3 position = center + viewDirection * (extents.z + 5f);
            targetCamera.transform.position = position;
            targetCamera.transform.LookAt(center);

            previewRenderUtility.lights[0].transform.localEulerAngles = new Vector3(10, 10, 0);
            previewRenderUtility.lights[0].intensity = 2;
        }

        public void Dispose()
        {
            if (previewRenderUtilities == null) return;
            foreach (PreviewRenderUtility utility in previewRenderUtilities)
            {
                utility.Cleanup();
            }
        }
    }
}