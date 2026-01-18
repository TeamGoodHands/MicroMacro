using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Module.Application.SceneSwitch
{
    public class ImageFader : MonoBehaviour, IFadeHandler
    {
      
        private Image img = null;
        private float timer = 0.0f;
        private FadeState fadeState = FadeState.None;
        
        private enum FadeState { None, FadingIn, FadingOut }
        [Header("フェード処理にかかる時間")]
        [SerializeField] private float fadeDuration = 1.0f; 

        private void Awake()
        {
            img = GetComponent<Image>();
            if (img == null)
            {
                Debug.LogError("ImageComponentがnullです");
                return;
            }
            
            if (!img.enabled)
                img.enabled = true;
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
                timer = fadeDuration - timer;
            }
            else
            {
                ResetTimer();
                SetImageProperties(0,true);
            }

            fadeState = FadeState.FadingOut;
        }

        /// <summary>
        /// 大体フェード完了したらtrueを返す
        /// </summary>
        public bool IsFadeInComplete() => fadeState == FadeState.FadingIn && img.color.a < 0.55f;
        public bool IsFadeOutComplete() => fadeState == FadeState.None && img.color.a >= 1;
        public bool IsFading() => fadeState != FadeState.None;

        /// <summary>
        /// フェード中の進行率を計算する(0～1)
        /// </summary>
        private float GetFadeProgress()
        {
            if (fadeDuration <= 0f) // NaN対策 0秒の時はフェードなし
                return 1f;
            
            return Mathf.Clamp01(timer / fadeDuration);
        }

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
            // α値を更新
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);

            if (timer >= fadeDuration)
            {
                onComplete?.Invoke();
            }

            // 多少フリーズしても良いように最大値を設定
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            timer += dt;
        }
   
        private void OnFadeInComplete()
        {
            SetImageProperties(0,  false);
            fadeState = FadeState.None;
        }
        
        private void OnFadeOutComplete()
        {
            SetImageProperties(1,  true);
            fadeState = FadeState.None;
        }
    }
}