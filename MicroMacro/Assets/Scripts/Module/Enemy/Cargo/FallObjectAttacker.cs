using System;
using System.Buffers;
using Constants;
using CoreModule.Helper;
using CoreModule.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using Module.Scaling;
using Module.UI;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace Module.Enemy.Cargo
{
    public class FallObjectAttacker : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private FallObjectEnemyChecker enemyChecker;
        [SerializeField] private Renderer fallEffectRenderer;
        [SerializeField] private Texture2D fallFireTexture;
        [SerializeField] private VisualEffect fallParticleEffect;
        [SerializeField] private VisualEffect fallImpactEffect;
        [SerializeField] private VisualEffect invalidImpactEffect;
        [SerializeField] private float flipInterval = 0.15f;
        [SerializeField] private MinMaxValue frontFlipRange;
        [SerializeField] private MinMaxValue backFlipRange;

        private FallEffectWrapper fallEffectShader;
        private float currentEffectRadius;
        private Tween currentFallTween;
        private UniqueRandom frontIndexRandom;
        private UniqueRandom backIndexRandom;
        private bool isPlaying;

        private void Start()
        {
            enemyChecker.OnHit += Damage;
            enemyChecker.OnCheckStart += AddListenerFallEffect;
            enemyChecker.OnCheckStop += RemoveListenerFallEffect;

            fallEffectShader = new FallEffectWrapper(fallEffectRenderer.material)
            {
                MainTexture = fallFireTexture
            };

            frontIndexRandom = new UniqueRandom((int)frontFlipRange.Min, (int)frontFlipRange.Max);
            backIndexRandom = new UniqueRandom((int)backFlipRange.Min, (int)backFlipRange.Max);
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
            if (isPlaying)
                return;
            
            PlayFallEffect().Forget();
            fallParticleEffect.Play();
        }

        private void StopAttackEffect()
        {
            StopFallEffect().Forget();
            fallParticleEffect.Stop();
        }

        private async UniTaskVoid PlayFallEffect()
        {
            TimeSpan flipSpan = TimeSpan.FromSeconds(flipInterval);

            fallEffectRenderer.enabled = true;
            isPlaying = true;

            fallEffectShader.FrontTileIndex = 4;
            fallEffectShader.BackTileIndex = 4;
            await UniTask.Delay(flipSpan, cancellationToken: destroyCancellationToken);

            fallEffectShader.FrontTileIndex = 5;
            fallEffectShader.BackTileIndex = 0;
            await UniTask.Delay(flipSpan, cancellationToken: destroyCancellationToken);

            while (isPlaying)
            {
                int frontIndex = frontIndexRandom.Next();
                int backIndex = backIndexRandom.Next();

                fallEffectShader.FrontTileIndex = frontIndex;
                fallEffectShader.BackTileIndex = backIndex;

                await UniTask.Delay(flipSpan, cancellationToken: destroyCancellationToken);
            }
        }

        private async UniTaskVoid StopFallEffect()
        {
            PlayImpactEffect();

            if (scaler.CurrentStep > 0)
            {
                await transform.DOShakePosition(0.25f, 0.1f, 30, 90, false, false).WithCancellation(destroyCancellationToken);
            }

            if (destroyCancellationToken.IsCancellationRequested)
            {
                return;
            }

            isPlaying = false;
            fallEffectRenderer.enabled = false;
        }

        private void PlayImpactEffect()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, Layer.Mask.Enemy))
            {
                if (scaler.CurrentStep > 0)
                {
                    fallImpactEffect.transform.position = hit.point;
                    fallImpactEffect.Play();
                }
                else
                {
                    invalidImpactEffect.transform.position = hit.point;
                    invalidImpactEffect.Play();
                }
            }
        }

        private void Damage(GameObject target)
        {
            // スケールを大きくされていればダメージを与える
            if (scaler.CurrentStep > 0)
            {
                int damage = scaler.CurrentStep;
                target.GetComponent<HealthStatus>().Damage(damage);
                SoundManager.instance.Play("打撃1");
            }
        }
    }
}