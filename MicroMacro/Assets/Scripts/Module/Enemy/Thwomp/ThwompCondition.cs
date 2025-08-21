using System;
using UnityEngine;

namespace Module.Enemy.Thwomp
{
    [Serializable]
    public class ThwompCondition
    {
        public enum State
        {
            Idle,
            Ascending,
            Moving,
            Falling,
            Death
        }

        [SerializeField] private State currentState;
        [SerializeField] private float lastAttackTime;

        public State CurrentState
        {
            get => currentState;
            set => currentState = value;
        }

        public float LastAttackTime
        {
            get => lastAttackTime;
            set => lastAttackTime = value;
        }
    }
}