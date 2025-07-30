using System;
using System.Collections.Generic;
using System.Linq;
using CoreModule.Serialization;
using UnityEngine;

namespace LevelEditor.Runtime
{
    public class MicMacLevelData : MonoBehaviour
    {
        private const int MapSize = 8192;
        [SerializeField, HideInInspector] private List<Vector2Int> overlapCoords = new List<Vector2Int>();

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
            Dictionary<long, GameObject> mapData = GetMapData();

            // Y座標でグリッドデータを分割する
            var splitedFilters = SplitByY(mapData);

            foreach (var filters in splitedFilters.Values)
            {
                // 座標が連続しているMeshFilterをグループ化する
                var continuousX = GroupByContinuousX(filters);

                // 各グループをメッシュとして結合する
                foreach (List<MeshFilter> group in continuousX)
                {
                    GameObject combinedObject = meshCombiner.CombineMeshes(group);
                    combinedObject.transform.SetParent(transform, false);
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Vector2Int coord in overlapCoords)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(new Vector3(coord.x, coord.y, -5), Vector3.one * 0.4f);
            }
        }

        private Dictionary<long, GameObject> GetMapData()
        {
            List<GameObject> gridObjects = transform.Cast<Transform>().Select(obj => obj.gameObject).Where(obj => obj.isStatic).ToList();
            CheckOverlap(gridObjects);

            if (overlapCoords.Count > 0)
            {
                Debug.LogWarning("重複して配置されているオブジェクトがあります！");
            }

            var mapData = new Dictionary<long, GameObject>();

            foreach (GameObject obj in gridObjects)
            {
                Vector2 position = obj.transform.position;
                mapData.Add(CoordToIndex(new Vector2Int((int)position.x, (int)position.y)), obj);
            }

            return mapData;
        }

        public List<Vector2Int> CheckOverlapNow()
        {
            return CheckOverlap(transform.Cast<Transform>().Select(obj => obj.gameObject).Where(obj => obj.isStatic).ToList());
        }

        private List<Vector2Int> CheckOverlap(List<GameObject> objects)
        {
            var checkedCoords = new HashSet<Vector2Int>();
            overlapCoords.Clear();

            foreach (GameObject obj in objects)
            {
                Vector2Int gridPos = new Vector2Int((int)obj.transform.position.x, (int)obj.transform.position.y);

                if (checkedCoords.Contains(gridPos))
                {
                    overlapCoords.Add(gridPos);
                }

                checkedCoords.Add(gridPos);
            }

            return overlapCoords;
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
    }
}