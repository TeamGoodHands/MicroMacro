using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class AwakeBox : MonoBehaviour
    {
        [SerializeField] private FallObjectEnemyChecker enemyChecker;

        private void Start()
        {
            enemyChecker.StartCheck();

            enemyChecker.OnHit += OnHit;
        }

        private void OnHit(GameObject obj)
        {
            enemyChecker.StopCheck();
        }
    }
}