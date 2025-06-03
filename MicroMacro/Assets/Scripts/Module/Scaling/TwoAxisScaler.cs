using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Module.Scaling
{
    /// <summary>
    /// XY軸方向にスケールするオブジェクト
    /// </summary>
    public class TwoAxisScaler : Scaler
    {
        [SerializeField, Header("1ステップあたりのスケール量")] private Vector2 scaleAmount;
        [SerializeField, Header("スケール時間")] private float scaleDuration;

        [SerializeField, Header("ピボットポイント (0,0:中心 0.5,0.5:右上 -0.5,-0.5:左下)"), Range(-0.5f, 0.5f)]
        private float pivotX;

        [SerializeField, Range(-0.5f, 0.5f)] private float pivotY;

        private Vector3 defaultScale;

        private void Start()
        {
            defaultScale = transform.localScale;
        }

        protected override async UniTask OnScale(CancellationToken cancellationToken)
        {
            Vector3 currentScale = transform.localScale;
            Vector3 currentPosition = transform.localPosition;
            Vector3 targetScale = defaultScale + (Vector3)scaleAmount * step;

            // スケール後の座標を求める
            Vector2 pivot = new Vector2(pivotX, pivotY);
            Vector2 scaledPosition = CalculateScaledPosition(pivot, targetScale);
            Vector3 positionOffset = (Vector3)scaledPosition - currentPosition;

            // targetScaleまで滑らかにスケールする
            float progress = 0f;

            await DOTween.To(() => progress,
                    value =>
                    {
                        progress = value;
                        transform.localScale = currentScale + (targetScale - currentScale) * progress;
                        transform.localPosition = currentPosition + positionOffset * progress;
                    }, 1f, scaleDuration)
                .SetEase(Ease.OutBack, 3f)
                .SetLink(gameObject)
                .WithCancellation(cancellationToken);
        }

        /// <summary>
        /// スケール後の座標を算出します
        /// </summary>
        private Vector2 CalculateScaledPosition(Vector2 pivot, Vector2 newScale)
        {
            Vector2 localPosition = transform.localPosition;
            Vector2 amount = newScale - (Vector2)transform.localScale;
            Vector2 position = localPosition - amount * pivot;

            return position;
        }
    }
}