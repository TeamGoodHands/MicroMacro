using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Module.Application.Dialogue
{
    public class StartDialogue : MonoBehaviour
    {
        [SerializeField] private string[] dialogues;
        [SerializeField] private float delayTime;
        [SerializeField] private DialogueManager dialogueManager;

        private async void Start()
        {
            await DelayEnqueue();
        }

        private async UniTask DelayEnqueue()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
            dialogueManager.EnqueueDialogues(dialogues);
        }
    }
}