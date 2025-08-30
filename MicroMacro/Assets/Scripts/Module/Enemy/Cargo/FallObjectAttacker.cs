using System;
using Module.Management;
using Module.Scaling;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class FallObjectAttacker : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private FallObjectEnemyChecker enemyChecker;

        private void Start()
        {
            enemyChecker.OnHit += Damage;
        }

        private void Damage(GameObject target)
        {
            // スケールを大きくされていればダメージを与える
            if (scaler.CurrentStep > 0)
            {
                target.GetComponent<EnemyStatus>().Damage(1);
                SoundManager.instance.Play("打撃1");
            }
        }
    }
}