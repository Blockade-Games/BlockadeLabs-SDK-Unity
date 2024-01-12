using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

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

        [SerializeField]
        private BlockadeLabsSkybox _skybox;
        public BlockadeLabsSkybox Skybox
        {
            get => _skybox;
            set => _skybox = value;
        }

        [SerializeField]
        private Material _skyboxMaterial;
        public Material SkyboxMaterial
        {
            get => _skyboxMaterial;
            set => _skyboxMaterial = value;
        }

        [SerializeField]
        private Material _depthMaterial;
        public Material DepthMaterial
        {
            get => _depthMaterial;
            set => _depthMaterial = value;
        }

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

        public bool CanRemix => _skybox?.GetRemixId().HasValue ?? false;

#if UNITY_EDITOR
        private int _progressId = 0;
#endif

        public bool CheckApiKeyValid()
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Contains("api.blockadelabs.com"))
            {
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

        public async Task LoadAsync()
        {
            ClearError();

            _styleFamilies = await ApiRequests.GetSkyboxStylesMenuAsync(_apiKey);
            if (_styleFamilies == null || _styleFamilies.Count == 0)
            {
                SetError("Something went wrong. Please recheck you API key.");
                return;
            }

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
                        items = new List<SkyboxStyle> { style }
                    };
                }

                _selectedStyleFamilyIndex = Math.Min(_selectedStyleFamilyIndex, _styleFamilies.Count - 1);
                _selectedStyleIndex = Math.Min(_selectedStyleIndex, _styleFamilies[_selectedStyleFamilyIndex].items.Count - 1);
            }

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

            if (_remix && !TrySetRemixId(request))
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
            var remixId = _skybox.GetRemixId();
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
                await Task.Delay(1000);
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

                await Task.Delay(1000);
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

        private void DestroyTextures(Texture2D[] textures)
        {
            foreach (var texture in textures)
            {
                if (texture)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(texture);
                    }
                    else
                    {
                        DestroyImmediate(texture);
                    }
                }
            }
        }

#if UNITY_EDITOR
        private async Task DownloadResultAsync(GetImagineResult result)
        {
            var textureUrl = result.request.file_url;
            var depthMapUrl = result.request.depth_map_url;
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
            var folderPath = AssetUtils.GetOrCreateFolder(prefix);
            var texturePath = folderPath + "/" + prefix + "_texture.png";
            var depthTexturePath = folderPath + "/" + prefix + "_depth_map_texture.png";
            var resultsPath = folderPath + "/" + prefix + "_data.txt";

            var tasks = new List<Task>();
            tasks.Add(ApiRequests.DownloadFileAsync(textureUrl, texturePath));
            if (!string.IsNullOrWhiteSpace(depthMapUrl))
            {
                tasks.Add(ApiRequests.DownloadFileAsync(depthMapUrl, depthTexturePath));
            }

            // WriteAllTextAsync not defined in Unity 20202.3
            File.WriteAllText(resultsPath, JsonConvert.SerializeObject(result));

            await Task.WhenAll(tasks);
            if (_isCancelled)
            {
                Directory.Delete(folderPath, true);
                return;
            }

            UpdateProgress(99);

            AssetDatabase.Refresh();

            var importer = TextureImporter.GetAtPath(texturePath) as TextureImporter;
            importer.maxTextureSize = 16384;
            importer.compressionQuality = 100;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            var colorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            var depthTexture = tasks.Count > 1 ? AssetDatabase.LoadAssetAtPath<Texture2D>(depthTexturePath) : null;

            var depthMaterial = CreateDepthMaterial(colorTexture, depthTexture, result.request.id);
            AssetDatabase.CreateAsset(depthMaterial, folderPath + "/" + prefix + "_depth_material.mat");

            var skyboxMaterial = CreateSkyboxMaterial(colorTexture);
            AssetDatabase.CreateAsset(skyboxMaterial, folderPath + "/" + prefix + "_material.mat");
            EditorGUIUtility.PingObject(skyboxMaterial);
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
            var tasks = new List<Task<Texture2D>>();
            tasks.Add(ApiRequests.DownloadTextureAsync(result.request.file_url));
            if (!string.IsNullOrWhiteSpace(result.request.depth_map_url))
            {
                tasks.Add(ApiRequests.DownloadTextureAsync(result.request.depth_map_url));
            }

            var textures = await Task.WhenAll(tasks);

            if (_isCancelled)
            {
                DestroyTextures(textures);
                return;
            }

            CreateMaterial(textures[0], textures.Length > 1 ? textures[1] : null, result.request.id);

            var panoramicMaterial = CreatePanoramicMaterial();

            // set the material on the scene camera
            Camera.main.GetComponent<Skybox>().material = panoramicMaterial;
        }
#endif

        private Material CreateDepthMaterial(Texture2D texture, Texture2D depthTexture, int remixId)
        {
            var material = new Material(_depthMaterial);
            material.mainTexture = texture;
            if (material.HasProperty("_DepthMap"))
            {
                material.SetTexture("_DepthMap", depthTexture);
            }

            if (_skybox)
            {
                _skybox.SetSkyboxMaterial(material, remixId);
            }

            return material;
        }

        private Material CreateSkyboxMaterial(Texture2D texture)
        {
            var material = new Material(_skyboxMaterial);
            material.mainTexture = texture;
            RenderSettings.skybox = material;
            foreach (var reflectionProbe in FindObjectsOfType<ReflectionProbe>())
            {
                reflectionProbe.RenderProbe();
            }

            return material;
        }

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

        [System.Diagnostics.Conditional("BLOCKADE_SDK_LOG")]
        private static void LogVerbose(string log)
        {
            Debug.Log(log);
        }
    }
}