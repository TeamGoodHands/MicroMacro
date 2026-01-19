using System;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using Module.Player.Component;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Module.Enemy.Hose.SnakeHose.State
{
    public class SmashAttackState : HierarchicalStateMachine.State
    {
        private readonly SnakeHoseComponents components;
        private readonly SnakeHoseParameter parameter;
        private readonly SnakeHoseCondition condition;

        private PlayerCondition playerCondition;
        private PlayBGM playBgm;

        public SmashAttackState(SnakeHoseComponents components, SnakeHoseParameter parameter, SnakeHoseCondition condition)
        {
            this.components = components;
            this.parameter = parameter;
            this.condition = condition;

            playerCondition = GameObject.FindGameObjectWithTag(Tag.Player).GetComponent<PlayerCondition>();
        }

        internal override void OnEnter()
        {
            PlayClearEffect().Forget();
        }

        private async UniTaskVoid PlayClearEffect()
        {
            components.HeadTransform.DOMove(new Vector3(0f, 5, -18f), 2f).SetEase(Ease.InOutSine).SetRelative();

            playBgm = Object.FindAnyObjectByType<PlayBGM>();
            playBgm.BGMSource.DOFade(0f, 1f);

            Time.timeScale = 0.5f;

            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: CancellationToken);

            playerCondition.IsPlayerLocked = true;

            Time.timeScale = 1f;

            components.ClearPlayer.ClearEffect().Forget();
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