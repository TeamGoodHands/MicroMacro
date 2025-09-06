//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEngine;

/**
 * RequiredFieldValidator is a validator meant to help ensure fields decorated with
 * RequiredAttribute have values assigned.
 **/
namespace AssetValidator {
    
    [FieldValidator(typeof(RequiredAttribute))]
    public sealed class RequiredFieldValidator : FieldValidatorBase {
        public override bool ValidateItem(object obj) {
            return !ObjectTools.IsNullReference(obj) || ObjectTools.IsNonEmptyString(obj);
        }

        protected override void LogFieldError(Object obj, string fieldName) {
            LogError(obj, "has a null assignment on field [" + fieldName + "].");
        }
    }
}
