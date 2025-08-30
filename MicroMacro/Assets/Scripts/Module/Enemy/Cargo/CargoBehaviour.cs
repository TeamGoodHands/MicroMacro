using System;
using CoreModule.AI.HSM;
using Module.Scaling;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class CargoBehaviour : MonoBehaviour
    {
        [SerializeField] private CargoComponent component;
        private HierarchicalStateMachine stateMachine;

        private void Start()
        {
            stateMachine = new HierarchicalStateMachine();

            MoveState moveState = new MoveState(component);
            BackAttackState backAttackState = new BackAttackState(component);
            SleepingState sleepingState = new SleepingState(component);
            PrepareMoveState prepareMoveState = new PrepareMoveState(component);
            stateMachine.AddState(moveState);
            stateMachine.AddState(backAttackState);
            stateMachine.AddState(sleepingState);
            stateMachine.AddState(prepareMoveState);
            stateMachine.AddTransition<SleepingState, MoveState>(() => component.Condition.CurrentState == CargoCondition.State.Move);
            stateMachine.AddTransition<MoveState, BackAttackState>(() => component.Condition.CurrentState == CargoCondition.State.BackAttack);
            stateMachine.AddTransition<BackAttackState, PrepareMoveState>(() => component.Condition.CurrentState == CargoCondition.State.PrepareMove);
            stateMachine.AddTransition<PrepareMoveState, MoveState>(() => component.Condition.CurrentState == CargoCondition.State.Move);
            

            stateMachine.Start<SleepingState>();
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