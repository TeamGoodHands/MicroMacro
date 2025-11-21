using CoreModule.AI.HSM;
using UnityEngine;

namespace Module.Player.State
{
    public class RideState : HierarchicalStateMachine.State
    {
        private readonly PlayerComponent component;

        public RideState(PlayerComponent component)
        {
            this.component = component;
        }

        internal override void OnEnter()
        {
            component.AnimatorWrapper.Speed = 0f;
            component.Rigidbody.isKinematic = true;
        }

        internal override void OnExit()
        {
            component.Rigidbody.isKinematic = false;
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