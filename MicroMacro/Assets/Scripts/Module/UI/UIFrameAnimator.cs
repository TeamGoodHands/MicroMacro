using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace Module.UI
{
    [RequireComponent(typeof(Image))]
    public class UIFrameAnimator : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Sprite[] defaultSprites;
        [SerializeField] private float fps = 3f;

        private CancellationTokenSource cts;
        private bool isPlaying = false;
        
        // 現在再生中のスプライトリストを保持
        private Sprite[] currentSprites; 

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

        public void Play(Sprite[] sprites, float frameRate = -1)
        {
            if (sprites == null || sprites.Length == 0) return;

            //  既に同じアニメーションが再生中なら何もしない（リセット防止）
            if (isPlaying && currentSprites == sprites) 
            {
                // FPSだけ変更したい場合はここで処理してもいいが、今回は割愛
                return;
            }

            // 違うアニメなら停止して新しく再生
            Stop();

            currentSprites = sprites; // 記録
            
            float currentFps = (frameRate > 0) ? frameRate : fps;
            cts = new CancellationTokenSource();
            
            PlayLoopAsync(sprites, currentFps, cts.Token).Forget();
        }

        public void Stop()
        {
            // キャンセル命令
            cts?.Cancel();
            cts?.Dispose();
            cts = null;

            // 状態のリセット
            isPlaying = false;
            currentSprites = null;
        }

        private async UniTaskVoid PlayLoopAsync(Sprite[] sprites, float frameRate, CancellationToken token)
        {
            isPlaying = true;
            int index = 0;
            float waitTime = 1f / frameRate;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (targetImage != null)
                    {
                        targetImage.sprite = sprites[index];
                    }

                    index = (index + 1) % sprites.Length;
                    await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime), DelayType.DeltaTime, cancellationToken: token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // キャンセル時は何もしない
            }
            finally
            {
          
                // 「今動いているcts」が「自分のtoken」と一致する場合のみリセット
                // (Play()でStop()が呼ばれた場合、ctsは既にnullか新しいものになっているため、
                //  ここでリセット処理は走らず、新しいアニメーションの状態が守られる)
        
                if (cts != null && cts.Token == token)
                {
                    isPlaying = false;
                    currentSprites = null;
                    cts?.Dispose();
                    cts = null;
                }
            }
        }
    }
}