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

    /// <summary>
    /// 再開イベントのデリゲート
    /// <param name="isForwards">再開する際に目標のスケール先に拡大するか(ポーズ後に元のサイズに戻る場合はfalse)</param>
    /// <param name="isMacro">再開する際に大きくなるか</param>>
    /// </summary>
    public delegate void ResumedEvent(bool isForwards, bool isMacro);

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
        [SerializeField, Header("ポーズ中か"), ReadOnly] protected bool isPause;

        /// <summary>
        /// 現在のスケール段階
        /// </summary>
        public int CurrentStep => currentStep;

        /// <summary>
        /// 前のスケール段階
        /// </summary>
        public int PreviousStep => previousStep;

        /// <summary>
        /// 拡大縮小のステート
        /// </summary>
        public State State => state;

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
        /// ポーズ中か
        /// </summary>
        public bool IsPause => isPause;

        /// <summary>
        /// スケール開始したときに呼ばれるイベント
        /// </summary>
        public event ScaledEvent OnScaleStarted;

        /// <summary>
        /// スケール完了したときに呼ばれるイベント
        /// </summary>
        public event ScaledEvent OnScaleCompleted;

        /// <summary>
        /// スケールを一時停止したときに呼ばれるイベント
        /// </summary>
        public event Action OnScalePaused;

        /// <summary>
        /// スケールを再開したときに呼ばれるイベント
        /// </summary>
        public event ResumedEvent OnScaleResumed;

        private CancellationTokenSource scaleCanceller;
        private ScaleEventArgs previousScaleInfo;

        /// <summary>
        /// オブジェクトを追加スケールします
        /// </summary>
        /// <param name="additionalStep">追加段階</param>
        /// <param name="forceScale"></param>
        public async UniTaskVoid Scale(int additionalStep, bool forceScale = false)
        {
            // ポーズ中の場合はそれを再開する
            if (isPause)
            {
                bool isMacro = additionalStep > 0;
                bool isForwards = isMacro == CurrentStep > PreviousStep;
                ResumeScale(isForwards, isMacro);
                return;
            }

            // コンポーネントが無効 or スケール中であればキャンセル
            if ((!enabled && !forceScale) || isScaling)
                return;

            // 過去のスケール情報を保存
            previousScaleInfo = new ScaleEventArgs(currentStep, previousStep, 0f, state);

            int nextStep = Mathf.Clamp(currentStep + additionalStep, minStep, maxStep);

            // スケール段階を更新
            previousStep = currentStep;
            currentStep = nextStep;
            state = GetScaleState();

            ScaleEventArgs args = new ScaleEventArgs(currentStep, previousStep, 0f, state);
            OnScaleStarted?.Invoke(args);

            // 同じスケールになる場合はスケールしない
            if (previousStep == nextStep)
                return;

            isScaling = true;

            // Destroy時のCancellationTokenとスケールのCancellationTokenをマージ
            scaleCanceller = new CancellationTokenSource();
            CancellationToken cancellationToken = MergeDestroyCancellation(scaleCanceller.Token);

            // スケール処理を待つ
            await OnScale(cancellationToken);

            isScaling = false;
            isPause = false;
            scaleCanceller?.Dispose();
            scaleCanceller = null;

            // スケール完了イベントを送信
            args = new ScaleEventArgs(currentStep, previousStep, 0f, state);
            OnScaleCompleted?.Invoke(args);
        }

        public void ScaleImmediate(int additionalStep, bool forceScale)
        {
            // ポーズ中の場合はそれを再開する
            if (isPause)
            {
                bool isMacro = additionalStep > 0;
                bool isForwards = isMacro == CurrentStep > PreviousStep;
                ResumeScale(isForwards, isMacro);
                return;
            }

            // コンポーネントが無効 or スケール中であればキャンセル
            if ((!enabled && !forceScale) || isScaling)
                return;

            // 過去のスケール情報を保存
            previousScaleInfo = new ScaleEventArgs(currentStep, previousStep, 0f, state);

            int nextStep = Mathf.Clamp(currentStep + additionalStep, minStep, maxStep);

            // スケール段階を更新
            previousStep = currentStep;
            currentStep = nextStep;
            state = GetScaleState();

            ScaleEventArgs args = new ScaleEventArgs(currentStep, previousStep, 0f, state);
            OnScaleStarted?.Invoke(args);

            // 同じスケールになる場合はスケールしない
            if (previousStep == nextStep)
                return;

            // スケールする
            OnScaleImmediate();

            isPause = false;

            // スケール完了イベントを送信
            args = new ScaleEventArgs(currentStep, previousStep, 0f, state);
            OnScaleCompleted?.Invoke(args);
        }

        /// <summary>
        /// 指定スケールにセットします
        /// </summary>
        /// <param name="step">指定段階</param>
        /// <param name="forceScale"></param>
        public UniTaskVoid SetScale(int step, bool forceScale = false)
        {
            int targetStep = Mathf.Clamp(step, minStep, maxStep);
            int scaleDiff = targetStep - CurrentStep;
            return Scale(scaleDiff, forceScale);
        }
        
        public void SetScaleImmediate(int step, bool forceScale = false)
        {
            int targetStep = Mathf.Clamp(step, minStep, maxStep);
            int scaleDiff = targetStep - CurrentStep;
            ScaleImmediate(scaleDiff, forceScale);
        }

        /// <summary>
        /// スケール処理を再開します
        /// </summary>
        /// <param name="isForwards">元の目標スケールへ再開するか</param>
        public void Resume(bool isForwards)
        {
            // ポーズしてない場合は再開しない
            if (!isPause)
                return;

            int stepDiff = currentStep - previousStep;
            bool isMacro = isForwards ? stepDiff > 0 : stepDiff < 0;

            ResumeScale(isForwards, isMacro);
        }

        /// <summary>
        /// スケール処理を一時停止します
        /// </summary>
        public void Pause()
        {
            if (!isScaling)
                return;

            isPause = true;
            OnPause();
            OnScalePaused?.Invoke();
        }


        private void ResumeScale(bool isForwards, bool isMacro)
        {
            isPause = false;

            // 再開する際に再開前のサイズに戻る場合は、データも元に戻す
            if (!isForwards)
            {
                previousStep = previousScaleInfo.PreviousStep;
                currentStep = previousScaleInfo.CurrentStep;
                state = previousScaleInfo.State;
            }

            OnResume(isForwards);
            OnScaleResumed?.Invoke(isForwards, isMacro);
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
            isScaling = false;
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
            SetScale(0, true).Forget();
        }

        protected abstract UniTask OnScale(CancellationToken cancellationToken);
        protected abstract void OnScaleImmediate();

        protected abstract void OnPause();
        protected abstract void OnResume(bool isForwards);

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