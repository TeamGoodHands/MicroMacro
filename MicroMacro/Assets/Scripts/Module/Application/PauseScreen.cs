using CoreModule.Input;
using Module.Application.SceneSwitch;
using Module.Management;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Module.Application
{
    public class PauseScreen : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition sceneManager;
        [SerializeField] private GameObject pauseScreenUI; 
        [SerializeField] private Button resumeButton;      
        [SerializeField] private Button restartButton;     
        [SerializeField] private Button stageSelectButton; 
        [SerializeField] private Button titleButton;       
        [SerializeField] private Button feedbackButton;    
        
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
            
            resumeButton.onClick.AddListener(ResumeGame);
            restartButton.onClick.AddListener(RestartLevel);
            stageSelectButton.onClick.AddListener(ReturnToStageSelect);
            titleButton.onClick.AddListener(ReturnToTitle);
            feedbackButton.onClick.AddListener(Feedback);
            
            pauseScreenUI.SetActive(false);

            pauseEvent = InputProvider.CreateEvent(ActionGuid.UI.Pause);
            playerInput = InputProvider.GetActionMap(ActionGuid.Player.MapId);
            
            pauseEvent.Started += OnTogglePause;
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

            playerInput.Disable();
            pauseScreenUI.SetActive(true);
            SoundManager.instance.Play("ポーズを開く");
            Time.timeScale = 0f; 
            IsPaused = true;
        }
        
        public void ResumeGame()
        {
            if (isTransitioning) return;

            pauseScreenUI.SetActive(false);
            SoundManager.instance.Play("ボタン決定");
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