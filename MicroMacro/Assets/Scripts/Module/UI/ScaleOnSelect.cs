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
        [Header("Animation Settings")]
        [SerializeField, Tooltip("選択時の拡大率（1.1なら1.1倍）")] 
        private float targetScale = 1.1f;
        
        [SerializeField, Tooltip("アニメーションにかかる時間（秒）")] 
        private float duration = 0.2f;

        private RectTransform rectTransform;
        private Vector3 defaultScale;
        private Tween scaleTween;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            defaultScale = rectTransform.localScale;
        }

        private void OnDisable()
        {
            // オブジェクトが非アクティブになった際（ポーズ画面を閉じた時など）に
            // スケールとアニメーションを初期状態にリセットする
            scaleTween?.Kill();
            rectTransform.localScale = defaultScale;
        }

        public void OnSelect(BaseEventData eventData)
        {
            scaleTween?.Kill();
            
            // ポーズ中(TimeScale = 0)でも動くように SetUpdate(true) を指定
            // OutBackで少し弾むように
            scaleTween = rectTransform.DOScale(defaultScale * targetScale, duration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            scaleTween?.Kill();
            
            // 選択が外れたら元のサイズに戻す
            scaleTween = rectTransform.DOScale(defaultScale, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }
    }
}