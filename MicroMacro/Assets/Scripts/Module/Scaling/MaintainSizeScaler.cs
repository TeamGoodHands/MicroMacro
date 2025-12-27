using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Module.Scaling
{
    /// <summary>
    /// オブジェクトの拡大縮小に合わせて、外側のトリガーを任意のサイズ差に保つようスケールするクラス。
    /// トリガーのサイズは基準になるオブジェクトと同じサイズなことが前提。
    /// </summary>
    public class MaintainSizeScaler : MonoBehaviour
    {
        [SerializeField, Header("基準とするオブジェクトのスケーラー")] private Scaler refScaler;

        [SerializeField, Header("差を保ちたいオブジェクトのスケーラー")] private Scaler scaler;
        
        [Tooltip("1 = 基準スケーラーから一段階大きい状態を保つ")]
        [SerializeField, Header("基準スケーラーから保つサイズ差")] private int stepOffset = 1;

        private void Awake()
        {
            if (refScaler != null)
            {
                refScaler.OnScaleStarted += OnScaleStarted;
            }
        }

        /// <summary>
        /// 念のため1フレ遅延かけてスケール呼び出す
        /// </summary>
        private IEnumerator Start()
        {
            yield return null;
            if (refScaler != null && scaler != null)
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
            int additionalStep = (refScaler.CurrentStep + stepOffset) - scaler.CurrentStep;
            scaler.Scale(additionalStep);
        }
        private void OnDestroy()
        {
            if (refScaler != null)
            {
                refScaler.OnScaleStarted -= OnScaleStarted;
            }
        }

        /// <summary>
        /// 基準オブジェクトの拡大縮小に合わせてトリガーもスケール
        /// </summary>
        /// <param name="args">refScalerの情報</param>
        private void OnScaleStarted(ScaleEventArgs args)
        {
            if (scaler == null)
            {
                Debug.LogError("trigScalerが未設定の状態でイベントを受信しました",　this);
                return;
            }
            
            int diff = args.CurrentStep - args.PreviousStep;
            if (diff == 0) return;
            scaler.Scale(diff);
        }
    }
}