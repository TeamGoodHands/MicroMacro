using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    [Serializable]
    public struct FallPatternPair
    {
        public int From;
        public int To;
    }

    [Serializable]
    public class FallPatternPairList
    {
        public FallPatternPair[] pattern;
    }

    [CreateAssetMenu(fileName = "FallPattern", menuName = "ScriptableObjects/FallPattern", order = 1)]
    public class FallPattern : ScriptableObject
    {
        [SerializeField] private List<FallPatternPairList> pattern;

        public List<FallPatternPair[]> GetPattern()
        {
            return pattern.Select(list => list.pattern).ToList();
        }
        
    }
}