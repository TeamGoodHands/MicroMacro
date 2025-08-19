using CoreModule.AI.HSM;
using UnityEngine;

namespace Module.Enemy.Thwomp
{
    public class AscentState : HierarchicalStateMachine.State
    {
        private readonly ThwompParameter parameter;
        private readonly ThwompCondition condition;
        private readonly Rigidbody rigidbody;
        private float ascendTargetY;

        public AscentState(ThwompComponent component)
        {
            this.parameter = component.Parameter;
            this.condition = component.Condition;
            this.rigidbody = component.Rigidbody;
        }

        internal override void OnEnter()
        {
            ascendTargetY = rigidbody.position.y + parameter.MoveHeight;
        }

        internal override void OnExit()
        {
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
            // 上昇が完了したか判定
            if (IsAscentCompleted())
            {
                // 完了したらステートを変える
                condition.CurrentState = ThwompCondition.State.Moving;
                return;
            }

            // 上昇
            PerformAscent();
        }

        private void PerformAscent()
        {
            Vector3 pos = rigidbody.position;
            pos.y = Mathf.Lerp(pos.y, ascendTargetY, parameter.UpSpeed * Time.deltaTime);
            rigidbody.position = pos;
        }

        private bool IsAscentCompleted()
        {
            return Mathf.Abs(rigidbody.position.y - ascendTargetY) <= 0.01f;
        }

        internal override void Dispose()
        {
        }
    }
}