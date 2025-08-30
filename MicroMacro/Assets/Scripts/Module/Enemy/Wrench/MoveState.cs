using CoreModule.AI.HSM;

namespace Module.Enemy.Wrench
{
    public class MoveState :HierarchicalStateMachine.State
    {
        private readonly WrenchComponent component;

        public MoveState(WrenchComponent component)
        {
            this.component = component;
        }

        internal override void OnEnter()
        {
        }

        internal override void OnExit()
        {
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
