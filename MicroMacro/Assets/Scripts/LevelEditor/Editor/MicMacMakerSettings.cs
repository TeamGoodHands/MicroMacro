using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Editor.LevelEditor
{
    [CreateAssetMenu(fileName = "MicMacMakerSettings", menuName = "MicMacMaker/MicMacMakerSettings", order = 1)]
    public class MicMacMakerSettings : ScriptableObject
    {
        public ObjectCategory[] ObjectCategories => objectCategories;
        public Vector2 SnapSize => snapSize;

        [SerializeField] private Vector2 snapSize;
        [SerializeField] private ObjectCategory[] objectCategories;

        [Serializable]
        public class ObjectCategory
        {
            public string Name => name;
            public PaletteItem[] Items => items;

            [SerializeField] private string name;
            [SerializeField] private PaletteItem[] items;
        }

        [Serializable]
        public struct PaletteItem
        {
            [SerializeField] private GameObject prefab;
            [SerializeField] private Vector2Int size;

            public GameObject Prefab => prefab;
            public GameObject Size => prefab;
        }
    }
}