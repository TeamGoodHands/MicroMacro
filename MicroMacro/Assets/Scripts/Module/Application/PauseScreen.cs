using CoreModule.Input;
using CoreModule.Utility;
using DG.Tweening;
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
        [SerializeField] private FadeAndSceneTransition sceneManager;
        [SerializeField] private GameObject pauseScreenUI;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button stageSelectButton;
        [SerializeField] private Button titleButton;
        [SerializeField] private Button feedbackButton;

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
            feedbackButton.onClick.AddListener(Feedback);

            pauseScreenUI.SetActive(false);

            pauseEvent = InputProvider.CreateEvent(ActionGuid.UI.Pause);
            playerInput = InputProvider.GetActionMap(ActionGuid.Player.MapId);

            pauseEvent.Started += OnTogglePause;
            
            audioMixer.GetFloat("BGM", out defaultVolume) ;
        }

        private void OnDestroy()
        {
            if (pauseEvent == null) return;

            pauseEvent.Started -= OnTogglePause;
            resumeButton.onClick.RemoveListener(ResumeGame);
            restartButton.onClick.RemoveListener(RestartLevel);
            stageSelectButton.onClick.RemoveListener(ReturnToStageSelect);
            titleButton.onClick.RemoveListener(ReturnToTitle);
            feedbackButton.onClick.RemoveListener(Feedback);
        }

        public bool IsPaused
        {
            get { return isPaused; }
            private set { isPaused = value; }
        }

        // InputSystemのコールバック
        public void OnTogglePause(InputAction.CallbackContext _)
        {
            // 遷移中は操作を受け付けない
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
            Debug.Log("PauseGame called");

            playerInput.Disable();
            pauseScreenUI.SetActive(true);
            SoundManager.instance.Play("ポーズ開く");
            Time.timeScale = 0f;
            IsPaused = true;
        }

        public void ResumeGame()
        {
            if (isTransitioning) return;

            bgmFadeTween?.Kill();
            bgmFadeTween = audioMixer.DOFadeVolume("BGM", AudioMixerExtension.ConvertDecibelToLinear(defaultVolume), 1f).SetUpdate(true);
            Debug.Log("ResumeGame called");

            pauseScreenUI.SetActive(false);
            SoundManager.instance.Play("ポーズ閉じる");
            playerInput.Enable();
            Time.timeScale = 1f;
            IsPaused = false;
        }

        public void ReturnToStageSelect()
        {
            if (isTransitioning) return; // 連打防止
            isTransitioning = true;

            Time.timeScale = 1f;
            sceneManager.StartTransition("StageSelect");
        }

        public void ReturnToTitle()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            Time.timeScale = 1f;
            sceneManager.StartTransition("Title");
        }

        public void RestartLevel()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            Time.timeScale = 1f;
            sceneManager.StartTransition(SceneManager.GetActiveScene().name);
        }

        public void Feedback()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            Time.timeScale = 1f;
            sceneManager.StartTransition("Feedback");
        }
    }
}