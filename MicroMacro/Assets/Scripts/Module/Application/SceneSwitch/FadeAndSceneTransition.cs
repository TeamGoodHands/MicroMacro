using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Module.Application.SceneSwitch
{
    public class FadeAndSceneTransition : MonoBehaviour
    {
        [Header("フェード処理")] public GameObject faderObj;
        [Header("移動するシーンの名前")] public string nextSceneName;

        private IFadeHandler fader;
        private bool isSceneTransitioning = false;

        private void Start()
        {
            // fadeHandlerがIFadeHandlerを実装していればIFadeHandler型に変換して代入
            fader = faderObj.GetComponent<IFadeHandler>();

            if (fader == null)
            {
                Debug.LogError("IFadeHandlerがアタッチされていません: " + faderObj);
            }
        }

        /// <summary>
        /// 遷移の開始 余計な音を止める->フェードアウト->シーン遷移
        /// </summary>
        public void StartTransition()
        {
            if (isSceneTransitioning || fader == null)
                return;

            if (string.IsNullOrEmpty(nextSceneName) || nextSceneName == SceneManager.GetActiveScene().name)
            {
                Debug.LogError("nextSceneNameが設定されていないか、現在のシーンと同じです");
                return;
            }

            isSceneTransitioning = true;

            //余計なサウンド停止 
            // SoundManager.instance.StopAllSound();

            fader.StartFadeOut();
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
            while (fader.IsFadeOutComplete())
            {
                yield return null;
            }

            // LoadSceneMode.Singleは現在のシーンを自動アンロード
            SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        }
    }
}
