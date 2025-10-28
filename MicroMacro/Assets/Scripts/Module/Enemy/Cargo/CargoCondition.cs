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
            BackAttack,
            PrepareMove,
            Death
        }

        [SerializeField] private State currentState = State.Sleeping;
        [SerializeField] private State previousState = State.Sleeping;
        [SerializeField] private Vector3 moveDelta = Vector3.zero;
        [SerializeField] private bool isStart;
        
        public event Action<State> OnStateChanged;

        public State CurrentState
        {
            get => currentState;
        }

        public State PreviousState
        {
            get => previousState;
        }

        public bool IsStart
        {
            get => isStart;
            set => isStart = value;
        }

        public Vector3 MoveDelta
        {
            get => moveDelta;
            set => moveDelta = value;
        }

        public void SwitchState(State newState)
        {
            previousState = currentState;
            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}