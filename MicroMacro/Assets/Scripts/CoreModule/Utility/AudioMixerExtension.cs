using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace CoreModule.Utility
{
    public static class AudioMixerExtension
    {
        // 無音とみなすデシベル値（定数にしておくことでマジックナンバーを回避）
        private const float MinDecibel = -80.0f;

        /// <summary>
        /// 0.0〜1.0の線形値でAudioMixerの音量をフェードします。
        /// 内部で自動的にデシベル変換を行います。
        /// </summary>
        /// <param name="mixer">対象のAudioMixer</param>
        /// <param name="paramName">Exposeされたパラメータ名</param>
        /// <param name="targetVolume">目標の音量（0.0〜1.0）</param>
        /// <param name="duration">所要時間</param>
        public static Tweener DOFadeVolume(
            this AudioMixer mixer,
            string paramName,
            float targetVolume,
            float duration)
        {
            if (mixer == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(paramName))
            {
                return null;
            }

            // 現在のデシベル値を取得
            float currentDecibel;
            bool result = mixer.GetFloat(paramName, out currentDecibel);

            if (!result)
            {
                Debug.LogWarning($"パラメータ '{paramName}' が見つかりません。");
                return null;
            }

            // 現在のデシベル値を0.0〜1.0の線形値に変換（開始値として使用）
            float startLinear = ConvertDecibelToLinear(currentDecibel);

            // 目標値のクランプ（念のため0〜1の範囲に収める）
            float clampedTarget = Mathf.Clamp01(targetVolume);

            // DOTweenで線形値を変化させ、Setterの中でデシベルに戻して適用する
            return DOTween.To(
                () => startLinear,
                linearValue =>
                {
                    // 線形値 -> デシベル変換
                    float dbValue = ConvertLinearToDecibel(linearValue);
                    mixer.SetFloat(paramName, dbValue);
                },
                clampedTarget,
                duration
            );
        }

        // 線形値(0-1) -> デシベル(-80 ~ 0) 変換ヘルパー
        public static float ConvertLinearToDecibel(float linear)
        {
            if (linear <= 0.0001f)
            {
                return MinDecibel;
            }

            return 20.0f * Mathf.Log10(linear);
        }

        // デシベル(-80 ~ 0) -> 線形値(0-1) 変換ヘルパー
        public static float ConvertDecibelToLinear(float decibel)
        {
            if (decibel <= MinDecibel)
            {
                return 0.0f;
            }

            return Mathf.Pow(10.0f, decibel / 20.0f);
        }
    }
}
