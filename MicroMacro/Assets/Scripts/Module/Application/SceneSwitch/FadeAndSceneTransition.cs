using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Module.Application.SceneSwitch
{
    public class FadeAndSceneTransition : MonoBehaviour
    {
        [Header("フェードCanvasのPrefab")]
        [SerializeField] GameObject fadeCanvasPrefab;
        [Header("移動するシーンの名前")]
        public string nextSceneName;

        private static GameObject faderObj;
        private static IFadeHandler fadeHandler;
        private bool isSceneTransitioning;

        private void Awake()
        {
            if (faderObj == null)
            {
                CreateAndRegisterFader();
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// FadeCanvasの生成、永続化
        /// </summary>
        private void CreateAndRegisterFader()
        {
            if (fadeCanvasPrefab == null)
            {
                Debug.LogError("FadeCanvasがアタッチされていません"); 
                return;
            }

            faderObj = Instantiate(fadeCanvasPrefab);
            DontDestroyOnLoad(faderObj);

            // faderがIFadeHandlerを実装していればIFadeHandler型に変換して代入
            fadeHandler = faderObj.GetComponentInChildren<IFadeHandler>();
            if (fadeHandler == null)
            {
                Debug.LogError("FadeCanvasにIFadeHandler実装がありません");
            }
        }

        /// <summary>
        /// シーン切り替え開始（フェードアウト → ロード → フェードイン）
        /// </summary>
        public void StartTransition()
        {
            if (isSceneTransitioning || fadeHandler == null)
                return;

            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError("次のシーン名が設定されていません。");
                return;
            }

            isSceneTransitioning = true;

            // サウンド停止等あれば
            // SoundManager.instance.StopAllSound();

            fadeHandler.StartFadeOut();
            StartCoroutine(LoadNextSceneAsync());
        }
        
        
        public void StartTransitionSame()
        {
            if (isSceneTransitioning || fadeHandler == null)
                return;

            nextSceneName = SceneManager.GetActiveScene().name;

            isSceneTransitioning = true;

            // サウンド停止等あれば
            // SoundManager.instance.StopAllSound();

            fadeHandler.StartFadeOut();
            StartCoroutine(LoadNextSceneAsync());
        }

        /// <summary>
        /// 名前指定してシーン移動したい場合
        /// </summary>
        public void StartTransition(string SceneName)
        {
            nextSceneName = SceneName;
            StartTransition();
        }

        /// <summary>
        /// フェードアウト完了を待ってシーンを非同期ロード
        /// </summary>
        private IEnumerator LoadNextSceneAsync()
        {
            // 同時に呼ばれたUpdateを反映させるため1フレ待機 (多分)
            yield return null;

            while (fadeHandler.IsFadeOutComplete() == false)
            {
                yield return null;
            }

            // LoadSceneMode.Singleは現在のシーンを自動アンロードしてくれる
            yield return SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (fadeHandler != null)
            {
                fadeHandler.StartFadeIn();
            }

            isSceneTransitioning = false;
            
            if (Time.timeScale == 0)
                Time.timeScale = 1f;   // ポーズ画面から遷移した際
        }
    }
}
