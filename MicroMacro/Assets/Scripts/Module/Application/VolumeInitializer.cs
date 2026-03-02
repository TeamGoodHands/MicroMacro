using UnityEngine;
using UnityEngine.Audio;

namespace Module.Application
{
    /// <summary>
    /// ゲーム起動時（Rootシーン）にAudioMixerの音量を初期化・復元するクラス
    /// </summary>
    public class VolumeInitializer : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;

        [Header("Parameter Names")]
        [SerializeField] private string masterParam = "Master";
        [SerializeField] private string bgmParam = "BGM";
        [SerializeField] private string seParam = "SE";

        [Header("Default Volumes (Linear 0.0001 - 1.0)")]
        [SerializeField, Range(0.0001f, 1f)] private float defaultMaster = 1f;
        [SerializeField, Range(0.0001f, 1f)] private float defaultBgm = 1f;
        [SerializeField, Range(0.0001f, 1f)] private float defaultSe = 1f;

        // PlayerPrefsで保存・読み込みするためのキー名
        private const string MasterKey = "Volume_Master";
        private const string BgmKey = "Volume_BGM";
        private const string SeKey = "Volume_SE";

        private void Start()
        {
            // PlayerPrefsから保存された音量（リニア値）を読み込む。
            // もし初回起動でデータがない場合は、Inspectorで設定したdefaultの数値が使われる
            float masterVolume = PlayerPrefs.GetFloat(MasterKey, defaultMaster);
            float bgmVolume = PlayerPrefs.GetFloat(BgmKey, defaultBgm);
            float seVolume = PlayerPrefs.GetFloat(SeKey, defaultSe);

            // AudioMixerに適用
            SetVolume(masterParam, masterVolume);
            SetVolume(bgmParam, bgmVolume);
            SetVolume(seParam, seVolume);
        }

        private void SetVolume(string parameterName, float linearVolume)
        {
            // リニア値(0~1)をデシベル(-80~0)に変換して適用
            // Mathf.Log10(0)は-Infinityになってしまうため、最小値を0.0001fに制限
            float clampedLinear = Mathf.Clamp(linearVolume, 0.0001f, 1f);
            float decibel = 20.0f * Mathf.Log10(clampedLinear);
            
            audioMixer.SetFloat(parameterName, decibel);
        }
    }
}