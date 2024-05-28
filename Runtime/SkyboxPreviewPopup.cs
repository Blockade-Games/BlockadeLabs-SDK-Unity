using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class SkyboxPreviewPopup : MonoBehaviour
    {
        [SerializeField]
        private RuntimeGuiManager _runtimeGuiManager;

        [SerializeField]
        private TextMeshProUGUI _titleText;

        [SerializeField]
        private RawImage _skyboxPreviewImage;

        [SerializeField]
        private TextMeshProUGUI _statusText;

        [SerializeField]
        private TextMeshProUGUI _promptText;

        [SerializeField]
        private TextMeshProUGUI _depthMapText;

        [SerializeField]
        private TextMeshProUGUI _seedText;

        [SerializeField]
        private TextMeshProUGUI _styleText;

        [SerializeField]
        private TextMeshProUGUI _typeText;

        [SerializeField]
        private TextMeshProUGUI _dateCompletedText;

        [SerializeField]
        private Button _viewButton;

        [SerializeField]
        private Button _closeButton;

        private ImagineResult _imagineResult;

        private void OnEnable()
        {
            _viewButton.onClick.AddListener(OnViewButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDisable()
        {
            _viewButton.onClick.RemoveListener(OnViewButtonClicked);
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        private void OnViewButtonClicked()
        {
            // TODO set skybox as active in main scene
            // TODO set imagine parameters to generate properties
            Debug.Log("On view skybox");
            gameObject.SetActive(false);
            _runtimeGuiManager.ToggleHistoryPanel();
        }

        private void OnCloseButtonClicked()
            => gameObject.SetActive(false);

        internal void ShowPreviewPopup(ImagineResult imagineResult, Texture preview)
        {
            _imagineResult = imagineResult;
            _titleText.text = $"World #{imagineResult.id}";
            _skyboxPreviewImage.texture = preview;
            _statusText.text = $"Status: <color=\"white\">{imagineResult.status}</color>";
            _promptText.text = $"Prompt: <color=\"white\">{imagineResult.prompt}</color>";
            _depthMapText.text = $"Depth Map: <color=\"white\">{(string.IsNullOrWhiteSpace(imagineResult.depth_map_url) ? "Off" : "On")}</color>";
            _seedText.text = $"Seed: <color=\"white\">{imagineResult.seed}</color>";
            _styleText.text = $"Style: <color=\"white\">{imagineResult.skybox_style_name}</color>";
            _typeText.text = $"Type: <color=\"white\">{imagineResult.type}</color>";
            _dateCompletedText.text = $"Date Completed: <color=\"white\">{imagineResult.completed_at:d}</color>";
            gameObject.SetActive(true);
        }
    }
}