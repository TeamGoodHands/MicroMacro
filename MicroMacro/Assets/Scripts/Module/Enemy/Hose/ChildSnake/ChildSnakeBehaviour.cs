using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Enemy.Hose.SnakeHose;
using Module.Management;
using Module.Scaling;
using Module.UI;
using UnityEngine;

namespace Module.Enemy.Hose.ChildSnake
{
    public class ChildSnakeBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private Transform bodyBone;
        [SerializeField] private Scaler scaler;
        [SerializeField] private HealthStatus healthStatus;
        [SerializeField] private SnakeHoseController controller;
        [SerializeField] private ChildSnakeParameter parameter;
        [SerializeField] private LockOnEffect lockOnEffect;
        [SerializeField] private Vector3[] targets;
        [SerializeField] private Vector3 destroyPosition;
        [SerializeField] private Transform[] raptures;

        public event Action OnDeath;

        private CancellationTokenSource damageCanceller;
        private CancellationTokenSource canceller;
        private int raptureIndex;

        public void Appear()
        {
            damageCanceller = new CancellationTokenSource();
            canceller = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, damageCanceller.Token);
            AppearAsync().Forget();

            scaler.OnScaleCompleted += HandleScale;
        }

        private void HandleScale(ScaleEventArgs args)
        {
            if (args.CurrentStep == scaler.MinStep)
            {
                ApplyDamage().Forget();
            }
        }

        private async UniTaskVoid AppearAsync()
        {
            await transform.DOLocalMove(targets[0], 2f).SetEase(Ease.OutBack);
            await UniTask.Delay(TimeSpan.FromSeconds(parameter.FirstDelay));
            PatrolRandomlyAsync(damageCanceller.Token).Forget();
        }

        private async UniTask ApplyDamage()
        {
            ResetEffect();
            damageCanceller.Cancel();
            damageCanceller.Dispose();

            Vector3 bodyScale = bodyBone.localScale;
            await bodyBone.DOScale(bodyScale * 1.5f, 1f);

            Transform rapture = raptures[raptureIndex++];
            Vector3 raptureScale = rapture.localScale;
            rapture.localScale = new Vector3(raptureScale.x, 0f, raptureScale.z);
            rapture.gameObject.SetActive(true);

            rapture.DOScale(raptureScale, 0.5f).SetEase(Ease.OutBack);
            bodyBone.localScale = bodyScale;
            scaler.SetScale(0, true);

            SoundManager.instance.Play("打撃6");

            await transform.DOShakePosition(1f, 0.1f, 30, 90, false, false);

            healthStatus.Damage(1);
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: destroyCancellationToken);

            if (raptureIndex >= raptures.Length)
            {
                DestroyAsync().Forget();
            }
            else
            {
                damageCanceller = new CancellationTokenSource();
                canceller = CancellationTokenSource.CreateLinkedTokenSource(canceller.Token, damageCanceller.Token);
                PatrolRandomlyAsync(damageCanceller.Token).Forget();
            }
        }

        private async UniTaskVoid PatrolRandomlyAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int targetIndex = UnityEngine.Random.Range(1, targets.Length);
                await transform
                    .DOLocalMove(targets[targetIndex], 1.5f)
                    .SetEase(Ease.OutBack)
                    .WithCancellation(token);

                await controller.LookAtPlayerSmoothAsync(1f, 1f, token);

                if (token.IsCancellationRequested)
                    return;

                lockOnEffect.LockOn();

                await controller.LookAtPlayerSmoothAsync(parameter.TimeToFacePlayer, 4f, token);

                await controller.ShakeBody(parameter.ShakeTime).WithCancellation(token);

                if (token.IsCancellationRequested)
                    return;

                lockOnEffect.LockOff();

                await controller.OnWater(token);

                await UniTask.Delay(TimeSpan.FromSeconds(parameter.AttackDuration), cancellationToken: token);

                await controller.OffWater(token);

                await controller.ResetAngle(parameter.TimeToResetAngle).WithCancellation(token);
            }
        }

        private void ResetEffect()
        {
            lockOnEffect.LockOff();
            controller.OffWaterImmediately();
        }

        private async UniTaskVoid DestroyAsync()
        {
            await transform.DOLocalMove(destroyPosition, 1f);
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }
}