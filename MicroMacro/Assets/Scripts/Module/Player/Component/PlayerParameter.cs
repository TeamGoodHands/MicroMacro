using System.Collections.Generic;
using UnityEngine;
using Module.Player.Weapon;
using UnityEngine.Serialization;

namespace Module.Player.Component
{
    /// <summary>
    /// プレイヤーの静的なパラメータを保持するクラス
    /// </summary>
    public class PlayerParameter : MonoBehaviour
    {
        [Header("移動")]
        [SerializeField, Header("加速度")] private float moveAccel;
        [SerializeField, Header("最大速度")] private float maxSpeed;
        [SerializeField, Header("減衰率")] private Vector2 damping;
        [SerializeField, Header("外から与えられた力の減衰率")] private Vector2 externalDamping;
        [SerializeField, Header("上昇時の重力")] private float gravityOnUp;
        [SerializeField, Header("下降時の重力")] private float gravityOnDown;
        [SerializeField, Header("ジャンプ力")] private float jumpPower;
        [SerializeField, Header("追加ジャンプ力")] private AnimationCurve additionalJumpPower;
        [SerializeField, Header("ジャンプ中移動係数")] private float airControl;
        [SerializeField, Header("着地判定距離")] private float checkGroundDistance;
        [SerializeField, Header("空中状態から着地判定を開始する時間")] private float groundInterval;
        [SerializeField, Header("ジャンプの頂点に来た時に停止するフレーム数")] private int topStopFrameCount;
        [SerializeField] private float landingTimeOffset;
        
        [Header("回転")]
        [SerializeField, Header("縦方向へのデッドゾーン")] private float verticalDeadZone;
        [SerializeField, Header("縦方向の有効角度")] private float verticalValidAngle;
        [SerializeField, Header("回転速度")] private float rotationSpeed;
        [SerializeField, Header("上下に向く速度")] private float verticalLookSpeed;

        [Header("武器")]
        [SerializeField, Header("武器リスト")] private List<AbstractWeapon> weapons;
        
        [Header("音")]
        [SerializeField,Header("足音を鳴らす速度")] private float footstepSoundSpeed;
        [SerializeField,Header("足音を鳴らす基準間隔")] private float footstepSoundInterval;
        [SerializeField,Header("足音を鳴らす最大間隔")] private float footstepMaxInterval;

        public float MoveAccel => moveAccel;
        public float MaxSpeed => maxSpeed;
        public Vector2 Damping => damping;
        public Vector2 ExternalDamping => externalDamping;
        public float GravityOnUp => gravityOnUp;
        public float GravityOnDown => gravityOnDown;
        public float JumpPower => jumpPower;
        public AnimationCurve AdditionalJumpPower => additionalJumpPower;
        public float AirControl => airControl;
        public int TopStopFrameCount => topStopFrameCount;
        public float CheckGroundDistance => checkGroundDistance;
        public float GroundInterval => groundInterval;
        public float VerticalDeadZone => verticalDeadZone;
        public float VerticalValidAngle => verticalValidAngle;
        public float RotationSpeed => rotationSpeed;
        public float VerticalLookSpeed => verticalLookSpeed;
        public IReadOnlyList<AbstractWeapon> Weapons => weapons;
        public float FootstepSoundInterval => footstepSoundInterval;
        public float FootstepSoundSpeed => footstepSoundSpeed;
        public float FootstepMaxInterval => footstepMaxInterval;
        public float LandingTimeOffset => landingTimeOffset;
    }
}