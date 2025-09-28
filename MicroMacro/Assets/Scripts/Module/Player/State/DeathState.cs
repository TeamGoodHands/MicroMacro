using CoreModule.AI.HSM;
using Module.Player.Component;
using PropertyGenerator.Generated;
using UnityEngine;

namespace Module.Player.State
{
    public class DeathState : HierarchicalStateMachine.State
    {
        private readonly Rigidbody rigidbody;
        private readonly PlayerCondition condition;
        private readonly PlayerControllerWrapper animatorWrapper;

        public DeathState(PlayerComponent component)
        {
            rigidbody = component.Rigidbody;
            condition = component.Condition;
            animatorWrapper = component.AnimatorWrapper;
        }

        internal override void OnEnter()
        {
            rigidbody.linearVelocity = Vector2.zero;
            rigidbody.isKinematic = true;
            condition.ExternalForce = Vector2.zero;
        }

        internal override void OnExit()
        {
            rigidbody.isKinematic = false;
            animatorWrapper.IsJumping = false;
            animatorWrapper.IsLanding = false;
            animatorWrapper.Speed = 0;
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