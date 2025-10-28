using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Application.Dialogue
{
     public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject dialogueWindow;
        [SerializeField] private Image characterIcon;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Animator dialogueAnimator;
        [SerializeField] private float hideDelay = 0.8f;
        
        private void Awake()
        {
            dialogueWindow.SetActive(false);
        }

        public void Display(DialogueItem item)
        {
            if (!dialogueWindow.activeSelf)
            {
                dialogueWindow.SetActive(true);
                // dialogueAnimator.SetBool("IsOpen", true);
            }
            dialogueText.text = item.Text; 
            // characterIcon.sprite = item.characterIcon;
        }

        public async UniTask HideAsync(CancellationToken cancellationToken)
        {
            // dialogueAnimator.SetBool("IsOpen", false);

            // アニメーション終了まで待機
            await UniTask.Delay(TimeSpan.FromSeconds(hideDelay), cancellationToken: cancellationToken);

            // オブジェクト破棄後等でなければ実行
            if (!cancellationToken.IsCancellationRequested)
            {
                dialogueWindow.SetActive(false);
            }
        }

    }
}