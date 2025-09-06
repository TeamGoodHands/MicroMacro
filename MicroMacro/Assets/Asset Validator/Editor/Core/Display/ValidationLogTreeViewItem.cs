//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEditor.IMGUI.Controls;

/**
 * ValidationLogTreeViewItem representing a single ValidationLog
 **/
namespace AssetValidator {

    internal sealed class ValidationLogTreeViewItem : TreeViewItem {
        // The ValidationLog associated with this ValidationLogTreeViewItem.
        public ValidationLog Log { get; }

        // Constructor that accepts a ValidationLog and sets an empty display name.
        public ValidationLogTreeViewItem(ValidationLog log, int id, int depth)
            : base(id, depth, string.Empty) {
            Log = log;
        }

        // Constructor that accepts a ValidationLog and a custom displayName.
        public ValidationLogTreeViewItem(ValidationLog log, int id, int depth, string displayName)
            : base(id, depth, displayName) {
            Log = log;
        }
    }
}
