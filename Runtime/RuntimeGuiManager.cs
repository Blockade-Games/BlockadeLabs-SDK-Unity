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
        private BlockadeImaginarium blockadeImaginarium;

        async void Start()
        {
            blockadeImaginarium = FindObjectOfType<BlockadeImaginarium>();
            await blockadeImaginarium.GetSkyboxStyleOptions();

            foreach (var skyboxStyle in blockadeImaginarium.skyboxStyles)
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
            if (blockadeImaginarium.PercentageCompleted() >= 0 && blockadeImaginarium.PercentageCompleted() < 100)
            {
                generateButton.text = blockadeImaginarium.PercentageCompleted() + "%";
            }
            else
            {
                generateButton.text = "GENERATE";
            }
        }
        
        public void GenerateSkybox()
        {
            if (blockadeImaginarium.PercentageCompleted() >= 0 && blockadeImaginarium.PercentageCompleted() < 100) return;

            blockadeImaginarium.skyboxStyleFields[0].value = promptInput.text;

            if (stylesDropdown.value > 0)
            {
                _ = blockadeImaginarium.InitializeSkyboxGeneration(
                    blockadeImaginarium.skyboxStyleFields,
                    blockadeImaginarium.skyboxStyles[stylesDropdown.value - 1].id,
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