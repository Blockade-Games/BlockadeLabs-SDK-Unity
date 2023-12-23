using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    internal static class BlockadeGUI
    {
        private static Dictionary<string, List<Texture2D>> _bgTextures = new Dictionary<string, List<Texture2D>>();

        internal static void Vertical(Action action)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            action();
            EditorGUILayout.EndVertical();
        }

        internal static void Vertical(float width, Action action)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(width));
            action();
            EditorGUILayout.EndVertical();
        }

        internal static void Vertical(GUIStyle style, Action action)
        {
            EditorGUILayout.BeginVertical(style, GUILayout.ExpandWidth(true));
            action();
            EditorGUILayout.EndVertical();
        }

        internal static void Horizontal(Action action)
        {
            EditorGUILayout.BeginHorizontal();
            action();
            EditorGUILayout.EndHorizontal();
        }

        internal static void Horizontal(GUIStyle style, Action action)
        {
            EditorGUILayout.BeginHorizontal(style);
            action();
            EditorGUILayout.EndHorizontal();
        }

        internal static Vector2 Scroll(Vector2 scrollPosition, Action action)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));
            action();
            EditorGUILayout.EndScrollView();
            return scrollPosition;
        }

        internal static Vector2 Scroll(Vector2 scrollPosition, GUIStyle style, Action action)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, style, GUILayout.ExpandWidth(true));
            action();
            EditorGUILayout.EndScrollView();
            return scrollPosition;
        }

        internal static void DisableGroup(bool disable, Action action)
        {
            EditorGUI.BeginDisabledGroup(disable);
            action();
            EditorGUI.EndDisabledGroup();
        }

        private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        internal static bool ImageButton(string guid, int width, int height)
        {
            if (!_textures.TryGetValue(guid, out var texture))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                _textures[guid] = texture;
            }

            return GUILayout.Button(texture, GUILayout.Width(width), GUILayout.Height(height));
        }

        public static void Image(string guid, int width, int height)
        {
            if (!_textures.TryGetValue(guid, out var texture))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                _textures[guid] = texture;
            }

            GUILayout.Label(texture, GUILayout.Width(width), GUILayout.Height(height));
        }

        public static bool Checkbox(bool value, Action action)
        {
            Horizontal(() =>
            {
                GUILayout.Space(10);
                value = EditorGUILayout.Toggle(value, GUILayout.Width(20));
                action();
            });

            return value;
        }

        public static bool Checkbox(bool value, string label, GUIStyle labelStyle)
        {
            return Checkbox(value, () => GUILayout.Label(label, labelStyle));
        }

        public static void Link(string label, string url, GUIStyle labelStyle)
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent(label), labelStyle);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (GUI.Button(rect, label, labelStyle))
            {
                Application.OpenURL(url);
            }
        }

        public static int StyleFontSize;
        public static string StyleTag;

        public static GUIStyle CreateStyle()
        {
            var style = new GUIStyle();
            style.wordWrap = true;
            style.richText = true;
            style.fontSize = StyleFontSize;
            return style;
        }

        public static GUIStyle CreateStyle(Color textColor)
        {
            var style = CreateStyle();
            SetTextColor(style, textColor);
            return style;
        }

        public static GUIStyle CreateStyle(Color textColor, Color backgroundColor)
        {
            var style = CreateStyle(textColor);
            SetBackgroundColor(style, backgroundColor);
            return style;
        }

        public static void SetBackgroundColor(GUIStyle style, Color color)
        {
            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, color);
            bgTex.Apply();

            style.normal.background =
                style.active.background =
                style.focused.background =
                style.hover.background = bgTex;

            bgTex.hideFlags = HideFlags.DontSave;

            if (!_bgTextures.TryGetValue(StyleTag, out var textures))
            {
                textures = new List<Texture2D>();
                _bgTextures[StyleTag] = textures;
            }

            textures.Add(bgTex);
        }

        public static void CleanupBackgroundTextures(string styleTag)
        {
            if (_bgTextures.TryGetValue(styleTag, out var textures))
            {
                foreach (var texture in textures)
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }

                _bgTextures.Remove(styleTag);
            }
        }

        public static void SetTextColor(GUIStyle style, Color color)
        {
            style.normal.textColor =
                style.active.textColor =
                style.focused.textColor =
                style.hover.textColor = color;
        }

        public static Color Gray(int gray)
        {
            return new Color(gray / 255f, gray / 255f, gray / 255f);
        }

        public static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}