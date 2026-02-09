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

        [Header("BGMフェードアウト時間（秒）")]
        [SerializeField] private float bgmFadeDuration = 1.0f;

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
            // faderObjが存在しない場合は作成
            if (faderObj == null)
            {
                CreateAndRegisterFader();
            }

            // UniTaskVoidにすることで、投げっぱなしで実行
            // ただし、このScriptが破棄されると止まってしまうので、
            // DontDestroyOnLoadされているfaderObjのライフサイクルに紐づける
            CancellationToken token = faderObj != null 
                ? faderObj.GetCancellationTokenOnDestroy() 
                : this.GetCancellationTokenOnDestroy();

            TransitionSequence(token).Forget();
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
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Debug.Log($"[Transition] Start: {nextSceneName}");

            InputSystem.actions.Disable();
            
            fadeHandler.StartFadeOut();
            
            // BGMフェードアウト開始（完了を待たずに進むが、シーンロード直前にStopAllSoundで止まる可能性あり）
            // UniTaskとして保持しておき、必要なら待つことも可能。
            // ここでは並列で走らせる。
            var bgmFadeTask = SoundManager.instance.FadeOutAndStopAllBGM(bgmFadeDuration, token);

            // フェードアウト完了待ち
            await UniTask.WaitUntil(() => fadeHandler.IsFadeOutComplete(), cancellationToken: token);
            Debug.Log($"[Transition] FadeOut Complete: {sw.ElapsedMilliseconds}ms");
            
            // BGMフェードは並列で走らせたまま、シーンロードへ進む
            // bgmFadeTaskはSoundManager側で完了時にStopしてくれるので放置でOK
            // await bgmFadeTask; 
            // SoundManager.instance.StopAllSound(); // これを呼ぶとフェード中のBGMも止まるので削除

            long beforeLoad = sw.ElapsedMilliseconds;
            // LoadSceneMode.Singleは現在のシーンを自動アンロードしてくれる
            // .ToUniTask() をつけることで await できるようになる
            await SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single).ToUniTask(cancellationToken: token);
            Debug.Log($"[Transition] Scene Load Complete: {sw.ElapsedMilliseconds - beforeLoad}ms (Total: {sw.ElapsedMilliseconds}ms)");
        }
       
        /// <summary>
        /// 切り替わったシーン先のSceneManagerオブジェクトから呼ばれる関数
        /// </summary>
        /// <param name="token"></param>
        private async UniTask OnSceneLoadedSequence(CancellationToken token)
        {
            if (fadeHandler == null)
                return;
            
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Debug.Log("[Transition] Scene Initialized");

            InputSystem.actions.Disable();  // 最初のシーン読み込み時はこの関数しか呼ばれないので入力切っておく
            
            // 念のため1フレーム待つ（Update反映用)
            await UniTask.Yield(token);

            fadeHandler.StartFadeIn();

            // フェードイン完了待ち
            await UniTask.WaitUntil(() => fadeHandler.IsFadeInComplete(), cancellationToken: token);
            Debug.Log($"[Transition] FadeIn Complete: {sw.ElapsedMilliseconds}ms");

            InputSystem.actions.Enable();

            isSceneTransitioning = false;
        }
    }
}
