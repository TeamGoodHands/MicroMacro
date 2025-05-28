using System;
using UnityEngine;

namespace Editor.LevelEditor
{
    [CreateAssetMenu(fileName = "LevelObjectDictionary", menuName = "LevelObjectDictionary")]
    public class LevelObjectCategories : ScriptableObject
    {
        public ObjectCategory[] ObjectCategories => objectCategories;
        [SerializeField] private ObjectCategory[] objectCategories;

        [Serializable]
        public class ObjectCategory
        {
            public string Name => name;
            public bool Snap => snap;
            public GameObject[] Prefabs => prefabs;

            [SerializeField] private string name;
            [SerializeField] private bool snap;
            [SerializeField] private GameObject[] prefabs;
        }
    }
}