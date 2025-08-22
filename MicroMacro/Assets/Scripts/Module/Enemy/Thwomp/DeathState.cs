using System;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;
using Object = UnityEngine.Object;

namespace Module.Enemy.Thwomp
{
    public class DeathState : HierarchicalStateMachine.State
    {
        private readonly GameObject bodyObject;
        private readonly VisualEffect crackEffect;

        public DeathState(ThwompComponent component)
        {
            bodyObject = component.Rigidbody.transform.GetChild(0).gameObject;
            crackEffect = component.CrackEffect;
        }

        internal override void OnEnter()
        {
            DestroyTask().Forget();
        }

        internal override void OnExit() { }

        internal override void Update() { }

        private async UniTaskVoid DestroyTask()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: CancellationToken);
            
            if (bodyObject == null)
                return;

            bodyObject.SetActive(false);
            crackEffect.SetInt("Count", 1000);
            crackEffect.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: CancellationToken);

            Object.Destroy(bodyObject.transform.parent.gameObject);
        }

        internal override void UpdatePhysics() { }

        internal override void Dispose() { }
    }
}