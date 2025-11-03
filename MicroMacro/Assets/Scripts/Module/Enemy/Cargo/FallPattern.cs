using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    [Serializable]
    public struct Wave
    {
        public float Time;
        public int From;
        public int To;
    }

    [CreateAssetMenu(fileName = "FallPattern", menuName = "ScriptableObjects/FallPattern", order = 1)]
    public class FallPattern : ScriptableObject
    {
        [SerializeField] private List<Wave> waves;

        public List<Wave> GetWaves()
        {
            return waves;
        }
    }
}