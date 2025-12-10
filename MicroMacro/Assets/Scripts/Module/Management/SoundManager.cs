using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Audio;

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
            public float     playedTime;  // 前回再生した時間 ※publicじゃないと動かないかも
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
        [SerializeField] private　float playableDistance = 0.2f;
        
        //別名(name)をキーとした管理用Dictionary
        private Dictionary<string, BGMData> BGMDictionary = new Dictionary<string, BGMData>();
        private Dictionary<string, SEData> SEDictionary = new Dictionary<string, SEData>();
        
        public static SoundManager instance = null;
        
        //AudioSource（スピーカー）を同時に鳴らしたい音の数だけ用意
        private AudioSource[] audioSourceList = new AudioSource[10];

        private void SetInstance()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        private void Awake()
        {
            for (int i = 0; i < audioSourceList.Length; i++)
            {
                audioSourceList[i] = gameObject.AddComponent<AudioSource>();
            }
            
            //それぞれDictionaryにセット
            foreach (BGMData bgmData in bgmDatas)
            {
                BGMDictionary.Add(bgmData.name, bgmData);
            }

            foreach (SEData SEData in SEDatas)
            {
                SEDictionary.Add(SEData.name, SEData);
            }
            
            SetInstance();
        }

        /// <summary>
        /// 未使用のAudioSourceの取得 全て使用中ならnull
        /// </summary>
        private AudioSource GetUnusedAudioSource(AudioClip clip)
        {
         
            // すでに同じclipが入ったAudioSourceがあるならそれで再生
            foreach (var audioSource in audioSourceList)
            {
                if (audioSource.clip == clip)
                {
                    return audioSource;
                }
            }
            foreach (var audioSource in audioSourceList)
            {
                if (audioSource.isPlaying == false)
                    return audioSource;
            }
            return null; 
        }
        
        private void PlayBGM(AudioClip bgm, float volume)
        {
            // 同時に鳴らす場合を考慮して毎回変数生成する(必要ないかも)
            AudioSource audioSource = GetUnusedAudioSource(bgm);

            if (audioSource == null)
            {
                Debug.Log("BGM play failed");
                return;
            }

            audioSource.volume = volume;
            audioSource.loop = true;
            audioSource.clip = bgm;
            audioSource.outputAudioMixerGroup = audioMixerGroups.BGM;
            audioSource.Play();
        }

        private void PlaySE(AudioClip clip, float volume)
        {
            // 同時に鳴らす場合を考慮して毎回変数生成する(必要ないかも)
            AudioSource audioSource = GetUnusedAudioSource(clip);

            if (audioSource == null)
            {
                Debug.Log("SE play failed");
                return;
            }

            audioSource.volume = volume;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = audioMixerGroups.SE;
            audioSource.PlayOneShot(clip);
        }

        /// <summary>
        /// これを再生したいタイミングで呼び出す。名前を指定してBGM、SE一覧から検索、再生。
        /// </summary>
        /// <param name="name">AudioClipの名前でなく登録した別名</param>
        public void Play(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("再生名が null/空文字です。");
                return;
            }

            // それぞれの管理用Dictionaryから別名で検索、一致したら再生
            if (BGMDictionary.TryGetValue(name, out BGMData bgmData))
            {
                PlayBGM(bgmData.audioClip, bgmData.volume);
            }
            else if (SEDictionary.TryGetValue(name, out SEData seData))
            {
                if (Time.realtimeSinceStartup - seData.playedTime < playableDistance)
                {
                    Debug.Log("まだ再生できません。");
                    return;
                }
                
                seData.playedTime = Time.realtimeSinceStartup; 　//次回用に今回の再生時間の保持 
                PlaySE(seData.audioClip, seData.volume);
            }
            else
            {
                Debug.LogWarning("その別名は登録されていません:{name}");
            }
        }

        public void Play(string name, float volume)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("再生名が null/空文字です。");
                return;
            }

            volume = Mathf.Clamp01(volume);
            
            // それぞれの管理用Dictionaryから別名で検索、一致したら再生
            if (BGMDictionary.TryGetValue(name, out BGMData bgmData))
            {
                PlayBGM(bgmData.audioClip, volume);
            }
            else if (SEDictionary.TryGetValue(name, out SEData seData))
            {
                if (Time.realtimeSinceStartup - seData.playedTime < playableDistance)
                {
                    Debug.Log("まだ再生できません。");
                    return;
                }
                
                seData.playedTime = Time.realtimeSinceStartup; 　//次回用に今回の再生時間の保持 
                PlaySE(seData.audioClip, volume);
            }
            else
            {
                Debug.LogWarning("その別名は登録されていません:{name}");
            }
        }

        /// <summary>
        /// nameのAudioClipを使っているAudioSourceの取得
        /// </summary>
        /// <param name="name"></param>
        private AudioSource GetUsingAudioSource(string name)
        {
        
            if (BGMDictionary.TryGetValue(name, out BGMData bgmData))
            {
                for (int i = 0; i < audioSourceList.Length; i++)
                {
                    if (audioSourceList[i].clip == bgmData.audioClip)
                        return audioSourceList[i];
                }
            }
            else if (SEDictionary.TryGetValue(name, out SEData seData))
            {
                for (int i = 0; i < audioSourceList.Length; i++)
                {
                    if (audioSourceList[i].clip == seData.audioClip)
                        return audioSourceList[i];
                }
            }
            
            Debug.LogError("オーディオソースの取得に失敗しました。");
            return null;
        }

        public bool GetIsPlaying(string name)
        {
            AudioSource audioSource =  GetUsingAudioSource(name);
            if (audioSource == null)
            {
             //   Debug.Log("そのクリップは再生されていません");
                return false;
            }
            else
            {
                return true;
            }
        }
        public void StopPlay(string name)
        {
            AudioSource audioSource =  GetUsingAudioSource(name);

            if (audioSource == null)
            { 
                // Debug.Log("そのクリップは再生されていません");
                return;
            }
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        /// <summary>
        /// 今流している音をすべて止める
        /// </summary>
        public void StopAllSound()
        {
            foreach (var audioSource in audioSourceList)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
        
        /// <summary>
        /// 普通の関数を作って外部から呼び出しをしやすく
        /// </summary>
        public void DelayPlay(string name, float DelayTime)
        {
            StartCoroutine(DelayPlayProcess(name, DelayTime));
        }
        private IEnumerator DelayPlayProcess(string name, float DelayTime)
        {
            // 遅延かけて再生
            yield return new WaitForSeconds(DelayTime);
            Play(name);
        }
    }
}