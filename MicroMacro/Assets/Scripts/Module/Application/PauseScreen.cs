using CoreModule.Input;
using CoreModule.Utility;
using DG.Tweening;
using Module.Application.SceneSwitch;
using Module.Management;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Module.Application
{
    public class PauseScreen : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition sceneManager;
        [SerializeField] private GameObject pauseScreenUI;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button stageSelectButton;
        [SerializeField] private Button titleButton;
        
        [SerializeField] private Button VolumeSettingsButton; 
        [SerializeField] private VolumeSettingsScreen volumeSettingsScreen;

        private bool isPaused = false;
        private bool isTransitioning = false;
        private InputEvent pauseEvent;
        private InputActionMap playerInput;
        private Tween bgmFadeTween;
        private float defaultVolume;

        private void Start()
        {
            if (sceneManager == null)
            {
                sceneManager = FindAnyObjectByType<FadeAndSceneTransition>();
            }

            resumeButton.onClick.AddListener(ResumeGame);
            restartButton.onClick.AddListener(RestartLevel);
            stageSelectButton.onClick.AddListener(ReturnToStageSelect);
            titleButton.onClick.AddListener(ReturnToTitle);
            
            VolumeSettingsButton.onClick.AddListener(OpenVolumeSettings);

            pauseScreenUI.SetActive(false);

            pauseEvent = InputProvider.CreateEvent(ActionGuid.UI.Pause);
            playerInput = InputProvider.GetActionMap(ActionGuid.Player.MapId);

            pauseEvent.Started += OnTogglePause;

            audioMixer.GetFloat("BGM", out defaultVolume);
        }

        private void OnDestroy()
        {
            if (pauseEvent == null) return;

            pauseEvent.Started -= OnTogglePause;
            resumeButton.onClick.RemoveListener(ResumeGame);
            restartButton.onClick.RemoveListener(RestartLevel);
            stageSelectButton.onClick.RemoveListener(ReturnToStageSelect);
            titleButton.onClick.RemoveListener(ReturnToTitle);
            VolumeSettingsButton.onClick.RemoveListener(OpenVolumeSettings);
        }

        public bool IsPaused
        {
            get { return isPaused; }
            private set { isPaused = value; }
        }

        public void OnTogglePause(InputAction.CallbackContext _)
        {
            if (isTransitioning) return;

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            if (isTransitioning) return;

            bgmFadeTween?.Kill();
            bgmFadeTween = audioMixer.DOFadeVolume("BGM", 0.5f, 1f).SetUpdate(true);

            playerInput.Disable();
            pauseScreenUI.SetActive(true);
            SoundManager.instance.Play("ポーズ開く");
            Time.timeScale = 0f;
            IsPaused = true;
        }

        public void ResumeGame()
        {
            if (isTransitioning) return;

            // ★注意点: ポーズ中に設定画面でBGM音量を変更した場合、元のdefaultVolumeに戻すと
            // 変更した音量が無効になってしまうため、再取得するようにしています。
            audioMixer.GetFloat("BGM", out defaultVolume); 

            bgmFadeTween?.Kill();
            bgmFadeTween = audioMixer.DOFadeVolume("BGM", AudioMixerExtension.ConvertDecibelToLinear(defaultVolume), 1f).SetUpdate(true);

            pauseScreenUI.SetActive(false);
            SoundManager.instance.Play("ポーズ閉じる");
            playerInput.Enable();
            Time.timeScale = 1f;
            IsPaused = false;
        }

        public void ReturnToStageSelect()
        {
            if (isTransitioning) return; 
            isTransitioning = true;

            bgmFadeTween?.Kill();
            audioMixer.SetFloat("BGM", AudioMixerExtension.ConvertDecibelToLinear(defaultVolume));

            Time.timeScale = 1f;
            sceneManager.StartNormalTransition("StageSelect");
        }

        public void ReturnToTitle()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            bgmFadeTween?.Kill();
            audioMixer.SetFloat("BGM", AudioMixerExtension.ConvertDecibelToLinear(defaultVolume));

            Time.timeScale = 1f;
            sceneManager.StartNormalTransition("Title");
        }

        public void RestartLevel()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            bgmFadeTween?.Kill();
            audioMixer.SetFloat("BGM", AudioMixerExtension.ConvertDecibelToLinear(defaultVolume));

            Time.timeScale = 1f;
            sceneManager.StartNormalTransition(SceneManager.GetActiveScene().name);
        }

       
        public void OpenVolumeSettings()
        {
            if (isTransitioning) return;
            
            if (volumeSettingsScreen != null)
            {
                // 決定音などを鳴らす場合はここに追加
                volumeSettingsScreen.OpenScreen();
            }
            else
            {
                Debug.LogWarning("VolumeSettingsScreen がアサインされていません！");
            }
        }
    }
}