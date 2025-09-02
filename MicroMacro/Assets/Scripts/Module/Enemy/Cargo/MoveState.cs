using Constants;
using CoreModule.AI.HSM;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    /// <summary>
    /// ボスが左右に移動するステート
    /// </summary>
    public class MoveState : HierarchicalStateMachine.State
    {
        private readonly CargoComponent component;
        private readonly Rigidbody player;
        private readonly CinemachineBasicMultiChannelPerlin perlin;

        public MoveState(CargoComponent component)
        {
            this.component = component;
            player = GameObject.FindWithTag(Tag.Player).GetComponent<Rigidbody>();
            perlin = component.CineMachinePerlin;
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
            Transform target = GetMoveTarget();

            // 移動先に到達したら
            if (IsTargetReached(target))
            {
                // 移動先の座標に強制移動
                SetPositionToTarget(target);
                
                // ステートを進める
                component.Condition.CurrentState = CargoCondition.State.BackAttack;
            }
            
            // x軸上の移動方向を取得
            float xDirection = component.MoveParent.position.x < target.position.x ? 1f : -1f;

            UpdatePosition(xDirection);
            UpdateAnimator(xDirection);
        }

        private void UpdatePosition(float direction)
        {
            Rigidbody moveParent = component.MoveParent;
            Vector3 velocity = new Vector3(direction * component.Parameter.MoveSpeed, 0, 0);

            // 移動速度を足す
            Vector3 position = moveParent.position;
            position += velocity;
            moveParent.position = position;

            // プレイヤーのRigidBodyも更新する
            player.MovePosition(player.position + velocity);
        }

        private Transform GetMoveTarget()
        {
            bool isStart = component.Condition.IsStart;
            return isStart ? component.Goal : component.Start;
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
            Vector3 goal = target.transform.position;
            Vector3 start = component.MoveParent.position;

            // x座標上の距離
            float distance = Mathf.Abs((goal - start).x);

            // 現在の移動速度で1フレーム以内に到達する or 到達した距離内であれば到達したとみなす
            return distance <= component.Parameter.MoveSpeed * 0.5f;
        }

        private void SetPositionToTarget(Transform target)
        {
            Vector3 position = component.MoveParent.position;
            position.x = target.position.x;
            component.MoveParent.position = position;
        }

        internal override void Dispose()
        {
        }
    }
}