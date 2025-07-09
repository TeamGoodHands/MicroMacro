using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Module.UI
{
    public class ButtonSelector : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private Image buttonBackground;
        public Action<bool> OnSelectStateChanged;

        private void Awake()
        {
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
        }

        public void OnDeselect(BaseEventData eventData)
        {
            buttonBackground.enabled = false;
            OnSelectStateChanged?.Invoke(false);
        }
    }
}