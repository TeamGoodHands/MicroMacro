using Constants;
using UnityEngine;

namespace Module.Gimmick
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovementBinder : MonoBehaviour
    {
        [SerializeField] private Rigidbody rigidBody;
        private Rigidbody playerRigidBody;
        private Vector3 prevPosition;
        private Vector3 moveDelta;

        private void Update()
        {
            // 床の移動量を先に計算
            moveDelta = rigidBody.position - prevPosition;

            // プレイヤーが乗っていたら、移動分加算
            if (playerRigidBody != null)
            {
                // position変更だけでなく、速度も床に合わせると自然
                playerRigidBody.position += moveDelta;
            }

            prevPosition = rigidBody.position;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.root.CompareTag(Tag.Handle.Player) &&
                other.transform.root.TryGetComponent(out Rigidbody playerRigidbody))
            {
                playerRigidBody = playerRigidbody;
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (other.transform.root.CompareTag(Tag.Handle.Player))
            {
                playerRigidBody = null;
            }
        }
    }
}