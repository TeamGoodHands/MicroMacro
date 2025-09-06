//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * ActiveSceneValidatorManager inspects components in a single scene whose type is contained in the 
 * passed ClassTypeCache instance and outputs any logs for validation to the passed LogCache instance.
 **/
namespace AssetValidator {

    internal sealed class ActiveSceneValidatorManager : InstanceValidatorManagerBase {
        public ActiveSceneValidatorManager(LogCache logCache)
            : base(logCache) {
        }

        public override void Search() {
            objectsToValidate.Clear();
            var objs = Object.FindObjectsOfType<GameObject>();
            foreach (var obj in objs) {
                objectsToValidate.Add(obj);
                var components = obj.GetComponents<MonoBehaviour>();
                foreach (var c in components) {
                    if (c != null)
                        objectsToValidate.Add(c);
                }
            }
        }

        protected override void OnLogCreated(ValidationLog validationLog) {
            validationLog.scenePath = SceneManager.GetActiveScene().path;
            validationLog.source = LogSource.Scene;
            base.OnLogCreated(validationLog);
        }
    }
}
