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
        private TMP_Text _generateButton;
        public TMP_Text GenerateButton
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

            await _blockadeLabsSkybox.LoadAsync();
        }

        private void OnPropertyChanged()
        {
            _promptInput.text = _blockadeLabsSkybox.Prompt;
            _enhancePromptToggle.IsOn = _blockadeLabsSkybox.EnhancePrompt;
            _negativeTextInput.text = _blockadeLabsSkybox.NegativeText;
        }

        private void OnStateChanged()
        {
            // TODO: Let user know if they need to set the API key
            SetInteractable(_blockadeLabsSkybox.CurrentState == BlockadeLabsSkybox.State.Ready);

            if (_blockadeLabsSkybox.CurrentState == BlockadeLabsSkybox.State.Generating)
            {
                _generateButton.text = _blockadeLabsSkybox.PercentageCompleted() + "%";
            }
            else
            {
                _generateButton.text = "GENERATE";
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

        private void OnNegativeTextToggleChanged(bool newValue)
        {
            _blockadeLabsSkybox.NegativeText = newValue ? _negativeTextInput.text : "";
        }

        private void OnNegativeTextInputChanged(string newValue)
        {
            _blockadeLabsSkybox.NegativeText = _negativeTextToggle.IsOn ? newValue : "";
        }

        private void OnEnhancePromptToggleChanged(bool newValue)
        {
            _blockadeLabsSkybox.EnhancePrompt = newValue;
        }

        public void GenerateSkybox()
        {
            _blockadeLabsSkybox.GenerateSkyboxAsync(true);
        }
    }
}