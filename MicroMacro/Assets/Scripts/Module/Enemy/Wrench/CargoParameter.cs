using System;
using UnityEngine;

namespace Module.Enemy.Wrench
{
    [Serializable]
    public class WrenchParameter
    {
        [SerializeField] private float moveSpeed;

        public float MoveSpeed => moveSpeed;
    }
}