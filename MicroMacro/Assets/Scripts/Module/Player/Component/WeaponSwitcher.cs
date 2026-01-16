using System;
using System.Collections.Generic;
using System.Linq;
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

        public T GetWeapon<T>() where T : AbstractWeapon
        {
            T result = weapons.OfType<T>().FirstOrDefault();
            
            if (result == null)
                throw new InvalidOperationException($"Weapon of type {typeof(T).Name} not found.");

            return result;
        }

        public void Switch()
        {
            // テストプレイ期間中は無効化
            weapons[currentIndex].OnDisabled();

            // インデックスをループして進める
            currentIndex = (currentIndex + 1) % weapons.Count;

            weapons[currentIndex].OnEnabled();
        }

        public void Destroy()
        {
            // 武器の無効化
            foreach (AbstractWeapon weapon in weapons)
            {
                weapon.OnDisabled();
            }
        }
    }
}