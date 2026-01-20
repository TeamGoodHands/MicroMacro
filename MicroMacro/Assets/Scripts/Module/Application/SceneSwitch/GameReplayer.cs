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
    
        private void Start()
        {
            playerHealthStatus = GameObject.FindWithTag(Tag.Player).GetComponent<HealthStatus>();
            playerHealthStatus.OnDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            sceneTransition.StartTransition(nextSceneName);
        }
    }
}
