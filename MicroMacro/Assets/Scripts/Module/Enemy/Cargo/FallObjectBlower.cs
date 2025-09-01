using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class FallObjectBlower : MonoBehaviour
    {
        [SerializeField] private FallObjectEnemyChecker enemyChecker;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private Collider objectCollider;
        [SerializeField] private float blowPower;
        [SerializeField] private float torquePower;

        private float blowDirection = -1;

        public void SetBlowDirection(float direction)
        {
            blowDirection = direction;
        }

        private void Start()
        {
            enemyChecker.OnHit += OnHit;
        }

        private void OnHit(GameObject obj)
        {
            AddForceAsync().Forget();
        }

        private async UniTaskVoid AddForceAsync()
        {
            await UniTask.Yield();
            
            rigidBody.isKinematic = false;
            objectCollider.enabled = false;
            rigidBody.AddForce(Vector3.up * blowPower + Vector3.right * (blowDirection * blowPower));
            rigidBody.AddTorque(-Vector3.forward * (blowDirection * torquePower));
        }
    }
}