using CoreModule.AI.HSM;
using Module.Player.Component;
using PropertyGenerator.Generated;
using UnityEngine;

namespace Module.Player.State
{
    /// <summary>
    /// 何も操作できないステート
    /// </summary>
    public class LockState : HierarchicalStateMachine.State
    {
        private readonly Rigidbody rigidbody;
        private readonly PlayerControllerWrapper animatorWrapper;
        private readonly PlayerCondition condition;

        public LockState(PlayerComponent component)
        {
            rigidbody = component.Rigidbody;
            condition = component.Condition;
            animatorWrapper = component.AnimatorWrapper;
        }

        internal override void OnEnter()
        {
            rigidbody.linearVelocity = Vector2.zero;
            condition.ExternalForce = Vector2.zero;
            animatorWrapper.Speed = 0f;
            animatorWrapper.IsJumping = false;
            animatorWrapper.IsGround = true;
        }

        internal override void OnExit()
        {
            rigidbody.linearVelocity = Vector2.zero;
            condition.ExternalForce = Vector2.zero;
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