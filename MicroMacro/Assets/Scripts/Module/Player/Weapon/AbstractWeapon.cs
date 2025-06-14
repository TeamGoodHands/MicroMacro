using UnityEngine;
using UnityEngine.Timeline;

namespace Module.Player.Weapon
{
    /// <summary>
    /// 武器の基底クラス
    /// </summary>
    public abstract class AbstractWeapon : MonoBehaviour
    {
        public abstract void Initialize(PlayerComponent component);
        public abstract void OnEnabled();
        public abstract void OnDisabled();
    }
}