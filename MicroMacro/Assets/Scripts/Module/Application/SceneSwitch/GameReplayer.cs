using Constants;
using Module.UI;
using UnityEngine;

namespace Module.Application.SceneSwitch
{
    public class GameReplayer : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition sceneTransition;
        [SerializeField] private string nextSceneName;
        private HealthStatus playerHealthStatus;

        public bool IsActive { get; set; } = true;
    
        private void Start()
        {
            playerHealthStatus = GameObject.FindWithTag(Tag.Player).GetComponent<HealthStatus>();
            playerHealthStatus.OnDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            if (!IsActive) return;
            sceneTransition.StartNormalTransition(nextSceneName);
        }
    }
}
