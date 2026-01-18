using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;

namespace Module.Management
{
    public class SoundManager : MonoBehaviour
    {
        [Serializable]
        public class BGMData
        {
            public string    name;
            public AudioClip audioClip;
            [Range(0f, 1f)]
            public float     volume;
        }

        [Serializable]
        public class SEData
        {
            public string    name;
            public AudioClip audioClip;
            [HideInInspector]
            public float     playedTime;
            [Range(0f, 1f)]
            public float     volume;
        }

        [Serializable]
        private class AudioMixerGroups
        {
            public AudioMixerGroup BGM;
            public AudioMixerGroup SE;
        }
        
        [SerializeField] private AudioMixerGroups audioMixerGroups;

        [SerializeField] private BGMData[] bgmDatas;
        [SerializeField] private SEData[]  SEDatas;
        [Header("一度再生してから、次再生出来るまでの間隔(秒)")]
        [SerializeField] private float playableDistance = 0.2f;

        private Dictionary<string, BGMData> BGMDictionary = new Dictionary<string, BGMData>();
        private Dictionary<string, SEData> SEDictionary = new Dictionary<string, SEData>();

        public static SoundManager instance = null;

        private AudioSource[] audioSourceList = new AudioSource[10];

        private void SetInstance()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Awake()
        {
            SetInstance();
            
            if (instance != this)
                return;

            for (int i = 0; i < audioSourceList.Length; i++)
            {
                audioSourceList[i] = CreateNewAudioSourceGameObject();
            }

            foreach (BGMData bgmData in bgmDatas)
            {
                if (!BGMDictionary.ContainsKey(bgmData.name))
                    BGMDictionary.Add(bgmData.name, bgmData);
            }

            foreach (SEData SEData in SEDatas)
            {
                if (!SEDictionary.ContainsKey(SEData.name))
                    SEDictionary.Add(SEData.name, SEData);
            }
        }

        private AudioSource CreateNewAudioSourceGameObject()
        {
            GameObject obj = new GameObject("AudioSourceObj");
            obj.transform.SetParent(transform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            
            return source;
        }

        private AudioSource GetUnusedAudioSource(AudioClip clip)
        {
            foreach (AudioSource audioSource in audioSourceList)
            {
                if (audioSource.isPlaying == false)
                    return audioSource;
            }

            return null; 
        }

        /// <summary>
        /// BGM再生処理
        /// </summary>
        /// <returns>再生に使用したAudioSource</returns>
        private AudioSource PlayBGM(AudioClip bgm, float volume, Vector3 position, bool is3D)
        {
            AudioSource audioSource = GetUnusedAudioSource(bgm);

            if (audioSource == null)
            {
                Debug.Log("BGM play failed: No unused AudioSource found.");
                
                return null;
            }

            if (is3D)
            {
                audioSource.transform.position = position;
                audioSource.spatialBlend = 1f;
            }
            else
            {
                audioSource.transform.localPosition = Vector3.zero;
                audioSource.spatialBlend = 0f;
            }

            audioSource.volume = volume;
            audioSource.loop = true;
            audioSource.clip = bgm;
            audioSource.outputAudioMixerGroup = audioMixerGroups.BGM;
            audioSource.Play();
            
            return audioSource;
        }

        /// <summary>
        /// SE再生処理
        /// </summary>
        /// <returns>再生に使用したAudioSource</returns>
        private AudioSource PlaySE(AudioClip clip, float volume, Vector3 position, bool is3D)
        {
            AudioSource audioSource = GetUnusedAudioSource(clip);

            if (audioSource == null)
            {
                Debug.Log("SE play failed: No unused AudioSource found.");
                
                return null;
            }

            if (is3D)
            {
                audioSource.transform.position = position;
                audioSource.spatialBlend = 1f;
            }
            else
            {
                audioSource.transform.localPosition = Vector3.zero;
                audioSource.spatialBlend = 0f;
            }

            audioSource.volume = volume;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = audioMixerGroups.SE;
            audioSource.PlayOneShot(clip);
            
            return audioSource;
        }

        // --- Public Play Methods ---

        /// <summary>
        /// 名前を指定して再生
        /// </summary>
        /// <returns>再生中のAudioSource（失敗時はnull）</returns>
        public AudioSource Play(string name)
        {
            return PlayInternal(name, 1.0f, Vector3.zero, false);
        }

        public AudioSource Play(string name, float volume)
        {
            return PlayInternal(name, volume, Vector3.zero, false);
        }

        public AudioSource PlayAtPoint(string name, Vector3 position, float volume = 1.0f)
        {
            return PlayInternal(name, volume, position, true);
        }

        /// <summary>
        /// 内部再生処理。AudioSourceを返すように変更
        /// </summary>
        private AudioSource PlayInternal(string name, float volumeRate, Vector3 position, bool is3D)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("再生名が null/空文字です。");
                
                return null;
            }

            if (BGMDictionary.TryGetValue(name, out BGMData bgmData))
            {
                return PlayBGM(bgmData.audioClip, bgmData.volume * volumeRate, position, is3D);
            }
            else if (SEDictionary.TryGetValue(name, out SEData seData))
            {
                if (Time.realtimeSinceStartup - seData.playedTime < playableDistance)
                {
                    return null;
                }
                
                seData.playedTime = Time.realtimeSinceStartup;
                
                return PlaySE(seData.audioClip, seData.volume * volumeRate, position, is3D);
            }
            else
            {
                Debug.LogWarning($"その別名は登録されていません: {name}");
                
                return null;
            }
        }

        // --- Stop Methods ---

        /// <summary>
        /// 名前指定で停止（従来の互換性用）
        /// </summary>
        public void StopPlay(string name)
        {
            AudioSource audioSource = GetUsingAudioSource(name);

            if (audioSource == null)
                return;

            audioSource.Stop();
        }

        /// <summary>
        /// 【新規追加】AudioSourceのハンドルを直接指定して停止
        /// Play関数の戻り値をここに渡してください
        /// </summary>
        public void Stop(AudioSource source)
        {
            if (source == null)
                return;

            // すでに止まっている、または破棄されている場合のチェック
            if (source.isPlaying)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// 全停止
        /// </summary>
        public void StopAllSound()
        {
            foreach (AudioSource audioSource in audioSourceList)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }

        // --- Helper ---

        private AudioSource GetUsingAudioSource(string name)
        {
            AudioClip targetClip = null;

            if (BGMDictionary.TryGetValue(name, out BGMData bgmData))
            {
                targetClip = bgmData.audioClip;
            }
            else if (SEDictionary.TryGetValue(name, out SEData seData))
            {
                targetClip = seData.audioClip;
            }

            if (targetClip == null)
            {
                Debug.LogError("クリップが見つかりません。");
                
                return null;
            }

            foreach (AudioSource audioSource in audioSourceList)
            {
                if (audioSource.clip == targetClip && audioSource.isPlaying)
                    return audioSource;
            }
            
            return null;
        }
        
        // --- Delay Play ---

        public void DelayPlay(string name, float DelayTime)
        {
            DelayPlayAsync(name, DelayTime, Vector3.zero, false).Forget();
        }

        public void DelayPlayAtPoint(string name, float DelayTime, Vector3 position)
        {
            DelayPlayAsync(name, DelayTime, position, true).Forget();
        }

        private async UniTaskVoid DelayPlayAsync(string name, float DelayTime, Vector3 position, bool is3D)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(DelayTime), ignoreTimeScale: false);
            
            if (this == null) 
                return;

            PlayInternal(name, 1.0f, position, is3D);
        }
    }
}