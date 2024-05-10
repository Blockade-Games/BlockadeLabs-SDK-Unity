using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if PUSHER_PRESENT
using PusherClient;
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
            set => _apiKey = value;
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
                    Reload();
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

        [SerializeField]
        private int _selectedStyleFamilyIndex;
        public SkyboxStyleFamily SelectedStyleFamily
        {
            get => _styleFamilies?[_selectedStyleFamilyIndex];
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
            get => SelectedStyleFamily?.items?[_selectedStyleIndex];
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

        public enum State
        {
            NeedApiKey,
            Ready,
            Generating
        }

        private State _state = State.NeedApiKey;
        public State CurrentState => _state;
        public event Action OnStateChanged;

        private string _lastError;
        public string LastError => _lastError;
        public event Action OnErrorChanged;

        public event Action OnPropertyChanged;

        private float _percentageCompleted = -1;
        public float PercentageCompleted => _percentageCompleted;

        private bool _isCancelled;

        public bool CanRemix => _modelVersion == SkyboxAiModelVersion.Model2 &&
             (_skyboxMesh?.GetRemixId().HasValue ?? false);

#if UNITY_EDITOR
        private int _progressId = 0;
#endif

        public bool CheckApiKeyValid()
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Contains("api.blockadelabs.com"))
            {
                _state = State.NeedApiKey;
                return false;
            }

            return true;
        }

        private void SetState(State state)
        {
            _state = state;
            OnStateChanged?.Invoke();
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

            _styleFamilies = await ApiRequests.GetSkyboxStylesMenuAsync(_apiKey, _modelVersion);
            if (_styleFamilies == null || _styleFamilies.Count == 0)
            {
                SetError("Something went wrong. Please recheck you API key.");
                return;
            }

            // Remove anything with status: "disabled"
            _styleFamilies.ForEach(x => x.items?.RemoveAll(y => y.status == "disabled"));
            _styleFamilies.RemoveAll(x => x.status == "disabled" || x.items?.Count == 0);

            // Ensure each style has a family to simplify logic everywhere.
            for (int i = 0; i < _styleFamilies.Count; i++)
            {
                if (_styleFamilies[i].type == "style")
                {
                    var style = _styleFamilies[i];
                    _styleFamilies[i] = new SkyboxStyleFamily
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

            _selectedStyleFamilyIndex = Math.Min(_selectedStyleFamilyIndex, _styleFamilies.Count - 1);
            _selectedStyleIndex = Math.Min(_selectedStyleIndex, _styleFamilies[_selectedStyleFamilyIndex].items.Count - 1);

            OnPropertyChanged?.Invoke();
            SetState(State.Ready);
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
                negative_text = _negativeText,
                seed = _seed,
                enhance_prompt = _enhancePrompt,
                skybox_style_id = SelectedStyle.id,
            };

            if (CanRemix && _remix && !TrySetRemixId(request))
            {
                return;
            }

            ClearError();
            SetState(State.Generating);
            _isCancelled = false;
            UpdateProgress(5);

            try
            {
                var response = await ApiRequests.GenerateSkyboxAsync(request, _apiKey);
                if (_isCancelled)
                {
                    return;
                }

                if (response == null)
                {
                    throw new Exception("Error generating skybox.");
                }

                if (response.status == "error")
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

        private bool TrySetRemixId(CreateSkyboxRequest request)
        {
            var remixId = _skyboxMesh?.GetRemixId();
            if (!remixId.HasValue)
            {
                SetError("Missing skybox ID. Please use a previously generated skybox or disable remix.");
                return false;
            }

            request.remix_imagine_id = remixId.Value;
            return true;
        }

#if PUSHER_PRESENT
        private async Task<GetImagineResult> WaitForPusherResultAsync(string pusherChannel, string pusherEvent)
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

            return new GetImagineResult { request = request };
        }

        private struct PusherEvent
        {
            public string data;
        }
#endif

        private async Task<GetImagineResult> PollForResultAsync(string imagineObfuscatedId)
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

                var result = await ApiRequests.GetRequestStatusAsync(imagineObfuscatedId, _apiKey);
                if (_isCancelled)
                {
                    break;
                }

                if (result.request.status == "error")
                {
                    SetGenerateFailed(result.request.error_message);
                    break;
                }

                if (result.request.status == "complete")
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
        private async Task DownloadResultAsync(GetImagineResult result)
        {
            var textureUrl = result.request.file_url;
            var depthMapUrl = result.request.depth_map_url;
            bool haveDepthMap = !string.IsNullOrWhiteSpace(depthMapUrl);
            var prompt = result.request.prompt;

            if (string.IsNullOrWhiteSpace(textureUrl))
            {
                SetGenerateFailed("Returned skybox texture url is empty.");
                return;
            }

            var maxLength = 20;
            if (prompt.Length > maxLength)
            {
                prompt = prompt.Substring(0, maxLength);
            }

            var prefix = AssetUtils.CreateValidFilename(prompt);
            var folderPath = AssetUtils.CreateUniqueFolder(prefix);
            var texturePath = folderPath + "/" + prefix + " texture.png";
            var depthTexturePath = folderPath + "/" + prefix + " depth texture.png";
            var resultsPath = folderPath + "/" + prefix + " data.txt";

            var tasks = new List<Task>();
            tasks.Add(ApiRequests.DownloadFileAsync(textureUrl, texturePath));
            if (haveDepthMap)
            {
                tasks.Add(ApiRequests.DownloadFileAsync(depthMapUrl, depthTexturePath));
            }

            // WriteAllTextAsync not defined in Unity 2020.3
            File.WriteAllText(resultsPath, JsonConvert.SerializeObject(result));

            await Task.WhenAll(tasks);
            if (_isCancelled)
            {
                Directory.Delete(folderPath, true);
                return;
            }

            if (_skyboxMesh)
            {
                _skyboxMesh.SetMetadata(result);
            }

            UpdateProgress(99);

            AssetDatabase.Refresh();

            var colorImporter = TextureImporter.GetAtPath(texturePath) as TextureImporter;
            colorImporter.maxTextureSize = 8192;
            colorImporter.textureCompression = TextureImporterCompression.Uncompressed;
            colorImporter.mipmapEnabled = false;
            colorImporter.textureShape = TextureImporterShape.TextureCube;
            colorImporter.SaveAndReimport();

            if (haveDepthMap)
            {
                var depthImporter = TextureImporter.GetAtPath(depthTexturePath) as TextureImporter;
                depthImporter.maxTextureSize = 2048;
                depthImporter.textureCompression = TextureImporterCompression.Uncompressed;
                depthImporter.mipmapEnabled = false;
                depthImporter.wrapModeU = TextureWrapMode.Repeat;
                depthImporter.wrapModeV = TextureWrapMode.Clamp;
                depthImporter.SaveAndReimport();
            }

            var colorTexture = AssetDatabase.LoadAssetAtPath<Cubemap>(texturePath);

            var depthTexture = AssetDatabase.LoadAssetAtPath<Texture>(depthTexturePath);
            var depthMaterial = CreateDepthMaterial(colorTexture, depthTexture);
            if (depthMaterial != null)
            {
                AssetDatabase.CreateAsset(depthMaterial, folderPath + "/" + prefix + " depth material.mat");
            }

            var skyboxMaterial = CreateSkyboxMaterial(colorTexture);
            if (skyboxMaterial != null)
            {
                AssetDatabase.CreateAsset(skyboxMaterial, folderPath + "/" + prefix + " skybox material.mat");
                AssetUtils.PingAsset(skyboxMaterial);
            }

#if UNITY_HDRP
            var volumeProfile = CreateVolumeProfile(colorTexture);
            if (volumeProfile != null)
            {
                AssetDatabase.CreateAsset(volumeProfile, folderPath + "/" + prefix + " HDRP volume profile.asset");
            }
#endif
        }

        private string ValidateFilename(string prompt)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                prompt = prompt.Replace(c, '_');
            }

            while (prompt.Contains("__"))
            {
                prompt = prompt.Replace("__", "_");
            }

            return prompt.TrimStart('_').TrimEnd('_');
        }
