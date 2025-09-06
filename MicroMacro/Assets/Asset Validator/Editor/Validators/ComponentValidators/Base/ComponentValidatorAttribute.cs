//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;

/**
 * ComponentValidatorAttribute is a decorator meant for usage on ComponentValidatorBase
 * derived implementations to uniquely identify them and a target type that it will validate.
 **/
namespace AssetValidator {

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ComponentValidatorAttribute : Attribute {
         // The target type that a validator will track.
        public Type TargetType { get; }

        public ComponentValidatorAttribute(Type targetType) {
            TargetType = targetType;
        }
    }
}
