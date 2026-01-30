using System.Threading;
using Constants;
using Cysharp.Threading.Tasks;
using UnityEngine;

// CancellationTokenのために必要です

namespace Test
{
    public class TransparencySystem : MonoBehaviour
    {
        [SerializeField] private MeshRenderer targetMeshRenderer;

        [SerializeField, Tooltip("透明化にかかる時間（秒）")]
        private float fadeDuration = 0.5f;
        [SerializeField]
        private float fadeAlpha = 0.3f;

        private Color originalColor;
        private Color transparentColor;
        private CancellationTokenSource cancellationTokenSource;

        private int colorPropertyID = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (targetMeshRenderer == null)
                return;

            originalColor = targetMeshRenderer.material.GetColor(colorPropertyID);
            // アルファ値を0.5にして半透明の目標色を作成
            transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, fadeAlpha);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tag.Handle.Player))
                return;

            // 前の処理があればキャンセルし、新しいトークンを発行
            RefreshCancellationToken();
            // じわじわと透明色へ
            FadeTransparencyAsync(transparentColor, cancellationTokenSource.Token).Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(Tag.Handle.Player))
                return;

            // 前の処理があればキャンセルし、新しいトークンを発行
            RefreshCancellationToken();
            // じわじわと元の色へ
            FadeTransparencyAsync(originalColor, cancellationTokenSource.Token).Forget();
        }

        // キャンセレーショントークンを更新するメソッド
        private void RefreshCancellationToken()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            cancellationTokenSource = new CancellationTokenSource();
        }

        private async UniTaskVoid FadeTransparencyAsync(Color targetColor, CancellationToken token)
        {
            float elapsedTime = 0f;
            Color startColor = targetMeshRenderer.material.GetColor(colorPropertyID);

            while (elapsedTime < fadeDuration)
            {
                // 処理の途中でキャンセル要求が来ていないかチェック
                if (token.IsCancellationRequested)
                    return;

                elapsedTime += Time.deltaTime;
                // 時間経過に応じて、開始色から目標色へ徐々に変化（線形補間）
                float t = Mathf.Clamp01(elapsedTime / fadeDuration);
                targetMeshRenderer.material.SetColor(colorPropertyID, Color.Lerp(startColor, targetColor, t));

                // 次のフレームまで待機
                await UniTask.Yield();
            }

            // ループ終了後、念のため最終的な色を確実に設定（キャンセルされていなければ）
            if (!token.IsCancellationRequested)
                targetMeshRenderer.material.SetColor(colorPropertyID, targetColor);
        }

        // オブジェクトが破棄される際にも確実にキャンセル処理を行う
        private void OnDestroy()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
        }
    }
}