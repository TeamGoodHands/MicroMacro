using UnityEngine;

namespace Module.Enemy.Hose.ChildSnake
{
    public class ChildSnakeParameter : MonoBehaviour
    {
        [SerializeField] private float firstDelay;
        [SerializeField] private float timeToFacePlayer;
        [SerializeField] private float shakeTime;
        [SerializeField] private float attackDuration;
        [SerializeField] private float timeToResetAngle;
        
        public float FirstDelay => firstDelay;
        public float TimeToFacePlayer => timeToFacePlayer;
        public float ShakeTime => shakeTime;
        public float AttackDuration => attackDuration;
        public float TimeToResetAngle => timeToResetAngle;
    }
}