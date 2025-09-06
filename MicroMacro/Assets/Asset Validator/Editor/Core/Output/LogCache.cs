//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Collections;
using System.Collections.Generic;

/**
 * LogCache represents a cache of logs from a validation session.
 **/
namespace AssetValidator {

    internal sealed class LogCache : IReadOnlyList<ValidationLog> {
        readonly IList<ValidationLog> logs;

        public LogCache() {
            logs = new List<ValidationLog>();
        }

        // Adds a ValidationLog to the cache.
        internal void OnLogCreated(ValidationLog log) {
            logs.Add(log);
        }

        // Clears all ValidationLogs from the cache.
        internal void ClearLogs() {
            logs.Clear();
        }

        // Returns true if there are any ValidationLogs, otherwise false.
        internal bool HasLogs() {
            return logs.Count > 0;
        }

        // The total number of validation logs in the cache.
        public int Count => logs.Count;

        // Returns a ValidationLog present at position <paramref name="index"/> in the cache.
        public ValidationLog this[int index] {
            get { return logs[index]; }
        }

        public IEnumerator<ValidationLog> GetEnumerator() {
            return logs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
