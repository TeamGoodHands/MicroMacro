using UnityEditor;
using UnityEngine;

namespace LevelEditor.Editor
{
    [InitializeOnLoad]
    public static class QuickTeleport
    {
        // コンストラクタ（エディタ起動時やコンパイル後に呼ばれる）
        static QuickTeleport()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            
            // ctrl + shift + 左クリック
            if (e.type == EventType.MouseDown && e.button == 0 &&
                e.shift && e.control)  
            {
                TeleportPlayer(e.mousePosition);
            }
        }

        private static void TeleportPlayer(Vector2 mousePos)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player == null)
            {
                Debug.LogWarning("QuickTeleport: Playerタグがついたオブジェクトが見つかりません。");
                return;
            }
            
            // マウスの位置からワールド空間へのレイを作る
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            Vector3 targetPosition;
           
            // z=0の平面上の位置を計算する
            Plane plane = new Plane(Vector3.back, Vector3.zero);

            if (plane.Raycast(ray, out float enter))
            {
                // rayと平面(Z=0)が交差するポイントを取得
                targetPosition = ray.GetPoint(enter);
                    
                // 念のためZを完全に0にする（計算誤差対策）
                targetPosition.z = 0f;
            }
            else
            {
                // 万が一平面と交差しない場合（カメラが真上を向いている等）の安全策
                targetPosition = ray.GetPoint(10f);
                targetPosition.z = 0f;
            }
                
            
            
            // ctrl + zで元に戻せるようにUndo登録
            Undo.RecordObject(player.transform, "Teleport Player");
            
            player.transform.position = targetPosition;
            Debug.Log($"Teleported {player.name} to {targetPosition}");
        }
    }
}