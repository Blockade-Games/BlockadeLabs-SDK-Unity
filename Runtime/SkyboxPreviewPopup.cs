using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
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
        private Image _model3Tag;

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
        private TextMeshProUGUI _remixDetails;

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
                    // get built in skybox shader
                    _skyboxShader ??= Shader.Find("Skybox/Panoramic");
                    _previewMaterial = new Material(_skyboxShader)
                    {
                        name = "Skybox Preview Material"
                    };
                    _previewSkyboxRenderer.sharedMaterial = _previewMaterial;
                }

                return _previewMaterial;
            }
        }

#if !UNITY_2022_1_OR_NEWER
        private System.Threading.CancellationTokenSource _destroyCancellationTokenSource = new System.Threading.CancellationTokenSource();
        // ReSharper disable once InconsistentNaming
        // this is the same name as the unity property introduced in 2022+
        private System.Threading.CancellationToken destroyCancellationToken => _destroyCancellationTokenSource.Token;
#endif

        private void OnEnable()
        {
            _viewButton.interactable = _runtimeGuiManager.Generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready;
            _runtimeGuiManager.Generator.OnStateChanged += Generator_OnStateChanged;
            _viewButton.onClick.AddListener(OnViewButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            _likeToggle.onValueChanged.AddListener(OnLikeToggleValueChanged);
        }

        private void Generator_OnStateChanged(BlockadeLabsSkyboxGenerator.State state)
        {
            _viewButton.interactable = state == BlockadeLabsSkyboxGenerator.State.Ready;
        }

        private void OnDisable()
        {
            _runtimeGuiManager.Generator.OnStateChanged -= Generator_OnStateChanged;
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

#if !UNITY_2022_1_OR_NEWER
            _destroyCancellationTokenSource?.Cancel();
            _destroyCancellationTokenSource?.Dispose();
#endif
        }

        private async void OnViewButtonClicked()
        {
            _runtimeGuiManager.ToggleHistoryPanel();
            gameObject.SetActive(false);
            await _runtimeGuiManager.Generator.DownloadResultAsync(_imagineResult);
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

        internal void ShowPreviewPopup(ImagineResult imagineResult)
        {
            _imagineResult = imagineResult;
            GetSkyboxTextures(_imagineResult);
            _titleText.text = $"World #{imagineResult.id}";
            _likeToggle.SetIsOnWithoutNotify(imagineResult.isMyFavorite);
            _model3Tag.gameObject.SetActive(_imagineResult.model == "Model 3");
            _statusText.text = $"Status: <color=\"white\">{imagineResult.status}</color>";
            _promptText.text = imagineResult.prompt;
            _negativeTextTitle.gameObject.SetActive(!string.IsNullOrWhiteSpace(imagineResult.negative_text));
            _negativePromptScrollRect.gameObject.SetActive(!string.IsNullOrWhiteSpace(imagineResult.negative_text));
            _negativePromptText.text = imagineResult.negative_text;
            var hasDepth = !string.IsNullOrWhiteSpace(imagineResult.depth_map_url);
            _depthMapText.text = $"Depth Map: <color=\"white\">{(hasDepth ? "On" : "Off")}</color>";
            _depthMapStatusText.text = _depthMapText.text;
            _seedText.text = $"Seed: <color=\"white\">{imagineResult.seed}</color>";
            _styleText.text = $"Style: <color=\"white\">{imagineResult.skybox_style_name.ToTitleCase()}</color>";
            _typeText.text = $"Type: <color=\"white\">{imagineResult.type.ToTitleCase()}</color>";
            _dateCompletedText.text = $"Date Completed: <color=\"white\">{imagineResult.completed_at:d}</color>";
            _remixDetails.text = imagineResult.remix_imagine_id.HasValue
                ? $"Remixed From: <color=\"white\">World #{imagineResult.remix_imagine_id.Value:d}</color>" : string.Empty;
            gameObject.SetActive(true);
        }

        private static readonly Dictionary<string, Tuple<Texture2D, Texture2D>> _imageCache = new Dictionary<string, Tuple<Texture2D, Texture2D>>();

        private async void GetSkyboxTextures(ImagineResult result)
        {
            try
            {
                _depthMapText.gameObject.SetActive(false);
                _depthPreviewImage.gameObject.SetActive(false);
                _depthPreviewImage.texture = null;
                PreviewMaterial.mainTexture = null;

                if (_imageCache.TryGetValue(result.obfuscated_id, out var cachedImages))
                {
                    var (skybox, depth) = cachedImages;

                    if (skybox != null)
                    {
                        PreviewMaterial.mainTexture = skybox;
                        _previewSkyboxRenderer.sharedMaterial = PreviewMaterial;
                    }

                    if (depth != null)
                    {
                        _depthPreviewImage.texture = depth;
                        _depthMapText.gameObject.SetActive(true);
                        _depthPreviewImage.gameObject.SetActive(true);
                    }
                }
                else
                {
                    var (skybox, depth) = cachedImages = new Tuple<Texture2D, Texture2D>(null, null);
                    _imageCache[result.obfuscated_id] = cachedImages;
                    var downloadTasks = new List<Task>();

                    if (skybox == null)
                    {
                        downloadTasks.Add(ApiRequests.DownloadTextureAsync(result.file_url, cancellationToken: destroyCancellationToken).ContinueWith(task =>
                        {
                            skybox = task.Result;
                        }));
                    }

                    if (depth == null && !string.IsNullOrWhiteSpace(result.depth_map_url))
                    {
                        downloadTasks.Add(ApiRequests.DownloadTextureAsync(result.depth_map_url, cancellationToken: destroyCancellationToken).ContinueWith(task =>
                        {
                            depth = task.Result;
                        }));
                    }

                    await Task.WhenAll(downloadTasks).ConfigureAwait(true);

                    _imageCache[result.obfuscated_id] = new Tuple<Texture2D, Texture2D>(skybox, depth);

                    if (_imagineResult.obfuscated_id == result.obfuscated_id)
                    {
                        PreviewMaterial.mainTexture = skybox;
                        _previewSkyboxRenderer.sharedMaterial = PreviewMaterial;
                        _depthPreviewImage.texture = depth;
                        _depthMapText.gameObject.SetActive(depth != null);
                        _depthPreviewImage.gameObject.SetActive(depth != null);
                    }
                }
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException ||
                    e is OperationCanceledException)
                {
                    // ignored
                    return;
                }

                Debug.LogException(e);
            }
        }
    }
}