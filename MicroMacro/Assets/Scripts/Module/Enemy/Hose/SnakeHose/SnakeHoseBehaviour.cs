using System;
using CoreModule.AI.HSM;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose
{
    public class SnakeHoseBehaviour : MonoBehaviour
    {
        [SerializeField] private SnakeController controller;
        [SerializeField] private SnakeHoseCondition condition;
        [SerializeField] private SnakeHoseParameter parameter;
        [SerializeField] private SnakeHoseTapBehaviour tapBehaviour;

        private HierarchicalStateMachine stateMachine;

        private void Start()
        {
            stateMachine = new HierarchicalStateMachine();

            stateMachine.AddState(new MoveState(controller, parameter, condition));
            stateMachine.AddState(new AttackState(tapBehaviour,parameter,condition));

            stateMachine.AddTransition<MoveState, AttackState>(() => condition.CurrentState == SnakeHoseCondition.State.Attack);
            stateMachine.AddTransition<AttackState, MoveState>(() => condition.CurrentState == SnakeHoseCondition.State.Move);

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