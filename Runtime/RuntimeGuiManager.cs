using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class RuntimeGuiManager : MonoBehaviour
    {
        [SerializeField]
        private BlockadeLabsSkybox _blockadeLabsSkybox;
        public BlockadeLabsSkybox BlockadeLabsSkybox
        {
            get { return _blockadeLabsSkybox; }
            set { _blockadeLabsSkybox = value; }
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
        private GameObject _remixPopup;
        public GameObject RemixPopup
        {
            get { return _remixPopup; }
            set { _remixPopup = value; }
        }

        [SerializeField]
        private Toggle _remixPopupToggle;
        public Toggle RemixPopupToggle
        {
            get { return _remixPopupToggle; }
            set { _remixPopupToggle = value; }
        }

        [SerializeField]
        private RectTransform _progressBar;
        public RectTransform ProgressBar
        {
            get { return _progressBar; }
            set { _progressBar = value; }
        }

        private float _createUnderlineOffset;
        private bool _anyStylePicked;

        async void Start()
        {
            _blockadeLabsSkybox.OnPropertyChanged += OnPropertyChanged;
            OnPropertyChanged();

            _blockadeLabsSkybox.OnStateChanged += OnStateChanged;
            OnStateChanged();

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

            await _blockadeLabsSkybox.LoadAsync();
        }

        private void OnPropertyChanged()
        {
            _promptInput.text = _blockadeLabsSkybox.Prompt;
            _enhancePromptToggle.IsOn = _blockadeLabsSkybox.EnhancePrompt;
            _negativeTextInput.text = _blockadeLabsSkybox.NegativeText;
            UpdateHintText();
            UpdateGenerateButtonText();
            UpdatePromptCharacterLimit();
            UpdateNegativeTextCharacterLimit();
            _promptCharacterWarning.SetActive(false);
            _negativeTextCharacterWarning.SetActive(false);

            if (_anyStylePicked)
            {
                _selectedStyleText.text = _blockadeLabsSkybox.SelectedStyle?.name ?? "Select a Style";
                _stylePickerPanel.SetSelectedStyle(_blockadeLabsSkybox.SelectedStyleFamily, _blockadeLabsSkybox.SelectedStyle);
            }
        }

        private void OnStateChanged()
        {
            // TODO: Let user know if they need to set the API key
            SetInteractable(_blockadeLabsSkybox.CurrentState == BlockadeLabsSkybox.State.Ready);
            UpdateGenerateButtonText();

            if (_blockadeLabsSkybox.CurrentState == BlockadeLabsSkybox.State.Ready)
            {
                _stylePickerPanel.SetStyles(_blockadeLabsSkybox.StyleFamilies);
            }
        }

        private void SetInteractable(bool interactable)
        {
            var selectables = GetComponentsInChildren<Selectable>();
            foreach (var selectable in selectables)
            {
                selectable.interactable = interactable;
            }
        }

        private void OnPromptInputChanged(string newValue)
        {
            _blockadeLabsSkybox.Prompt = newValue;
        }

        private void UpdatePromptCharacterLimit()
        {
            if (_blockadeLabsSkybox.SelectedStyle != null)
            {
                _promptCharacterLimit.text = _blockadeLabsSkybox.Prompt.Length + "/" + _blockadeLabsSkybox.SelectedStyle.maxChar;
                _promptCharacterLimit.color = _blockadeLabsSkybox.Prompt.Length > _blockadeLabsSkybox.SelectedStyle.maxChar ? Color.red : Color.white;
            }
            else
            {
                _promptCharacterLimit.text = "";
            }
        }

        private void OnNegativeTextToggleChanged(bool newValue)
        {
            _blockadeLabsSkybox.NegativeText = newValue ? _negativeTextInput.text : "";
        }

        private void UpdateNegativeTextCharacterLimit()
        {
            if (_blockadeLabsSkybox.SelectedStyle != null)
            {
                _negativeTextCharacterLimit.text = _blockadeLabsSkybox.NegativeText.Length + "/" + _blockadeLabsSkybox.SelectedStyle.negativeTextMaxChar;
                _negativeTextCharacterLimit.color = _blockadeLabsSkybox.NegativeText.Length > _blockadeLabsSkybox.SelectedStyle.negativeTextMaxChar ? Color.red : Color.white;
            }
            else
            {
                _negativeTextCharacterLimit.text = "";
            }
        }

        private void OnNegativeTextInputChanged(string newValue)
        {
            _blockadeLabsSkybox.NegativeText = _negativeTextToggle.IsOn ? newValue : "";
        }

        private void OnEnhancePromptToggleChanged(bool newValue)
        {
            _blockadeLabsSkybox.EnhancePrompt = newValue;
        }

        private void OnCreateButtonClicked()
        {
            _blockadeLabsSkybox.Remix = false;
            _remixPopup.SetActive(false);
        }

        private void OnRemixButtonClicked()
        {
            _blockadeLabsSkybox.Remix = true;
            if (!_remixPopupToggle.isOn)
            {
                _remixPopup.SetActive(true);
            }
        }

        private void OnGenerateButtonClicked()
        {
            if (_blockadeLabsSkybox.CurrentState == BlockadeLabsSkybox.State.Generating)
            {
                _blockadeLabsSkybox.Cancel();
                return;
            }

            if (_blockadeLabsSkybox.Prompt.Length == 0)
            {
                _promptCharacterWarning.SetActive(true);
                _promptCharacterWarning.GetComponentInChildren<TMP_Text>().text = "Prompt cannot be empty";
                return;
            }

            if (_blockadeLabsSkybox.Prompt.Length > _blockadeLabsSkybox.SelectedStyle.maxChar)
            {
                _promptCharacterWarning.SetActive(true);
                _promptCharacterWarning.GetComponentInChildren<TMP_Text>().text = $"Prompt should be {_blockadeLabsSkybox.SelectedStyle.maxChar} characters or less";
                return;
            }

            if (_blockadeLabsSkybox.NegativeText.Length > _blockadeLabsSkybox.SelectedStyle.negativeTextMaxChar)
            {
                _negativeTextCharacterWarning.SetActive(true);
                _negativeTextCharacterWarning.GetComponentInChildren<TMP_Text>().text = $"Negative text should be {_blockadeLabsSkybox.SelectedStyle.negativeTextMaxChar} characters or less";
                return;
            }

            if (!_anyStylePicked)
            {
                _stylePickerPanel.gameObject.SetActive(true);
                return;
            }

            _blockadeLabsSkybox.GenerateSkyboxAsync(true);
        }

        private void OnStylePicked(SkyboxStyle style)
        {
            _anyStylePicked = true;
            _blockadeLabsSkybox.SelectedStyle = style;
        }

        private void UpdateGenerateButtonText()
        {
            var tmpText = _generateButton.GetComponentInChildren<TMP_Text>();

            if (_blockadeLabsSkybox.CurrentState == BlockadeLabsSkybox.State.Generating)
            {
                tmpText.text = "CANCEL";
            }
            else if (_blockadeLabsSkybox.Remix)
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
                _hintText.text = _blockadeLabsSkybox.Remix ? _remixHint : _createHint;
            }
        }

        private void Update()
        {
            _createRemixUnderline.localPosition = Vector3.Lerp(_createRemixUnderline.localPosition,
                new Vector3(_blockadeLabsSkybox.Remix ? _remixUnderlineOffset : _createUnderlineOffset,
                    _createRemixUnderline.localPosition.y,
                    _createRemixUnderline.localPosition.z), Time.deltaTime * 10);

            _progressBar.anchorMax = new Vector2(_blockadeLabsSkybox.PercentageCompleted / 100f, 1);
        }
    }
}