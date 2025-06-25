using Constants;
using Module.Player.Component;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    private PlayerStatus playerStatus;

    private void Start()
    {
        playerStatus = GameObject.FindWithTag(Tag.Player).GetComponent<PlayerStatus>();
        playerStatus.OnDeath += OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
