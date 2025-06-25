using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Module.Scaling
{
    /// <summary>
    /// オブジェクトのスケールの巻き戻しを行うクラス
    /// </summary>
    [Serializable]
    public class ScaleRewinder
    {
        [SerializeField, Header("巻き戻し開始までの時間")] private float rewindDelay = 1f;
        [SerializeField, Header("段階的に小さくするか")] private bool isPhasedRewind = false;

        private Scaler scaler;
        private CancellationTokenSource rewindCanceller;

        public void Schedule(Scaler scaler)
        {
            if (scaler == null)
            {
                throw new ArgumentNullException(nameof(scaler), "Scaler cannot be null.");
            }

            this.scaler = scaler;
            scaler.OnScaleCompleted += OnScaleCompleted;
        }

        public void Dispose()
        {
            if (scaler != null)
            {
                scaler.OnScaleCompleted -= OnScaleCompleted;
            }

            rewindCanceller?.Cancel();
            rewindCanceller?.Dispose();
            rewindCanceller = null;
        }

        private void OnScaleCompleted(ScaleEventArgs args)
        {
            // スケールが0のときは巻き戻し不要
            if (args.CurrentStep == 0)
                return;

            // 現在の巻き戻しをキャンセル
            rewindCanceller?.Cancel();
            rewindCanceller?.Dispose();
            rewindCanceller = new CancellationTokenSource();

            RewindAsync().Forget();
        }

        private async UniTaskVoid RewindAsync()
        {
            // 遅延させる
            await UniTask.Delay(TimeSpan.FromSeconds(rewindDelay), cancellationToken: rewindCanceller.Token);

            // 巻き戻し量
            int rewindAmount = scaler.CurrentStep > 0 ? -1 : 1;

            // 巻き戻しステップ数
            int stepCount = isPhasedRewind ? rewindAmount : -scaler.MaxStep;

            scaler.Scale(stepCount).Forget();
        }
    }
}