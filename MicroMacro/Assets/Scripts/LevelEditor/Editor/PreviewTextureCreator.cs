using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.LevelEditor
{
    /// <summary>
    /// GameObjectのプレビューテクスチャを生成するクラス
    /// </summary>
    public class PreviewTextureCreator
    {
        // フィールドとしての保持は不要になったため削除し、メソッド内で完結させます
        // private PreviewRenderUtility[] previewRenderUtilities;
        // private GameObject[] instances;

        public List<RenderTexture> CreatePreviewTextures(GameObject[] prefabs)
        {
            if (prefabs == null || prefabs.Length == 0)
                return new List<RenderTexture>();

            prefabs = GetNotNullPrefabs(prefabs);
            var textures = new List<RenderTexture>();

            // 1. Utilityはループの外で1つだけ作成して使い回す
            var renderUtility = new PreviewRenderUtility(true);

            // カメラ設定（初期設定）
            renderUtility.camera.fieldOfView = 30;

            try
            {
                foreach (GameObject prefab in prefabs)
                {
                    // 2. プレビュー用のインスタンスを生成
                    GameObject instance = Object.Instantiate(prefab);

                    // Utilityに追加
                    renderUtility.AddSingleGO(instance);

                    // 複数Rendererのバウンディングを結合して扱う
                    Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();

                    // レンダラーがない場合はスキップ等の処理が必要かもしれませんが、
                    // ここではとりあえずカメラ調整を実行
                    SetupUtility(renderUtility, renderers);

                    // 3. レンダリング実行
                    renderUtility.BeginPreview(new Rect(0, 0, 128, 128), GUIStyle.none);
                    renderUtility.camera.Render();
                    RenderTexture resultParams = (RenderTexture)renderUtility.EndPreview();

                    // 4. 重要: 結果を新しいRenderTextureにコピーして保存する
                    // (Utilityを使い回すとresultParamsの中身が次で上書きされるか、Cleanupで消えるため)
                    var newTexture = new RenderTexture(128, 128, 24);
                    // 名前をつけておくとデバッグしやすい
                    newTexture.name = $"Preview_{prefab.name}";

                    // GPU上でテクスチャをコピー
                    Graphics.Blit(resultParams, newTexture);

                    textures.Add(newTexture);

                    // 5. 次のループのためにインスタンスを即座に破棄してシーンを空にする
                    Object.DestroyImmediate(instance);
                }
            }
            finally
            {
                // 6. 最後に必ずクリーンアップ
                renderUtility.Cleanup();
            }

            return textures;
        }

        private GameObject[] GetNotNullPrefabs(GameObject[] prefabs)
        {
            return prefabs.Where(prefab => prefab != null).ToArray();
        }

        // 複数RendererのBoundsをまとめてカメラやライトの調整を実施
        private void SetupUtility(PreviewRenderUtility previewRenderUtility, Renderer[] targetRenderers)
        {
            Camera targetCamera = previewRenderUtility.camera;

            // 重要: カメラ設定をリセット（前のループの設定が残るのを防ぐ）
            targetCamera.transform.position = Vector3.zero;
            targetCamera.transform.rotation = Quaternion.identity;

            if (targetCamera == null || targetRenderers == null || targetRenderers.Length == 0)
            {
                // レンダラーがない場合の安全策
                targetCamera.transform.position = new Vector3(0, 0, -10);
                targetCamera.transform.LookAt(Vector3.zero);
                return;
            }

            // --- 1. Boundsの計算 ---
            Bounds bounds = targetRenderers[0].bounds;
            for (int i = 1; i < targetRenderers.Length; i++)
            {
                bounds.Encapsulate(targetRenderers[i].bounds);
            }

            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
            float objectSize = extents.magnitude; // オブジェクトの全体サイズ感

            // --- 2. カメラのクリッピングプレーン（描画範囲）の調整 ---
            // ★ここが修正ポイント: オブジェクトの大きさに合わせて描画限界を広げる
            targetCamera.nearClipPlane = 0.01f;
            targetCamera.farClipPlane = objectSize * 20f + 1000f; // 十分な奥行きを確保

            // --- 3. カメラ位置とサイズの調整 ---
            targetCamera.orthographic = true;

            float padding = 1.5f; // 余白
            float verticalSize = extents.y;
            float horizontalSize = extents.x / targetCamera.aspect;
            targetCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize) * padding;

            // カメラを斜め上など「見やすい位置」ではなく、真正面から引いた位置に置く場合
            // Boundsの奥行き(z) + 少し離れた距離 に配置
            Vector3 viewDirection = -Vector3.forward;
            float distance = extents.z + objectSize * 2f + 5f; // 少し余裕を持って離す

            Vector3 position = center + viewDirection * distance;

            targetCamera.transform.position = position;
            targetCamera.transform.LookAt(center);

            // --- 4. ライトの調整 ---
            // 逆光で真っ黒にならないよう、ライトもカメラに合わせて調整するとベターです
            if (previewRenderUtility.lights.Length > 0)
            {
                Light l = previewRenderUtility.lights[0];
                l.transform.rotation = Quaternion.Euler(50, -30, 0); // 一般的な見やすい角度
                l.intensity = 1.0f;
            }
        }


        // クラス自体でDisposeを持つ必要がなくなったため削除可能ですが、
        // もしこのクラスがTextureの寿命管理もするなら、生成したRenderTextureの破棄処理が必要です。
        public void Dispose()
        {
            // 今回の修正でpreviewRenderUtilitiesフィールドは使わなくなったので
            // ここでのCleanup処理は不要になります。
            // 代わりに、生成した textures リストの中身をどこかで Release する責任が呼び出し元に発生します。
        }
    }
}