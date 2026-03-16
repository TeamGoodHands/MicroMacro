using CoreModule.Input;
using Module.Application.SceneSwitch;
using Module.Management;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Module.Application
{
    public class PauseScreen : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private FadeAndSceneTransition sceneManager;
        [SerializeField] private GameObject pauseScreenUI;
        [SerializeField] private AudioMixer audioMixer;
        
        [Header("Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button stageSelectButton;
        [SerializeField] private Button titleButton;

        [Header("Volume Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private string masterParameterName = VolumeManager.ParamMaster;
        
        [Space(10)]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private string bgmParameterName = VolumeManager.ParamBgm;
        
        [Space(10)]
        [SerializeField] private Slider seSlider;
        [SerializeField] private string seParameterName = VolumeManager.ParamSe;

        private bool isPaused = false;
        private bool isTransitioning = false;
        private InputEvent pauseEvent;
        private InputActionMap playerInput;

        private void Start()
        {
            if (sceneManager == null)
            {
                sceneManager = FindAnyObjectByType<FadeAndSceneTransition>();
            }

            // ボタンのリスナー登録
            resumeButton.onClick.AddListener(ResumeGame);
            restartButton.onClick.AddListener(RestartLevel);
            stageSelectButton.onClick.AddListener(ReturnToStageSelect);
            titleButton.onClick.AddListener(ReturnToTitle);

            // スライダーの初期化
            SetupSlider(masterSlider);
            SetupSlider(bgmSlider);
            SetupSlider(seSlider);

            // 保存されている音量を読み込み、スライダーに反映してからリスナーを登録する
            masterSlider.value = VolumeManager.GetVolume(masterParameterName);
            bgmSlider.value = VolumeManager.GetVolume(bgmParameterName);
            seSlider.value = VolumeManager.GetVolume(seParameterName);

            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            bgmSlider.onValueChanged.AddListener(SetBgmVolume);
            seSlider.onValueChanged.AddListener(SetSeVolume);

            pauseScreenUI.SetActive(false);

            // 入力設定
            pauseEvent = InputProvider.CreateEvent(ActionGuid.UI.Pause);
            playerInput = InputProvider.GetActionMap(ActionGuid.Player.MapId);
            pauseEvent.Started += OnTogglePause;
        }

        private void OnDestroy()
        {
            if (pauseEvent != null)
            {
                pauseEvent.Started -= OnTogglePause;
            }
            
            resumeButton.onClick.RemoveListener(ResumeGame);
            restartButton.onClick.RemoveListener(RestartLevel);
            stageSelectButton.onClick.RemoveListener(ReturnToStageSelect);
            titleButton.onClick.RemoveListener(ReturnToTitle);

            masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
            bgmSlider.onValueChanged.RemoveListener(SetBgmVolume);
            seSlider.onValueChanged.RemoveListener(SetSeVolume);
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

            // ポーズ画面を開く直前に、現在の設定値をスライダーの見た目に同期させる
            // (タイトル画面などで変更された値が反映されるようにVolumeManagerから取得)
            masterSlider.value = VolumeManager.GetVolume(masterParameterName);
            bgmSlider.value = VolumeManager.GetVolume(bgmParameterName);
            seSlider.value = VolumeManager.GetVolume(seParameterName);

            playerInput.Disable();
            pauseScreenUI.SetActive(true);
            SoundManager.instance.Play("ポーズ開く");
            Time.timeScale = 0f;
            IsPaused = true;
        }

        public void ResumeGame()
        {
            if (isTransitioning) return;

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
            Time.timeScale = 1f;
            sceneManager.StartNormalTransition("StageSelect");
        }

        public void ReturnToTitle()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            Time.timeScale = 1f;
            sceneManager.StartNormalTransition("Title");
        }

        public void RestartLevel()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            Time.timeScale = 1f;
            sceneManager.StartNormalTransition(SceneManager.GetActiveScene().name);
        }

        private void SetupSlider(Slider slider)
        {
            if (slider == null) return;
            slider.minValue = 0.0001f;
            slider.maxValue = 1f;
        }

        // 実際の変換・設定・保存処理は全て VolumeManager に任せる
        private void SetMasterVolume(float linearVolume) => VolumeManager.SetVolume(audioMixer, masterParameterName, linearVolume);
        private void SetBgmVolume(float linearVolume) => VolumeManager.SetVolume(audioMixer, bgmParameterName, linearVolume);
        private void SetSeVolume(float linearVolume) => VolumeManager.SetVolume(audioMixer, seParameterName, linearVolume);
    }
}