using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using CoreModule.Input;
using Cysharp.Threading.Tasks;
using System.Threading;
using Module.Management;
using UnityEngine.EventSystems; 
using UnityEngine.Serialization; // Added for FormerlySerializedAs

namespace Module.Application.SceneSwitch
{
    public class FadeAndSceneTransition : MonoBehaviour
    {
        [Header("ページめくり用Prefab")]
        [FormerlySerializedAs("fadeCanvasPrefab")]
        [SerializeField] GameObject pageFlipFadePrefab;

        [Header("通常フェード用Prefab")]
        [SerializeField] GameObject normalFadePrefab;

        [Header("移動するシーンの名前")]
        public string nextSceneName;

        [Header("BGMフェードアウト時間（秒）")]
        [SerializeField] private float bgmFadeDuration = 1.0f;

        // 2つの永続フェードオブジェクトを保持
        private static GameObject pageFlipInstance;
        private static GameObject normalFadeInstance;

        // 現在使用中のフェードハンドラ
        private static IFadeHandler fadeHandler;
        private bool isSceneTransitioning;
        
        private void Awake()
        {
            // ここでのインスタンス生成は行わず、StartTransition時に必要に応じて生成する形に変更
            // ただし、以前の互換性のため、何かしら初期化が必要なら検討
            
            SceneManager.sceneLoaded += OnSceneLoadedWrapper;
        }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedWrapper;
        }

        private void OnSceneLoadedWrapper(Scene scene, LoadSceneMode mode)
            => OnSceneLoadedSequence(CancellationToken.None).Forget();
        
        /// <summary>
        /// 指定されたプレハブからフェーダーを取得・生成し、Activeにする
        /// </summary>
        private IFadeHandler GetOrCreateFader(bool usePageFlip)
        {
            GameObject targetInstance = usePageFlip ? pageFlipInstance : normalFadeInstance;
            GameObject prefab = usePageFlip ? pageFlipFadePrefab : normalFadePrefab;

            // まだ生成されていなければ生成
            if (targetInstance == null)
            {
                if (prefab == null)
                {
                    Debug.LogError($"{(usePageFlip ? "PageFlip" : "Normal")} Fade Prefab is missing!");
                    return null;
                }
                targetInstance = Instantiate(prefab);
                DontDestroyOnLoad(targetInstance);

                // Static変数に保存
                if (usePageFlip) pageFlipInstance = targetInstance;
                else normalFadeInstance = targetInstance;
            }

            // 表示状態を切り替え
            if (pageFlipInstance != null) pageFlipInstance.SetActive(usePageFlip);
            if (normalFadeInstance != null) normalFadeInstance.SetActive(!usePageFlip);

            return targetInstance.GetComponentInChildren<IFadeHandler>();
        }

        /// <summary>
        /// シーン切り替え開始（ページめくり演出を使用）
        /// </summary>
        public void StartPageFlipTransition() => StartTransitionInternal(true);

        /// <summary>
        /// シーン切り替え開始（通常フェード演出を使用）
        /// </summary>
        public void StartNormalTransition() => StartTransitionInternal(false);

        /// <summary>
        /// シーン切り替え開始（デフォルト：ページめくり）
        /// ※互換性のため残しています
        /// </summary>
        public void StartTransition() => StartPageFlipTransition();

        /// <summary>
        /// 内部処理：指定されたモードで遷移を開始
        /// </summary>
        private void StartTransitionInternal(bool usePageFlip)
        {
            // フェーダーを準備
            fadeHandler = GetOrCreateFader(usePageFlip);
            
            if (fadeHandler == null)
            {
                Debug.LogError("FadeHandlerの取得に失敗しました");
                return;
            }

            // 現在アクティブなフェーダーのGameObject
            GameObject activeObj = usePageFlip ? pageFlipInstance : normalFadeInstance;

            // UniTaskVoidにすることで、投げっぱなしで実行
            // ただし、このScriptが破棄されると止まってしまうので、
            // DontDestroyOnLoadされているfaderObjのライフサイクルに紐づける
            CancellationToken token = activeObj != null 
                ? activeObj.GetCancellationTokenOnDestroy() 
                : this.GetCancellationTokenOnDestroy();

            TransitionSequence(token).Forget();
        }
        
        public void StartTransitionSame()
        {
           nextSceneName = SceneManager.GetActiveScene().name;
           StartNormalTransition(); // リロードは通常フェードが無難？一旦通常にしておく
        }

        /// <summary>
        /// 名前指定してページめくり遷移
        /// </summary>
        public void StartPageFlipTransition(string sceneName)
        {
            nextSceneName = sceneName;
            StartPageFlipTransition();
        }

        /// <summary>
        /// 名前指定して通常フェード遷移
        /// </summary>
        public void StartNormalTransition(string sceneName)
        {
            nextSceneName = sceneName;
            StartNormalTransition();
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
