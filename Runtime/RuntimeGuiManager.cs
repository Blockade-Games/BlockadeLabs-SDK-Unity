using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class RuntimeGuiManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField promptInput;
        [SerializeField] private TMP_Dropdown stylesDropdown;
        [SerializeField] private TMP_Text generateButton;
        [SerializeField] private Toggle enhancePromptToggle;
        [SerializeField] private GameObject popupPanel;
        private BlockadeLabsSkybox blockadeLabsSkybox;

        async void Start()
        {
            blockadeLabsSkybox = FindObjectOfType<BlockadeLabsSkybox>();

            await blockadeLabsSkybox.GetSkyboxStyleOptions();

            foreach (var skyboxStyle in blockadeLabsSkybox.skyboxStyles)
            {
                stylesDropdown.options.Add(new TMP_Dropdown.OptionData() { text = skyboxStyle.name });
            }
            
            enhancePromptToggle.onValueChanged.AddListener(OnTargetToggleValueChanged);
            Debug.Log(enhancePromptToggle);
        }
        
        void OnTargetToggleValueChanged(bool newValue) {
            Image targetImage = enhancePromptToggle.targetGraphic as Image;
            Image targetCheckmarkImage = enhancePromptToggle.graphic as Image;

            if (targetImage != null && targetCheckmarkImage != null) {
                if (newValue) {
                    targetImage.enabled = false;
                    targetCheckmarkImage.enabled = true;
                } else {
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
            if (blockadeLabsSkybox.PercentageCompleted() >= 0 && blockadeLabsSkybox.PercentageCompleted() < 100)
            {
                generateButton.text = blockadeLabsSkybox.PercentageCompleted() + "%";
            }
            else
            {
                generateButton.text = "GENERATE";
            }
        }
        
        public void GenerateSkybox()
        {
            if (blockadeLabsSkybox.PercentageCompleted() >= 0 && blockadeLabsSkybox.PercentageCompleted() < 100) return;

            // set prompt
            var prompt = blockadeLabsSkybox.skyboxStyleFields.First(
                skyboxStyleField => skyboxStyleField.key == "prompt"
            );
            
            // set enhance_prompt
            var enhancePrompt = blockadeLabsSkybox.skyboxStyleFields.First(
                skyboxStyleField => skyboxStyleField.key == "enhance_prompt"
            );
            
            prompt.value = promptInput.text;
            enhancePrompt.value = enhancePromptToggle.isOn ? "true" : "false";
            
            if (stylesDropdown.value > 0)
            {
                _ = blockadeLabsSkybox.CreateSkybox(
                    blockadeLabsSkybox.skyboxStyleFields,
                    blockadeLabsSkybox.skyboxStyles[stylesDropdown.value - 1].id,
                    true
                );
            }
        }

        public void TogglePopup()
        {
            popupPanel.SetActive(!popupPanel.activeInHierarchy);
        }
    }
}