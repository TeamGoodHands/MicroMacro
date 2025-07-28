using System;
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
            if (trigScaler != null)
            {
                refScaler.OnScaleStarted += OnScaleStarted;
                refScaler.OnScaleCompleted += OnScaleCompleted;
            }
        }

        private void Start() => InitTrigScaler();
        
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
            if (trigScaler != null)
            {
                refScaler.OnScaleStarted -= OnScaleStarted;
                refScaler.OnScaleCompleted -= OnScaleCompleted;
            }
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            
        }

        /// <summary>
        /// 基準オブジェクトの拡大縮小に合わせてトリガーもスケール
        /// </summary>
        /// <param name="args">refScalerの情報</param>
        private void OnScaleCompleted(ScaleEventArgs args)
        {
            if (args.CurrentStep > args.PreviousStep)
            {
                trigScaler.Scale(1);
            }
            else if (args.CurrentStep < args.PreviousStep)
            {
                trigScaler.Scale(-1);
            }
            else
            {
                Debug.LogAssertion("変動が発生しませんでした。");
            }
        }
    }
}