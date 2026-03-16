using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using DG.Tweening;
using Module.Management;

namespace Module.Application
{
    public class TitleSettingsScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject settingsPanel; 
        [SerializeField] private AudioMixer audioMixer;
        
        [Header("Buttons & Focus")]
        [SerializeField] private Button openButton;  
        [SerializeField] private Button closeButton; 
        // 修正：最初に選択させたいオブジェクトをインスペクターで設定できるように変更
        [SerializeField] private GameObject firstSelectedObject; 

        [Header("Volume Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private string masterParameterName = VolumeManager.ParamMaster;
        
        [Space(10)]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private string bgmParameterName = VolumeManager.ParamBgm;
        
        [Space(10)]
        [SerializeField] private Slider seSlider;
        [SerializeField] private string seParameterName = VolumeManager.ParamSe;

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private Ease closeEase = Ease.InBack;

        private GameObject previouslySelectedButton;

        private void Start()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
                settingsPanel.transform.localScale = Vector3.zero;
            }

            if (openButton != null) openButton.onClick.AddListener(OpenSettings);
            if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);

            SetupSlider(masterSlider);
            SetupSlider(bgmSlider);
            SetupSlider(seSlider);

            if (masterSlider != null) masterSlider.value = VolumeManager.GetVolume(masterParameterName);
            if (bgmSlider != null) bgmSlider.value = VolumeManager.GetVolume(bgmParameterName);
            if (seSlider != null) seSlider.value = VolumeManager.GetVolume(seParameterName);

            if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
            if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBgmVolume);
            if (seSlider != null) seSlider.onValueChanged.AddListener(SetSeVolume);
        }

        private void OnDestroy()
        {
            if (openButton != null) openButton.onClick.RemoveListener(OpenSettings);
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseSettings);

            if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
            if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(SetBgmVolume);
            if (seSlider != null) seSlider.onValueChanged.RemoveListener(SetSeVolume);
            
            if (settingsPanel != null)
            {
                settingsPanel.transform.DOKill();
            }
        }

        public void OpenSettings()
        {
            if (EventSystem.current != null)
            {
                previouslySelectedButton = EventSystem.current.currentSelectedGameObject;
            }

            if (masterSlider != null) masterSlider.value = VolumeManager.GetVolume(masterParameterName);
            if (bgmSlider != null) bgmSlider.value = VolumeManager.GetVolume(bgmParameterName);
            if (seSlider != null) seSlider.value = VolumeManager.GetVolume(seParameterName);

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                
                settingsPanel.transform.DOKill();
                settingsPanel.transform.localScale = Vector3.zero;
                settingsPanel.transform.DOScale(Vector3.one, animationDuration)
                    .SetEase(openEase)
                    .SetUpdate(true); 
            }
            
            // スライダー固定ではなく、インスペクターで指定したオブジェクト（閉じるボタン等）を選択する
            if (EventSystem.current != null && firstSelectedObject != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelectedObject);
            }
            
            SoundManager.instance.Play("ポーズ開く");
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.transform.DOKill();
                settingsPanel.transform.DOScale(Vector3.zero, animationDuration)
                    .SetEase(closeEase)
                    .SetUpdate(true)
                    .OnComplete(() => 
                    {
                        settingsPanel.SetActive(false);

                        if (EventSystem.current != null && previouslySelectedButton != null && previouslySelectedButton.activeInHierarchy)
                        {
                            EventSystem.current.SetSelectedGameObject(null);
                            EventSystem.current.SetSelectedGameObject(previouslySelectedButton);
                        }
                    });
            }
            
            SoundManager.instance.Play("ポーズ閉じる");
        }

        private void SetupSlider(Slider slider)
        {
            if (slider == null) return;
            slider.minValue = 0.0001f;
            slider.maxValue = 1f;
        }

        private void SetMasterVolume(float linearVolume) => VolumeManager.SetVolume(audioMixer, masterParameterName, linearVolume);
        private void SetBgmVolume(float linearVolume) => VolumeManager.SetVolume(audioMixer, bgmParameterName, linearVolume);
        private void SetSeVolume(float linearVolume) => VolumeManager.SetVolume(audioMixer, seParameterName, linearVolume);
    }
}