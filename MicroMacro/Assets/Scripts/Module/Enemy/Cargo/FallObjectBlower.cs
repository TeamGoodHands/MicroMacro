using System;
using Constants;
using Cysharp.Threading.Tasks;
using Module.Scaling;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class FallObjectBlower : MonoBehaviour
    {
        [SerializeField] private FallObjectEnemyChecker enemyChecker;
        [SerializeField] private ProjectileObject projectileObject;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private Collider objectCollider;
        [SerializeField] private Scaler scaler;
        [SerializeField] private float blowPower;
        [SerializeField] private float torquePower;

        private float blowDirection = -1;
        private bool isBlowing;
        private Transform playerTransform;

        private void Start()
        {
            playerTransform = GameObject.FindWithTag(Tag.Player).transform;

            enemyChecker.OnHit += OnHit;
            enemyChecker.OnCheckStart += OnCheckStart;

            if (projectileObject != null)
            {
                projectileObject.OnArrived += OnArrived;
            }
        }

        private void OnCheckStart()
        {
            isBlowing = false;
        }

        private void OnArrived()
        {
            Blow();
        }

        private void OnHit(GameObject obj)
        {
            Blow();
        }

        private void Blow()
        {
            if (isBlowing)
                return;

            blowDirection = Mathf.Sign((transform.position - playerTransform.position).x);
            AddForceAsync().Forget();

            isBlowing = true;
        }

        private async UniTaskVoid AddForceAsync()
        {
            await UniTask.Yield();

            rigidBody.isKinematic = true;
            objectCollider.enabled = false;

            if (scaler.CurrentStep > 0)
            {
                // ヒットストップ
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: destroyCancellationToken);
            }

            rigidBody.isKinematic = false;
            rigidBody.AddForce(Vector3.up * blowPower + Vector3.right * (blowDirection * blowPower * 0.5f));
            rigidBody.AddTorque(-Vector3.forward * (blowDirection * torquePower));
        }
    }
}