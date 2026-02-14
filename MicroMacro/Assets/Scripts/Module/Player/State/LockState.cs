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
        private readonly PlayerParameter parameter;

        public LockState(PlayerComponent component)
        {
            rigidbody = component.Rigidbody;
            condition = component.Condition;
            animatorWrapper = component.AnimatorWrapper;
            parameter = component.Parameter;
        }

        internal override void OnEnter()
        {
            rigidbody.linearVelocity = Vector2.zero;
            condition.ExternalForce = Vector2.zero;
            animatorWrapper.Speed = 0f;
            animatorWrapper.IsJumping = false;
            animatorWrapper.IsGround = true;
            condition.Direction = condition.LastSideInput;
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
            if (rigidbody.isKinematic)
                return;
            
            Vector3 velocity = rigidbody.linearVelocity;
            velocity.y += parameter.GravityOnDown;
            rigidbody.linearVelocity = velocity;
        }

        internal override void Dispose()
        {
        }
    }
}