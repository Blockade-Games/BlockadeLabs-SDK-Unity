using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlockadeLabsSDK;
using UnityEditor;
using UnityEngine;

public class BlockadeImaginarium : MonoBehaviour
{
    [Tooltip("API Key from Blockade Labs")]
    [SerializeField]
    public string apiKey;
    
    [Tooltip("Specifies if in-game GUI should be displayed")]
    [SerializeField]
    public bool enableGUI = false;
    
    [Tooltip("Specifies if in-game Skybox GUI should be displayed")]
    [SerializeField]
    public bool enableSkyboxGUI = false;

    [Tooltip("Specifies if the result should automatically be assigned as the sprite of the current game objects sprite renderer")]
    [SerializeField]
    public bool assignToSpriteRenderer = true;
    
    [Tooltip("Specifies if the result should automatically be assigned as the texture of the current game objects renderer material")]
    [SerializeField]
    public bool assignToMaterial = false;

    [Tooltip("The result image")]
    [SerializeField]
    public Texture2D resultImage;
    
    public Texture2D previewImage { get; set; }
    public List<GeneratorField> generatorFields = new List<GeneratorField>();
    public List<SkyboxStyleField> skyboxStyleFields = new List<SkyboxStyleField>();
    public List<Generator> generators = new List<Generator>();
    public List<SkyboxStyle> skyboxStyles = new List<SkyboxStyle>();
    public string[] generatorOptions;
    public string[] skyboxStyleOptions;
    public int generatorOptionsIndex = 0;
    public int skyboxStyleOptionsIndex = 0;
    public int lastGeneratorOptionsIndex = 0;
    public int lastSkyboxStyleOptionsIndex = 0;
    public int imagineId = 0;
    private int progressId;
    GUIStyle guiStyle;
    
    [HideInInspector]
    private float percentageCompleted = -1;
    private bool isCancelled;

    public void OnGUI()
    {
        if (enableGUI)
        {
            DrawGUILayout();
        } 
        else if (enableSkyboxGUI)
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

    private void DrawGUILayout()
    {
        DefineStyles();
        
        GUILayout.BeginArea(new Rect(Screen.width - (Screen.width / 3), 0, 300, Screen.height), guiStyle);
        
        if (GUILayout.Button("Get Generators"))
        {
            _ = GetGeneratorsWithFields();
        }

        // Iterate over generator fields and render them
        if (generatorFields.Count > 0)
        {
            RenderInGameFields();
        }
            
        GUILayout.EndArea(); 
    }
    
    private void RenderSkyboxInGameFields()
    {
        GUILayout.BeginVertical("Box");
        skyboxStyleOptionsIndex = GUILayout.SelectionGrid(skyboxStyleOptionsIndex, skyboxStyleOptions, 1);
        GUILayout.EndVertical();
            
        if (skyboxStyleOptionsIndex != lastSkyboxStyleOptionsIndex) {
            GetSkyboxStyleFields(skyboxStyleOptionsIndex);
            lastSkyboxStyleOptionsIndex = skyboxStyleOptionsIndex;
        }
            
        foreach (var field in skyboxStyleFields)
        {
            // Begin horizontal layout
            GUILayout.BeginHorizontal();
            
            // Create label for field
            GUILayout.Label(field.name + "*");

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

    private void RenderInGameFields()
    {
        GUILayout.BeginVertical("Box");
        generatorOptionsIndex = GUILayout.SelectionGrid(generatorOptionsIndex, generatorOptions, 1);
        GUILayout.EndVertical();
            
        if (generatorOptionsIndex != lastGeneratorOptionsIndex) {
            GetGeneratorFields(generatorOptionsIndex);
            lastGeneratorOptionsIndex = generatorOptionsIndex;
        }
            
        foreach (var field in generatorFields)
        {
            // Begin horizontal layout
            GUILayout.BeginHorizontal();
            
            var required = field.required ? "*" : "";
            // Create label for field
            GUILayout.Label(field.key + required);

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
                _ = InitializeGeneration(generatorFields, generators[generatorOptionsIndex].generator, true);
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
                "prompt",
                ""
            )
        );
        skyboxStyleFields.Add(promptField);
    }
    
    public async Task GetGeneratorsWithFields()
    {
        generators = await ApiRequests.GetGenerators(apiKey);
        generatorOptions = generators.Select(s => s.generator).ToArray();

        GetGeneratorFields(generatorOptionsIndex);
    }
    
    public void GetGeneratorFields(int index)
    {
        generatorFields = new List<GeneratorField>();
        
        foreach (KeyValuePair<string, Param> fieldData in generators[index].@params)
        {
            var field = new GeneratorField(fieldData);
            generatorFields.Add(field);
        }
    }
    
    public async Task InitializeSkyboxGeneration(List<SkyboxStyleField> skyboxStyleFields, int id, bool runtime = false)
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
        progressId = Progress.Start("Generating Skybox Assets");

        var createSkyboxId = await ApiRequests.CreateSkybox(skyboxStyleFields, id, apiKey);

        if (createSkyboxId != 0)
        {
            imagineId = createSkyboxId;
            percentageCompleted = 33;
            CalculateProgress();

            var pusherManager = false;
            
            #if PUSHER_PRESENT
                        
            pusherManager = FindObjectOfType<PusherManager>();
                        
            #endif

            if (
                !pusherManager || 
                (pusherManager && !runtime)
            )
            {
                _ = GetAssets();
            }
        }
    }

    public async Task InitializeGeneration(List<GeneratorField> generatorFields, string generator, bool runtime = false)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Debug.Log("You need to provide an Api Key in api options.");
            return;
        }
        
