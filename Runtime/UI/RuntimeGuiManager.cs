using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
        [FormerlySerializedAs("_skybox")]
        private BlockadeLabsSkyboxMesh _skyboxMesh;
        public BlockadeLabsSkyboxMesh SkyboxMesh
        {
            get { return _skyboxMesh; }
            set { _skyboxMesh = value; }
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
        private GameObject _remixImagePanel;
        public GameObject RemixImagePanel
        {
            get { return _remixImagePanel; }
            set { _remixImagePanel = value; }
        }

        [SerializeField]
        private RawImage _remixImage;
        public RawImage RemixImage
        {
            get { return _remixImage; }
            set { _remixImage = value; }
        }

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
        private Toggle _likeToggle;
        public Toggle LikeToggle
        {
            get { return _likeToggle; }
            set { _likeToggle = value; }
        }

        [SerializeField]
        private Button _uploadButton;
        public Button UploadButton
        {
            get { return _uploadButton; }
            set { _uploadButton = value; }
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
            _skyboxMesh.BakedMesh = null;
            _skyboxMesh.MeshDensity = MeshDensity.Medium;
            _skyboxMesh.DepthScale = _depthScaleSlider.minValue;
            _generator.SendNegativeText = !string.IsNullOrWhiteSpace(_generator.NegativeText);

            // Bind to property changes that will update the UI
            _generator.OnPropertyChanged += OnGeneratorPropertyChanged;
            OnGeneratorPropertyChanged();

            _skyboxMesh.OnPropertyChanged += OnSkyboxPropertyChanged;
            OnSkyboxPropertyChanged();

            _skyboxMesh.OnLoadingChanged += OnSkyboxLoadingChanged;

            _generator.OnStateChanged += OnStateChanged;
            OnStateChanged(_generator.CurrentState);

            _generator.OnErrorChanged += OnErrorChanged;
            OnErrorChanged();

            // Titlebar
            _model2Button.onClick.AddListener(() => SetModelVersion(SkyboxModel.Model2));
            _model3Button.onClick.AddListener(() => SetModelVersion(SkyboxModel.Model3));

            // Prompt Panel Controls
            _promptInput.onValueChanged.AddListener(OnPromptInputChanged);
            _negativeTextToggle.OnValueChanged.AddListener(OnNegativeTextToggleChanged);
            _negativeTextInput.onValueChanged.AddListener(OnNegativeTextInputChanged);
            _enhancePromptToggle.OnValueChanged.AddListener(OnEnhancePromptToggleChanged);
            _createButton.onClick.AddListener(OnCreateButtonClicked);
            _remixButton.onClick.AddListener(OnRemixButtonClicked);
            _likeToggle.onValueChanged.AddListener(OnLikeToggle);
            _uploadButton.onClick.AddListener(OnRemixUpload);
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
            _lowDensityToggle.OnTurnedOn.AddListener(() => _skyboxMesh.MeshDensity = MeshDensity.Low);
            _mediumDensityToggle.OnTurnedOn.AddListener(() => _skyboxMesh.MeshDensity = MeshDensity.Medium);
            _highDensityToggle.OnTurnedOn.AddListener(() => _skyboxMesh.MeshDensity = MeshDensity.High);
            _epicDensityToggle.OnTurnedOn.AddListener(() => _skyboxMesh.MeshDensity = MeshDensity.Epic);
            _depthScaleSlider.onValueChanged.AddListener(_ => _skyboxMesh.DepthScale = _depthScaleSlider.value);
            _savePrefabButton.onClick.AddListener(OnSavePrefabButtonClicked);

            // History Panel Controls
            _historyButton.onClick.AddListener(ToggleHistoryPanel);
        }

        private void OnEnable()
        {
            _generator.Load();
        }

        private void OnGeneratorPropertyChanged()
        {
            UpdateVersionSelector();
            _promptInput.text = _generator.Prompt;
            _enhancePromptToggle.IsOn = _generator.EnhancePrompt;
            _negativeTextToggle.IsOn = _generator.SendNegativeText;
            _negativeTextInput.text = _generator.NegativeText;

            _stylePickerPanel.SetStyles(_generator.StyleFamilies);
            UpdateHintText();
            UpdateGenerateButton();
            UpdatePromptCharacterLimit();
            UpdateNegativeTextCharacterLimit();
            UpdateCanRemix();
            UpdateLikeToggle();
            UpdateRemixPanel();
            UpdateSpheres();

            _promptCharacterWarning.SetActive(false);
            _negativeTextCharacterWarning.SetActive(false);

            _selectedStyleText.text = _generator.SelectedStyle?.Name ?? "Select a Style";
            _stylePickerPanel.SetSelectedStyle(_generator.SelectedStyle);
        }

        private void OnSkyboxPropertyChanged()
        {
            _lowDensityToggle.IsOn = _skyboxMesh.MeshDensity == MeshDensity.Low;
            _mediumDensityToggle.IsOn = _skyboxMesh.MeshDensity == MeshDensity.Medium;
            _highDensityToggle.IsOn = _skyboxMesh.MeshDensity == MeshDensity.High;
            _epicDensityToggle.IsOn = _skyboxMesh.MeshDensity == MeshDensity.Epic;
            _depthScaleSlider.value = _skyboxMesh.DepthScale;
#if !UNITY_EDITOR
            _savePrefabButton.gameObject.SetActive(false);
#endif
        }

        private void OnSkyboxLoadingChanged(bool isLoading)
        {
            _loadingPopup.SetActive(isLoading);
            _loadingPopupText.text = _skyboxMesh.LoadingText;
        }

        private void OnStateChanged(BlockadeLabsSkyboxGenerator.State state)
        {
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
            UpdateLikeToggle();
            UpdateTip();

            _historyButton.interactable = _generator.CurrentState != BlockadeLabsSkyboxGenerator.State.NeedApiKey;

            if (_generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready)
            {
                _demoCamera.ResetRotation();
                UpdateSpheres();
            }
        }

        private void UpdateCanRemix()
        {
            _remixButton.interactable = _generator.CanRemix && _generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready;

            foreach (var disabledColor in _remixButton.GetComponentsInChildren<DisabledColor>())
            {
                disabledColor.Disabled = !_remixButton.interactable;
            }
        }

        private void UpdateMeshCreatorButton()
        {
            _meshCreatorButton.interactable = _skyboxMesh.HasDepthTexture && _generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready;

            foreach (var disabledColor in _meshCreatorButton.GetComponentsInChildren<DisabledColor>())
            {
                disabledColor.Disabled = !_meshCreatorButton.interactable;
            }
        }

        private async void UpdateLikeToggle()
        {
            _likeToggle.interactable = _generator.HasSkyboxMetadata && _generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready;
            _likeToggle.isOn = false;

            // get the latest favorite status
            if (_generator.HasSkyboxMetadata)
            {
                var skybox = await BlockadeLabsSkyboxGenerator.BlockadeLabsClient.SkyboxEndpoint.GetSkyboxInfoAsync(_generator.SkyboxMesh.SkyboxAsset.Id);
                _likeToggle.SetIsOnWithoutNotify(skybox.IsMyFavorite);
            }
        }

        private void UpdateRemixPanel()
        {
            _uploadButton.gameObject.SetActive(Application.isEditor);
            _uploadButton.interactable = _generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready;

            foreach (var chile in _uploadButton.GetComponentsInChildren<DisabledColor>())
            {
                chile.Disabled = !_uploadButton.interactable;
            }

            _remixImagePanel.SetActive(_generator.Remix && _generator.RemixImage != null);
            _remixImage.texture = _generator.RemixImage;

            if (_generator.RemixImage != null)
            {
                var aspectRatioFitter = _remixImage.GetComponent<AspectLayoutElement>();
                aspectRatioFitter.heightToWidth = _generator.RemixImage.height / (float)_generator.RemixImage.width;
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

        private void SetModelVersion(SkyboxModel version)
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
                _promptCharacterLimit.text = _generator.Prompt.Length + "/" + _generator.SelectedStyle.MaxChar;
                _promptCharacterLimit.color = _generator.Prompt.Length > _generator.SelectedStyle.MaxChar ? Color.red : Color.white;
            }
            else
            {
                _promptCharacterLimit.text = "";
            }
        }

        private void OnNegativeTextToggleChanged(bool newValue)
        {
            _generator.SendNegativeText = newValue;
        }

        private void UpdateNegativeTextCharacterLimit()
        {
            if (_generator.SelectedStyle != null)
            {
                _negativeTextCharacterLimit.text = _generator.NegativeText.Length + "/" + _generator.SelectedStyle.NegativeTextMaxChar;
                _negativeTextCharacterLimit.color = _generator.NegativeText.Length > _generator.SelectedStyle.NegativeTextMaxChar ? Color.red : Color.white;
            }
            else
            {
                _negativeTextCharacterLimit.text = "";
            }
        }

        private void OnNegativeTextInputChanged(string newValue)
        {
            _generator.NegativeText = newValue;
        }

        private void OnEnhancePromptToggleChanged(bool newValue)
        {
            _generator.EnhancePrompt = newValue;
        }

        private void OnCreateButtonClicked()
        {
            _generator.Remix = false;
            _generator.ViewRemixImage = false;
        }

        private void OnRemixButtonClicked()
        {
            _generator.Remix = true;
        }

        private async void OnLikeToggle(bool newValue)
        {
            var result = await BlockadeLabsSkyboxGenerator.BlockadeLabsClient.SkyboxEndpoint.ToggleFavoriteAsync(_skyboxMesh.SkyboxAsset.Id);
            _likeToggle.SetIsOnWithoutNotify(result.IsMyFavorite);
        }

        private async void OnRemixUpload()
        {
#if UNITY_EDITOR
            _remixImagePanel.SetActive(false);
            var remixFilePath = EditorUtility.OpenFilePanel("Select Remix Image", string.Empty, "png,jpg,jpeg");

            if (!string.IsNullOrWhiteSpace(remixFilePath))
            {
                var texture = await Rest.DownloadTextureAsync($"file://{remixFilePath}");
                _generator.Remix = true;
                _generator.RemixImage = texture;
                _demoCamera.ResetRotation();

                // todo if aspect ratio isn't 2:1 show popup warning
            }
#else
            await System.Threading.Tasks.Task.CompletedTask;
#endif
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

            if (_generator.SelectedStyle == null)
            {
                _stylePickerPanel.gameObject.SetActive(true);
                return;
            }

            if (_generator.Prompt.Length > _generator.SelectedStyle.MaxChar)
            {
                _promptCharacterWarning.SetActive(true);
                _promptCharacterWarning.GetComponentInChildren<TMP_Text>().text = $"Prompt should be {_generator.SelectedStyle.MaxChar} characters or less";
                return;
            }

            if (_generator.NegativeText.Length > _generator.SelectedStyle.NegativeTextMaxChar)
            {
                _negativeTextCharacterWarning.SetActive(true);
                _negativeTextCharacterWarning.GetComponentInChildren<TMP_Text>().text = $"Negative text should be {_generator.SelectedStyle.NegativeTextMaxChar} characters or less";
                return;
            }

            _generator.GenerateSkybox();
        }

        private IEnumerator CoAnimateDepthScale(float target)
        {
            var start = _skyboxMesh.DepthScale;
            var time = 0.0f;
            var duration = 0.1f;

            while (time < duration)
            {
                time += Time.deltaTime;
                _skyboxMesh.DepthScale = Mathf.Lerp(start, target, time / duration);
                yield return null;
            }

            _skyboxMesh.DepthScale = target;
        }

        private void OnMeshCreatorButtonClicked()
        {
            StartCoroutine(CoAnimateDepthScale(_depthScaleSlider.minValue + (_depthScaleSlider.maxValue - _depthScaleSlider.minValue) / 3f));
            _promptPanel.SetActive(false);
            _meshCreator.SetActive(true);
            _generator.Remix = false;
            _generator.ViewRemixImage = false;
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
            StartCoroutine(CoSavePrefab());
        }

        private IEnumerator CoSavePrefab()
        {
            if (_skyboxMesh.HasDepthTexture)
            {
                _skyboxMesh.BakeMesh();

                while (_skyboxMesh.BakedMesh == null)
                {
                    yield return null;
                }
            }

            _skyboxMesh.SavePrefab();
            _skyboxMesh.BakedMesh = null;
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

#if UNITY_2023_1_OR_NEWER
            foreach (var reflectionProbe in FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None))
            {
                reflectionProbe.mode = ReflectionProbeMode.Realtime;
                reflectionProbe.RenderProbe();
            }
#else
            foreach (var reflectionProbe in FindObjectsOfType<ReflectionProbe>())
            {
                reflectionProbe.mode = ReflectionProbeMode.Realtime;
                reflectionProbe.RenderProbe();
            }
#endif
        }

        private void OnStylePicked(SkyboxStyle style)
        {
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
            }
            else if (_editButton.GetComponent<Hoverable>().IsHovered)
            {
                _hintText.text = _editHint;

                if (_generator.ModelVersion == SkyboxModel.Model3)
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
            }
            else
            {
                _hintText.text = (_generator.HasSkyboxMetadata && _generator.Remix) ? _remixHint : _createHint;
            }

            _modeTooltip.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tooltipText.GetPreferredValues().x + 20);
            _modeTooltip.SetActive(tooltipText.text != "");
        }

        private void UpdateVersionSelector()
        {
            var isModel2 = _generator.ModelVersion == SkyboxModel.Model2;
            var isModel3 = _generator.ModelVersion == SkyboxModel.Model3;

            _model2Button.gameObject.SetActive(!isModel2);
            _model3Button.gameObject.SetActive(!isModel3);
            _model2Selected.SetActive(isModel2);
            _model3Selected.SetActive(isModel3);
        }

        private async void UpdateTip()
        {
            if (_generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Generating)
            {
                try
                {
                    var tip = await BlockadeLabsSkyboxGenerator.BlockadeLabsClient.SkyboxEndpoint.GetOneTipAsync();

                    if (!string.IsNullOrWhiteSpace(tip))
                    {
                        _tipText.text = "<b>Tip:</b> " + tip.Replace("<p>", "").Replace("</p>", "");
                        _tipContainer.SetActive(true);
                    }
                }
                catch (Exception e)
                {
                    switch (e)
                    {
                        case RestException restException:
                            if (restException.RestResponse.Body.Contains("There is no skybox tips!"))
                            {
                                return;
                            }
                            Debug.LogException(restException);
                            break;
                        default:
                            Debug.LogException(e);
                            break;
                    }
                }
            }
            else
            {
                _tipContainer.SetActive(false);
            }
        }

        public void ToggleHistoryPanel()
        {
            _generator.Remix = false;
            _generator.ViewRemixImage = false;
            _historyPanel.gameObject.SetActive(!_historyPanel.gameObject.activeSelf);
            _viewButton.gameObject.SetActive(!_historyPanel.gameObject.activeSelf);
            _versionSelector.gameObject.SetActive(!_historyPanel.gameObject.activeSelf);
            _bottomSection.SetActive(!_historyPanel.gameObject.activeSelf);
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