using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.SceneManagement;

namespace BlockadeLabsSDK.Editor
{
    [InitializeOnLoad]
    internal class StartScreen : EditorWindow
    {
        private const string _website = "https://www.blockadelabs.com/";
        private const string _discord = "https://discord.gg/kqKB3X4TJz";
        private const string _github = "https://github.com/Blockade-Games/BlockadeLabs-SDK-Unity";

        private const string _showOnStartKey = "BlockadeLabsSDK_StartMenu_ShowOnStart";
        private const string _versionKey = "BlockadeLabsSDK_StartMenu_Version";
        private const string _shownKey = "BlockadeLabsSDK_StartMenu_Shown";

        private const string _menuRoot = "Tools/Blockade Labs";
        private const string _styleTag = "BlockadeLabsSDK_StartScreen";

        private const string _packageJsonGuid = "fb5dc3ed6cb9cdf469562dd9cddee9b9";
        private const string _logoGuid = "608219909903ac246ac3a12aae8675f0";
        private const string _skyboxSceneGuid = "d9b6ab5207db7f8438e56b4c66ea03aa";
        private const string _fontGuid = "1b5ca89b842763248980ff468bb292ea";
        private const string _changelogGuid = "0519ee665fde4ef0bb74e40b3fffff42";

        private GUIStyle _buttonStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _warningStyle;
        private GUIStyle _versionStyle;
        private GUIStyle _boldStyle;
        private GUIStyle _bgStyle;
        private GUIStyle _paddedSection;
        private GUIStyle _footerStyle;

        private static ShowOnStart _showOnStart;

        private static string _version;

        internal static readonly string WindowTitle = "Blockade Labs Skybox AI";
        private static readonly string[] _showOnStartOptions = {
            "Always", "On Update", "Never"
        };

        private Vector2 _scrollPosition;

        private List<string> _changelog = new List<string>();

        private enum ShowOnStart
        {
            Always,
            OnUpdate,
            Never
        }

        static StartScreen()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            Initialize();
        }

