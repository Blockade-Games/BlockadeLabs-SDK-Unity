﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class SkyboxPreviewPopup : MonoBehaviour
    {
        [SerializeField]
        private BlockadeDemoCamera _previewCamera;

        [SerializeField]
        private Renderer _previewSkyboxRenderer;

        [SerializeField]
        private Shader _skyboxShader;

        [SerializeField]
        private RuntimeGuiManager _runtimeGuiManager;

        [SerializeField]
        private TextMeshProUGUI _titleText;

        [SerializeField]
        private RawImage _depthPreviewImage;

        [SerializeField]
        private Toggle _likeToggle;

        [SerializeField]
        private TextMeshProUGUI _statusText;

        [SerializeField]
        private TextMeshProUGUI _promptText;

        [SerializeField]
        private TextMeshProUGUI _negativeTextTitle;

        [SerializeField]
        private ScrollRect _negativePromptScrollRect;

        [SerializeField]
        private TextMeshProUGUI _negativePromptText;

        [SerializeField]
        private TextMeshProUGUI _depthMapText;

        [SerializeField]
        private TextMeshProUGUI _depthMapStatusText;

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
        private Material _previewMaterial;

        private Material PreviewMaterial
        {
            get
            {
                if (_previewMaterial == null)
                {
                    _previewMaterial = new Material(_skyboxShader)
                    {
                        name = "Skybox Preview Material"
                    };
                    _previewSkyboxRenderer.sharedMaterial = _previewMaterial;
                }

                return _previewMaterial;
            }
        }

        private void OnEnable()
        {
            _viewButton.onClick.AddListener(OnViewButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            _likeToggle.onValueChanged.AddListener(OnLikeToggleValueChanged);
        }

        private void OnDisable()
        {
            _viewButton.onClick.RemoveListener(OnViewButtonClicked);
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            _likeToggle.onValueChanged.RemoveListener(OnLikeToggleValueChanged);
        }

        private void OnDestroy()
        {
            if (_previewMaterial)
            {
                Destroy(_previewMaterial);
            }
        }

        private async void OnViewButtonClicked()
        {
            _runtimeGuiManager.ToggleHistoryPanel();
            await _runtimeGuiManager.Generator.DownloadResultAsync(
                new GetImagineResult
                {
                    request = _imagineResult
                });
            gameObject.SetActive(false);
        }

        private void OnCloseButtonClicked()
        {
            gameObject.SetActive(false);
            _previewCamera.ResetView();
        }

        private async void OnLikeToggleValueChanged(bool value)
        {
            var result = await ApiRequests.ToggleFavorite(_imagineResult.id);
            _likeToggle.SetIsOnWithoutNotify(result.isMyFavorite);
        }

        internal void ShowPreviewPopup(ImagineResult imagineResult, Texture2D preview, Texture2D depth = null)
        {
            _imagineResult = imagineResult;
            _titleText.text = $"World #{imagineResult.id}";
            PreviewMaterial.mainTexture = preview;
            _depthPreviewImage.texture = depth;
            _depthPreviewImage.enabled = depth != null;
            _likeToggle.SetIsOnWithoutNotify(imagineResult.isMyFavorite);
            _statusText.text = $"Status: <color=\"white\">{imagineResult.status}</color>";
            _promptText.text = imagineResult.prompt;
            _negativeTextTitle.gameObject.SetActive(!string.IsNullOrWhiteSpace(imagineResult.negative_text));
            _negativePromptScrollRect.gameObject.SetActive(!string.IsNullOrWhiteSpace(imagineResult.negative_text));
            _negativePromptText.text = imagineResult.negative_text;
            _depthMapText.text = $"Depth Map: <color=\"white\">{(depth == null ? "Off" : "On")}</color>";
            _depthMapStatusText.text = _depthMapText.text;
            _seedText.text = $"Seed: <color=\"white\">{imagineResult.seed}</color>";
            _styleText.text = $"Style: <color=\"white\">{imagineResult.skybox_style_name.ToTitleCase()}</color>";
            _typeText.text = $"Type: <color=\"white\">{imagineResult.type.ToTitleCase()}</color>";
            _dateCompletedText.text = $"Date Completed: <color=\"white\">{imagineResult.completed_at:d}</color>";
            gameObject.SetActive(true);
        }
    }
}