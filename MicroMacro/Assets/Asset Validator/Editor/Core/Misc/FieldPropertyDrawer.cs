//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/**
 * Renders the attribute field property drawers in red if validation failed.
 **/
namespace AssetValidator {
    
    [CustomPropertyDrawer(typeof(FieldAttribute), true)]
    public class FieldPropertyDrawer : PropertyDrawer {
        static readonly Dictionary<string, bool> isValidDic = new();
        bool isInitialized = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var originalColor = GUI.color;
            string path = property.propertyPath;
            GUI.color = (!isValidDic.ContainsKey(path) || isValidDic[path]) ? originalColor : Color.red;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label, true);
            if (EditorGUI.EndChangeCheck() || !isInitialized)
                isValidDic[path] = Validate(property);
            GUI.color = originalColor;
        }

        bool Validate(SerializedProperty property) {
            isInitialized = true;
            var fieldValidators = ReflectionTools.GetAllDerivedInstancesOfType<FieldValidatorBase>();
            foreach (var validator in fieldValidators) {
                if (attribute.GetType() == validator.TypeToTrack) {
                    object item;
                    if (property.type == "string")
                        item = property.stringValue;
                    else
                        item = property.objectReferenceValue;
                    if (!validator.ValidateItem(item))
                        return false;
                }
            }
            return true;
        }
    }
}
