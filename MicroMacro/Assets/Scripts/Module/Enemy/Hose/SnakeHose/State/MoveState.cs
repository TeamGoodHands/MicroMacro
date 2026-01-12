using CoreModule.AI.HSM;
using CoreModule.Helper;
using DG.Tweening;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose
{
    public class MoveState : HierarchicalStateMachine.State
    {
        private readonly SnakeController snakeController;
        private readonly SnakeHoseParameter parameter;
        private readonly SnakeHoseCondition condition;

        private Tween moveSpeedTween;
        private float moveTime;

        public MoveState(SnakeController snakeController, SnakeHoseParameter parameter, SnakeHoseCondition condition)
        {
            this.snakeController = snakeController;
            this.parameter = parameter;
            this.condition = condition;
        }

        internal override void OnEnter()
        {
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