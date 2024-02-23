using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace BlockadeLabsSDK.Editor
{
    internal class Survey : EditorWindow
    {
        private static readonly string[] _npsOptions = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        private static readonly string[] _npsBgColors = new string[] { "#ff0000", "#ff1100", "#ff2200", "#ff3300", "#ff4400", "#ff5500", "#ff6600", "#ff9900", "#ffCC00", "#88ff00", "#00ff00" };
        private static readonly string[] _npsTextColors = new string[] { "#ffffff", "#ffffff", "#ffffff", "#ffffff", "#ffffff", "#ffffff", "#ffffff", "#000000", "#000000", "#ffffff", "#ffffff" };

        private const string _windowTitle = "Skybox AI Feedback";
        private const string _styleTag = "BlockadeLabsSDK_Survey";

        private static readonly  Vector2 _windowSize = new Vector2(866, 360);

        private GUIStyle _bgStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _boldStyle;
        private GUIStyle _toggleStyle;
        private GUIStyle _previousButtonStyle;
        private GUIStyle _nextButtonStyle;
        private GUIStyle _nextButtonDisabledStyle;
        private GUIStyle _askLaterButtonStyle;
        private GUIStyle _textAreaStyle;
        private GUIStyle[] _npsButtonStyles;

        private static string _apiKey;
        private static GetFeedbacksResponse _feedbacks;
        private static PostFeedbacksRequest _response;
        private static int _currentQuestion;

        private int _intAnswer = -1;
        private string _stringAnswer = "";

        [MenuItem("TEST/Feedback Survey")]
        public static void Test()
        {
            var json = Resources.Load<TextAsset>("feedback-test");
            _feedbacks = Newtonsoft.Json.JsonConvert.DeserializeObject<GetFeedbacksResponse>(json.text);
            _response = new PostFeedbacksRequest() {
                    id = _feedbacks.id,
                    version = _feedbacks.version,
                    channel = "website - desktop",
                    data = _feedbacks.data.Select(d => new FeedbackAnswer() { id = d.id }).ToList()
                };
            _currentQuestion = 0;
            ShowSurvey();
        }

        public static async void Trigger(string apiKey)
        {
            if (_feedbacks != null)
            {
                // Already showing
                return;
            }

            try
            {
                _feedbacks = await ApiRequests.GetFeedbacksAsync(apiKey);
            }
            catch (Exception)
            {
                return;
            }

            if (_feedbacks.data?.Count > 0)
            {
                Debug.Log("Feedbacks: " + _feedbacks.data.Count);
                _response = new PostFeedbacksRequest() {
                    id = _feedbacks.id,
                    version = _feedbacks.version,
                    channel = "website - desktop",
                    data = _feedbacks.data.Select(d => new FeedbackAnswer() { id = d.id }).ToList()
                };

                _currentQuestion = 0;

                _apiKey = apiKey;

                ShowSurvey();
            }
            else
            {
                Debug.Log("No feedbacks");
            }
        }

        public static void ShowSurvey()
        {
            var window = GetWindow<Survey>(true, _windowTitle, true);
            window.Show();
        }

        private void OnEnable()
        {
            _bodyStyle = null;
        }

        private void OnDisable()
        {
            if (_feedbacks != null && !string.IsNullOrWhiteSpace(_apiKey) && _currentQuestion < _feedbacks.data.Count)
            {
                var requestData = new PostFeedbacksSkipRequest()
                {
                    id = _feedbacks.id,
                    channel = "website - desktop",
                    ask_me_later = true
                };

                ApiRequests.PostFeedbackSkipAsync(requestData, _apiKey);
            }

            _feedbacks = null;
            _response = null;
            BlockadeGUI.CleanupBackgroundTextures(_styleTag);
        }

        private void InitStyles()
        {
            if (_bodyStyle != null) return;

            BlockadeGUI.StyleFontSize = 14;
            BlockadeGUI.StyleTag = _styleTag;
            BlockadeGUI.StyleFont = WindowUtils.GetFont();

            _bgStyle = BlockadeGUI.CreateStyle(Color.white, BlockadeGUI.HexColor("#313131"));

            _titleStyle = BlockadeGUI.CreateStyle(Color.white);
            _titleStyle.fontSize = 18;

            _bodyStyle = BlockadeGUI.CreateStyle(Color.white);
            _bodyStyle.margin.left = 10;
            _bodyStyle.margin.top = 10;
            _bodyStyle.stretchWidth = false;

            _boldStyle = new GUIStyle(_bodyStyle);
            _boldStyle.fontStyle = FontStyle.Bold;
            _boldStyle.fontSize++;

            _previousButtonStyle = BlockadeGUI.CreateStyle(Color.white);
            _previousButtonStyle.fontSize = 16;
            _previousButtonStyle.stretchWidth = false;
            _previousButtonStyle.alignment = TextAnchor.MiddleCenter;
            _previousButtonStyle.fontStyle = FontStyle.Bold;

            _nextButtonStyle = BlockadeGUI.CreateStyle(Color.black, BlockadeGUI.HexColor("#02ee8b"));
            _nextButtonStyle.fontSize = 16;
            _nextButtonStyle.stretchWidth = false;
            _nextButtonStyle.alignment = TextAnchor.MiddleCenter;
            _nextButtonStyle.fontStyle = FontStyle.Bold;

            _nextButtonDisabledStyle = BlockadeGUI.CreateStyle(BlockadeGUI.HexColor("#313131"), BlockadeGUI.HexColor("#4E4E4E"));
            _nextButtonDisabledStyle.fontSize = 16;
            _nextButtonDisabledStyle.stretchWidth = false;
            _nextButtonDisabledStyle.alignment = TextAnchor.MiddleCenter;
            _nextButtonDisabledStyle.fontStyle = FontStyle.Bold;

            _toggleStyle = new GUIStyle(EditorStyles.toggle);

            _textAreaStyle = new GUIStyle(EditorStyles.textArea);
            _textAreaStyle.padding = new RectOffset(10, 10, 10, 10);
            _textAreaStyle.fontSize = 14;

            _askLaterButtonStyle = BlockadeGUI.CreateStyle(Color.white);

            _npsButtonStyles = new GUIStyle[_npsOptions.Length];
            for (int i = 0; i < _npsOptions.Length; i++)
            {
                _npsButtonStyles[i] = BlockadeGUI.CreateStyle(_npsTextColors[i], _npsBgColors[i]);
                _npsButtonStyles[i].fontSize = 14;
                _npsButtonStyles[i].stretchWidth = false;
                _npsButtonStyles[i].alignment = TextAnchor.MiddleCenter;
            }

            minSize = maxSize = _windowSize;
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

            BlockadeGUI.VerticalExpanded(_bgStyle, () =>
            {
                if (_currentQuestion < _feedbacks.data.Count)
                {
                    DrawFeedbackQuestion(_feedbacks.data[_currentQuestion], _response.data[_currentQuestion]);
                }
                else
                {
                    DrawThankYou();
                }
            });
        }

        private void DrawFeedbackQuestion(FeedbackData data, FeedbackAnswer answer)
        {
            GUILayout.Space(99);

            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label(data.question, _titleStyle);
            });

            GUILayout.Space(28);

            switch (data.type)
            {
                case "quantitative":
                    _intAnswer = ToggleGroup(_intAnswer, _npsOptions, _npsButtonStyles);
                    break;
                case "qualitative":
                    BlockadeGUI.HorizontalCentered(() =>
                    {
                        _stringAnswer = GUILayout.TextArea(_stringAnswer, _textAreaStyle, GUILayout.Height(105), GUILayout.Width(810));
                    });
                    break;
                case "choice":
                    _intAnswer = ToggleGroup(_intAnswer, data.options.ToArray(), data.options.Select(o => _toggleStyle).ToArray());
                    break;
            }

            GUILayout.Space(28);

            BlockadeGUI.Horizontal(() =>
            {
                GUILayout.Space(28);

                if (_currentQuestion > 0)
                {
                    if (BlockadeGUI.BoxButton("PREVIOUS", 130, 48, _previousButtonStyle, BlockadeGUI.HexColor("#313131"), BlockadeGUI.HexColor("#8F8F8F"), 2))
                    {
                        _currentQuestion--;
                    }
                }

                GUILayout.FlexibleSpace();

                bool canProceed = _intAnswer != -1 || !string.IsNullOrWhiteSpace(_stringAnswer);

                if (BlockadeGUI.Button("NEXT", canProceed ? _nextButtonStyle : _nextButtonDisabledStyle, 92, 48) && canProceed)
                {
                    if (data.type == "quantitative" || data.type == "choice")
                    {
                        answer.answer = _intAnswer;
                    }
                    else
                    {
                        answer.answer = _stringAnswer;
                    }

                    _intAnswer = -1;
                    _stringAnswer = "";
                    NextQuestionOrSubmit();
                }

                GUILayout.Space(28);
            });

            if (BlockadeGUI.Link("Ask me later", _askLaterButtonStyle, GUILayoutUtility.GetLastRect().center))
            {
                Close();
            }
        }

        private void NextQuestionOrSubmit()
        {
            _currentQuestion++;

            if (_currentQuestion >= _feedbacks.data.Count)
            {
                Submit();
            }
        }

        private async void Submit()
        {
            try
            {
                await ApiRequests.PostFeedbackAsync(_response, _apiKey);
            }
            catch (Exception)
            {
            }
        }

        private void DrawThankYou()
        {
            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label("Thank you for your feedback!", _bodyStyle);
            });

            BlockadeGUI.HorizontalCentered(() =>
            {
                if (GUILayout.Button("Close", _nextButtonStyle))
                {
                    Close();
                }
            });
        }
    }
}