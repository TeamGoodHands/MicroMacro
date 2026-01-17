using UnityEngine;
using UnityEngine.EventSystems; 
using DG.Tweening;             
using Cysharp.Threading.Tasks; 
using System.Threading;        

namespace Module.UI
{
    public class SlideButtonAnim : ButtonAnimBase
    {
        [SerializeField]　private RectTransform rectTransform;

        [Header("スライド設定 (DOTween)")]
        [SerializeField] private float slideDistance = 30f;
        [Tooltip("アニメーションにかかる時間（秒）")]
        [SerializeField] private float duration = 0.2f;         // 速度ではなく時間で指定するのがDOTween
        [SerializeField] private Ease easeType = Ease.OutQuad;  // 動きの緩急を決める設定（Out～系は最初早く最後に減速） 

        private Vector2 originalPosition;

        // キャンセル制御用
        private CancellationTokenSource cts;

        void Awake()
        {
            originalPosition = rectTransform.anchoredPosition;
        }
        
        void OnEnable()
        {
            // 有効化されたときに位置を即座に戻す（前回の動きが残らないように）
            rectTransform.anchoredPosition = originalPosition;
            
            // 内部的に選ばれてるならボタンならアニメーション開幕再生
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                // 親クラスの OnSelect を呼ぶことで、
                // 「isSelectedの更新」と「OnActiveの実行」を同時に
                OnSelect(null); 
            }
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        protected override void OnActive()
        {
            // 右の位置を計算して非同期実行
            Vector2 target = originalPosition + new Vector2(slideDistance, 0);
            MoveToTargetAsync(target).Forget();
        }

        protected override void OnInactive()
        {
            // 元の位置へ非同期実行
            MoveToTargetAsync(originalPosition).Forget();
        }

        private async UniTaskVoid MoveToTargetAsync(Vector2 target)
        {
            // 既に破棄処理が始まっていたら何もしない（エラー防止の念押し）
            if (this == null || gameObject == null) return;
            
            // これをやらないと、マウスを高速で出し入れした時に挙動がおかしくなる
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            // 新しい動きを始める前に、今の動きを止める
            // (SetLinkしていても、上書き時はKillしておくと安全)
            rectTransform.DOKill();

            try
            {
                // DOTweenを実行し、UniTaskで待機する
                // .ToUniTask にトークンを渡すと、cts.Cancel() が呼ばれたら即座に停止する
                await rectTransform.DOAnchorPos(target, duration)
                    .SetEase(easeType)
                    .SetLink(gameObject) // GameObjectが破棄されたらTweenも自動破棄する
                    .ToUniTask(cancellationToken: cts.Token);
            }
            catch (System.OperationCanceledException)
            {
                // キャンセルされた（＝逆方向のアニメーションが始まった）場合は
                // 例外をつぶして終了
            }
        }
    }
}