using UnityEngine;
using UnityEngine.EventSystems;

namespace Module.UI
{
    /// <summary>
    /// メニュー表示時に、指定したボタンを初期選択状態にするクラス
    /// CanvasやPanelのルートオブジェクトにアタッチして使用
    /// </summary>
    public class MenuInitialSelector : MonoBehaviour
    {
        [Header("最初に選択状態にしたいボタン")]
        [SerializeField] private GameObject firstSelectButton;

        private void OnEnable()
        {
            SelectFirstButton();
        }

        private void SelectFirstButton()
        {
            if (firstSelectButton != null && EventSystem.current != null)
            {
                // 一旦選択を解除してから再設定することで、確実にOnSelectを走らせる
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelectButton); 
            }
        }
    }
}