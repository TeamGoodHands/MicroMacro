//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

/**
 * A FieldValidatorBase is the abstract base class for any validator that is inspecting
 * fields on an Object type instance.
 **/
namespace AssetValidator {

    public abstract class FieldValidatorBase : AbstractInstanceValidator {
        // The binding flags for fields that can be validated on an instance. This includes both public,
        // private, and protected instance fields.
        const BindingFlags DefaultFieldFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        // The base FieldValidatorAttribute type.
        static readonly Type FieldTargetAttrType = typeof(FieldValidatorAttribute);

        readonly Dictionary<Type, bool> TypeHasTrackedAttributeCache = new();

        protected FieldValidatorBase() {
            var t = GetType();
            var vFieldTargets = (FieldValidatorAttribute[])t.GetCustomAttributes(FieldTargetAttrType, false);
            Assert.IsTrue(vFieldTargets.Length > 0,
                "Subclasses of BaseVFieldValidator are required to be decorated with a VFieldTargetAttribute to determine what VFieldAttribute they are validating");
            typeToTrack = vFieldTargets[0].TargetType;
            TypeHasTrackedAttributeCache.Clear();
        }

        // Returns true if any of Object's fields pertain to this validator, otherwise returns false.
        public sealed override bool AppliesTo(Object obj) {
            var fields = GetFieldInfosApplyTo(obj);
            return fields.Any();
        }

        // Returns all FieldInfo instances on object that pertain to this validator.
        public IEnumerable<KeyValuePair<string, object>> GetFieldInfosApplyTo(object obj) {
            return GetFieldInfosApplyTo(obj.GetType(), obj);
        }

        // Returns all FieldInfo instances on Type that pertain to this validator.
        public IEnumerable<KeyValuePair<string, object>> GetFieldInfosApplyTo(Type type, object obj) {
            var allFieldInfos = new Dictionary<string, object>();

            // Skip Unity or system assemblies
            if (ReflectionTools.IsInternalAssembly(type.Assembly))
                return allFieldInfos;

            // Check if type in cache and doesn't contain any tracked attributes
            if (TypeHasTrackedAttributeCache.TryGetValue(type, out bool val) && val == false)
                return allFieldInfos;

            var fieldInfos = type.GetFields(DefaultFieldFlags);
            foreach (var fieldInfo in fieldInfos) {
                if (TypeHasTrackedAttributeType(fieldInfo))
                    allFieldInfos[fieldInfo.Name] = fieldInfo.GetValue(obj);

                // Add serialized fields for field validation as well
                var fieldType = fieldInfo.FieldType;
                if (!fieldType.IsPrimitive && fieldType != typeof(string) && !fieldType.IsSubclassOf(typeof(Object))) {
                    var serializedProperty = fieldInfo.GetValue(obj);
                    if (serializedProperty == null)
                        continue;

                    // If serialized field is a list, iterate through its items
                    if (serializedProperty is IEnumerable<object>) {
                        var enumerable = serializedProperty as IEnumerable<object>;
                        int count = 0;
                        foreach (var item in enumerable) {
                            if (item == null)
                                continue;

                            var fieldInfos2 = item.GetType().GetFields(DefaultFieldFlags);
                            foreach (var fieldInfo2 in fieldInfos2) {
                                if (TypeHasTrackedAttributeType(fieldInfo2))
                                    allFieldInfos[fieldInfo2.Name + " " + count] = fieldInfo2.GetValue(item);
                            }
                            count++;
                        }
                    }
                    else {
                        var fieldInfos2 = fieldInfo.FieldType.GetFields(DefaultFieldFlags);
                        foreach (var fieldInfo2 in fieldInfos2) {
                            if (TypeHasTrackedAttributeType(fieldInfo2))
                                allFieldInfos[fieldInfo2.Name] = fieldInfo2.GetValue(serializedProperty);
                        }
                    }
                }
            }

            // Cache type
            if (!TypeHasTrackedAttributeCache.ContainsKey(type))
                TypeHasTrackedAttributeCache.Add(type, allFieldInfos.Count > 0);

            return allFieldInfos;
        }

        // Returns true if a given FieldInfo is tracked by this validator due to it being 
        // decorated with the relevant FieldAttribute type.
        bool TypeHasTrackedAttributeType(FieldInfo fieldInfo) {
            return fieldInfo.GetCustomAttributes(typeToTrack, false).Length > 0;
        }

        // General validator for field validators which works on lists
        public override bool Validate(Object obj) {
            bool isValid = true;
            var fields = GetFieldInfosApplyTo(obj);
            foreach (var field in fields) {
                string fieldName = field.Key;
                var value = field.Value;
                if (value is IEnumerable<object>) {
                    var enumerable = value as IEnumerable<object>;
                    int count = 0;
                    foreach (var item in enumerable) {
                        bool result = ValidateItem(item);
                        if (!result)
                            LogFieldError(obj, fieldName);
                        isValid &= result;
                        count++;
                    }
                }
                else {
                    bool result = ValidateItem(value);
                    if (!result)
                        LogFieldError(obj, fieldName);
                    isValid &= result;
                }
            }
            return isValid;
        }

        // Override this method to implement custom field validators
        public abstract bool ValidateItem(object obj);

        // Override this method to implement custom error messages
        protected virtual void LogFieldError(Object obj, string fieldName) {
            LogError(obj, "has an invalid field [" + fieldName + "].");
        }
    }
}
