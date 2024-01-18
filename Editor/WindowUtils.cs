using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    internal static class WindowUtils
    {
        public const string MenuRoot = "Tools/Blockade Labs";

        private const string _packageJsonGuid = "fb5dc3ed6cb9cdf469562dd9cddee9b9";
        private const string _logoGuid = "7a7ad95e1e1c4ee488d47d37aa36e95a";
        private const string _fontGuid = "1b5ca89b842763248980ff468bb292ea";

        private static string _version;

        public static void DrawLogo()
        {
            BlockadeGUI.Image(_logoGuid, 165, 42);
        }

        public static string GetVersion()
        {
            if (_version == null)
            {
                var projectFilePath = AssetDatabase.GUIDToAssetPath(_packageJsonGuid);
                var lines = File.ReadAllText(projectFilePath);
                var rx = new Regex("\"version\": \"(.*?)\"");
                _version = rx.Match(lines).Groups[1].Value;
            }

            return _version;
        }

        public static void CenterOnEditor(EditorWindow window)
        {
#if UNITY_2020_1_OR_NEWER
            var main = EditorGUIUtility.GetMainWindowPosition();
            var pos = window.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            window.position = pos;
#endif
        }

        public static Font GetFont()
        {
            return AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(_fontGuid));
        }
    }
}