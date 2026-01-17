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
            BeamAttack,
            SmashAttack
        }

        [SerializeField, ReadOnly] private State currentState = State.Appear;
        [SerializeField, ReadOnly] private Vector3 defaultPosition;

        public State CurrentState
        {
            get => currentState;
            set => currentState = value;
        }

        public Vector3 DefaultPosition
        {
            get => defaultPosition;
            set => defaultPosition = value;
        }
    }
}