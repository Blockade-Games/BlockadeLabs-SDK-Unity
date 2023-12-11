using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlockadeLabsSDK
{
    public class BlockadeLabsSkybox : MonoBehaviour
    {
        [Tooltip("API Key from Blockade Labs. Get one at api.blockadelabs.com")]
        [SerializeField]
        private string _apiKey = "API key needed. Get one at api.blockadelabs.com";
        public string ApiKey
        {
            get => _apiKey;
            set => _apiKey = value;
        }

        [Tooltip("Specifies if the result should automatically be assigned as the texture of the current game objects renderer material")]
        [SerializeField]
        private bool _assignToMaterial = true;
        public bool AssignToMaterial
        {
            get => _assignToMaterial;
            set => _assignToMaterial = value;
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

        private int _lastGeneratedId;
        private Texture2D _lastGeneratedTexture;

        public bool CanRemix => GetRemixId().HasValue;

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

        public async void GenerateSkyboxAsync(bool runtime = false)
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

                UpdateProgress(33);

                var pusherManager = false;

    #if PUSHER_PRESENT
                pusherManager = FindObjectOfType<PusherManager>();
    #endif

                if (pusherManager && runtime)
                {
    #if PUSHER_PRESENT
                    _ = PusherManager.instance.SubscribeToChannel(imagineObfuscatedId);
    #endif
                }
                else
                {
                    await PollGenerateStatusAsync(response.obfuscated_id);
                }
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

        private int? GetRemixId()
        {
            if (!TryGetComponent<Renderer>(out var renderer) || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null)
            {
                return null;
            }

            if (renderer.sharedMaterial.mainTexture.name == "default_skybox_texture")
            {
                return 0;
            }

            if (renderer.sharedMaterial.mainTexture == _lastGeneratedTexture)
            {
                return _lastGeneratedId;
            }

#if UNITY_EDITOR
            // In editor, read the remix ID from the data file saved next to the texture.
            var texturePath = AssetDatabase.GetAssetPath(renderer.sharedMaterial.mainTexture);
            var resultPath = texturePath.Substring(0, texturePath.LastIndexOf('_')) + "_data.txt";
            if (File.Exists(resultPath))
            {
                return JsonConvert.DeserializeObject<GetImagineResult>(File.ReadAllText(resultPath)).request.id;
            }
#endif
            return null;
        }

        private bool TrySetRemixId(CreateSkyboxRequest request)
        {
            var remixId = GetRemixId();
            if (!remixId.HasValue)
            {
                SetError("Missing skybox ID. Please use a previously generated skybox or disable remix.");
                return false;
            }

            request.remix_imagine_id = remixId.Value;
            return true;
        }

        public async Task PollGenerateStatusAsync(string imagineObfuscatedId)
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
                    UpdateProgress(80);
                    await OnGenerateComplete(result);
                    break;
                }
            }
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

        private async Task OnGenerateComplete(GetImagineResult result)
        {
            var textureUrl = result.request.file_url;
            var depthMapUrl = result.request.depth_map_url;
            var prompt = result.request.prompt;

            if (string.IsNullOrWhiteSpace(textureUrl))
            {
                SetGenerateFailed("Returned skybox texture url is empty.");
                return;
            }

            var tasks = new List<Task<Texture2D>>();
            tasks.Add(ApiRequests.DownloadTextureAsync(textureUrl));
            if (!string.IsNullOrWhiteSpace(depthMapUrl))
            {
                tasks.Add(ApiRequests.DownloadTextureAsync(depthMapUrl));
            }

            var textures = await Task.WhenAll(tasks);
            if (_isCancelled)
            {
                DestroyTextures(textures);
                return;
            }

            foreach (var texture in textures)
            {
                if (!texture)
                {
                    DestroyTextures(textures);
                    SetGenerateFailed("Error downloading textures.");
                    return;
                }
            }

            foreach (var texture in textures)
            {
                texture.Compress(true);
            }

            UpdateProgress(99);

            if (_assignToMaterial && TryGetComponent<Renderer>(out var renderer) && renderer.sharedMaterial != null)
            {
#if UNITY_EDITOR
                Undo.RecordObject(renderer.sharedMaterial, "Assign Skybox Texture");
#endif
                renderer.sharedMaterial.mainTexture = textures[0];
            }

#if UNITY_EDITOR
            var resultJson = JsonConvert.SerializeObject(result);
            var depthTexture = textures.Length > 1 ? textures[1] : null;
            SaveAssets(textures[0], prompt, depthTexture, resultJson);
#endif

            _lastGeneratedId = result.request.id;
            _lastGeneratedTexture = textures[0];
            UpdateProgress(0);
            SetState(State.Ready);
        }

#if UNITY_EDITOR
        private void SaveAssets(Texture2D texture, string prompt, Texture2D depthMapTexture, string response)
        {
            var generateFolder = "Blockade Labs SDK";
            var pathFromAssets = "Assets/" + generateFolder;
            if (!AssetDatabase.IsValidFolder(pathFromAssets))
            {
                AssetDatabase.CreateFolder("Assets", generateFolder);
            }

            var maxLength = 20;
            if (prompt.Length > maxLength)
            {
                prompt = prompt.Substring(0, maxLength);
            }

            var prefix = ValidateFilename(prompt);

            // Create a folder to store the new assets
            var folderPath = AssetDatabase.GenerateUniqueAssetPath(pathFromAssets + "/" + prefix);
            AssetDatabase.CreateFolder(pathFromAssets, prefix);

            // Create the texture asset
            AssetDatabase.CreateAsset(texture, folderPath + "/" + prefix + "_texture.asset");

            if (depthMapTexture)
            {
                // Create the depth map asset

                AssetDatabase.CreateAsset(depthMapTexture, folderPath + "/" + prefix + "_depth_map_texture.asset");
            }

            // Save the response so we can use it later to remix
            File.WriteAllText(folderPath + "/" + prefix + "_data.txt", response);
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
    }
}