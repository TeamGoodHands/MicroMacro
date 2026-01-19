using UnityEngine;
using UnityEngine.UI;

namespace Module.Application.SceneSwitch
{
    [RequireComponent(typeof(Image))] // Image必須にする
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
            
            // 念のための初期化：最初は透明かつ操作可能にしておく
            // これがないと、Prefab保存時にraycastTargetがtrueだとゲーム開始時に操作できなくなる
            SetImageProperties(0, false); 
            
            if (!img.enabled)
                img.enabled = true;
        }

        private void Update()
        {
            if (fadeState == FadeState.FadingIn)
            {
                // 1 -> 0 (透明へ)
                UpdateFade(1 - GetFadeProgress(), OnFadeInComplete);
            }
            else if (fadeState == FadeState.FadingOut)
            {
                // 0 -> 1 (暗転へ)
                UpdateFade(GetFadeProgress(), OnFadeOutComplete);
            }
        }
       
        public void StartFadeIn()
        {
            if (fadeState != FadeState.None)
                return;

            fadeState = FadeState.FadingIn;
            ResetTimer();
            // フェードイン中も操作させたくない場合は true
            SetImageProperties(1, true);
        }

        public void StartFadeOut()
        {
            if (fadeState == FadeState.FadingOut)
                return;
            
            // フェード開始した瞬間から入力をブロックする
            // 透明度(alpha)に関わらずRaycastTargetをtrueにすることで、
            // このImageより奥にあるボタンへのクリックを遮断
            img.raycastTarget = true;

    #if UNITY_EDITOR
            Debug.Log("フェードアウト開始：入力ロック有効");
    #endif

            if (fadeState == FadeState.FadingIn)
            {
                // フェードイン中に割り込まれた場合は時間を反転してスムーズに移行
                timer = fadeDuration - timer;
            }
            else
            {
                ResetTimer();
                // まだ透明(0)だが、raycastTargetはtrue(操作不能)にする
                SetImageProperties(0, true);
            }

            fadeState = FadeState.FadingOut;
        }

        public bool IsFadeInComplete() => fadeState == FadeState.FadingIn && img.color.a < 0.05f; // 閾値を少し緩めに
        public bool IsFadeOutComplete() => fadeState == FadeState.None && img.color.a >= 0.95f;
        public bool IsFading() => fadeState != FadeState.None;

        private float GetFadeProgress()
        {
            if (fadeDuration <= 0f) return 1f;
            return Mathf.Clamp01(timer / fadeDuration);
        }

        private void ResetTimer() => timer = 0.0f;

        /// <summary>
        /// Imageのプロパティ設定
        /// </summary>
        /// <param name="alpha">透明度</param>
        /// <param name="raycastTarget">trueならクリックを吸う（後ろのボタンを押せなくする）</param>
        private void SetImageProperties(float alpha, bool raycastTarget)
        {
            if(img == null) return;

            var c = img.color;
            img.color = new Color(c.r, c.g, c.b, alpha);
            img.raycastTarget = raycastTarget;
        }

        private void UpdateFade(float alpha, System.Action onComplete)
        {
            // アルファ値のみ更新（raycastTargetは状態遷移時のみ触る）
            var c = img.color;
            img.color = new Color(c.r, c.g, c.b, alpha);

            if (timer >= fadeDuration)
            {
                onComplete?.Invoke();
            }

            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            timer += dt;
        }
   
        private void OnFadeInComplete()
        {
            // フェードイン完了＝画面が見えた状態
            // ここでようやく操作ロックを解除する
            SetImageProperties(0, false);
            fadeState = FadeState.None;
        }
        
        private void OnFadeOutComplete()
        {
            // フェードアウト完了＝真っ暗
            // 操作ロックは継続(true)
            SetImageProperties(1, true);
            fadeState = FadeState.None;
        }
    }
}