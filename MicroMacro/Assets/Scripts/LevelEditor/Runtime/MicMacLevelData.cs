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
            return coord.y * MapSize + coord.x;
        }

        private MeshCombiner meshCombiner = new MeshCombiner();

        private void OnValidate()
        {
            Debug.Log(MapData.Values.First().Object);
        }

        private void Start()
        {
            var meshFilters = new List<MeshFilter>();
            const string colliderObjectKey = "Collider";

            Debug.Log(MapData.Values.First().Object);

            foreach (Transform rootTransform in MapData.Values.Select(cellData => cellData.Object.transform))
            {
                Transform colliderObj = rootTransform.Find(colliderObjectKey);
                if (colliderObj != null && colliderObj.TryGetComponent(out MeshFilter meshFilter))
                {
                    meshFilters.Add(meshFilter);
                }
            }

            GameObject combinedObject = meshCombiner.CombineMeshes(meshFilters);
            combinedObject.transform.SetParent(transform, false);
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
            Debug.Log(obj.GetInstanceID());
            Connections = connections;
        }
    }
}