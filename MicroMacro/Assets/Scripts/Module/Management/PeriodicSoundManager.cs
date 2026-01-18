using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Module.Management;
using UnityEngine;

namespace CoreModule.Utility
{
    public class PeriodicSoundManager : IDisposable
    {
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 定期的な音の再生を開始します。
        /// </summary>
        public void Play(string soundName, float intervalSeconds)
        {
            if (cancellationTokenSource != null)
                return;

            cancellationTokenSource = new CancellationTokenSource();
            PlayLoopAsync(soundName, intervalSeconds, cancellationTokenSource.Token).Forget();
        }

        /// <summary>
        /// 再生を停止します。
        /// </summary>
        public void Stop()
        {
            if (cancellationTokenSource == null)
                return;

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        private async UniTask PlayLoopAsync(string soundName, float intervalSeconds, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    SoundManager.instance.Play(soundName);

                    TimeSpan delayTimeSpan = TimeSpan.FromSeconds(intervalSeconds);
                    await UniTask.Delay(delayTimeSpan, cancellationToken: cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // キャンセル時は静かに処理を終えます。
            }
        }

        /// <summary>
        /// 指定された秒数の間だけ、定期的な音の再生を行います。
        /// </summary>
        public async UniTask PlayForDurationAsync(string soundName, float intervalSeconds, float durationSeconds)
        {
            if (durationSeconds <= 0f)
                return;

            Stop();

            var durationCts = new CancellationTokenSource();
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(durationCts.Token);

            // 指定秒数後にキャンセルを実行する
            durationCts.CancelAfterSlim(TimeSpan.FromSeconds(durationSeconds));

            await PlayLoopAsync(soundName, intervalSeconds, cancellationTokenSource.Token);

            Stop();
        }

        /// <summary>
        /// インスタンスが不要になった際にリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}