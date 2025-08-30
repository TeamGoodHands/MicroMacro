using System;
using CoreModule.AI.HSM;
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
            stateMachine.AddState(moveState);
            stateMachine.AddState(backAttackState);
            stateMachine.AddTransition<MoveState, BackAttackState>(() => component.Condition.CurrentState == CargoCondition.State.BackAttack);

            stateMachine.Start<MoveState>();
        }

        private void Update()
        {
            stateMachine.Update();
        }

        private void FixedUpdate()
        {
            stateMachine.UpdatePhysics();
        }
    }
}