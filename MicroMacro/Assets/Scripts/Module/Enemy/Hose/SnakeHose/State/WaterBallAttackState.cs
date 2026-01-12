using System;
using System.Threading;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using Module.Scaling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Module.Enemy.Hose.SnakeHose
{
    public class WaterBallAttackState : HierarchicalStateMachine.State
    {
        private readonly SnakeHoseParameter parameter;
        private readonly Transform transform;
        private readonly Scaler scaler;
        private readonly AutoShuffleBag shootRandomBag = new AutoShuffleBag(0, 2);

        public WaterBallAttackState(SnakeHoseParameter parameter, Transform transform, Scaler scaler)
        {
            this.parameter = parameter;
            this.transform = transform;
            this.scaler = scaler;
        }

        internal override void OnEnter()
        {
            PatrolRandomlyAsync(parameter.AttackHeight, CancellationToken).Forget();
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