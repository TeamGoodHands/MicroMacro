using UnityEngine;
using UnityEngine.EventSystems;

namespace Module.UI
{
    /// <summary>
    /// マウスオーバー時にそのオブジェクトを選択状態(Select)にするクラス
    /// ボタン等のインタラクティブなUI要素にアタッチして使用
    /// </summary>
    public class SelectOnHover : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
            }
        }
    }
}