using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events; 
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Module.UI
{
    /// <summary>
    /// ボタン決定（Submit）時に、カーソルをボタン周囲で公転させながら収縮（サイズ0へ）させ、
    /// 完了後にイベントを発火するエフェクト
    /// </summary>
    public class SubmitOrbitEffect : MonoBehaviour, ISubmitHandler
    {
        [Header("ターゲット参照")]
        [Tooltip("動かす対象のRectTransform（カーソル画像など）")]
        [SerializeField] private RectTransform targetRectTransform;
        
        [Tooltip("画像切り替え用のアニメーター（アタッチされていれば設定）")]
        [SerializeField] private UIFrameAnimator targetAnimator;

        [Header("公転・収縮設定")]
        [Tooltip("アニメーション時間")]
        [SerializeField] private float duration = 0.5f;

        [Tooltip("開始時の回転半径（ボタン中心からの距離）")]
        [SerializeField] private float startRadius = 100f;

        [Tooltip("回転数（正の値で時計回り、負の値で反時計回り）")]
        [SerializeField] private float rotations = 2f;

        [Tooltip("収縮時のイージング")]
        [SerializeField] private Ease moveEase = Ease.OutQuart;

        [Header("演出用画像")]
        [Tooltip("決定時に再生する画像リスト")]
        [SerializeField] private Sprite[] effectSprites;
        [SerializeField] private float fps = 2f;

        [Header("イベント")]
        [Tooltip("アニメーション完了時に発火（シーン遷移などを登録）")]
        [SerializeField] private UnityEvent onComplete;

        private CancellationTokenSource cts;
        private bool hasStarted = false;

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (targetRectTransform == null)
            {
                Debug.LogWarning("SubmitOrbitEffect: Target is not assigned.");
                return;
            }
            if (hasStarted)
                return;

            PlayOrbitEffectAsync().Forget();
            hasStarted = true;
        }

        private async UniTaskVoid PlayOrbitEffectAsync()
        {
            // キャンセル処理
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            // TravelingCursorの干渉を防ぐ 
            // ターゲット自体、もしくはその親などにTravelingCursorがいるか探して停止させる
            var cursorCtrl = targetRectTransform.GetComponentInParent<TravelingCursor>();
            if (cursorCtrl != null)
            {
                cursorCtrl.SetPaused(true);
            }

            // 画像アニメーション切り替え
            if (targetAnimator != null && effectSprites != null && effectSprites.Length > 0)
            {
                targetAnimator.Play(effectSprites, fps);
            }

            // 公転・収縮アニメーション
            targetRectTransform.DOKill();
            
            // 計算用の初期パラメータを確保
            Vector3 centerPos = transform.position; // このボタンが中心
            Vector3 startScale = targetRectTransform.localScale; // 現在のサイズ（キャンセル時の復帰にも使用）

            try
            {
                await DOVirtual.Float(0f, 1f, duration, value =>
                {
                    float t = value; // 0.0 -> 1.0 の進行度
                    
                    if (targetRectTransform != null)
                    {
                        // 公転移動 (半径を徐々に0にする)
                        float currentRadius = Mathf.Lerp(startRadius, 0f, t);
                        // 角度を回す (進行度 * 回転数 * 360度)
                        float currentAngle = (t * rotations * 360f) * Mathf.Deg2Rad;

                        float x = Mathf.Cos(currentAngle) * currentRadius;
                        float y = Mathf.Sin(currentAngle) * currentRadius;
                        
                        targetRectTransform.position = centerPos + new Vector3(x, y, 0);

                        // サイズ縮小 (現在のサイズ -> 0)
                        targetRectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                    }
                })
                .SetEase(moveEase)
                .SetLink(gameObject)
                .ToUniTask(cancellationToken: token);
                
                // アニメーション完了後にイベント発火
                onComplete?.Invoke();
            }
            catch (System.OperationCanceledException)
            {
                // エラーやキャンセルで中断された場合、サイズが0のまま残らないように元に戻す
                if (targetRectTransform != null)
                {
                    targetRectTransform.localScale = startScale;
                }
            }
            finally
            {
                // エラーやキャンセルで終わった場合でも、もしオブジェクトが生きていれば
                // TravelingCursorのポーズを解除してあげる（シーン遷移しない場合への配慮）
                if (cursorCtrl != null && cursorCtrl.gameObject != null)
                {
                    // 必要に応じてここで SetPaused(false) を呼ぶ
                    // cursorCtrl.SetPaused(false); 
                }
            }
        }
    }
}