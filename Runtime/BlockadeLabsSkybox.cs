using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

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
                _prompt = value;
                OnPropertyChanged?.Invoke();
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
                _negativeText = value;
                OnPropertyChanged?.Invoke();
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
                _seed = value;
                OnPropertyChanged?.Invoke();
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
                _enhancePrompt = value;
                OnPropertyChanged?.Invoke();
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
                _remix = value;
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
        public event Action OnStateChanged;

        private string _lastError;
        public string LastError => _lastError;
        public event Action OnErrorChanged;

        public event Action OnPropertyChanged;

        private float _percentageCompleted = -1;
        public float PercentageCompleted => _percentageCompleted;

        private int _progressId = 0;
        private bool _isCancelled;

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
                SetGenerateFailed("Error generating skybox: " + e.Message);
            }
        }

        private void SetGenerateFailed(string message)
        {
            SetError(message);
            UpdateProgress(-1);
            SetState(State.Ready);
        }

        public bool TrySetRemixId(CreateSkyboxRequest request)
        {
            if (!TryGetComponent<Renderer>(out var renderer) || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null)
            {
                SetError("Remix skybox requires a skybox texture to remix.");
                return false;
            }

            var texturePath = AssetDatabase.GetAssetPath(renderer.sharedMaterial.mainTexture);
            var resultPath = texturePath.Substring(0, texturePath.LastIndexOf('_')) + "_data.txt";
            if (!File.Exists(resultPath))
            {
                SetError("Could not find skybox data file to remix: " + resultPath);
                return false;
            }

            var result = JsonConvert.DeserializeObject<GetImagineResult>(File.ReadAllText(resultPath));
            request.remix_imagine_id = result.request.id;
            return true;
        }

        public async Task PollGenerateStatusAsync(string imagineObfuscatedId)
        {
            while (!_isCancelled)
            {
                await Task.Delay(1000);
                if (_isCancelled)
                {
                    break;
                }

                var response = await ApiRequests.GetRequestStatusAsync(imagineObfuscatedId, _apiKey);
                if (_isCancelled)
                {
                    break;
                }

                if (response == null)
                {
                    SetGenerateFailed("Error generating skybox.");
                    break;
                }

                var result = JsonConvert.DeserializeObject<GetImagineResult>(response);
                if (result.request.status == "error")
                {
                    SetGenerateFailed(result.request.error_message);
                    break;
                }

                if (result.request.status == "complete")
                {
                    UpdateProgress(66);
                    await OnGenerateComplete(result, response);
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

        private async Task OnGenerateComplete(GetImagineResult result, string response)
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

            UpdateProgress(80);

            if (_assignToMaterial && TryGetComponent<Renderer>(out var renderer) && renderer.sharedMaterial != null)
            {
#if UNITY_EDITOR
                Undo.RecordObject(renderer.sharedMaterial, "Assign Skybox Texture");
#endif
                renderer.sharedMaterial.mainTexture = textures[0];
            }

#if UNITY_EDITOR
            SaveAssets(textures[0], prompt, textures.Length > 1 ? textures[1] : null, response);
#endif

            UpdateProgress(100);

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