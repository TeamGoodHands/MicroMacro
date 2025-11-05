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

        // 念のためセリフ単体のEnqueueも外部から呼び出し可能に。
        public void Enqueue(string itemName)
        {
            if (DialogueDatabase.Instance == null)
            {
                Debug.LogError("[DialogueManager] DialogueDatabaseが初期化されていません。");
                return;
            }
            
            DialogueItem item = DialogueDatabase.Instance.GetItem(itemName);
            if (item != null)
            {
                dialogueQueue.Enqueue(item);
                Debug.Log("Enqueue: " + item.EntryName);
                if (!isDisplaying)
                {
                    ProcessQueueAsync().Forget();
                }
            }
        }
        
        /// <summary>
        /// まとめて複数のセリフをキューに入れたい場合の関数。
        /// </summary>
        public void EnqueueDialogues(String[] dialogues)
        {
            foreach (var dialogue in dialogues)
            {
                Enqueue(dialogue);
            }
        }

        public void ClearQueue()
        {
            if (dialogueQueue.Count > 0)
            {
                dialogueQueue.Clear();
                Debug.Log("Queueがクリアされました。");
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