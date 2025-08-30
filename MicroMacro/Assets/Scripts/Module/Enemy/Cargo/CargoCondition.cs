using System;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    [Serializable]
    public class CargoCondition
    {
        public enum State
        {
            Move,
            BackAttack,
            FallObjects
        }

        [SerializeField] private State currentState;
        [SerializeField] private bool isStart;

        public State CurrentState
        {
            get => currentState;
            set => currentState = value;
        }

        public bool IsStart
        {
            get => isStart;
            set => isStart = value;
        }
    }
}