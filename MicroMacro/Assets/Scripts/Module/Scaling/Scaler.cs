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
    /// スケールイベントのデリゲート
    /// </summary>
    public delegate void ScaledEvent(ScaleEventArgs args);

    public readonly struct ScaleEventArgs
    {
        public readonly int CurrentStep;
        public readonly int PreviousStep;
        public readonly float Duration;
        public readonly State State;

        public ScaleEventArgs(int currentStep, int previousStep, float duration, State state)
        {
            CurrentStep = currentStep;
            PreviousStep = previousStep;
            Duration = duration;
            State = state;
        }
    }

    /// <summary>
    /// オブジェクトをスケールする基底クラス
    /// </summary>
    public abstract class Scaler : MonoBehaviour
    {
        [SerializeField, Header("最小段階")] protected int minStep = 0;
        [SerializeField, Header("最大段階")] protected int maxStep = 3;
        [SerializeField, Header("現在の段階"), ReadOnly] protected int currentStep;
        [SerializeField, Header("前の段階"), ReadOnly] protected int previousStep;
        [SerializeField, Header("現在のステート"), ReadOnly] protected State state;
        [SerializeField, Header("スケール中か"), ReadOnly] protected bool isScaling;

        /// <summary>
        /// 現在のスケール段階
        /// </summary>
        public int CurrentStep => currentStep;

        /// <summary>
        /// 最大のスケール段階
        /// </summary>
        public int MaxStep => maxStep;

        /// <summary>
        /// 最小のスケール段階
        /// </summary>
        public int MinStep => minStep;

        /// <summary>
        /// スケール中か
        /// </summary>
        public bool IsScaling => isScaling;

        /// <summary>
        /// スケール開始したときに呼ばれるイベント
        /// </summary>
        public event ScaledEvent OnScaleStarted;

        /// <summary>
        /// スケール完了したときに呼ばれるイベント
        /// </summary>
        public event ScaledEvent OnScaleCompleted;

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
            previousStep = currentStep;
            currentStep = Mathf.Clamp(currentStep + additionalStep, minStep, maxStep);
            state = GetScaleState();

            // スケール開始イベントを送信
            var args = new ScaleEventArgs(currentStep, previousStep, 0f, state);
            OnScaleStarted?.Invoke(args);

            // スケール処理を待つ
            await OnScale(cancellationToken);

            isScaling = false;
            scaleCanceller?.Dispose();
            scaleCanceller = null;

            // スケール完了イベントを送信
            OnScaleCompleted?.Invoke(args);
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

            currentStep = previousStep;
            state = GetScaleState();
        }

        protected abstract UniTask OnScale(CancellationToken cancellationToken);

        private CancellationToken MergeDestroyCancellation(CancellationToken scaleCancellationToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, scaleCancellationToken).Token;
        }

        private State GetScaleState()
        {
            if (currentStep == minStep)
                return State.MinScale;

            if (currentStep == maxStep)
                return State.MaxScale;

            return State.InScale;
        }
    }
}