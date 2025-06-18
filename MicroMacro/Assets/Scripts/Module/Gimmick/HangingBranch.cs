using DG.Tweening;
using Module.Scaling;
using UnityEngine;

namespace Module.Gimmick
{
    public class HangingBranch : MonoBehaviour
    {
        [SerializeField] private Transform actorTransform;
        [SerializeField] private Rigidbody childRigidbody;
        [SerializeField] private Scaler footScaler;
        [SerializeField] private float stepAngle;
        [SerializeField] private float rotateDuration;
        [SerializeField] private float radius;

        private void Start()
        {
            footScaler.OnScaleCompleted += OnScaleStarted;
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            Vector3 rotationAmount = new Vector3(0f, 0f, (args.CurrentStep - args.PreviousStep) * stepAngle);
            Vector3 targetRotation = actorTransform.localEulerAngles + rotationAmount;
            actorTransform.DOLocalRotate(targetRotation, rotateDuration).SetEase(Ease.OutBack);

            Vector2 targetPosition = new Vector2(
                radius * Mathf.Cos(Mathf.Deg2Rad * args.CurrentStep * stepAngle),
                radius * Mathf.Sin(Mathf.Deg2Rad * args.CurrentStep * stepAngle)
            );

            childRigidbody.DOMove(transform.TransformPoint(targetPosition), rotateDuration).SetEase(Ease.OutBack);
        }
    }
}