using UnityEngine;

namespace CoreModule.Attribute
{
    [System.Serializable]
    public class SceneField
    {
        [SerializeField] private string sceneName;

        public string SceneName => sceneName;

        public static implicit operator string(SceneField sceneField)
        {
            return sceneField.SceneName;
        }
    }
}