using UnityEngine;
using UnityEngine.Audio;

namespace Module.Management
{
    /// <summary>
    /// 音量の計算、保存、読み込みを一元管理するクラス
    /// </summary>
    public static class VolumeManager
    {
        // パラメータ名の定数化（文字列の打ち間違いを防ぐため）
        public const string ParamMaster = "Master";
        public const string ParamBgm = "BGM";
        public const string ParamSe = "SE";

        // PlayerPrefs保存時のキーの接頭辞
        private const string KeyPrefix = "Volume_";

        /// <summary>
        /// 音量を設定し、AudioMixerとPlayerPrefsに反映する
        /// </summary>
        public static void SetVolume(AudioMixer mixer, string paramName, float linearVolume)
        {
            if (mixer == null) return;
            
            float clampedLinear = Mathf.Clamp01(linearVolume);  
            float decibel = clampedLinear <= 0f ? -80f : 20.0f * Mathf.Log10(clampedLinear);
            
            mixer.SetFloat(paramName, decibel);
            
            // PlayerPrefsに保存
            PlayerPrefs.SetFloat(KeyPrefix + paramName, clampedLinear);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// PlayerPrefsに保存されている音量（リニア値）を取得する
        /// </summary>
        public static float GetVolume(string paramName, float defaultValue = 1f)
        {
            return PlayerPrefs.GetFloat(KeyPrefix + paramName, defaultValue);
        }
    }
}