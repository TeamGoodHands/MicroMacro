using Constants;
using Module.Player;
using Module.Player.Component; 
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class WaterPusher : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WaterPhysicsSystem physicsSystem; 
        
        [Header("Parameters")]
        [SerializeField] private float centeringStrength = 5.0f; 
        [SerializeField] private float exitPower = 1.5f;         
        [SerializeField] private float lateralDamping = 0.1f;    

        // ▼ 追加: このエリア専用の押し出し倍率
        [SerializeField, Header("水流の中にいるときの押し出し倍率")] 
        private float pushStrengthMultiplier = 1.0f;

        private Rigidbody playerRb;
        private PlayerMovement playerMovement;

        private void Start()
        {
            GameObject player = GameObject.FindWithTag(Tag.Player);
            if (player != null)
            {
                playerRb = player.GetComponent<Rigidbody>();
                var behaviour = player.GetComponent<PlayerBehaviour>();
                if (behaviour != null)
                {
                    playerMovement = behaviour.Component.PlayerMovement;
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (playerRb == null) return;
            
            if (other.CompareTag(Tag.Handle.Player))
            {
                ApplyWaterControl();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (playerMovement == null) return;

            if (other.CompareTag(Tag.Handle.Player))
            {
                // 脱出時のブーストにも倍率を乗せるかはお好みで（今回は乗せていません）
                Vector3 force = physicsSystem.CalculateForceForPusher(true);
                playerMovement.AddExternalForce(force * exitPower);
            }
        }

        private void ApplyWaterControl()
        {
            // 1. 横方向の速度減衰
            Vector3 currentVel = playerRb.linearVelocity;
            Vector3 forwardAxis = transform.up; 
            Vector3 projectedVel = Vector3.Project(currentVel, forwardAxis);
            
            playerRb.linearVelocity = Vector3.Lerp(currentVel, projectedVel, lateralDamping);

            // 2. センタリング
            Vector3 toPlayer = playerRb.position - transform.position;
            Vector3 axisPoint = transform.position + Vector3.Project(toPlayer, forwardAxis);
            
            Vector3 correctedPos = Vector3.Lerp(playerRb.position, axisPoint, centeringStrength * Time.fixedDeltaTime);
            playerRb.MovePosition(correctedPos);

            // 3. 推進力の付与
            Vector3 baseForce = physicsSystem.CalculateForceForPusher(true);
            
            // ▼ 修正: ここで倍率をドンと掛け算します
            Vector3 finalForce = baseForce * pushStrengthMultiplier;
            
            playerRb.AddForce(finalForce);
        }
    }
}