using System;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    using UnityEngine;

    public class ProjectileShooter : MonoBehaviour
    {
        [SerializeField] private float localTimeScale = 1.0f;

        private bool isShooting;
        private Vector3 startPos;
        private Vector3 velocity; // 初速ベクトル
        private float timeScale;
        private float elapsed;
        private float flightTime;

        public float LocalTimeScale
        {
            get => localTimeScale;
            set => localTimeScale = value;
        }

        public void Shoot(Vector3 targetPosition, float flightTime)
        {
            startPos = transform.position;
            this.flightTime = flightTime;

            Vector3 diff = targetPosition - startPos;

            float g = Mathf.Abs(Physics.gravity.y);
            float t = flightTime;

            // 水平方向の速度
            Vector3 horizontal = new Vector3(diff.x, 0, diff.z) / t;

            // 垂直方向の速度
            float vy = (diff.y + 0.5f * g * t * t) / t;

            velocity = horizontal + Vector3.up * vy;
            
            isShooting = true;
        }

        private void Update()
        {
            if (!isShooting)
                return;
            
            elapsed += Time.deltaTime * LocalTimeScale;

            float t = Mathf.Min(elapsed, flightTime);

            // 放物線の公式
            float g = Mathf.Abs(Physics.gravity.y);
            Vector3 pos = startPos
                          + velocity * t
                          + Vector3.down * (0.5f * g * t * t);

            transform.position = pos;
        }
    }
}