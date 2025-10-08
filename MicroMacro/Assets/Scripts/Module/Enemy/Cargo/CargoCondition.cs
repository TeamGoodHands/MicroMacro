using System;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    [Serializable]
    public class CargoCondition
    {
        public enum State
        {
            Sleeping,
            Move,
            BlowAway,
            BackAttack,
            PrepareMove
        }

        [SerializeField] private State currentState = State.Sleeping;
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