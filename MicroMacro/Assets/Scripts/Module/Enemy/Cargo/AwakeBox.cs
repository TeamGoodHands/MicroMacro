using Module.Management;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class AwakeBox : MonoBehaviour
    {
        [SerializeField] private FallObjectEnemyChecker enemyChecker;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private float customGravity;

        private void Start()
        {
            enemyChecker.StartCheck();

            enemyChecker.OnHit += OnHit;
        }

        private void FixedUpdate()
        {
            rigidBody.AddForce(Vector3.down * customGravity, ForceMode.Acceleration);
        }

        private void OnHit(GameObject obj)
        {
            enemyChecker.StopCheck();

            SoundManager.instance.Play("打撃1");
            SoundManager.instance.Play("Boss2");
        }
    }
}