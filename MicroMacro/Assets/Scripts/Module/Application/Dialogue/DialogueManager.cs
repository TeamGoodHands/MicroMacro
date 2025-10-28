using UnityEngine;
using System.Collections.Generic;
using UnityEngine;

namespace Module.Application.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private DialogueDatabase dialogueDatabase;
        
        private Queue<DialogueItem> dialogueQueue = new Queue<DialogueItem>();
        private bool isDisplaying;

        private void Update()
        {
            if (!isDisplaying && dialogueQueue.Count > 0)
            {
                DialogueItem item = dialogueQueue.Dequeue();
            }
        }

        public void Enqueue(string entryName)
        {
            DialogueItem item = dialogueDatabase.GetItem(entryName);
            if (item != null)
            {
                dialogueQueue.Enqueue(item);
            }
        }
    }
}