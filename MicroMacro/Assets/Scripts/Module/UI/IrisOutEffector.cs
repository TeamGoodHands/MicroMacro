using System;
using DG.Tweening;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.UI;

namespace Module.UI
{
    public class IrisOutEffector : MonoBehaviour
    {
        [Header("閉じるとき")]
        [SerializeField] private float closeFocusRadius;
        [SerializeField] private float closeFocusDuration;

        [SerializeField] private float closeCompleteRadius;
        [SerializeField] private float closeCompleteDuration;

        [Header("開くとき")]
        [SerializeField] private float openFocusRadius;
        [SerializeField] private float openFocusDuration;

        [SerializeField] private float openCompleteRadius;
        [SerializeField] private float openCompleteDuration;


        private Tween currentTween;
        private IrisOverlayWrapper irisOverlay;

        private void Start()
        {
            if (TryGetComponent(out Image image))
            {
                irisOverlay = new IrisOverlayWrapper(image.material);
            }
            else
            {
                throw new Exception("IrisOutEffector: Image is not found");
            }

            irisOverlay.Radius = openCompleteRadius;
            irisOverlay.Center01 = new Vector2(0.5f, 0.5f);
        }

        public void DoIrisOut(Vector3 playerPosition)
        {
            SetIrisPosition(playerPosition);

            currentTween?.Kill();

            Sequence irisOutSequence = DOTween.Sequence();
            irisOutSequence.Append(DOVirtual.Float(irisOverlay.Radius, closeFocusRadius, closeFocusDuration, t => { irisOverlay.Radius = t; })
                .SetEase(Ease.OutBack));

            irisOutSequence.Append(DOVirtual.Float(closeFocusRadius, closeCompleteRadius, closeCompleteDuration, t => { irisOverlay.Radius = t; })
                .SetEase(Ease.OutBack));

            currentTween = irisOutSequence;
        }

        public void DoIrisIn(Vector3 playerPosition)
        {
            SetIrisPosition(playerPosition);

            currentTween?.Kill();

            Sequence irisOutSequence = DOTween.Sequence();
            irisOutSequence.Append(DOVirtual.Float(irisOverlay.Radius, openFocusRadius, openFocusDuration, t => { irisOverlay.Radius = t; })
                .SetEase(Ease.OutBack));

            irisOutSequence.Append(DOVirtual.Float(openFocusRadius, openCompleteRadius, openCompleteDuration, t => { irisOverlay.Radius = t; })
                .SetEase(Ease.OutBack));

            currentTween = irisOutSequence;
        }

        private void SetIrisPosition(Vector3 playerPosition)
        {
            Vector2 screenPos = Camera.main.WorldToViewportPoint(playerPosition);
            screenPos.x = Mathf.Clamp01(screenPos.x);
            screenPos.y = Mathf.Clamp01(screenPos.y);
            irisOverlay.Center01 = screenPos;
        }
    }
}