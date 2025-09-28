using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using UnityEngine;

namespace Module.Scaling
{
    /// <summary>
    /// XY軸方向にスケールするオブジェクト
    /// </summary>
    public class TwoAxisScaler : Scaler
    {
        [SerializeField, Header("1ステップあたりのスケール量")] private Vector2 scaleAmount = new Vector2(1, 1);
        [SerializeField, Header("スケール時間")] private float scaleDuration = 0.5f;
        [SerializeField, Header("座標移動の無効化")] private bool lockPosition = false;

        [SerializeField, Header("ピボットポイント (0,0:中心 0.5,0.5:右上 -0.5,-0.5:左下)"), Range(-0.5f, 0.5f)]
        private float pivotX;

        [SerializeField, Range(-0.5f, 0.5f)] private float pivotY;

        private Vector3 defaultScale;
        private Tween currentTween;
        private Rigidbody rigidBody;

        private void Awake()
        {
            defaultScale = transform.localScale;
            rigidBody = GetComponent<Rigidbody>();
        }

        protected override async UniTask OnScale(CancellationToken cancellationToken)
        {
            Vector3 currentScale = transform.localScale;
            Vector3 currentPosition = transform.localPosition;

            // 物理の影響を受けていないRigidbodyを起動する
            if (rigidBody != null)
            {
                rigidBody.WakeUp();
            }

            currentTween?.Kill();

            // targetScaleまで滑らかにスケールする
            TwoAxisScaleArgs args = CalculateScaleArgs(currentPosition, Vector3.zero);
            args.Duration = scaleDuration;
            currentTween = CreateScaleTween(currentPosition, currentScale, args).SetLink(gameObject);

            // 完了を待っている間にキャンセルされたらtweenをキルする
            await using (cancellationToken.Register(() => currentTween?.Kill()))
            {
                // 完了を待つタスク
                UniTask completeTask = currentTween.AsyncWaitForCompletion().AsUniTask();
                
                // 巻き戻しを待つタスク
                UniTask rewindTask = currentTween.AsyncWaitForRewind().AsUniTask();

                // いずれかの完了を待つ
                await UniTask.WhenAny(completeTask, rewindTask);
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

        private Tween CreateScaleTween(Vector3 currentPosition, Vector3 currentScale, TwoAxisScaleArgs args)
        {
            float progress = 0f;
            return DOTween.To(() => progress,
                    value =>
                    {
                        progress = value;
                        transform.localScale = currentScale + (args.TargetScale - currentScale) * progress;

                        if (!lockPosition)
                        {
                            transform.localPosition = currentPosition + args.PositionOffset * progress;
                        }
                    }, 1f, args.Duration)
                .SetEase(Ease.OutBack, 3f);
        }

        private TwoAxisScaleArgs CalculateScaleArgs(Vector3 currentPosition, Vector3 scaleOffset)
        {
            // 目標スケール値を求める
            Vector3 targetScale = defaultScale + (Vector3)scaleAmount * currentStep + scaleOffset;

            // スケール後の座標を求める
            Vector2 pivot = new Vector2(pivotX, pivotY);
            Vector2 scaledPosition = CalculateScaledPosition(pivot, targetScale);
            Vector3 positionOffset = (Vector3)scaledPosition - currentPosition;

            return new TwoAxisScaleArgs() { TargetScale = targetScale, PositionOffset = positionOffset };
        }

        /// <summary>
        /// スケール後の座標を算出します
        /// </summary>
        private Vector2 CalculateScaledPosition(Vector2 pivot, Vector2 newScale)
        {
            // ピボットを基準にした座標補正の計算
            Vector2 localPosition = transform.localPosition;
            Vector2 changeAmount = newScale - (Vector2)transform.localScale;

            // ピボット分のオフセットを適用
            return localPosition - changeAmount * pivot;
        }

        private struct TwoAxisScaleArgs
        {
            public Vector3 TargetScale; // 目標スケール値
            public Vector3 PositionOffset; // 前の地点からの座標の差分
            public float Duration; // スケール時間
        }
    }
}