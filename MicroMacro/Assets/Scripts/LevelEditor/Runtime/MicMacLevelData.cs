using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using CoreModule.Serialization;
using UnityEngine;
using Object = System.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LevelEditor.Runtime
{
    [ExecuteAlways]
    public class MicMacLevelData : MonoBehaviour
    {
        private const int MapSize = 8192;
        [SerializeField, HideInInspector] private bool showBlocks;

        public bool ShowBlocks
        {
            get => showBlocks;
            set => showBlocks = value;
        }

        public long CoordToIndex(Vector2Int coord)
        {
            var offsetX = coord.x + (MapSize / 2);
            var offsetY = coord.y + (MapSize / 2);
            return offsetY * MapSize + offsetX;
        }

        public Vector2Int IndexToCoord(long index)
        {
            var x = (int)(index % MapSize) - (MapSize / 2);
            var y = (int)(index / MapSize) - (MapSize / 2);
            return new Vector2Int(x, y);
        }

        private MeshCombiner meshCombiner = new MeshCombiner();

        private void Start()
        {
            if (!Application.isPlaying)
                return;

            Dictionary<long, GameObject> mapData = GetMapData();

            // Y座標でグリッドデータを分割する
            Dictionary<int, List<(int x, MeshFilter filter)>> splitedFilters = SplitByY(mapData);

            var continuousX = new List<List<MeshFilter>>();

            foreach (var filters in splitedFilters.Values)
            {
                // 座標が連続しているMeshFilterをグループ化する
                continuousX.AddRange(GroupByContinuousX(filters));
            }

            var continuousY = GroupByContinuousY(continuousX);

            // 各グループをメッシュとして結合する
            foreach (List<MeshFilter> group in continuousY)
            {
                GameObject combinedObject = meshCombiner.CombineMeshes(group);
                combinedObject.transform.SetParent(transform);
            }

            if (!showBlocks)
            {
                // ゲーム開始時に見た目オブジェクトを削除する
                foreach (GameObject obj in mapData.Select(pair => pair.Value))
                {
                    Destroy(obj);
                }
            }
        }

        private Dictionary<long, GameObject> GetMapData()
        {
            List<GameObject> gridObjects = transform.Cast<Transform>()
                .Select(obj => obj.gameObject)
                .Where(obj => obj.CompareTag(Tag.Handle.LevelGrid))
                .ToList();

            var mapData = new Dictionary<long, GameObject>();

            foreach (GameObject obj in gridObjects)
            {
                Vector3 position = Snapping.Snap(obj.transform.position, Vector2.one);
                mapData.Add(CoordToIndex(new Vector2Int((int)position.x, (int)position.y)), obj);
            }

            return mapData;
        }

        public void RemoveOverlaps()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            var checkedCoords = new HashSet<Vector2Int>();

            foreach (GameObject obj in transform.Cast<Transform>()
                         .Select(obj => obj.gameObject)
                         .Where(obj => obj.CompareTag(Tag.Handle.LevelGrid)))
            {
                Vector3 snappedPosition = Snapping.Snap(obj.transform.position, Vector2.one);
                Vector2Int gridPos = new Vector2Int((int)snappedPosition.x, (int)snappedPosition.y);

                if (checkedCoords.Contains(gridPos))
                {
                    Undo.DestroyObjectImmediate(obj);
                    continue;
                }

                checkedCoords.Add(gridPos);
            }
#endif
        }

        private Dictionary<int, List<(int x, MeshFilter filter)>> SplitByY(Dictionary<long, GameObject> mapData)
        {
            const string colliderObjectKey = "Collider";
            var meshFilters = new Dictionary<int, List<(int x, MeshFilter filter)>>();

            foreach (var data in mapData)
            {
                // 各セルのオブジェクトからコライダーオブジェクトを取得
                Transform colliderObj = data.Value.transform.Find(colliderObjectKey);

                if (colliderObj != null && colliderObj.TryGetComponent(out MeshFilter meshFilter))
                {
                    Vector2Int coord = IndexToCoord(data.Key);

                    // Y座標をキーにして、MeshFilterをリストに追加
                    if (meshFilters.ContainsKey(coord.y))
                    {
                        meshFilters[coord.y].Add((coord.x, meshFilter));
                    }
                    else
                    {
                        meshFilters.Add(coord.y, new List<(int x, MeshFilter filter)>() { (coord.x, meshFilter) });
                    }
                }
            }

            return meshFilters;
        }

        private List<List<MeshFilter>> GroupByContinuousX(List<(int x, MeshFilter filter)> filters)
        {
            var groups = new List<List<MeshFilter>>();

            if (filters.Count == 0)
                return groups;

            // x座標でソート
            var sortedFilters = filters.OrderBy(f => f.x).ToList();

            // 最初の要素をグループに追加
            var currentGroup = new List<MeshFilter> { sortedFilters[0].filter };
            var previousX = sortedFilters[0].x;

            // 2番目以降の要素を処理
            for (int i = 1; i < sortedFilters.Count; i++)
            {
                var currentX = sortedFilters[i].x;

                // x座標が連続しているか確認
                if (currentX == previousX + 1)
                {
                    currentGroup.Add(sortedFilters[i].filter);
                }
                else
                {
                    // 連続していない場合は新しいグループを作成
                    groups.Add(currentGroup);
                    currentGroup = new List<MeshFilter> { sortedFilters[i].filter };
                }

                previousX = currentX;
            }

            // 最後のグループを追加
            groups.Add(currentGroup);

            return groups;
        }

        private List<List<MeshFilter>> GroupByContinuousY(List<List<MeshFilter>> filters)
        {
            var groupedFilters = new Dictionary<Vector2, List<MeshFilter>>();

            // グループ内の各ブロックのX座標の平均を求める
            foreach (List<MeshFilter> meshFilters in filters)
            {
                float x = meshFilters.Average(filter => filter.transform.position.x);
                float y = meshFilters.First().transform.position.y;

                groupedFilters.Add(new Vector2(x, y), meshFilters);
            }

            // y座標でグループ化されたフィルターをソート
            var sortedFilters = groupedFilters.OrderBy(kvp => kvp.Key.y).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var result = new List<List<MeshFilter>>();

            int checkedCount = 0;

            while (checkedCount < groupedFilters.Count)
            {
                var (ave, meshFilters) = sortedFilters.First();
                checkedCount++;

                bool isContinuous = true;
                Vector2 targetAve = ave;
                int blockCount = meshFilters.Count;

                do
                {
                    // y座標を1ずつ増やして同じ種類のグループを探す
                    targetAve.y += 1;

                    bool isTargetAve = sortedFilters.TryGetValue(targetAve, out var targetFilters);
                    isContinuous = isTargetAve && targetFilters.Count == blockCount;

                    if (isContinuous)
                    {
                        meshFilters.AddRange(targetFilters);
                        sortedFilters.Remove(targetAve);
                        checkedCount++;
                    }
                }
                while (isContinuous);

                result.Add(meshFilters);
                sortedFilters.Remove(ave);
            }

            return result;
        }
    }
}