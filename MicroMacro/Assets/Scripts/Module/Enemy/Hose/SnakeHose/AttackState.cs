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

        public AttackState(SnakeHoseTapBehaviour tapBehaviour, SnakeHoseParameter parameter, SnakeHoseCondition condition)
        {
            this.tapBehaviour = tapBehaviour;
            this.parameter = parameter;
            this.condition = condition;
        }

        internal override void OnEnter()
        {
            Debug.Log("Attack");
            AttackSequence().Forget();
        }
        
        private async UniTaskVoid AttackSequence()
        {
            await tapBehaviour.LookAtPlayer(parameter.TimeToFacePlayer);
            
            await tapBehaviour.ShakeBody(parameter.ShakeTime);

            await tapBehaviour.OnWater();

            await UniTask.Delay(TimeSpan.FromSeconds(parameter.AttackDuration));

            await tapBehaviour.OffWater();
            
            await tapBehaviour.ResetAngle(parameter.TimeToResetAngle);

            condition.CurrentState = SnakeHoseCondition.State.Move;
        }

        internal override void OnExit()
        {
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