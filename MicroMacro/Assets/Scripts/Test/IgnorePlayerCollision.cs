using CoreModule.Input;
using UnityEngine;

public class IgnorePlayerCollision : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // このオブジェクトのColliderを取得
        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null) return;

        // "Player"タグの付いたすべてのオブジェクトを探す
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null)
            {
                // 親オブジェクトとプレイヤーの衝突を無視
                Physics.IgnoreCollision(myCollider, playerCollider);
            }
        }
    }
}
