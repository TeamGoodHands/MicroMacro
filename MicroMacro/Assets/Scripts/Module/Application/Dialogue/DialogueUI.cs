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
        
        // 各吹き出しの初期スケールを保持
        private Vector3 fuguDefaultScale;
        private Vector3 playerDefaultScale;
        
        // 現在操作中のオブジェクト
        private Image currentBubble;
        private TextMeshProUGUI currentText;
        private Vector3 currentTargetScale;
        
        private CancellationTokenSource cts;

        private void Awake()
        {
            // 初期スケールを個別に保存 現状同じ値
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

            // テキストをページ分割（長文対応）
            List<string> pages = SplitTextToPages(item.Text, maxCharsPerPage);

            // 最初のページをセットし、文字数0（透明）にしておく
            currentText.text = pages[0];
            currentText.maxVisibleCharacters = 0;
            
            currentBubble.gameObject.SetActive(true);
            await PlayOpenAnimationAsync(token);
            
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

                // 現在表示された文字を取得
                char currentChar = text[i - 1];

                // 句読点判定：句読点なら長く待つ、それ以外は通常の速度
                bool isPunctuation = IsPunctuation(currentChar);
                float waitTime = isPunctuation ? punctuationDelay : textSpeed;

                await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
            }
        }

        /// <summary>
        /// 文字列をページ分割する。
        /// 区切りがいい（句読点がある）なら多少超過しても許容するように。
        /// </summary>
        private List<string> SplitTextToPages(string text, int maxChars)
        {
            var list = new List<string>();
            int currentPos = 0;

            while (currentPos < text.Length)
            {
                // 残りの文字数がmaxChars以下なら、すべて追加して終了
                if (text.Length - currentPos <= maxChars)
                {
                    list.Add(text.Substring(currentPos));
                    break;
                }

                // 基本の分割位置
                int splitLength = maxChars;
                
                // 超過を許容して区切り文字（句読点など）を探す
                // maxCharsの位置から、maxOverrunChars分だけ先をチェック
                bool foundSplitChar = false;
                for (int offset = 0; offset <= maxOverrunChars; offset++)
                {
                    int checkIndex = currentPos + maxChars + offset;

                    // テキストの範囲外ならループ終了
                    if (checkIndex >= text.Length) break;

                    // 区切り文字が見つかったら、そこで切る（その文字を含めるため +1）
                    if (IsSplitPosition(text[checkIndex]))
                    {
                        splitLength = maxChars + offset + 1;
                        foundSplitChar = true;
                        break;
                    }
                }
                
                // ここで句読点の直前に改ページみたいな、「手前」を探す処理を入れても良い。

                list.Add(text.Substring(currentPos, splitLength));
                currentPos += splitLength;
            }

            return list;
        }

        /// <summary>
        /// 句読点の判定
        /// </summary>
        private bool IsPunctuation(char c)
        {
            return "、。！？!?,.".IndexOf(c) >= 0;
        }
        
        /// <summary>
        /// 区切りが良い文字かどうか判定（ページ切り替え時の判断）
        /// </summary>
        private bool IsSplitPosition(char c)
        {
            // 句読点、感嘆符、スペース、閉じ括弧などを区切りとみなす
            return "、。！？!?,. 　」』)".IndexOf(c) >= 0;
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