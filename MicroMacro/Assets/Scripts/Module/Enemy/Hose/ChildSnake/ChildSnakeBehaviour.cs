using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField] private Vector3[] targets;
        [SerializeField] private Vector3 destroyPosition;
        [SerializeField] private Transform[] raptures;

        private CancellationTokenSource damageCanceller;
        private CancellationTokenSource canceller;
        private int raptureIndex;

        private void Start()
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
            PatrolRandomlyAsync().Forget();
        }

        private async UniTask ApplyDamage()
        {
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
                PatrolRandomlyAsync().Forget();
            }
        }

        private async UniTaskVoid PatrolRandomlyAsync()
        {
            
        }

        private async UniTaskVoid DestroyAsync()
        {
            Debug.Log("Destroy");
            await transform.DOLocalMove(destroyPosition, 1f);
            Destroy(gameObject);
        }
    }
}