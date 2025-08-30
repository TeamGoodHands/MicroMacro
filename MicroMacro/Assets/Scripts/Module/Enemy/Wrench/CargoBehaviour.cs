using System;
using CoreModule.AI.HSM;
using Module.Player;
using UnityEngine;

namespace Module.Enemy.Wrench
{
    public class WrenchBehaviour : MonoBehaviour
    {
        [SerializeField] private WrenchComponent component;
        private HierarchicalStateMachine stateMachine;

        private void Start()
        {
            stateMachine = new HierarchicalStateMachine();

            MoveState moveState = new MoveState(component);
            stateMachine.AddState(moveState);
            
            stateMachine.Start<MoveState>();
        }
    }
}