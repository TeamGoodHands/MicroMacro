using System;
using System.Collections.Generic;
using Module.Application.Data;
using Module.Management;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Module.UI
{
    public class StageButtonSelector : MonoBehaviour
    {
        [SerializeField] private List<StageButtonController> stageButtons;

        private void Start()
        {
            StageButtonController buttonController = stageButtons[0];
            
            // 最後にクリアしたステージがあるか
            if (SaveManager.Instance.LatestClearedStageId != null)
            {
                // クリアしたステージのボタンを検索
                int index = stageButtons.FindIndex(button => button.StageId == SaveManager.Instance.LatestClearedStageId);
                
                if (index != -1 && index + 1 < stageButtons.Count)
                {
                    buttonController = stageButtons[index];
                }
            }

            Select(buttonController);
        }

        private void Select(StageButtonController buttonController)
        {
            if (buttonController.gameObject != null && EventSystem.current != null)
            {
                // 一旦選択を解除してから再設定することで、確実にOnSelectを走らせる
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(buttonController.gameObject);
            }
        }
    }
}