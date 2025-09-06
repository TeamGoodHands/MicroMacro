//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;
using Object = UnityEngine.Object;

/**
 * AbstractInstanceValidator validates Object instances of a specific Type
 **/
namespace AssetValidator {

    public abstract class AbstractInstanceValidator {
        internal event Action<ValidationLog> LogCreated;

        public string TypeName {
            get {
                if (string.IsNullOrEmpty(typeName))
                    typeName = GetType().Name;
                return typeName;
            }
        }
        string typeName;

        public Type TypeToTrack => typeToTrack;
        protected Type typeToTrack;

        // Validates an Object
        public abstract bool Validate(Object obj);

        // Returns true if this validator applies to Object, otherwise returns false.
        public abstract bool AppliesTo(Object obj);

        // Dispatches a ValidationLog instance via LogCreated.
        protected void LogError(Object obj, string message) {
            string objName = obj == null ? string.Empty : "[" + obj.name + "] ";
            LogCreated?.Invoke(new ValidationLog {
                validatorName = TypeName,
                message = objName + message,
                objectPath = ObjectTools.GetObjectPath(obj)
            });
        }
    }
}
