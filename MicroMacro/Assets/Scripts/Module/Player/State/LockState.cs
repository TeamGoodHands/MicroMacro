using CoreModule.AI.HSM;

namespace Module.Player.State
{
    /// <summary>
    /// 何も操作できないステート
    /// </summary>
    public class LockState :  HierarchicalStateMachine.State
    {
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