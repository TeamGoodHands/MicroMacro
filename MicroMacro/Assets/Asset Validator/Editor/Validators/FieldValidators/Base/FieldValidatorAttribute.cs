//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;
using UnityEngine.Assertions;

/**
 * FieldValidatorAttribute is used by derived classes of FieldValidatorBase to determine
 * whether or not it can validate a particular Object instance type.
 **/
namespace AssetValidator {
    
    [AttributeUsage(AttributeTargets.Class)]
    public class FieldValidatorAttribute : Attribute {
        // The target FieldAttribute-derived type that a validator will track.
        public Type TargetType { get; }

        public FieldValidatorAttribute(Type targetType) {
            Assert.IsTrue(targetType.IsSubclassOf(typeof(FieldAttribute)), 
                "FieldValidatorAttribute must target an attribute deriving from FieldValidatorAttribute");
            TargetType = targetType;
        }
    }
}
