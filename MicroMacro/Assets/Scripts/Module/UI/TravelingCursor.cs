using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks; // 追加

namespace Module.UI
{
    [RequireComponent(typeof(UIFrameAnimator))] // アニメーター必須
    public class TravelingCursor : MonoBehaviour
    {
        [Header("移動設定")]
        [SerializeField] private RectTransform cursorRectTransform;
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutExpo;

        [Header("アニメーション設定")]
        [SerializeField] private Sprite[] idleSprites;   // 待機中のパラパラ画像
        [SerializeField] private Sprite[] movingSprites; // 移動中のパラパラ画像
        [SerializeField] private float animationFps = 4f;

        [Header("オプション")]
        [SerializeField] private bool limitToAnimButtons = true;

        private UIFrameAnimator animator;
        private GameObject lastTargetObject;

        private void Awake()
        {
            animator = GetComponent<UIFrameAnimator>();
        }

        private void Start()
        {
            // 最初は待機アニメ
            animator.Play(idleSprites, animationFps);
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

            // 非同期で移動処理を開始
            MoveSequenceAsync(currentSelected).Forget();
        }

        private async UniTaskVoid MoveSequenceAsync(GameObject target)
        {
            var targetRect = target.GetComponent<RectTransform>();
            if (targetRect == null) return;

            // 移動アニメーションに切り替え
            animator.Play(movingSprites, animationFps);
            
            cursorRectTransform.DOKill();
            await cursorRectTransform.DOMove(targetRect.position, moveDuration)
                .SetEase(moveEase)
                .SetLink(gameObject)
                .ToUniTask(); // UniTaskで待機可能にする

            //  移動が終わったら待機アニメーションに戻す
            // (移動中に別のターゲットに移った場合、この処理はキャンセルされないが、
            //  次のMoveSequenceAsyncが即座に上書きするので問題ない)
            animator.Play(idleSprites, animationFps);
        }
    }
}