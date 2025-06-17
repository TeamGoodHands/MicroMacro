using UnityEngine;

namespace Module.Player.Component
{
    public class PlayerStatus : MonoBehaviour, IDamageable
    {
        public void Damage(int damage)
        {
            Debug.Log("ダメージを受けました");
        }
    }
}