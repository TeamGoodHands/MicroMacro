using Constants;
using Module.UI;
using UnityEngine;

namespace Module.Application.SceneSwitch
{
    public class GameReplayer : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition sceneTransition;
        private HealthStatus playerHealthStatus;
    
        private void Start()
        {
            playerHealthStatus = GameObject.FindWithTag(Tag.Player).GetComponent<HealthStatus>();
            playerHealthStatus.OnDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            Debug.Log("プレイヤーが死亡しました。ゲームをリプレイします。");
            sceneTransition.StartTransitionSame();
        }
    }
}
