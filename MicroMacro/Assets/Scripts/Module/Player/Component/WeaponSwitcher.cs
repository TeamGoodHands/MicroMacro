using System.Collections.Generic;
using Module.Player.Weapon;

namespace Module.Player.Component
{
    /// <summary>
    /// 武器の切り替えを管理するクラス
    /// </summary>
    public class WeaponSwitcher
    {
        private readonly PlayerComponent component;
        private readonly IReadOnlyList<AbstractWeapon> weapons;
        private int currentIndex;

        public WeaponSwitcher(PlayerComponent component)
        {
            this.component = component;
            weapons = component.Parameter.Weapons;
        }

        public void Initialize()
        {
            // 武器の初期化
            foreach (AbstractWeapon weapon in weapons)
            {
                weapon.Initialize(component);
                weapon.OnDisabled();
            }
            
            weapons[currentIndex].OnEnabled();
        }

        public void Switch()
        {
            weapons[currentIndex].OnDisabled();

            // インデックスをループして進める
            currentIndex = (currentIndex + 1) % weapons.Count;

            weapons[currentIndex].OnEnabled();
        }
    }
}