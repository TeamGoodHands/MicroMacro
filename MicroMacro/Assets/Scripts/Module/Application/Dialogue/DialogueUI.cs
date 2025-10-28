using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Application.Dialogue
{
     public class DialogueUI : MonoBehaviour
    {
        

        [SerializeField] private DialogueCollection[] dialogueCollection;
        private readonly Dictionary<string, DialogueItem> dataDictionary = new();

        [SerializeField] private Image image;
        [SerializeField] private TextMeshPro TMpro;
        [SerializeField] private Animator dialogueAnim;
        [SerializeField] private bool systemMessage;

        public event Action OnTaskClear;
        
        private void Awake()
        {
            // セリフ表示用とシステムメッセージ表示用で分ける
            InitializeDataDictionary();
            image.enabled = false;
        }

        private void InitializeDataDictionary()
        {
            for (int i = 0; i < dialogueCollection.Length; i++)
            {
                for (int j = 0; j < dialogueCollection[i].items.Length; j++)
                {
                    var dialogueData = dialogueCollection[i].items[j];
                    if (dialogueData != null && !string.IsNullOrEmpty(dialogueData.EntryName))
                    {
                        dataDictionary.TryAdd(dialogueData.EntryName, dialogueData);
                    }
                }
            }
        }

        private float elapsedTime;
        private bool isShowing; 
        private DialogueItem currentDialogue;
        private void Update()
        {
            if (!isShowing && dialogueQueue.Count > 0)
            {
                currentDialogue = dialogueQueue.Dequeue();
                Display(currentDialogue.Text);
                isShowing = true;
            }
            
            if (isShowing)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime > currentDialogue.DisplayTime / 2f)
                {
                    // 表示時間の半分でサウンド停止   
                    // SoundManager.instance.StopPlay("Speak");
                }

                if (elapsedTime > currentDialogue.DisplayTime)
                {
                    CheckHide();
                    isShowing = false;
                    elapsedTime = 0f;
                }
            }
        }

        private void Display(String dialogue)
        {
           // image.sprite = dialogue;
            TMpro.text = dialogue;
            if (!image.enabled)
            {
                if (!systemMessage)
                {
                  //  dialogueAnim.SetBool("Open", true);
                  //  SoundManager.instance.DelayPlay("Speak", 0.4f);   // アニメーションに合わせてセリフのサウンドも遅延かける
                }
                
                image.enabled = true;
            }
          
           // SoundManager.instance.Play("Speak");   // しゃべるのはセリフの時だけ
            
        }

        private void CheckHide()
        {
            if (image.sprite == null)
                return;
            
            // 一連のセリフ表示の最後ならimageの表示off
            if (dialogueQueue.Count == 0)
            {
                if (!systemMessage)
                // dialogueAnim.SetBool("Open", false);
                
                StartCoroutine(DelayHide());
            }
        }
        
        public void Enqueue(string name)
        {
            DialogueItem item = GetDialogueData(name);
            if (item == null) return;
            
            dialogueQueue.Enqueue(item);
        }

        public void ClearQueue()
        {
            if (isShowing)
            {
                /*SoundManager.instance.StopPlay("Speak");
                if (!systemMessage)
                    dialogueAnim.SetBool("Open", false);*/
                
                StartCoroutine(DelayHide());
                isShowing = false;
                elapsedTime = 0f;
            }
            
            dialogueQueue.Clear();
            OnTaskClear?.Invoke();
            
            Debug.Log("キューがクリアされました");
         
        }
        
        private DialogueItem GetDialogueData(string name)
        {
            if (dataDictionary.TryGetValue(name, out DialogueItem data))
            {
                return data;
            }
            else
            {
                Debug.Log("データの取得に失敗");
                return null;
            }
        }

        public IEnumerator DelayEnqueue(string name, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            Enqueue(name);
            yield return null;
        }

        private IEnumerator DelayHide()
        {
            yield return new WaitForSeconds(0.8f);
            image.enabled = false;
        }
    }
}