using System;
using Constants;
using UnityEngine;

namespace Module.Player.Weapon
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody rigBody;
        [SerializeField] private bool isMacro;
        [SerializeField] private float disappearDistance = 10f;

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
            // TODO: ここにスケール処理を書く   
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