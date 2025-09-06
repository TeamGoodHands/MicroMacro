//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

/**
 * Helper methods for creating and manipulating ValidationLogTreeView instances.
 **/
namespace AssetValidator {

    internal static class ValidationLogTreeViewTools {
        // Returns a root element TreeViewItem where the values are based on a collection of 
        // ValidationLogs in logs and grouped by validator type.
        public static TreeViewItem CreateTreeGroupedByValidatorType(IReadOnlyList<ValidationLog> logs) {
            int id = -1;
            var rootItem = new TreeViewItem(++id, -1);

            // Create a lookup of all vLogs grouped by validator name
            var dict = new Dictionary<string, List<ValidationLog>>();
            foreach (var vLog in logs) {
                if (!dict.ContainsKey(vLog.validatorName))
                    dict[vLog.validatorName] = new List<ValidationLog> { vLog };
                else
                    dict[vLog.validatorName].Add(vLog);
            }

            // From the lookup and root item, create a child object for each validator
            // type and for each validator type add all logs of that type as children.
            foreach (var kvp in dict) {
                var header = new ValidationLogTreeViewHeader(++id, 0, kvp.Key);
                var kLogs = kvp.Value;
                foreach (var kLog in kLogs)
                    header.AddChild(new ValidationLogTreeViewItem(kLog, ++id, 1));
                SetLogCounts(header, kLogs);
                rootItem.AddChild(header);
            }

            return rootItem;
        }

        static void SetLogCounts(ValidationLogTreeViewHeader header, IEnumerable<ValidationLog> logs) {
            int errorCount = 0;
            foreach (var x in logs)
                errorCount++;
            header.SetLogCounts(errorCount);
        }
    }
}
