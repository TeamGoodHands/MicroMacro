using NaughtyAttributes;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose
{
    public class SnakeHoseCondition : MonoBehaviour
    {
        public enum State
        {
            Appear,
            WaterBallAttack,
            BeamAttack
        }

        [SerializeField, ReadOnly] private State currentState = State.Appear;

        public State CurrentState
        {
            get => currentState;
            set => currentState = value;
        }
    }
}