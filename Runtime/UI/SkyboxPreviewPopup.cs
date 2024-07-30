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

        private SkyboxInfo _skybox;
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
            await _runtimeGuiManager.Generator.DownloadResultAsync(_skybox, true, destroyCancellationToken);
        }

        private void OnCloseButtonClicked()
        {
            gameObject.SetActive(false);
            _previewCamera.ResetView();
        }

        private async void OnLikeToggleValueChanged(bool value)
        {
            var result = await BlockadeLabsSkyboxGenerator.BlockadeLabsClient.SkyboxEndpoint.ToggleFavoriteAsync(_skybox, destroyCancellationToken);
            _likeToggle.SetIsOnWithoutNotify(result.IsMyFavorite);
        }

        internal void ShowPreviewPopup(SkyboxInfo imagineResult)
        {
            _skybox = imagineResult;
            GetSkyboxTextures(_skybox);
            _titleText.text = $"World #{imagineResult.Id}";
            _likeToggle.SetIsOnWithoutNotify(imagineResult.IsMyFavorite);
            _model3Tag.gameObject.SetActive(_skybox.Model == SkyboxModel.Model3);
            _statusText.text = $"Status: <color=\"white\">{imagineResult.Status}</color>";
            _promptText.text = imagineResult.Prompt;
            _negativeTextTitle.gameObject.SetActive(!string.IsNullOrWhiteSpace(imagineResult.NegativeText));
            _negativePromptScrollRect.gameObject.SetActive(!string.IsNullOrWhiteSpace(imagineResult.NegativeText));
            _negativePromptText.text = imagineResult.NegativeText;
            var hasDepth = !string.IsNullOrWhiteSpace(imagineResult.DepthTextureUrl);
            _depthMapText.text = $"Depth Map: <color=\"white\">{(hasDepth ? "On" : "Off")}</color>";
            _depthMapStatusText.text = _depthMapText.text;
            _seedText.text = $"Seed: <color=\"white\">{imagineResult.Seed}</color>";
            _styleText.text = $"Style: <color=\"white\">{imagineResult.SkyboxStyleName.ToTitleCase()}</color>";
            _typeText.text = $"Type: <color=\"white\">{imagineResult.Type.ToTitleCase()}</color>";
            _dateCompletedText.text = $"Date Completed: <color=\"white\">{imagineResult.CompletedAt:d}</color>";
            _remixDetails.text = imagineResult.RemixId.HasValue
                ? $"Remixed From: <color=\"white\">World #{imagineResult.RemixId.Value:d}</color>" : string.Empty;
            gameObject.SetActive(true);
        }

        private static readonly Dictionary<string, Tuple<Texture2D, Texture2D>> _imageCache = new Dictionary<string, Tuple<Texture2D, Texture2D>>();

        private async void GetSkyboxTextures(SkyboxInfo result)
        {
            try
            {
                _depthMapText.gameObject.SetActive(false);
                _depthPreviewImage.gameObject.SetActive(false);
                _depthPreviewImage.texture = null;
                PreviewMaterial.mainTexture = null;

                if (_imageCache.TryGetValue(result.ObfuscatedId, out var cachedImages))
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
                    _imageCache[result.ObfuscatedId] = cachedImages;
                    var downloadTasks = new List<Task>();

                    if (skybox == null)
                    {
                        downloadTasks.Add(Rest.DownloadTextureAsync(result.MainTextureUrl, cancellationToken: destroyCancellationToken).ContinueWith(task =>
                        {
                            skybox = task.Result;
                        }, destroyCancellationToken));
                    }

                    if (depth == null && !string.IsNullOrWhiteSpace(result.DepthTextureUrl))
                    {
                        downloadTasks.Add(Rest.DownloadTextureAsync(result.DepthTextureUrl, cancellationToken: destroyCancellationToken).ContinueWith(task =>
                        {
                            depth = task.Result;
                        }, destroyCancellationToken));
                    }

                    try
                    {
                        await Task.WhenAll(downloadTasks).ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    _imageCache[result.ObfuscatedId] = new Tuple<Texture2D, Texture2D>(skybox, depth);

                    if (_skybox.ObfuscatedId == result.ObfuscatedId)
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