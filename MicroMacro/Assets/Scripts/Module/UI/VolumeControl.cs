using System;
using Module.Management;
using UnityEngine;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Module.UI
{
    public class VolumeControl : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;

        [SerializeField] private Slider MasterSlider;
        [SerializeField] private Slider BGMSlider; 
        [SerializeField] private Slider SESlider;
        
        private bool isInitialized = false;

        private void Start()
        {
            audioMixer.GetFloat("Master", out float masterVolume);
            MasterSlider.value = masterVolume;
            
            audioMixer.GetFloat("BGM", out float bgmVolume);
            BGMSlider.value = bgmVolume;

            audioMixer.GetFloat("SE", out float seVolume);
            SESlider.value = seVolume;
            isInitialized = true;
        }

        public void SetMasterVol(float volume)
        {
            audioMixer.SetFloat("Master", volume);
        }

        public void SetBGM(float volume)
        {
            audioMixer.SetFloat("BGM", volume);
        }

        public void SetSE(float volume)
        {
            audioMixer.SetFloat("SE", volume);
            
            if (!isInitialized) return; // 初期化時はサウンド鳴らさない
            
            SoundManager.instance.Play("ボタン決定");           
        }
    }
}