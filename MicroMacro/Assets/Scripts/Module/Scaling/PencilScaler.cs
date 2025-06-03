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

        protected override async UniTask OnScale(CancellationToken cancellationToken)
        {
            if (currentStep < 0 || currentStep >= bodyBones.Length)
            {
                Debug.LogError($"Invalid step value: {currentStep}");
                return;
            }

            Transform target = bodyBones[-currentStep - 1];
            // 前のステップからの差分をスケール量とする
            float amount = scaleAmount * (currentStep - previousStep);
            headBone.DOLocalMoveY(headBone.localPosition.y + amount, scaleDuration).SetEase(Ease.OutBack);
            await target.DOScaleY(0f, scaleDuration).SetEase(Ease.OutBack);
        }
    }
}