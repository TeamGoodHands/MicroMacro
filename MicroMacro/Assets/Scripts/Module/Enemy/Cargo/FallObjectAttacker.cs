using System;
using DG.Tweening;
using Module.Management;
using Module.Scaling;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Enemy.Cargo
{
    public class FallObjectAttacker : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private FallObjectEnemyChecker enemyChecker;
        [SerializeField] private Renderer fallEffectRenderer;
        [SerializeField] private VisualEffect fallParticleEffect;

        private WaterCircleShaderWrapper waterCircleShaderWrapper;
        private float currentEffectRadius;
        private Tween currentFallTween;

        private void Start()
        {
            enemyChecker.OnHit += Damage;
            enemyChecker.OnCheckStart += AddListenerFallEffect;
            enemyChecker.OnCheckStop += RemoveListenerFallEffect;
            waterCircleShaderWrapper = new WaterCircleShaderWrapper(fallEffectRenderer.material);
        }

        private void AddListenerFallEffect()
        {
            if (scaler.CurrentStep > 0)
            {
                PlayAttackEffect();
            }
            
            scaler.OnScaleStarted += CheckFallEffect;
        }

        private void RemoveListenerFallEffect()
        {
            StopAttackEffect();
            
            scaler.OnScaleStarted -= CheckFallEffect;
        }

        private void CheckFallEffect(ScaleEventArgs args)
        {
            if (args.CurrentStep > 0)
            {
                PlayAttackEffect();
            }
            else
            {
                StopAttackEffect();
            }
        }

        private void PlayAttackEffect()
        {
            currentFallTween?.Kill();
            currentFallTween = DOTween.To(() => currentEffectRadius, value => currentEffectRadius = value, 0.3f, 0.3f)
                .SetEase(Ease.OutSine)
                .OnStart(() => fallEffectRenderer.enabled = true)
                .OnUpdate(() => waterCircleShaderWrapper.Radius = currentEffectRadius);
            
            fallParticleEffect.Play();
        }

        private void StopAttackEffect()
        {
            currentFallTween?.Kill();
            currentFallTween = DOTween.To(() => currentEffectRadius, value => currentEffectRadius = value, 0f, 0.2f)
                .SetEase(Ease.OutSine)
                .OnUpdate(() => waterCircleShaderWrapper.Radius = currentEffectRadius)
                .OnComplete(() => fallEffectRenderer.enabled = false);
            
            fallParticleEffect.Stop();
        }

        private void Damage(GameObject target)
        {
            // スケールを大きくされていればダメージを与える
            if (scaler.CurrentStep > 0)
            {
                target.GetComponent<EnemyStatus>().Damage(1);
                SoundManager.instance.Play("打撃1");
            }
        }
    }
}