using System;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class BackAttackState : HierarchicalStateMachine.State
    {
        private readonly CargoComponent component;
        private readonly Rigidbody player;
        private readonly CinemachineBasicMultiChannelPerlin perlin;
        private bool doMoveBack;
        private Vector3 targetPosition;

        public BackAttackState(CargoComponent component)
        {
            this.component = component;

            player = GameObject.FindWithTag(Tag.Player).GetComponent<Rigidbody>();
            perlin = component.CinemachineCamera
                .GetCinemachineComponent(CinemachineCore.Stage.Noise)
                .GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        internal override void OnEnter()
        {
            BackAttack().Forget();
        }

        private async UniTaskVoid BackAttack()
        {
            float attackDelay = component.Parameter.AttackDelay;

            await UniTask.Delay(TimeSpan.FromSeconds(attackDelay), cancellationToken: CancellationToken);
            
            UpdateAnimator();

            doMoveBack = true;
            targetPosition = component.Transform.position + new Vector3(0, 0, component.Parameter.BackAttackDistanceZ);
            
            await UniTask.WaitUntil(() => doMoveBack == false, cancellationToken: CancellationToken);
            
            component.Condition.CurrentState = CargoCondition.State.Move;
        }

        internal override void OnExit()
        {
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
            if (!doMoveBack)
                return;
            
            MoveBack();

            if (IsTargetReached(targetPosition))
            {
                doMoveBack = false; 
            }
        }

        private void MoveBack()
        {
            Rigidbody moveParent = component.MoveParent;
            Vector3 velocity = new Vector3(0, 0, component.Parameter.AttackMoveSpeed);

            Vector3 position = moveParent.position;
            position += velocity;
            moveParent.position = position;

            // プレイヤーのRigidBodyも更新する
            player.MovePosition(player.position + velocity);
        }

        private bool IsTargetReached(Vector3 targetPosition)
        {
            Vector3 position = component.Transform.position;
            Vector3 diff = targetPosition - position;
            float distance = Mathf.Abs(diff.z);

            return distance <= component.Parameter.AttackMoveSpeed * 0.5f;
        }

        private void UpdateAnimator()
        {
            component.AnimatorWrapper.Direction = 0f;
        }

        internal override void Dispose()
        {
        }
    }
}