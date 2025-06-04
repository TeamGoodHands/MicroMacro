using System;
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

        /// <summary>
        /// 固定フレームごとに、接触中のプレイヤーRigidbodyに自身の移動量を加算し、移動差分を更新します。
        /// </summary>
        private void FixedUpdate()
        {
            if (playerRigidBody != null)
            {
                // プレイヤーのRigidbodyに移動量を適用
                playerRigidBody.position += moveDelta;
            }

            moveDelta = rigidBody.position - prevPosition;
            prevPosition = rigidBody.position;
        }

        /// <summary>
        /// プレイヤーオブジェクトと衝突した際に、そのRigidbody参照をキャッシュします。
        /// </summary>
        /// <param name="other">衝突したコライダー情報。</param>
        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.root.CompareTag(Tag.Handle.Player) &&
                other.transform.root.TryGetComponent(out Rigidbody playerRigidbody))
            {
                playerRigidBody = playerRigidbody;
            }
        }

        /// <summary>
        /// プレイヤーオブジェクトとの衝突が終了した際に、プレイヤーのRigidbody参照を解除します。
        /// </summary>
        /// <param name="other">衝突が終了したコライダー情報。</param>
        private void OnCollisionExit(Collision other)
        {
            if (other.transform.root.CompareTag(Tag.Handle.Player))
            {
                playerRigidBody = null;
            }
        }
    }
}