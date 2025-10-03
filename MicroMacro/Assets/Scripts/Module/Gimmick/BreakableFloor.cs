using System;
using Module.Player.Component;
using UnityEngine;

namespace Module.Gimmick
{
    public class BreakableFloor : MonoBehaviour
    {
        [Header("床を破壊するオブジェクト")]
        [SerializeField] private GameObject triggerObj;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject == triggerObj)
            {
                Break();
            }
        }

        private void Break()
        {
            Destroy(gameObject);
        }
    }
}