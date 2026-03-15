using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Module.UI
{
    /// <summary>
    /// UI要素が選択された時にDOTweenでスケールを拡大するクラス
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ScaleOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [Header("Target Settings")]
        [SerializeField, Tooltip("アニメーション対象のRectTransform（未指定なら自身を取得）")]
        private RectTransform targetRect;

        [Header("Animation Settings")]
        [SerializeField, Tooltip("選択時の拡大率（1.1なら1.1倍）")] 
        private float targetScale = 1.1f;
        
        [SerializeField, Tooltip("アニメーションにかかる時間（秒）")] 
        private float duration = 0.2f;

        private Vector3 defaultScale;
        private Tween scaleTween;

        private void Awake()
        {
            // インスペクターで指定がない場合は、自身のRectTransformを使用
            if (targetRect == null)
            {
                targetRect = GetComponent<RectTransform>();
            }
            
            defaultScale = targetRect.localScale;
        }

        private void OnDisable()
        {
            // オブジェクトが非アクティブになった際（ポーズ画面を閉じた時など）に
            // スケールとアニメーションを初期状態にリセットする
            scaleTween?.Kill();
            if (targetRect != null)
            {
                targetRect.localScale = defaultScale;
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (targetRect == null) return;

            scaleTween?.Kill();
            
            // ポーズ中(TimeScale = 0)でも動くように SetUpdate(true) を指定
            scaleTween = targetRect.DOScale(defaultScale * targetScale, duration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (targetRect == null) return;

            scaleTween?.Kill();
            
            // 元のサイズに戻す
            scaleTween = targetRect.DOScale(defaultScale, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }
    }
}