//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Text;
using UnityEditor;
using UnityEngine;

/**
 * Utility functions for drawing the scene validator window
 **/
namespace AssetValidator {

    internal static class GraphicsTools {
        public static Texture2D ErrorIconSmall {
            get {
                if (errorIconSmall == null)
                    errorIconSmall = (Texture2D)EditorGUIUtility.Load("icons/console.erroricon.sml.png");
                return errorIconSmall;
            }
        }
        static Texture2D errorIconSmall;

        static readonly StringBuilder stringBuilder = new();

        public static GUIStyle GetLogHeaderStyle() {
            return EditorGUIUtility.isProSkin ? EditorStyles.boldLabel : EditorStyles.whiteBoldLabel;
        }

        // Returns a multi-line string created from message where
        // each line in the string is limited to a number of characters equal or less than charsPerLine
        public static string GetMultilineString(string message, int charsPerLine) {
            stringBuilder.Clear();
            for (int i = 0; i < message.Length; i += charsPerLine) {
                int strLength = charsPerLine;
                if (i + strLength > message.Length)
                    strLength = message.Length - i;

                stringBuilder.AppendLine(message.Substring(i, strLength));
            }
            return stringBuilder.ToString();
        }

        // Returns the number of characters that can be made visible in a rect
        // based on its width and the length of message.
        public static int GetCharactersPerRow(Rect rect, string message) {
            float messageChunkSize = 1f;
            Vector2 messageWidth = GUI.skin.label.CalcSize(new GUIContent(message));
            if (messageWidth.x > rect.width)
                messageChunkSize = rect.width / messageWidth.x;

            return Mathf.CeilToInt(messageChunkSize * message.Length);
        }
    }
}
