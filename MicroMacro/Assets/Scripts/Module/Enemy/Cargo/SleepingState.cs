using System;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class SleepingState : HierarchicalStateMachine.State
    {
        private readonly CargoComponent component;
        private readonly Vector3[] localPositions;
        private readonly Vector3[] defaultPositions;

        public SleepingState(CargoComponent component)
        {
            this.component = component;
            component.Status.OnDamage += OnDamage;

            localPositions = new Vector3[component.Eyes.Length];
            defaultPositions = new Vector3[component.Eyes.Length];

            for (var i = 0; i < component.Eyes.Length; i++)
            {
                Transform transform = component.Eyes[i];
                Vector3 position = transform.localPosition;
                defaultPositions[i] = position;

                position.y -= 0.01f;
                position.z -= 0.01f;
                localPositions[i] = position;
            }
        }

        private void OnDamage(int damage)
        {
            AnimateAsync().Forget();
        }

        private async UniTaskVoid AnimateAsync()
        {
            float delta = 0f;

            _ = DOTween.To(() => delta, x =>
            {
                for (int i = 0; i < 2; i++)
                {
                    float d = Mathf.Lerp(0f, 0.008f, x);
                    localPositions[i] = defaultPositions[i] + new Vector3(0f, d, d);
                }

                delta = x;
            }, 1f, 0.5f).SetEase(Ease.OutBack, 10f);

            // 仮ダメージアニメーション
            _ = component.BodyTransform.DOShakeRotation(0.5f, new Vector3(7f, 0f, 0f), 25);

            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            component.Condition.CurrentState = CargoCondition.State.Move;
            component.AnimatorWrapper.SetDamageTrigger();
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

        internal override void LateUpdate()
        {
            for (int i = 0; i < component.Eyes.Length; i++)
            {
                component.Eyes[i].localPosition = localPositions[i];
            }
        }

        internal override void UpdatePhysics()
        {
        }

        internal override void Dispose()
        {
        }
    }
}