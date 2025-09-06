//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;
using AssetValidator;

/**
 * Fields decorated with ResourcePathAttribute indicates that the object.ToString
 * method return value resolves to a path to an asset in a Resources folder.
 **/
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ResourcePathAttribute : FieldAttribute {
}
