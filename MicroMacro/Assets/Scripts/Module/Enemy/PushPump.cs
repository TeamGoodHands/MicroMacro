using System;
using System.Threading;
using CoreModule.ObjectPool;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Module.Enemy
{
    public class PushPump : MonoBehaviour
    {
        [SerializeField, Header("発射する弾のPrefab")] private GameObject bulletPrefab;
        [SerializeField, Header("発射するポイント")] private Transform shootPoint;
        [SerializeField, Header("発射力")] private float power = 1f;
        [SerializeField, Header("発射間隔")] private float interval = 1f;
        [SerializeField, Header("プールする量")] private int poolAmount = 10;

        private CancellationTokenSource shootCanceller;
        private ObjectPool<GameObject> bulletPool;

        private void Start()
        {
            shootCanceller = new CancellationTokenSource();
            
            // 弾のObjectPoolの初期化
            bulletPool = new ObjectPool<GameObject>(() => OnBulletCreate(bulletPrefab), null, poolAmount);

            // 発射ループを開始する
            ShootBulletLoopAsync(shootCanceller.Token).Forget();
        }

        private GameObject OnBulletCreate(GameObject bulletPrefab)
        {
            GameObject obj = Instantiate(bulletPrefab, ObjectPool.Root, true);
            obj.SetActive(false);
            return obj;
        }

        private void OnDestroy()
        {
            shootCanceller?.Cancel();
            shootCanceller?.Dispose();
            shootCanceller = null;
        }

        private async UniTaskVoid ShootBulletLoopAsync(CancellationToken token)
        {
            // interval間隔で発射する
            while (!token.IsCancellationRequested)
            {
                Shoot();
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
            }
        }

        private void Shoot()
        {
            // プールが空の場合は終了
            if (!bulletPool.TryGet(out GameObject bulletObject))
            {
                Debug.LogError("ObjectPoolが空になりました");
                return;
            }

            if (bulletObject.TryGetComponent(out DamageBullet damageBullet))
            {
                // ヒット時はObjectPoolに返却する
                damageBullet.OnHit += () =>
                {
                    bulletObject.SetActive(false);
                    bulletPool.Return(bulletObject);
                };

                // 弾の有効化
                bulletObject.SetActive(true);

                // 発射座標を設定
                bulletObject.transform.position = shootPoint.position;

                // 力を与えて発射
                damageBullet.AddForce(shootPoint.right * power);
            }
            else
            {
                Debug.LogError("BulletObjectはDamageBulletをアタッチしていません。");
            }
        }
    }
}