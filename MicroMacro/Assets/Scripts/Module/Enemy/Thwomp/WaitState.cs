using Constants;
using CoreModule.AI.HSM;
using Module.Scaling;
using UnityEngine;

namespace Module.Enemy.Thwomp
{
    public class WaitState : HierarchicalStateMachine.State
    {
        private readonly Transform transform;
        private readonly ThwompParameter parameter;
        private readonly ThwompCondition condition;
        private readonly TwoAxisScaler scaler;
        private readonly Transform player;
        private bool doResetScaler;

        public WaitState(ThwompComponent component)
        {
            transform = component.Rigidbody.transform;
            parameter = component.Parameter;
            condition = component.Condition;
            scaler = component.Scaler;

            player = GameObject.FindWithTag(Tag.Player).transform;
        }

        internal override void OnEnter()
        {
            doResetScaler = true;
        }

        internal override void OnExit()
        {
        }

        internal override void Update()
        {
            // 攻撃間隔を待つ
            if (condition.LastAttackTime + parameter.AttackIntervalTime >= Time.time)
                return;

            // スケーラーをリセットして無効化する
            if (doResetScaler)
            {
                scaler.ResetScale();
                doResetScaler = false;
            }

            // プレイヤーが一定距離まで近づき検知
            if (IsPlayerApproach())
            {
                condition.CurrentState = ThwompCondition.State.Ascending;
            }
        }

        private bool IsPlayerApproach()
        {
            float sqrDistance = (player.position - transform.position).sqrMagnitude;
            float sqrRange = parameter.DetectionRange * parameter.DetectionRange;

            return sqrDistance <= sqrRange;
        }

        internal override void UpdatePhysics()
        {
        }

        internal override void Dispose()
        {
        }
    }
}