using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField, Header("オーバー演出時間")] private float overDuration = 0.5f;
        [SerializeField, Header("座標移動の無効化")] private bool lockPosition = false;

        [SerializeField, Header("ピボットポイント (0,0:中心 0.5,0.5:右上 -0.5,-0.5:左下)"), Range(-0.5f, 0.5f)]
        private float pivotX;

        [SerializeField, Range(-0.5f, 0.5f)] private float pivotY;

        private Vector3 defaultScale;
        private Tween currentTween;


        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        protected override async UniTask OnScale(CancellationToken cancellationToken)
        {
            Vector3 currentScale = transform.localScale;
            Vector3 currentPosition = transform.localPosition;

            currentTween?.Kill();

            // targetScaleまで滑らかにスケールする
            if (previousStep == CurrentStep && (CurrentStep == MaxStep || CurrentStep == MinStep))
            {
                // オーバー演出
                float sign = CurrentStep == MaxStep ? 1f : -1f;
                TwoAxisScaleArgs args = CalculateScaleArgs(currentPosition, new Vector3(0.5f, 0.5f) * sign);
                args.Duration = overDuration;

                Tween scaleTween = CreateScaleTween(currentPosition, currentScale, args);

                TwoAxisScaleArgs unScaleArgs = new TwoAxisScaleArgs()
                {
                    TargetScale = currentScale,
                    PositionOffset = -args.PositionOffset,
                    Duration = overDuration
                };
                Tween unScaleTween = CreateScaleTween(currentPosition + args.PositionOffset, args.TargetScale, unScaleArgs);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(scaleTween);
                sequence.Append(unScaleTween);
                currentTween = sequence;
            }
            else
            {
                // 通常のスケール
                TwoAxisScaleArgs args = CalculateScaleArgs(currentPosition, Vector3.zero);
                args.Duration = scaleDuration;
                currentTween = CreateScaleTween(currentPosition, currentScale, args);
            }

            await currentTween.SetLink(gameObject).WithCancellation(cancellationToken);
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