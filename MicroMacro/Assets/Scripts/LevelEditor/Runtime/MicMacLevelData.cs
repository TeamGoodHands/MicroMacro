using System;
using System.Collections.Generic;
using CoreModule.Serialization;
using UnityEngine;

namespace LevelEditor.Runtime
{
    public class MicMacLevelData : MonoBehaviour
    {
        private const int MapSize = 8192;
        [SerializeField, HideInInspector] private SerializableDictionary<long, ConnectionPair> mapData;
        public Dictionary<long, ConnectionPair> MapData => mapData.ToDictionary();

        public long CoordToIndex(Vector2Int coord)
        {
            return coord.y * MapSize + coord.x;
        }
    }

    [Serializable]
    public struct ConnectionPair
    {
        public GameObject Object;
        public long[] Connections;

        public ConnectionPair(GameObject obj, long[] connections)
        {
            Object = obj;
            Connections = connections;
        }
    }
}