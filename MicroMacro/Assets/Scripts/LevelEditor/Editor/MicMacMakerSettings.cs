using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Editor.LevelEditor
{
    [CreateAssetMenu(fileName = "MicMacMakerSettings", menuName = "MicMacMaker/MicMacMakerSettings", order = 1)]
    public class MicMacMakerSettings : ScriptableObject
    {
        public ObjectCategory[] ObjectCategories => objectCategories;

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