using UnityEngine;
using UnityEngine.EventSystems;
using Module.Management;

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
        [Header("開始時に再生するサウンド")]
        [SerializeField] private string soundNameOnEnabled = null;
        
        private void OnEnable()
        {
            SelectFirstButton();
            if (!string.IsNullOrEmpty(soundNameOnEnabled))
            {
                SoundManager.instance.Play(soundNameOnEnabled);
            }
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