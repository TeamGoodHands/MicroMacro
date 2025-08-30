using System;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    [Serializable]
    public class CargoParameter
    {
        [Header("左右に動く状態")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private float amplitudeGainOnMove;
        [SerializeField] private float frequencyGainOnMove;
        
        [Header("後ろに攻撃する状態")]
        [SerializeField] private float amplitudeGainOnImpact;
        [SerializeField] private float frequencyGainOnImpact;
        [SerializeField] private float backAttackDistanceZ;
        [SerializeField] private float attackDelay;
        [SerializeField] private float attackMoveSpeed;

        public float MoveSpeed => moveSpeed;
        public float AmplitudeGainOnMove => amplitudeGainOnMove;
        public float FrequencyGainOnMove => frequencyGainOnMove;
        public float AmplitudeGainOnImpact => amplitudeGainOnImpact;
        public float FrequencyGainOnImpact => frequencyGainOnImpact;
        public float BackAttackDistanceZ => backAttackDistanceZ;
        public float AttackDelay => attackDelay;
        public float AttackMoveSpeed => attackMoveSpeed;
    }
}