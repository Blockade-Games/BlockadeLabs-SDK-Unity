using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

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
            get => SelectedStyleFamily?.items[_selectedStyleIndex];
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
            set => _negativeText = value;
        }

        [Tooltip("The seed for the random number generator. Use this to generate different skyboxes from the same prompt.")]
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

        [Tooltip("Give your existing world a new style by describing it")]
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

        private string _imagineObfuscatedId = "";
        private int _progressId;
        private float percentageCompleted = -1;
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

            SetState(State.Ready);
        }

        private List<SkyboxStyleField> CreateFields()
        {
            var fields = new List<SkyboxStyleField>();

            // add the default fields
            fields.AddRange(new List<SkyboxStyleField>
            {
                new SkyboxStyleField(
                    new UserInput(
                        "prompt",
                        1,
                        "Prompt",
                        _prompt,
                        "textarea"
                    )
                ),
                new SkyboxStyleField(
                    new UserInput(
                        "negative_text",
                        2,
                        "Negative text",
                        _negativeText,
                        "text"
                    )
                ),
                new SkyboxStyleField(
                    new UserInput(
                        "seed",
                        3,
                        "Seed",
                        _seed.ToString(),
                        "text"
                    )
                ),
                new SkyboxStyleField(
                    new UserInput(
                        "enhance_prompt",
                        4,
                        "Enhance prompt",
                        _enhancePrompt ? "true" : "false",
                        "boolean"
                    )
                ),
            });

            return fields;
        }

        public async void GenerateSkyboxAsync(bool runtime = false)
        {
            ClearError();
            SetState(State.Generating);
            _isCancelled = false;
            percentageCompleted = 1;

#if UNITY_EDITOR
            _progressId = Progress.Start("Generating Skybox Assets");
#endif

            var createSkyboxObfuscatedId = await ApiRequests.GenerateSkyboxAsync(CreateFields(), SelectedStyle.id, _apiKey);

            InitializeGetAssets(runtime, createSkyboxObfuscatedId);
        }

        private void InitializeGetAssets(bool runtime, string createImagineObfuscatedId)
        {
            if (createImagineObfuscatedId != "")
            {
                _imagineObfuscatedId = createImagineObfuscatedId;
                percentageCompleted = 33;
                CalculateProgress();

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
                    _ = GetAssetsAsync();
                }
            }
        }

        public async Task GetAssetsAsync()
        {
            var textureUrl = "";
            var depthMapUrl = "";
            var prompt = "";
            var count = 0;

            while (!_isCancelled)
            {
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif

                await Task.Delay(1000);

                if (_isCancelled)
                {
                    break;
                }

                count++;

                var getImagineResult = await ApiRequests.GetImagineAsync(_imagineObfuscatedId, _apiKey);

                if (getImagineResult.Count > 0)
                {
                    percentageCompleted = 66;
                    CalculateProgress();
                    textureUrl = getImagineResult["textureUrl"];
                    depthMapUrl = getImagineResult["depthMapUrl"];
                    prompt = getImagineResult["prompt"];
                    break;
                }
            }

            if (_isCancelled)
            {
                percentageCompleted = -1;
                _imagineObfuscatedId = "";
                return;
            }

            if (!string.IsNullOrWhiteSpace(textureUrl))
            {
                var image = await ApiRequests.GetImagineImageAsync(textureUrl);
                var texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
                texture.LoadImage(image);

                percentageCompleted = 80;
                CalculateProgress();

                if (_assignToMaterial)
                {
                    var r = GetComponent<Renderer>();

                    if (r != null)
                    {
                        if (r.sharedMaterial != null)
                        {
                            r.sharedMaterial.mainTexture = texture;
                        }
                    }
                }

                percentageCompleted = 90;
                CalculateProgress();

                texture.Compress(true);

                var depthMapEmpty = string.IsNullOrWhiteSpace(depthMapUrl);
                var depthMapTexture = new Texture2D(1, 1, TextureFormat.RGB24, false); ;

                if (!depthMapEmpty)
                {
                    var depthMapImage = await ApiRequests.GetImagineImageAsync(depthMapUrl);
                    depthMapTexture.LoadImage(depthMapImage);
                    depthMapTexture.Compress(true);
                }

                SaveAssets(texture, prompt, depthMapEmpty, depthMapTexture);
            }

            percentageCompleted = 100;
            CalculateProgress();
#if UNITY_EDITOR
            Progress.Remove(_progressId);
#endif
            SetState(State.Ready);
        }

        private void SaveAssets(Texture2D texture, string prompt, bool depthMapEmpty, Texture2D depthMapTexture)
        {
#if UNITY_EDITOR
            if (AssetDatabase.Contains(texture) || (!depthMapEmpty && AssetDatabase.Contains(depthMapTexture)))
            {
                Debug.Log("Texture already in assets database.");
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Blockade Labs SDK Assets"))
            {
                AssetDatabase.CreateFolder("Assets", "Blockade Labs SDK Assets");
            }

            var maxLength = 20;

            if (prompt.Length > maxLength)
            {
                prompt = prompt.Substring(0, maxLength);
            }

            var validatedPrompt = ValidateFilename(prompt);
            var textureName = validatedPrompt + "_texture";
            CreateAsset(textureName, texture);

            if (!depthMapEmpty)
            {
                var depthMapTextureName = validatedPrompt + "_depth_map_texture";
                CreateAsset(depthMapTextureName, depthMapTexture);
            }
#endif
            _imagineObfuscatedId = "";
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

        private void CalculateProgress()
        {
#if UNITY_EDITOR
            Progress.Report(_progressId, percentageCompleted / 100f);
#endif
        }

        public float PercentageCompleted() => percentageCompleted;

        public void Cancel()
        {
            _isCancelled = true;
            percentageCompleted = -1;
#if UNITY_EDITOR
            Progress.Remove(_progressId);
#endif
        }

        private void CreateAsset(string textureName, Texture2D texture)
        {
#if UNITY_EDITOR
            var counter = 0;

            while (true)
            {
                var modifiedTextureName = counter == 0 ? textureName : textureName + "_" + counter;

                var textureAssets =
                    AssetDatabase.FindAssets(modifiedTextureName, new[] { "Assets/Blockade Labs SDK Assets" });

                if (textureAssets.Length > 0)
                {
                    counter++;
                    continue;
                }

                AssetDatabase.CreateAsset(texture, "Assets/Blockade Labs SDK Assets/" + modifiedTextureName + ".asset");
                break;
            }
#endif
        }

        public void EditorPropertyChanged()
        {
            OnPropertyChanged?.Invoke();
        }
    }
}