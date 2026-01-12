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
        [Header("References")]
        [SerializeField] private Image fuguSpeechBubble;
        [SerializeField] private Image playerSpeechBubble;
        [SerializeField] private TextMeshProUGUI fuguText;
        [SerializeField] private TextMeshProUGUI playerText;

        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private float textSpeed = 0.05f;

        // 各吹き出しの初期スケール
        private Vector3 fuguDefaultScale;
        private Vector3 playerDefaultScale;
        
        // 現在操作中のオブジェクト
        private Image currentBubble;
        private TextMeshProUGUI currentText;
        private Vector3 currentTargetScale;

        private CancellationTokenSource cts;

        private void Awake()
        {
            // 初期スケールを個別に保存 現状同じなので片方でも問題ない
            fuguDefaultScale = fuguSpeechBubble.rectTransform.localScale;
            playerDefaultScale = playerSpeechBubble.rectTransform.localScale;
            
            InitializeBubble(fuguSpeechBubble);
            InitializeBubble(playerSpeechBubble);
        }

        private void InitializeBubble(Image bubble)
        {
            bubble.rectTransform.localScale = Vector3.zero;
            bubble.gameObject.SetActive(false);
        }

        public async UniTask ShowDialogueAsync(DialogueItem item)
        {
            if (item == null) return;

            // 前の処理をキャンセル
            CancelCurrentProcess();

            cts = new CancellationTokenSource();
            var token = cts.Token;
            
            // 操作対象を決定
            SetupActiveObjects(item.isFuguDialogue);

            // 表示する前にテキストをセットし、見えないようにしておく(0文字)
            currentText.text = item.Text;
            currentText.maxVisibleCharacters = 0;
            
            currentBubble.gameObject.SetActive(true);
            await PlayOpenAnimationAsync(token);
            
            // タイプライター演出開始
            await PlayTypewriterEffectAsync(item.Text, token);
        }
        
        private void SetupActiveObjects(bool isFugu)
        {
            if (isFugu)
            {
                CloseBubbleImmediate(playerSpeechBubble);
                currentBubble = fuguSpeechBubble;
                currentText = fuguText;
                currentTargetScale = fuguDefaultScale;
            }
            else
            {
                CloseBubbleImmediate(fuguSpeechBubble);
                currentBubble = playerSpeechBubble;
                currentText = playerText;
                currentTargetScale = playerDefaultScale;
            }
        }

        private void CloseBubbleImmediate(Image speechBubble)
        {
            if (speechBubble.gameObject.activeSelf)
            {
                speechBubble.gameObject.SetActive(false);
                speechBubble.rectTransform.localScale = Vector3.zero;
            }
        }

        private async UniTask PlayOpenAnimationAsync(CancellationToken token)
        {
            var rect = currentBubble.rectTransform;
            rect.localScale = Vector3.zero;

            await rect.DOScale(currentTargetScale, animationDuration)
                .SetEase(Ease.OutBack)
                .ToUniTask(cancellationToken: token);
        }

        private async UniTask PlayTypewriterEffectAsync(string text, CancellationToken token)
        {
            // ここでのテキストセットは念のため残しても良いが、
            // ShowDialogueAsyncですでに行っているのでループ処理だけでOK。
            
            int totalLength = text.Length;

            for (int i = 0; i <= totalLength; i++)
            {
                currentText.maxVisibleCharacters = i;
                if (token.IsCancellationRequested) return;
                
                await UniTask.Delay(TimeSpan.FromSeconds(textSpeed), cancellationToken: token);
            }
        }

        public async UniTask HideAsync()
        {
            CancelCurrentProcess();

            if (currentBubble == null || !currentBubble.gameObject.activeSelf) return;

            await currentBubble.rectTransform
                .DOScale(Vector3.zero, animationDuration)
                .SetEase(Ease.InBack)
                .ToUniTask();

            currentBubble.gameObject.SetActive(false);
        }

        private void CancelCurrentProcess()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        private void OnDestroy()
        {
            CancelCurrentProcess();
        }
    }
}