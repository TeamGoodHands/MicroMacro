using System;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose
{
    public class AttackState : HierarchicalStateMachine.State
    {
        private readonly SnakeHoseController controller;
        private readonly SnakeHoseParameter parameter;
        private readonly SnakeHoseCondition condition;
        private readonly LockOnEffect lockOnEffect;

        public AttackState(SnakeHoseController controller, SnakeHoseParameter parameter, SnakeHoseCondition condition, LockOnEffect lockOnEffect)
        {
            this.controller = controller;
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

            await controller.LookAtPlayerSmoothAsync(parameter.TimeToFacePlayer, 4f);

            await controller.ShakeBody(parameter.ShakeTime);

            lockOnEffect.LockOff();

            await controller.OnWater();

            await UniTask.Delay(TimeSpan.FromSeconds(parameter.AttackDuration));

            await controller.OffWater();

            await controller.ResetAngle(parameter.TimeToResetAngle);

            condition.CurrentState = SnakeHoseCondition.State.Move;
        }

        internal override void OnExit() { }

        internal override void Update() { }

        internal override void UpdatePhysics() { }

        internal override void Dispose() { }
    }
}