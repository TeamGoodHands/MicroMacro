using Constants;
using CoreModule.AI.HSM;
using UnityEngine;

namespace Module.Enemy.Thwomp
{
    public class WaitState : HierarchicalStateMachine.State
    {
        private readonly Transform transform;
        private readonly ThwompParameter parameter;
        private readonly ThwompCondition condition;
        private readonly Transform player;

        public WaitState(ThwompComponent component)
        {
            transform = component.Rigidbody.transform;
            parameter = component.Parameter;
            condition = component.Condition;
            
            player = GameObject.FindWithTag(Tag.Player).transform;
        }

        internal override void OnEnter()
        {
        }

        internal override void OnExit()
        {
        }

        internal override void Update()
        {
            // 攻撃間隔を待つ
            if (condition.LastAttackTime + parameter.AttackIntervalTime >= Time.time)
                return;
            
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