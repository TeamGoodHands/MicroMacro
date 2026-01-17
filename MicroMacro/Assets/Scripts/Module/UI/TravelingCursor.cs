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

        private void Update()
        {
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
            // ここでDispose()してしまうと、直後の非同期処理内で
            // トークンチェックした時にエラーになることがあるため、Cancelのみ行い、
            // DisposeはGCに任せるか、確実に終わったタイミングで行うのが安全。
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
                
                // キャンセルされていたら「待機アニメ」には移行させない
                if (token.IsCancellationRequested) 
                    return;

                // 正常に完走した（キャンセルされていない）場合のみ、待機アニメに戻す
                animator.Play(idleSprites, animationFps);
            }
            catch (System.OperationCanceledException)
            {
                // キャンセル時は何もしない（次のアニメーションが既に再生されているため）
            }
        }
    }
}