using System;
using Module.Player;
using Module.Player.Component;
using UnityEngine;
using Constants;
using UnityEngine.Serialization;

namespace Module.Gimmick
{
    /// <summary>
    /// (例：貫通床)
    /// 通常時は貫通可能で、aroundTriggerに触れると床に乗れるようになる。乗った状態で下入力で再度貫通可能に。
    /// </summary>
    public class ThroughPlatform : MonoBehaviour
    {
        [SerializeField] private ThroughTrigger aroundTrigger;

        [Tooltip("貫通するオブジェクトと同じサイズのトリガーを設定してください。")]
        [SerializeField] private ThroughTrigger objectTrigger;

        [Header("貫通の入力方向")]
        [SerializeField] private Direction inputDirection;

        [Header("完全なすり抜けを可能にするか")]
        [Tooltip("trueにすると、スティック入力をし続けた時に引っ掛からずに通り抜けれます")]
        [SerializeField] private bool isCompleteThrough = true;

        [Header("一方通行にするか")]
        [SerializeField] private bool isOneWay = false;

        [Header("すり抜けるための入力時間")]
        [Tooltip("ここで設定した秒数だけ指定方向に入力し続けると、床を貫通してすり抜けます。")]
        [SerializeField] private float requiredInputTime = 0.25f;

        private float inputTimer;
        private Vector2 requiredDirection;
        private PlayerCondition condition;
        private Rigidbody rigidBody;

        private void Start() => Initialize();

        /// <summary>
        /// Inspectorで設定された方向に基づいて初期化。
        /// </summary>
        private void Initialize()
        {
            rigidBody = GetComponent<Rigidbody>();

            if (aroundTrigger != null)
                aroundTrigger.OnTriggerChanged += StatusCheck;
            if (objectTrigger != null)
                objectTrigger.OnTriggerChanged += StatusCheck;

            // switch文を簡略化したswitch式
            requiredDirection = inputDirection switch
            {
                Direction.Up => Vector2.up,
                Direction.Down => Vector2.down,
                Direction.Left => Vector2.left,
                Direction.Right => Vector2.right,
                _ => Vector2.zero
            };
        }

        private void OnDestroy()
        {
            if (aroundTrigger != null)
                aroundTrigger.OnTriggerChanged -= StatusCheck;
            if (objectTrigger != null)
                objectTrigger.OnTriggerChanged -= StatusCheck;
        }

        private void FixedUpdate()
        {
            // RigidBodyがスリープ状態のときは強制的に起動する
            if (rigidBody != null && rigidBody.IsSleeping())
            {
                rigidBody.WakeUp();
            }
        }

        private void SwitchPlatformLayer(bool isEnabled)
        {
            // 貫通可能なオブジェクトのレイヤーを切り替える
            gameObject.layer = isEnabled ? Layer.WaterOnly : Layer.ThroughPlatform;
        }

        /// <summary>
        ///  二つのトリガーをもとに台を接触可能な状態にするか判断
        /// </summary>
        private void StatusCheck(GameObject player)
        {
            if (condition == null)
                condition = player.GetComponent<PlayerCondition>();

            // WARNING: Triggerを大きくしすぎると、下入力しながら落下->Triggerに入ってから離す、ですり抜けが出来てしまう可能性あり。
            // 引っ掛からずにすり抜けも可能に
            if (condition.Direction == requiredDirection && isCompleteThrough)
                return;

            if (aroundTrigger.IsTriggered && !objectTrigger.IsTriggered)
                SwitchPlatformLayer(true); // 接触可能な状態に

            if (!aroundTrigger.IsTriggered && !objectTrigger.IsTriggered)
                SwitchPlatformLayer(false);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (isOneWay || condition == null)
                return;

            if (collision.gameObject.CompareTag(Tag.Handle.Player))
            {
                // 指定方向の入力がある場合
                if (condition.Direction == requiredDirection)
                {
                    inputTimer += Time.deltaTime;

                    if (inputTimer >= requiredInputTime)
                    {
                        SwitchPlatformLayer(false);
                        inputTimer = 0f;
                    }
                }
                else
                {
                    inputTimer = 0f;
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            // 離れたらリセット
            if (collision.gameObject.CompareTag(Tag.Handle.Player))
            {
                inputTimer = 0f;
            }
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}