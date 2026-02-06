using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMP_Ruby;

namespace Module.Application.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image fuguSpeechBubble;
        [SerializeField] private Image playerSpeechBubble;
        
        
        // アニメーション用でTMPの参照を残しておく
        [SerializeField] private TextMeshProUGUI fuguText;
        [SerializeField] private TextMeshProUGUI playerText;
        
        [SerializeField] private TextMeshProRuby fuguRuby;
        [SerializeField] private TextMeshProRuby playerRuby;

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
        private TextMeshProRuby currentRuby; 
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
            if (item == null)
                return;

            // 前の処理をキャンセル
            CancelCurrentProcess();

            cts = new CancellationTokenSource();
            var token = cts.Token;
            
            // 操作対象を決定（フグorプレイヤー）
            SetupActiveObjects(item.isFuguDialogue);

            // プロセッサでページ分割
            List<string> pages = textProcessor.SplitTextToPages(item.Text, maxCharsPerPage, maxOverrunChars);

            if (pages.Count == 0)
                return;
            
            // 最初のページをセット
            // Ruby経由でセットすることでタグ変換を行う
            currentRuby.Text = pages[0];
            
            // 最初は非表示
            currentText.maxVisibleCharacters = 0;
            
            currentBubble.gameObject.SetActive(true);
            
            // 開くアニメーション待機
            await PlayOpenAnimationAsync(token);
            
            // アニメーション待機中にDestroy/Cancelされた場合、ここで処理を止める。
            if (token.IsCancellationRequested || currentBubble == null)
                return;
            
            // ふわふわ浮かす
            StartFloatingAnimation();
            
            // ページごとにタイプライター演出を実行
            for (int i = 0; i < pages.Count; i++)
            {
                // ページ切り替え時にテキストを更新
                currentRuby.Text = pages[i];
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
                currentRuby   = fuguRuby; // Rubyコンポーネントも切り替え
                
                currentTargetScale = fuguDefaultScale; 
                currentTargetPos   = fuguDefaultPos;
            }
            else
            {
                CloseSpeechBubble(fuguSpeechBubble);
                currentBubble = playerSpeechBubble;
                currentText   = playerText;
                currentRuby   = playerRuby; // Rubyコンポーネントも切り替え
                
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
          
            if (currentBubble == null)
                return;

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
            // ここも念のためチェック
            if (currentBubble == null) return;
            
            var rect = currentBubble.rectTransform;
            
            rect.localScale = Vector3.zero;

            // Pivotの設定はエディタ側で行うため、ここでは純粋にスケールのみ実行
            await rect.DOScale(currentTargetScale, animationDuration)
                .SetEase(Ease.OutBack)
                .ToUniTask(cancellationToken: token);
        }

        private async UniTask PlayTypewriterEffectAsync(string text, CancellationToken token)
        {
            // TMPの情報を強制更新して、正しい文字数情報(textInfo)を生成させる
            currentText.ForceMeshUpdate();

            // 実際に画面に表示される文字の総数を取得（ルビ文字も含まれる）
            int totalVisibleCharacters = currentText.textInfo.characterCount;

            // 表示される文字数分だけループする
            // これにより、タグの文字列（<r=...>など）は無視され、表示上の文字のみで進行
            for (int i = 1; i <= totalVisibleCharacters; i++)
            {
                currentText.maxVisibleCharacters = i;
                
                if (token.IsCancellationRequested) 
                    return;

                // 配列範囲外アクセス対策
                // 非同期中にテキストが更新/クリアされた場合、characterInfoのサイズが変わっている可能性があるためチェック
                if (i - 1 >= currentText.textInfo.characterInfo.Length)
                {
                    break;
                }

                // 現在表示させた文字の情報を取得（句読点判定のため）
                // indexは 0 始まりなので i - 1
                TMP_CharacterInfo charInfo = currentText.textInfo.characterInfo[i - 1];
                char currentChar = charInfo.character;

                // 句読点なら待機時間を長くする
                bool isPunctuation = textProcessor.IsPunctuation(currentChar);
                float waitTime = isPunctuation ? punctuationDelay : textSpeed;

                await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
            }
            
            // 念のため最後に全表示（計算誤差などで最後が漏れるのを防ぐ）
            currentText.maxVisibleCharacters = 99999;
        }

        public async UniTask HideAsync()
        {
            CancelCurrentProcess();
            
            cts = new CancellationTokenSource();
            var token = cts.Token;

            if (currentBubble == null || !currentBubble.gameObject.activeSelf) return;

            // 閉じるアニメーション
            await currentBubble.rectTransform
                .DOScale(Vector3.zero, animationDuration)
                .SetEase(Ease.InBack)
                .ToUniTask(cancellationToken: token);
            
            currentBubble.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// アニメーションなしで即非表示するメソッド。(死亡時やシーン遷移用)
        /// </summary>
        public void HideImmediate()
        {
            CancelCurrentProcess(); // タスクキャンセル

            // DOKillをSetActive(false)より先に呼ぶように変更
            // 無効化されたオブジェクトのTween操作によるエラー回避
            if (fuguSpeechBubble != null && fuguSpeechBubble.gameObject.activeSelf) 
            {
                fuguSpeechBubble.rectTransform.DOKill();
                fuguSpeechBubble.gameObject.SetActive(false);
            }
        
            if (playerSpeechBubble != null && playerSpeechBubble.gameObject.activeSelf) 
            {
                playerSpeechBubble.rectTransform.DOKill();
                playerSpeechBubble.gameObject.SetActive(false);
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
            // CancelCurrentProcessを呼ぶ前にTweenを明示的にKillする
            // UniTaskのキャンセル処理とOnDestroyによるDOTweenの自動破棄が競合して
            // IndexOutOfRangeExceptionが発生するのを防ぐため
            if (fuguSpeechBubble != null) fuguSpeechBubble.rectTransform.DOKill();
            if (playerSpeechBubble != null) playerSpeechBubble.rectTransform.DOKill();
            if (currentBubble != null) currentBubble.rectTransform.DOKill();

            CancelCurrentProcess();
        }
    }
}