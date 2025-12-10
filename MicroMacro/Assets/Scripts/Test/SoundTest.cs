using System;
using UnityEngine;
using Module.Management;
using UnityEngine.InputSystem;

namespace Test
{
    public class SoundTest : MonoBehaviour
    {
        [Header("再生したい音の名前")] [SerializeField] private string[] soundName;

        private void Start()
        {
            SoundManager.instance.Play("決定");
            // 初期化や設定が必要な場合はここに記述
            Debug.Log("決定を鳴らしました");
        }

        private void Update()
        {
            for (int i = 0; i < soundName.Length; i++)
            {
                if (Keyboard.current[Key.Digit0 + i].wasPressedThisFrame)
                {
                    if (SoundManager.instance.GetIsPlaying(soundName[i]))
                    {
                        SoundManager.instance.StopPlay(soundName[i]);
                        Debug.Log("再生停止");
                        return;
                    }

                    SoundManager.instance.Play(soundName[i]);
                    Debug.Log(i + "番目のサウンドを再生");
                }
            }
        }
    }
}