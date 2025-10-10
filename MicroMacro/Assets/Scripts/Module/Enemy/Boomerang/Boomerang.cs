using System;
using System.Threading;
using CoreModule.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Scaling;
using PropertyGenerator.Generated;
using UnityEngine;

namespace Module.Enemy.Boomerang
{
    public class Boomerang : MonoBehaviour
    {
        [Min(0.001f)] [SerializeField] private float oneWayDistance = 10f; // 片道距離 D
        [Min(0.001f)] [SerializeField] private float oneWayTime = 1.0f; // 片道時間 T
        [SerializeField] private float spinDegPerSec = 720f; // 見た目の回転
        [SerializeField, Header("ブーメランを投げる間隔")] private float throwInterval = 1f;
        [SerializeField, Header("発射位置の調整")] private float shootHeightOffset = -0.1f;

        [SerializeField] private EnemyStatus status;
        [SerializeField] private Scaler boomerang;
        [SerializeField] private Rigidbody boomerangRig;
        [SerializeField] private TransformSyncer capTransformSyncer;
        [SerializeField] private Transform headBoneTransform;
        [SerializeField] private BoomerangWrapper animatorWrapper;

        private void Start()
        {
            // 死亡イベントを登録
            status.OnDeath += OnDeath;

            LoopThrow().Forget();
        }

        private async UniTaskVoid LoopThrow()
        {
            while (!destroyCancellationToken.IsCancellationRequested)
            {
                animatorWrapper.IsAttacking = true;

                await UniTask.Delay(TimeSpan.FromSeconds(oneWayTime * 2f), cancellationToken: destroyCancellationToken);

                animatorWrapper.IsAttacking = false;

                await UniTask.Delay(TimeSpan.FromSeconds(throwInterval), cancellationToken: destroyCancellationToken);
            }
        }

        private async void OnDeath()
        {
            // ちょっと揺らす
            await transform.DOShakePosition(0.5f, 0.5f, 10, 90f, false, true);

            Destroy(gameObject);
        }

        public void Catch()
        {
            SetCapLockState(true);
        }

        private void SetCapLockState(bool isLock)
        {
            capTransformSyncer.gameObject.SetActive(!isLock);
            capTransformSyncer.syncPosition = isLock;
            capTransformSyncer.syncRotation = isLock;
        }


        private bool isActive = false;
        private float t = 0f;
        private float v0; // 初速 = 2D/T
        private float a; // 加速度 = 2D/T^2（向きは -uForward）
        private Vector3 uForward; // 投げ方向の単位ベクトル
        private Vector3 originPos; // 投擲時の基準位置

        private void FixedUpdate()
        {
            if (!isActive)
                return;

            float T = oneWayTime;
            t += Time.fixedDeltaTime;

            // 見た目の回転
            var spin = Quaternion.AngleAxis(spinDegPerSec * Time.fixedDeltaTime, Vector3.up);
            boomerangRig.MoveRotation(boomerangRig.rotation * spin);

            // 常に逆向き加速度を加え続ける
            Vector3 aVec = -a * uForward;
            Vector3 v = uForward * v0 + aVec * t;
            boomerangRig.linearVelocity = v;

            // 2T 経過で戻りきり → 停止
            if (t >= 2f * T)
            {
                Finish();
            }
        }

        /// <summary>
        /// 外部から呼び出す：ブーメラン投擲開始
        /// </summary>
        public void Throw()
        {
            SetCapLockState(false);

            float D = Mathf.Max(0.0001f, oneWayDistance);
            float T = Mathf.Max(0.0001f, oneWayTime);

            v0 = 2f * D / T;
            a = 2f * D / (T * T);

            uForward = (transform.right + Vector3.up * shootHeightOffset).normalized;
            originPos = headBoneTransform.position;

            t = 0f;
            isActive = true;

            boomerangRig.isKinematic = false;
            boomerangRig.position = originPos;
            boomerangRig.rotation = headBoneTransform.rotation;
            boomerangRig.linearVelocity = uForward * v0;
            boomerangRig.angularVelocity = Vector3.zero;
        }

        private void Finish()
        {
            isActive = false;

            Vector3 catchPos = headBoneTransform.position;
            Quaternion catchRot = headBoneTransform.rotation;

            boomerangRig.linearVelocity = Vector3.zero;
            boomerangRig.angularVelocity = Vector3.zero;
            boomerangRig.isKinematic = true;
            boomerangRig.position = catchPos;
            boomerangRig.rotation = catchRot;
        }

        private void OnDestroy()
        {
            if (status != null)
            {
                status.OnDeath -= OnDeath;
            }
        }

        private void CheckBoomerangHit()
        {
            if (boomerang.CurrentStep > 0)
            {
                status.Damage(1);
            }
        }
    }
}