using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Module.Scaling
{
    // Scaler を継承してキノコの頭（Transform）とコライダーを拡大/縮小するコンポーネント
    public class MushroomScaler : Scaler
    {
        [SerializeField] private Transform headScale;
        [SerializeField] private BoxCollider boxCollider;

        [SerializeField, Header("1ステップあたりのスケール量(Y軸)")]
        private float scaleAmount = 1;

        [SerializeField, Header("スケール時間")]
        private float scaleDuration = 0.5f;

        /// <summary>
        /// コライダーサイズの調整パラメータ(固定)
        /// </summary>
        private const float fixMultiplier = 0.01f;

        [SerializeField] private Ease scaleEase = Ease.OutBack;

        private float defaultPosition;
        private float defaultColliderSize;
        private Rigidbody rigidBody;
        private Tween currentTween;

        private void Awake()
        {
            defaultPosition = headScale.localPosition.z;
            defaultColliderSize = boxCollider != null ? boxCollider.size.y : 0f;
            rigidBody = GetComponent<Rigidbody>();
        }

        protected override async UniTask OnScale(CancellationToken cancellationToken)
        {
            StartRigidbodyIfNeeded();
            KillCurrentTween();

            ScalerArgsFloat args = CalculateScaleArgs();
            currentTween = CreateScaleTween(args).SetLink(gameObject);

            using (cancellationToken.Register(() => currentTween?.Kill()))
            {
                UniTask completeTask = currentTween.AsyncWaitForCompletion().AsUniTask();
                UniTask rewindTask = currentTween.AsyncWaitForRewind().AsUniTask();
                await UniTask.WhenAny(completeTask, rewindTask);
            }
        }

        protected override void OnScaleImmediate()
        {
            StartRigidbodyIfNeeded();
            KillCurrentTween();

            ScalerArgsFloat args = CalculateScaleArgs();

            ApplyHeadPosition(args.TargetScale);

            if (boxCollider != null)
            {
                var (targetSize, targetCenter) = CalculateColliderTargets();
                ApplyColliderImmediate(targetSize, targetCenter);
            }
        }

        protected override void OnPause()
        {
            currentTween?.Pause();
        }

        protected override void OnResume(bool isForwards)
        {
            if (isForwards)
            {
                currentTween?.Play();
            }
            else
            {
                currentTween?.PlayBackwards();
            }
        }

        private ScalerArgsFloat CalculateScaleArgs()
        {
            float targetScale = defaultPosition + scaleAmount * currentStep * fixMultiplier;

            return new ScalerArgsFloat()
            {
                TargetScale = targetScale,
                PositionOffset = 0,
                Duration = scaleDuration
            };
        }

        private Tween CreateScaleTween(ScalerArgsFloat args)
        {
            float progress = 0f;
            float startPosition = headScale.localPosition.z;

            float startColliderPosition = boxCollider != null ? boxCollider.center.y : 0f;
            float startColliderSize = boxCollider != null ? boxCollider.size.y : 0f;

            var (nextColliderSize, nextColliderPosition) = CalculateColliderTargets();

            return DOTween.To(() => progress,
                value =>
                {
                    progress = value;
                    UpdateTweenState(progress, startPosition, args.TargetScale,
                                     startColliderSize, nextColliderSize,
                                     startColliderPosition, nextColliderPosition);
                }, 1f, args.Duration)
                .SetEase(scaleEase, 3f);
        }

        // 頭部位置を即時／補間で適用する小関数
        private void ApplyHeadPosition(float z)
        {
            Vector3 lp = headScale.localPosition;
            lp.z = z;
            headScale.localPosition = lp;
        }

        // コライダーの目標サイズ・中心位置を算出
        private (float size, float center) CalculateColliderTargets()
        {
            float size = defaultColliderSize + scaleAmount * currentStep * 0.5f;
            float center = scaleAmount * currentStep * 0.25f;
            return (size, center);
        }

        // コライダーの即時反映処理
        private void ApplyColliderImmediate(float targetSize, float targetCenter)
        {
            Vector3 size = boxCollider.size;
            size.y = targetSize;
            boxCollider.size = size;

            Vector3 center = boxCollider.center;
            center.y = targetCenter;
            boxCollider.center = center;
        }

        // Tween の各フレームで呼ぶ更新処理を分離
        private void UpdateTweenState(float progress,
                                      float startPos, float targetPos,
                                      float startSize, float targetSize,
                                      float startCenter, float targetCenter)
        {
            // 頭部の線形補間
            Vector3 lp = headScale.localPosition;
            lp.z = startPos + (targetPos - startPos) * progress;
            headScale.localPosition = lp;

            if (boxCollider != null)
            {
                Vector3 bs = boxCollider.size;
                bs.y = startSize + (targetSize - startSize) * progress;
                boxCollider.size = bs;

                Vector3 bc = boxCollider.center;
                bc.y = startCenter + (targetCenter - startCenter) * progress;
                boxCollider.center = bc;
            }
        }

        private void StartRigidbodyIfNeeded()
        {
            if (rigidBody != null)
            {
                rigidBody.WakeUp();
            }
        }

        private void KillCurrentTween()
        {
            if (currentTween != null)
            {
                currentTween.Kill();
                currentTween = null;
            }
        }
    }
}
