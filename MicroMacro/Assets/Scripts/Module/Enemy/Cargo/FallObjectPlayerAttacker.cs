using System;
using Constants;
using Module.Player.Component;
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

        private void OnCollisionEnter(Collision other)
        {
            if (!doAttack)
                return;

            if (other.gameObject.CompareTag(Tag.Handle.Player) &&
                other.gameObject.TryGetComponent(out PlayerStatus playerStatus))
            {
                playerStatus.Damage(1);
                doAttack = false;
            }
        }

        public void Reset()
        {
            doAttack = true;
        }
    }
}