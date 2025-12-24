using System;
using Constants;
using CoreModule.Utility;
using Cysharp.Threading.Tasks;
using Module.Scaling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Player.Weapon
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody rigBody;
        [SerializeField] private GameObject body;
        [SerializeField] private VisualEffect hitEffect;
        [SerializeField] private VisualEffect trailEffect;
        [SerializeField] private int scaleStep;
        [SerializeField] private float disappearDistance;
        [SerializeField] private float effectZOffset;
        [SerializeField] private float effectXOffset;
        [SerializeField] private float disappearDelay;

        [Header("加わる重力の強さ")]
        [SerializeField] private float gravityScale;

        public event Action OnHit;
        private Camera mainCamera;
        private bool isHitting;
        private bool isVertical;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            // 画面外に出たら無効化する
            if (IsOutOfScreen())
            {
                Disable(false).Forget();
            }

            // useGravityをtrueにするとさらに余計に重力が加わり弾道予測線がずれる
            rigBody.AddForce(new Vector2(0, Physics.gravity.y * gravityScale), ForceMode.Acceleration);
        }

        public void Shoot(Vector2 force, bool isVertical)
        {
            this.isVertical = isVertical;
            trailEffect.gameObject.SetActive(true);
            trailEffect.Play();
            rigBody.AddForce(force, ForceMode.Impulse);
        }

        private void HandleHit(GameObject hitObject)
        {
            if (isHitting)
                return;

            if (hitObject.transform.TryGetComponentInParent(out IScaleSender scaler))
            {
                scaler.Scale(scaleStep).Forget();
            }

            // サイズ変動が無くても無効化
            Disable(true).Forget();
        }

        private void OnTriggerEnter(Collider other)
        {
            // TODO: ここにスケール処理を書く   
            HandleHit(other.gameObject);
        }

        private void OnCollisionEnter(Collision other)
        {
            // TODO: グレと通常弾でクラス分けたい
            HandleHit(other.gameObject);
        }

        private bool IsOutOfScreen()
        {
            Vector2 diff = (Vector2)transform.position - (Vector2)mainCamera.transform.position;
            return diff.sqrMagnitude > disappearDistance * disappearDistance;
        }

        private void OnEnable()
        {
            isHitting = false;
            body.SetActive(true);
            hitEffect.transform.localPosition = Vector3.zero;
        }

        private async UniTaskVoid Disable(bool isHit)
        {
            isHitting = true;

            rigBody.linearVelocity = Vector3.zero;
            body.gameObject.SetActive(false);

            if (isHit)
            {
                await PlayHitEffect();
            }

            OnHit?.Invoke();
            OnHit = null;
        }

        private async UniTask PlayHitEffect()
        {
            var sparkRotation = isVertical ? new Vector2(90f, 0f) : new Vector2(0f, 90f);

            hitEffect.SetVector2("SparkRotation", sparkRotation);

            hitEffect.Play();
            trailEffect.Stop();
            trailEffect.gameObject.SetActive(false);

            await UniTask.Delay(TimeSpan.FromSeconds(disappearDelay), cancellationToken: destroyCancellationToken);
        }
    }
}