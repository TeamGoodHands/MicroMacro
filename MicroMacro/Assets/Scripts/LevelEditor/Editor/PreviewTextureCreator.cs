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
            // カメラのセットアップ
            var camera = previewRenderUtility.camera;
            camera.fieldOfView = 30f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000;

            Bounds bounds = targetRenderer.bounds;
            Vector3 center = bounds.center;

            float fovVertical = camera.fieldOfView * Mathf.Deg2Rad;
            float aspect = camera.aspect;
            float fovHorizontal = 2f * Mathf.Atan(Mathf.Tan(fovVertical / 2f) * aspect);

            // バウンディングボックスのサイズから「横」「縦」の半径を取得
            Vector3 extents = bounds.extents;
            float radiusVertical = extents.y;
            float radiusHorizontal = extents.x;

            // 必要な距離を両方向で計算して大きい方を使う
            float distanceV = radiusVertical / Mathf.Tan(fovVertical / 2f);
            float distanceH = radiusHorizontal / Mathf.Tan(fovHorizontal / 2f);
            float distance = Mathf.Max(distanceV, distanceH) * 1.8f;

            Vector3 viewDirection = camera.transform.forward;
            Vector3 newPosition = center - viewDirection * distance;

            camera.transform.position = newPosition;
            camera.transform.LookAt(center);

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