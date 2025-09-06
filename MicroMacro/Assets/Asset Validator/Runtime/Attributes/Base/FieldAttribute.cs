//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;
using System.Diagnostics;
using UnityEngine;

/**
 * A FieldAttribute targets a public instance field and is intended to have a matching FieldAttribute
 * subclass that is designed to validate that field. It is only available in the editor.
 **/
namespace AssetValidator {
	
	[AttributeUsage(AttributeTargets.Field), Conditional("UNITY_EDITOR")]
	public abstract class FieldAttribute : PropertyAttribute {
	}
}
