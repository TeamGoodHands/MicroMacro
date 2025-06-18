using System;
using Module.Player.Component;
using Module.Gimmick;
using UnityEngine;

namespace Module.Player
{
    [Serializable]
    public class PlayerComponent
    {
        [SerializeField] private PlayerParameter parameter;
        [SerializeField] private PlayerCondition condition;
        [SerializeField] private Rigidbody rigidbody;
       
        public PlayerParameter Parameter => parameter;
        public PlayerCondition Condition => condition;
        public Rigidbody Rigidbody => rigidbody;
        public Transform Transform => rigidbody.transform;
        public PlayerMovement PlayerMovement => playerMovement ??= new PlayerMovement(parameter, condition);
        public PlayerRotation PlayerRotation => playerRotation ??= new PlayerRotation(parameter, condition);
        public WeaponSwitcher WeaponSwitcher => weaponSwitcher ??= new WeaponSwitcher(this);

        private PlayerMovement playerMovement;
        private PlayerRotation playerRotation;
        private WeaponSwitcher weaponSwitcher;
    }
}