        internal static void Initialize()
        {
            bool alreadyShown = SessionState.GetBool(_shownKey, false);
            SessionState.SetBool(_shownKey, true);

            var version = GetVersion();
            var lastVersion = EditorPrefs.GetString(_versionKey, "0.0.0");
            var newVersion = version.CompareTo(lastVersion) > 0;
            if (newVersion)
            {
                EditorPrefs.SetString(_versionKey, version);
                alreadyShown = false;
            }

            _showOnStart = (ShowOnStart)EditorPrefs.GetInt(_showOnStartKey, 0);
            bool showPref = _showOnStart == ShowOnStart.Always ||
                (_showOnStart == ShowOnStart.OnUpdate && newVersion);
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !alreadyShown && showPref)
            {
                ShowStartScreen();
            }
        }

        void OnEnable()
        {
            _bodyStyle = null;
        }

        void OnDisable()
        {
            BlockadeGUI.CleanupBackgroundTextures(_styleTag);
        }

        [MenuItem(_menuRoot + "/Open Skybox AI Scene", false, 0)]
        public static void OpenSkyboxAIScene()
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(_skyboxSceneGuid);
            EditorSceneManager.OpenScene(scenePath);
        }

        [MenuItem(_menuRoot + "/Start Screen", false, 0)]
        public static void ShowStartScreen()
        {
            StartScreen window = GetWindow<StartScreen>(true, WindowTitle, true);
            window.minSize = new Vector2(800, 600);
            window.maxSize = window.minSize;
            window.Show();
        }

        [MenuItem(_menuRoot + "/Support (Discord)", false, 3)]
        public static void OpenSupport()
        {
            Application.OpenURL(_discord);
        }

        private Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        private void InitStyles()
        {
            if (_bodyStyle != null) return;

            BlockadeGUI.StyleFontSize = 14;
            BlockadeGUI.StyleTag = _styleTag;
            BlockadeGUI.StyleFont = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(_fontGuid));

            _bodyStyle = BlockadeGUI.CreateStyle(Color.white);
            _bodyStyle.margin.left = 10;
            _bodyStyle.margin.top = 10;
            _bodyStyle.stretchWidth = false;

            _boldStyle = new GUIStyle(_bodyStyle);
            _boldStyle.fontStyle = FontStyle.Bold;
            _boldStyle.fontSize++;

            _warningStyle = new GUIStyle(_bodyStyle);
            _warningStyle.normal.textColor = Color.yellow;

            _buttonStyle = BlockadeGUI.CreateStyle(Color.black, HexColor("#02ee8b"));
            _buttonStyle.fontSize = 14;
            _buttonStyle.margin.left = 10;
            _buttonStyle.margin.bottom = 5;
            _buttonStyle.padding = new RectOffset(10, 10, 5, 5);
            _buttonStyle.stretchWidth = false;
            _buttonStyle.alignment = TextAnchor.MiddleLeft;

            _versionStyle = new GUIStyle(_bodyStyle);
            _versionStyle.padding.right = 10;

            _bgStyle = BlockadeGUI.CreateStyle(Color.white, Color.black);

            _paddedSection = new GUIStyle();
            _paddedSection.padding = new RectOffset(10, 10, 10, 10);

            _footerStyle = BlockadeGUI.CreateStyle(new Color(0.8f, 0.8f, 0.8f));
            _footerStyle.fontSize = 12;
            _footerStyle.padding.top = 3;

            CenterOnEditor(this);
            ReadChangeLog();
        }

        private void LinkButton(string label, string url, GUIStyle style = null, int width = 170)
        {
            if (style == null) style = _buttonStyle;
            var labelContent = new GUIContent(label);
            var position = GUILayoutUtility.GetRect(width, 35, style);
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            if (GUI.Button(position, labelContent, style))
            {
                Application.OpenURL(url);
            }
        }

        private bool Button(string label, GUIStyle style = null, int width = 170)
        {
            if (style == null) style = _buttonStyle;
            var labelContent = new GUIContent(label);
            var position = GUILayoutUtility.GetRect(width, 35, style);
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, labelContent, style);
        }

        private void Bullet(string text)
        {
            BlockadeGUI.Horizontal(() =>
            {
                var ws = 1 + text.IndexOf('-');
                for (int i = 0; i < ws; i++)
                {
                    GUILayout.Space(10);
                }
                GUILayout.Label("•", _bodyStyle);

                GUILayout.Label(text.Substring(ws + 1), _bodyStyle);
            });
        }

        private void ReadChangeLog()
        {
            _changelog.Clear();
            var changelogPath = AssetDatabase.GUIDToAssetPath(_changelogGuid);
            var changelogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(changelogPath);
            _changelog = changelogAsset.text.Split('\n')
                .Select(x => Regex.Replace(x.TrimEnd(), @"\*\*(.*?)\*\*", "<b>$1</b>"))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var start = _changelog.FindIndex(l => l.StartsWith("## "));
            // var end = _changelog.FindIndex(start + 1, l => l.StartsWith("## "));
            // if (end < 0) end = _changelog.Count;
            _changelog = _changelog.GetRange(start, _changelog.Count - start);
        }

        private void WhatsNew()
        {
            EditorGUILayout.Space();
            if (Button("Open Skybox AI Scene", _buttonStyle, 550))
            {
                OpenSkyboxAIScene();
                Close();
            }
            EditorGUILayout.Space();

            for (int i = 0; i < _changelog.Count; i++)
            {
                var line = _changelog[i];
                if (line.StartsWith("###"))
                {
                    EditorGUILayout.Space();
                    GUILayout.Label(line.Substring(4), _boldStyle);
                    EditorGUILayout.Space();
                }
                else if (line.StartsWith("##"))
                {
                    EditorGUILayout.Space();
                    GUILayout.Label(line.Substring(3), _boldStyle);
                    EditorGUILayout.Space();
                }
                else
                {
                    Bullet(line);
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.Space();
        }

        private void OnGUI()
        {
            InitStyles();

            BlockadeGUI.Vertical(_bgStyle, () =>
            {
                BlockadeGUI.Horizontal(_paddedSection, () =>
                {
                    BlockadeGUI.Image(_logoGuid, 120, 36);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Version: " + GetVersion(), _versionStyle, GUILayout.ExpandHeight(true));
                });

                BlockadeGUI.Horizontal(() =>
                {
                    BlockadeGUI.Vertical(180, () =>
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("Resources", _boldStyle);
                        GUILayout.Space(10);
                        LinkButton("Website", _website);
                        LinkButton("Discord Invite", _discord);
                        LinkButton("Docs (Github)", _github);
                        GUILayout.FlexibleSpace();
                    });

                    BlockadeGUI.Vertical(() =>
                    {
                        _scrollPosition = BlockadeGUI.Scroll(_scrollPosition, () =>
                        {
                            GUILayout.Label("Thank you for using Blockade Labs Skybox AI!", _boldStyle, GUILayout.ExpandWidth(true));

                            EditorGUILayout.Space();

                            GUILayout.Label("You're invited to join the Discord community for support and feedback. Let us know how to make Skybox AI better for you!", _bodyStyle);

                            EditorGUILayout.Space();

                            WhatsNew();
                        });
                    });

                    EditorGUILayout.Space();
                });

                BlockadeGUI.Horizontal(_paddedSection, () =>
                {
                    GUILayout.Label($"{_menuRoot}/Start Screen", _footerStyle);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Show On Start: ", _footerStyle);
                    var newShowOnStart = (ShowOnStart)EditorGUILayout.Popup((int)_showOnStart, _showOnStartOptions);
                    if (_showOnStart != newShowOnStart)
                    {
                        _showOnStart = newShowOnStart;
                        EditorPrefs.SetInt(_showOnStartKey, (int)_showOnStart);
                    }
                });
            });
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
    }
}