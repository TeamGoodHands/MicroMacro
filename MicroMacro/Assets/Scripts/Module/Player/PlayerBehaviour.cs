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
            stateMachine.AddState(new AliveState(component));
            stateMachine.AddState(new DeathState());
            stateMachine.AddState<GroundState, AliveState>(new GroundState(component));
            stateMachine.AddState<InAirState, AliveState>(new InAirState(component));

            // ステート遷移の初期化
            stateMachine.AddTransition<GroundState, InAirState>(() => component.Condition.IsGround == false);
            stateMachine.AddTransition<InAirState, GroundState>(() => component.Condition.IsGround == true);

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
    }
}