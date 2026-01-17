#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class InspectorLockShortcut
{
    // ショートカットキーの設定 (例: Ctrl + Q)
    [MenuItem("Tools/Toggle Inspector Lock %q")]
    static void ToggleLock()
    {
        // 現在アクティブなエディタ（インスペクター）のロック状態を反転
        ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
        
        // UIを強制再描画してアイコンの見た目を更新
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }
}
#endif