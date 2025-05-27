using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ObjectPlacer
    {
        private GameObject targetObject;

        public void StartPlaceSequence(GameObject prefab)
        {
            Debug.Log("Start Place Sequence: " + prefab.name);
            targetObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            SceneView.duringSceneGui += HandleSceneGUI;
        }

        public void StopPlaceSequence()
        {
            Debug.Log("Stop Place Sequence: " + targetObject.name);
            Object.DestroyImmediate(targetObject);
            SceneView.duringSceneGui -= HandleSceneGUI;
        }

        private void HandleSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseMove:
                    Debug.Log("Mouse Move");
                    Vector3 screenPosition = e.mousePosition * EditorGUIUtility.pixelsPerPoint;
                    screenPosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - screenPosition.y;
                    Vector3 worldPosition = sceneView.camera.ScreenToWorldPoint(screenPosition);
                    worldPosition.z = 0f;
                    targetObject.transform.position = worldPosition;

                    break;
                case EventType.MouseDown:
                    PrefabUtility.InstantiatePrefab(targetObject);
                    Debug.Log("Mouse Down");
                    break;
                case EventType.MouseLeaveWindow:
                    Debug.Log("Mouse Leave Window");
                    break;
                case EventType.MouseEnterWindow:
                    Debug.Log("Mouse Enter Window");
                    break;
                case EventType.ScrollWheel:
                    Debug.Log("Scroll Wheel");
                    break;
            }
        }
    }
}