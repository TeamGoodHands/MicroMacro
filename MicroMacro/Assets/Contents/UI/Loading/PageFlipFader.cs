using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Module.Application.SceneSwitch
{
    /// <summary>
    /// ページめくりエフェクトを使用したフェードハンドラー
    /// IFadeHandlerを実装し、FadeAndSceneTransitionと統合可能
    /// </summary>
    public class PageFlipFader : MonoBehaviour, IFadeHandler
    {
        [Header("References")]
        [SerializeField]
        private PageFlipManager pageFlipManager;

        [Header("Page Flip Settings")]
        [SerializeField]
        [Tooltip("最後の1枚を残してシーン遷移する")]
        private bool keepLastPage = true;

        [SerializeField]
        [Tooltip("FadeIn時に最後のページを非表示にするまでの遅延（秒）")]
        private float fadeInDelay = 0.1f;

        [SerializeField]
        [Tooltip("マスクディゾルブ演出を使用する（筆で塗られたような演出）")]
        private bool useMaskDissolve = true;

        private FadeState currentState = FadeState.None;
        private CancellationTokenSource fadeCts;

        private enum FadeState
        {
            None,
            FadingOut,
            FadeOutComplete,
            FadingIn,
            FadeInComplete
        }

        private void Awake()
        {
            if (pageFlipManager == null)
            {
                pageFlipManager = GetComponentInChildren<PageFlipManager>();
            }

            if (pageFlipManager == null)
            {
                Debug.LogError("PageFlipFader: PageFlipManagerが見つかりません");
            }
        }

        private void OnDestroy()
        {
            fadeCts?.Cancel();
            fadeCts?.Dispose();
        }

        #region IFadeHandler Implementation

        /// <summary>
        /// フェードアウト開始（画面を隠す = ページをめくる）
        /// </summary>
        public void StartFadeOut()
        {
            if (currentState == FadeState.FadingOut)
                return;

            // 前回のフェードをキャンセル
            fadeCts?.Cancel();
            fadeCts = new CancellationTokenSource();

            currentState = FadeState.FadingOut;
            FadeOutSequenceAsync(fadeCts.Token).Forget();
        }

        /// <summary>
        /// フェードイン開始（画面を表示 = ページを非表示に）
        /// </summary>
        public void StartFadeIn()
        {
            if (currentState == FadeState.FadingIn)
                return;

            fadeCts?.Cancel();
            fadeCts = new CancellationTokenSource();

            currentState = FadeState.FadingIn;
            FadeInSequenceAsync(fadeCts.Token).Forget();
        }

        public bool IsFadeOutComplete()
        {
            return currentState == FadeState.FadeOutComplete;
        }

        public bool IsFadeInComplete()
        {
            return currentState == FadeState.FadeInComplete || currentState == FadeState.None;
        }

        public bool IsFading()
        {
            return currentState == FadeState.FadingOut || currentState == FadeState.FadingIn;
        }

        #endregion

        #region Private Methods

        private async UniTaskVoid FadeOutSequenceAsync(CancellationToken token)
        {
            if (pageFlipManager == null)
            {
                currentState = FadeState.FadeOutComplete;
                return;
            }

            try
            {
                // 1. 現在のカメラ映像をキャプチャ
                pageFlipManager.CaptureCurrentScreen();

                // 2. メッシュを表示
                pageFlipManager.SetVisible(true);

                // 3. パラパラめくり
                await pageFlipManager.FlipRapidAsync(-1, keepLastPage, token);

                // 4. 完了状態に
                currentState = FadeState.FadeOutComplete;

#if UNITY_EDITOR
                Debug.Log("PageFlipFader: FadeOut完了");
#endif
            }
            catch (System.OperationCanceledException)
            {
                // キャンセルされた場合は何もしない
            }
        }

        private async UniTaskVoid FadeInSequenceAsync(CancellationToken token)
        {
            if (pageFlipManager == null)
            {
                currentState = FadeState.FadeInComplete;
                return;
            }

            try
            {
                // まず現在のカメラで位置を設定（一瞬の消失を防ぐ）
                pageFlipManager.RefreshPosition();
                
                // シーン遷移後、カメラの完全初期化を待つ
                await UniTask.Yield(token);
                await UniTask.Yield(token);
                
                // 再度位置を調整（カメラが変わった場合に対応）
                pageFlipManager.RefreshPosition();
                
                // 指定秒数待つ
                if (fadeInDelay > 0)
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(fadeInDelay), cancellationToken: token);
                }
                
                // マスクディゾルブ演出（筆で塗られたように消える）
                if (useMaskDissolve && keepLastPage)
                {
                    await pageFlipManager.MaskDissolveLastPageAsync(token);
                }
                else
                {
                    pageFlipManager.CleanupRapidPages();
                }
                
                // メッシュを非表示
                pageFlipManager.SetVisible(false);

                currentState = FadeState.FadeInComplete;

                await UniTask.Yield(cancellationToken: token);
                currentState = FadeState.None;

#if UNITY_EDITOR
                Debug.Log("PageFlipFader: FadeIn完了");
#endif
            }
            catch (System.OperationCanceledException)
            {
                // キャンセルされた場合は何もしない
            }
        }

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Test FadeOut")]
        private void TestFadeOut()
        {
            StartFadeOut();
        }

        [ContextMenu("Test FadeIn")]
        private void TestFadeIn()
        {
            StartFadeIn();
        }
#endif
    }
}
