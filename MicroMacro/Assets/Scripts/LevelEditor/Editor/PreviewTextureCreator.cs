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

        public RenderTexture[] CreatePreviewTextures(GameObject[] prefabs)
        {
            previewRenderUtilities = new PreviewRenderUtility[prefabs.Length];
            instances = new GameObject[prefabs.Length];

            for (int i = 0; i < prefabs.Length; i++)
            {
                var renderUtility = new PreviewRenderUtility(true);

                // プレビューのセットアップ
                previewRenderUtilities[i] = renderUtility;
                instances[i] = Object.Instantiate(prefabs[i]);
                SetupUtility(renderUtility, instances[i].GetComponentInChildren<Renderer>());

                // プレビュー対象のGameObjectを追加
                renderUtility.AddSingleGO(instances[i]);
            }

            var textures = new RenderTexture[prefabs.Length];

            for (int i = 0; i < prefabs.Length; i++)
            {
                // 対象オブジェクトをレンダリングしてテクスチャを取得
                previewRenderUtilities[i].camera.Render();
                RenderTexture texture = (RenderTexture)previewRenderUtilities[i].EndPreview();
                textures[i] = texture;
            }

            return textures;
        }

        private void SetupUtility(PreviewRenderUtility previewRenderUtility, Renderer targetRenderer)
        {
            Camera targetCamera = previewRenderUtility.camera;
            float padding = 2f; // カメラのパディング

            if (targetCamera == null || targetRenderer == null)
                return;

            targetCamera.orthographic = true;

            // Get bounds
            Bounds bounds = targetRenderer.bounds;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            // Calculate orthographic size
            float verticalSize = extents.y;
            float horizontalSize = extents.x / targetCamera.aspect;
            targetCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize) * padding;

            // Position camera
            Vector3 viewDirection = -Vector3.forward;
            Vector3 position = center + viewDirection * (extents.z + 5f);
            targetCamera.transform.position = position;
            targetCamera.transform.LookAt(center);

            // ライトののセットアップ
            previewRenderUtility.lights[0].transform.localEulerAngles = new Vector3(10, 10, 0);
            previewRenderUtility.lights[0].intensity = 4;

            previewRenderUtility.BeginPreview(new Rect(0, 0, 128, 128), GUIStyle.none);
        }

        public void Dispose()
        {
            foreach (PreviewRenderUtility utility in previewRenderUtilities)
            {
                utility.Cleanup();
            }
        }
    }
}