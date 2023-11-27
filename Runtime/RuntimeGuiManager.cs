using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class RuntimeGuiManager : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _promptInput;
        public TMP_InputField PromptInput
        {
            get { return _promptInput; }
            set { _promptInput = value; }
        }

        [SerializeField]
        private TMP_Dropdown _stylesDropdown;
        public TMP_Dropdown StylesDropdown
        {
            get { return _stylesDropdown; }
            set { _stylesDropdown = value; }
        }

        [SerializeField]
        private TMP_Text _generateButton;
        public TMP_Text GenerateButton
        {
            get { return _generateButton; }
            set { _generateButton = value; }
        }

        [SerializeField]
        private Toggle _enhancePromptToggle;
        public Toggle EnhancePromptToggle
        {
            get { return _enhancePromptToggle; }
            set { _enhancePromptToggle = value; }
        }

        [SerializeField]
        private GameObject _popupPanel;
        public GameObject PopupPanel
        {
            get { return _popupPanel; }
            set { _popupPanel = value; }
        }

        [SerializeField]
        private BlockadeLabsSkybox _blockadeLabsSkybox;
        public BlockadeLabsSkybox BlockadeLabsSkybox
        {
            get { return _blockadeLabsSkybox; }
            set { _blockadeLabsSkybox = value; }
        }

        private bool _initialized = false;

        void Awake()
        {
            if (_blockadeLabsSkybox == null)
            {
                Debug.LogError("BlockadeLabsSkybox must be set.");
                return;
            }

            if (_promptInput == null)
            {
                Debug.LogError("PromptInput must be set.");
                return;
            }

            if (_stylesDropdown == null)
            {
                Debug.LogError("StylesDropdown must be set.");
                return;
            }

            if (_generateButton == null)
            {
                Debug.LogError("GenerateButton must be set.");
                return;
            }

            if (_enhancePromptToggle == null)
            {
                Debug.LogError("EnhancePromptToggle must be set.");
                return;
            }

            if (_popupPanel == null)
            {
                Debug.LogError("PopupPanel must be set.");
                return;
            }

            if (!_blockadeLabsSkybox.CheckApiKeyValid())
            {
                Debug.LogError("API key is not valid.");
                return;
            }

            _initialized = true;
        }

        async void Start()
        {
            if (!_initialized)
            {
                return;
            }

            await _blockadeLabsSkybox.LoadOptionsAsync();

            // foreach (var skyboxStyle in _blockadeLabsSkybox.SkyboxStyles)
            // {
            //     _stylesDropdown.options.Add(new TMP_Dropdown.OptionData() { text = skyboxStyle.name });
            // }

            _enhancePromptToggle.onValueChanged.AddListener(OnTargetToggleValueChanged);
        }

        void OnTargetToggleValueChanged(bool newValue) {
            Image targetImage = _enhancePromptToggle.targetGraphic as Image;
            Image targetCheckmarkImage = _enhancePromptToggle.graphic as Image;

            if (targetImage != null && targetCheckmarkImage != null)
            {
                if (newValue)
                {
                    targetImage.enabled = false;
                    targetCheckmarkImage.enabled = true;
                }
                else
                {
                    targetImage.enabled = true;
                    targetCheckmarkImage.enabled = false;
                }
            }
        }

        private void Update()
        {
            SetGenerateButtonText();
        }

        private void SetGenerateButtonText()
        {
            if (_blockadeLabsSkybox.PercentageCompleted() >= 0 && _blockadeLabsSkybox.PercentageCompleted() < 100)
            {
                _generateButton.text = _blockadeLabsSkybox.PercentageCompleted() + "%";
            }
            else
            {
                _generateButton.text = "GENERATE";
            }
        }

        public void GenerateSkybox()
        {
            if (_blockadeLabsSkybox.PercentageCompleted() >= 0 && _blockadeLabsSkybox.PercentageCompleted() < 100) return;

            // set prompt
            var prompt = _blockadeLabsSkybox.SkyboxStyleFields.First(
                skyboxStyleField => skyboxStyleField.key == "prompt"
            );

            // set enhance_prompt
            var enhancePrompt = _blockadeLabsSkybox.SkyboxStyleFields.First(
                skyboxStyleField => skyboxStyleField.key == "enhance_prompt"
            );

            prompt.value = _promptInput.text;
            enhancePrompt.value = _enhancePromptToggle.isOn ? "true" : "false";

            if (_stylesDropdown.value > 0)
            {
                _blockadeLabsSkybox.GenerateSkyboxAsync(true);
            }
        }

        public void TogglePopup()
        {
            _popupPanel.SetActive(!_popupPanel.activeInHierarchy);
        }
    }
}