using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Module.Enemy.Cargo
{
    public class FirstBlowEffector : MonoBehaviour
    {
        [Header("吹き飛ばすオブジェクト")] [SerializeField] private List<ProjectileObject> objects = new();

        [Header("落下対象の地点")] [SerializeField] private List<Transform> fallTargets = new();

        [Header("最初に吹き飛ばす際のばらつき (秒)")] [SerializeField, Min(0)] private float flightInterval = 2f;

        private int[] fallPointPattern;
        private List<ProjectileObjectCache> caches;

        private void Start()
        {
            // 落下地点の割り当て配列を確保
            fallPointPattern = new int[objects.Count];
            GenerateFallPointPattern(fallTargets.Count);

            caches = new List<ProjectileObjectCache>();
            foreach (ProjectileObject projectileObject in objects)
            {
                caches.Add(new ProjectileObjectCache(projectileObject));
            }
        }

        /// <summary>
        /// 全てのオブジェクトを上に吹き飛ばします
        /// </summary>
        public void Blow()
        {
            float currentInterval = flightInterval;

            for (var i = 0; i < objects.Count; i++)
            {
                ProjectileObject projectileObject = objects[i];
                int targetIdx = fallPointPattern[i];
                Vector3 targetPos = fallTargets[targetIdx].position;

                projectileObject.Launch(targetPos, currentInterval, 0.8f);
                currentInterval += flightInterval;
            }
        }

        public void StopBlow()
        {
            foreach (ProjectileObject projectileObject in objects)
            {
                projectileObject.Stop();
                projectileObject.gameObject.SetActive(false);
            }
        }

        public void ResetObjects()
        {
            foreach (var c in caches)
            {
                c.Obj.transform.SetPositionAndRotation(c.DefaultPos, Quaternion.identity);

                c.Rb.isKinematic = true;
                c.Col.enabled = true;

                c.Obj.gameObject.SetActive(true);
            }
        }

        private void GenerateFallPointPattern(int fallTargetsCount)
        {
            int uniqueCount = Mathf.Min(fallTargetsCount, fallPointPattern.Length);

            // 前半は必ずユニークに割り当てる
            for (var i = 0; i < uniqueCount; i++)
            {
                fallPointPattern[i] = i;
            }

            // 後半はランダムに重複可で割り当てる
            for (var i = uniqueCount; i < fallPointPattern.Length; i++)
            {
                fallPointPattern[i] = Random.Range(0, fallTargetsCount);
            }

            // 最終的に全体をシャッフル
            Shuffle(fallPointPattern);
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