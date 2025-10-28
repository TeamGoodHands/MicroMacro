using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Module.Application.Dialogue
{
    [Serializable]
    public class DialogueItem
    {
        public string EntryName;          // 登録名
        public string Text;          // セリフ
        public float  DisplayTime;   // 表示時間
        public Sprite characterIcon;  
    }
    
    // 右クリックから作成できるように
    [CreateAssetMenu(fileName = "DialogueData", menuName = "ScriptableObjects/DialogueData")]
    public class DialogueCollection : ScriptableObject
    {
       public DialogueItem[] items; // ScriptableObjectの配列
    }
}