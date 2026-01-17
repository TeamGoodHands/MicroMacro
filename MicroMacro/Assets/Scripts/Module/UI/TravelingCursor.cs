using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Module.UI
{
    [RequireComponent(typeof(UIFrameAnimator))]
    public class TravelingCursor : MonoBehaviour
    {
        [Header("移動設定")]
        [SerializeField] private RectTransform cursorRectTransform;
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutExpo;

        [Header("アニメーション設定")]
        [SerializeField] private Sprite[] idleSprites;
        [SerializeField] private Sprite[] movingSprites;
        [SerializeField] private float animationFps = 4f;

        [Header("オプション")]
        [SerializeField] private bool limitToAnimButtons = true;

        private UIFrameAnimator animator;
        private GameObject lastTargetObject;
        
        private CancellationTokenSource moveSequenceCts;
        
        // 追加: 外部から制御するための停止フラグ
        private bool isPaused = false;

        private void Awake()
        {
            animator = GetComponent<UIFrameAnimator>();
        }

        private void Start()
        {
            animator.Play(idleSprites, animationFps);
        }

        private void OnDestroy()
        {
            moveSequenceCts?.Cancel();
            moveSequenceCts?.Dispose();
        }

        // 追加: 外部からカーソルの動きを制御するメソッド
        public void SetPaused(bool paused)
        {
            isPaused = paused;
            
            if (isPaused)
            {
                // 停止時は現在のアニメーションやTweenも止める
                moveSequenceCts?.Cancel();
                cursorRectTransform.DOKill();
            }
        }

        private void Update()
        {
            // 追加: ポーズ中は処理しない
            if (isPaused) return;

            if (EventSystem.current == null) return;
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected == null || currentSelected == lastTargetObject) return;

            // ターゲット更新
            lastTargetObject = currentSelected;

            if (limitToAnimButtons)
            {
                if (currentSelected.GetComponent<ButtonAnimBase>() == null) return;
            }

            // 古い移動シーケンスをキャンセル
            moveSequenceCts?.Cancel();
            moveSequenceCts = new CancellationTokenSource();

            // 新しい移動を開始
            MoveSequenceAsync(currentSelected, moveSequenceCts.Token).Forget();
        }

        private async UniTaskVoid MoveSequenceAsync(GameObject target, CancellationToken token)
        {
            var targetRect = target.GetComponent<RectTransform>();
            if (targetRect == null) return;

            // 移動アニメーションに切り替え
            animator.Play(movingSprites, animationFps);
            
            // 古いTweenをキル
            cursorRectTransform.DOKill();
            
            try 
            {
                // 移動開始
                await cursorRectTransform.DOMove(targetRect.position, moveDuration)
                    .SetEase(moveEase)
                    .SetLink(gameObject)
                    .ToUniTask(cancellationToken: token); 
                
                if (token.IsCancellationRequested) return;

                animator.Play(idleSprites, animationFps);
            }
            catch (System.OperationCanceledException)
            {
                // キャンセル時は何もしない
            }
        }
    }
}