//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Collections.Generic;
using UnityEngine;

/**
 * GameObject component validator to check for missing references and duplicate components.
 * Over-ride this class setting ExcludedComponents if needed.
 **/
namespace AssetValidator {
    
    [ComponentValidator(typeof(GameObject))]
    public class GameObjectValidator : ComponentValidatorBase {
        // Over-ride this in a derived subclass to exclude certain components
        protected List<System.Type> ExcludedComponentsFromDuplicateCheck = new() {
        };

        public override bool Validate(Object obj) {
            var go = obj as GameObject;
            return CheckGameObject(go, go.transform);
        }

        bool CheckGameObject(GameObject obj, Transform currentTransform) {
            bool valid = true;
            var componentCount = new Dictionary<System.Type, int>();
            var components = currentTransform.GetComponents<MonoBehaviour>();
            foreach (var c in components) {
                if (!c) {
                    LogError(obj, "has a missing component.");
                    continue;
                }

                var componentType = c.GetType();
                if (IsExcludedComponent(componentType) || !c.enabled)
                    continue;

                // Count components
                if (!componentCount.ContainsKey(componentType))
                    componentCount.Add(componentType, 1);
                else {
                    componentCount[componentType]++;
                    if (componentCount[componentType] > 1) {
                        string childStr = obj == currentTransform.gameObject ? string.Empty : " on [" + currentTransform.name + "]";
                        LogError(obj, "has a duplicate component [" + componentType + "]" + childStr + ".");
                        return false;
                    }
                }
            }

            // Recurse children
            foreach (Transform child in currentTransform)
                valid &= CheckGameObject(obj, child);

            return valid;
        }

        bool IsExcludedComponent(System.Type componentType) {
            foreach (var excludedComponent in ExcludedComponentsFromDuplicateCheck) {
                if (componentType == excludedComponent)
                    return true;
            }
            return false;
        }
    }
}
