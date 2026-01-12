using System;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Module.Player.Component;

namespace Module.Application.Dialogue
{
    /// <summary>
    /// セリフ再生のロジック管理
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private PlayerStatus playerStatus;
    
        private readonly Queue<DialogueItem> dialogueQueue = new Queue<DialogueItem>();
        private bool isDisplaying;
        private bool isClearRequested = false;
        
        private void HandlePlayerOnDeath()
        {
            AbortDialogue(isImmediate: true);
        }

        private void Awake()
        {
            if (playerStatus == null)
            {
                Debug.LogError("[DialogueManager] PlayerStatusが設定されていません。");
                return;
            }

            playerStatus.OnDeath += HandlePlayerOnDeath;
        }

        private void OnDestroy()
        {
            if (playerStatus != null)
            {
                playerStatus.OnDeath -= HandlePlayerOnDeath;
            }
        }


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
                // Debug.Log("Enqueue: " + item.EntryName);
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
                
                await dialogueUI.ShowDialogueAsync(currentItem);
                
                await UniTask.Delay(TimeSpan.FromSeconds(currentItem.DisplayTime),
                    cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            // isClearRequestedがtrueまたはqueueが空でwindowを閉じる
            if (isClearRequested || dialogueQueue.Count == 0)
            {
                await dialogueUI.HideAsync();
            }

            isDisplaying     = false;
            isClearRequested = false;
        }

        /// <summary>
        /// 現在のセリフを強制終了
        /// </summary>
        /// <param name="isImmediate">trueならアニメーションなしで即消し（死亡時など）</param>
        public void AbortDialogue(bool isImmediate = false)
        {
            // 待機中のセリフをすべて破棄
            ClearQueue();
            
            isClearRequested = true;
            isDisplaying = false; 

            // UI側に閉じる命令を出す
            if (isImmediate)
            {
                dialogueUI.HideImmediate();
            }
            else
            {
                // asyncメソッドを同期メソッドから呼ぶのでForgetする
                dialogueUI.HideAsync().Forget();
            }
        }
    }
}