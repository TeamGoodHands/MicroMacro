using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Scaling;
using UnityEngine;

namespace Module.Gimmick
{
    /// <summary>
    /// 吊り下がった枝を制御するクラス
    /// </summary>
    public class HangingBranch : MonoBehaviour
    {
        [SerializeField] private Transform branchTransform;
        [SerializeField] private Rigidbody childRigidbody;
        [SerializeField] private Scaler footScaler;
        [SerializeField, Header("ステップごとの回転角度（度）")] private float stepAngle = 15f;
        [SerializeField, Header("回転・移動にかける時間（秒）")] private float rotateDuration = 0.5f;
        [SerializeField, Header("枝から子オブジェクトまでの半径")] private float radius = 1f;

        private void Start()
        {
            footScaler.OnScaleCompleted += HandleScaleCompleted;
        }

        private void HandleScaleCompleted(ScaleEventArgs args)
        {
            // 枝の回転
            RotateBranch(args);

            // 葉の回転
            MoveChild(args);
        }

        private void RotateBranch(ScaleEventArgs args)
        {
            float angleDiff = (args.CurrentStep - args.PreviousStep) * stepAngle;

            Vector3 rotationOffset = new Vector3(0f, 0f, angleDiff);
            Vector3 targetRotation = branchTransform.localEulerAngles + rotationOffset;
            branchTransform.DOLocalRotate(targetRotation, rotateDuration).SetEase(Ease.OutBack);
        }

        private void MoveChild(ScaleEventArgs args)
        {
            // 回転角度
            float angle = args.CurrentStep * stepAngle * Mathf.Deg2Rad;

            // 回転後の座標を算出
            Vector2 localPos = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));

            // 指定した座標まで移動させる
            Vector3 targetWorldPos = transform.TransformPoint(localPos);
            childRigidbody.DOMove(targetWorldPos, rotateDuration).SetEase(Ease.OutBack);
        }
    }
}