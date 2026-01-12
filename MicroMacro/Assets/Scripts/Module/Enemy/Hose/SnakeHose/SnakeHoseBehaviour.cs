using System;
using CoreModule.AI.HSM;
using Module.Enemy.Hose.SnakeHose.State;
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
        [SerializeField] private Animator animator;

        private HierarchicalStateMachine stateMachine;

        private void Start()
        {
            stateMachine = new HierarchicalStateMachine();

            stateMachine.AddState(new AliveState());
            stateMachine.AddState<AppearState, AliveState>(new AppearState(animator, parameter, condition));
            stateMachine.AddState<WaterBallAttackState, AliveState>(new WaterBallAttackState(parameter, headTransform, scaler));

            stateMachine.AddTransition<AppearState, WaterBallAttackState>(() => condition.CurrentState == SnakeHoseCondition.State.WaterBallAttack);
            // stateMachine.AddTransition<MoveState, AppearState>(() => condition.CurrentState == SnakeHoseCondition.State.WaterBallAttack);

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

        private void OnDestroy()
        {
            stateMachine?.Dispose();
        }
    }
}