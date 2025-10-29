using System;
using System.Collections.Generic;
using CoreModule.ObjectPool;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Enemy.Cargo;
using Module.Enemy.ThornBallModule;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Module.Enemy.Spanner
{
    public class ThornBalls : MonoBehaviour
    {
        [SerializeField] private GameObject thornBallPrefab;
        [SerializeField] private GameObject bigThornBallPrefab;
        [SerializeField] private Transform spawnArea;
        [SerializeField] private List<ThornBallAttackPattern> attackPatterns;
        [SerializeField] private Vector2 initialDirection;
        [SerializeField] private Vector3 initialAngularVelocity;
        [SerializeField] private CargoBehaviour cargoBehaviour;

        private ObjectPool<ThornBall> thornBallPool;
        private ObjectPool<ThornBall> bigThornBallPool;
        private HashSet<ThornBall> thornBalls;
        private int attackCount;

        private void Start()
        {
            thornBallPool = new ObjectPool<ThornBall>(
                () =>
                {
                    GameObject obj = Instantiate(thornBallPrefab, transform);
                    obj.SetActive(false);
                    return obj.GetComponent<ThornBall>();
                },
                item => item.Reset());

            bigThornBallPool = new ObjectPool<ThornBall>(
                () =>
                {
                    GameObject obj = Instantiate(bigThornBallPrefab, transform);
                    obj.SetActive(false);
                    return obj.GetComponent<ThornBall>();
                },
                item => item.Reset());

            thornBalls = new HashSet<ThornBall>();

            cargoBehaviour.Condition.OnStateChanged += HandleStateChanged;
        }

        private void HandleStateChanged(CargoCondition.State state)
        {
            if (state == CargoCondition.State.Move)
            {
                StartAttackSequence(cargoBehaviour.Condition.IsStart).Forget();
            }
        }

        private async UniTaskVoid StartAttackSequence(bool isLeft)
        {
            ThornBallAttackPattern attackPattern = attackPatterns[0];
            if (attackCount > 0)
            {
                attackPattern = attackPatterns[Random.Range(1, attackPatterns.Count)];
            }

            // 時間順でソート
            var patterns = attackPattern.GetPatterns();
            patterns.Sort((a, b) => a.Time.CompareTo(b.Time));

            float direction = isLeft ? 1f : -1f;
            Vector3 startPosition = CalculateStartPosition(isLeft);
            Vector3 positionUnit = CalculatePositionUnit(attackPattern, isLeft);
            float prevLaunchTime = 0f;

            foreach (ThornBallAttackPattern.Pattern pattern in patterns)
            {
                // 1フレームのズレも起こさないように同じ予約時間ではDelayしない
                if (!Mathf.Approximately(prevLaunchTime, pattern.Time))
                {
                    // 次の発射時間まで待機
                    TimeSpan delay = TimeSpan.FromSeconds(pattern.Time - prevLaunchTime);

                    await UniTask.Delay(delay, cancellationToken: destroyCancellationToken);
                }

                prevLaunchTime = pattern.Time;
                Vector3 position = startPosition + positionUnit * pattern.Position;

                DoAttack(pattern, direction, position);
            }

            attackCount++;
        }

        private Vector3 CalculateStartPosition(bool isLeft)
        {
            Vector3 startPosition = spawnArea.position;
            startPosition.x += isLeft ? spawnArea.localScale.x * 0.5f : -spawnArea.localScale.x * 0.5f;

            return startPosition;
        }

        private Vector3 CalculatePositionUnit(ThornBallAttackPattern pattern, bool isLeft)
        {
            float positionUnit = spawnArea.localScale.x / pattern.GetAreaDivide();
            positionUnit *= isLeft ? -1f : 1f;

            return new Vector3(positionUnit, 0f, 0f);
        }

        private void DoAttack(ThornBallAttackPattern.Pattern pattern, float direction, Vector3 position)
        {
            ThornBall thornBall = pattern.IsBig ? bigThornBallPool.GetOrCreate() : thornBallPool.GetOrCreate();
            Vector2 movementDirection = initialDirection;
            movementDirection.x *= direction;

            thornBall.gameObject.SetActive(true);
            thornBall.transform.position = position;
            thornBall.PerformLinearMovement(movementDirection);

            thornBalls.Add(thornBall);
            thornBall.OnDeath += HandleOnDeath;
        }

        private async void HandleOnDeath(ThornBall thornBall)
        {
            thornBall.OnDeath -= HandleOnDeath;
            thornBalls.Remove(thornBall);

            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: destroyCancellationToken);

            thornBall.gameObject.SetActive(false);

            if (thornBall.IsBig)
            {
                bigThornBallPool.Return(thornBall);
            }
            else
            {
                thornBallPool.Return(thornBall);
            }
        }

        private void FixedUpdate()
        {
            foreach (ThornBall thornBall in thornBalls)
            {
                thornBall.SetMoveDelta(cargoBehaviour.Condition.MoveDelta);
            }
        }

        private void OnDestroy()
        {
            if (cargoBehaviour != null && cargoBehaviour.Condition != null)
            {
                cargoBehaviour.Condition.OnStateChanged -= HandleStateChanged;
            }
        }
    }
}