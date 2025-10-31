using System;
using Constants;
using UnityEngine;

namespace Module.Application.Dialogue
{
    public class ArrivalDialogue : MonoBehaviour
    {
        [Header("到着時に表示するセリフ")] [SerializeField] private String[] arrivalDialogues;
        [SerializeField] private DialogueManager dialogueManager;

        private bool hasBeenTriggered = false;
        private void Awake()
        {
            if (arrivalDialogues == null)
            {
                Debug.LogAssertion("到着時セリフが未設定です。");
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