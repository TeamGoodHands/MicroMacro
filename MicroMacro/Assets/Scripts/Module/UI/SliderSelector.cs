using System;
using Module.Management;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Module.UI
{
    public class SliderSelector : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [Header("スライダーのツマミ背景")]
        [SerializeField] private Image handleBackground;
        private Action<bool> OnSelectStateChanged;

        private void Awake()
        {
            if (handleBackground == null)
            {
                Debug.LogError("Sliderのツマミ背景が設定されていません。", this.gameObject);
                return;
            }
            handleBackground.enabled = false;
        }
        
        public void OnSelect(BaseEventData eventData)
        {
            handleBackground.enabled = true;
            OnSelectStateChanged?.Invoke(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            handleBackground.enabled = false;
            OnSelectStateChanged?.Invoke(false);
            SoundManager.instance.Play("ボタンセレクト");  // ポーズ等を開くと同時に鳴らないようこっちで呼ぶ
        }
    }
}