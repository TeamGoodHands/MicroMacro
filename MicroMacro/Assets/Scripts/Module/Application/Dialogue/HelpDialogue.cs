using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Constants;

namespace Module.Application.Dialogue
{
    public class HelpDialogue : MonoBehaviour
    {
        [SerializeField] private DialogueManager dialogueManager;
        
        [Header("呼び出すセリフたち")] [SerializeField] private String[] queueTexts;
        [Header("〇秒経過でお助けUIを表示")] [SerializeField] private float helpTriggerTime;
        
        private float elapsedTime;
        private bool  isPlayerInside;

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(Tag.Handle.Player))
            {
                // エリア内なのか確認
                isPlayerInside = true;
            }
        }
        
        private void Update()
        {
            if (isPlayerInside)
            {
                elapsedTime += Time.deltaTime;
                
                if (elapsedTime >= helpTriggerTime)
                {
                    // セリフキューに追加
                    foreach (var text in queueTexts)
                    {
                        dialogueManager.Enqueue(text);
                    }
                    
                    // 経過時間リセット 再表示しないなら必要なし。
                    isPlayerInside = false;
                    elapsedTime = 0f;
                }
            }
        }
    }
}