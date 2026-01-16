using System;
using Constants;
using Module.Player.Component;
using Module.UI;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class FallObjectPlayerAttacker : MonoBehaviour
    {
        [SerializeField] private FallObjectEnemyChecker enemyChecker;
        private bool doAttack = true;

        private void Start()
        {
            enemyChecker.OnHit += OnHit;
        }

        private void OnHit(GameObject obj)
        {
            doAttack = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!doAttack)
                return;

            if (other.gameObject.CompareTag(Tag.Handle.Player) &&
                other.gameObject.TryGetComponent(out HealthStatus playerStatus))
            {
                playerStatus.Damage(1);
                doAttack = false;
                gameObject.layer = Layer.IgnorePlayer;
            }
        }

        public void Reset()
        {
            doAttack = true;
            gameObject.layer = Layer.Default;
        }
    }
}