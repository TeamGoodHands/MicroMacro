using CoreModule.AI.HSM;
using UnityEngine;
using UnityEngine.Assertions;

namespace Module.Enemy.Cargo
{
    public class CargoBehaviour : MonoBehaviour
    {
        [SerializeField] private CargoComponent component;
        private HierarchicalStateMachine stateMachine;
        
        public CargoCondition Condition => component.Condition;

        private void Start()
        {
            CheckComponentReference();

            stateMachine = new HierarchicalStateMachine();

            // ステートの登録
            MoveState moveState = new MoveState(component);
            BackAttackState backAttackState = new BackAttackState(component);
            SleepingState sleepingState = new SleepingState(component);
            PrepareMoveState prepareMoveState = new PrepareMoveState(component);
            DeathState deathState = new DeathState(component);

            stateMachine.AddState(moveState);
            stateMachine.AddState(backAttackState);
            stateMachine.AddState(sleepingState);
            stateMachine.AddState(prepareMoveState);
            stateMachine.AddState(deathState);

            // 遷移の登録
            stateMachine.AddTransition<SleepingState, BackAttackState>(() =>
                component.Condition.PreviousState == CargoCondition.State.Sleeping &&
                component.Condition.CurrentState == CargoCondition.State.BackAttack);
            stateMachine.AddTransition<MoveState, BackAttackState>(() => component.Condition.CurrentState == CargoCondition.State.BackAttack);
            stateMachine.AddTransition<BackAttackState, PrepareMoveState>(() => component.Condition.CurrentState == CargoCondition.State.PrepareMove);
            stateMachine.AddTransition<PrepareMoveState, MoveState>(() => component.Condition.CurrentState == CargoCondition.State.Move);
            stateMachine.AddTransition<BackAttackState, DeathState>(() => component.Condition.CurrentState == CargoCondition.State.Death);
            
            stateMachine.Start<SleepingState>();
        }

        private void CheckComponentReference()
        {
            Assert.IsNotNull(component.CineMachinePerlin, "component.CineMachinePerlin != null");
            Assert.IsNotNull(component.MoveParent, "component.MoveParent != null");
            Assert.IsNotNull(component.Start, "component.Start != null");
            Assert.IsNotNull(component.Goal, "component.Goal != null");
        }

        private void Update()
        {
            stateMachine.Update();
        }

        private void LateUpdate()
        {
            stateMachine.LateUpdate();
        }

        private void FixedUpdate()
        {
            stateMachine.UpdatePhysics();
        }
    }
}