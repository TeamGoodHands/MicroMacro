using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TempQuitGame : MonoBehaviour
{
    // 簡易終了処理
    private void Update()
    {
        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; //ゲームプレイ終了
#else
    Application.Quit();
#endif
        }
    }
}
