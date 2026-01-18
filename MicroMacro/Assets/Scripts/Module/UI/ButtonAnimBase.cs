using UnityEngine;
using UnityEngine.EventSystems;

namespace Module.UI
{
    public abstract class ButtonAnimBase : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool isHovered = false;
        private bool isSelected = false;

        // 子で具体的に実装させるメソッド
        protected abstract void OnActive();   // 選択orホバーされた時の動き
        protected abstract void OnInactive(); // 外れた時の動き

        // 状態更新ロジック
        private void UpdateVisuals()
        {
            // マウスが乗っているor選択されているならActive
            if (isHovered || isSelected)
            {
                OnActive();
            }
            else
            {
                OnInactive();
            }
        }

        // 共通部分
        public void OnSelect(BaseEventData eventData)
        {
            isSelected = true;
            UpdateVisuals();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isSelected = false;
            UpdateVisuals();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateVisuals();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            UpdateVisuals();
        }

        // 無効化時にフラグと見た目をリセット
        protected virtual void OnDisable()
        {
            isHovered = false;
            isSelected = false;
        }
    }
}