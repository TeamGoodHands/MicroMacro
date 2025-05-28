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
                SetupUtility(renderUtility);

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

        private void SetupUtility(PreviewRenderUtility previewRenderUtility)
        {
            // カメラのセットアップ
            var camera = previewRenderUtility.camera;
            camera.fieldOfView = 30f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000;
            camera.transform.position = new Vector3(1.5f, 0.8f, -3f);
            camera.transform.LookAt(Vector3.zero);

            previewRenderUtility.BeginPreview(new Rect(0, 0, 128, 128), GUIStyle.none);

            // ライトののセットアップ
            previewRenderUtility.lights[0].transform.localEulerAngles = new Vector3(10, 10, 0);
            previewRenderUtility.lights[0].intensity = 4;
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