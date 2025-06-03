using DG.Tweening;
using Module.Scaling;
using UnityEngine;

namespace Module.Gimmick
{
    public class HangingBranch : MonoBehaviour
    {
        [SerializeField] private Transform actorTransform;
        [SerializeField] private Transform childTransform;
        [SerializeField] private Scaler footScaler;
        [SerializeField] private float stepAngle;
        [SerializeField] private float rotateDuration;
        [SerializeField] private float radius;

        private void Start()
        {
            footScaler.OnScaleStarted += OnScaleStarted;
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            Vector3 targetRotation = actorTransform.localEulerAngles + new Vector3(0f, 0f, (args.CurrentStep - args.PreviousStep) * stepAngle);
            actorTransform.DOLocalRotate(targetRotation, rotateDuration).SetEase(Ease.OutBack);

            Vector3 targetPosition = new Vector3(
                radius * Mathf.Cos(Mathf.Deg2Rad * args.CurrentStep * stepAngle),
                radius * Mathf.Sin(Mathf.Deg2Rad * args.CurrentStep * stepAngle),
                childTransform.localPosition.z
            );
            childTransform.DOLocalMove(targetPosition, rotateDuration).SetEase(Ease.OutBack);
        }
    }
}