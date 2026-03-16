using UnityEngine;
using UnityEngine.Audio;
using Module.Management;

namespace Module.Application
{
    /// <summary>
    /// ゲーム起動時（Rootシーン）にAudioMixerの音量を初期化・復元するクラス
    /// </summary>
    public class VolumeInitializer : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;

        [Header("Parameter Names")]
        [SerializeField] private string masterParam = VolumeManager.ParamMaster;
        [SerializeField] private string bgmParam = VolumeManager.ParamBgm;
        [SerializeField] private string seParam = VolumeManager.ParamSe;

        [Header("Default Volumes (Linear 0.0001 - 1.0)")]
        [SerializeField, Range(0.0001f, 1f)] private float defaultMaster = 1f;
        [SerializeField, Range(0.0001f, 1f)] private float defaultBgm = 1f;
        [SerializeField, Range(0.0001f, 1f)] private float defaultSe = 1f;

        private void Start()
        {
            // VolumeManagerを使って、保存されている値の読み込みとMixerへの適用を行う
            float masterVolume = VolumeManager.GetVolume(masterParam, defaultMaster);
            float bgmVolume = VolumeManager.GetVolume(bgmParam, defaultBgm);
            float seVolume = VolumeManager.GetVolume(seParam, defaultSe);

            VolumeManager.SetVolume(audioMixer, masterParam, masterVolume);
            VolumeManager.SetVolume(audioMixer, bgmParam, bgmVolume);
            VolumeManager.SetVolume(audioMixer, seParam, seVolume);
        }
    }
}