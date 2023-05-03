using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK
{
    public class BlockadeImaginarium : MonoBehaviour
    {
        [Tooltip("API Key from Blockade Labs")] [SerializeField]
        public string apiKey;

        [Tooltip("Specifies if in-game Skybox GUI should be displayed")] [SerializeField]
        public bool enableSkyboxGUI = false;

        [Tooltip("Specifies if the result should automatically be assigned as the texture of the current game objects renderer material")]
        [SerializeField]
        public bool assignToMaterial = false;
        
        public List<SkyboxStyleField> skyboxStyleFields;
        public List<SkyboxStyle> skyboxStyles;
        public string[] skyboxStyleOptions;
        public int skyboxStyleOptionsIndex = 0;
        public int lastSkyboxStyleOptionsIndex = 0;
        public string imagineObfuscatedId = "";
        private int progressId;
        GUIStyle guiStyle;

        [HideInInspector] private float percentageCompleted = -1;
        private bool isCancelled;

        public void OnGUI()
        {
            if (enableSkyboxGUI)
            {
                DrawSkyboxGUILayout();
            }
        }

        private void DrawSkyboxGUILayout()
        {
            DefineStyles();

            GUILayout.BeginArea(new Rect(Screen.width - (Screen.width / 3), 0, 300, Screen.height), guiStyle);

            if (GUILayout.Button("Get Styles"))
            {
                _ = GetSkyboxStyleOptions();
            }

            // Iterate over skybox fields and render them
            if (skyboxStyleFields.Count > 0)
            {
                RenderSkyboxInGameFields();
            }

            GUILayout.EndArea();
        }

        private void RenderSkyboxInGameFields()
        {
            GUILayout.BeginVertical("Box");
            skyboxStyleOptionsIndex = GUILayout.SelectionGrid(skyboxStyleOptionsIndex, skyboxStyleOptions, 1);
            GUILayout.EndVertical();

            if (skyboxStyleOptionsIndex != lastSkyboxStyleOptionsIndex)
            {
                GetSkyboxStyleFields(skyboxStyleOptionsIndex);
                lastSkyboxStyleOptionsIndex = skyboxStyleOptionsIndex;
            }

            foreach (var field in skyboxStyleFields)
            {
                // Begin horizontal layout
                GUILayout.BeginHorizontal();

                // Create label for field
                GUILayout.Label(field.name);

                // Create text field for field value
                field.value = GUILayout.TextField(field.value);

                // End horizontal layout
                GUILayout.EndHorizontal();
            }

            if (PercentageCompleted() >= 0 && PercentageCompleted() < 100)
            {
                if (GUILayout.Button("Cancel (" + PercentageCompleted() + "%)"))
                {
                    Cancel();
                }
            }
            else
            {
                if (GUILayout.Button("Generate"))
                {
                    _ = InitializeSkyboxGeneration(skyboxStyleFields, skyboxStyles[skyboxStyleOptionsIndex].id, true);
                }
            }
        }

        private void DefineStyles()
        {
            guiStyle = new GUIStyle();
            guiStyle.fontSize = 20;
            guiStyle.normal.textColor = Color.white;
            guiStyle.normal.background = new Texture2D(1, 1);
            guiStyle.normal.background.SetPixel(0, 0, Color.blue);
            guiStyle.normal.background.Apply();
            guiStyle.margin = new RectOffset(20, 20, 20, 20);
            guiStyle.padding = new RectOffset(20, 20, 20, 20);
        }

        public async Task GetSkyboxStyleOptions()
        {
            skyboxStyles = await ApiRequests.GetSkyboxStyles(apiKey);
            skyboxStyleOptions = skyboxStyles.Select(s => s.name).ToArray();
            
            GetSkyboxStyleFields(skyboxStyleOptionsIndex);
        }

        public void GetSkyboxStyleFields(int index)
        {
            skyboxStyleFields = new List<SkyboxStyleField>(); 

            // add the default prompt field
            var promptField = new SkyboxStyleField(
                new UserInput(
                    "prompt",
                    1,
                    "Prompt",
                    ""
                )
            );
            
            skyboxStyleFields.Add(promptField);
        }

        public async Task InitializeSkyboxGeneration(List<SkyboxStyleField> skyboxStyleFields, int id,
            bool runtime = false)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Debug.Log("You need to provide an Api Key in api options.");
                return;
            }

            isCancelled = false;
            await CreateSkybox(skyboxStyleFields, id, runtime);
        }

        async Task CreateSkybox(List<SkyboxStyleField> skyboxStyleFields, int id, bool runtime = false)
        {
            percentageCompleted = 1;

            #if UNITY_EDITOR
                progressId = Progress.Start("Generating Skybox Assets");
            #endif

            var createSkyboxObfuscatedId = await ApiRequests.CreateSkybox(skyboxStyleFields, id, apiKey);

            InitializeGetAssets(runtime, createSkyboxObfuscatedId);
        }

        private void InitializeGetAssets(bool runtime, string createImagineObfuscatedId)
        {
            if (createImagineObfuscatedId != "")
            {
                imagineObfuscatedId = createImagineObfuscatedId;
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
                    _ = GetAssets();
                }
            }
        }

        public async Task GetAssets()
        {
            var textureUrl = "";
            var prompt = "";
            var count = 0;

            while (!isCancelled)
            {
                #if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
                #endif

                await Task.Delay(1000);

                if (isCancelled)
                {
                    break;
                }

                count++;

                var getImagineResult = await ApiRequests.GetImagine(imagineObfuscatedId, apiKey);

                if (getImagineResult.Count > 0)
                {
                    percentageCompleted = 66;
                    CalculateProgress();
                    textureUrl = getImagineResult["textureUrl"];
                    prompt = getImagineResult["prompt"];
                    break;
                }
            }

            if (isCancelled)
            {
                percentageCompleted = -1;
                imagineObfuscatedId = "";
                return;
            }

            if (!string.IsNullOrWhiteSpace(textureUrl))
            {
                var image = await ApiRequests.GetImagineImage(textureUrl);

                var texture = new Texture2D(512, 512, TextureFormat.RGB24, false);
                texture.LoadImage(image);

                percentageCompleted = 80;
                CalculateProgress();

                if (assignToMaterial)
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
                SaveAssets(texture, prompt);
            }

            percentageCompleted = 100;
            CalculateProgress();
            #if UNITY_EDITOR
                Progress.Remove(progressId);
            #endif
        }

        private void SaveAssets(Texture2D texture, string prompt)
        {
            #if UNITY_EDITOR
                if (AssetDatabase.Contains(texture))
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

                var textureName = ValidateFilename(prompt) + "_texture";

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

            imagineObfuscatedId = "";
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
                Progress.Report(progressId, percentageCompleted / 100f);
            #endif
        }

        public float PercentageCompleted() => percentageCompleted;

        public void Cancel()
        {
            isCancelled = true;
            percentageCompleted = -1;
            #if UNITY_EDITOR
                Progress.Remove(progressId);
            #endif
        }
    }
}