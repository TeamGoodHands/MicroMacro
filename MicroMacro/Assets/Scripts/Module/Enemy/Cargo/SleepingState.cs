using System;
using CoreModule.AI.HSM;
using CoreModule.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Module.Enemy.Cargo
{
    public class SleepingState : HierarchicalStateMachine.State
    {
        private readonly CargoComponent component;
        private readonly RadialBlurFeature blurFeature;

        private int damageCount;
        private bool isAwaking;
        private CargoShaderWrapper cargoShaderWrapper;
        private Sequence damageColorSequence;

        public SleepingState(CargoComponent component)
        {
            this.component = component;

            if (!RendererFeatures.TryGetRendererFeature(out blurFeature))
            {
                Debug.LogError("RadialBlurFeatureが存在しません!");
            }

            component.HpBarCanvasGroup.alpha = 0f;
            cargoShaderWrapper = new CargoShaderWrapper(component.Renderer.material);
        }

        private void OnDamage(int damage)
        {
            if (damage == component.Status.MaxHealth)
                return;

            AnimateAsync().Forget();
        }

        private async UniTaskVoid AnimateAsync()
        {
            component.AnimatorWrapper.SetDamageTrigger();
            PlayDamageColor();

            damageCount++;

            if (damageCount < 3 || isAwaking)
                return;

            isAwaking = true;

            await DoAwake();
        }

        private void PlayDamageColor()
        {
            Color color = component.Parameter.DamageAdditionalColor;
            damageColorSequence?.Kill();
            damageColorSequence = DOTween.Sequence();
            damageColorSequence.Append(DOTween.To(() => Color.black, c => cargoShaderWrapper.AdditionalColor = c, color, 0.1f));
            damageColorSequence.Append(DOTween.To(() => color, c => cargoShaderWrapper.AdditionalColor = c, Color.black, 0.1f));
            damageColorSequence.Play();
        }

        private async UniTask DoAwake()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f));

            component.AnimatorWrapper.SetAngryTrigger();

            component.Status.SetHealth(component.Status.MaxHealth);
            component.BossCamera.Priority = -1;
            component.NearInEnemyCamera.Priority = 100;

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            SoundManager.instance.Play("目を開く");

            RadialBlurParams parameter = blurFeature.GetParams();

            parameter.Intensity = 0.3f;

            // 目を開いた瞬間にラディアルブラー
            _ = DOTween.To(() => parameter.Intensity, x => parameter.Intensity = x, 0f, 0.3f);

            await UniTask.Delay(TimeSpan.FromSeconds(1.2f));

            component.NearInEnemyCamera.Priority = -1;
            component.BossCamera.Priority = 100;
            _ = component.HpBarCanvasGroup.DOFade(1f, 2f);

            SoundManager.instance.Play("Boss2");

            await UniTask.Delay(TimeSpan.FromSeconds(1.4f));

            component.Condition.SwitchState(CargoCondition.State.BackAttack);
        }

        internal override void OnEnter()
        {
            component.Status.OnDamage += OnDamage;
        }

        internal override void OnExit()
        {
            component.Status.OnDamage -= OnDamage;
        }

        internal override void Update() { }

        internal override void LateUpdate() { }

        internal override void UpdatePhysics() { }

        internal override void Dispose()
        {
            RadialBlurParams parameter = blurFeature.GetParams();
            parameter.Intensity = 0f;
        }
    }
}