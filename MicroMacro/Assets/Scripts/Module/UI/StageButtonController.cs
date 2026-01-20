using UnityEngine;
using Module.UI;
using Module.Application.Data; // SaveManagerのnamespaceを追加

public class StageButtonController : MonoBehaviour
{
    [Header("ステージID設定 (例: 1-1, Boss)")]
    [SerializeField] private string stageId; // intからstringに変更して拡張性確保
    [SerializeField] private UIFrameAnimator frameAnimator;

    [Header("画像リソース")]
    [SerializeField] private Sprite[] normalSprites;  
    [SerializeField] private Sprite[] clearedSprites; 
    
    public string StageId => stageId;

    void Start()
    {
        // SaveManagerが存在しない場合の安全策
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveManager not found. Playing normal animation.");
            frameAnimator.Play(normalSprites, 3f);
            return;
        }

        // セーブデータを確認
        bool isCleared = SaveManager.Instance.IsStageCleared(stageId); 

        if (isCleared)
        {
            frameAnimator.Play(clearedSprites, 3f);
        }
        else
        {
            frameAnimator.Play(normalSprites, 3f);
        }
    }
}