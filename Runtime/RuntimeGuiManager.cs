using TMPro;
using UnityEngine;

namespace BlockadeLabsSDK
{
    public class RuntimeGuiManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField promptInput;
        [SerializeField] private TMP_Dropdown stylesDropdown;
        [SerializeField] private TMP_Text generateButton;
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

            blockadeLabsSkybox.skyboxStyleFields[0].value = promptInput.text;

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