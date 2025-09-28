using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Scaling;
using UnityEngine;

namespace Module.Enemy.Boomerang
{
    public class Boomerang : MonoBehaviour
    {
        [SerializeField, Header("ブーメランの移動速度")] private float boomerangSpeed;
        [SerializeField, Header("ブーメランの回転速度")] private int boomerangRotateSpeed = 5;
        [SerializeField, Header("ブーメランを飛ばす距離")] private float boomerangRadius = 5f;
        [SerializeField, Header("ブーメランを投げる間隔")] private float boomerangInterval = 1f;
        [SerializeField, Header("プレイヤーを感知する距離")] private float detectDistance = 1f;

        [SerializeField] private EnemyStatus status;
        [SerializeField] private Scaler boomerang;
        [SerializeField] private Rigidbody boomerangRig;

        private CancellationTokenSource behaviourCanceller;

        private void Start()
        {
            // 死亡イベントを登録
            status.OnDeath += OnDeath;

            behaviourCanceller = new CancellationTokenSource();
            DoBoomerang(behaviourCanceller.Token).Forget();
        }

        private async void OnDeath()
        {
            behaviourCanceller.Cancel();

            // ちょっと揺らす
            await transform.DOShakePosition(0.5f, 0.5f, 10, 90f, false, true);

            Destroy(gameObject);
        }

        private async UniTaskVoid DoBoomerang(CancellationToken cancellationToken)
        {
            float duration = boomerangRadius / boomerangSpeed;
            Vector3 targetPosition = boomerang.transform.position + transform.right * boomerangRadius;

            // 自身が破棄されるまでループする
            while (!cancellationToken.IsCancellationRequested)
            {
                await ThrowBoomerang(targetPosition, duration);

                CheckBoomerangHit();

                await UniTask.Delay(TimeSpan.FromSeconds(boomerangInterval), cancellationToken: cancellationToken);
                boomerang.SetScale(0, true);
            }
        }

        private async UniTask ThrowBoomerang(Vector3 targetPosition, float duration)
        {
            // 累積回転角度を保持
            float totalAngle = 0f;
            Quaternion startRotation = boomerangRig.rotation;

            DOTween.To(
                    () => 0f,
                    angle =>
                    {
                        // 回転量を取得
                        float delta = angle - totalAngle;
                        totalAngle = angle;

                        // Rigidbodyをz軸で回転
                        boomerangRig.MoveRotation(boomerangRig.rotation * Quaternion.AngleAxis(delta, Vector3.forward));
                    },
                    360f * boomerangRotateSpeed, // 行き帰りで n 回転
                    duration * 2f // 往復分の時間
                )
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    // 終了時にスタート回転へ戻す（ズレ防止）
                    boomerangRig.MoveRotation(startRotation);
                });

            // 指定した位置まで移動する
            await DOTween.To(
                    () => boomerangRig.position,
                    x => boomerangRig.MovePosition(x),
                    targetPosition,
                    duration
                ).SetEase(Ease.InOutQuad)
                .SetLoops(2, LoopType.Yoyo); // 逆再生して戻って来る
        }

        private void OnDestroy()
        {
            if (status != null)
            {
                status.OnDeath -= OnDeath;
            }

            behaviourCanceller?.Cancel();
            behaviourCanceller?.Dispose();
            behaviourCanceller = null;
        }

        private void CheckBoomerangHit()
        {
            if (boomerang.CurrentStep > 0)
            {
                status.Damage(1);
            }
        }
    }
}