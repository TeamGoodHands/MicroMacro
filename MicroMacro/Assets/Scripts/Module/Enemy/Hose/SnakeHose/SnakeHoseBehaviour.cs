using System;
using CoreModule.AI.HSM;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace Module.Enemy.Hose.SnakeHose
{
    public class SnakeHoseBehaviour : MonoBehaviour
    {
        [SerializeField] private SnakeController controller;
        [SerializeField] private SnakeHoseCondition condition;
        [SerializeField] private SnakeHoseParameter parameter;
        [SerializeField] private SnakeHoseController snakeHoseController;
        [SerializeField] private PlayableDirector director;
        [SerializeField] private LockOnEffect lockOnEffect;

        private HierarchicalStateMachine stateMachine;

        private void Start()
        {
            stateMachine = new HierarchicalStateMachine();

            stateMachine.AddState(new AliveState(director, parameter));
            stateMachine.AddState<MoveState, AliveState>(new MoveState(controller, parameter, condition));
            stateMachine.AddState<AttackState, AliveState>(new AttackState(snakeHoseController, parameter, condition, lockOnEffect));

            stateMachine.AddTransition<MoveState, AttackState>(() => condition.CurrentState == SnakeHoseCondition.State.Attack);
            stateMachine.AddTransition<AttackState, MoveState>(() => condition.CurrentState == SnakeHoseCondition.State.Move);

            stateMachine.Start<AliveState>();
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