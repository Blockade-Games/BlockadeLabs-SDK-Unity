using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class RuntimeGuiManager : MonoBehaviour
    {
        [Header("Core Components")]
        #region Core Components

        [SerializeField]
        private BlockadeLabsSkyboxGenerator _generator;
        public BlockadeLabsSkyboxGenerator Generator
        {
            get { return _generator; }
            set { _generator = value; }
        }

        [SerializeField]
        private BlockadeLabsSkyboxMesh _skybox;
        public BlockadeLabsSkyboxMesh Skybox
        {
            get { return _skybox; }
            set { _skybox = value; }
        }

        [SerializeField]
        private BlockadeDemoCamera _demoCamera;
        public BlockadeDemoCamera DemoCamera
        {
            get { return _demoCamera; }
            set { _demoCamera = value; }
        }

        #endregion Core Components

        [Header("Skybox Generator")]

        #region Skybox Generator

        [SerializeField]
        private GameObject _bottomSection;
        public GameObject BottomSection
        {
            get { return _bottomSection; }
            set { _bottomSection = value; }
        }

        [SerializeField]
        private TMP_InputField _promptInput;
        public TMP_InputField PromptInput
        {
            get { return _promptInput; }
            set { _promptInput = value; }
        }

        [SerializeField]
        private Button _createButton;
        public Button CreateButton
        {
            get { return _createButton; }
            set { _createButton = value; }
        }

        [SerializeField]
        private Button _remixButton;
        public Button RemixButton
        {
            get { return _remixButton; }
            set { _remixButton = value; }
        }

        [SerializeField]
        private Button _editButton;
        public Button EditButton
        {
            get { return _editButton; }
            set { _editButton = value; }
        }

        [SerializeField]
        private Transform _createRemixUnderline;
        public Transform CreateRemixUnderline
        {
            get { return _createRemixUnderline; }
            set { _createRemixUnderline = value; }
        }

        [SerializeField]
        private float _remixUnderlineOffset;
        public float RemixUnderlineOffset
        {
            get { return _remixUnderlineOffset; }
            set { _remixUnderlineOffset = value; }
        }

        [SerializeField]
        private TMP_Text _hintText;
        public TMP_Text HintText
        {
            get { return _hintText; }
            set { _hintText = value; }
        }

        [SerializeField]
        private string _createHint;
        public string CreateHint
        {
            get { return _createHint; }
            set { _createHint = value; }
        }

        [SerializeField]
        private string _remixHint;
        public string RemixHint
        {
            get { return _remixHint; }
            set { _remixHint = value; }
        }

        [SerializeField]
        private string _editHint;
        public string EditHint
        {
            get { return _editHint; }
            set { _editHint = value; }
        }

        [SerializeField]
        private string _meshCreatorHint;
        public string MeshCreatorHint
        {
            get { return _meshCreatorHint; }
            set { _meshCreatorHint = value; }
        }

        [SerializeField]
        private GameObject _modeTooltip;
        public GameObject ModeTooltip
        {
            get { return _modeTooltip; }
            set { _modeTooltip = value; }
        }

        [SerializeField]
        private GameObject _tipContainer;
        public GameObject TipContainer
        {
            get { return _tipContainer; }
            set { _tipContainer = value; }
        }

        [SerializeField]
        private TMP_Text _tipText;
        public TMP_Text TipText
        {
            get { return _tipText; }
            set { _tipText = value; }
        }

        [SerializeField]
        private Button _generateButton;
        public Button GenerateButton
        {
            get { return _generateButton; }
            set { _generateButton = value; }
        }

        [SerializeField]
        private MultiToggle _enhancePromptToggle;
        public MultiToggle EnhancePromptToggle
        {
            get { return _enhancePromptToggle; }
            set { _enhancePromptToggle = value; }
        }

        [SerializeField]
        private TMP_InputField _negativeTextInput;
        public TMP_InputField NegativeTextInput
        {
            get { return _negativeTextInput; }
            set { _negativeTextInput = value; }
        }

        [SerializeField]
        private MultiToggle _negativeTextToggle;
        public MultiToggle NegativePromptToggle
        {
            get { return _negativeTextToggle; }
            set { _negativeTextToggle = value; }
        }

        [SerializeField]
        private StylePickerPanel _stylePickerPanel;
        public StylePickerPanel StylePickerPanel
        {
            get { return _stylePickerPanel; }
            set { _stylePickerPanel = value; }
        }

        [SerializeField]
        private TMP_Text _selectedStyleText;
        public TMP_Text SelectedStyleText
        {
            get { return _selectedStyleText; }
            set { _selectedStyleText = value; }
        }

        [SerializeField]
        private TMP_Text _promptCharacterLimit;
        public TMP_Text PromptCharacterLimit
        {
            get { return _promptCharacterLimit; }
            set { _promptCharacterLimit = value; }
        }

        [SerializeField]
        private TMP_Text _negativeTextCharacterLimit;
        public TMP_Text NegativeTextCharacterLimit
        {
            get { return _negativeTextCharacterLimit; }
            set { _negativeTextCharacterLimit = value; }
        }

        [SerializeField]
        private GameObject _promptCharacterWarning;
        public GameObject PromptCharacterWarning
        {
            get { return _promptCharacterWarning; }
            set { _promptCharacterWarning = value; }
        }

        [SerializeField]
        private GameObject _negativeTextCharacterWarning;
        public GameObject NegativeTextCharacterWarning
        {
            get { return _negativeTextCharacterWarning; }
            set { _negativeTextCharacterWarning = value; }
        }

        [SerializeField]
        private RectTransform _progressBar;
        public RectTransform ProgressBar
        {
            get { return _progressBar; }
            set { _progressBar = value; }
        }

        [SerializeField]
        private MultiToggle _showSpheresToggle;
        public MultiToggle ShowSpheresToggle
        {
            get { return _showSpheresToggle; }
            set { _showSpheresToggle = value; }
        }

        [SerializeField]
        private GameObject _spheres;
        public GameObject Spheres
        {
            get { return _spheres; }
            set { _spheres = value; }
        }

        #endregion Skybox Generator

        [Header("Mesh Creator")]
        #region Mesh Creator

        [SerializeField]
        private GameObject _meshCreator;
        public GameObject MeshCreator
        {
            get { return _meshCreator; }
            set { _meshCreator = value; }
        }

        [SerializeField]
        private Button _meshCreatorButton;
        public Button MeshCreatorButton
        {
            get { return _meshCreatorButton; }
            set { _meshCreatorButton = value; }
        }

        [SerializeField]
        private Button _meshCreatorBackButton;
        public Button MeshCreatorBackButton
        {
            get { return _meshCreatorBackButton; }
            set { _meshCreatorBackButton = value; }
        }

        [SerializeField]
        private MultiToggle _lowDensityToggle;
        public MultiToggle LowDensityToggle
        {
            get { return _lowDensityToggle; }
            set { _lowDensityToggle = value; }
        }

        [SerializeField]
        private MultiToggle _mediumDensityToggle;
        public MultiToggle MediumDensityToggle
        {
            get { return _mediumDensityToggle; }
            set { _mediumDensityToggle = value; }
        }

        [SerializeField]
        private MultiToggle _highDensityToggle;
        public MultiToggle HighDensityToggle
        {
            get { return _highDensityToggle; }
            set { _highDensityToggle = value; }
        }

        [SerializeField]
        private MultiToggle _epicDensityToggle;
        public MultiToggle EpicDensityToggle
        {
            get { return _epicDensityToggle; }
            set { _epicDensityToggle = value; }
        }

        [SerializeField]
        private Slider _depthScaleSlider;
        public Slider DepthScaleSlider
        {
            get { return _depthScaleSlider; }
            set { _depthScaleSlider = value; }
        }

        [SerializeField]
        private Button _savePrefabButton;
        public Button SavePrefabButton
        {
            get { return _savePrefabButton; }
            set { _savePrefabButton = value; }
        }

        [SerializeField]
        private GameObject _promptPanel;
        public GameObject PromptPanel
        {
            get { return _promptPanel; }
            set { _promptPanel = value; }
        }

        #endregion Mesh Creator

        [Header("Popups")]
        #region Popups

        [SerializeField]
        private GameObject _helloPopup;
        public GameObject HelloPopup
        {
            get { return _helloPopup; }
            set { _helloPopup = value; }
        }

        [SerializeField]
        private GameObject _errorPopup;
        public GameObject ErrorPopup
        {
            get { return _errorPopup; }
            set { _errorPopup = value; }
        }

        [SerializeField]
        private TMP_Text _errorText;
        public TMP_Text ErrorText
        {
            get { return _errorText; }
            set { _errorText = value; }
        }

        [SerializeField]
        private GameObject _loadingPopup;
        public GameObject LoadingPopup
        {
            get { return _loadingPopup; }
            set { _loadingPopup = value; }
        }

        [SerializeField]
        private TMP_Text _loadingPopupText;
        public TMP_Text LoadingPopupText
        {
            get { return _loadingPopupText; }
            set { _loadingPopupText = value; }
        }

        [SerializeField]
        private DialogBox _dialogPopup;
        public DialogBox DialogPopup
        {
            get { return _dialogPopup; }
            set { _dialogPopup = value; }
        }

        [SerializeField]
        private SkyboxPreviewPopup _previewPopup;
        public SkyboxPreviewPopup PreviewPopup
        {
            get => _previewPopup;
            set => _previewPopup = value;
        }

        #endregion Popups

        [Header("Titlebar")]
        #region Titlebar

        [SerializeField]
        private Button _historyButton;
        public Button HistoryButton
        {
            get { return _historyButton; }
            set { _historyButton = value; }
        }

        [SerializeField]
        private GameObject _viewButton;
        public GameObject ViewButton
        {
            get { return _viewButton; }
            set { _viewButton = value; }
        }

        [SerializeField]
        private GameObject _versionSelector;
        public GameObject VersionSelector
        {
            get { return _versionSelector; }
            set { _versionSelector = value; }
        }

        [SerializeField]
        private Button _model2Button;
        public Button Model2Button
        {
            get { return _model2Button; }
            set { _model2Button = value; }
        }

        [SerializeField]
        private GameObject _model2Selected;
        public GameObject Model2Selected
        {
            get { return _model2Selected; }
            set { _model2Selected = value; }
        }

        [SerializeField]
        private Button _model3Button;
        public Button Model3Button
        {
            get { return _model3Button; }
            set { _model3Button = value; }
        }

        [SerializeField]
        private GameObject _model3Selected;
        public GameObject Model3Selected
        {
            get { return _model3Selected; }
            set { _model3Selected = value; }
        }

        #endregion Titlebar

        [Header("History")]
        #region History

        [SerializeField]
        private HistoryPanel _historyPanel;
        public HistoryPanel HistoryPanel
        {
            get { return _historyPanel; }
            set { _historyPanel = value; }
        }

        #endregion History

        private float _createUnderlineOffset;
        private bool _anyStylePicked;

        private void Start()
        {
            _helloPopup.SetActive(false);
            _viewButton.SetActive(true);
            _versionSelector.SetActive(true);
            _promptPanel.SetActive(true);
            _tipContainer.SetActive(false);
            _historyPanel.gameObject.SetActive(false);
            _historyButton.gameObject.SetActive(true);
            _previewPopup.gameObject.SetActive(false);

            // Initialize values
            _skybox.BakedMesh = null;
            _skybox.MeshDensity = MeshDensity.Medium;
            _skybox.DepthScale = _depthScaleSlider.minValue;

            // Bind to property changes that will update the UI
            _generator.OnPropertyChanged += OnGeneratorPropertyChanged;
            OnGeneratorPropertyChanged();

            _skybox.OnPropertyChanged += OnSkyboxPropertyChanged;
            OnSkyboxPropertyChanged();

            _skybox.OnLoadingChanged += OnSkyboxLoadingChanged;

            _generator.OnStateChanged += OnStateChanged;
            OnStateChanged();

            _generator.OnErrorChanged += OnErrorChanged;
            OnErrorChanged();

            // Titlebar
            _model2Button.onClick.AddListener(() => SetModelVersion(SkyboxAiModelVersion.Model2));
            _model3Button.onClick.AddListener(() => SetModelVersion(SkyboxAiModelVersion.Model3));

            // Prompt Panel Controls
            _promptInput.onValueChanged.AddListener(OnPromptInputChanged);
            _negativeTextToggle.OnValueChanged.AddListener(OnNegativeTextToggleChanged);
            _negativeTextInput.onValueChanged.AddListener(OnNegativeTextInputChanged);
            _enhancePromptToggle.OnValueChanged.AddListener(OnEnhancePromptToggleChanged);
            _createButton.onClick.AddListener(OnCreateButtonClicked);
            _remixButton.onClick.AddListener(OnRemixButtonClicked);
            _createUnderlineOffset = _createRemixUnderline.localPosition.x;
            _createButton.GetComponent<Hoverable>().OnHoverChanged.AddListener(_ => UpdateHintText());
            _remixButton.GetComponent<Hoverable>().OnHoverChanged.AddListener(_ => UpdateHintText());
            _editButton.GetComponent<Hoverable>().OnHoverChanged.AddListener(_ => UpdateHintText());
            _meshCreatorButton.GetComponent<Hoverable>().OnHoverChanged.AddListener(_ => UpdateHintText());
            _stylePickerPanel.OnStylePicked += OnStylePicked;
            _generateButton.onClick.AddListener(OnGenerateButtonClicked);
            _meshCreatorButton.onClick.AddListener(OnMeshCreatorButtonClicked);
            _showSpheresToggle.OnValueChanged.AddListener(OnShowSpheresToggleChanged);

            // Mesh Creator Controls
            _meshCreatorBackButton.onClick.AddListener(OnMeshCreatorBackButtonClicked);
            _lowDensityToggle.OnTurnedOn.AddListener(() => _skybox.MeshDensity = MeshDensity.Low);
            _mediumDensityToggle.OnTurnedOn.AddListener(() => _skybox.MeshDensity = MeshDensity.Medium);
            _highDensityToggle.OnTurnedOn.AddListener(() => _skybox.MeshDensity = MeshDensity.High);
            _epicDensityToggle.OnTurnedOn.AddListener(() => _skybox.MeshDensity = MeshDensity.Epic);
            _depthScaleSlider.onValueChanged.AddListener(_ => _skybox.DepthScale = _depthScaleSlider.value);
            _savePrefabButton.onClick.AddListener(OnSavePrefabButtonClicked);

            // History Panel Controls
            _historyButton.onClick.AddListener(() =>
            {
                _historyPanel.gameObject.SetActive(!_historyPanel.gameObject.activeSelf);
                _viewButton.gameObject.SetActive(!_historyPanel.gameObject.activeSelf);
                _versionSelector.gameObject.SetActive(!_historyPanel.gameObject.activeSelf);
                _bottomSection.SetActive(!_historyPanel.gameObject.activeSelf);
            });

            _generator.Reload();
        }

        private void OnGeneratorPropertyChanged()
        {
            UpdateVersionSelector();
            _promptInput.text = _generator.Prompt;
            _enhancePromptToggle.IsOn = _generator.EnhancePrompt;
            _negativeTextInput.text = _generator.NegativeText;
            UpdateHintText();
            UpdateGenerateButton();
            UpdatePromptCharacterLimit();
            UpdateNegativeTextCharacterLimit();
            UpdateCanRemix();
            _promptCharacterWarning.SetActive(false);
            _negativeTextCharacterWarning.SetActive(false);

            if (_anyStylePicked)
            {
                _selectedStyleText.text = _generator.SelectedStyle?.name ?? "Select a Style";
                _stylePickerPanel.SetSelectedStyle(_generator.SelectedStyleFamily, _generator.SelectedStyle);
            }
        }

        private void OnSkyboxPropertyChanged()
        {
            _lowDensityToggle.IsOn = _skybox.MeshDensity == MeshDensity.Low;
            _mediumDensityToggle.IsOn = _skybox.MeshDensity == MeshDensity.Medium;
            _highDensityToggle.IsOn = _skybox.MeshDensity == MeshDensity.High;
            _epicDensityToggle.IsOn = _skybox.MeshDensity == MeshDensity.Epic;
            _depthScaleSlider.value = _skybox.DepthScale;
#if !UNITY_EDITOR
            _savePrefabButton.gameObject.SetActive(false);
#endif
        }

        private void OnSkyboxLoadingChanged(bool isLoading)
        {
            _loadingPopup.SetActive(isLoading);
            _loadingPopupText.text = _skybox.LoadingText;
        }

        private void OnStateChanged()
        {
            if (_generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready)
            {
                _stylePickerPanel.SetStyles(_generator.StyleFamilies);
            }

            var selectables = _promptPanel.GetComponentsInChildren<Selectable>();
            foreach (var selectable in selectables)
            {
                selectable.interactable = _generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready;
            }

            var disabledColors = _promptPanel.GetComponentsInChildren<DisabledColor>();
            foreach (var disabledColor in disabledColors)
            {
                disabledColor.Disabled = _generator.CurrentState != BlockadeLabsSkyboxGenerator.State.Ready;
            }

            UpdateGenerateButton();
            UpdateCanRemix();
            UpdateMeshCreatorButton();
            UpdateTip();

            if (_generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready)
            {
                UpdateSpheres();
            }
        }

        private void UpdateCanRemix()
        {
            bool canRemix = _generator.CanRemix;
            _remixButton.interactable = _generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready && canRemix;
            foreach (var disabledColor in _remixButton.GetComponentsInChildren<DisabledColor>())
            {
                disabledColor.Disabled = !_remixButton.interactable;
            }
        }

        private void UpdateMeshCreatorButton()
        {
            _meshCreatorButton.interactable = _skybox.HasDepthTexture && _generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready;
            var disabledColors = _meshCreatorButton.GetComponentsInChildren<DisabledColor>();
            foreach (var disabledColor in disabledColors)
            {
                disabledColor.Disabled = !_meshCreatorButton.interactable;
            }
        }

        private void OnErrorChanged()
        {
            if (!string.IsNullOrEmpty(_generator.LastError))
            {
                _errorPopup.SetActive(true);
                _errorText.text = _generator.LastError;
            }
            else
            {
                _errorPopup.SetActive(false);
            }
        }

        private void SetModelVersion(SkyboxAiModelVersion version)
        {
            _generator.ModelVersion = version;
            _generator.Remix = false;
        }

        private void OnPromptInputChanged(string newValue)
        {
            _generator.Prompt = newValue;
        }

        private void UpdatePromptCharacterLimit()
        {
            if (_generator.SelectedStyle != null)
            {
                _promptCharacterLimit.text = _generator.Prompt.Length + "/" + _generator.SelectedStyle.maxChar;
                _promptCharacterLimit.color = _generator.Prompt.Length > _generator.SelectedStyle.maxChar ? Color.red : Color.white;
            }
            else
            {
                _promptCharacterLimit.text = "";
            }
        }

        private void OnNegativeTextToggleChanged(bool newValue)
        {
            _generator.NegativeText = newValue ? _negativeTextInput.text : "";
        }

        private void UpdateNegativeTextCharacterLimit()
        {
            if (_generator.SelectedStyle != null)
            {
                _negativeTextCharacterLimit.text = _generator.NegativeText.Length + "/" + _generator.SelectedStyle.negativeTextMaxChar;
                _negativeTextCharacterLimit.color = _generator.NegativeText.Length > _generator.SelectedStyle.negativeTextMaxChar ? Color.red : Color.white;
            }
            else
            {
                _negativeTextCharacterLimit.text = "";
            }
        }

        private void OnNegativeTextInputChanged(string newValue)
        {
            _generator.NegativeText = _negativeTextToggle.IsOn ? newValue : "";
        }

        private void OnEnhancePromptToggleChanged(bool newValue)
        {
            _generator.EnhancePrompt = newValue;
        }

        private void OnCreateButtonClicked()
        {
            _generator.Remix = false;
        }

        private void OnRemixButtonClicked()
        {
            _generator.Remix = true;
        }

        private void OnGenerateButtonClicked()
        {
            if (_generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Generating)
            {
                _generator.Cancel();
                return;
            }

            if (_generator.Prompt.Length == 0)
            {
                _promptCharacterWarning.SetActive(true);
                _promptCharacterWarning.GetComponentInChildren<TMP_Text>().text = "Prompt cannot be empty";
                return;
            }

            if (_generator.Prompt.Length > _generator.SelectedStyle.maxChar)
            {
                _promptCharacterWarning.SetActive(true);
                _promptCharacterWarning.GetComponentInChildren<TMP_Text>().text = $"Prompt should be {_generator.SelectedStyle.maxChar} characters or less";
                return;
            }

            if (_generator.NegativeText.Length > _generator.SelectedStyle.negativeTextMaxChar)
            {
                _negativeTextCharacterWarning.SetActive(true);
                _negativeTextCharacterWarning.GetComponentInChildren<TMP_Text>().text = $"Negative text should be {_generator.SelectedStyle.negativeTextMaxChar} characters or less";
                return;
            }

            if (!_anyStylePicked)
            {
                _stylePickerPanel.gameObject.SetActive(true);
                return;
            }

            _generator.GenerateSkyboxAsync();
        }

        private IEnumerator CoAnimateDepthScale(float target)
        {
            var start = _skybox.DepthScale;
            var time = 0.0f;
            var duration = 0.1f;

            while (time < duration)
            {
                time += Time.deltaTime;
                _skybox.DepthScale = Mathf.Lerp(start, target, time / duration);
                yield return null;
            }

            _skybox.DepthScale = target;
        }

        private void OnMeshCreatorButtonClicked()
        {
            StartCoroutine(CoAnimateDepthScale(_depthScaleSlider.minValue + (_depthScaleSlider.maxValue - _depthScaleSlider.minValue) / 3f));
            _promptPanel.SetActive(false);
            _meshCreator.SetActive(true);
            UpdateSpheres();
            UpdateCamera();
        }

        private void OnShowSpheresToggleChanged(bool toggleOn)
        {
            UpdateSpheres();
            UpdateCamera();
        }

        private void OnMeshCreatorBackButtonClicked()
        {
            StartCoroutine(CoAnimateDepthScale(_depthScaleSlider.minValue));
            _promptPanel.SetActive(true);
            _meshCreator.SetActive(false);
            UpdateSpheres();
            UpdateCamera();
        }

        private void OnSavePrefabButtonClicked()
        {
            if (_skybox.HasDepthTexture)
            {
                _skybox.BakeMesh();
            }

            _skybox.SavePrefab();
            _skybox.BakedMesh = null;
        }

        private void UpdateCamera()
        {
            if (_meshCreator.activeSelf)
            {
                _demoCamera.SetMode(BlockadeDemoCamera.Mode.MeshCreator);
            }
            else if (_showSpheresToggle.IsOn)
            {
                _demoCamera.SetMode(BlockadeDemoCamera.Mode.CenterOrbit);
            }
            else
            {
                _demoCamera.SetMode(BlockadeDemoCamera.Mode.SkyboxDefault);
            }
        }

        private void UpdateSpheres()
        {
            if (_meshCreator.activeSelf || !_showSpheresToggle.IsOn)
            {
                _spheres.SetActive(false);
                return;
            }

            _spheres.SetActive(true);

            foreach (var reflectionProbe in FindObjectsOfType<ReflectionProbe>())
            {
                reflectionProbe.mode = ReflectionProbeMode.Realtime;
                reflectionProbe.RenderProbe();
            }
        }

        private void OnStylePicked(SkyboxStyle style)
        {
            _anyStylePicked = true;
            _generator.SelectedStyle = style;
        }

        private void UpdateGenerateButton()
        {
            _generateButton.interactable = _generator.CurrentState != BlockadeLabsSkyboxGenerator.State.NeedApiKey;

            var tmpText = _generateButton.GetComponentInChildren<TMP_Text>();

            if (_generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Generating)
            {
                tmpText.text = "CANCEL";
            }
            else if (_generator.Remix)
            {
                tmpText.text = "REMIX THIS";
            }
            else
            {
                tmpText.text = "GENERATE";
            }
        }

        private void UpdateHintText()
        {
            var tooltipText = _modeTooltip.GetComponentInChildren<TextMeshProUGUI>();
            tooltipText.text = "";

            if (_createButton.GetComponent<Hoverable>().IsHovered)
            {
                _hintText.text = _createHint;
            }
            else if (_remixButton.GetComponent<Hoverable>().IsHovered)
            {
                _hintText.text = _remixHint;

                if (_generator.ModelVersion == SkyboxAiModelVersion.Model3)
                {
                    tooltipText.text = "Coming soon to SkyboxAI Model 3";
                    _modeTooltip.transform.SetParent(_remixButton.transform, false);
                }
            }
            else if (_editButton.GetComponent<Hoverable>().IsHovered)
            {
                _hintText.text = _editHint;

                if (_generator.ModelVersion == SkyboxAiModelVersion.Model3)
                {
                    tooltipText.text = "Coming soon to SkyboxAI Model 3";
                    _modeTooltip.transform.SetParent(_editButton.transform, false);
                }
                else
                {
                    tooltipText.text = "Coming soon";
                    _modeTooltip.transform.SetParent(_editButton.transform, false);
                }
            }
            else if (_meshCreatorButton.GetComponent<Hoverable>().IsHovered)
            {
                _hintText.text = _meshCreatorHint;

                if (!_skybox.HasDepthTexture && _generator.ModelVersion == SkyboxAiModelVersion.Model3)
                {
                    tooltipText.text = "Coming soon to SkyboxAI Model 3";
                    _modeTooltip.transform.SetParent(_meshCreatorButton.transform, false);
                }
            }
            else
            {
                _hintText.text = (_generator.CanRemix && _generator.Remix) ? _remixHint : _createHint;
            }

            _modeTooltip.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, tooltipText.GetPreferredValues().x + 20);
            _modeTooltip.SetActive(tooltipText.text != "");
        }

        private void UpdateVersionSelector()
        {
            bool isModel2 = _generator.ModelVersion == SkyboxAiModelVersion.Model2;
            bool isModel3 = _generator.ModelVersion == SkyboxAiModelVersion.Model3;

            _model2Button.gameObject.SetActive(!isModel2);
            _model3Button.gameObject.SetActive(!isModel3);
            _model2Selected.SetActive(isModel2);
            _model3Selected.SetActive(isModel3);
        }

        private async void UpdateTip()
        {
            if (_generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Generating)
            {
                var tip = await ApiRequests.GetSkyboxTipAsync(_generator.ApiKey, _generator.ModelVersion);
                _tipText.text = "<b>Tip:</b> " + tip.tip.Replace("<p>", "").Replace("</p>", "");
                _tipContainer.SetActive(true);
            }
            else
            {
                _tipContainer.SetActive(false);
            }
        }

        private void Update()
        {
            _createRemixUnderline.localPosition = Vector3.Lerp(_createRemixUnderline.localPosition,
                new Vector3(_generator.Remix ? _remixUnderlineOffset : _createUnderlineOffset,
                    _createRemixUnderline.localPosition.y,
                    _createRemixUnderline.localPosition.z), Time.deltaTime * 10);

            _progressBar.anchorMax = new Vector2(_generator.PercentageCompleted / 100f, 1);
        }
    }
}