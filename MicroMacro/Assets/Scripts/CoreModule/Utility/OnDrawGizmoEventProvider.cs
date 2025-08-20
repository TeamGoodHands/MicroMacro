using System;
using UnityEngine;

namespace CoreModule.Utility
{
    public class OnDrawGizmoEventProvider : MonoBehaviour
    {
        public event Action OnDrawGizmosEvent;
        
        private void OnDrawGizmos()
        {
            OnDrawGizmosEvent?.Invoke();
        }
    }
}