using System;
using UnityEngine;

namespace Module.Application.SceneSwitch
{
    public class TempQuitGame : MonoBehaviour
    {
        // 簡易終了処理
        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; //ゲームプレイ終了
#else
    Application.Quit();
#endif
            }
        }
    }
}