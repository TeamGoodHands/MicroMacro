using System;
using CoreModule.AI.HSM;
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
            stateMachine.AddState(new LockState());

            stateMachine.AddState(new AliveState(component));
            stateMachine.AddState(new DeathState(component));
            stateMachine.AddState<GroundState, AliveState>(new GroundState(component));
            stateMachine.AddState<InAirState, AliveState>(new InAirState(component));
            stateMachine.AddState<RideState, AliveState>(new RideState(component));

            // ステート遷移の初期化
            stateMachine.AddTransition<GroundState, InAirState>(() => component.Condition.IsGround == false);
            stateMachine.AddTransition<InAirState, GroundState>(() => component.Condition.IsGround == true);
            stateMachine.AddTransition<AliveState, LockState>(() => component.Condition.IsPlayerLocked == true);
            stateMachine.AddTransition<LockState, AliveState>(() => component.Condition.IsPlayerLocked == false);
            stateMachine.AddTransition<AliveState, DeathState>(() => component.PlayerStatus.CurrentHealth == 0);
            stateMachine.AddTransition<DeathState, AliveState>(() => component.PlayerStatus.CurrentHealth > 0);
            stateMachine.AddTransition<GroundState, RideState>(() => component.Condition.IsRiding == true);
            stateMachine.AddTransition<RideState, GroundState>(() => component.Condition.IsRiding == false);

            // ステートマシンはAliveStateから起動
            stateMachine.Start<AliveState>();
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