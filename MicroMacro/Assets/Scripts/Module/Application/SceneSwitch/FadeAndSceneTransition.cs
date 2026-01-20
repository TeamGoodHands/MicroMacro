using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using CoreModule.Input;
using Cysharp.Threading.Tasks;
using System.Threading;
using Module.Management;
using UnityEngine.EventSystems; 

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
            
            SceneManager.sceneLoaded += OnSceneLoadedWrapper;
        }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedWrapper;
        }

        private void OnSceneLoadedWrapper(Scene scene, LoadSceneMode mode)
            => OnSceneLoadedSequence(CancellationToken.None).Forget();
        
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
        /// シーン切り替え開始（外部から呼ばれる入り口）
        /// </summary>
        public void StartTransition()
        {
            // UniTaskVoidにすることで、投げっぱなしで実行
            TransitionSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }
        
        public void StartTransitionSame()
        {
           nextSceneName = SceneManager.GetActiveScene().name;
           StartTransition();
        }

        /// <summary>
        /// 名前指定してシーン移動したい場合
        /// </summary>
        public void StartTransition(string sceneName)
        {
            nextSceneName = sceneName;
            StartTransition();
        }

        /// <summary>
        /// 一連の遷移処理
        /// </summary>
        private async UniTaskVoid TransitionSequence(CancellationToken token)
        {
            if (isSceneTransitioning || fadeHandler == null)
                return;
            
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError("次のシーン名が設定されていません。");
                return;
            }
            
            isSceneTransitioning = true;
            InputSystem.actions.Disable();
            
            fadeHandler.StartFadeOut();
            
            // フェードアウト完了待ち
            await UniTask.WaitUntil(() => fadeHandler.IsFadeOutComplete(), cancellationToken: token);
            
            SoundManager.instance.StopAllSound();

            // LoadSceneMode.Singleは現在のシーンを自動アンロードしてくれる
            // .ToUniTask() をつけることで await できるようになる
            await SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single).ToUniTask(cancellationToken: token);
        }
       
        /// <summary>
        /// 切り替わったシーン先のSceneManagerオブジェクトから呼ばれる関数
        /// </summary>
        /// <param name="token"></param>
        private async UniTask OnSceneLoadedSequence(CancellationToken token)
        {
            if (fadeHandler == null)
                return;
            
            InputSystem.actions.Disable();  // 最初のシーン読み込み時はこの関数しか呼ばれないので入力切っておく
            
            // 念のため1フレーム待つ（Update反映用)
            await UniTask.Yield(token);
             
            fadeHandler.StartFadeIn();

            await UniTask.WaitUntil(() => fadeHandler.IsFadeInComplete(), cancellationToken: token);
             
            InputSystem.actions.Enable();
            isSceneTransitioning = false;
        }
    }
}
