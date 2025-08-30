using System;
using System.Collections.Generic;
using CoreModule.Helper;
using Cysharp.Threading.Tasks;
using Module.Scaling;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Module.Enemy.Cargo
{
    public class FallObjectsSequencer : MonoBehaviour
    {
        [SerializeField, Header("落下させるオブジェクト")] private List<ProjectileObject> objects;
        [SerializeField, Header("落下対象の地点")] private List<Transform> fallTargets;
        [SerializeField, Header("落下開始の地点")] private List<Transform> launchPoints;
        [SerializeField, Header("最初に吹き飛ばす際のばらつき")] private float fallIntervalOnStart = 2f;
        [SerializeField, Header("実際に落とす際のばらつき")] private float fallIntervalOnFallMode = 2f;
        [SerializeField, Header("物を何秒かけて落とすか")] private float fallTime = 2f;
        [SerializeField, Header("吹き飛ばしてから何秒後に落下開始するか")] private float fallModeSwitchDelay = 2f;

        private List<Vector3> defaultPositions;
        private int[] fallPointPattern;

        private void Start()
        {
            Assert.IsTrue(objects.Count >= fallTargets.Count, "objects.Count >= fallTargets.Count");
            fallPointPattern = new int[objects.Count];

            defaultPositions = new List<Vector3>();
            foreach (ProjectileObject obj in objects)
            {
                defaultPositions.Add(obj.transform.position);
            }
        }

        public async UniTask DoSequence()
        {
            GenerateFallPointPattern();

            // 最初に物を吹き飛ばす (演出用)
            float flightTime = fallIntervalOnStart;
            for (int i = 0; i < objects.Count; i++)
            {
                ProjectileObject obj = objects[i];
                int targetIndex = fallPointPattern[i];

                Vector3 targetPosition = fallTargets[targetIndex].position;
                obj.Launch(targetPosition, flightTime);

                flightTime += fallIntervalOnStart;
            }

            // 待機
            await UniTask.Delay(TimeSpan.FromSeconds(fallModeSwitchDelay));

            // プレイヤーから見えない地点に行ったら一旦止める
            foreach (ProjectileObject obj in objects)
            {
                obj.Stop();
            }

            // 実際に決められた地点から落下させる
            for (int i = 0; i < objects.Count; i++)
            {
                ProjectileObject obj = objects[i];

                // 開始地点はずらす
                int targetIndex = fallPointPattern[i];
                int launchIndex = (targetIndex + Random.Range(0, launchPoints.Count)) % launchPoints.Count;

                Vector3 targetPosition = fallTargets[targetIndex].position;
                Vector3 launchPosition = launchPoints[launchIndex].position;
                obj.transform.position = launchPosition;
                obj.Launch(targetPosition, fallTime);

                await UniTask.Delay(TimeSpan.FromSeconds(fallIntervalOnFallMode));
            }

            // 最後のオブジェクトの落下を待機
            await UniTask.Delay(TimeSpan.FromSeconds(fallIntervalOnFallMode));
        }

        public void Collect()
        {
            for (var i = 0; i < objects.Count; i++)
            {
                ProjectileObject obj = objects[i];
                obj.transform.position = defaultPositions[i];
                obj.transform.rotation = Quaternion.identity;

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                obj.GetComponent<FallObjectPlayerAttacker>().Reset();
                obj.GetComponent<Scaler>().ResetScale();
            }
        }

        private void GenerateFallPointPattern()
        {
            int index = 0;

            // どのポイントも必ず一回は選択されるようにする
            while (index < fallTargets.Count)
            {
                fallPointPattern[index] = index;
                index++;
            }

            // 残りはランダムで選択する
            while (index < objects.Count)
            {
                fallPointPattern[index] = Random.Range(0, fallTargets.Count);
                index++;
            }

            // シャッフル
            Shuffle(fallPointPattern);
        }

        private static void Shuffle<T>(T[] array)
        {
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}