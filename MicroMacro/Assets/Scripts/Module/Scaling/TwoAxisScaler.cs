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
        [SerializeField, Header("1ステップあたりのスケール量")] private Vector2 scaleAmount;
        [SerializeField, Header("スケール時間")] private float scaleDuration;

        protected override async UniTask OnScale(CancellationToken cancellationToken)
        {
            // 前のステップからの差分をスケール量とする
            Vector3 amount = (Vector3)scaleAmount * (step - prevStep);
            await transform.DOScale(transform.localScale + amount, scaleDuration).SetEase(Ease.OutBack);
        }
    }
}