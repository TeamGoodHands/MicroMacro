using System;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;

namespace Module.Enemy.Cargo
{
    /// <summary>
    /// 攻撃場所から移動場所に移動するステート
    /// </summary>
    public class PrepareMoveState : HierarchicalStateMachine.State
    {
        private readonly CargoComponent component;
        private readonly Rigidbody player;
        private readonly CinemachineBasicMultiChannelPerlin perlin;
        private bool doMove;

        public PrepareMoveState(CargoComponent component)
        {
            this.component = component;
            player = GameObject.FindWithTag(Tag.Player).GetComponent<Rigidbody>();
            perlin = component.CineMachinePerlin;
        }

        internal override void OnEnter()
        {
            perlin.NoiseProfile = component.MoveNoise;
            perlin.AmplitudeGain = component.Parameter.AmplitudeGainOnMove;
            perlin.FrequencyGain = component.Parameter.FrequencyGainOnMove;
            perlin.enabled = true;
            doMove = true;
        }

        internal override void OnExit()
        {
            perlin.enabled = false;
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
            if (!doMove)
                return;

            Transform target = GetMoveTarget();

            // 移動先に到達したら
            if (IsTargetReached(target))
            {
                // 移動先の座標に強制移動
                SetPositionToTarget(target);

                SwitchToMove().Forget();
            }

            UpdatePosition();
            UpdateAnimator();
        }


        private void UpdatePosition()
        {
            Rigidbody moveParent = component.MoveParent;
            Vector3 velocity = new Vector3(0, 0, -component.Parameter.MoveSpeed);

            // 移動速度を足す
            Vector3 position = moveParent.position;
            position += velocity;
            moveParent.position = position;

            // プレイヤーのRigidBodyも更新する
            player.MovePosition(player.position + velocity);
        }

        private void UpdateAnimator()
        {
            component.AnimatorWrapper.DirectionX = 0;
        }

        private bool IsTargetReached(Transform target)
        {
            Vector3 goal = target.transform.position;
            Vector3 start = component.MoveParent.position;

            // z座標上の距離
            float distance = Mathf.Abs((goal - start).z);

            // 現在の移動速度で1フレーム以内に到達する or 到達した距離内であれば到達したとみなす
            return distance <= component.Parameter.MoveSpeed * 0.5f;
        }

        private void SetPositionToTarget(Transform target)
        {
            Vector3 position = component.MoveParent.position;
            position.z = target.position.z;
            component.MoveParent.position = position;
        }

        private Transform GetMoveTarget()
        {
            bool isStart = component.Condition.IsStart;
            return isStart ? component.Goal : component.Start;
        }

        private async UniTaskVoid SwitchToMove()
        {
            // 移動停止
            doMove = false;

            // MoveStateに行くまで少し待つ
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: CancellationToken);

            component.Condition.SwitchState(CargoCondition.State.Move); 
        }

        internal override void Dispose()
        {
        }
    }
}