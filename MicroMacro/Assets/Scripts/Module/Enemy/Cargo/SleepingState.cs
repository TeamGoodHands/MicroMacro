using System;
using CoreModule.AI.HSM;
using CoreModule.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Module.Enemy.Cargo
{
    public class SleepingState : HierarchicalStateMachine.State
    {
        private readonly CargoComponent component;
        private readonly Vector3[] localPositions;
        private readonly Vector3[] defaultPositions;
        private readonly RadialBlurFeature blurFeature;

        private int damageCount;

        public SleepingState(CargoComponent component)
        {
            this.component = component;

            if (!RendererFeatures.TryGetRendererFeature(out blurFeature))
            {
                Debug.LogError("RadialBlurFeatureが存在しません!");
            }

            localPositions = new Vector3[component.Eyes.Length];
            defaultPositions = new Vector3[component.Eyes.Length];

            CloseEyes();
            
            component.HpBarCanvasGroup.alpha = 0f;
        }

        private void OnDamage(int damage)
        {
            if (damage == component.Status.MaxHealth)
                return;
            
            AnimateAsync().Forget();
        }

        private void CloseEyes()
        {
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

        private async UniTaskVoid AnimateAsync()
        {
            float delta = 0f;

            // 仮ダメージアニメーション
            _ = component.BodyTransform.DOShakeRotation(0.5f, new Vector3(7f, 0f, 0f), 25);

            damageCount++;

            if (damageCount < 3)
                return;

            await UniTask.Delay(TimeSpan.FromSeconds(2f));

            component.Status.SetHealth(component.Status.MaxHealth);
            component.NearInEnemyCamera.Priority = 100;

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            SoundManager.instance.Play("目を開く");

            RadialBlurParams parameter = blurFeature.GetParams();

            parameter.Intensity = 0.3f;

            // 目を開いた瞬間にラディアルブラー
            _ = DOTween.To(() => parameter.Intensity, x => parameter.Intensity = x, 0f, 0.3f);

            // 目を前に飛び出す
            _ = DOTween.To(() => delta, x =>
            {
                for (int i = 0; i < 2; i++)
                {
                    float d = Mathf.Lerp(0f, 0.004f, x);
                    localPositions[i] = defaultPositions[i] + new Vector3(0f, d, d);
                }

                delta = x;
            }, 1f, 0.5f).SetEase(Ease.OutBack, 10f);

            await UniTask.Delay(TimeSpan.FromSeconds(1.2f));

            component.NearInEnemyCamera.Priority = -1;
            component.BossCamera.Priority = 1000;
            _ = component.HpBarCanvasGroup.DOFade(1f, 2f);

            SoundManager.instance.Play("Boss2");

            await UniTask.Delay(TimeSpan.FromSeconds(1.4f));

            component.Condition.CurrentState = CargoCondition.State.Move;
            component.AnimatorWrapper.SetDamageTrigger();
        }

        internal override void OnEnter()
        {
            component.Status.OnDamage += OnDamage;
        }

        internal override void OnExit()
        {
            component.Status.OnDamage -= OnDamage;
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
            RadialBlurParams parameter = blurFeature.GetParams();
            parameter.Intensity = 0f;
        }
    }
}