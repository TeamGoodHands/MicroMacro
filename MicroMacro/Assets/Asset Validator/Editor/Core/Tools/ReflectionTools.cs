//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/**
 * Helper methods for introspecting code via reflection.
 **/
namespace AssetValidator {

    public static class ReflectionTools {
        const string AssetValidatorNamespace = "AssetValidator";
        static readonly string[] SystemAssemblyPrefixes = new string[] { 
            "Unity", "System", "mscorlib", "Bee", "nunit", "PlayerBuildProgramLibrary", 
            "ExCSS", "Mono", "netstandard" 
        };

        // Returns an IEnumerable of non-abstract, non-generic class instances of Types derived from T.
        public static IEnumerable<T> GetAllDerivedInstancesOfType<T>() where T : class {

            var objects = new List<T>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                // Skip Unity and system assemblies
                if (IsInternalAssembly(assembly))
                    continue;

                var assemblyTypes = assembly.GetTypes().Where(
                        myType => myType.IsClass && !myType.IsAbstract && !myType.IsGenericType && myType.IsSubclassOf(typeof(T)));
                foreach (var type in assemblyTypes) {
                    // Check if AssetValidator base validator is over-ridden                  
                    bool isOverridden = false;
                    foreach (T obj in objects) {
                        if (obj.GetType().Name == type.Name) {
                            if (obj.GetType().FullName.StartsWith(AssetValidatorNamespace))
                                objects.Remove(obj);
                            else
                                isOverridden = true;
                            break;
                        }
                    }
                    if (!isOverridden)
                        objects.Add((T)Activator.CreateInstance(type));
                }
            }
            return objects;
        }

        public static bool IsInternalAssembly(Assembly assembly) {
            string assemblyName = assembly.FullName;
            foreach (string prefix in SystemAssemblyPrefixes) {
                if (assemblyName.StartsWith(prefix))
                    return true;
            }
            return false;
        }
    }
}
