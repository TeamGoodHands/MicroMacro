using System;
using CoreModule.Utility;
using Module.Player.Component;
using UnityEngine;

namespace Module.Enemy
{
    public class DamageBullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody rigBody;
        [SerializeField] private int damageAmount;
        [SerializeField] private float disappearDistance;

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
        }

        public void AddForce(Vector2 force)
        {
            rigBody.AddForce(force, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.TryGetComponentInParent(out IDamageable damageable))
            {
               damageable.Damage(damageAmount);
            }

            Disable();
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