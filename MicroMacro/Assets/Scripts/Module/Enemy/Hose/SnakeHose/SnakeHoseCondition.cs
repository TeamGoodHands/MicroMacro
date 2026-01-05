using NaughtyAttributes;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose
{
    public class SnakeHoseCondition : MonoBehaviour
    {
        public enum State
        {
            Move,
            Attack
        }

        [SerializeField, ReadOnly] private State currentState = State.Move;

        public State CurrentState
        {
            get => currentState;
            set => currentState = value;
        }
        
        public void Attack()
        {
            CurrentState = State.Attack;
        }

        public void Move()
        {
            CurrentState = State.Move;
        }
    }
}