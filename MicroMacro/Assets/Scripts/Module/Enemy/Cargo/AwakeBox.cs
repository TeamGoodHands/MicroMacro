using Module.Management;
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
            
            SoundManager.instance.Play("打撃1");
            SoundManager.instance.Play("Boss2");
        }
    }
}