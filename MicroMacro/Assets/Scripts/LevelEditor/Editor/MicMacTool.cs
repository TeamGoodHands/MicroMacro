using Editor.LevelEditor;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace LevelEditor.Editor
{
    [EditorTool("MicMacMaker Tool")]
    internal class MicMacTool : EditorTool
    {
        private GUIContent iconContent;
        private MicMacMaker editorWindow;

        private void OnEnable()
        {
            iconContent = new GUIContent()
            {
                image = null,
                text = "M",
                tooltip = "MicMacMaker"
            };
        }

        public override GUIContent toolbarIcon => iconContent;

        // ツールが表示される条件
        public override bool IsAvailable()
        {
            return EditorWindow.HasOpenInstances<MicMacMaker>();
        }

        public override void OnActivated()
        {
            if (EditorWindow.HasOpenInstances<MicMacMaker>())
            {
                editorWindow = EditorWindow.GetWindow<MicMacMaker>();
                editorWindow.SetEnable(true);
            }
        }

        public override void OnWillBeDeactivated()
        {
            if (EditorWindow.HasOpenInstances<MicMacMaker>())
            {
                editorWindow = EditorWindow.GetWindow<MicMacMaker>();
                editorWindow.SetEnable(false);
            }
        }

        // This is called for each window that your tool is active in. Put the functionality of your tool here.
        public override void OnToolGUI(EditorWindow window)
        {
            // シーンビューのコントロールを奪う
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            EditorGUI.BeginChangeCheck();

            Event evt = Event.current;

            if (evt.type == EventType.MouseDown && evt.button == 1)
            {
                // editorWindow.OnEraseEnable(true);
                evt.Use(); // イベント消費
            }
            else if (evt.type == EventType.MouseUp && evt.button == 1)
            {
                // editorWindow.OnEraseEnable(false);
                evt.Use(); // イベント消費
            }

            if (EditorGUI.EndChangeCheck())
            {
            }

            SceneView sceneView = window as SceneView;
            editorWindow.OnSceneGUI();
            sceneView.Repaint();
        }
    }
}