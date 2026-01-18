using UnityEngine;
using UnityEngine.EventSystems; 
using DG.Tweening;             
using Cysharp.Threading.Tasks; 
using System.Threading;
using UnityEngine.Serialization;

namespace Module.UI
{
    public class SlideButtonAnim : ButtonAnimBase
    {
        [SerializeField] private RectTransform rectTransform;

        [Header("スライド設定 (DOTween)")]
        [Tooltip("移動距離（画像の向きに合わせて移動します）")]
        [SerializeField] private float slideDistance = 30f;
        
        [Tooltip("動き始める前の待機時間（秒）")]
        [SerializeField] private float delay = 0f;
        
        [Tooltip("アニメーションにかかる時間（秒）")]
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private Ease easeType = Ease.OutQuad;

        private Vector2 originalPosition;
        private CancellationTokenSource cts;

        void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            originalPosition = rectTransform.anchoredPosition;
        }
        
        void OnEnable()
        {
            rectTransform.anchoredPosition = originalPosition;
            
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
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
            // 画像にとっての右方向を計算
            Vector3 direction = rectTransform.localRotation * Vector3.right;

            // 方向ベクトルに距離を掛けて、移動量(オフセット)を算出
            Vector2 offset = (Vector2)(direction * slideDistance);

            // 元の位置に足す
            Vector2 target = originalPosition + offset;

            // 出現時は startDelay を適用する
            MoveToTargetAsync(target, delay).Forget();
        }

        protected override void OnInactive()
        {
            // 元に戻す
            MoveToTargetAsync(originalPosition, delay).Forget();
        }

        // オプションでディレイも可能
        private async UniTaskVoid MoveToTargetAsync(Vector2 target, float delay)
        {
            if (this == null || gameObject == null) return;
            
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            rectTransform.DOKill();

            try
            {
                // SetDelayを追加して待機時間を反映
                await rectTransform.DOAnchorPos(target, duration)
                    .SetDelay(delay)
                    .SetEase(easeType)
                    .SetLink(gameObject)
                    .SetUpdate(true) // ポーズ中も稼働
                    .ToUniTask(cancellationToken: cts.Token);
            }
            catch (System.OperationCanceledException)
            {
            }
        }
    }
}