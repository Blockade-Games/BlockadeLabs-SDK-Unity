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
        private const string _apikeyUrl = "https://api.blockadelabs.com/";
        private const string _discord = "https://discord.gg/kqKB3X4TJz";
        private const string _github = "https://github.com/Blockade-Games/BlockadeLabs-SDK-Unity";

        private const string _versionKey = "BlockadeLabsSDK_StartMenu_Version";
        private const string _shownKey = "BlockadeLabsSDK_StartMenu_Shown";

        private const string _styleTag = "BlockadeLabsSDK_StartScreen";

        private const string _skyboxSceneGuid = "d9b6ab5207db7f8438e56b4c66ea03aa";
        private const string _changelogGuid = "0519ee665fde4ef0bb74e40b3fffff42";

        private const string _windowTitle = "Skybox AI by Blockade Labs";
        private static string[] _showStartScreenOptions;

        private GUIStyle _bodyStyle;
        private GUIStyle _boldStyle;
        private GUIStyle _linkStyle;
        private GUIStyle _navLinkStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _bgStyle;
        private GUIStyle _titleSection;
        private GUIStyle _showStartScreenStyle;
        private GUIStyle _showStartScreenPopupStyle;
        private GUIStyle _versionStyle;
        private GUIStyle _h1Style;
        private GUIStyle _h2Style;
        private GUIStyle _h3Style;
        private GUIStyle _h4Style;

        private static string _version;
        private Vector2 _scrollPosition;
        private List<string> _changelog = new List<string>();

        static StartScreen()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            Initialize();
        }

        private static void Initialize()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            bool alreadyShown = SessionState.GetBool(_shownKey, false);
            SessionState.SetBool(_shownKey, true);

            var version = WindowUtils.GetVersion();
            var lastVersion = EditorPrefs.GetString(_versionKey, "0.0.0");
            var newVersion = version.CompareTo(lastVersion) > 0;
            if (newVersion)
            {
                EditorPrefs.SetString(_versionKey, version);
                alreadyShown = false;
            }

            var showStartScreen = PluginSettings.Get().ShowStartScreen;
            bool showPref = showStartScreen == ShowStartScreen.Always ||
                (showStartScreen == ShowStartScreen.OnUpdate && newVersion);
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !alreadyShown && showPref)
            {
                OpenStartScreen();
            }
        }

        private void OnEnable()
        {
            _bodyStyle = null;
            _showStartScreenOptions = ShowStartScreen.GetNames(typeof(ShowStartScreen)).Select(x => ObjectNames.NicifyVariableName(x)).ToArray();
        }

        private void OnDisable()
        {
            BlockadeGUI.CleanupBackgroundTextures(_styleTag);
        }

        [MenuItem(WindowUtils.MenuRoot + "/Open Skybox AI Scene", false, 0)]
        public static void OpenSkyboxAIScene()
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(_skyboxSceneGuid);
            EditorSceneManager.OpenScene(scenePath);
        }

        [MenuItem(WindowUtils.MenuRoot + "/Start Screen", false, 0)]
        public static void OpenStartScreen()
        {
            StartScreen window = GetWindow<StartScreen>(true, _windowTitle, true);
            window.minSize = new Vector2(1020, 688);
            window.maxSize = window.minSize;
            window.Show();
        }

        [MenuItem(WindowUtils.MenuRoot + "/Support (Discord)", false, 3)]
        public static void OpenSupport()
        {
            Application.OpenURL(_discord);
        }

        private void InitStyles()
        {
            if (_bodyStyle != null) return;

            BlockadeGUI.StyleFontSize = 16;
            BlockadeGUI.StyleTag = _styleTag;
            BlockadeGUI.StyleFont = WindowUtils.GetFont();

            _bodyStyle = BlockadeGUI.CreateStyle(Color.white);
            _bodyStyle.stretchWidth = false;

            _linkStyle = BlockadeGUI.CreateStyle(Color.white);
            _linkStyle.fontStyle = FontStyle.Bold;

            _navLinkStyle = BlockadeGUI.CreateStyle("#02ee8b");
            _navLinkStyle.fontStyle = FontStyle.Bold;

            _buttonStyle = BlockadeGUI.CreateStyle(Color.black, BlockadeGUI.HexColor("#02ee8b"));
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.stretchWidth = false;
            _buttonStyle.alignment = TextAnchor.MiddleCenter;

            _versionStyle = BlockadeGUI.CreateStyle(Color.white);
            _versionStyle.fontSize = 14;
            _versionStyle.padding.right = 12;
            _versionStyle.wordWrap = false;

            _bgStyle = BlockadeGUI.CreateStyle(Color.white, BlockadeGUI.HexColor("#313131"));
            _bgStyle.padding = new RectOffset(28, 0, 16, 16);

            _titleSection = new GUIStyle();
            _titleSection.padding = new RectOffset(0, 0, 0, 28);

            _showStartScreenStyle = BlockadeGUI.CreateStyle(Color.white);
            _showStartScreenStyle.fontSize = 12;
            _showStartScreenStyle.alignment = TextAnchor.MiddleCenter;

            _showStartScreenPopupStyle = new GUIStyle(EditorStyles.popup);
            _showStartScreenPopupStyle.fontSize = 12;
            _showStartScreenPopupStyle.font = BlockadeGUI.StyleFont;
            BlockadeGUI.SetTextColor(_showStartScreenPopupStyle, Color.white);
            _showStartScreenPopupStyle.alignment = TextAnchor.MiddleCenter;

            _h1Style = BlockadeGUI.CreateStyle(Color.white);
            _h1Style.fontStyle = FontStyle.Bold;
            _h1Style.fontSize = 32;

            _h2Style = BlockadeGUI.CreateStyle(Color.white);
            _h2Style.fontStyle = FontStyle.Bold;
            _h2Style.fontSize = 24;

            _h3Style = BlockadeGUI.CreateStyle(Color.white);
            _h3Style.fontStyle = FontStyle.Bold;
            _h3Style.fontSize = 20;

            _h4Style = BlockadeGUI.CreateStyle("#B1B1B1");
            _h4Style.fontSize = 12;

            WindowUtils.CenterOnEditor(this);
            ReadChangeLog();
        }

        private void Bullet(string text, string bullet = "•")
        {
            BlockadeGUI.Horizontal(() =>
            {
                var ws = 1 + text.IndexOf('-');
                for (int i = 0; i < ws; i++)
                {
                    GUILayout.Space(8);
                }

                GUILayout.Label(bullet, _bodyStyle);
                GUILayout.Space(8);
                GUILayout.Label(text.Substring(ws + 1), _bodyStyle, GUILayout.ExpandWidth(true));
            });
        }

        private void ReadChangeLog()
        {
            _changelog.Clear();
            var changelogPath = AssetDatabase.GUIDToAssetPath(_changelogGuid);
            var changelogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(changelogPath);
            _changelog = changelogAsset.text.Split('\n')
                .Select(x => Regex.Replace(x.TrimEnd(), @"\*\*(.*?)\*\*", "<b>$1</b>"))
                .Select(x => Regex.Replace(x, @"\*(.*?)\*", "<i>$1</i>"))
                .Select(x => Regex.Replace(x, @"`(.*?)`", "<b>$1</b>"))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var start = _changelog.FindIndex(l => l.StartsWith("## "));
            // var end = _changelog.FindIndex(start + 1, l => l.StartsWith("## "));
            // if (end < 0) end = _changelog.Count;
            _changelog = _changelog.GetRange(start, _changelog.Count - start);
        }

        private void News()
        {
            BlockadeGUI.HorizontalLine(BlockadeGUI.HexColor("#797979"));
            EditorGUILayout.Space(8);

            GUILayout.Label("BLOCKADE NEWS", _h4Style);

            GUILayout.Space(28);

            GUILayout.Label("New Feature Headline", _h2Style);

            GUILayout.Space(8);

            GUILayout.Label("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris dignissim risus sed hendrerit ullamcorper. Aliquam justo est, semper ac nisi sit amet, aliquet aliquam nunc. Sed vitae nisl eget nunc aliquam aliquet. Sed vitae nisl eget nunc aliquam aliquet.", _bodyStyle);
        }

        private void Changelog()
        {
            BlockadeGUI.HorizontalLine(BlockadeGUI.HexColor("#797979"));
            EditorGUILayout.Space(8);

            GUILayout.Label("RELEASE NOTES", _h4Style);

            GUILayout.Space(28);

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
                    if (i > 0)
                    {
                        EditorGUILayout.Space(28);
                    }

                    GUILayout.Label(line.Substring(3), _h3Style);
                    EditorGUILayout.Space();
                }
                else
                 {
                    Bullet(line);
                    EditorGUILayout.Space();
                }
            }
        }

        private void OnGUI()
        {
            InitStyles();

            BlockadeGUI.Vertical(_bgStyle, () =>
            {
                BlockadeGUI.Horizontal(_titleSection, () =>
                {
                    WindowUtils.DrawLogo();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Version: " + WindowUtils.GetVersion(), _versionStyle, GUILayout.ExpandHeight(true));
                });

                BlockadeGUI.Horizontal(() =>
                {
                    BlockadeGUI.Vertical(196, () =>
                    {
                        GUILayout.Label("RESOURCES", _h4Style);

                        GUILayout.Space(28);

                        if (BlockadeGUI.Link("GET API KEY", _navLinkStyle))
                        {
                            Application.OpenURL(_apikeyUrl);
                        }

                        GUILayout.Space(28);

                        if (BlockadeGUI.Link("DOCS (GITHUB)", _navLinkStyle))
                        {
                            Application.OpenURL(_github);
                        }

                        GUILayout.Space(28);

                        if (BlockadeGUI.Link("DISCORD INVITE", _navLinkStyle))
                        {
                            Application.OpenURL(_discord);
                        }

                        GUILayout.Space(28);

                        if (BlockadeGUI.Link("WEBSITE", _navLinkStyle))
                        {
                            Application.OpenURL(_website);
                        }

                        GUILayout.FlexibleSpace();

                        GUILayout.Label("Show On Start: ", _showStartScreenStyle);
                        GUILayout.Space(4);

                        var settings = PluginSettings.Get();
                        var newShowStartScreen = (ShowStartScreen)EditorGUILayout.Popup((int)settings.ShowStartScreen, _showStartScreenOptions, _showStartScreenPopupStyle, GUILayout.Width(205));
                        if (settings.ShowStartScreen != newShowStartScreen)
                        {
                            settings.ShowStartScreen = newShowStartScreen;
                            settings.Save();
                        }
                    });

                    GUILayout.Space(24);

                    BlockadeGUI.VerticalLine(BlockadeGUI.HexColor("#797979"));

                    GUILayout.Space(32);

                    BlockadeGUI.Vertical(() =>
                    {
                        _scrollPosition = BlockadeGUI.Scroll(_scrollPosition, () =>
                        {
                            GUILayout.Label("How to Get Started", _h1Style, GUILayout.ExpandWidth(true));

                            EditorGUILayout.Space(4);

                            BlockadeGUI.Horizontal(() =>
                            {
                                GUILayout.Space(8);
                                GUILayout.Label("1.", _bodyStyle);
                                GUILayout.Space(8);
                                GUILayout.Label("Go to ", _bodyStyle);
                                if (BlockadeGUI.Link("api.blockadelabs.com", _linkStyle))
                                {
                                    Application.OpenURL(_apikeyUrl);
                                }
                                GUILayout.Label(" to get your API key.", _bodyStyle);
                            });

                            EditorGUILayout.Space(4);

                            Bullet("- Open the Skybox AI Scene.", "2.");

                            EditorGUILayout.Space(4);

                            Bullet("- Select the \"Blockade Labs Skybox Generator\" gameObject and enter your API key.", "3.");

                            EditorGUILayout.Space(4);

                            Bullet("- Enter play mode to start generating in our immersive experience, or use the component in editor mode if you prefer.", "4.");

                            EditorGUILayout.Space(24);

                            if (BlockadeGUI.Button("OPEN SKYBOX AI SCENE", _buttonStyle, 273, 48))
                            {
                                OpenSkyboxAIScene();
                            }

                            // EditorGUILayout.Space(28);
                            // News();

                            EditorGUILayout.Space(28);
                            Changelog();

                            EditorGUILayout.Space(28);
                        });
                    });

                    EditorGUILayout.Space();
                });
            });
        }
    }
}