using System;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose
{
    public class AttackState : HierarchicalStateMachine.State
    {
        private readonly SnakeHoseTapBehaviour tapBehaviour;
        private readonly SnakeHoseParameter parameter;
        private readonly SnakeHoseCondition condition;
        private readonly LockOnEffect lockOnEffect;

        public AttackState(SnakeHoseTapBehaviour tapBehaviour, SnakeHoseParameter parameter, SnakeHoseCondition condition, LockOnEffect lockOnEffect)
        {
            this.tapBehaviour = tapBehaviour;
            this.parameter = parameter;
            this.condition = condition;
            this.lockOnEffect = lockOnEffect;
        }

        internal override void OnEnter()
        {
            AttackSequence().Forget();
        }

        private async UniTaskVoid AttackSequence()
        {
            lockOnEffect.LockOn();

            await tapBehaviour.LookAtPlayerSmoothAsync(parameter.TimeToFacePlayer, 4f);

            await tapBehaviour.ShakeBody(parameter.ShakeTime);

            lockOnEffect.LockOff();

            await tapBehaviour.OnWater();

            await UniTask.Delay(TimeSpan.FromSeconds(parameter.AttackDuration));

            await tapBehaviour.OffWater();

            await tapBehaviour.ResetAngle(parameter.TimeToResetAngle);

            condition.CurrentState = SnakeHoseCondition.State.Move;
        }

        internal override void OnExit() { }

        internal override void Update() { }

        internal override void UpdatePhysics() { }

        internal override void Dispose() { }
    }
}