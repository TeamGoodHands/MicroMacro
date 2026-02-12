using System;
using CoreModule.Input;
using CoreModule.ObjectPool;
using Cysharp.Threading.Tasks;
using Module.Application.SceneSwitch;
using Module.Management;
using Module.Player.Component;
using Module.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace Module.Enemy.Hose.STG
{
    public class ShootingGame : MonoBehaviour
    {
        [Header("トランスフォーム")] [SerializeField] private Transform enemyTransform;
        [SerializeField] private Transform allyTransform;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField] private Collider playerCollider;
        [SerializeField] private STGPlayer stgPlayer;
        [SerializeField] private PlayerHealthView playerHealthView;
        [SerializeField] private PlayerAnimationEventReceiver animationEventReceiver;

        [Header("自動スクロール")] [SerializeField] private float scrollSpeed = 3f;

        [Header("Ally 移動")] [SerializeField] private float allyMoveSpeed = 8f;
        [SerializeField] private Vector2 moveBoundsX = new Vector2(-5f, 5f);
        [SerializeField] private Vector2 moveBoundsZ = new Vector2(-3f, 3f);

        [Header("射撃")] [SerializeField] private Transform muzzleTransform;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float bulletSpeed = 20f;
        [SerializeField] private float shootInterval = 0.15f;
        [SerializeField] private int poolAmount = 20;

        [Header("敵スポーン")] [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform shootPivot;
        [SerializeField] private float enemySpawnInterval = 3f;
        [SerializeField] private float enemyMoveSpeed = 6f; // 移動速度
        [SerializeField] private float enemySpawnRadius = 15f;
        [SerializeField] private float enemySpawnAngle = 90f; // 放射角（度）
        [SerializeField] private int enemyPoolAmount = 5;
        
        [Header("Timeline")] [SerializeField] private PlayableDirector director;
        [SerializeField] private float rewindTime;

        [Header("Wave System")]
        [SerializeField] private float activeWaveDuration = 10f;
        [SerializeField] private float restWaveDuration = 3f;


        // 入力イベント
        private InputEvent moveEvent;
        private InputEvent macroShootEvent;
        private InputEvent microShootEvent;

        // 内部状態
        private ObjectPool<GameObject> bulletPool;
        private ObjectPool<GameObject> enemyPool;
        private Vector3 initialPosition;
        private Vector3 initialPlayerPosition;
        private Vector3 initialAllyLocalPosition;
        private Vector3 playerPosition;
        private Vector3 allyWorldPosition;
        private Vector3 allyOrigin;
        private float lastShootTime;
        private float enemySpawnTimer;
        
        // Wave Control
        private float waveTimer;
        private bool isWaveActive;

        private bool isShooting;
        private bool isSpawning;
        private bool canPlayerShoot;
        private bool canPlayerMove;
        
        private GameReplayer gameReplayer;

        public bool IsShooting => isShooting;
        public bool CanPlayerShoot => canPlayerShoot;

        public async void StartGame()
        {
            playerHealthView.DoStopTimeOnDamage = false;
            isSpawning = true;
            canPlayerShoot = true;
            canPlayerMove = true;

            // Initialize Wave
            isWaveActive = true;
            waveTimer = 0f;

            // GameReplayerを無効化（シーンリロードを防ぐ）
            gameReplayer = FindFirstObjectByType<GameReplayer>();
            if (gameReplayer != null)
            {
                gameReplayer.IsActive = false;
            }

            // プールの初期化
            bulletPool = new ObjectPool<GameObject>(CreateBullet, null, null, poolAmount);
            if (enemyPrefab != null)
                enemyPool = new ObjectPool<GameObject>(CreateEnemy, null, null, enemyPoolAmount);

            // 入力イベントの取得
            moveEvent = InputProvider.CreateEvent(ActionGuid.Player.Move);
            macroShootEvent = InputProvider.CreateEvent(ActionGuid.Player.MacroShoot);
            microShootEvent = InputProvider.CreateEvent(ActionGuid.Player.MicroShoot);

            // 射撃イベントの登録（長押し対応）
            macroShootEvent.Started += OnShootStarted;
            macroShootEvent.Canceled += OnShootCanceled;
            microShootEvent.Started += OnShootStarted;
            microShootEvent.Canceled += OnShootCanceled;

            // 親子関係の設定
            enemyTransform.SetParent(transform, true);
            allyTransform.SetParent(transform, true);

            // プレイヤーの物理・当たり判定設定
            playerRigidbody.isKinematic = true;
            
            // コライダー有効化（敵弾との衝突用）
            playerCollider.enabled = true;
            playerCollider.isTrigger = true;

            stgPlayer.ResetHp();
            
            // 死亡イベント登録
            if (stgPlayer.HealthStatus != null)
            {
                stgPlayer.HealthStatus.OnDeath += OnPlayerDeath;
            }

            initialPosition = transform.position; // 初期位置を保存
            initialPlayerPosition = playerTransform.position;
            initialAllyLocalPosition = allyTransform.localPosition;
            
            playerPosition = playerTransform.position;
            allyWorldPosition = allyTransform.position;
            allyOrigin = allyWorldPosition;

            animationEventReceiver.OnAnimatorMoveEvent += () => { playerTransform.position = playerPosition; };

            while (!destroyCancellationToken.IsCancellationRequested)
            {
                // ── 自動スクロール ──
                Vector3 worldDelta = transform.TransformDirection(Vector3.forward * (scrollSpeed * Time.fixedDeltaTime));
                transform.position += worldDelta;
                playerPosition += worldDelta;
                allyWorldPosition += worldDelta;
                allyOrigin += worldDelta;

                // ── Ally 移動（ワールド XZ 平面） ──
                Vector3 prevAllyPosition = allyWorldPosition;

                if (canPlayerMove)
                {
                    Vector2 moveInput = moveEvent.ReadValue<Vector2>();
                    Vector3 worldMove = new Vector3(moveInput.x, 0f, moveInput.y) * (allyMoveSpeed * Time.fixedDeltaTime);
                    allyWorldPosition += worldMove;
                }
                else
                {
                    // 強制的に初めのローカル座標（= allyOrigin）に戻る
                    allyWorldPosition = Vector3.MoveTowards(allyWorldPosition, allyOrigin, allyMoveSpeed * Time.fixedDeltaTime);
                }

                // クランプ（初期位置からのオフセット範囲）
                allyWorldPosition.x = Mathf.Clamp(allyWorldPosition.x, allyOrigin.x + moveBoundsX.x, allyOrigin.x + moveBoundsX.y);
                allyWorldPosition.z = Mathf.Clamp(allyWorldPosition.z, allyOrigin.z + moveBoundsZ.x, allyOrigin.z + moveBoundsZ.y);
                allyTransform.position = allyWorldPosition;

                // プレイヤーに差分だけ適用
                playerPosition += allyWorldPosition - prevAllyPosition;

                // ── 長押し射撃 ──
                if (isShooting)
                {
                    TryShoot();
                }

                // ── Wave System Update ──
                if (isSpawning)
                {
                    waveTimer += Time.fixedDeltaTime;
                    float currentLimit = isWaveActive ? activeWaveDuration : restWaveDuration;
                    
                    if (waveTimer >= currentLimit)
                    {
                        waveTimer = 0f;
                        isWaveActive = !isWaveActive;
                        // Debug.Log($"Wave State Changed: {(isWaveActive ? "Active" : "Rest")}");
                    }
                }

                // ── 敵スポーン ──
                if (isWaveActive)
                {
                    enemySpawnTimer += Time.fixedDeltaTime;
                    if (isSpawning && enemyPrefab != null && enemySpawnTimer >= enemySpawnInterval)
                    {
                        enemySpawnTimer = 0f;
                        SpawnEnemy();
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, destroyCancellationToken);
            }
        }

        private void OnDestroy()
        {
            // 射撃イベントの解除
            if (macroShootEvent != null)
            {
                macroShootEvent.Started -= OnShootStarted;
                macroShootEvent.Canceled -= OnShootCanceled;
            }
            if (microShootEvent != null)
            {
                microShootEvent.Started -= OnShootStarted;
                microShootEvent.Canceled -= OnShootCanceled;
            }

            if (stgPlayer != null && stgPlayer.HealthStatus != null)
            {
                stgPlayer.HealthStatus.OnDeath -= OnPlayerDeath;
            }

            if (gameReplayer != null)
            {
                gameReplayer.IsActive = true;
            }
        }

        private void OnShootStarted(InputAction.CallbackContext _) => isShooting = true;
        private void OnShootCanceled(InputAction.CallbackContext _) => isShooting = false;

        /// <summary>
        /// 発射間隔を守りつつ弾を生成して発射する
        /// </summary>
        private void TryShoot()
        {
            if (!canPlayerShoot || Time.time - lastShootTime < shootInterval)
                return;

            if (bulletPrefab == null)
                return;

            lastShootTime = Time.time;

            // プールから弾を取得
            GameObject bulletObj = bulletPool.GetOrCreate();
            Transform spawnPoint = muzzleTransform != null ? muzzleTransform : allyTransform;
            bulletObj.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            bulletObj.SetActive(true);

            if (bulletObj.TryGetComponent(out STGBullet bullet))
            {
                bullet.OnReturn += () =>
                {
                    bulletPool.Return(bulletObj);
                };
                bullet.Shoot(spawnPoint.forward * bulletSpeed, gameObject);
                SoundManager.instance.Play("水球発射", 0.3f);
            }
        }

        private GameObject CreateBullet()
        {
            GameObject obj = Instantiate(bulletPrefab, transform);
            obj.SetActive(false);
            return obj;
        }

        /// <summary>
        /// 敵を扇形領域の円周上から対角線上へ横断させる
        /// </summary>
        private void SpawnEnemy()
        {
            // 基準位置（shootPivot があればそれ、なければ transform）
            Vector3 basePos = shootPivot != null ? shootPivot.position : transform.position;

            // ランダムな角度を選択 (-angle/2 ～ angle/2)
            float halfAngle = enemySpawnAngle * 0.5f;
            float angle = UnityEngine.Random.Range(-halfAngle, halfAngle);

            // XZ平面上のベクトルを計算
            // angle=0 が Z軸前方、90が右、-90が左
            Quaternion rot = Quaternion.Euler(0f, angle, 0f);
            Vector3 direction = rot * Vector3.forward;

            // start: 円周上の点
            // end: 中心を通って反対側の点
            Vector3 startOffset = direction * enemySpawnRadius;
            Vector3 endOffset = -direction * enemySpawnRadius;

            Vector3 start = basePos + startOffset;
            Vector3 end = basePos + endOffset;

            // 高さを適用（basePos.y そのまま。オフセットはXZのみ）
            start.y = basePos.y;
            end.y = basePos.y;

            // プールから取得
            GameObject enemyObj = enemyPool.GetOrCreate();
            enemyObj.SetActive(true);

            if (enemyObj.TryGetComponent(out STGEnemy enemy))
            {
                // 移動方向（中心に向かう）
                Vector3 moveDir = -direction;
                Vector3 velocity = moveDir * enemyMoveSpeed;

                // 寿命計算（直径 + マージン）
                float distance = (enemySpawnRadius * 2f) + 10f;
                float lifeTime = enemyMoveSpeed > 0f ? distance / enemyMoveSpeed : 0f;

                enemy.transform.position = start;
                enemy.transform.rotation = Quaternion.LookRotation(moveDir); // 進行方向を向く（オプション）

                enemy.Initialize(velocity, lifeTime);
                enemy.OnReturn += () =>
                {
                    enemyPool.Return(enemyObj);
                };
                // 弾の親として自分自身（ShootingGame）を渡す
                enemy.Activate(transform, destroyCancellationToken).Forget();
            }
        }



        private void OnDrawGizmos()
        {
            if (shootPivot != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 center = shootPivot.position;
                
                // 扇形の描画
                float halfAngle = enemySpawnAngle * 0.5f;
                Quaternion leftRot = Quaternion.Euler(0f, -halfAngle, 0f);
                Quaternion rightRot = Quaternion.Euler(0f, halfAngle, 0f);

                Vector3 leftDir = leftRot * Vector3.forward * enemySpawnRadius;
                Vector3 rightDir = rightRot * Vector3.forward * enemySpawnRadius;

                Gizmos.DrawLine(center, center + leftDir);
                Gizmos.DrawLine(center, center + rightDir);

                // 円弧（簡易版：線分で近似）
                int segments = 20;
                float step = enemySpawnAngle / segments;
                Vector3 prevPos = center + leftDir;
                for (int i = 1; i <= segments; i++)
                {
                    float currentAngle = -halfAngle + (step * i);
                    Quaternion rot = Quaternion.Euler(0f, currentAngle, 0f);
                    Vector3 nextPos = center + (rot * Vector3.forward * enemySpawnRadius);
                    Gizmos.DrawLine(prevPos, nextPos);
                    prevPos = nextPos;
                }
            }
        }

        private GameObject CreateEnemy()
        {
            GameObject obj = Instantiate(enemyPrefab, transform);
            obj.SetActive(false);
            return obj;
        }

        public void Stop()
        {
            isSpawning = false;
        }

        public void DestroyAll()
        {
            canPlayerShoot = false;
            canPlayerMove = false;
            isShooting = false;
            
            // 当たり判定無効化
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }

            // アクティブな敵をすべて強制死亡（VFX再生）させる
            foreach (var enemy in GetComponentsInChildren<STGEnemy>())
            {
                if (enemy.gameObject.activeSelf)
                {
                    enemy.ForceDeath();
                }
            }

            // アクティブな弾をすべて無効化
            foreach (var bullet in GetComponentsInChildren<STGBullet>())
            {
                if (bullet.gameObject.activeSelf)
                {
                    bullet.Deactivate(false);
                }
            }
        }

        [SerializeField] private Module.Application.SceneSwitch.FadeAndSceneTransition fadeTransition;

        private bool autoResetOnDeath = true;

        public void SetAutoReset(bool enable)
        {
            autoResetOnDeath = enable;
        }

        private async void OnPlayerDeath()
        {
            if (autoResetOnDeath)
            {
                // Wait for death animation
                await UniTask.Delay(TimeSpan.FromSeconds(1.5f), cancellationToken: this.GetCancellationTokenOnDestroy());

                // Fade Out
                if (fadeTransition != null)
                {
                    await fadeTransition.FadeOut(false);
                }

                ResetGame();

                // Wait a frame to ensure reset fits
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());

                // Fade In
                if (fadeTransition != null)
                {
                    await fadeTransition.FadeIn(false);
                }
            }
        }

        public void ResetGame()
        {
            // キャンセル要求
            DestroyAll();
            
            // プレイヤー復活
            stgPlayer.ResetHp();
            
            // 内部状態リセット
            isShooting = false;
            isSpawning = true;
            canPlayerShoot = true;
            canPlayerMove = true;
            enemySpawnTimer = 0f;
            lastShootTime = 0f;
            
            // Activate Wave on reset, but reset timer
            isWaveActive = true;
            waveTimer = 0f;

            if (playerCollider != null)
            {
                playerCollider.enabled = true;
            }

            // 位置リセット
            transform.position = initialPosition;
            
            // Ally位置リセット
            allyTransform.localPosition = initialAllyLocalPosition;
            allyWorldPosition = allyTransform.position;
            allyOrigin = allyWorldPosition;
            
            // プレイヤー位置リセット
            playerPosition = initialPlayerPosition;
            playerTransform.position = playerPosition;

            // Timeline Rewind
            if (director != null)
            {
                director.time = rewindTime;
                director.Evaluate();
                director.Play();
            }
        }


    }
}