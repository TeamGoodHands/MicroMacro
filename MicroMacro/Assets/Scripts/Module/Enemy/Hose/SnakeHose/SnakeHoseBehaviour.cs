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
        [SerializeField] private SnakeHoseCondition condition;
        [SerializeField] private SnakeHoseParameter parameter;
        [SerializeField] private SnakeHoseComponents components;

        private HierarchicalStateMachine stateMachine;

        private void Start()
        {
            stateMachine = new HierarchicalStateMachine();

            stateMachine.AddState(new AliveState());
            stateMachine.AddState<AppearState, AliveState>(new AppearState(components, parameter, condition));
            stateMachine.AddState<WaterBallAttackState, AliveState>(new WaterBallAttackState(parameter, components, condition));
            stateMachine.AddState<BeamAttackState, AliveState>(new BeamAttackState(components, parameter, condition));

            stateMachine.AddTransition<AppearState, WaterBallAttackState>(() => condition.CurrentState == SnakeHoseCondition.State.WaterBallAttack);
            stateMachine.AddTransition<WaterBallAttackState, BeamAttackState>(() => condition.CurrentState == SnakeHoseCondition.State.BeamAttack);

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