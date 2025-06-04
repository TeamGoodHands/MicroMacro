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

        /// <summary>
        /// スケールイベントの情報を指定された値で初期化します。
        /// </summary>
        /// <param name="currentStep">現在のスケールステップ。</param>
        /// <param name="previousStep">直前のスケールステップ。</param>
        /// <param name="duration">スケール操作の所要時間（秒）。</param>
        /// <param name="state">現在のスケール状態。</param>
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
        /// <summary>
        /// 指定した段階だけオブジェクトのスケールを非同期で変更します。
        /// </summary>
        /// <param name="additionalStep">現在のスケール段階に加算する値。</param>
        /// <remarks>
        /// スケール中は再度呼び出しても無効です。スケール開始時と完了時にイベントが発火します。
        /// </remarks>
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
        /// <summary>
        /// 現在進行中のスケール操作をキャンセルし、スケール状態とステップを直前の値に戻します。
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

        /// <summary>
        /// 現在のスケールステップに基づいてスケール状態を返します。
        /// </summary>
        /// <returns>最小値の場合は <see cref="State.MinScale"/>、最大値の場合は <see cref="State.MaxScale"/>、それ以外は <see cref="State.InScale"/> を返します。</returns>
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