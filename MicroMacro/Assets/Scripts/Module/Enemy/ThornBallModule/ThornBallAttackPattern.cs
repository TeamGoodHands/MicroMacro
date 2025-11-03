using System;
using System.Collections.Generic;
using UnityEngine;

namespace Module.Enemy.ThornBallModule
{
    [CreateAssetMenu(fileName = "ThornBallAttackPattern", menuName = "ScriptableObjects/ThornBallAttackPattern", order = 1)]
    public class ThornBallAttackPattern : ScriptableObject
    {
        [Serializable]
        public struct Pattern
        {
            public float Time;
            public int Position;
            public bool IsBig;
        }
        
        [SerializeField] private int areaDivide;
        [SerializeField] private List<Pattern> patterns;


        public float GetAreaDivide()
        {
            return areaDivide; 
        }

        public List<Pattern> GetPatterns()
        {
            return patterns;
        }
    }
}
