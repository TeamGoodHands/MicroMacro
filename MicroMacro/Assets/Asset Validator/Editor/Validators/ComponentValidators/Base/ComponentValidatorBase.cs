//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEngine;
using UnityEngine.Assertions;

/**
 * ComponentValidatorBase is a abstract validator class intended for validating an object
 * instance deriving from either MonoBehaviour or ScriptableObject.
 **/
namespace AssetValidator {

    public abstract class ComponentValidatorBase : AbstractInstanceValidator {
        const string HasComponentValidatorWarning =
            "Subclasses of ComponentValidatorBase are required to be decorated with a ComponentValidatorAttribute to " +
            "determine what type they are validating";

        protected ComponentValidatorBase() {
            var vObjectTargets = (ComponentValidatorAttribute[])GetType().
                GetCustomAttributes(typeof(ComponentValidatorAttribute), true);
            Assert.IsFalse(vObjectTargets.Length == 0, HasComponentValidatorWarning);
            typeToTrack = vObjectTargets[0].TargetType;
        }

        public sealed override bool AppliesTo(Object obj) {
            return typeToTrack.IsAssignableFrom(obj.GetType());
        }
    }
}
