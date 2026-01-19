using System;
using DG.Tweening;

namespace Module.Enemy.Cargo
{
    using UnityEngine;

    public class ProjectileObject : MonoBehaviour
    {
        [SerializeField] private float localTimeScale = 1.0f;

        private FallObjectEnemyChecker enemyChecker;
        private bool isShooting;
        private Vector3 startPos;
        private Vector3 velocity; // 初速ベクトル
        private float timeScale;
        private float elapsed;
        private float flightTime;

        public event Action OnArrived;

        public float LocalTimeScale
        {
            get => localTimeScale;
            set => localTimeScale = value;
        }

        private void Start()
        {
            enemyChecker = GetComponent<FallObjectEnemyChecker>();
            enemyChecker.OnHit += OnHit;
        }

        public void Launch(Vector3 targetPosition, float flightTime, float scaleTime)
        {
            startPos = transform.position;
            localTimeScale = scaleTime;
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
            enemyChecker.StartCheck();
        }

        public void Stop()
        {
            isShooting = false;
            elapsed = 0;
            enemyChecker.StopCheck();
        }

        private void OnHit(GameObject target)
        {
            Stop();
        }

        private void Update()
        {
            if (!isShooting)
                return;

            elapsed += Time.deltaTime * LocalTimeScale;

            float t = Mathf.Min(elapsed, flightTime);

            // 放物線で飛ばす
            float g = Mathf.Abs(Physics.gravity.y);
            Vector3 pos = startPos + velocity * t + Vector3.down * (0.5f * g * t * t);

            transform.position = pos;

            // 到着時間ギリギリになったら到着したとみなす
            if (flightTime - elapsed < 0.01f)
            {
                Stop();
                OnArrived?.Invoke();
            }
        }

        public void Disable()
        {
            transform.DOScale(0f, 0.3f).OnComplete(() => { gameObject.SetActive(false); });
        }
    }
}