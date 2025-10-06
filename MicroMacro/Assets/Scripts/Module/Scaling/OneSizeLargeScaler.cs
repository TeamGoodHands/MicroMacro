using System.Collections;
using UnityEngine;

namespace Module.Scaling
{
    /// <summary>
    /// オブジェクトの拡大縮小に合わせて、外側のトリガーを一段階大きい状態を保つようスケールするクラス。
    /// トリガーのサイズは基準になるオブジェクトと同じサイズなことが前提。
    /// </summary>
    public class OneSizeLargeScaler : MonoBehaviour
    {
        [SerializeField, Header("基準とするオブジェクトのスケーラー")] private Scaler refScaler;

        [SerializeField, Header("トリガーのスケーラー")] private Scaler trigScaler;

        private void Awake()
        {
            if (refScaler != null || trigScaler != null)
            {
                refScaler.OnScaleStarted += OnScaleStarted;
                refScaler.OnScalePaused  += OnScalePaused;
                refScaler.OnScaleResumed += OnScaleResumed;
            }
        }
        private void OnDestroy()
        {
            if (refScaler != null || trigScaler != null)
            {
                refScaler.OnScaleStarted -= OnScaleStarted;
                refScaler.OnScalePaused  -= OnScalePaused;
                refScaler.OnScaleResumed -= OnScaleResumed;
            }
        }

        /// <summary>
        /// 念のため1フレ遅延かけてスケール呼び出す
        /// </summary>
        private IEnumerator Start()
        {
            yield return null;
            if (refScaler != null && trigScaler != null)
            {
                InitTrigScaler();
            }
            else
            {
                Debug.LogError("refScalerまたはtrigScalerが設定されていません。", this);
            }
        }

        /// <summary>
        /// トリガーのサイズを、基準のオブジェクト+1になるよう初期化
        /// </summary>
        private void InitTrigScaler()
        {
            int additionalStep = (refScaler.CurrentStep + 1) - trigScaler.CurrentStep;
            trigScaler.Scale(additionalStep);
        }
       
        private void OnScalePaused()
        {
            trigScaler.Pause();
        }

        private void OnScaleResumed(bool isForward, bool isMacro)
        {
            trigScaler.Resume(isForward);
        }

        /// <summary>
        /// 基準オブジェクトの拡大縮小に合わせてトリガーもスケール
        /// </summary>
        /// <param name="args">refScalerの情報</param>
        private void OnScaleStarted(ScaleEventArgs args)
        {
            if (trigScaler == null)
            {
                Debug.LogError("trigScalerが未設定の状態でイベントを受信しました",　this);
                return;
            }
            
            int diff = args.CurrentStep - args.PreviousStep;
            if (diff == 0) return;
            trigScaler.Scale(diff);
        }
    }
}