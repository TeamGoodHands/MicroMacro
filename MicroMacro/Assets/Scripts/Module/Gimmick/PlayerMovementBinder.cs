using Constants;
using UnityEngine;

namespace Module.Gimmick
{
    /// <summary>
    /// 動的なオブジェクトにプレイヤーを追従させるコンポーネント
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(10)]
    public class PlayerMovementBinder : MonoBehaviour
    {
        [SerializeField] private Rigidbody rigidBody;
        private Rigidbody playerRigidBody;
        private Vector3 prevPosition;
        private Vector3 moveDelta;

        private void Start()
        {
            prevPosition = rigidBody.position;
        }

        private void Update()
        {
            // 床の移動量を先に計算
            moveDelta = rigidBody.position - prevPosition;

            // プレイヤーが乗っていたら、移動分加算
            if (playerRigidBody != null)
            {
                // プレイヤーが浮いてしまうので上方向の加算は行わない
                moveDelta.y = Mathf.Min(moveDelta.y, 0f);
                
                playerRigidBody.position += moveDelta;
            }

            prevPosition = rigidBody.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tag.Handle.Player) &&
                other.TryGetComponent(out Rigidbody playerRigidbody))
            {
                playerRigidBody = playerRigidbody;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.root.CompareTag(Tag.Handle.Player))
            {
                playerRigidBody = null;
            }
        }
    }
}