using System;
using Constants;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Editor
{
    public class ObjectPlacer
    {
        private GameObject prefab;
        private GameObject targetObject;
        private GameObject parentObject;
        private bool isErasing;

        public event Action OnSequenceCanceled;
        public bool IsPlacing => prefab != null;

        public ObjectPlacer()
        {
            parentObject = GameObject.FindWithTag(Tag.Level);
        }

        public void StartPlaceSequence(GameObject prefab)
        {
            Debug.Log("Start Place Sequence: " + prefab.name);
            this.prefab = prefab;
            targetObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            targetObject.SetActive(false);
            SceneView.duringSceneGui += HandleSceneGUI;
            SceneView.lastActiveSceneView.Focus();
        }

        public void StopPlaceSequence()
        {
            if (targetObject == null)
                return;

            Debug.Log("Stop Place Sequence: " + targetObject.name);
            Object.DestroyImmediate(targetObject);
            prefab = null;
            SceneView.duringSceneGui -= HandleSceneGUI;
        }

        public void StartEraseSequence()
        {
            Debug.Log("Start Erase Sequence: " + prefab.name);
            SceneView.duringSceneGui += HandleSceneGUI;
            isErasing = true;
        }

        private void HandleSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseMove:
                    Vector2 mousePosition = e.mousePosition;
                    MoveSelectedObject(mousePosition);
                    break;

                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (isErasing)
                        {
                            Object.DestroyImmediate(Selection.activeGameObject);
                        }

                        GameObject obj = PrefabUtility.InstantiatePrefab(prefab, parentObject.transform) as GameObject;
                        obj.transform.SetPositionAndRotation(targetObject.transform.position, targetObject.transform.rotation);
                        Undo.RegisterCreatedObjectUndo(obj, "Place Object: " + obj.name);
                    }

                    break;

                case EventType.MouseLeaveWindow:
                    targetObject.SetActive(false);
                    break;

                case EventType.MouseEnterWindow:
                    targetObject.SetActive(true);
                    break;

                case EventType.ScrollWheel:
                    if (e.shift)
                    {
                        RotateSelectedObject(e.delta);
                    }

                    break;
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Escape)
                    {
                        StopPlaceSequence();
                        OnSequenceCanceled?.Invoke();
                    }

                    break;
            }
        }

        private void MoveSelectedObject(Vector2 mousePosition)
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;
            Vector3 screenPosition = mousePosition * EditorGUIUtility.pixelsPerPoint;
            screenPosition.y = sceneView.camera.pixelHeight - screenPosition.y;
            Vector3 worldPosition = sceneView.camera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;

            // スナッピングする
            worldPosition = Snapping.Snap(worldPosition, EditorSnapSettings.move);

            targetObject.transform.position = worldPosition;
        }

        private void RotateSelectedObject(Vector2 delta)
        {
            float direction = Mathf.Sign(delta.x);
            targetObject.transform.eulerAngles += new Vector3(0f, 0f, direction * 90f);
        }
    }
}