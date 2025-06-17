using UnityEngine;

namespace Module.Gimmick
{
    public class ThroughTrigger : MonoBehaviour
    {
        
        
        [SerializeField] private ThroughPlatform throughPlatform;
        private bool isTriggered;
        public bool IsTriggered { get; private set; }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                IsTriggered = true;
                throughPlatform.StatusCheck();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                IsTriggered = false;
                throughPlatform.StatusCheck();
            }
        }
    }
}