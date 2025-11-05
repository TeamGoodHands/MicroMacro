using System;
using Constants;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Module.Application.Dialogue
{
    public class ArrivalDialogue : MonoBehaviour
    {
        [Header("到着時に表示するセリフ")] [SerializeField] private String[] arrivalDialogues;
        [SerializeField] private DialogueManager dialogueManager;

        private bool hasBeenTriggered = false;
        
        /// <summary>
        /// クラスを無効化してもOnTriggerは呼ばれるため、セリフが設定されていない場合は
        /// 最初から発動済みに(無効化)しておく。
        /// </summary>
        private void Awake()
        {
            if (dialogueManager == null)
            {
                Debug.LogWarning("[ArrivalDialogue] DialogueManagerが設定されていません。", this.gameObject);
                hasBeenTriggered = true;
                return;
            }
      
            if (arrivalDialogues == null || arrivalDialogues.Length == 0)
            {
                hasBeenTriggered = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasBeenTriggered)
                return;
            
            if (other.gameObject.CompareTag(Tag.Handle.Player))
            {
                dialogueManager.EnqueueDialogues(arrivalDialogues);
                hasBeenTriggered = true;
            }
        }
    }
}