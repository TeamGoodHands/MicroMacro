using Constants;
using CoreModule.AI.HSM;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class MoveState : HierarchicalStateMachine.State
    {
        private readonly CargoComponent component;
        private readonly Rigidbody player;
        private readonly CinemachineBasicMultiChannelPerlin perlin;

        public MoveState(CargoComponent component)
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
        }

        internal override void OnExit()
        {
            perlin.enabled = false;
            component.Condition.IsStart = !component.Condition.IsStart;
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
            bool isStart = component.Condition.IsStart;
            Transform target = isStart ? component.Goal : component.Start;
            float direction = component.MoveParent.position.x < target.position.x ? 1f : -1f;

            if (IsTargetReached(target))
            {
                SetPositionToTarget(target);
                component.Condition.CurrentState = CargoCondition.State.BackAttack;
            }

            UpdatePosition(direction);
            UpdateAnimator(direction);
        }

        private void UpdatePosition(float direction)
        {
            Rigidbody moveParent = component.MoveParent;
            Vector3 velocity = new Vector3(direction * component.Parameter.MoveSpeed, 0, 0);

            Vector3 position = moveParent.position;
            position += velocity;
            moveParent.position = position;

            // プレイヤーのRigidBodyも更新する
            player.MovePosition(player.position + velocity);
        }

        private void UpdateAnimator(float direction)
        {
            if (direction > 0)
            {
                component.AnimatorWrapper.Direction = -1f;
            }
            else if (direction < 0)
            {
                component.AnimatorWrapper.Direction = 1f;
            }
            else
            {
                component.AnimatorWrapper.Direction = 0f;
            }
        }

        private bool IsTargetReached(Transform target)
        {
            Vector3 targetPosition = target.transform.position;
            Vector3 position = component.MoveParent.position;
            Vector3 diff = targetPosition - position;
            float distance = Mathf.Abs(diff.x);

            return distance <= component.Parameter.MoveSpeed * 0.5f;
        }

        private void SetPositionToTarget(Transform target)
        {
            Vector3 position = component.MoveParent.position;
            position.x = target.position.x;
            component.MoveParent.position = position;
        }

        private void ShakeCamera()
        {
        }

        internal override void Dispose()
        {
        }
    }
}