using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Editor.LevelEditor
{
    [CreateAssetMenu(fileName = "MicMacMakerSettings", menuName = "MicMacMaker/MicMacMakerSettings", order = 1)]
    public class MicMacMakerSettings : ScriptableObject
    {
        [SerializeField] private ObjectCategory[] objectCategories;
        [SerializeField] private Material groundMaterial;
        
        public ObjectCategory[] ObjectCategories => objectCategories;
        public Material GroundMaterial => groundMaterial;

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