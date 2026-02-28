using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using DG.Tweening;

namespace Module.Application
{
    public class VolumeSettingsScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject volumeScreenUI; 
        [SerializeField] private RectTransform windowRect;
        [SerializeField] private Button closeButton;

        [Header("Audio Settings")]
        [SerializeField] private AudioMixer audioMixer;
        
        [Space(10)]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private string masterParameterName = "Master";
        
        [Space(10)]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private string bgmParameterName = "BGM";
        
        [Space(10)]
        [SerializeField] private Slider seSlider;
        [SerializeField] private string seParameterName = "SE";

        private void Awake()
        {
            SetupSlider(masterSlider);
            SetupSlider(bgmSlider);
            SetupSlider(seSlider);

            closeButton.onClick.AddListener(CloseScreen);
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            bgmSlider.onValueChanged.AddListener(SetBgmVolume);
            seSlider.onValueChanged.AddListener(SetSeVolume);
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(CloseScreen);
            masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
            bgmSlider.onValueChanged.RemoveListener(SetBgmVolume);
            seSlider.onValueChanged.RemoveListener(SetSeVolume);
        }

        private void SetupSlider(Slider slider)
        {
            if (slider == null) return;
            slider.minValue = 0.0001f;
            slider.maxValue = 1f;
        }

        public void OpenScreen()
        {
            // UIのGameObject自体をアクティブにする
            volumeScreenUI.SetActive(true);
            
            SyncSliderWithMixer(masterSlider, masterParameterName);
            SyncSliderWithMixer(bgmSlider, bgmParameterName);
            SyncSliderWithMixer(seSlider, seParameterName);

            // スケールアニメーションでポップアップ表示
            windowRect.localScale = Vector3.one * 0.8f;
            windowRect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void CloseScreen()
        {
            // 少し縮小するアニメーションの後に非アクティブにする
            windowRect.DOScale(Vector3.one * 0.8f, 0.2f).SetUpdate(true).OnComplete(() => 
            {
                volumeScreenUI.SetActive(false);
            });
        }

        private void SyncSliderWithMixer(Slider slider, string parameterName)
        {
            if (slider == null) return;
            if (audioMixer.GetFloat(parameterName, out float currentDb))
            {
                slider.value = Mathf.Pow(10f, currentDb / 20f);
            }
        }

        private void SetMasterVolume(float linearVolume) => SetVolume(masterParameterName, linearVolume);
        private void SetBgmVolume(float linearVolume) => SetVolume(bgmParameterName, linearVolume);
        private void SetSeVolume(float linearVolume) => SetVolume(seParameterName, linearVolume);

        private void SetVolume(string parameterName, float linearVolume)
        {
            float decibel = 20.0f * Mathf.Log10(linearVolume);
            audioMixer.SetFloat(parameterName, decibel);
        }
    }
}