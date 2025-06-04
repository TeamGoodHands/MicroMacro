using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Module.Scaling
{
    public class PencilScaler : Scaler
    {
        [SerializeField, Header("1ステップあたりのスケール量")] private float scaleAmount;
        [SerializeField, Header("スケール時間")] private float scaleDuration;
        [SerializeField, Header("本体のボーン")] private Transform[] bodyBones;
        [SerializeField, Header("先端のボーン")] private Transform headBone;
        [SerializeField, Header("本体のコライダー")] private BoxCollider bodyCollider;

        protected override async UniTask OnScale(CancellationToken cancellationToken)
        {
            int index = -currentStep - 1;

            if (0 > index || index >= bodyBones.Length)
            {
                return;
            }

            Transform target = bodyBones[index];
            // 前のステップからの差分をスケール量とする
            float amount = scaleAmount * (currentStep - previousStep);
            headBone.DOLocalMoveY(headBone.localPosition.y + amount, scaleDuration).SetEase(Ease.OutBack);
            DOTween.To(() => bodyCollider.center,
                    value => bodyCollider.center = value,
                    new Vector3(bodyCollider.center.x, bodyCollider.center.y + amount * 50f, bodyCollider.center.z),
                    scaleDuration)
                .SetEase(Ease.OutBack);

            DOTween.To(() => bodyCollider.size,
                    value => bodyCollider.size = value,
                    new Vector3(bodyCollider.size.x, bodyCollider.size.y + amount * 100f, bodyCollider.size.z),
                    scaleDuration)
                .SetEase(Ease.OutBack);

            await target.DOScaleY(0f, scaleDuration).SetEase(Ease.OutBack);
        }
    }
}