using System;
using System.Linq;
using Constants;
using LevelEditor.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LevelEditor.Editor
{
    public class ObjectPlacer
    {
        public event Action OnSequenceCanceled;

        private GameObject prefab;
        private GameObject targetObject;
        private MicMacLevelData parentObject;
        private bool isErasing;
        private bool isWindowEnter;
        private Vector2Int mousePosition;

        public ObjectPlacer()
        {
            UpdateParentObject();
        }

        public void Enable()
        {
            isWindowEnter = true;
            SceneView.duringSceneGui += HandleSceneGUI;
        }

        public void Disable()
        {
            SceneView.duringSceneGui -= HandleSceneGUI;
        }

        public void UpdateParentObject()
        {
            if (parentObject != null)
                return;

            GameObject parent = GameObject.FindWithTag(Tag.Level);
            if (parent == null || !parent.TryGetComponent(out parentObject))
            {
                Debug.LogError("マップオブジェクトの親オブジェクトがありません。");
            }
        }

        public void StartPlaceSequence(GameObject prefab)
        {
            UpdateParentObject();
            this.prefab = prefab;
            targetObject = PrefabUtility.InstantiatePrefab(prefab, parentObject.transform) as GameObject;
            targetObject.SetActive(false);
            SceneView.lastActiveSceneView.Focus();
        }

        public void StopPlaceSequence()
        {
            if (targetObject == null)
                return;

            Object.DestroyImmediate(targetObject);
            prefab = null;
        }

        public void SetEraseMode(bool isErasing)
        {
            bool prevErasing = this.isErasing;
            this.isErasing = isErasing;

            if (!prevErasing && isErasing)
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(false);
                }
            }
            else if (prevErasing && !isErasing)
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(true);
                }
            }
        }

        public void DestroyAll()
        {
            UpdateParentObject();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Delete Multiple Map Objects");

            foreach (GameObject obj in parentObject.MapData.Select(item => item.Value.Object))
            {
                if (obj == null)
                    continue;

                Undo.DestroyObjectImmediate(obj);
            }

            Undo.RecordObject(parentObject, "Delete Map Data");
            parentObject.MapData.Clear();

            EditorUtility.SetDirty(parentObject);

            Undo.CollapseUndoOperations(undoGroup);
        }

        public void DrawMousePosition()
        {
            if (!isWindowEnter)
                return;

            Handles.color = isErasing ? new Color(1f, 0.11f, 0f) : new Color(0f, 0.91f, 1f);
            Handles.DrawAAPolyLine(5f,
                new Vector3(mousePosition.x - 0.5f, mousePosition.y - 0.5f, 0f),
                new Vector3(mousePosition.x - 0.5f, mousePosition.y + 0.5f, 0f),
                new Vector3(mousePosition.x + 0.5f, mousePosition.y + 0.5f, 0f),
                new Vector3(mousePosition.x + 0.5f, mousePosition.y - 0.5f, 0f),
                new Vector3(mousePosition.x - 0.5f, mousePosition.y - 0.5f, 0f));
        }

        public void DrawMap()
        {
            if (parentObject == null || parentObject.MapData == null)
                return;

            foreach (var data in parentObject.MapData)
            {
                Vector2Int coord = parentObject.IndexToCoord(data.Key);

                Handles.color = new Color(1f, 0.48f, 0.51f);
                Handles.DrawWireCube(new Vector3(coord.x, coord.y, 0f), Vector3.one);
            }
        }

        private void HandleSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            UpdateEraseMode(e);

            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                TryPlaceOrErase(e);
            }

            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
            {
                mousePosition = GetCurrentGridPosition();
                MoveSelectedObject();
            }

            else if (e.type == EventType.MouseLeaveWindow)
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(false);
                }

                isWindowEnter = false;
            }
            else if (e.type == EventType.MouseEnterWindow)
            {
                if (targetObject != null && !isErasing)
                {
                    targetObject.SetActive(true);
                }

                isWindowEnter = true;
            }
            else if (e.type == EventType.ScrollWheel)
            {
                if (e.shift && !isErasing)
                {
                    RotateSelectedObject(e.delta);
                }
            }
            else if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    StopPlaceSequence();
                    OnSequenceCanceled?.Invoke();
                }
            }
        }

        private void UpdateEraseMode(Event e)
        {
            if (e.button == 1)
            {
                if (e.type == EventType.MouseUp)
                {
                    SetEraseMode(false);
                }
                else if (e.type == EventType.MouseDown)
                {
                    SetEraseMode(true);
                }
            }
            else if (e.type == EventType.MouseMove)
            {
                SetEraseMode(false);
            }
        }

        private void MoveSelectedObject()
        {
            if (targetObject == null)
                return;

            targetObject.transform.localPosition = new Vector3(mousePosition.x, mousePosition.y, 0f);
        }

        private void RotateSelectedObject(Vector2 delta)
        {
            float direction = Mathf.Sign(delta.x);
            targetObject.transform.eulerAngles += new Vector3(0f, 0f, direction * 90f);
        }

        private void Place()
        {
            if (prefab == null)
                return;

            // グリッド上の座標を取得
            Vector3 position = targetObject.transform.localPosition;
            Vector2Int gridPos = new Vector2Int((int)position.x, (int)position.y);
            long gridIndex = parentObject.CoordToIndex(gridPos);

            // 既に配置されている位置であれば何もしない
            if (parentObject.MapData.ContainsKey(gridIndex))
            {
                return;
            }

            int undoGroup = Undo.GetCurrentGroup();

            Undo.SetCurrentGroupName("Place Object");
            {
                // オブジェクト作成
                GameObject obj = PrefabUtility.InstantiatePrefab(prefab, parentObject.transform) as GameObject;
                obj.transform.SetPositionAndRotation(targetObject.transform.position, targetObject.transform.rotation);
                Undo.RegisterCreatedObjectUndo(obj, "Place Object: " + obj.name);

                // マップデータ変更
                Undo.RecordObject(parentObject, "Erase Map Data");
                parentObject.MapData.Add(gridIndex, new CellData(obj, new long[] { gridIndex }));

                // シーンに変更を登録
                EditorUtility.SetDirty(parentObject);
            }
            Undo.CollapseUndoOperations(undoGroup);
        }

        private void TryPlaceOrErase(Event e)
        {
            if (e.button == 0)
            {
                Place();
            }

            if (e.button == 1)
            {
                Erase();
            }
        }

        private void Erase()
        {
            // インデックスからマップデータを取得
            long gridIndex = parentObject.CoordToIndex(mousePosition);
            if (!parentObject.MapData.TryGetValue(gridIndex, out var value))
                return;

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Erase Object");
            {
                // 対象のオブジェクトを削除
                Undo.DestroyObjectImmediate(value.Object);

                // マップデータ変更
                Undo.RecordObject(parentObject, "Erase Map Data");
                parentObject.MapData.Remove(gridIndex);

                // シーンに変更を登録
                EditorUtility.SetDirty(parentObject);
            }
            Undo.CollapseUndoOperations(undoGroup);
        }

        private Vector2Int GetCurrentGridPosition()
        {
            UpdateParentObject();

            // マウス座標からワールド座標に変換
            SceneView sceneView = SceneView.currentDrawingSceneView;
            Vector3 screenPosition = Event.current.mousePosition * EditorGUIUtility.pixelsPerPoint;
            screenPosition.y = sceneView.camera.pixelHeight - screenPosition.y;
            Vector3 worldPosition = sceneView.camera.ScreenToWorldPoint(screenPosition);
            worldPosition -= parentObject.transform.position;

            // スナッピングする
            worldPosition = Snapping.Snap(worldPosition, EditorSnapSettings.move);
            return new Vector2Int((int)worldPosition.x, (int)worldPosition.y);
        }
    }
}