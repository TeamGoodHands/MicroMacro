using CoreModule.Input;
using Module.Application.SceneSwitch;
using Module.Management;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;

namespace Module.Application
{
    public class PauseScreen : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition sceneManager;
        [SerializeField] private GameObject pauseScreenUI; // ポーズメニューのUIパネル
        [SerializeField] private Button resumeButton;      // 再開
        [SerializeField] private Button restartButton;     // やり直し
        [SerializeField] private Button stageSelectButton; // ステージセレクトへ戻る
        [SerializeField] private Button titleButton;       // タイトルへ戻る
        
        private bool isPaused = false;
        private InputEvent pauseEvent;
        private InputActionMap playerInput;
        
        private void Start()
        {
            // 各ボタンにリスナーを追加
            resumeButton.onClick.AddListener(ResumeGame);
            restartButton.onClick.AddListener(RestartLevel);
            stageSelectButton.onClick.AddListener(ReturnToStageSelect);
            titleButton.onClick.AddListener(ReturnToTitle);

            // ポーズメニューを非表示にする
            pauseScreenUI.SetActive(false);

            pauseEvent = InputProvider.CreateEvent(ActionGuid.Player.Pause);
            playerInput = InputProvider.GetActionMap(ActionGuid.Player.MapId);
            
            pauseEvent.Started += OnTogglePause;
        }
        
        private void OnDestroy()
        {
            if (pauseEvent == null)
                return;
            
            pauseEvent.Started -= OnTogglePause;
        }

        public bool IsPaused
        {
            private set { isPaused = value; }
            get { return isPaused; }
        }
        public void OnTogglePause(InputAction.CallbackContext _)
        { 
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
            playerInput.Disable();  // プレイヤーの入力切っておく
            pauseScreenUI.SetActive(true);
            resumeButton.Select();
            
            SoundManager.instance.Play("ポーズを開く");
            Time.timeScale = 0f; 
            IsPaused = true;
        }
        
        public void ResumeGame()
        {
            pauseScreenUI.SetActive(false);
            
            playerInput.Enable();
            Time.timeScale = 1f; 
            IsPaused = false;
        }

        public void ReturnToStageSelect()
        {
            Time.timeScale = 1f; 
            sceneManager.StartTransition("StageSelect");
        }

        public void ReturnToTitle()
        {
            Time.timeScale = 1f; 
            sceneManager.StartTransition("Title");
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f; 
            sceneManager.StartTransition(SceneManager.GetActiveScene().name);
        }
        
        public void QuitGame()
        {
            Time.timeScale = 1f;

            // アプリケーション終了（エディタでは動作しない）
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
       
    }
}