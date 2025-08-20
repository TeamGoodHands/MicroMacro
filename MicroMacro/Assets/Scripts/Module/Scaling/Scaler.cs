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
        [SerializeField, Header("最小段階")] int minStep = 0;
        [SerializeField, Header("最大段階")] int maxStep = 3;
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
        public int MaxStep
        {
            get { return maxStep; }
            set
            {
                if (!IsScaling)
                {
                    maxStep = value;
                }
                else
                {
                    Debug.LogError("スケール中にmaxStepを変更することはできません。");
                }
            }
        }

        /// <summary>
        /// 最小のスケール段階
        /// </summary>
        public int MinStep
        {
            get { return minStep; }
            set
            {
                if (!IsScaling)
                {
                    minStep = value;
                }
                else
                {
                    Debug.LogError("スケール中にminStepを変更することはできません。");
                }
            }
        }

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
        /// 指定スケールにセットします
        /// </summary>
        /// <param name="step">指定段階</param>
        public UniTaskVoid SetScale(int step)
        {
            int targetStep = Mathf.Clamp(step, minStep, maxStep);
            int scaleDiff = targetStep - currentStep;
            return Scale(scaleDiff);
        }
        
        /// <summary>
        /// オブジェクトを追加スケールします
        /// </summary>
        /// <param name="additionalStep">追加段階</param>
        public async UniTaskVoid Scale(int additionalStep)
        {
            // コンポーネントが無効 or スケール中であればキャンセル
            if (!enabled || isScaling)
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

        /// <summary>
        /// スケールを初期ステップに戻します
        /// </summary>
        public void ResetScale()
        {
            // 現在のスケール処理をキャンセル
            scaleCanceller?.Cancel();
            scaleCanceller?.Dispose();
            scaleCanceller = null;
            
            // スケールを初期値に戻す
            SetScale(0).Forget();
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