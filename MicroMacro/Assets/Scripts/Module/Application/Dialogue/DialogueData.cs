using UnityEngine;
using System;


namespace Module.Application.Dialogue
{
    [Serializable]
    public class DialogueData
    {
        public string Name;        // 登録名
        public string Dialogue;    // セリフ
        public float  DisplayTime; // 表示時間
    }
    
    // 右クリックから作成できるように
    [CreateAssetMenu(fileName = "DialogueData", menuName = "ScriptableObjects/DialogueData")]
    public class DialogueObjects : ScriptableObject
    {
        public DialogueData[] data; // ScriptableObjectの配列
    }
}