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
        private Vector2 direction;
        private PlayerCondition condition;

        private void Start() => Initialize();

        /// <summary>
        /// Inspectorで設定された方向に基づいて初期化。
        /// </summary>
        private void Initialize()
        {
            if (aroundTrigger != null)
                aroundTrigger.OnTriggerChanged += StatusCheck;
            if (objectTrigger != null)
                objectTrigger.OnTriggerChanged += StatusCheck;
            
            // switch文を簡略化したswitch式
            direction = inputDirection switch
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
        
        private void SwitchPlatformLayer(bool isEnabled)
        {
            // 貫通可能なオブジェクトのレイヤーを切り替える
            gameObject.layer = isEnabled ? Layer.Default : Layer.ThroughPlatform;
        }
        
        /// <summary>
        ///  二つのトリガーをもとに台を接触可能な状態にするか判断
        /// </summary>
        private void StatusCheck(GameObject player)
        {
            if (condition == null)
                condition = player.GetComponentInParent<PlayerCondition>();
            
            // WARNING: Triggerを大きくしすぎると、下入力しながら落下->Triggerに入ってから離す、ですり抜けが出来てしまう可能性あり。
            // 引っ掛からずにすり抜けも可能に
            if (condition.Direction == direction && isCompleteThrough)
                return;
            
            if (aroundTrigger.IsTriggered && !objectTrigger.IsTriggered)
            {
                // 接触可能な状態に
                SwitchPlatformLayer(true);
            }
            
            if (!aroundTrigger.IsTriggered && !objectTrigger.IsTriggered)
                SwitchPlatformLayer(false);
        }
          
        private void OnCollisionStay(Collision collision)
        {
            if (isOneWay || condition == null)
                return;
            
            // 下入力で降りる
            if (collision.gameObject.CompareTag(Tag.Handle.Player))
            {
                if (condition.Direction == direction)
                {
                    SwitchPlatformLayer(false);
                }
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