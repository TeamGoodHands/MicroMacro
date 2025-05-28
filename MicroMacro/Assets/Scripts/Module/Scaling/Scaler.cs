using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

namespace Module.Scaling
{
    public enum State
    {
        InScale,
        MinScale,
        MaxScale
    }

    /// <summary>
    /// オブジェクトをスケールする基底クラス
    /// </summary>
    public abstract class Scaler : MonoBehaviour
    {
        [SerializeField, Header("最小段階")] protected int minStep = 0;
        [SerializeField, Header("最大段階")] protected int maxStep = 3;
        [SerializeField, Header("現在の段階"), ReadOnly] protected int step;
        [SerializeField, Header("前の段階"), ReadOnly] protected int prevStep;
        [SerializeField, Header("現在のステート"), ReadOnly] protected State state;
        [SerializeField, Header("スケール中か"), ReadOnly] protected bool isScaling;

        /// <summary>
        /// 現在のスケール段階
        /// </summary>
        public int CurrentStep => step;

        /// <summary>
        /// スケール中か
        /// </summary>
        public bool IsScaling => isScaling;

        /// <summary>
        /// スケールしたときに呼ばれるイベント
        /// </summary>
        public event Action<int, State> OnScaled;

        private CancellationTokenSource scaleCanceller;

        /// <summary>
        /// オブジェクトをスケールします
        /// </summary>
        /// <param name="additionalStep">追加段階</param>
        public async UniTaskVoid Scale(int additionalStep)
        {
            // スケール中であればキャンセル
            if (isScaling)
                return;

            isScaling = true;

            // Destroy時のCancellationTokenとスケールのCancellationTokenをマージ
            scaleCanceller = new CancellationTokenSource();
            CancellationToken cancellationToken = MergeDestroyCancellation(scaleCanceller.Token);

            // スケール段階を更新
            prevStep = step;
            step = Mathf.Clamp(step + additionalStep, minStep, maxStep);
            state = GetScaleState();

            // スケール処理を待つ
            await OnScale(cancellationToken);

            isScaling = false;
            scaleCanceller?.Dispose();
            scaleCanceller = null;
            OnScaled?.Invoke(step, state);
        }

        /// <summary>
        /// スケール処理をキャンセルします
        /// </summary>
        public void CancelScale()
        {
            if (!isScaling || scaleCanceller == null)
                return;

            scaleCanceller.Cancel();
            scaleCanceller.Dispose();
            scaleCanceller = null;

            step = prevStep;
            state = GetScaleState();
        }

        protected abstract UniTask OnScale(CancellationToken cancellationToken);

        private CancellationToken MergeDestroyCancellation(CancellationToken scaleCancellationToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, scaleCancellationToken).Token;
        }

        private State GetScaleState()
        {
            if (step == minStep)
                return State.MinScale;

            if (step == maxStep)
                return State.MaxScale;

            return State.InScale;
        }
    }
}