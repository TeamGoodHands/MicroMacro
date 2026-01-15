using System;
using System.Threading;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using Module.Scaling;
using Module.UI;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Module.Enemy.Hose.SnakeHose
{
    public class WaterBallAttackState : HierarchicalStateMachine.State
    {
        private readonly SnakeHoseParameter parameter;
        private readonly SnakeHoseCondition condition;
        private readonly Transform transform;
        private readonly Transform bodyBone;
        private readonly Scaler scaler;
        private readonly HealthStatus healthStatus;
        private readonly AutoShuffleBag shootRandomBag = new AutoShuffleBag(0, 2);

        private CancellationTokenSource damageCanceller = new CancellationTokenSource();
        private int raptureIndex;

        public WaterBallAttackState(SnakeHoseParameter parameter, SnakeHoseComponents components, SnakeHoseCondition condition)
        {
            this.parameter = parameter;
            this.condition = condition;
            this.transform = components.HeadTransform;
            this.bodyBone = components.BodyTransform;
            healthStatus = components.HealthStatus;
            this.scaler = components.Scaler;
        }

        internal override void OnEnter()
        {
            CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, damageCanceller.Token);
            PatrolRandomlyAsync(parameter.AttackHeight, source.Token).Forget();

            scaler.OnScaleCompleted += HandleScale;
        }

        private void HandleScale(ScaleEventArgs args)
        {
            if (args.CurrentStep == scaler.MinStep)
            {
                ApplyDamage().Forget();
            }
        }

        /// <summary>
        /// キャンセルされるまでランダムな地点への移動を繰り返す非同期関数
        /// </summary>
        private async UniTask PatrolRandomlyAsync(Vector2[] waypoints, CancellationToken token)
        {
            // 配列の安全性を確認します
            // 早めのリターンには改行を入れ、波括弧は使いません
            if (waypoints == null)
                return;

            if (waypoints.Length == 0)
                return;

            // トークンがキャンセルされるまで無限ループを行います
            while (!token.IsCancellationRequested)
            {
                // varは使いません。型推論に頼らない硬派なスタイルです
                int targetIndex = shootRandomBag.Next();
                Vector2 targetPosition = waypoints[targetIndex];

                // 目的地に到達するまで移動処理を行います
                while (!token.IsCancellationRequested && Vector2.Distance(transform.localPosition, targetPosition) > 0.01f)
                {
                    float step = parameter.HoseMovementBaseSpeed * Time.deltaTime;
                    transform.localPosition = Vector2.MoveTowards(transform.localPosition, targetPosition, step);

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                if (token.IsCancellationRequested)
                    return;

                // 座標を正確に合わせます
                transform.localPosition = targetPosition;

                ShootWaterBall();

                // 到着後、少し休憩を挟みます（人間にも機械にも休息は必要です）
                float seconds = Mathf.Max(0f, parameter.HoseShootInterval - Mathf.Abs(scaler.CurrentStep) * parameter.HoseShootIntervalOffset);
                await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: token);
            }
        }

        private void ShootWaterBall()
        {
            GameObject waterBall = Object.Instantiate(parameter.WaterBallPrefab, parameter.ShootPivot.position, Quaternion.identity);
            Rigidbody rigidbody = waterBall.GetComponent<Rigidbody>();
            rigidbody.linearVelocity = -Vector2.right *
                                       (parameter.WaterBallShootPower * (Mathf.Abs(scaler.CurrentStep) * parameter.HoseMovementSpeedMultiplier));

            Transform childTransform = waterBall.transform.GetChild(0);
            Vector3 localScale = childTransform.localScale;
            localScale.x += Mathf.Abs(scaler.CurrentStep) * parameter.HoseMovementScaleOffset;
            localScale.y -= Mathf.Abs(scaler.CurrentStep) * parameter.HoseMovementScaleOffset;
            localScale.z -= Mathf.Abs(scaler.CurrentStep) * parameter.HoseMovementScaleOffset;
            childTransform.localScale = localScale;
        }

        private async UniTask ApplyDamage()
        {
            damageCanceller.Cancel();
            damageCanceller.Dispose();

            Vector3 bodyScale = bodyBone.localScale;
            await bodyBone.DOScale(bodyScale * 1.5f, parameter.DamageScaleDuration);

            Transform rapture = parameter.Raptures[raptureIndex++];
            Vector3 raptureScale = rapture.localScale;
            rapture.localScale = new Vector3(raptureScale.x, 0f, raptureScale.z);
            rapture.gameObject.SetActive(true);

            rapture.DOScale(raptureScale, 0.5f).SetEase(Ease.OutBack);
            bodyBone.localScale = bodyScale;
            scaler.SetScale(0, true);

            
            SoundManager.instance.Play("打撃6");
            
            await transform.DOShakePosition(1f, 0.1f, 30, 90, false, false);
            

            healthStatus.Damage(1);
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: CancellationToken);


            condition.CurrentState = SnakeHoseCondition.State.BeamAttack;
            return;

            if (raptureIndex >= parameter.Raptures.Length)
            {
                condition.CurrentState = SnakeHoseCondition.State.BeamAttack;
            }
            else
            {
                damageCanceller = new CancellationTokenSource();
                CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, damageCanceller.Token);
                PatrolRandomlyAsync(parameter.AttackHeight, source.Token).Forget();
            }
        }

        internal override void OnExit()
        {
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
        }

        internal override void Dispose()
        {
        }
    }
}