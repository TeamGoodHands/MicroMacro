using UnityEngine;
using Module.UI;

public class StageButtonController : MonoBehaviour
{
    [SerializeField] private int stageId; // ステージIDなど
    [SerializeField] private UIFrameAnimator frameAnimator;

    [Header("画像リソース")]
    [SerializeField] private Sprite[] normalSprites;  // 未クリア時の画像セット
    [SerializeField] private Sprite[] clearedSprites; // クリア時の画像セット

    void Start()
    {
        // ここでセーブデータを確認するイメージ
        // bool isCleared = SaveManager.IsStageCleared(stageId); 
        bool isCleared = false; // 仮

        if (isCleared)
        {
            // クリア済みの画像を再生
            frameAnimator.Play(clearedSprites, 3f);
        }
        else
        {
            // 通常の画像を再生
            frameAnimator.Play(normalSprites, 3f);
        }
    }
}