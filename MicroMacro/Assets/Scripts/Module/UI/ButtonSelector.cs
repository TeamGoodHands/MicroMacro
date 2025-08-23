using System;
using Module.Management;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Module.UI
{
    public class ButtonSelector : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private Image buttonBackground;
        private Button button;
        public Action<bool> OnSelectStateChanged;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
            
            if (transform.parent != null)
            {
                buttonBackground = transform.parent.GetComponent<Image>();
            }

            if (buttonBackground == null)
            {
                Debug.Log("ButtonSelector: buttonBackground is null");
                return;
            }

            buttonBackground.enabled = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            buttonBackground.enabled = true;
            OnSelectStateChanged?.Invoke(true);
            SoundManager.instance.Play("ボタンセレクト");
        }

        public void OnDeselect(BaseEventData eventData)
        {
            buttonBackground.enabled = false;
            OnSelectStateChanged?.Invoke(false);
        }

        public void OnButtonClick()
        {
            SoundManager.instance.Play("ボタン決定");
        }
    }
}