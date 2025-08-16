using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

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
            if (refScaler != null)
            {
                refScaler.OnScaleCompleted += OnScaleCompleted;
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
        private void OnDestroy()
        {
            if (refScaler != null)
            {
                refScaler.OnScaleCompleted -= OnScaleCompleted;
            }
        }

        /// <summary>
        /// 基準オブジェクトの拡大縮小に合わせてトリガーもスケール
        /// </summary>
        /// <param name="args">refScalerの情報</param>
        private void OnScaleCompleted(ScaleEventArgs args)
        {
            int diff = args.CurrentStep - args.PreviousStep;

            if (diff != 0)
            {
                trigScaler.Scale(diff);
            }
            else
            {
                Debug.Log("変動が発生しませんでした。");
            }
    
        }
    }
}