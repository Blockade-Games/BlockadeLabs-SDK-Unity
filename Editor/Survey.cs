using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace BlockadeLabsSDK.Editor
{
    internal class Survey : EditorWindow
    {
        private static readonly string[] _quantitativeColors = new string[]
        {
            "#FC5555", "#FC5555", "#FC5555", "#FC5555", "#FC5555",
            "#F6B100", "#F6B100", "#F6B100", "#02EE8B", "#02EE8B"
        };

        private const string _windowTitle = "Skybox AI Feedback";
        private const string _styleTag = "BlockadeLabsSDK_Survey";

        private static readonly  Vector2 _windowSize = new Vector2(866, 360);

        private GUIStyle _bgStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _optionStyle;
        private GUIStyle _optionDescriptionStyle;
        private GUIStyle _previousButtonStyle;
        private GUIStyle _nextButtonStyle;
        private GUIStyle _nextButtonDisabledStyle;
        private GUIStyle _askLaterButtonStyle;
        private GUIStyle _textAreaStyle;
        private GUIStyle _finishedStyle;

        private static string _apiKey;
        private static GetFeedbacksResponse _feedbacks;
        private static PostFeedbacksRequest _response;
        private static int _currentQuestion;

        [MenuItem("TEST/Feedback Survey")]
        public static void Test()
        {
            ShowSurvey();
        }

        private void ShowTestFeedback()
        {
            var json = Resources.Load<TextAsset>("feedback-test");
            _feedbacks = Newtonsoft.Json.JsonConvert.DeserializeObject<GetFeedbacksResponse>(json.text);
            InitializeResponse();
            _currentQuestion = 0;
        }

        private static void InitializeResponse()
        {
            _response = new PostFeedbacksRequest() {
                id = _feedbacks.id,
                version = _feedbacks.version,
                channel = "website - desktop",
                data = _feedbacks.data.Select(d => new FeedbackAnswer()
                {
                    id = d.id,
                    answer = (d.type == "qualitative") ? "" : -1
                }).ToList()
            };
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
                InitializeResponse();
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
            _bgStyle = null;
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
            if (_bgStyle != null) return;

            ShowTestFeedback(); // TODO: Remove

            BlockadeGUI.StyleFontSize = 14;
            BlockadeGUI.StyleTag = _styleTag;
            BlockadeGUI.StyleFont = WindowUtils.GetFont();

            _bgStyle = BlockadeGUI.CreateStyle(Color.white, BlockadeGUI.HexColor("#313131"));
            _bgStyle.padding.top = 86;
            _bgStyle.padding.bottom = 28;
            _bgStyle.padding.left = 28;
            _bgStyle.padding.right = 28;

            _titleStyle = BlockadeGUI.CreateStyle(Color.white);
            _titleStyle.fontSize = 18;
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.stretchWidth = true;

            _optionStyle = BlockadeGUI.CreateStyle(Color.white);
            _optionStyle.alignment = TextAnchor.MiddleCenter;

            _optionDescriptionStyle = BlockadeGUI.CreateStyle(Color.white);
            _optionDescriptionStyle.wordWrap = false;

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

            _textAreaStyle = new GUIStyle(EditorStyles.textArea);
            _textAreaStyle.padding = new RectOffset(10, 10, 10, 10);
            _textAreaStyle.fontSize = 14;

            _finishedStyle = BlockadeGUI.CreateStyle(Color.white);
            _finishedStyle.fontSize = 32;
            _finishedStyle.fontStyle = FontStyle.Bold;

            _askLaterButtonStyle = BlockadeGUI.CreateStyle(Color.white);

            minSize = maxSize = _windowSize;
            WindowUtils.CenterOnEditor(this);
        }

        private void OnGUI()
        {
            wantsMouseMove = true;
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

        private Vector2 _textScroll;

        private void DrawFeedbackQuestion(FeedbackData data, FeedbackAnswer answer)
        {
            GUILayout.Space(13);

            BlockadeGUI.Horizontal(() =>
            {
                GUILayout.Label(data.question, _titleStyle, GUILayout.ExpandWidth(true));
            });

            GUILayout.Space(28);

            bool canProceed = false;

            switch (data.type)
            {
                case "quantitative":
                    answer.answer = DrawQuantitative((int)answer.answer, data.low_hint, data.high_hint);
                    canProceed = (int)answer.answer >= 0;
                    break;
                case "qualitative":
                    _textScroll = BlockadeGUI.Scroll(_textScroll, 105, () =>
                    {
                        answer.answer = GUILayout.TextArea((string)answer.answer, _textAreaStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    });

                    canProceed = !string.IsNullOrWhiteSpace((string)answer.answer);
                    break;
                case "choice":
                    answer.answer = DrawChoice((int)answer.answer, data.options, _nextButtonStyle, _nextButtonDisabledStyle);
                    canProceed = (int)answer.answer >= 0;
                    break;
            }

            GUILayout.FlexibleSpace();

            BlockadeGUI.Horizontal(() =>
            {
                if (_currentQuestion > 0)
                {
                    if (BlockadeGUI.BoxButton("PREVIOUS", 130, 48, _previousButtonStyle, BlockadeGUI.HexColor("#313131"), BlockadeGUI.HexColor("#8F8F8F"), 2))
                    {
                        _textScroll = Vector2.zero;
                        _currentQuestion--;
                    }
                }

                GUILayout.FlexibleSpace();

                var lastQuestion = _currentQuestion == _feedbacks.data.Count - 1;
                var nextButtonText = lastQuestion ? "FINISH & SUBMIT" : "NEXT";
                var nextButtonStyle = canProceed ? _nextButtonStyle : _nextButtonDisabledStyle;
                var nextButtonWidth = lastQuestion ? 182 : 92;
                if (BlockadeGUI.Button(nextButtonText, nextButtonStyle, nextButtonWidth, 48) && canProceed)
                {
                    _textScroll = Vector2.zero;
                    _currentQuestion++;
                    if (lastQuestion)
                    {
                        Submit();
                    }
                }
            });

            if (BlockadeGUI.Link("Ask me later", _askLaterButtonStyle, GUILayoutUtility.GetLastRect().center))
            {
                Close();
            }
        }

        private int DrawQuantitative(int selected, string lowHint, string highHint)
        {
            var totalWidth = 600;
            var gap = 6;
            var gapTotalWidth = gap * (_quantitativeColors.Length - 1);
            var optionWidth = (totalWidth - gapTotalWidth) / _quantitativeColors.Length;

            BlockadeGUI.HorizontalCentered(() =>
            {
                BlockadeGUI.Vertical(totalWidth, () =>
                {
                    BlockadeGUI.Horizontal(() =>
                    {
                        for (int i = 0; i < _quantitativeColors.Length; i++)
                        {
                            var rect = BlockadeGUI.GetRect(optionWidth, 50);
                            bool hovered = rect.Contains(Event.current.mousePosition);
                            var borderWidth = (hovered || selected == i) ? 4 : 2;
                            if (BlockadeGUI.BoxButton((i + 1).ToString(), rect, _optionStyle, BlockadeGUI.HexColor("#313131"), BlockadeGUI.HexColor(_quantitativeColors[i]), borderWidth))
                            {
                                selected = i;
                            }

                            if (hovered && selected != i)
                            {
                                float lineLength = 10;
                                float lineY = rect.center.y + 10;
                                BlockadeGUI.Line(Color.white, rect.center.x - lineLength / 2, lineY, rect.center.x + lineLength / 2, lineY);
                            }

                            if (i < _quantitativeColors.Length - 1)
                            {
                                GUILayout.Space(gap);
                            }
                        }
                    });

                    GUILayout.Space(8);

                    BlockadeGUI.Horizontal(() =>
                    {
                        GUILayout.Label(lowHint, _optionDescriptionStyle);
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(highHint, _optionDescriptionStyle);
                    });
                });
            });

            return selected;
        }

        private int DrawChoice(int selected, List<string> options, GUIStyle style, GUIStyle selectedStyle)
        {
            BlockadeGUI.HorizontalCentered(() =>
            {
                var totalWidth = 600 / options.Count;
                var gap = 6;
                var totalGap = gap * (options.Count - 1);
                var optionWidth = (totalWidth - totalGap);

                for (int i = 0; i < options.Count; i++)
                {
                    var rect = BlockadeGUI.GetRect(optionWidth, 50);
                    var borderColor = selected == i ? BlockadeGUI.HexColor("#02ee8b") : BlockadeGUI.HexColor("#8F8F8F");
                    var borderWidth = selected == i ? 4 : 2;

                    if (BlockadeGUI.BoxButton(options[i], rect, _optionStyle, BlockadeGUI.HexColor("#313131"), borderColor, borderWidth))
                    {
                        selected = i;
                    }

                    if (i < options.Count - 1)
                    {
                        GUILayout.Space(gap);
                    }
                }
            });

            return selected;
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
                BlockadeGUI.Image("d72328bf14e301c4d8a66331b344be13", 43, 43);
            });

            GUILayout.Space(8);

            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label("Finished!", _finishedStyle);
            });

            GUILayout.Space(8);

            BlockadeGUI.HorizontalCentered(() =>
            {
                GUILayout.Label("Thank you for your feedback. We appreciate your help in making our product better.", _titleStyle, GUILayout.Width(479));
            });

            GUILayout.FlexibleSpace();

            BlockadeGUI.HorizontalCentered(() =>
            {
                if (BlockadeGUI.Button("BACK TO SKYBOX AI", _nextButtonStyle, 211, 48))
                {
                    Close();
                }
            });
        }
    }
}