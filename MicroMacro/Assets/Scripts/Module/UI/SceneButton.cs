using UnityEngine;
using UnityEngine.UI;

namespace Module.UI
{
    public class SceneButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private string sceneName;
        
        public string SceneName => sceneName;
        public Button Button => button;
    }
}