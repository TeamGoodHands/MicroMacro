using System;
using System.Collections.Generic;
using System.Linq;
using CoreModule.Helper;
using Cysharp.Threading.Tasks;
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
        [Header("落下させるオブジェクト")]
        [SerializeField] private List<ProjectileObject> objects = new();

        [Header("落下対象の地点")]
        [SerializeField] private List<Transform> fallTargets = new();

        [Header("落下開始の地点")]
        [SerializeField] private List<Transform> launchPoints = new();

        [Header("最初に吹き飛ばす際のばらつき (秒)")]
        [SerializeField, Min(0)] private float flightInterval = 2f;

        [Header("実際に落とす際のばらつき (秒)")]
        [SerializeField, Min(0)] private float fallInterval = 2f;

        [Header("物を何秒かけて落とすか")]
        [SerializeField, Min(0)] private float fallDuration = 2f;

        [Header("吹き飛ばしてから落下開始までの待機時間 (秒)")]
        [SerializeField, Min(0)] private float switchDelay = 2f;

        [Header("シーケンスを終了するまでの時間")]
        [SerializeField, Min(0)] private float finishSequenceDelay = 2f;

        private sealed class ProjectileObjectCache
        {
            public ProjectileObject Obj { get; }
            public Rigidbody Rb { get; }
            public Collider Col { get; }
            public FallObjectPlayerAttacker Attacker { get; }
            public Scaler Scaler { get; }
            public Vector3 DefaultPos { get; }

            public ProjectileObjectCache(ProjectileObject obj)
            {
                Obj = obj;
                Rb = obj.GetComponent<Rigidbody>();
                Col = obj.GetComponent<Collider>();
                Attacker = obj.GetComponent<FallObjectPlayerAttacker>();
                Scaler = obj.GetComponent<Scaler>();
                DefaultPos = obj.transform.position;
            }
        }

        private readonly List<ProjectileObjectCache> caches = new();
        private int[] fallPointPattern = Array.Empty<int>();

        private void Awake()
        {
            Assert.IsTrue(objects.Count >= fallTargets.Count, "objects.Count must be >= fallTargets.Count");

            // キャッシュ生成
            caches.AddRange(objects.Select(o => new ProjectileObjectCache(o)));

            // 落下地点の割り当て配列を確保
            fallPointPattern = new int[objects.Count];
        }

        /// <summary>
        /// 打ち上げ演出→落下攻撃の一連シーケンスを実行。
        /// </summary>
        public async UniTask PlayAsync()
        {
            GenerateFallPointPattern();

            // 打ち上げ演出
            LaunchAll();

            // 一旦停止し落下攻撃へ切り替え
            await UniTask.Delay(TimeSpan.FromSeconds(switchDelay));
            StopAll();

            // 落下攻撃
            await FallAllAsync();

            await UniTask.Delay(TimeSpan.FromSeconds(finishSequenceDelay));
        }

        /// <summary>
        /// 全オブジェクトを初期位置に戻してリセット。
        /// </summary>
        public void Collect()
        {
            foreach (var c in caches)
            {
                c.Obj.transform.SetPositionAndRotation(c.DefaultPos, Quaternion.identity);

                c.Rb.isKinematic = true;
                c.Col.enabled = true;
                c.Rb.linearVelocity = Vector3.zero;
                c.Rb.angularVelocity = Vector3.zero;

                c.Attacker.Reset();
                c.Scaler.ResetScale();
            }
        }

        private void LaunchAll()
        {
            float currentInterval = flightInterval;

            for (var i = 0; i < caches.Count; i++)
            {
                var cache = caches[i];
                var targetIdx = fallPointPattern[i];
                var targetPos = fallTargets[targetIdx].position;

                cache.Obj.Launch(targetPos, currentInterval, 0.8f);
                currentInterval += flightInterval;
            }
        }

        private async UniTask FallAllAsync()
        {
            for (var i = 0; i < caches.Count; i++)
            {
                var cache = caches[i];
                var targetIdx = fallPointPattern[i];
                var launchIdx = RandomLaunchIndex(targetIdx);

                // 位置をリセットして落下開始
                cache.Obj.transform.position = launchPoints[launchIdx].position;
                cache.Obj.Launch(fallTargets[targetIdx].position, fallDuration, 0.4f);

                await UniTask.Delay(TimeSpan.FromSeconds(fallInterval), cancellationToken: destroyCancellationToken);
            }
        }

        private void StopAll()
        {
            foreach (var c in caches)
            {
                c.Obj.Stop();
            }
        }

        private void GenerateFallPointPattern()
        {
            // 前半は必ずユニークに割り当てる
            for (var i = 0; i < fallTargets.Count; i++)
                fallPointPattern[i] = i;

            // 後半はランダムに重複可で割り当てる
            for (var i = fallTargets.Count; i < fallPointPattern.Length; i++)
                fallPointPattern[i] = Random.Range(0, fallTargets.Count);

            // 最終的に全体をシャッフル
            Shuffle(fallPointPattern);
        }

        private int RandomLaunchIndex(int targetIdx)
        {
            // 0 〜 launchPoints.Count-1 内で循環取得
            var offset = Random.Range(0, launchPoints.Count);
            return (targetIdx + offset) % launchPoints.Count;
        }

        private static void Shuffle<T>(IList<T> array)
        {
            for (var i = array.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}