//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEditor.IMGUI.Controls;
using UnityEngine;

/**
 * ValidationLogTreeViewHeader represents a group of related ValidationLogTreeViewItem
 * entries and indicates the counts of informational, warning, and error logs present.
 **/
namespace AssetValidator {
    
    internal sealed class ValidationLogTreeViewHeader : TreeViewItem {
        const int MaxErrorCount = 999;

        public int ErrorCount { get; set; }

        // Empty-constructor to allow access to the base TreeViewItem constructor equivalent.
        public ValidationLogTreeViewHeader(int id, int depth, string displayName)
            : base(id, depth, displayName) {
        }

        // Sets the number of error, warning, and informational log entries visible on this header.
        public void SetLogCounts(int errors) {
            ErrorCount = Mathf.Clamp(errors, 0, MaxErrorCount);
        }
    }
}
