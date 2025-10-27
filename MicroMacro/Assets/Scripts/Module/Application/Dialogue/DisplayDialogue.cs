using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Module.Application.Dialogue
{
     public class DisplayDialogue : MonoBehaviour
    {
        public static DisplayDialogue dialogue;
        
        Queue<DialogueData> task = new Queue<DialogueData>();

        [SerializeField] private DialogueObjects[] dialogueObjects;
        private readonly Dictionary<string, DialogueData> dataDictionary = new();

        [SerializeField] private Image image;
        [SerializeField] private TextMeshPro TMpro;
        [SerializeField] private Animator dialogueAnim;
        [SerializeField] private bool systemMessage;

        public event Action OnTaskClear;
        
        private void Awake()
        {
            // セリフ表示用とシステムメッセージ表示用で分ける
            dialogue = this;
            InitializeDataDictionary();
            image.enabled = false;
        }

        private void InitializeDataDictionary()
        {
            for (int i = 0; i < dialogueObjects.Length; i++)
            {
                for (int j = 0; j < dialogueObjects[i].data.Length; j++)
                {
                    var dialogueData = dialogueObjects[i].data[j];
                    if (dialogueData != null && !string.IsNullOrEmpty(dialogueData.Name))
                    {
                        dataDictionary.TryAdd(dialogueData.Name, dialogueData);
                    }
                }
            }
        }

        private float elapsedTime;
        private bool isShowing; 
        private DialogueData data;
        private void Update()
        {
            if (!isShowing && task.Count > 0)
            {
                 data = task.Dequeue();
                Display(data.Dialogue);
                isShowing = true;
            }
            
            
            if (isShowing)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime > data.DisplayTime / 2f)
                {
                    // 表示時間の半分でサウンド停止   
                    // SoundManager.instance.StopPlay("Speak");
                }

                if (elapsedTime > data.DisplayTime)
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
            if (task.Count == 0)
            {
                if (!systemMessage)
                // dialogueAnim.SetBool("Open", false);
                
                StartCoroutine(DelayHide());
            }
        }
        
        public void Enqueue(string name)
        {
            DialogueData _data = GetDialogueData(name);
            if (_data == null) return;
            
            task.Enqueue(_data);
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
            
            task.Clear();
            OnTaskClear?.Invoke();
            
            Debug.Log("キューがクリアされました");
         
        }
        
        private DialogueData GetDialogueData(string name)
        {
            if (dataDictionary.TryGetValue(name, out DialogueData data))
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