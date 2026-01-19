using System;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Module.Enemy.Cargo
{
    public class DeathState : HierarchicalStateMachine.State
    {
        private readonly CargoComponent component;

        public DeathState(CargoComponent component)
        {
            this.component = component;
        }

        internal override void OnEnter()
        {
            DeathAsync().Forget();
        }

        private async UniTaskVoid DeathAsync()
        {
            component.AnimatorWrapper.SetDeathTrigger();
            component.BossCamera.Priority = -1;
            component.DeathCamera.Priority = 100;

            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: CancellationToken);

            Time.timeScale = 0.3f;

            component.BGMSource.DOFade(0f, 0.4f);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: CancellationToken);

            Time.timeScale = 1f;
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