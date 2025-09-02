using System;
using CoreModule.AI.HSM;
using Module.Scaling;
using UnityEngine;
using UnityEngine.Assertions;

namespace Module.Enemy.Cargo
{
    public class CargoBehaviour : MonoBehaviour
    {
        [SerializeField] private CargoComponent component;
        private HierarchicalStateMachine stateMachine;

        private void Start()
        {
            CheckComponentReference();

            stateMachine = new HierarchicalStateMachine();

            // ステートの登録
            MoveState moveState = new MoveState(component);
            BackAttackState backAttackState = new BackAttackState(component);
            SleepingState sleepingState = new SleepingState(component);
            PrepareMoveState prepareMoveState = new PrepareMoveState(component);
            
            stateMachine.AddState(moveState);
            stateMachine.AddState(backAttackState);
            stateMachine.AddState(sleepingState);
            stateMachine.AddState(prepareMoveState);
            
            // 遷移の登録
            stateMachine.AddTransition<SleepingState, MoveState>(() => component.Condition.CurrentState == CargoCondition.State.Move);
            stateMachine.AddTransition<MoveState, BackAttackState>(() => component.Condition.CurrentState == CargoCondition.State.BackAttack);
            stateMachine.AddTransition<BackAttackState, PrepareMoveState>(() => component.Condition.CurrentState == CargoCondition.State.PrepareMove);
            stateMachine.AddTransition<PrepareMoveState, MoveState>(() => component.Condition.CurrentState == CargoCondition.State.Move);

            stateMachine.Start<SleepingState>();
        }

        private void CheckComponentReference()
        {
            Assert.IsNotNull(component.CineMachinePerlin, "component.CineMachinePerlin != null");
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