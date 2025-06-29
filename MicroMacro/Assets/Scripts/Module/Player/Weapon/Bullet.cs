using System;
using Constants;
using Module.Scaling;
using Unity.VisualScripting;
using UnityEngine;

namespace Module.Player.Weapon
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody rigBody;
        [SerializeField] private int scaleStep;
        [SerializeField] private float disappearDistance;
        [Header("加わる重力の強さ")]
        [SerializeField] private float gravityScale;

        public event Action OnHit;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            // 画面外に出たら無効化する
            if (IsOutOfScreen())
            {
                Disable();
            }

            if (rigBody.useGravity)
            {
                rigBody.AddForce(Vector2.down * gravityScale, ForceMode.Force);
            }
        }

        public void AddForce(Vector2 force)
        {
            rigBody.AddForce(force, ForceMode.Impulse);
        }

        private void HandleHit(GameObject hitObject)
        {
            if (hitObject.TryGetComponent(out Scaler scaler))
            {
                scaler.Scale(scaleStep).Forget();
            }
            // サイズ変動が無くても無効化
            Disable();
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

        private void Disable()
        {
            OnHit?.Invoke();
            OnHit = null;

            rigBody.linearVelocity = Vector3.zero;
        }
    }
}