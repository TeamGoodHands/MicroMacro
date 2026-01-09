using System;
using CoreModule.AI.HSM;
using Module.Scaling;
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
        [SerializeField] private Scaler scaler;
        [SerializeField] private LockOnEffect lockOnEffect;
        [SerializeField] private Transform headTransform;

        private HierarchicalStateMachine stateMachine;

        private void Start()
        {
            stateMachine = new HierarchicalStateMachine();

            stateMachine.AddState(new AliveState());
            // stateMachine.AddState<MoveState, AliveState>(new MoveState(controller, parameter, condition));
            stateMachine.AddState<AttackState, AliveState>(new AttackState(parameter, headTransform, snakeHoseController, scaler));

            // stateMachine.AddTransition<MoveState, AttackState>(() => condition.CurrentState == SnakeHoseCondition.State.Attack);
            // stateMachine.AddTransition<AttackState, MoveState>(() => condition.CurrentState == SnakeHoseCondition.State.Move);

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