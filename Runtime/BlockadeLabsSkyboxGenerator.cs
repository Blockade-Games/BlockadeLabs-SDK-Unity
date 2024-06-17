using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using UnityEngine;

#if UNITY_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

#if PUSHER_PRESENT
using PusherClient;
using System.Linq;
#endif

namespace BlockadeLabsSDK
{
    public class BlockadeLabsSkyboxGenerator : MonoBehaviour
    {
        [Tooltip("API Key from Blockade Labs. Get one at api.blockadelabs.com")]
        [SerializeField]
        private string _apiKey = "API key needed. Get one at api.blockadelabs.com";
        public string ApiKey
        {
            get => _apiKey;
            set
            {
                if (_apiKey == value) { return; }
                _apiKey = value;
                CheckApiKeyValid();
            }
        }

        [Tooltip("The version of the generation engine to use.")]
        [SerializeField]
        private SkyboxAiModelVersion _modelVersion = SkyboxAiModelVersion.Model3;
        public SkyboxAiModelVersion ModelVersion
        {
            get => _modelVersion;
            set
            {
                if (_modelVersion != value)
                {
                    _modelVersion = value;
                    OnPropertyChanged?.Invoke();
                    UpdateActiveStyleList();
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
        public Volume HDRPVolume
        {
            get => _HDRPVolume;
            set => _HDRPVolume = value;
        }

        [SerializeField, Tooltip("Optional volume profile to copy from for HDRP.")]
        private VolumeProfile _HDRPVolumeProfile;
        public VolumeProfile HDRPVolumeProfile
        {
            get => _HDRPVolumeProfile;
            set => _HDRPVolumeProfile = value;
        }
#endif

        private List<SkyboxStyleFamily> _styleFamilies;
        public IReadOnlyList<SkyboxStyleFamily> StyleFamilies => _styleFamilies;

        private List<SkyboxStyleFamily> _allModelStyleFamilies;
        public IReadOnlyList<SkyboxStyleFamily> AllModelStyleFamilies => _allModelStyleFamilies;

        private List<SkyboxStyleFamily> _model2Styles;
        private List<SkyboxStyleFamily> _model3Styles;

        [SerializeField]
        private int _selectedStyleFamilyIndex;
        public SkyboxStyleFamily SelectedStyleFamily
        {
            get => _selectedStyleFamilyIndex >= 0 ? _styleFamilies?[_selectedStyleFamilyIndex] : null;
            set
            {
                _selectedStyleFamilyIndex = _styleFamilies.IndexOf(value);
                _selectedStyleIndex = 0;
                OnPropertyChanged?.Invoke();
            }
        }

        [SerializeField]
        private int _selectedStyleIndex;
        public SkyboxStyle SelectedStyle
        {
            get => _selectedStyleIndex >= 0 ? SelectedStyleFamily?.items?[_selectedStyleIndex] : null;
            set
            {
                _selectedStyleFamilyIndex = _styleFamilies.IndexOf(_styleFamilies.Find(x => x.items.Contains(value)));
                _selectedStyleIndex = _styleFamilies[_selectedStyleFamilyIndex].items.IndexOf(value);
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
        private bool _enhancePrompt = false;
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

        private bool _isCancelled;

        public bool HasSkyboxMetadata => _skyboxMesh != null && _skyboxMesh.SkyboxAsset != null;

        public bool CanRemix => HasSkyboxMetadata || _remixImage != null;

#if UNITY_EDITOR
        private int _progressId = 0;
#endif

        private void OnDestroy()
        {
            DestroyRemixImage();
        }

        public bool CheckApiKeyValid()
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Contains("api.blockadelabs.com"))
            {
                SetError("Something went wrong. Please recheck you API key.");
                SetState(State.NeedApiKey);
                return false;
            }

            ApiRequests.ApiKey = _apiKey;
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

        public async void Reload()
        {
            if (CheckApiKeyValid())
            {
                await LoadAsync();
            }
        }

        public async Task LoadAsync()
        {
            ClearError();

            _model2Styles = await ApiRequests.GetSkyboxStylesMenuAsync(SkyboxAiModelVersion.Model2);

            if (_model2Styles == null || _model2Styles.Count == 0)
            {
                SetError("Something went wrong. Please recheck you API key.");
                return;
            }

            _model3Styles = await ApiRequests.GetSkyboxStylesMenuAsync(SkyboxAiModelVersion.Model3);

            if (_model3Styles == null || _model3Styles.Count == 0)
            {
                SetError("Something went wrong. Please recheck you API key.");
                return;
            }

            CleanupStyleFamilyList(_model2Styles);
            CleanupStyleFamilyList(_model3Styles);
            _allModelStyleFamilies = _model3Styles.Concat(_model2Styles).ToList();
            UpdateActiveStyleList();
            SetState(State.Ready);
        }

        private void UpdateActiveStyleList()
        {
            _styleFamilies = _modelVersion == SkyboxAiModelVersion.Model2 ? _model2Styles : _model3Styles;

            _selectedStyleFamilyIndex = -1;
            _selectedStyleIndex = -1;

            OnPropertyChanged?.Invoke();
        }

        private void CleanupStyleFamilyList(List<SkyboxStyleFamily> styleFamilies)
        {
            // Remove anything with status: "disabled"
            styleFamilies.ForEach(x => x.items?.RemoveAll(y => y.status == "disabled"));
            styleFamilies.RemoveAll(x => x.status == "disabled" || x.items?.Count == 0);

            // Ensure each style has a family to simplify logic everywhere.
            for (int i = 0; i < styleFamilies.Count; i++)
            {
                if (styleFamilies[i].type == "style")
                {
                    var style = styleFamilies[i];
                    styleFamilies[i] = new SkyboxStyleFamily
                    {
                        type = "family",
                        id = style.id,
                        name = style.name,
                        sortOrder = style.sortOrder,
                        description = style.description,
                        maxChar = style.maxChar,
                        negativeTextMaxChar = style.negativeTextMaxChar,
                        image = style.image,
                        premium = style.premium,
                        isNew = style.isNew,
                        experimental = style.experimental,
                        status = style.status,
                        model = style.model,
                        model_version = style.model_version,
                        items = new List<SkyboxStyle> { style }
                    };
                }
            }
        }

        public async void GenerateSkyboxAsync()
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

            var request = new CreateSkyboxRequest
            {
                prompt = _prompt,
                negative_text = SendNegativeText ? _negativeText : string.Empty,
                seed = _seed,
                enhance_prompt = _enhancePrompt,
                skybox_style_id = SelectedStyle.id,
            };

            if (_remix)
            {
                if (_remixImage != null)
                {
                    request.control_model = _modelVersion == SkyboxAiModelVersion.Model3 ? "remix" : "sketch";
                    request.control_image = _remixImage.EncodeToPNG();
                }
                else
                {
                    if (!HasSkyboxMetadata)
                    {
                        SetError("Missing skybox ID. Please use a previously generated skybox or disable remix.");
                        return;
                    }

                    request.remix_imagine_id = _skyboxMesh.SkyboxAsset.Id;
                }
            }

            ClearError();
            SetState(State.Generating);
            _isCancelled = false;
            UpdateProgress(5);

            try
            {
                var response = await ApiRequests.GenerateSkyboxAsync(request);
                if (_isCancelled)
                {
                    return;
                }

                if (response == null)
                {
                    throw new Exception("Error generating skybox.");
                }

                if (response.status == Status.Error)
                {
                    throw new Exception(response.error_message);
                }

                if (_enhancePrompt)
                {
                    _prompt = response.prompt;
                    _enhancePrompt = false;
                    OnPropertyChanged?.Invoke();
                }

                UpdateProgress(33);

#if PUSHER_PRESENT
                var result = await WaitForPusherResultAsync(response.pusher_channel, response.pusher_event);
#else
                var result = await PollForResultAsync(response.obfuscated_id);
#endif
                if (_isCancelled || result == null)
                {
                    return;
                }

                UpdateProgress(80);

                await DownloadResultAsync(result);

                if (_isCancelled)
                {
                    return;
                }

                UpdateProgress(-1);
                SetState(State.Ready);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                SetGenerateFailed("Error generating skybox: " + e.Message);
            }
        }

        private void SetGenerateFailed(string message)
        {
            SetError(message);
            UpdateProgress(-1);
            SetState(State.Ready);
        }


#if PUSHER_PRESENT
        private async Task<ImagineResult> WaitForPusherResultAsync(string pusherChannel, string pusherEvent)
        {
            const string key = "a6a7b7662238ce4494d5";
            const string cluster = "mt1";

            var pusher = new Pusher(key, new PusherOptions()
            {
                Cluster = cluster,
                Encrypted = true
            });

            pusher.Error += (s, ex) => SetGenerateFailed("Pusher Exception: " + ex.Message);
            pusher.ConnectionStateChanged += (s, state) => LogVerbose("Pusher Connection State Changed: " + state);

            await pusher.ConnectAsync();

            var channel = await pusher.SubscribeAsync(pusherChannel);
            if (channel == null)
            {
                return null;
            }

            var tcs = new TaskCompletionSource<GetImagineRequest>();
            channel.Bind(pusherEvent, (string evt) =>
            {
                LogVerbose("Pusher Event: " + evt);
                var data = JsonConvert.DeserializeObject<PusherEvent>(evt).data;
                var request = JsonConvert.DeserializeObject<GetImagineRequest>(data);
                if (request.status == "error" || request.status == "complete")
                {
                    channel.Unbind(pusherEvent);
                    tcs.SetResult(request);
                }
            });

            LogVerbose("Waiting for Pusher event: " + pusherChannel + " " + pusherEvent);
            while (!tcs.Task.IsCompleted && !_isCancelled)
            {
                await WaitForSeconds(1);
                if (_percentageCompleted < 80)
                {
                    UpdateProgress(_percentageCompleted + 2);
                }
            }

            LogVerbose("Pusher received event.");
            await pusher.DisconnectAsync();

            var request = tcs.Task.Result;
            if (request.status == "error")
            {
                SetGenerateFailed(request.error_message);
                return null;
            }

            return request;
        }

        private struct PusherEvent
        {
            public string data;
        }
#endif

        private async Task<ImagineResult> PollForResultAsync(string imagineObfuscatedId)
        {
            while (!_isCancelled)
            {
                if (_percentageCompleted < 80)
                {
                    UpdateProgress(_percentageCompleted + 2);
                }

                await WaitForSeconds(1);
                if (_isCancelled)
                {
                    break;
                }

                var result = await ApiRequests.GetRequestStatusAsync(imagineObfuscatedId);
                if (_isCancelled)
                {
                    break;
                }

                if (result.status == Status.Error)
                {
                    SetGenerateFailed(result.error_message);
                    break;
                }

                if (result.status == Status.Complete)
                {
                    return result;
                }
            }

            return null;
        }

        private async Task WaitForSeconds(float seconds)
        {
#if UNITY_EDITOR
            await Task.Delay((int)(seconds * 1000)).ConfigureAwait(true);
#else
            var tcs = new TaskCompletionSource<object>();
            StartCoroutine(WaitForSecondsEnumerator(tcs, seconds));
            await tcs.Task;
#endif
        }

        private IEnumerator WaitForSecondsEnumerator(TaskCompletionSource<object> tcs, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            tcs.SetResult(null);
        }

#if UNITY_EDITOR
        internal async Task DownloadResultAsync(ImagineResult result, bool setSkybox = true)
        {
            var textureUrl = result.file_url;
            var depthMapUrl = result.depth_map_url;
            var hasDepthMap = !string.IsNullOrWhiteSpace(depthMapUrl);
            var prompt = result.prompt;

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
            var folderPath = GetOrCreateSkyboxFolder(prefix, result.id);
            var skyboxAIPath = $"{folderPath}/{prefix}.asset";
            var skyboxAI = AssetDatabase.LoadAssetAtPath<SkyboxAI>(skyboxAIPath);

            if (skyboxAI == null)
            {
                skyboxAI = ScriptableObject.CreateInstance<SkyboxAI>();
                skyboxAI.SetMetadata(result);
                AssetDatabase.CreateAsset(skyboxAI, skyboxAIPath);
            }
            else
            {
                skyboxAI.SetMetadata(result);
                EditorUtility.SetDirty(skyboxAI);
            }


            var tasks = new List<Task>();
            var texturePath = $"{folderPath}/{prefix} texture.png";
            var depthTexturePath = $"{folderPath}/{prefix} depth texture.png";

            if (skyboxAI.SkyboxTexture == null)
            {
                tasks.Add(ApiRequests.DownloadFileAsync(textureUrl, texturePath));
            }

            if (hasDepthMap && skyboxAI.DepthTexture == null)
            {
                tasks.Add(ApiRequests.DownloadFileAsync(depthMapUrl, depthTexturePath));
            }

            await Task.WhenAll(tasks).ConfigureAwait(true);

            if (_isCancelled)
            {
                Directory.Delete(folderPath, true);
                File.Delete($"{folderPath}.meta");
                AssetDatabase.Refresh();
                return;
            }

            if (tasks.Count > 0)
            {
                AssetDatabase.Refresh();
            }

            if (skyboxAI.SkyboxTexture == null)
            {
                var colorImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                colorImporter.maxTextureSize = 8192;
                colorImporter.textureCompression = TextureImporterCompression.Uncompressed;
                colorImporter.mipmapEnabled = false;
                colorImporter.textureShape = TextureImporterShape.TextureCube;
                colorImporter.SaveAndReimport();

                var skyboxTexture = AssetDatabase.LoadAssetAtPath<Cubemap>(texturePath);
                skyboxAI.SkyboxTexture = skyboxTexture;
            }

            if (hasDepthMap && skyboxAI.DepthTexture == null)
            {
                var depthImporter = (TextureImporter)AssetImporter.GetAtPath(depthTexturePath);
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
                }

                skyboxAI.VolumeProfile = volumeProfile;
                EditorUtility.SetDirty(skyboxAI);
            }

            if (setSkybox)
            {
                SetVolumeProfile(skyboxAI.VolumeProfile);
            }
#endif

            if (setSkybox)
            {
                _viewRemixImage = false;
                AssetDatabase.SaveAssetIfDirty(skyboxAI);
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
#if !UNITY_2021_1_OR_NEWERs
                    // ReSharper disable once MethodHasAsyncOverload
                    // WriteAllTextAsync not defined in Unity 2020.3
                    var skyboxData = File.ReadAllText(dataFile);
#else
                    var skyboxData = await File.ReadAllTextAsync(dataFile).ConfigureAwait(true);
#endif
                    var imagineResult = JsonConvert.DeserializeObject<GetImagineResult>(skyboxData).request;

                    if (imagineResult.id == skyboxId)
                    {
                        return currentDirectoryPath.ToProjectPath();
                    }
                }
            }

            // we didn't find a skyboxAI asset or json file, so we need to create a new folder
            return AssetUtils.CreateUniqueFolder(prefix);
        }
#else
        internal async Task DownloadResultAsync(ImagineResult result, bool setSkybox = true)
        {
            bool useComputeShader = SystemInfo.supportsComputeShaders && _cubemapComputeShader != null;

            var tasks = new List<Task<Texture2D>>();
            tasks.Add(ApiRequests.DownloadTextureAsync(result.file_url, !useComputeShader));
            if (!string.IsNullOrWhiteSpace(result.depth_map_url))
            {
                tasks.Add(ApiRequests.DownloadTextureAsync(result.depth_map_url));
            }

            var textures = await Task.WhenAll(tasks);

            if (_isCancelled)
            {
                ObjectUtils.Destroy(textures);
                return;
            }

            var skyboxAI = ScriptableObject.CreateInstance<SkyboxAI>();
            skyboxAI.SetMetadata(result);

            skyboxAI.SkyboxTexture = PanoramicToCubemap.Convert(
                textures[0], useComputeShader ? _cubemapComputeShader : null, 2048);

            ObjectUtils.Destroy(textures[0]);

            skyboxAI.DepthTexture = textures.Length > 1 ? textures[1] : null;
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
#endif

        private void SetSkyboxMetadata(SkyboxAI skybox)
        {
            Prompt = skybox.Prompt;
            SendNegativeText = !string.IsNullOrWhiteSpace(skybox.NegativeText);
            NegativeText = skybox.NegativeText;
            ModelVersion = skybox.Model;
            _styleFamilies = _modelVersion == SkyboxAiModelVersion.Model2 ? _model2Styles : _model3Styles;

            foreach (var family in _allModelStyleFamilies)
            {
                foreach (var style in family.items)
                {
                    if (style.id == skybox.SkyboxStyleId)
                    {
                        SelectedStyleFamily = family;
                        SelectedStyle = style;
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
            }
            else
            {
                SetDepthMaterial(_depthMaterial);
                SetSkyboxMaterial(_skyboxMaterial);
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
            if (_HDRPVolumeProfile == null)
            {
                return null;
            }

            var volumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            volumeProfile.name = _HDRPVolumeProfile.name;

            foreach (var item in _HDRPVolumeProfile.components)
            {
                var volumeComponent = Instantiate(item);

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
#endif

        private void UpdateProgress(float percentageCompleted)
        {
            _percentageCompleted = percentageCompleted;

#if UNITY_EDITOR
            bool showProgress = percentageCompleted >= 0 && percentageCompleted < 100;

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
#endif
        }

        public void Cancel()
        {
            _isCancelled = true;
            UpdateProgress(-1);
            SetState(State.Ready);
        }

        public void EditorPropertyChanged()
        {
            OnPropertyChanged?.Invoke();
        }

        [System.Diagnostics.Conditional("BLOCKADE_DEBUG")]
        private static void LogVerbose(string log)
        {
            Debug.Log(log);
        }
    }
}