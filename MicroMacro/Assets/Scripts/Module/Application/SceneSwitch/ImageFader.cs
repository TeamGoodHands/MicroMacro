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
        private FadeState m_fadeState = FadeState.None;
        
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

            if (firstFadeInComp)
            {
                FadeInComplete();
            }
            else
            {
                StartCoroutine(WaitForFadeStart(0.5f));
            }
        }

        private void Update()
        {
            if (m_fadeState == FadeState.FadingIn)
            {
                UpdateFade(1 - GetFadeProgress(), FadeInComplete);
            }
            else if (m_fadeState == FadeState.FadingOut)
            {
                UpdateFade(GetFadeProgress(), FadeOutComplete);
            }
        }
       
        public void StartFadeIn()
        {
            if (m_fadeState != FadeState.None)
                return;

            m_fadeState = FadeState.FadingIn;
            ResetTimer();
            SetImageProperties(1, 1, true);
        }

        public void StartFadeOut()
        {
            if (m_fadeState != FadeState.None)
                return;
            
    #if UNITY_EDITOR
            Debug.Log("フェードアウト開始");
    #endif

            m_fadeState = FadeState.FadingOut;
            ResetTimer();
            SetImageProperties(0,0, true);
            
        }

        /// <summary>
        /// フェード終了時にα値が 0 = フェードイン終了 
        /// </summary>
        public bool IsFadeInComplete() => m_fadeState == FadeState.None && img.color.a == 0;

        public bool IsFadeOutComplete() => m_fadeState == FadeState.None && img.color.a >= 1;

        /// <summary>
        /// フェード中の進行率を計算する(0～1)
        /// </summary>
        private float GetFadeProgress() => Mathf.Clamp01(timer / FadeDuration);

        private void ResetTimer() => timer = 0.0f;

        /// <summary>
        /// Imageのプロパティ一括設定 徐々にalpha値変化させる
        /// </summary>
        /// <param name="fillAmount"></param>
        /// <param name="raycastTarget"></param>
        private void SetImageProperties(float alpha, float fillAmount, bool raycastTarget)
        {
            img.color = new Color(0, 0, 0, alpha);
            img.fillAmount = fillAmount;
            img.raycastTarget = raycastTarget;
        }

        /// <summary>
        /// フェードの更新 完了したらイベント呼ぶ
        /// </summary>
        /// <param name="progress">進捗</param>
        /// <param name="onComplete"></param>
        private void UpdateFade(float progress, System.Action onComplete)
        {
            SetImageProperties(progress, progress, true);

            if (timer >= FadeDuration)
            {
                onComplete.Invoke();
            }

            timer += Time.deltaTime;
        }
   
        private void FadeInComplete()
        {
            SetImageProperties(0, 0, false);
            m_fadeState = FadeState.None;
        }
        
        private void FadeOutComplete()
        {
            SetImageProperties(1, 1, false);
            m_fadeState = FadeState.None;
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