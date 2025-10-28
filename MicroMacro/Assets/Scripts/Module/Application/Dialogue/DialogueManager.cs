using System;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Module.Application.Dialogue
{
    /// <summary>
    /// セリフ再生のロジック管理
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;
        
        private readonly Queue<DialogueItem> dialogueQueue = new Queue<DialogueItem>();
        private bool isDisplaying;
        private bool isClearRequested = false;
        
        public void Enqueue(string itemName)
        {
            DialogueItem item = DialogueDatabase.Instance.GetItem(itemName);
            if (item != null)
            {
                dialogueQueue.Enqueue(item);
                if (!isDisplaying)
                {
                    ProcessQueueAsync().Forget();
                }
            }
        }

        public void ClearQueue()
        {
            if (dialogueQueue.Count > 0)
            {
                dialogueQueue.Clear();
            }

            // 表示中のセリフがあったら中断フラグ立てる
            if (isDisplaying)
            {
                isClearRequested = true;
            }
        }

        /// <summary>
        /// Queueを処理するメイン部分
        /// </summary>
        private async UniTaskVoid ProcessQueueAsync()
        {
            isDisplaying = true;

            while (dialogueQueue.Count > 0)
            {
                if (isClearRequested)
                    break;

                DialogueItem currentItem = dialogueQueue.Dequeue();
                dialogueUI.Display(currentItem);

                await UniTask.Delay(TimeSpan.FromSeconds(currentItem.DisplayTime),
                    cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            // isClearRequestedがtrueまたはqueueが空でwindowを閉じる
            if (isClearRequested || dialogueQueue.Count == 0)
            {
                await dialogueUI.HideAsync(this.GetCancellationTokenOnDestroy());
            }

            isDisplaying     = false;
            isClearRequested = false;
        }
    }
}