using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Module.UI
{
    public class ScaleButtonAnim : ButtonAnimBase
    {
        [Header("ターゲット設定")]
        [Tooltip("拡大縮小させたい対象（親のRectTransformなどをセット）")]
        [SerializeField] private RectTransform rectTransform;

        [Header("拡大設定 (DOTween)")]
        [Tooltip("選択時の拡大倍率 (例: 1.2 で1.2倍)")]
        [SerializeField] private float activeScale = 1.2f;
        
        [Tooltip("アニメーションにかかる時間（秒）。数値を大きくするとゆっくりになります")]
        [SerializeField] private float duration = 0.4f; // 0.2 -> 0.4 に変更（ゆっくりに）
        
        [SerializeField] private Ease easeType = Ease.OutBack;

        private Vector3 originalScale;
        private CancellationTokenSource cts;

        void Awake()
        {
            // もしアタッチし忘れていたら、自動で自分のやつを使う（保険）
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

            // 初期サイズを記憶
            originalScale = rectTransform.localScale;
        }

        void OnEnable()
        {
            // 有効化時にサイズをリセット
            if (rectTransform != null)
            {
                rectTransform.localScale = originalScale;
            }

            // 最初から選ばれている場合の処理
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == gameObject)
            {
                OnSelect(null);
            }
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        protected override void OnActive()
        {
            // 拡大
            ScaleToTargetAsync(originalScale * activeScale).Forget();
        }

        protected override void OnInactive()
        {
            // 元のサイズに戻す
            ScaleToTargetAsync(originalScale).Forget();
        }

        private async UniTaskVoid ScaleToTargetAsync(Vector3 targetScale)
        {
            if (this == null || gameObject == null || rectTransform == null) return;

            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
            
            rectTransform.DOKill(); // 重複動作防止

            try
            {
                await rectTransform.DOScale(targetScale, duration)
                    .SetEase(easeType)
                    .SetLink(gameObject)
                    .ToUniTask(cancellationToken: cts.Token);
            }
            catch (System.OperationCanceledException)
            {
                // キャンセル時は何もしない
            }
        }
    }
}