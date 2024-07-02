#if false

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace BlockadeLabsSDK.Editor
{
    internal class Survey : EditorWindow
    {
        private struct SurveyData
        {
            public string sdkVersion;
            public string unityVersion;
            public int satisfaction;
            public int nps;
            public string benefits;
            public string improvements;
        }

        private enum SurveyState
        {
            Ask,
            DontAsk,
            Completed
        }

        private enum SurveyPage
        {
            Satisfaction,
            Benefits,
            Improvements,
            Promoter,
            ThankYou
        }

        private static readonly string[] _satisfactionOptions = new string[] { "Extremely disappointed", "Somewhat disappointed", "Not disappointed" };
        private static readonly string[] _npsOptions = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        private static readonly string[] _npsBgColors = new string[] { "#ff0000", "#ff1100", "#ff2200", "#ff3300", "#ff4400", "#ff5500", "#ff6600", "#ff9900", "#ffCC00", "#88ff00", "#00ff00" };
        private static readonly string[] _npsTextColors = new string[] { "#ffffff", "#ffffff", "#ffffff", "#ffffff", "#ffffff", "#ffffff", "#ffffff", "#000000", "#000000", "#ffffff", "#ffffff" };

        private static readonly Vector2 _initialSize = new Vector2(580, 400);
        private static readonly Vector2 _expandedSize = new Vector2(580, 520);

        private const string _stateKey = "BlockadeLabsSDK_Survey_State";
        private const string _lastAskedKey = "BlockadeLabsSDK_Survey_LastAsked";
        private const string _generatedCountKey = "BlockadeLabsSDK_Survey_GeneratedCount";

        private const int _askFrequency = 5;

        private const string _windowTitle = "Skybox AI Feedback";
        private const string _styleTag = "BlockadeLabsSDK_Survey";

        private GUIStyle _bodyStyle;
        private GUIStyle _boldStyle;
        private GUIStyle _toggleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _dontAskButtonStyle;
        private GUIStyle _textAreaStyle;
        private GUIStyle[] _npsButtonStyles;

        private SurveyData _surveyData;
        private SurveyPage _page;

        public static bool Completed => EditorPrefs.GetInt(_stateKey, 0) == (int)SurveyState.Completed;

        public static bool ShouldAsk()
        {
            if (Application.isBatchMode)
            {
                return false;
            }

            if (EditorPrefs.GetInt(_stateKey, 0) != (int)SurveyState.Ask)
            {
                return false;
            }

            var generatedCount = EditorPrefs.GetInt(_generatedCountKey, 0);
            return generatedCount % _askFrequency == 0;
        }

        [MenuItem(WindowUtils.MenuRoot + "/Feedback")] // TODO: Remove
        public static void ShowSurvey()
        {
            var window = GetWindow<Survey>(true, _windowTitle, true);
            window.Show();
        }

        private void OnEnable()
        {
            _bodyStyle = null;
            _surveyData = new SurveyData {
                sdkVersion = WindowUtils.GetVersion(),
                unityVersion = Application.unityVersion,
                satisfaction = -1,
                nps = -1,
            };
        }

        private void OnDisable()
        {
            BlockadeGUI.CleanupBackgroundTextures(_styleTag);
        }

        private void InitStyles()
        {
            if (_bodyStyle != null) return;

            BlockadeGUI.StyleFontSize = 14;
            BlockadeGUI.StyleTag = _styleTag;
            BlockadeGUI.StyleFont = WindowUtils.GetFont();

            _bodyStyle = BlockadeGUI.CreateStyle(Color.white);
            _bodyStyle.margin.left = 10;
            _bodyStyle.margin.top = 10;
            _bodyStyle.stretchWidth = false;

            _boldStyle = new GUIStyle(_bodyStyle);
            _boldStyle.fontStyle = FontStyle.Bold;
            _boldStyle.fontSize++;

            _buttonStyle = BlockadeGUI.CreateStyle(Color.black, BlockadeGUI.HexColor("#02ee8b"));
            _buttonStyle.fontSize = 14;
            _buttonStyle.margin.left = 10;
            _buttonStyle.margin.bottom = 5;
            _buttonStyle.padding = new RectOffset(10, 10, 5, 5);
            _buttonStyle.stretchWidth = false;
            _buttonStyle.alignment = TextAnchor.MiddleLeft;

            _toggleStyle = new GUIStyle(EditorStyles.toggle);
            _textAreaStyle = BlockadeGUI.CreateStyle(Color.black, Color.white);
            _dontAskButtonStyle = new GUIStyle(EditorStyles.miniButton);

            _npsButtonStyles = new GUIStyle[_npsOptions.Length];
            for (int i = 0; i < _npsOptions.Length; i++)
            {
                _npsButtonStyles[i] = BlockadeGUI.CreateStyle(_npsTextColors[i], _npsBgColors[i]);
                _npsButtonStyles[i].fontSize = 14;
                _npsButtonStyles[i].stretchWidth = false;
                _npsButtonStyles[i].alignment = TextAnchor.MiddleCenter;
            }

            WindowUtils.CenterOnEditor(this);
        }

        private int ToggleGroup(int selected, string[] options, GUIStyle[] styles)
        {
            int newSelected = selected;
            BlockadeGUI.Horizontal(() =>
            {
                for (int i = 0; i < options.Length; i++)
                {
                    var option = options[i];
                    if (GUILayout.Toggle(selected == i, option, styles[i]))
                    {
                        newSelected = i;
                    }

                    if (i < options.Length - 1)
                    {
                        GUILayout.FlexibleSpace();
                    }
                }
            });

            return newSelected;
        }

        private void OnGUI()
        {
            InitStyles();

            BlockadeGUI.Vertical(() =>
            {

                switch (_page)
                {
                    case SurveyPage.Satisfaction:
                        SatisfactionPage();
                        break;
                    case SurveyPage.Benefits:
                        BenefitsPage();
                        break;
                    case SurveyPage.Improvements:
                        ImprovementsPage();
                        break;
                    case SurveyPage.Promoter:
                        PromoterPage();
                        break;
                    case SurveyPage.ThankYou:
                        ThankYouPage();
                        break;
                }
            });
        }

        private void SatisfactionPage()
        {
            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label("How would you feel if you could no longer use Skybox AI?", _bodyStyle);
            });

            _surveyData.satisfaction = ToggleGroup(_surveyData.satisfaction, _satisfactionOptions, new GUIStyle[] { _toggleStyle, _toggleStyle, _toggleStyle });

            BlockadeGUI.Horizontal(() =>
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Ask me later", _dontAskButtonStyle))
                {
                    Close();
                }

                if (GUILayout.Button("Don't ask again", _dontAskButtonStyle))
                {
                    EditorPrefs.SetInt(_stateKey, (int)SurveyState.DontAsk);
                    Close();
                }
            });

            if (_surveyData.satisfaction != -1)
            {
                _page = SurveyPage.Benefits;
            }
        }

        private void BenefitsPage()
        {
            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label("What is the main benefit you get from Skybox AI?", _bodyStyle);
            });

            _surveyData.benefits = GUILayout.TextArea(_surveyData.benefits, _textAreaStyle, GUILayout.Height(100));

            BlockadeGUI.Horizontal(() =>
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Submit", _buttonStyle))
                {
                    _page = SurveyPage.Improvements;
                }
            });
        }

        private void ImprovementsPage()
        {
            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label("How can Skybox AI be improved for you?", _bodyStyle);
            });

            _surveyData.improvements = GUILayout.TextArea(_surveyData.improvements, _textAreaStyle, GUILayout.Height(100));

            BlockadeGUI.Horizontal(() =>
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Submit", _buttonStyle))
                {
                    _page = SurveyPage.Promoter;
                }
            });
        }

        private void PromoterPage()
        {
            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label("How likely are you to recommend Skybox AI to a colleague or friend?", _bodyStyle);
            });

            _surveyData.nps = ToggleGroup(_surveyData.nps, _npsOptions, _npsButtonStyles);

            if (_surveyData.nps != -1)
            {
                SendSurvey();
                _page = SurveyPage.ThankYou;
            }
        }

        private void ThankYouPage()
        {
            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label("Thank you for your feedback!", _bodyStyle);
            });

            BlockadeGUI.HorizontalCentered(() =>
            {
                if (GUILayout.Button("Close", _buttonStyle))
                {
                    Close();
                }
            });
        }

        private void SendSurvey()
        {
            Debug.Log("Sending feedback...");
        }
    }
}

#endif