using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class RuntimeGuiManager : MonoBehaviour
    {
        [SerializeField]
        private BlockadeLabsSkyboxGenerator _generator;
        public BlockadeLabsSkyboxGenerator Generator
        {
            get { return _generator; }
            set { _generator = value; }
        }

        [SerializeField]
        private BlockadeLabsSkybox _skybox;
        public BlockadeLabsSkybox Skybox
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
        private GameObject _viewButton;
        public GameObject ViewButton
        {
            get { return _viewButton; }
            set { _viewButton = value; }
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
        private GameObject _helloPopup;
        public GameObject HelloPopup
        {
            get { return _helloPopup; }
            set { _helloPopup = value; }
        }

        [SerializeField]
        private GameObject _helpPopup;
        public GameObject HelpPopup
        {
            get { return _helpPopup; }
            set { _helpPopup = value; }
        }

        [SerializeField]
        private GameObject _remixPopup;
        public GameObject RemixPopup
        {
            get { return _remixPopup; }
            set { _remixPopup = value; }
        }

        [SerializeField]
        private Toggle _remixDontShowAgainToggle;
        public Toggle RemixDontShowAgainToggle
        {
            get { return _remixDontShowAgainToggle; }
            set { _remixDontShowAgainToggle = value; }
        }

        [SerializeField]
        private RectTransform _progressBar;
        public RectTransform ProgressBar
        {
            get { return _progressBar; }
            set { _progressBar = value; }
        }

        [SerializeField]
        private GameObject _promptPanel;
        public GameObject PromptPanel
        {
            get { return _promptPanel; }
            set { _promptPanel = value; }
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

        private float _createUnderlineOffset;
        private bool _anyStylePicked;

        async void Start()
        {
            _helloPopup.SetActive(false);
            _helpPopup.SetActive(true);
            _viewButton.SetActive(true);
            _promptPanel.SetActive(true);

            // Initialize values
            _skybox.DepthScale = _depthScaleSlider.minValue;

            // Bind to property changes that will update the UI
            _generator.OnPropertyChanged += OnGeneratorPropertyChanged;
            OnGeneratorPropertyChanged();

            _skybox.OnPropertyChanged += OnSkyboxPropertyChanged;
            OnSkyboxPropertyChanged();

            _generator.OnStateChanged += OnStateChanged;
            OnStateChanged();

            _generator.OnErrorChanged += OnErrorChanged;
            OnErrorChanged();

            // Prompt Panel Controls
            _promptInput.onValueChanged.AddListener(OnPromptInputChanged);
            _negativeTextToggle.OnValueChanged.AddListener(OnNegativeTextToggleChanged);
            _negativeTextInput.onValueChanged.AddListener(OnNegativeTextInputChanged);
            _enhancePromptToggle.OnValueChanged.AddListener(OnEnhancePromptToggleChanged);
            _createButton.onClick.AddListener(OnCreateButtonClicked);
            _remixButton.onClick.AddListener(OnRemixButtonClicked);
            _createUnderlineOffset = _createRemixUnderline.localPosition.x;
            _createButton.GetComponent<Hoverable>().OnHoverChanged.AddListener((_) => UpdateHintText());
            _remixButton.GetComponent<Hoverable>().OnHoverChanged.AddListener((_) => UpdateHintText());
            _stylePickerPanel.OnStylePicked += OnStylePicked;
            _generateButton.onClick.AddListener(OnGenerateButtonClicked);
            _meshCreatorButton.onClick.AddListener(OnMeshCreatorButtonClicked);

            // Mesh Creator Controls
            _meshCreatorBackButton.onClick.AddListener(OnMeshCreatorBackButtonClicked);
            _lowDensityToggle.OnTurnedOn.AddListener(() => _skybox.MeshDensity = MeshDensity.Low);
            _mediumDensityToggle.OnTurnedOn.AddListener(() => _skybox.MeshDensity = MeshDensity.Medium);
            _highDensityToggle.OnTurnedOn.AddListener(() => _skybox.MeshDensity = MeshDensity.High);
            _epicDensityToggle.OnTurnedOn.AddListener(() => _skybox.MeshDensity = MeshDensity.Epic);
            _depthScaleSlider.onValueChanged.AddListener((_) => _skybox.DepthScale = _depthScaleSlider.value);

            await _generator.LoadAsync();
        }

        private void OnGeneratorPropertyChanged()
        {
            _promptInput.text = _generator.Prompt;
            _enhancePromptToggle.IsOn = _generator.EnhancePrompt;
            _negativeTextInput.text = _generator.NegativeText;
            UpdateHintText();
            UpdateGenerateButton();
            UpdatePromptCharacterLimit();
            UpdateNegativeTextCharacterLimit();
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
        }

        private void UpdateCanRemix()
        {
            bool canRemix = _generator.CanRemix;
            _remixButton.interactable = _generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready && canRemix;
            _remixButton.GetComponentInChildren<DisabledColor>().Disabled = !_remixButton.interactable;

            if (!canRemix)
            {
                _generator.Remix = false;
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
                _helpPopup.SetActive(false);
                _remixPopup.SetActive(false);
                _errorPopup.SetActive(true);
                _errorText.text = _generator.LastError;
            }
            else
            {
                _errorPopup.SetActive(false);
            }
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
            _remixPopup.SetActive(false);
        }

        private void OnRemixButtonClicked()
        {
            _generator.Remix = true;
            if (!_remixDontShowAgainToggle.isOn)
            {
                _remixPopup.SetActive(true);
            }
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
            _demoCamera.SetZoom(-0.5f);
            _promptPanel.SetActive(false);
            _meshCreator.SetActive(true);
        }

        private void OnMeshCreatorBackButtonClicked()
        {
            StartCoroutine(CoAnimateDepthScale(_depthScaleSlider.minValue));
            _demoCamera.SetZoom(0.0f);
            _promptPanel.SetActive(true);
            _meshCreator.SetActive(false);
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
            if (_createButton.GetComponent<Hoverable>().IsHovered)
            {
                _hintText.text = _createHint;
            }
            else if (_remixButton.GetComponent<Hoverable>().IsHovered)
            {
                _hintText.text = _remixHint;
            }
            else
            {
                _hintText.text = _generator.Remix ? _remixHint : _createHint;
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