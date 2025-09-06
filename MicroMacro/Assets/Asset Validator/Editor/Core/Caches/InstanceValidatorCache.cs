//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Collections.Generic;

/**
 * InstanceValidatorCache is a cache of AbstractInstanceValidator derived instances.
 **/
namespace AssetValidator {
    
    internal class InstanceValidatorCache {
        public int Count => validators.Count;

        readonly List<AbstractInstanceValidator> validators;

        public InstanceValidatorCache() {
            validators = new List<AbstractInstanceValidator>();

            // Get and add all field validators, excluding override disabled ones.
            var fieldValidators = ReflectionTools.GetAllDerivedInstancesOfType<FieldValidatorBase>();
            foreach (var fieldValidator in fieldValidators)
                validators.Add(fieldValidator);

            // Get and add all component validators, excluding override disabled ones.
            var componentValidators = ReflectionTools.GetAllDerivedInstancesOfType<ComponentValidatorBase>();
            foreach (var componentValidator in componentValidators)
                validators.Add(componentValidator);
        }

        // Returns the AbstractInstanceValidator derived instance at position
        public AbstractInstanceValidator this[int index] {
            get { return validators[index]; }
        }
    }
}
