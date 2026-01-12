using System;
using System.Collections.Generic;
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
        [Header("吹き出しの開閉アニメーション時間")] [SerializeField] private float animationDuration = 0.3f;
        [Header("通常の文字送り速度")] [SerializeField] private float textSpeed = 0.05f;        
        [Header("句読点での待機時間")] [SerializeField] private float punctuationDelay = 0.4f; 
        [Header("1ページあたりの最大文字数")] [SerializeField] private int maxCharsPerPage = 20;     
        [Header("ページ送り時の待機時間")][SerializeField] private float pageAutoAdvanceDelay = 0.1f; 
        [Header("ページ分割時に許容する最大超過文字数")] [SerializeField] private int maxOverrunChars = 5;
        
        // 各吹き出しの初期スケールと位置を保持（アニメーションで使用)
        private Vector3 fuguDefaultScale;
        private Vector3 playerDefaultScale;
        
        private Vector2 fuguDefaultPos;
        private Vector2 playerDefaultPos;
        private Vector2 currentTargetPos; 
        
        // 現在操作中のオブジェクト
        private Image currentBubble;
        private TextMeshProUGUI currentText;
        private Vector3 currentTargetScale;
        
        private CancellationTokenSource cts;
        
        private readonly DialogueTextProcessor textProcessor = new DialogueTextProcessor();

        private void Awake()
        {
            // 初期スケールを個別に保存 現状同じ値
            fuguDefaultScale = fuguSpeechBubble.rectTransform.localScale;
            playerDefaultScale = playerSpeechBubble.rectTransform.localScale;
            
            fuguDefaultPos = fuguSpeechBubble.rectTransform.anchoredPosition;
            playerDefaultPos = playerSpeechBubble.rectTransform.anchoredPosition;
            
            InitializeBubble(fuguSpeechBubble);
            InitializeBubble(playerSpeechBubble);
        }

        private void InitializeBubble(Image bubble)
        {
            bubble.rectTransform.localScale = Vector3.zero;
            bubble.gameObject.SetActive(false);
        }

        /// <summary>
        /// 会話を表示するメイン処理
        /// </summary>
        public async UniTask ShowDialogueAsync(DialogueItem item)
        {
            if (item == null) return;

            // 前の処理をキャンセル
            CancelCurrentProcess();

            cts = new CancellationTokenSource();
            var token = cts.Token;
            
            // 操作対象を決定（フグorプレイヤー）
            SetupActiveObjects(item.isFuguDialogue);

            // プロセッサでページ分割
            List<string> pages = textProcessor.SplitTextToPages(item.Text, maxCharsPerPage, maxOverrunChars);

            // 最初のページをセットし、文字数0（透明）にしておく
            currentText.text = pages[0];
            currentText.maxVisibleCharacters = 0;
            
            currentBubble.gameObject.SetActive(true);
            await PlayOpenAnimationAsync(token);
            
            // ふわふわ浮かす
            StartFloatingAnimation();
            
            // ページごとにタイプライター演出を実行
            for (int i = 0; i < pages.Count; i++)
            {
                // ページ切り替え時にテキストを更新してリセット
                currentText.text = pages[i];
                currentText.maxVisibleCharacters = 0;
                
                await PlayTypewriterEffectAsync(pages[i], token);

                // 最後のページでなければ、読み終わりの待機時間を挟んで次へ
                if (i < pages.Count - 1)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(pageAutoAdvanceDelay), cancellationToken: token);

                    // クリック待ちにしたい場合
                    // await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0), cancellationToken: token);
                }
            }
        }
        
        /// <summary>
        /// ウィンドウ切り替え
        /// </summary>
        private void SetupActiveObjects(bool isFugu)
        {
            if (isFugu)
            {
                CloseSpeechBubble(playerSpeechBubble);
                currentBubble = fuguSpeechBubble;
                currentText   = fuguText;
                
                currentTargetScale = fuguDefaultScale; 
                currentTargetPos   = fuguDefaultPos;
            }
            else
            {
                CloseSpeechBubble(fuguSpeechBubble);
                currentBubble = playerSpeechBubble;
                currentText   = playerText;
                
                currentTargetScale = playerDefaultScale;
                currentTargetPos   = playerDefaultPos; 
            }
        }

        // 即閉じ
        private void CloseSpeechBubble(Image speechBubble)
        {
            if (speechBubble.gameObject.activeSelf)
            {
                speechBubble.gameObject.SetActive(false);
                speechBubble.rectTransform.localScale = Vector3.zero;
            }
        }
        
        private void StartFloatingAnimation()
        {
            // 多重起動防止 & 位置リセット
            currentBubble.rectTransform.DOKill();
            currentBubble.rectTransform.anchoredPosition = currentTargetPos;

            // 上下にふわふわさせる
            // Y座標を +10 くらい移動させて戻す
            float floatingRange = 10f; 
            float cycleDuration = 1.0f;

            currentBubble.rectTransform
                .DOAnchorPosY(currentTargetPos.y + floatingRange, cycleDuration)
                .SetEase(Ease.InOutSine) // ゆったりした動き
                .SetLoops(-1, LoopType.Yoyo) // 無限往復
                .SetLink(currentBubble.gameObject);
        }

        private async UniTask PlayOpenAnimationAsync(CancellationToken token)
        {
            var rect = currentBubble.rectTransform;
            
            rect.localScale = Vector3.zero;

            // Pivotの設定はエディタ側で行うため、ここでは純粋にスケールのみ実行
            await rect.DOScale(currentTargetScale, animationDuration)
                .SetEase(Ease.OutBack)
                .ToUniTask(cancellationToken: token);
        }

        private async UniTask PlayTypewriterEffectAsync(string text, CancellationToken token)
        {
            int totalLength = text.Length;

            // 1文字ずつ表示文字数を増やしていく
            for (int i = 1; i <= totalLength; i++)
            {
                currentText.maxVisibleCharacters = i;
                if (token.IsCancellationRequested) return;

                // 現在の文字を取得
                char currentChar = text[i - 1];

                // 句読点なら待機時間を長くする
                bool isPunctuation = textProcessor.IsPunctuation(currentChar);
                float waitTime = isPunctuation ? punctuationDelay : textSpeed;

                await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
            }
        }

        public async UniTask HideAsync()
        {
            CancelCurrentProcess();

            if (currentBubble == null || !currentBubble.gameObject.activeSelf) return;

            // 閉じるアニメーション
            await currentBubble.rectTransform
                .DOScale(Vector3.zero, animationDuration)
                .SetEase(Ease.InBack)
                .ToUniTask();

            currentBubble.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// アニメーションなしで即非表示するメソッド。(死亡時やシーン遷移用)
        /// </summary>
        public void HideImmediate()
        {
            CancelCurrentProcess(); // タスクキャンセル

            // アニメーション待機せずに即非表示
            if (fuguSpeechBubble != null) 
            {
                fuguSpeechBubble.gameObject.SetActive(false);
                fuguSpeechBubble.rectTransform.DOKill(); // 動いているTweenも殺す
            }
        
            if (playerSpeechBubble != null) 
            {
                playerSpeechBubble.gameObject.SetActive(false);
                playerSpeechBubble.rectTransform.DOKill();
            }
        
            currentBubble = null;
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