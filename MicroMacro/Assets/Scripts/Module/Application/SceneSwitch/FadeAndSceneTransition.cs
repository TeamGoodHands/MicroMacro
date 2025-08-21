using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Module.Application.SceneSwitch
{
    public class FadeAndSceneTransition : MonoBehaviour
    {
        [Header("フェード処理")] public GameObject fadeHandler;
        [Header("移動するシーンの名前")] public string nextSceneName;

        private IFadeHandler m_fade;
        private bool isSceneTransitioning = false;

        private void Start()
        {
            // fadeHandlerがIFadeHandlerを実装していればIFadeHandler型に変換して代入
            m_fade = fadeHandler.GetComponent<IFadeHandler>();

            if (m_fade == null)
            {
                Debug.LogError("No IFadeHandler attached to fadeHandler");
            }
            
        }

        /// <summary>
        /// 遷移の開始 余計な音を止める->フェードアウト->シーン遷移
        /// </summary>
        public void StartTransition()
        {
            if (isSceneTransitioning || m_fade == null)
                return;

            if (nextSceneName == SceneManager.GetActiveScene().name)
            {
                Debug.LogError("nextSceneNameと現在アクティブなシーンが同じです");
                return;
            }

            isSceneTransitioning = true;

            //余計なサウンド停止 
            // SoundManager.instance.StopAllSound();

            m_fade.StartFadeOut();
            StartCoroutine(LoadNextSceneAsync());
        }

        /// <summary>
        /// 名前指定してシーン移動
        /// </summary>
        /// <param name="nextSceneName"></param>
        public void StartTransition(string nextSceneName)
        {
            this.nextSceneName = nextSceneName;
            StartTransition();
        }

        private IEnumerator LoadNextSceneAsync()
        {
            while (m_fade.IsFadeOutComplete() == false)
            {
                yield return null;
            }

            SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);

            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        }
    }
}
