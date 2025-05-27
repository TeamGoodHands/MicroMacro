using System;
using UnityEngine;

namespace Editor.LevelEditor
{
    [CreateAssetMenu(fileName = "MicMacMakerSettings", menuName = "MicMacMakerSettings", order = 1)]
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
            public GameObject[] Prefabs => prefabs;

            [SerializeField] private string name;
            [SerializeField] private GameObject[] prefabs;
        }
    }
}