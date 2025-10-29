using System;
using System.Collections.Generic;
using System.Linq;
using CoreModule.Helper;
using CoreModule.ObjectPool;
using CoreModule.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Scaling;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Module.Enemy.Cargo
{
    /// <summary>
    /// 複数オブジェクトを「打ち上げ演出 → 落下攻撃」させるシーケンサ。
    /// </summary>
    public sealed class FallObjectsSequencer : MonoBehaviour
    {
        [Header("落下対象の地点")] [SerializeField] private List<Transform> fallTargets = new();

        [Header("落下開始の地点")] [SerializeField] private List<Transform> launchPoints = new();

        [Header("落下開始地点まで移動する時間")] [SerializeField, Min(0)] private float setUpDuration = 1f;

        [Header("オブジェクトを揺らす時間")] [SerializeField, Min(0)] private float shakeDuration = 1.5f;

        [Header("物を何秒かけて落とすか")] [SerializeField, Min(0)] private float fallDuration = 2f;

        [Header("落下物が消えるまでの時間")] [SerializeField, Min(0)] private float disappearDuration = 3f;

        [Header("吹き飛ばしてから落下開始までの待機時間 (秒)")] [SerializeField, Min(0)] private float switchDelay = 2f;

        [Header("シーケンスを終了するまでの時間")] [SerializeField, Min(0)] private float finishSequenceDelay = 2f;

        [SerializeField] private FirstBlowEffector firstBlowEffector;

        private FallPattern fallPattern;
        private ObjectPool<ProjectileObjectCache> fallObjectPool;
        private bool isPlaying;

        public void SetPattern(FallPattern fallPattern)
        {
            this.fallPattern = fallPattern;
        }

        public void SetPool(ObjectPool<ProjectileObjectCache> pool)
        {
            fallObjectPool = pool;
        }

        /// <summary>
        /// 打ち上げ演出→落下攻撃の一連シーケンスを実行。
        /// </summary>
        public async UniTask PlayAsync()
        {
            firstBlowEffector.Blow();

            // 一旦停止し落下攻撃へ切り替え
            await UniTask.Delay(TimeSpan.FromSeconds(switchDelay), cancellationToken: destroyCancellationToken);

            // 吹き飛ばしたオブジェクトを一旦停止
            firstBlowEffector.StopBlow();

            // 落下攻撃
            isPlaying = true;
            await StartFallSequence();
            isPlaying = false;

            await UniTask.Delay(TimeSpan.FromSeconds(finishSequenceDelay), cancellationToken: destroyCancellationToken);
        }

        public void Collect()
        {
            firstBlowEffector.ResetObjects();
        }

        public void Cancel()
        {
            isPlaying = false;
        }

        private async UniTask StartFallSequence()
        {
            List<Wave> waves = fallPattern.GetWaves();

            // 時間順でソート
            waves.Sort((a, b) => a.Time.CompareTo(b.Time));

            float prevLaunchTime = 0f;

            foreach (Wave wave in waves)
            {
                // 1フレームのズレも起こさないように同じ予約時間ではDelayしない
                if (!Mathf.Approximately(prevLaunchTime, wave.Time))
                {
                    // 次の発射時間まで待機
                    TimeSpan delay = TimeSpan.FromSeconds(wave.Time - prevLaunchTime);

                    await UniTask.Delay(delay, cancellationToken: destroyCancellationToken);
                }

                // 再生を中止されていたら辞める
                if (!isPlaying)
                    return;

                prevLaunchTime = wave.Time;

                DoLaunch(wave).Forget();
            }
        }

        private async UniTaskVoid DoLaunch(Wave wave)
        {
            ProjectileObjectCache cache = fallObjectPool.GetOrCreate();
            cache.Obj.gameObject.SetActive(true);

            // 落下開始地点まで移動
            await MoveToLaunchPoint(wave, cache, launchPoints, setUpDuration);

            // 振動
            await DoShake(cache, shakeDuration);

            // 実際に落とす
            LaunchDown(wave, cache, fallTargets, fallDuration);

            await UniTask.Delay(TimeSpan.FromSeconds(disappearDuration), cancellationToken: destroyCancellationToken);

            fallObjectPool.Return(cache);
        }

        private async UniTask MoveToLaunchPoint(
            Wave wave,
            ProjectileObjectCache cache,
            List<Transform> launchPoints,
            float duration
        )
        {
            int launchIdx = wave.From;
            Vector3 launchPosition = launchPoints[launchIdx].position;

            // 落下開始位置のちょっと上にセット
            cache.Obj.transform.position = launchPosition + Vector3.up * 2f;

            // 落下開始位置まで動く
            _ = cache.Obj.transform.DOMove(launchPosition, duration);

            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: destroyCancellationToken);
        }

        private async UniTask DoShake(ProjectileObjectCache cache, float duration)
        {
            _ = cache.Obj.transform.DOShakePosition(duration, 0.1f, 30, 90, false, false);

            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: destroyCancellationToken);
        }

        private void LaunchDown(
            Wave wave,
            ProjectileObjectCache cache,
            List<Transform> fallTargets,
            float duration
        )
        {
            int targetIdx = wave.To;
            Assert.IsTrue(0 <= targetIdx && targetIdx < fallTargets.Count, "Invalid target index.");
            cache.Obj.Launch(fallTargets[targetIdx].position, duration, 0.4f);
        }
    }
}