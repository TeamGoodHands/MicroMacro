using Constants;
using CoreModule.AI.HSM;
using UnityEngine;

namespace Module.Enemy.Thwomp
{
    public class MoveState : HierarchicalStateMachine.State
    {
        private readonly ThwompParameter parameter;
        private readonly ThwompCondition condition;
        private readonly Rigidbody rigidbody;
        private readonly Transform player;
        private float moveTargetX;
        private float startTime;

        public MoveState(ThwompComponent component)
        {
            this.parameter = component.Parameter;
            this.condition = component.Condition;
            this.rigidbody = component.Rigidbody;

            player = GameObject.FindWithTag(Tag.Player).transform;
        }

        internal override void OnEnter()
        {
            startTime = Time.time;
        }

        internal override void OnExit()
        {
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
            // 遅延
            if (startTime + parameter.MoveDelay > Time.time)
            {
                moveTargetX = player.position.x;
                return;
            }

            if (IsMoveCompleted())
            {
                condition.CurrentState = ThwompCondition.State.Falling;
                return;
            }

            PerformMove();
        }

        private void PerformMove()
        {
            Vector3 pos = rigidbody.position;
            pos.x = Mathf.MoveTowards(pos.x, moveTargetX, parameter.MoveSpeed * Time.deltaTime);
            rigidbody.position = pos;
        }

        private bool IsMoveCompleted()
        {
            return Mathf.Abs(rigidbody.position.x - moveTargetX) <= 0.01f;
        }

        internal override void Dispose()
        {
        }
    }
}