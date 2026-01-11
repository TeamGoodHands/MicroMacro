using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Module.Application.Dialogue
{
     public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject dialogueWindow;
        [SerializeField] private Image characterIcon;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject dialogueTextObject;
        [Header("ウィンドウ表示、非表示にかかる時間")][SerializeField] private float playBackTime = 0.3f;
        [SerializeField] [Range(0f, 1f)] private float maxSize;
        
        private void Awake()
        {
            if (dialogueWindow == null)
                Debug.LogAssertion("dialogueWindowが未設定です。");
            
            dialogueWindow.SetActive(false);
            dialogueWindow.transform.localScale = Vector3.zero;
        }

        public void Display(DialogueItem item)
        {
            if (item == null)
            {
                Debug.LogError("itemの取得及びセリフ表示に失敗しました。");
                return;
            }
            
            dialogueText.text = item.Text; 
            characterIcon.sprite = item.characterIcon;
            if (!dialogueWindow.activeSelf)
            {
                dialogueWindow.SetActive(true);
                dialogueWindow.transform.DOScale(new Vector3(maxSize, maxSize, 1f), playBackTime);
            }
        }

        public async UniTask HideAsync(CancellationToken cancellationToken)
        {
            dialogueWindow.transform.DOScale(Vector3.zero, playBackTime);
                    
            // アニメーション終了まで待機
            await UniTask.Delay(TimeSpan.FromSeconds(playBackTime), cancellationToken: cancellationToken);

            // オブジェクト破棄後等でなければ実行
            if (!cancellationToken.IsCancellationRequested)
            {
                dialogueWindow.SetActive(false);
            }
        }

    }
}