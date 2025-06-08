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
        [SerializeField, HideInInspector] private SerializableDictionary<long, CellData> mapData;
        public Dictionary<long, CellData> MapData => mapData.ToDictionary();

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
            // Y座標でグリッドデータを分割する
            var splitedFilters = SplitByY();

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

        private Dictionary<int, List<(int x, MeshFilter filter)>> SplitByY()
        {
            const string colliderObjectKey = "Collider";
            var meshFilters = new Dictionary<int, List<(int x, MeshFilter filter)>>();

            foreach (var data in MapData)
            {
                // 各セルのオブジェクトからコライダーオブジェクトを取得
                Transform colliderObj = data.Value.Object.transform.Find(colliderObjectKey);

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

    [Serializable]
    public struct CellData
    {
        public GameObject Object;
        public long[] Connections;

        public CellData(GameObject obj, long[] connections)
        {
            Object = obj;
            Connections = connections;
        }
    }
}