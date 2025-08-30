using System;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Enemy.Cargo
{
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
            perlin = component.CinemachineCamera
                .GetCinemachineComponent(CinemachineCore.Stage.Noise)
                .GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        internal override void OnEnter()
        {
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

            bool isStart = component.Condition.IsStart;
            Transform target = isStart ? component.Goal : component.Start;

            if (IsTargetReached(target))
            {
                SetPositionToTarget(target);
                SwitchToMove().Forget();
                doMove = false;
            }

            UpdatePosition();
            UpdateAnimator();
        }


        private void UpdatePosition()
        {
            Rigidbody moveParent = component.MoveParent;
            Vector3 velocity = new Vector3(0, 0, -component.Parameter.MoveSpeed);

            Vector3 position = moveParent.position;
            position += velocity;
            moveParent.position = position;

            // プレイヤーのRigidBodyも更新する
            player.MovePosition(player.position + velocity);
        }

        private void UpdateAnimator()
        {
            component.AnimatorWrapper.Direction = 0f;
        }

        private bool IsTargetReached(Transform target)
        {
            Vector3 targetPosition = target.transform.position;
            Vector3 position = component.MoveParent.position;
            Vector3 diff = targetPosition - position;
            float distance = Mathf.Abs(diff.z);

            return distance <= component.Parameter.MoveSpeed * 0.5f;
        }

        private void SetPositionToTarget(Transform target)
        {
            Vector3 position = component.MoveParent.position;
            position.z = target.position.z;
            component.MoveParent.position = position;
        }


        private async UniTaskVoid SwitchToMove()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: CancellationToken);
            component.Condition.CurrentState = CargoCondition.State.Move;
        }

        internal override void Dispose()
        {
        }
    }
}