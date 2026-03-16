using UnityEngine;
using UnityEngine.UI;
using Module.Application.Data;
using Module.Application.SceneSwitch;
using UnityEngine.EventSystems; // フォーカス制御のために追加

namespace Module.UI
{
    public class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private FadeAndSceneTransition sceneTransition;
        
        [Header("遷移先")]
        [SerializeField] private string firstStageSceneName = "StageSelect"; 

        private void Start()
        {
            // ボタンのリスナー登録
            newGameButton.onClick.AddListener(OnNewGameClicked);
            continueButton.onClick.AddListener(OnContinueClicked);

            // セーブデータがなければ「続きから」を押せなくする
            bool hasSave = false;
            continueButton.interactable = false;
            if (SaveManager.Instance != null)
            {
                hasSave = SaveManager.Instance.HasSaveData();
                continueButton.interactable = hasSave;
            }

            // --- 修正箇所：状態確定後に初期フォーカスをスクリプトから設定 ---
            // セーブがあれば「続きから」、なければ「初めから」を選択
            GameObject firstSelected = hasSave ? continueButton.gameObject : newGameButton.gameObject;
            
            if (EventSystem.current != null)
            {
                // 一度nullにして確実にOnSelectを走らせる
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelected);
            }
        }

        private void OnNewGameClicked()
        {
            // データを消して新規開始
            if (SaveManager.Instance == null)
            {
                Debug.LogError("SaveManager is not initialized. Abort New Game transition.");
                return;
            }
            SaveManager.Instance.DeleteSave();
            sceneTransition.StartPageFlipTransition("Opening");
        }

        private void OnContinueClicked()
        {
            // データはそのままで遷移
            sceneTransition.StartPageFlipTransition(firstStageSceneName);
        }
    }
}