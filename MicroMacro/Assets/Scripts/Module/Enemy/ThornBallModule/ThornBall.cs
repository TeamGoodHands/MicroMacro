using System;
using Constants;
using Module.Enemy.Cargo;
using Module.Gimmick;
using Module.Scaling;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Enemy.ThornBallModule
{
    public class ThornBall : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private Thorn thorn;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private Collider bodyCollider;
        [SerializeField] private Collider bulletOnlyCollider;
        [SerializeField] private VisualEffect deathEffect;
        [SerializeField] private FallObjectEnemyChecker enemyChecker;
        [SerializeField] private bool isBig;

        [SerializeField] private float linearSpeed;
        [SerializeField] private float rotateSpeed;
        [SerializeField, ReadOnly] private bool isAlive = true;

        private bool isLinearMoving;
        private float rotateDirection;
        private Transform playerTransform;
        private Vector3 moveDirection;
        private Vector3 moveDelta;

        public event Action<ThornBall> OnDeath;
        public bool IsBig => isBig;

        private void Start()
        {
            scaler.OnScaleCompleted += HandleScaleCompleted;
            thorn.OnThornDamaged += HandleThornDamaged;
            enemyChecker.OnHit += HandleOnHit;

            playerTransform = GameObject.FindWithTag(Tag.Player).transform;
        }

        private void HandleOnHit(GameObject obj)
        {
            StopLinearMovement();
        }

        private void HandleThornDamaged()
        {
            Death();
        }

        public void PerformLinearMovement(Vector2 direction)
        {
            isLinearMoving = true;
            rigidBody.isKinematic = true;
            moveDirection = direction;
            enemyChecker.StartCheck();
        }

        public void SetMoveDelta(Vector3 moveDelta)
        {
            this.moveDelta = moveDelta;
        }

        private void StopLinearMovement()
        {
            isLinearMoving = false;
            rigidBody.isKinematic = false;
            enemyChecker.StopCheck();

            rotateDirection = playerTransform != null
                ? Mathf.Sign(transform.position.x - playerTransform.position.x)
                : Mathf.Sign(moveDirection.x == 0 ? 1f : moveDirection.x);
        }

        private void FixedUpdate()
        {
            if (isLinearMoving)
            {
                rigidBody.MovePosition(rigidBody.position + moveDirection * linearSpeed);
            }
            else if (!rigidBody.isKinematic)
            {
                rigidBody.MovePosition(rigidBody.position + moveDelta);
                rigidBody.angularVelocity = Vector3.forward * (rotateDirection * rotateSpeed);
            }
        }

        private void HandleScaleCompleted(ScaleEventArgs args)
        {
            if (args.CurrentStep == scaler.MinStep)
            {
                Death();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(Tag.Handle.DeathArea))
            {
                Death();
            }
        }

        private void Death()
        {
            if (!isAlive)
                return;

            if (scaler.CurrentStep > scaler.MinStep)
            {
                scaler.SetScale(scaler.MinStep, true);
            }

            isAlive = false;
            deathEffect.Play();
            bulletOnlyCollider.enabled = false;
            bodyCollider.enabled = false;
            rigidBody.isKinematic = true;
            enemyChecker.StopCheck();

            OnDeath?.Invoke(this);
        }

        public void Reset()
        {
            scaler.SetScale(0, true);
            isAlive = true;
            isLinearMoving = false;
            bulletOnlyCollider.enabled = true;
            bodyCollider.enabled = true;
            rigidBody.isKinematic = true;
        }
    }
}