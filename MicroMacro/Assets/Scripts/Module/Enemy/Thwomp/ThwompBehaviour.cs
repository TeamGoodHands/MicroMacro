using System;
using Constants;
using CoreModule.AI.BT;
using CoreModule.AI.HSM;
using UnityEngine;

namespace Module.Enemy.Thwomp
{
    public class ThwompBehaviour : MonoBehaviour
    {
        [SerializeField] private ThwompComponent component;

        private BehaviourTree controlTree;
        private HierarchicalStateMachine stateMachine;

        private void Start()
        {
            stateMachine = new HierarchicalStateMachine();
            stateMachine.AddState(new WaitState(component));
            stateMachine.AddState(new AscentState(component));
            stateMachine.AddState(new MoveState(component));
            stateMachine.AddState(new FallState(component));
            
            stateMachine.AddTransition<WaitState, AscentState>(() => component.Condition.CurrentState == ThwompCondition.State.Ascending);
            stateMachine.AddTransition<AscentState, MoveState>(() => component.Condition.CurrentState == ThwompCondition.State.Moving);
            stateMachine.AddTransition<MoveState, FallState>(() => component.Condition.CurrentState == ThwompCondition.State.Falling);
            stateMachine.AddTransition<FallState, WaitState>(() => component.Condition.CurrentState == ThwompCondition.State.Idle);
            
            stateMachine.Start<WaitState>();
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