using System;
using System.Threading;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using Module.Player.State;
using UnityEngine;

namespace Module.Player
{
    /// <summary>
    /// プレイヤーのステートを管理するクラス
    /// </summary>
    public class PlayerBehaviour : MonoBehaviour
    {
        [SerializeField] private PlayerComponent component;

        public PlayerComponent Component => component;

        private HierarchicalStateMachine stateMachine;

        private void Start()
        {
            stateMachine = new HierarchicalStateMachine();

            // ステートの初期化
            stateMachine.AddState(new LockState(component));

            stateMachine.AddState(new AliveState(component));
            stateMachine.AddState(new DeathState(component, component.Parameter));
            stateMachine.AddState<GroundState, AliveState>(new GroundState(component));
            stateMachine.AddState<InAirState, AliveState>(new InAirState(component));
            stateMachine.AddState<RideState, AliveState>(new RideState(component));

            // ステート遷移の初期化
            stateMachine.AddTransition<GroundState, InAirState>(() => component.Condition.IsGround == false);
            stateMachine.AddTransition<InAirState, GroundState>(() => component.Condition.IsGround == true);
            stateMachine.AddTransition<AliveState, LockState>(() => component.Condition.IsPlayerLocked == true);
            stateMachine.AddTransition<LockState, AliveState>(() => component.Condition.IsPlayerLocked == false);
            stateMachine.AddTransition<AliveState, DeathState>(() => component.HealthStatus.CurrentHealth == 0);
            stateMachine.AddTransition<DeathState, AliveState>(() => component.HealthStatus.CurrentHealth > 0);
            stateMachine.AddTransition<GroundState, RideState>(() => component.Condition.IsRiding == true);
            stateMachine.AddTransition<RideState, GroundState>(() => component.Condition.IsRiding == false);

            // ステートマシンはAliveStateから起動
            stateMachine.Start<AliveState>();
            
            component.HealthStatus.OnDamage += _ =>
            {
                PlayInvincibleTime().Forget();
            };
        }
        
        private async UniTaskVoid PlayInvincibleTime()
        {
            gameObject.layer = Layer.Invincible;
            await BlinkMeshAsync(component.Parameter.InvincibleTime);
            gameObject.layer = Layer.Player;
        }

        private async UniTask BlinkMeshAsync(float durationSeconds, float intervalSeconds = 0.1f)
        {
            CancellationToken token = gameObject.GetCancellationTokenOnDestroy();
            float elapsedTime = 0f;
            
            await UniTask.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken: token);

            while (elapsedTime < durationSeconds)
            {
                // レンダラーの有効状態を反転させます
                component.MeshRenderer.enabled = !component.MeshRenderer.enabled;

                // 指定された間隔だけ待機します
                await UniTask.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken: token);
                elapsedTime += intervalSeconds;
            }

            // 最終的に必ず表示状態に戻します
            component.MeshRenderer.enabled = true;
        }

        private void Update()
        {
            stateMachine.Update();
        }

        private void FixedUpdate()
        {
            stateMachine.UpdatePhysics();
        }

        private void OnDestroy()
        {
            // ステートマシンのクリーンアップ
            stateMachine?.Dispose();
        }
    }
}