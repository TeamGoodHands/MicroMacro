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
        [SerializeField, Header("ブーメランを投げる間隔")] private float throwInterval = 1f;
        [SerializeField, Header("発射位置の調整")] private float shootHeightOffset = -0.1f;

        [SerializeField] private EnemyStatus status;
        [SerializeField] private Scaler boomerang;
        [SerializeField] private Rigidbody boomerangRig;
        [SerializeField] private TransformSyncer capTransformSyncer;
        [SerializeField] private Transform headBoneTransform;
        [SerializeField] private BoomerangWrapper animatorWrapper;
        [SerializeField] private Animator capAnimator;

        [SerializeField, Min(0.0001f)] private float exponentialSharpness = 4.0f;

        private bool isActive = false;

        private static readonly int boomerangRotationHash = Animator.StringToHash("BoomerangRotation");

        private void Start()
        {
            // 死亡イベントを登録
            status.OnDeath += OnDeath;
            status.OnDamage += OnDamage;

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

        private void OnDamage(int damage)
        {
            //今は適当に揺らす 
            transform.DOShakePosition(0.35f, 0.2f, 30, 90f, false, false);
        }

        private void OnDeath()
        {
            animatorWrapper.IsDeath = true;
            Destroy(gameObject, 1f);
        }

        public void Catch()
        {
            CheckBoomerangHit();

            capAnimator.enabled = false;
            boomerang.SetScaleImmediate(0, true);
            SetCapLockState(true);
        }

        private void SetCapLockState(bool isLock)
        {
            capAnimator.gameObject.SetActive(!isLock);
            capTransformSyncer.syncPosition = isLock;
            capTransformSyncer.syncRotation = isLock;

            if (isLock)
            {
                capTransformSyncer.UpdateManual();
            }
        }

        private float t = 0f;
        private float v0; // 初速 = 2D/T
        private float a; // 加速度 = 2D/T^2
        private Vector3 uForward; // 投げ方向の単位ベクトル
        private Vector3 originPos; // 投擲時の基準位置

        private void FixedUpdate()
        {
            if (!isActive)
            {
                return;
            }

            t += Time.fixedDeltaTime;

            float T = Mathf.Max(0.0001f, oneWayTime);
            float d = Mathf.Max(0.0001f, oneWayDistance);

            float u = t / T;

            float sdotPerSec = EvalExpPingPongVelocityPerSec(u, exponentialSharpness, T);
            Vector3 v = uForward * (d * sdotPerSec);
            boomerangRig.linearVelocity = v;

            // 2T 経過で戻りきり → 停止
            if (t >= 2f * T)
            {
                Finish();
            }
        }

        /// <summary>
        /// ブーメラン投擲開始
        /// </summary>
        public void Throw()
        {
            SetCapLockState(false);

            float d = Mathf.Max(0.0001f, oneWayDistance);
            float T = Mathf.Max(0.0001f, oneWayTime);

            v0 = 2f * d / T;
            a = 2f * d / (T * T);

            uForward = (transform.right + Vector3.up * shootHeightOffset).normalized;
            originPos = headBoneTransform.position;

            t = 0f;
            isActive = true;

            boomerangRig.isKinematic = false;
            boomerangRig.position = originPos;
            boomerangRig.rotation = headBoneTransform.rotation;
            boomerangRig.linearVelocity = uForward * v0; // ※初期フレームはこの速度、以降は上書き
            boomerangRig.angularVelocity = Vector3.zero;

            capAnimator.enabled = true;
            capAnimator.Play(boomerangRotationHash);
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

        // 指数イージング（ピンポン）速度：ds/dt を返すヘルパ
        private float EvalExpPingPongVelocityPerSec(float u, float k, float T)
        {
            float kk = Mathf.Max(0.0001f, k);
            float denom = 1f - Mathf.Exp(-kk);

            if (u <= 1f)
            {
                float x = Mathf.Clamp01(u);
                return (kk * Mathf.Exp(-kk * x)) / (denom * T);
            }
            else
            {
                float x = Mathf.Clamp01(2f - u);
                return -(kk * Mathf.Exp(-kk * x)) / (denom * T);
            }
        }
    }
}