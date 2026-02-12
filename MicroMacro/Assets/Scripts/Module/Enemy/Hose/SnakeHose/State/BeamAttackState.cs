using System;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Enemy.Hose.ChildSnake;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose
{
    public class BeamAttackState : HierarchicalStateMachine.State
    {
        private readonly SnakeHoseComponents components;
        private readonly SnakeHoseParameter parameter;
        private readonly SnakeHoseCondition condition;

        private static readonly int AttackModeHash = Animator.StringToHash("AttackMode");
        private int deathCount;

        public BeamAttackState(SnakeHoseComponents components, SnakeHoseParameter parameter, SnakeHoseCondition condition)
        {
            this.components = components;
            this.parameter = parameter;
            this.condition = condition;
        }

        internal override void OnEnter()
        {
            components.HeadTransform.DOMove(condition.DefaultPosition, 0.5f).SetEase(Ease.InOutSine);
            PrepareMove().Forget();
        }

        private async UniTaskVoid AppearChildren()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: CancellationToken);

            foreach (ChildSnakeBehaviour child in components.Children)
            {
                child.Appear();
                child.OnDeath += HandleDeath;
            }
        }

        private async UniTaskVoid PrepareMove()
        {
            await components.SplineTranform.DOLocalMove(parameter.DefaultPosition, 1f);

            components.BeamAttackCamera.Priority = 1000;
            components.Animator.enabled = true;
            components.Animator.Play("PrepareBeam");
            // components.Animator.SetInteger(AttackModeHash, 1);
            
            condition.CurrentState = SnakeHoseCondition.State.SmashAttack;
            return;

             AppearChildren().Forget();
        }

        private void HandleDeath()
        {
            deathCount++;

            if (deathCount >= components.Children.Length)
            {
                condition.CurrentState = SnakeHoseCondition.State.SmashAttack;
            }
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