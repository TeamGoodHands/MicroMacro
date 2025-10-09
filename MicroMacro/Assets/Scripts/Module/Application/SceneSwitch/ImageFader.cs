using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Application.SceneSwitch
{
    public class ImageFader : MonoBehaviour, IFadeHandler
    {
        [Header("フェードインなし(デバッグ用)")]
        public bool firstFadeInComp;

        private Image img = null;
        private float timer = 0.0f;
        private FadeState fadeState = FadeState.None;
        
        private enum FadeState { None, FadingIn, FadingOut }
        [Header("フェード処理にかかる時間")]
        [SerializeField] private  float FadeDuration = 1.0f; 

        private void Start()
        {
            img = GetComponent<Image>();
            if (img == null)
            {
                Debug.LogError("Image Component is null");
                return;
            }
            
            if (img.IsActive() == false)
                img.enabled = true;

            if (firstFadeInComp)
            {
                OnFadeInComplete();
            }
            else
            {
                StartCoroutine(WaitForFadeStart(0.5f));
            }
        }

        private void Update()
        {
            if (fadeState == FadeState.FadingIn)
            {
                // タイマーが進むにつれα値が1から0に(透明に)
                UpdateFade(1 - GetFadeProgress(), OnFadeInComplete);
            }
            else if (fadeState == FadeState.FadingOut)
            {
                // タイマーが進むにつれα値が0から1に(暗く)
                UpdateFade(GetFadeProgress(), OnFadeOutComplete);
            }
        }
       
        public void StartFadeIn()
        {
            if (fadeState != FadeState.None)
                return;

            fadeState = FadeState.FadingIn;
            ResetTimer();
            SetImageProperties(1, true);  // 開始時は真っ黒
        }

        public void StartFadeOut()
        {
            // フェードアウト中or完了している場合
            if (fadeState == FadeState.FadingOut)
                return;
            
    #if UNITY_EDITOR
            Debug.Log("フェードアウト開始");
    #endif

            if (fadeState == FadeState.FadingIn)
            {
                // フェードイン中にフェードアウトが呼ばれた場合、
                // 進行度を引き継いでスムーズに移行
                timer = FadeDuration - timer;
                Debug.Log("time = " + timer);
            }
            else
            {
                Debug.Log("call reset Timer");
                ResetTimer();
                SetImageProperties(0,true);
            }

            fadeState = FadeState.FadingOut;
        }

        /// <summary>
        /// フェード終了時にα値が 0 = フェードイン終了 
        /// </summary>
        public bool IsFadeInComplete() => fadeState == FadeState.None && img.color.a == 0;
        public bool IsFadeOutComplete() => fadeState == FadeState.None && img.color.a >= 1;
        public bool IsFading() => fadeState != FadeState.None;
        
        /// <summary>
        /// フェード中の進行率を計算する(0～1)
        /// </summary>
        private float GetFadeProgress() => Mathf.Clamp01(timer / FadeDuration);
        private void ResetTimer() => timer = 0.0f;

        /// <summary>
        /// Imageのプロパティ一括設定 徐々にalpha値変化させる
        /// </summary>
        private void SetImageProperties(float alpha, bool raycastTarget)
        {
            img.color = new Color(0, 0, 0, alpha);
            img.raycastTarget = raycastTarget;
        }

        /// <summary>
        /// フェードの更新 完了したらイベント呼ぶ
        /// </summary>
        /// <param name="alpha">進捗</param>
        private void UpdateFade(float alpha, System.Action onComplete)
        {
            Debug.Log(timer);
            // α値を更新
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);

            if (timer >= FadeDuration)
            {
                Debug.Log($"OnComplete {timer} : " + FadeDuration);
                onComplete?.Invoke();
            }

            timer += Time.deltaTime;
        }
   
        private void OnFadeInComplete()
        {
            SetImageProperties(0,  false);
            fadeState = FadeState.None;
        }
        
        private void OnFadeOutComplete()
        {
            SetImageProperties(1,  false);
            fadeState = FadeState.None;
        }

        /// <summary>
        /// フェード処理開始前に一定フレーム待機
        /// </summary>
        private IEnumerator WaitForFadeStart(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            StartFadeIn();
        }
    }
}