#else
        private async Task DownloadResultAsync(GetImagineResult result)
        {
            bool useComputeShader = SystemInfo.supportsComputeShaders && _cubemapComputeShader != null;

            var tasks = new List<Task<Texture2D>>();
            tasks.Add(ApiRequests.DownloadTextureAsync(result.request.file_url, !useComputeShader));
            if (!string.IsNullOrWhiteSpace(result.request.depth_map_url))
            {
                tasks.Add(ApiRequests.DownloadTextureAsync(result.request.depth_map_url));
            }

            var textures = await Task.WhenAll(tasks);

            if (_isCancelled)
            {
                ObjectUtils.Destroy(textures);
                return;
            }

            Cubemap cubemap;

            if (useComputeShader)
            {
                cubemap = PanoramicToCubemap.Convert(textures[0], _cubemapComputeShader, 2048);
            }
            else
            {
                cubemap = PanoramicToCubemap.Convert(textures[0], 2048);
            }

            ObjectUtils.Destroy(textures[0]);

            if (_skyboxMesh)
            {
                _skyboxMesh.SetMetadata(result);
            }

            CreateDepthMaterial(cubemap, textures.Length > 1 ? textures[1] : null);

            CreateSkyboxMaterial(cubemap);
#if UNITY_HDRP
            CreateVolumeProfile(cubemap);
#endif
        }
#endif

        private Material CreateDepthMaterial(Texture texture, Texture depthTexture)
        {
            if (_depthMaterial == null)
            {
                return null;
            }

            var material = new Material(_depthMaterial);
            material.mainTexture = texture;
            if (material.HasProperty("_DepthMap"))
            {
                material.SetTexture("_DepthMap", depthTexture);
            }

            if (_skyboxMesh.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.sharedMaterial = material;
            }

            return material;
        }

        private Material CreateSkyboxMaterial(Texture texture)
        {
            if (_skyboxMaterial == null)
            {
                return null;
            }

            var material = new Material(_skyboxMaterial);
            material.SetTexture("_Tex", texture);
            RenderSettings.skybox = material;
            DynamicGI.UpdateEnvironment();
            return material;
        }

#if UNITY_HDRP
        private VolumeProfile CreateVolumeProfile(Cubemap skyTexture)
        {
            if (_HDRPVolume == null || _HDRPVolumeProfile == null)
            {
                return null;
            }

            var hdrpAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "Unity.RenderPipelines.HighDefinition.Runtime");

            if (hdrpAssembly == null)
            {
                return null;
            }

            // This does a deep copy, while Instantiate does not.
            _HDRPVolume.sharedProfile = _HDRPVolumeProfile;
            var volumeProfile = _HDRPVolume.profile;

            var hdrpSkyType = hdrpAssembly.GetType("UnityEngine.Rendering.HighDefinition.HDRISky");
            if (volumeProfile.TryGet<VolumeComponent>(hdrpSkyType, out var sky))
            {
                var hdriSkyField = hdrpSkyType.GetField("hdriSky");
                var overrideMethod = hdriSkyField.FieldType.GetMethod("Override", new[] { typeof(Cubemap) });
                var hdriSkyValue = hdriSkyField.GetValue(sky);
                overrideMethod.Invoke(hdriSkyValue, new object[] { skyTexture });
            }

            _HDRPVolume.sharedProfile = volumeProfile;
            return volumeProfile;
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