        isCancelled = false;
        await CreateImagine(generatorFields, generator, runtime);
    }

    async Task CreateImagine(List<GeneratorField> generatorFields, string generator, bool runtime = false)
    {
        percentageCompleted = 1;
        progressId = Progress.Start("Generating Assets");

        var createImagineId = await ApiRequests.CreateImagine(generatorFields, generator, apiKey);

        if (createImagineId != 0)
        {
            imagineId = createImagineId;
            percentageCompleted = 33;
            CalculateProgress();

            var pusherManager = false;
            
            #if PUSHER_PRESENT
            
            pusherManager = FindObjectOfType<PusherManager>();
            
            #endif

            if (
                !pusherManager || 
                (pusherManager && !runtime)
            )
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
            EditorUtility.SetDirty(this);
            await Task.Delay(1000);
            
            if (isCancelled)
            {
                break;
            }

            count++;
            
            var getImagineResult = await ApiRequests.GetImagine(imagineId, apiKey);

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
            DestroyImmediate(previewImage);
            imagineId = 0;
            return;
        }

        if (!string.IsNullOrWhiteSpace(textureUrl))
        {
            var image = await ApiRequests.GetImagineImage(textureUrl);

            var texture = new Texture2D(512, 512, TextureFormat.RGB24, false);
            texture.LoadImage(image);
            resultImage = texture;

            var previewTexture = new Texture2D(128, 128, TextureFormat.RGB24, false);
            previewTexture.LoadImage(image);

            percentageCompleted = 80;
            CalculateProgress();
            
            if (previewImage != null)
            {
                DestroyImmediate(previewImage);
                previewImage = null;
            }
            
            previewImage = previewTexture;
            
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            
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
            
            if (assignToSpriteRenderer)
            {
                var spriteRenderer = GetComponent<SpriteRenderer>();
                
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = sprite;
                } 
            }
            
            percentageCompleted = 90;
            CalculateProgress();
            SaveAssets(texture, sprite, prompt);
        }

        percentageCompleted = 100;
        CalculateProgress();
        Progress.Remove(progressId);
    }

    private void SaveAssets(Texture2D texture, Sprite sprite, string prompt)
    {
        if (AssetDatabase.Contains(texture) && AssetDatabase.Contains(sprite))
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
        var spriteName = ValidateFilename(prompt) + "_sprite";
        
        var counter = 0;
        
        while (true)
        {
            var modifiedTextureName = counter == 0 ? textureName : textureName + "_" + counter;
            var modifiedSpriteName = counter == 0 ? spriteName : spriteName + "_" + counter;

            var textureAssets = AssetDatabase.FindAssets(modifiedTextureName, new[] { "Assets/Blockade Labs SDK Assets" });
            
            if (textureAssets.Length > 0)
            {
                counter++;
                continue;
            }

            AssetDatabase.CreateAsset(texture, "Assets/Blockade Labs SDK Assets/" + modifiedTextureName + ".asset");
            AssetDatabase.CreateAsset(sprite, "Assets/Blockade Labs SDK Assets/" + modifiedSpriteName + ".asset");
            break;
        }
        
        imagineId = 0;
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
        Progress.Report(progressId, percentageCompleted / 100f);
    }

    public float PercentageCompleted() => percentageCompleted;

    public void Cancel()
    {
        isCancelled = true;
        percentageCompleted = -1;
        Progress.Remove(progressId);
    }
}