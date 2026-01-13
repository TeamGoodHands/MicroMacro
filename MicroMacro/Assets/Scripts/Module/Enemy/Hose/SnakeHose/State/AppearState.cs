using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using UnityEditor.Animations;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose.State
{
    public class AppearState : HierarchicalStateMachine.State
    {
        private readonly Animator animator;
        private readonly SnakeHoseParameter parameter;
        private readonly SnakeHoseCondition condition;

        public AppearState(SnakeHoseComponents components, SnakeHoseParameter parameter, SnakeHoseCondition condition)
        {
            this.animator = components.Animator;
            this.parameter = parameter;
            this.condition = condition;
        }

        internal override void OnEnter()
        {
            WaitToNextStateAsync();
        }

        internal override void OnExit()
        {
            animator.enabled = false;
        }

        private async void WaitToNextStateAsync()
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(parameter.AppearDuration), cancellationToken: CancellationToken);

            condition.CurrentState = SnakeHoseCondition.State.WaterBallAttack;
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
        }

        internal override void Dispose()
        {
        }
    }
}