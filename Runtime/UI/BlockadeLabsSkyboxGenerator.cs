using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using UnityEngine;

#if UNITY_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlockadeLabsSDK
{
    public class BlockadeLabsSkyboxGenerator : MonoBehaviour
    {
        private static BlockadeLabsClient _blockadeLabsClient;

        internal static BlockadeLabsClient BlockadeLabsClient
        {
            get
            {
                _blockadeLabsClient ??= Configuration != null
                    ? new BlockadeLabsClient(Configuration)
                    : new BlockadeLabsClient(new BlockadeLabsAuthentication().LoadDefaultsReversed(), new BlockadeLabsClientSettings());
#if BLOCKADE_DEBUG
                _blockadeLabsClient.EnableDebug = true;
#endif
                return _blockadeLabsClient;
            }
            set => _blockadeLabsClient = value;
        }

        [SerializeField]
        [Obsolete("Use BlockadeLabsConfiguration instead")]
        [Tooltip("API Key from Blockade Labs. Get one at api.blockadelabs.com")]
#pragma warning disable CS0414 // Field is assigned but its value is never used
        // we will keep this serialized field so that we can convert to new Configuration
        private string _apiKey = "API key needed. Get one at api.blockadelabs.com";
#pragma warning restore CS0414

        [SerializeField]
        private BlockadeLabsConfiguration _configuration;

        public static BlockadeLabsConfiguration Configuration { get; internal set; }

        [SerializeField]
        [Tooltip("The version of the generation engine to use.")]
        private SkyboxModel _modelVersion = SkyboxModel.Model3;
        public SkyboxModel ModelVersion
        {
            get => _modelVersion;
            set
            {
                if (_modelVersion != value)
                {
                    _modelVersion = value;
                    UpdateActiveStyleList();
                    _selectedStyle = null;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        [SerializeField, Tooltip("Optional skybox mesh to apply the generated depth material.")]
        private BlockadeLabsSkyboxMesh _skyboxMesh;
        public BlockadeLabsSkyboxMesh SkyboxMesh
        {
            get => _skyboxMesh;
            set => _skyboxMesh = value;
        }

        [SerializeField, Tooltip("Optional material to copy for generating a skybox.")]
        private Material _skyboxMaterial;
        public Material SkyboxMaterial
        {
            get => _skyboxMaterial;
            set => _skyboxMaterial = value;
        }

        [SerializeField, Tooltip("Optional material to copy for generating a skybox with depth.")]
        private Material _depthMaterial;
        public Material DepthMaterial
        {
            get => _depthMaterial;
            set => _depthMaterial = value;
        }

        [SerializeField, Tooltip("Compute shader to use for converting panoramic to cubemap at runtime.")]
        private ComputeShader _cubemapComputeShader;
        public ComputeShader CubemapComputeShader
        {
            get => _cubemapComputeShader;
            set => _cubemapComputeShader = value;
        }

#if UNITY_HDRP
        [SerializeField, Tooltip("Optional volume to apply the generated volume profile to for HDRP.")]
        private Volume _HDRPVolume;

        /// <summary>
        /// <see cref="Volume"/> to apply the generated volume profile to for HDRP.
        /// </summary>
        public Volume HDRPVolume
        {
            get => _HDRPVolume;
            set => _HDRPVolume = value;
        }

        [SerializeField, Tooltip("Optional volume profile to copy from for HDRP.")]
        private VolumeProfile _HDRPVolumeProfile;

        /// <summary>
        /// <see cref="VolumeProfile"/> to copy from for HDRP.
        /// </summary>
        public VolumeProfile HDRPVolumeProfile
        {
            get => _HDRPVolumeProfile;
            set => _HDRPVolumeProfile = value;
        }
#endif

        private List<SkyboxStyle> _styleFamilies;
        public IReadOnlyList<SkyboxStyle> StyleFamilies => _styleFamilies;

        private List<SkyboxStyle> _allModelStyleFamilies;
        public IReadOnlyList<SkyboxStyle> AllModelStyleFamilies => _allModelStyleFamilies;

        private IReadOnlyList<SkyboxStyle> _model2StyleFamilies;
        private IReadOnlyList<SkyboxStyle> _model3StyleFamilies;

        [SerializeField]
        private SkyboxStyle _selectedStyle = null;

        public SkyboxStyle SelectedStyle
        {
            get => _selectedStyle?.Id == 0 ? null : _selectedStyle;
            set
            {
                if (_selectedStyle == value) { return; }
                _selectedStyle = value.Id == 0 ? null : value;
                OnPropertyChanged?.Invoke();
            }
        }

        [Tooltip("Describe the skybox you want to generate.")]
        [SerializeField]
        private string _prompt = "";
        public string Prompt
        {
            get => _prompt;
            set
            {
                if (_prompt != value)
                {
                    _prompt = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        [Tooltip("Which phrases to avoid in the generated skybox.")]
        [SerializeField]
        private string _negativeText = "";
        public string NegativeText
        {
            get => _negativeText;
            set
            {
                if (_negativeText != value)
                {
                    _negativeText = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        public bool SendNegativeText { get; set; } = true;

        [Tooltip("The seed is a specific value that guides the randomness in the image creation process. While usually assigned randomly, fixing the seed can help achieve consistent results with minor variations, despite other sources of entropy. This allows for controlled iterations of a prompt, with the seed ensuring a degree of predictability in the otherwise random generation process.")]
        [SerializeField]
        private int _seed;
        public int Seed
        {
            get => _seed;
            set
            {
                if (_seed != value)
                {
                    _seed = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        [Tooltip("Use AI to enhance your prompt.")]
        [SerializeField]
        private bool _enhancePrompt = true;
        public bool EnhancePrompt
        {
            get => _enhancePrompt;
            set
            {
                if (_enhancePrompt != value)
                {
                    _enhancePrompt = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        [Tooltip("Give your existing world a new style by describing it.")]
        [SerializeField]
        private bool _remix = false;
        public bool Remix
        {
            get => _remix;
            set
            {
                if (_remix != value)
                {
                    _remix = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        [SerializeField]
        private Texture2D _remixImage;

        public Texture2D RemixImage
        {
            get => _remixImage;
            set
            {
                if (_remixImage != value)
                {
                    DestroyRemixImage();
                    _remixImage = value;
                    _viewRemixImage = _remixImage != null;
                    UpdateSkyboxAndDepthMesh();
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private Cubemap _remixCubemap;
        private Material _remixDepthMaterial;
        private Material _remixSkyboxMaterial;
#if UNITY_HDRP
        private VolumeProfile _remixVolumeProfile;
#endif

        private bool _viewRemixImage;
        public bool ViewRemixImage
        {
            get => _viewRemixImage;
            set
            {
                _viewRemixImage = value;
                UpdateSkyboxAndDepthMesh();
                OnPropertyChanged?.Invoke();
            }
        }

        public enum State
        {
            NeedApiKey,
            Ready,
            Generating
        }

        private State _state = State.NeedApiKey;
        public State CurrentState => _state;
        public event Action<State> OnStateChanged;

        private string _lastError;
        public string LastError => _lastError;
        public event Action OnErrorChanged;

        public event Action OnPropertyChanged;

        private float _percentageCompleted = -1;
        public float PercentageCompleted => _percentageCompleted;

        private CancellationTokenSource _generationCts;

        public bool HasSkyboxMetadata => _skyboxMesh != null && _skyboxMesh.SkyboxAsset != null;

        public bool CanRemix => HasSkyboxMetadata || _remixImage != null;

#if UNITY_EDITOR
        private int _progressId;

        internal static event Action OnSurveyTrigger;
#endif

#if !UNITY_2022_1_OR_NEWER
        private CancellationTokenSource _destroyCancellationTokenSource = new CancellationTokenSource();
        // ReSharper disable once InconsistentNaming
        // this is the same name as the unity property introduced in 2022+
        private CancellationToken destroyCancellationToken => _destroyCancellationTokenSource.Token;
#endif

        private void OnDestroy()
        {
            DestroyRemixImage();
#if !UNITY_2022_1_OR_NEWER
            _destroyCancellationTokenSource?.Cancel();
            _destroyCancellationTokenSource?.Dispose();
#endif
        }

        /// <summary>
        /// Checks if the API key is valid.
        /// </summary>
        /// <returns>True, if the API key is valid.</returns>
        public bool CheckApiKeyValid()
        {
            if (_blockadeLabsClient != null)
            {
                if (_blockadeLabsClient.HasValidAuthentication == false)
                {
                    SetInvalid();
                    return false;
                }

                try
                {
                    _blockadeLabsClient.ValidateAuthentication();
                }
                catch (Exception)
                {
                    SetInvalid();
                    return false;
                }
            }
            else
            {
                if (_configuration != null)
                {
                    if (string.IsNullOrWhiteSpace(_configuration.ApiKey))
                    {
                        SetInvalid(false);
                        return false;
                    }

                    try
                    {
                        _blockadeLabsClient = new BlockadeLabsClient(_configuration);

                        if (!BlockadeLabsClient.HasValidAuthentication)
                        {
                            SetInvalid();
                            return false;
                        }

                        BlockadeLabsClient.ValidateAuthentication();
                    }
                    catch (Exception)
                    {
                        SetInvalid();
                        return false;
                    }
                }
                else
                {
                    SetInvalid();
                }
            }

            if (_blockadeLabsClient == null)
            {
                SetInvalid(false);
                return false;
            }

            void SetInvalid(bool showError = true)
            {
                if (showError)
                {
                    SetError("Something went wrong. Please recheck you API key.");
                }

                SetState(State.NeedApiKey);
            }

            return true;
        }

        private void SetState(State state)
        {
            _state = state;
            OnStateChanged?.Invoke(_state);
        }

        private void SetError(string error)
        {
            _lastError = error;
            Debug.LogError(error);
            OnErrorChanged?.Invoke();
        }

        private void ClearError()
        {
            _lastError = "";
            OnErrorChanged?.Invoke();
        }

        public async void Load()
        {
            try
            {
                await LoadAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static Task _loadingTask;

        public async Task LoadAsync()
        {
            if (_loadingTask != null)
            {
                await _loadingTask;
                return;
            }

            try
            {
                _loadingTask = LoadInternalAsync();
                await _loadingTask;
            }
            finally
            {
                _loadingTask = null;
            }
        }

        private async Task LoadInternalAsync()
        {
            ClearError();

            if (!CheckApiKeyValid())
            {
                return;
            }

            try
            {
                _model2StyleFamilies = (await BlockadeLabsClient.SkyboxEndpoint.GetSkyboxStylesMenuAsync(SkyboxModel.Model2, destroyCancellationToken))
                    .Where(style => style.Status != "disabled").ToList();
                _model3StyleFamilies = (await BlockadeLabsClient.SkyboxEndpoint.GetSkyboxStylesMenuAsync(SkyboxModel.Model3, destroyCancellationToken))
                    .Where(style => style.Status != "disabled").ToList();
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case RestException restException:
                        if (restException.RestResponse.Code == 403)
                        {
                            BlockadeLabsClient.HasValidAuthentication = false;
                            SetError("Something went wrong. Please recheck you API key.");
                            SetState(State.NeedApiKey);
                        }
                        else
                        {
                            Debug.LogException(e);
                        }
                        return;
                    default:
                        Debug.LogException(e);
                        return;
                }
            }

            _allModelStyleFamilies = _model3StyleFamilies.Concat(_model2StyleFamilies).ToList();
            UpdateActiveStyleList();
            SetState(State.Ready);
        }

        private void UpdateActiveStyleList()
        {
            _styleFamilies = (_modelVersion == SkyboxModel.Model2 ? _model2StyleFamilies : _model3StyleFamilies).ToList();
            OnPropertyChanged?.Invoke();
        }

        public async void GenerateSkybox()
        {
            if (string.IsNullOrWhiteSpace(_prompt))
            {
                SetError("Prompt is empty.");
                return;
            }

            if (SelectedStyle == null)
            {
                SetError("No style selected.");
                return;
            }

            var request = new SkyboxRequest(SelectedStyle, _prompt, _remix ? _remixImage : null, SendNegativeText ? _negativeText : string.Empty, _enhancePrompt, _seed);

            if (_remix && _remixImage == null)
            {
                if (!HasSkyboxMetadata)
                {
                    SetError("Missing skybox ID. Please use a previously generated skybox or disable remix.");
                    return;
                }

                request.RemixImagineId = _skyboxMesh.SkyboxAsset.Id;
            }

            ClearError();
            SetState(State.Generating);
            _generationCts?.Dispose();
            _generationCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_generationCts.Token, destroyCancellationToken);
            UpdateProgress(10);

            try
            {
                var progress = new Progress<SkyboxInfo>(info =>
                {
                    switch (info.Status)
                    {
                        case Status.Pending:
                            UpdateProgress(20);
                            break;
                        case Status.Dispatched:
                            UpdateProgress(30);
                            break;
                        case Status.Processing:
                            if (_percentageCompleted < 80)
                            {
                                UpdateProgress(_percentageCompleted + 2);
                            }
                            break;
                    }
                });

                var response = await BlockadeLabsClient.SkyboxEndpoint.GenerateSkyboxAsync(request, progressCallback: progress, cancellationToken: linkedCts.Token);

                if (response == null)
                {
                    throw new Exception("Error generating skybox.");
                }

                if (response.Status == Status.Error)
                {
                    throw new Exception(response.ErrorMessage);
                }

                if (_enhancePrompt)
                {
                    _prompt = response.Prompt;
                    _enhancePrompt = false;
                    OnPropertyChanged?.Invoke();
                }

                UpdateProgress(80);
                await DownloadResultAsync(response, true, linkedCts.Token);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case TaskCanceledException _:
                    case OperationCanceledException _:
                        break;
                    default:
                        Debug.LogException(e);
                        SetGenerateFailed($"Error generating skybox: {e.Message}");
                        break;
                }
            }
            finally
            {
                UpdateProgress(-1);
                SetState(State.Ready);
#if UNITY_EDITOR
                OnSurveyTrigger?.Invoke();
#endif
            }
        }

        private void SetGenerateFailed(string message)
        {
            SetError(message);
            UpdateProgress(-1);
            SetState(State.Ready);
        }

#if UNITY_EDITOR
        internal async Task DownloadResultAsync(SkyboxInfo skybox, bool setSkybox = true, CancellationToken cancellationToken = default)
        {
            var textureUrl = skybox.MainTextureUrl;
            var depthMapUrl = skybox.DepthTextureUrl;
            var hasDepthMap = !string.IsNullOrWhiteSpace(depthMapUrl);
            var prompt = skybox.Prompt;

            if (string.IsNullOrWhiteSpace(textureUrl))
            {
                SetGenerateFailed("Returned skybox texture url is empty.");
                return;
            }

            const int maxLength = 20;

            if (prompt.Length > maxLength)
            {
                prompt = prompt.Substring(0, maxLength);
            }

            AssetUtils.CreateGenerateBlockadeLabsFolder();
            var prefix = AssetUtils.CreateValidFilename(prompt);
            var folderPath = GetOrCreateSkyboxFolder(prefix, skybox.Id);
            var skyboxAIPath = $"{folderPath}/{prefix}.asset";
            var skyboxAI = AssetDatabase.LoadAssetAtPath<SkyboxAI>(skyboxAIPath);

            if (skyboxAI == null)
            {
                skyboxAI = ScriptableObject.CreateInstance<SkyboxAI>();
                skyboxAI.SetMetadata(skybox);
                AssetDatabase.CreateAsset(skyboxAI, skyboxAIPath);
            }
            else
            {
                skyboxAI.SetMetadata(skybox);
                EditorUtility.SetDirty(skyboxAI);
            }

            var tasks = new List<Task>();
            var texturePath = $"{folderPath}/{prefix} texture.png";
            var depthTexturePath = $"{folderPath}/{prefix} depth texture.png";

            if (skyboxAI.SkyboxTexture == null)
            {
                tasks.Add(Rest.DownloadFileAsync(textureUrl, destination: texturePath, debug: skybox.Client.EnableDebug, cancellationToken: cancellationToken));
            }

            if (hasDepthMap && skyboxAI.DepthTexture == null)
            {
                tasks.Add(Rest.DownloadFileAsync(depthMapUrl, destination: depthTexturePath, debug: skybox.Client.EnableDebug, cancellationToken: cancellationToken));
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case TaskCanceledException _:
                    case OperationCanceledException _:
                        // ignore
                        break;
                    default:
                        Debug.LogException(e);
                        break;
                }

                Directory.Delete(folderPath, true);
                File.Delete($"{folderPath}.meta");
                AssetDatabase.Refresh();
                return;
            }

            if (tasks.Count > 0)
            {
                AssetDatabase.Refresh();
            }

            if (skyboxAI.SkyboxTexture == null &&
                AssetImporter.GetAtPath(texturePath) is TextureImporter colorImporter)
            {
                colorImporter.maxTextureSize = 8192;
                colorImporter.textureCompression = TextureImporterCompression.Uncompressed;
                colorImporter.mipmapEnabled = false;
                colorImporter.textureShape = TextureImporterShape.TextureCube;
                colorImporter.SaveAndReimport();

                var skyboxTexture = AssetDatabase.LoadAssetAtPath<Cubemap>(texturePath);
                skyboxAI.SkyboxTexture = skyboxTexture;
            }

            if (hasDepthMap &&
                skyboxAI.DepthTexture == null &&
                AssetImporter.GetAtPath(depthTexturePath) is TextureImporter depthImporter)
            {
                depthImporter.maxTextureSize = 2048;
                depthImporter.textureCompression = TextureImporterCompression.Uncompressed;
                depthImporter.mipmapEnabled = false;
                depthImporter.wrapModeU = TextureWrapMode.Repeat;
                depthImporter.wrapModeV = TextureWrapMode.Clamp;
                depthImporter.isReadable = true;
                depthImporter.SaveAndReimport();

                var depthTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(depthTexturePath);
                skyboxAI.DepthTexture = depthTexture;
                EditorUtility.SetDirty(skyboxAI);
            }

            if (skyboxAI.DepthMaterial == null)
            {
                var depthMaterialPath = $"{folderPath}/{prefix} depth material.mat";
                var depthMaterial = AssetDatabase.LoadAssetAtPath<Material>(depthMaterialPath);

                if (depthMaterial == null)
                {
                    depthMaterial = CreateDepthMaterial(skyboxAI.SkyboxTexture, skyboxAI.DepthTexture);
                    AssetDatabase.CreateAsset(depthMaterial, depthMaterialPath);
                }

                skyboxAI.DepthMaterial = depthMaterial;
                EditorUtility.SetDirty(skyboxAI);
            }

            if (skyboxAI.SkyboxMaterial == null)
            {
                var skyboxMaterialPath = $"{folderPath}/{prefix} skybox material.mat";
                var skyboxMaterial = AssetDatabase.LoadAssetAtPath<Material>(skyboxMaterialPath);

                if (skyboxMaterial == null)
                {
                    skyboxMaterial = CreateSkyboxMaterial(skyboxAI.SkyboxTexture);
                    AssetDatabase.CreateAsset(skyboxMaterial, skyboxMaterialPath);
                }

                skyboxAI.SkyboxMaterial = skyboxMaterial;
                EditorUtility.SetDirty(skyboxAI);
            }

#if UNITY_HDRP
            if (skyboxAI.VolumeProfile == null)
            {
                var volumeProfilePath = $"{folderPath}/{prefix} HDRP volume profile.asset";
                var volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(volumeProfilePath);

                if (volumeProfile == null)
                {
                    volumeProfile = CreateVolumeProfile(skyboxAI.SkyboxTexture);
                    AssetDatabase.CreateAsset(volumeProfile, volumeProfilePath);

                    foreach (var volumeComponent in volumeProfile.components)
                    {
                        volumeComponent.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                        AssetDatabase.AddObjectToAsset(volumeComponent, volumeProfile);
                    }

                    EditorUtility.SetDirty(volumeProfile);
                    AssetDatabase.SaveAssetIfDirty(volumeProfile);
                }

                skyboxAI.VolumeProfile = volumeProfile;
                EditorUtility.SetDirty(skyboxAI);
            }
#endif

            if (setSkybox)
            {
                _viewRemixImage = false;
                AssetDatabase.SaveAssetIfDirty(skyboxAI);
                AssetDatabase.SaveAssets();
                SetSkyboxMetadata(skyboxAI);
                UpdateSkyboxAndDepthMesh();
                SetState(State.Ready);
            }

            AssetUtils.PingAsset(skyboxAI);
        }

        private string GetOrCreateSkyboxFolder(string prefix, int skyboxId)
        {
            if (AssetUtils.TryCreateFolder(prefix, out var folderPath))
            {
                return folderPath;
            }

            // check if there is a .txt file. If it exists then convert it to a skyboxAI asset and delete it
            // check all directories that start with the prefix, but contain the *data.txt
            var dataFiles = Directory.GetFiles(Application.dataPath, $"*{prefix} data.txt", SearchOption.AllDirectories).ToList();
            // check if there are any existing skyboxAI assets with the same id
            dataFiles.AddRange(Directory.GetFiles(Application.dataPath, $"{prefix}.asset", SearchOption.AllDirectories));

            foreach (var dataFile in dataFiles)
            {
                var currentDirectoryPath = Directory.GetParent(Path.GetFullPath(dataFile))!.FullName;
                var currentDirectorySkyboxAIPath = $"{currentDirectoryPath}/{prefix}.asset";

                if (File.Exists(currentDirectorySkyboxAIPath))
                {
                    var existingSkybox = AssetDatabase.LoadAssetAtPath<SkyboxAI>(currentDirectorySkyboxAIPath.ToProjectPath());

                    if (existingSkybox.Id == skyboxId)
                    {
                        return currentDirectoryPath.ToProjectPath();
                    }
                }
                else
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    // WriteAllTextAsync not defined in Unity 2020.3
                    var skyboxData = File.ReadAllText(dataFile);
                    var skyboxInfo = JsonConvert.DeserializeObject<SkyboxEndpoint.SkyboxInfoRequest>(skyboxData).SkyboxInfo;

                    if (skyboxInfo.Id == skyboxId)
                    {
                        return currentDirectoryPath.ToProjectPath();
                    }
                }
            }

            // we didn't find a skyboxAI asset or json file, so we need to create a new folder
            return AssetUtils.CreateUniqueFolder(prefix);
        }
#else
        internal async Task DownloadResultAsync(SkyboxInfo result, bool setSkybox = true, CancellationToken cancellationToken = default)
        {
            var useComputeShader = SystemInfo.supportsComputeShaders && _cubemapComputeShader != null;

            var tasks = new List<Task<Texture2D>>
            {
                Rest.DownloadTextureAsync(result.MainTextureUrl, debug: BlockadeLabsClient.EnableDebug, cancellationToken: cancellationToken)
            };

            if (!string.IsNullOrWhiteSpace(result.DepthTextureUrl))
            {
                tasks.Add(Rest.DownloadTextureAsync(result.DepthTextureUrl, debug: BlockadeLabsClient.EnableDebug, cancellationToken: cancellationToken));
            }

            Texture2D[] textures = null;

            try
            {
                textures = await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case TaskCanceledException _:
                    case OperationCanceledException _:
                        // ignore
                        if (textures != null)
                        {
                            foreach (var texture in textures)
                            {
                                texture.Destroy();
                            }
                        }
                        return;
                    default:
                        Debug.LogException(e);
                        break;
                }
            }

            var skyboxAI = ScriptableObject.CreateInstance<SkyboxAI>();
            skyboxAI.SetMetadata(result);

            if (textures?[0] != null)
            {
                skyboxAI.SkyboxTexture = PanoramicToCubemap.Convert(textures[0], useComputeShader ? _cubemapComputeShader : null, 2048);
                textures[0].Destroy();
            }

            skyboxAI.DepthTexture = textures?.Length > 1 ? textures[1] : null;
            skyboxAI.DepthMaterial = CreateDepthMaterial(skyboxAI.SkyboxTexture, skyboxAI.DepthTexture);
            skyboxAI.SkyboxMaterial = CreateSkyboxMaterial(skyboxAI.SkyboxTexture);

#if UNITY_HDRP
            skyboxAI.VolumeProfile = CreateVolumeProfile(skyboxAI.SkyboxTexture);

            if (setSkybox)
            {
                SetVolumeProfile(skyboxAI.VolumeProfile);
            }
#endif
            if (setSkybox)
            {
                SetSkyboxMetadata(skyboxAI);
                UpdateSkyboxAndDepthMesh();
                SetState(State.Ready);
            }
        }
#endif // UNITY_EDITOR

        private void SetSkyboxMetadata(SkyboxAI skybox)
        {
            Prompt = skybox.Prompt;
            SendNegativeText = !string.IsNullOrWhiteSpace(skybox.NegativeText);
            NegativeText = skybox.NegativeText;
            ModelVersion = skybox.Model;
            _styleFamilies = (_modelVersion == SkyboxModel.Model2 ? _model2StyleFamilies : _model3StyleFamilies).ToList();

            foreach (var family in _allModelStyleFamilies)
            {
                if (family.FamilyStyles != null)
                {
                    foreach (var style in family.FamilyStyles)
                    {
                        if (style.Id == skybox.SkyboxStyleId)
                        {
                            SelectedStyle = style;
                        }
                    }
                }
                else
                {
                    if (family.Id == skybox.SkyboxStyleId)
                    {
                        SelectedStyle = family;
                    }
                }
            }

            if (_skyboxMesh != null)
            {
                _skyboxMesh.SkyboxAsset = skybox;
            }
        }

        private void UpdateSkyboxAndDepthMesh()
        {
            if (_remixImage != null && _viewRemixImage)
            {
                EnsureRemixImageCubemap();
                SetDepthMaterial(_remixDepthMaterial);
                SetSkyboxMaterial(_remixSkyboxMaterial);
#if UNITY_HDRP
                SetVolumeProfile(_remixVolumeProfile);
#endif
            }
            else if (_skyboxMesh.SkyboxAsset != null)
            {
                if (_skyboxMesh.SkyboxAsset.DepthMaterial != null)
                {
                    SetDepthMaterial(_skyboxMesh.SkyboxAsset.DepthMaterial);
                }

                if (_skyboxMesh.SkyboxAsset.SkyboxMaterial != null)
                {
                    SetSkyboxMaterial(_skyboxMesh.SkyboxAsset.SkyboxMaterial);
                }
#if UNITY_HDRP
                if (_skyboxMesh.SkyboxAsset.SkyboxTexture != null)
                {
                    SetVolumeProfile(_skyboxMesh.SkyboxAsset.VolumeProfile);
                }
#endif
            }
            else
            {
                SetDepthMaterial(_depthMaterial);
                SetSkyboxMaterial(_skyboxMaterial);
#if UNITY_HDRP
                SetVolumeProfile(_HDRPVolumeProfile);
#endif
            }
        }

        private void EnsureRemixImageCubemap()
        {
            if (_remixCubemap == null)
            {
                _remixCubemap = PanoramicToCubemap.Convert(_remixImage, _cubemapComputeShader, _remixImage.width);
            }

            if (_remixDepthMaterial == null)
            {
                _remixDepthMaterial = CreateDepthMaterial(_remixCubemap, null);
            }
            else
            {
                _remixDepthMaterial.mainTexture = _remixCubemap;
            }

            if (_remixSkyboxMaterial == null)
            {
                _remixSkyboxMaterial = CreateSkyboxMaterial(_remixCubemap);
            }
            else
            {
                _remixSkyboxMaterial.SetTexture("_Tex", _remixCubemap);
            }
#if UNITY_HDRP
            if (_remixVolumeProfile == null)
            {
                _remixVolumeProfile = CreateVolumeProfile(_remixCubemap);
            }
#endif
        }

        private void DestroyRemixImage()
        {
            if (_remixImage != null)
            {
                _remixImage.Destroy();
                _remixImage = null;
            }

            if (_remixCubemap != null)
            {
                _remixCubemap.Destroy();
                _remixCubemap = null;
            }

            if (_remixDepthMaterial != null)
            {
                _remixDepthMaterial.Destroy();
                _remixDepthMaterial = null;
            }

            if (_remixSkyboxMaterial != null)
            {
                _remixSkyboxMaterial.Destroy();
                _remixSkyboxMaterial = null;
            }

#if UNITY_HDRP
            if (_remixVolumeProfile != null)
            {
                _remixVolumeProfile.Destroy();
                _remixVolumeProfile = null;
            }
#endif
        }

        private Material CreateDepthMaterial(Texture texture, Texture depthTexture)
        {
            if (_depthMaterial == null)
            {
                return null;
            }

            var material = new Material(_depthMaterial)
            {
                mainTexture = texture
            };

            if (material.HasProperty("_DepthMap"))
            {
                material.SetTexture("_DepthMap", depthTexture);
            }

            return material;
        }

        private void SetDepthMaterial(Material depthMaterial)
        {
            if (_skyboxMesh.TryGetComponent<Renderer>(out var skyboxRenderer))
            {
                skyboxRenderer.sharedMaterial = depthMaterial;
            }
        }

        private Material CreateSkyboxMaterial(Texture texture)
        {
            if (_skyboxMaterial == null)
            {
                return null;
            }

            var material = new Material(_skyboxMaterial);
            material.SetTexture("_Tex", texture);

            return material;
        }

        private void SetSkyboxMaterial(Material skyboxMaterial)
        {
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }

#if UNITY_HDRP
        private VolumeProfile CreateVolumeProfile(Cubemap skyTexture)
        {
            if (_HDRPVolumeProfile == null) { return null; }

            var volumeProfile = Instantiate(_HDRPVolumeProfile);
            volumeProfile.name = volumeProfile.name.Replace("(Clone)", string.Empty);
            volumeProfile.components.Clear();

            foreach (var item in _HDRPVolumeProfile.components)
            {
                var volumeComponent = Instantiate(item);
                volumeComponent.name = volumeComponent.name.Replace("(Clone)", string.Empty);

                if (volumeComponent is HDRISky hdriSky)
                {
                    hdriSky.hdriSky.Override(skyTexture);
                }

                volumeProfile.components.Add(volumeComponent);
            }

            return volumeProfile;
        }

        private void SetVolumeProfile(VolumeProfile volumeProfile)
        {
            if (_HDRPVolume != null)
            {
                _HDRPVolume.sharedProfile = volumeProfile;
            }
        }
#endif // UNITY_HDRP

        private void UpdateProgress(float percentageCompleted)
        {
            _percentageCompleted = percentageCompleted;

#if UNITY_EDITOR
            var showProgress = percentageCompleted >= 0 && percentageCompleted < 100;

            if (showProgress && _progressId == 0)
            {
                _progressId = Progress.Start("Generating Skybox Assets");
            }

            if (_progressId != 0)
            {
                Progress.Report(_progressId, percentageCompleted / 100f);

                if (!showProgress)
                {
                    Progress.Remove(_progressId);
                    _progressId = 0;
                }
            }
#endif // UNITY_EDITOR
        }

        public void Cancel()
        {
            _generationCts?.Cancel();
            _generationCts?.Dispose();
            UpdateProgress(-1);
            SetState(State.Ready);
        }

        internal void EditorPropertyChanged()
        {
            OnPropertyChanged?.Invoke();
        }
    }
}