using UnityEngine;
using UnityEngine.EventSystems;

namespace Module.UI
{
    /// <summary>
    /// 選択状態が外れる（nullになる）のを防ぎ、直前の選択を復帰させるクラス
    /// </summary>
    public class ForeverSelect : MonoBehaviour
    {
        private GameObject lastSelected;

        private void Update()
        {
            if (EventSystem.current == null) return;

            var current = EventSystem.current.currentSelectedGameObject;

            if (current != null)
            {
                // 何か選ばれていれば、それを「最後の選択」として記憶
                lastSelected = current;
            }
            else
            {
                // 何も選ばれていない状態で記憶があるなら復帰
                if (lastSelected != null && lastSelected.activeInHierarchy)
                {
                    EventSystem.current.SetSelectedGameObject(lastSelected);
                }
            }
        }
    }
}