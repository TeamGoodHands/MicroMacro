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
        [SerializeField] private Image fuguSpeechBubble;
        [SerializeField] private Image playerSpeechBubble;
        [SerializeField] private TextMeshProUGUI fuguText;
        [SerializeField] private TextMeshProUGUI playerText;
        [Header("ウィンドウ表示、非表示にかかる時間")][SerializeField] private float playBackTime = 0.3f;
        [SerializeField] [Range(0f, 1f)] private float maxSize;
        
        private TextMeshProUGUI activeText;
        private Image activeSpeechBubble;
        private Vector3 defaultScale;
        
        private void Awake()
        {
            defaultScale = fuguSpeechBubble.rectTransform.localScale;
            
            fuguSpeechBubble.rectTransform.localScale = Vector3.zero;
            playerSpeechBubble.rectTransform.localScale = Vector3.zero;
            
            fuguSpeechBubble.gameObject.SetActive(false);
            playerSpeechBubble.gameObject.SetActive(false);
        }

        public void Display(DialogueItem item)
        {
            if (item == null)
            {
                Debug.LogError("itemの取得及びセリフ表示に失敗しました。");
                return;
            }
            
            SwitchSpeechBubble(item.isFuguDialogue);
            
            // TODO: コルーチンで一文字ずつ表示実装    
            PrintDialogue(item.Text); 
            DialogueAnimation(true, item.isFuguDialogue);
           
        }
        
        /// <summary>
        /// フグのセリフかプレイヤーのセリフかによって吹き出しの画像と位置が変わるので、切り替え可能に
        /// </summary>
        private void SwitchSpeechBubble(bool isFugu)
        {
            if (isFugu)
            {
                if (activeSpeechBubble == fuguSpeechBubble)
                    return;
                
                activeSpeechBubble = fuguSpeechBubble;
                activeText = fuguText;
                Debug.Log("Call SwitchSpeechBubble: isFugu = " + isFugu);
                fuguSpeechBubble.gameObject.SetActive(true);
                playerSpeechBubble.gameObject.SetActive(false);
            }
            else
            {
                if (activeSpeechBubble == playerSpeechBubble)
                    return;
                
                activeSpeechBubble = playerSpeechBubble;
                activeText = playerText;
                
                fuguSpeechBubble.gameObject.SetActive(false);
                playerSpeechBubble.gameObject.SetActive(true);
            }
        }

        private void DialogueAnimation(bool isOpen, bool isFugu)
        {
            // TODO: フグのセリフは右下から左上、プレイヤーのセリフは左下から右上にアニメーションさせたい。閉じる際はその逆
            if (isOpen)
            {
                if (isFugu)
                    fuguSpeechBubble.rectTransform.DOScale(defaultScale, playBackTime);
                else
                    playerSpeechBubble.rectTransform.DOScale(defaultScale, playBackTime);
            }
            else
            {
                if (isFugu)
                    fuguSpeechBubble.rectTransform.DOScale(Vector3.zero, playBackTime);
                else
                    playerSpeechBubble.rectTransform.DOScale(Vector3.zero, playBackTime);
            }
        }

        public async UniTask HideAsync(CancellationToken cancellationToken)
        {
            bool isFugu = GetActiveImageIsFugu();
            DialogueAnimation(false, isFugu);
                    
            // アニメーション終了まで待機
            await UniTask.Delay(TimeSpan.FromSeconds(playBackTime), cancellationToken: cancellationToken);

            // オブジェクト破棄後等でなければ実行
            if (!cancellationToken.IsCancellationRequested)
            {
                activeSpeechBubble.gameObject.SetActive(false);
            }
        }
        
        private void PrintDialogue(string text)
        {
            activeText.text = text;
        }
        
        private bool GetActiveImageIsFugu()
        {
            return activeSpeechBubble == fuguSpeechBubble;
        }

    }
}