using Constants;
using Module.Player;
using Module.Player.Component;
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class WaterPusher : MonoBehaviour
    {
        [SerializeField] private GameObject waterObject;
        [SerializeField] private float centeringStrength = 5.0f; // 座標を寄せる強さ

        [SerializeField, Header("水を抜けたときに水流方向に吹き飛ばす強さ")]
        private float exitPower = 1.5f;

        private Rigidbody rigidBody;
        private PlayerMovement playerMovement;
        private WaterFlow waterFlow;

        private void Start()
        {
            GameObject player = GameObject.FindWithTag(Tag.Player);
            rigidBody = player.GetComponent<Rigidbody>();
            playerMovement = player.GetComponent<PlayerBehaviour>().Component.PlayerMovement;
        }

        private void GetWaterFlow()
        {
            if (waterFlow != null)
                return;
            
            if (waterObject.TryGetComponent(out IWaterFlow flow))
            {
                waterFlow = flow.WaterFlow;
            }
            else
            {
                Debug.LogError("WaterPusher: Water object does not have WaterFlow component.");
            }
        }

        private void OnTriggerStay(Collider other)
        {
            GetWaterFlow();
            
            if (other.gameObject.CompareTag(Tag.Handle.Player))
            {
                ApplyWaterForce();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(Tag.Handle.Player))
            {
                Vector3 verticalForce = waterFlow.CalculateVerticalForce(true);
                playerMovement.AddExternalForce(verticalForce * exitPower);
            }
        }

        private void ApplyWaterForce()
        {
            // --- 追加: 横方向の速度を殺す ---
            // 現在の速度を、このオブジェクトのY軸（進行方向）に投影する
            // これにより「進行方向成分」だけが残り、横ブレの速度が 0 になります
            Vector3 velocityAlongAxis = Vector3.Project(rigidBody.linearVelocity, transform.up);

            // 速度を上書き（横方向の慣性を消滅させる）
            rigidBody.linearVelocity = velocityAlongAxis;
            // -----------------------------

            Vector3 verticalForce = waterFlow.CalculateVerticalForce(true);
            Vector3 horizontalForce = waterFlow.CalculateHorizontalForce(true, rigidBody.position);

            // --- 座標を強制的に軸へ寄せる（前回の処理） ---
            Vector3 vectorToPlayer = rigidBody.position - transform.position;
            Vector3 projectionOnAxis = Vector3.Project(vectorToPlayer, transform.up);
            Vector3 targetAxisPoint = transform.position + projectionOnAxis;

            Vector3 newPosition = Vector3.Lerp(rigidBody.position, targetAxisPoint, centeringStrength * Time.deltaTime);
            rigidBody.position = newPosition;
            // ----------------------------------------

            // 最後に力を加える（これにより進行方向へは加速する）
            Vector3 force = verticalForce + horizontalForce;
            rigidBody.AddForce(force);
        }
    }
}