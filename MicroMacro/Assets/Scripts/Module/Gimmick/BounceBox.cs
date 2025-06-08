using System;
using Constants;
using Module.Player;
using Module.Scaling;
using UnityEngine;

namespace Gimmick
{
    public class BounceBox : MonoBehaviour
    {
        [SerializeField] private TwoAxisScaler scaler;
        [SerializeField] private float bounceForce = 5f;
        [SerializeField] private ScaleRewinder scaleRewinder;

        public event Action OnBounce;
        private bool isUpScaling = false;

        private void Start()
        {
            // スケールイベントを購読
            scaler.OnScaleStarted += OnScaleStarted;
            scaler.OnScaleCompleted += OnScaleCompleted;

            // 巻き戻しをスケジュール
            scaleRewinder.Schedule(scaler);
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            // スケールの段階が上がっていたらtrue
            isUpScaling = args.CurrentStep - args.PreviousStep > 0;
        }

        private void OnScaleCompleted(ScaleEventArgs args)
        {
            isUpScaling = false;
        }

        private void OnCollisionStay(Collision other)
        {
            if (!isUpScaling || other.contactCount == 0)
                return;

            GameObject obj = other.gameObject;

            // プレイヤーに当たった場合
            if (obj.CompareTag(Tag.Handle.Player) &&
                obj.TryGetComponent(out PlayerBehaviour behaviour))
            {
                // あたった面の法線の反対方向に力を加える (場合によっては変な方向になる)
                Vector2 bounceDirection = -other.GetContact(0).normal;
                behaviour.Component.PlayerMovement.AddExternalForce(bounceDirection * bounceForce);
                OnBounce?.Invoke();

                isUpScaling = false;
            }
        }

        private void OnDestroy()
        {
            // スケールイベントを解除
            scaler.OnScaleStarted -= OnScaleStarted;
            scaler.OnScaleCompleted -= OnScaleCompleted;
                
            // 巻き戻しを破棄
            scaleRewinder.Dispose();
        }
    }
}