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

        [SerializeField, Header("ピボットポイント (0,0:左下 1,1:右上)")]
        private Vector2 pivot;

        private Vector3 defaultScale;
        private Vector2 defaultPosition;

        private void Start()
        {
            defaultScale = transform.localScale;
            defaultPosition = transform.localPosition;
        }

        protected override async UniTask OnScale(CancellationToken cancellationToken)
        {
            Vector3 currentScale = transform.localScale;
            Vector3 currentPosition = transform.localPosition;
            Vector3 targetScale = defaultScale + (Vector3)scaleAmount * step;

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

        protected Vector2 pic;

        /// <summary>
        /// スケール後の座標を算出します
        /// </summary>
        private Vector2 CalculateScaledPosition(Vector2 pivot, Vector2 newScale)
        {
            Vector2 localPosition = transform.localPosition;
            Vector2 pivotDelta = transform.localScale * pivot;

            pic = localPosition + pivotDelta;

            Vector2 position = localPosition - pivotDelta * (newScale - (Vector2)transform.localScale);

            return position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere((Vector3)pic, 0.1f);
        }
    }
}