using System;
using System.Threading;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using Module.Player.Component;
using PropertyGenerator.Generated;
using UnityEngine;

namespace Module.Player.State
{
    public class DeathState : HierarchicalStateMachine.State
    {
        private readonly PlayerParameter parameter;
        private readonly Rigidbody rigidbody;
        private readonly SkinnedMeshRenderer meshRenderer;
        private readonly PlayerCondition condition;
        private readonly PlayerControllerWrapper animatorWrapper;

        public DeathState(PlayerComponent component, PlayerParameter parameter)
        {
            this.parameter = parameter;
            rigidbody = component.Rigidbody;
            meshRenderer = component.MeshRenderer;
            condition = component.Condition;
            animatorWrapper = component.AnimatorWrapper;
        }

        internal override void OnEnter()
        {
            rigidbody.linearVelocity = Vector2.zero;
            rigidbody.isKinematic = true;
            condition.ExternalForce = Vector2.zero;

            animatorWrapper.IsDeath = true;
        }

        internal override void OnExit()
        {
            rigidbody.isKinematic = false;
            animatorWrapper.IsJumping = false;
            animatorWrapper.Speed = 0;

            animatorWrapper.IsDeath = false;
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