using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace Module.UI
{
    /// <summary>
    /// Image画像をパラパラ漫画のように切り替えるコンポーネント
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIFrameAnimator : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        
        // デフォルトで再生したい場合用（インスペクタで設定）
        [SerializeField] private Sprite[] defaultSprites;
        [SerializeField] private float fps = 3f; // 1秒間に切り替わる回数

        private CancellationTokenSource cts;
        private bool isPlaying = false;

        private void Awake()
        {
            if (targetImage == null) targetImage = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (defaultSprites != null && defaultSprites.Length > 0)
            {
                Play(defaultSprites, fps);
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        /// <summary>
        /// アニメーションを再生する（外部から呼ぶ）
        /// </summary>
        public void Play(Sprite[] sprites, float frameRate = -1)
        {
            // 同じアニメーション中なら何もしないチェックを入れても良いが、
            // ここでは切り替えを優先してリセットする
            Stop();

            if (sprites == null || sprites.Length == 0) return;

            float currentFps = (frameRate > 0) ? frameRate : fps;
            cts = new CancellationTokenSource();
            
            // 非同期ループ開始
            PlayLoopAsync(sprites, currentFps, cts.Token).Forget();
        }

        public void Stop()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            isPlaying = false;
        }

        private async UniTaskVoid PlayLoopAsync(Sprite[] sprites, float frameRate, CancellationToken token)
        {
            isPlaying = true;
            int index = 0;
            // FPSから待機時間を計算 (例: 3fps = 0.33秒待機)
            float waitTime = 1f / frameRate;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (targetImage != null)
                    {
                        targetImage.sprite = sprites[index];
                    }

                    // 次の画像へ（ループ）
                    index = (index + 1) % sprites.Length;

                    // 待機 (Time.timeScaleの影響を受けるようにDelayType.DeltaTimeを指定)
                    await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime), DelayType.DeltaTime, cancellationToken: token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 停止処理
            }
            finally
            {
                isPlaying = false;
            }
        }
    }
}