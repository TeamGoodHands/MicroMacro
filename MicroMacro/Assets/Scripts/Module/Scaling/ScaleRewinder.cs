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
            this.scaler = scaler;
            scaler.OnScaleCompleted += OnScaleCompleted;
        }

        private void OnScaleCompleted(ScaleEventArgs args)
        {
            // スケールが最小段階のときは巻き戻し不要
            if (args.State == State.MinScale)
                return;
            
            // 現在の巻き戻しをキャンセル
            rewindCanceller?.Cancel();
            rewindCanceller?.Dispose();
            rewindCanceller = new CancellationTokenSource();
            
            RewindAsync().Forget();
        }

        private async UniTaskVoid RewindAsync()
        {
            // 巻き戻しステップ数
            int stepCount = isPhasedRewind ? -1 : -scaler.MaxStep;

            // 遅延させる
            await UniTask.Delay(TimeSpan.FromSeconds(rewindDelay), cancellationToken: rewindCanceller.Token);

            scaler.Scale(stepCount).Forget();
        }
    }
}