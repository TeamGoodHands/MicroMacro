using Module.Application.SceneSwitch;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        private void Start()
        {
            // 各ボタンにリスナーを追加
            resumeButton.onClick.AddListener(ResumeGame);
            restartButton.onClick.AddListener(RestartLevel);
            stageSelectButton.onClick.AddListener(ReturnToStageSelect);
            titleButton.onClick.AddListener(ReturnToTitle);

            // ポーズメニューを非表示にする
            pauseScreenUI.SetActive(false);
        }

        private void Update()
        {
            // ESCキーでポーズ切り替え
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
        public void TogglePause()
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
            resumeButton.Select();
            pauseScreenUI.SetActive(true);
            Time.timeScale = 0f; 
            isPaused = true;
        }
        
        public void ResumeGame()
        {
            pauseScreenUI.SetActive(false);
            Time.timeScale = 1f; 
            isPaused = false;
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