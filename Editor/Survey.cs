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

        private static readonly Vector2 _initialSize = new Vector2(580, 400);
        private static readonly Vector2 _expandedSize = new Vector2(580, 520);

        private const string _windowTitle = "Skybox AI Feedback";
        private const string _styleTag = "BlockadeLabsSDK_Survey";

        private GUIStyle _bodyStyle;
        private GUIStyle _boldStyle;
        private GUIStyle _toggleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _dontAskButtonStyle;
        private GUIStyle _textAreaStyle;
        private GUIStyle[] _npsButtonStyles;

        private static string _apiKey;
        private static GetFeedbacksResponse _feedbacks;
        private static PostFeedbacksRequest _response;
        private static int _currentQuestion;

        private int _intAnswer = -1;
        private string _stringAnswer = "";

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

            if (_feedbacks.data.Count > 0)
            {
                _response = new PostFeedbacksRequest() {
                    id = _feedbacks.id,
                    version = _feedbacks.version,
                    channel = "integration - unity",
                    data = _feedbacks.data.Select(d => new FeedbackAnswer() { id = d.id }).ToList()
                };

                _currentQuestion = 0;

                _apiKey = apiKey;

                ShowSurvey();
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
            switch (data.type)
            {
                case "quantitative":
                    DrawQuantitativeQuestion(data, answer);
                    break;
                case "qualitative":
                    DrawQualitativeQuestion(data, answer);
                    break;
                case "choice":
                    DrawChoiceQuestion(data, answer);
                    break;
            }
        }

        private void DrawQuantitativeQuestion(FeedbackData data, FeedbackAnswer answerData)
        {
            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label(data.question, _bodyStyle);
            });

            _intAnswer = ToggleGroup(_intAnswer, _npsOptions, _npsButtonStyles);
            if (_intAnswer != -1)
            {
                answerData.answer = _intAnswer;
                _intAnswer = -1;
                NextQuestionOrSubmit();
            }
        }

        private void DrawQualitativeQuestion(FeedbackData data, FeedbackAnswer answerData)
        {
            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label(data.question, _bodyStyle);
            });

            _stringAnswer = GUILayout.TextArea(_stringAnswer, _textAreaStyle, GUILayout.Height(100));

            BlockadeGUI.Horizontal(() =>
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Submit", _buttonStyle))
                {
                    answerData.answer = _stringAnswer;
                    _stringAnswer = "";
                    NextQuestionOrSubmit();
                }
            });
        }

        private void DrawChoiceQuestion(FeedbackData data, FeedbackAnswer answerData)
        {
            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label(data.question, _bodyStyle);
            });

            _intAnswer = ToggleGroup(_intAnswer, data.options.ToArray(), data.options.Select(o => _toggleStyle).ToArray());
            if (_intAnswer != -1)
            {
                answerData.answer = _intAnswer;
                _intAnswer = -1;
                NextQuestionOrSubmit();
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
                ApiRequests.PostFeedbackAsync(_response, _apiKey);
            }
            catch (Exception)
            {
            }

            Close();
        }

        private void DrawThankYou()
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
    }
}