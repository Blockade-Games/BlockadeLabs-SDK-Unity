using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    [CustomEditor(typeof(BlockadeLabsSkyboxGenerator))]
    internal class BlockadeLabsSkyboxGeneratorEditor : UnityEditor.Editor
    {
        private SerializedProperty _configuration;
        private SerializedProperty _apiKey;
        private SerializedProperty _modelVersion;
        private SerializedProperty _skyboxMesh;
        private SerializedProperty _skyboxMaterial;
        private SerializedProperty _depthMaterial;
        private SerializedProperty _cubemapComputeShader;
#if UNITY_HDRP
        private SerializedProperty _HDRPVolume;
        private SerializedProperty _HDRPVolumeProfile;
#endif
        private SerializedProperty _selectStyleFamily;
        private SerializedProperty _selectedStyle;
        private SerializedProperty _prompt;
        private SerializedProperty _negativeText;
        private SerializedProperty _remix;
        private SerializedProperty _remixImage;
        private SerializedProperty _seed;
        private SerializedProperty _enhancePrompt;

        private UnityEditor.Editor _configurationEditor;
        private static BlockadeLabsSkyboxGeneratorEditor _generatorEditor;

        private static bool HasValidAuth
        {
            get
            {
                try
                {
                    return BlockadeLabsSkyboxGenerator.BlockadeLabsClient.HasValidAuthentication;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private void OnEnable()
        {
            _configuration = serializedObject.FindProperty(nameof(_configuration));
            _apiKey = serializedObject.FindProperty(nameof(_apiKey));
            _modelVersion = serializedObject.FindProperty(nameof(_modelVersion));
            _skyboxMesh = serializedObject.FindProperty(nameof(_skyboxMesh));
            _skyboxMaterial = serializedObject.FindProperty(nameof(_skyboxMaterial));
            _depthMaterial = serializedObject.FindProperty(nameof(_depthMaterial));
            _cubemapComputeShader = serializedObject.FindProperty(nameof(_cubemapComputeShader));
#if UNITY_HDRP
            _HDRPVolume = serializedObject.FindProperty(nameof(_HDRPVolume));
            _HDRPVolumeProfile = serializedObject.FindProperty(nameof(_HDRPVolumeProfile));
#endif
            _selectStyleFamily = serializedObject.FindProperty(nameof(_selectStyleFamily));
            _selectedStyle = serializedObject.FindProperty(nameof(_selectedStyle));
            _prompt = serializedObject.FindProperty(nameof(_prompt));
            _negativeText = serializedObject.FindProperty(nameof(_negativeText));
            _remix = serializedObject.FindProperty(nameof(_remix));
            _remixImage = serializedObject.FindProperty(nameof(_remixImage));
            _seed = serializedObject.FindProperty(nameof(_seed));
            _enhancePrompt = serializedObject.FindProperty(nameof(_enhancePrompt));

            _generatorEditor = this;
            EditorApplication.delayCall += Initialize;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var generator = (BlockadeLabsSkyboxGenerator)target;

            DrawApiKey(generator);

            var hasAuth = HasValidAuth;

            if (hasAuth)
            {
                hasAuth = generator.CheckApiKeyValid();
            }

            var isReady = generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready;

            BlockadeGUI.DisableGroup(!isReady || !hasAuth, () =>
            {
                DrawSkyboxAssetFields();
                DrawSkyboxFields(generator);

                if (!string.IsNullOrWhiteSpace(generator.LastError))
                {
                    EditorGUILayout.HelpBox(generator.LastError, MessageType.Error);
                }
            });

            if (generator.CurrentState != BlockadeLabsSkyboxGenerator.State.Generating)
            {
                if (GUILayout.Button("Generate Skybox"))
                {
                    generator.GenerateSkybox();
                }
            }
            else
            {
                DrawProgress(generator);
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                generator.EditorPropertyChanged();
            }
        }

        private void DrawApiKey(BlockadeLabsSkyboxGenerator generator)
        {
            EditorGUILayout.Space();
            if (_configuration.objectReferenceValue != null)
            {
                BlockadeLabsSkyboxGenerator.Configuration = (BlockadeLabsConfiguration)_configuration.objectReferenceValue;
            }
            else
            {
                BlockadeLabsSkyboxGenerator.Configuration = BlockadeLabsConfigurationInspector.GetOrCreateInstance();
                _configuration.objectReferenceValue = BlockadeLabsSkyboxGenerator.Configuration;

                if (!string.IsNullOrWhiteSpace(_apiKey.stringValue))
                {
                    BlockadeLabsSkyboxGenerator.Configuration.ApiKey = _apiKey.stringValue;
                    _apiKey.stringValue = string.Empty;
                }
            }

            CreateCachedEditor(_configuration.objectReferenceValue, typeof(BlockadeLabsConfigurationInspector), ref _configurationEditor);
            BlockadeLabsConfigurationInspector.ShowApiHelpBox = generator.CurrentState == BlockadeLabsSkyboxGenerator.State.NeedApiKey;
            _configurationEditor?.OnInspectorGUI();
            EditorGUILayout.Space();
        }

        private void DrawSkyboxAssetFields()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Skybox Assets", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_skyboxMesh);
            EditorGUILayout.PropertyField(_skyboxMaterial);
            EditorGUILayout.PropertyField(_depthMaterial);
            EditorGUILayout.PropertyField(_cubemapComputeShader);
#if UNITY_HDRP
            EditorGUILayout.PropertyField(_HDRPVolume);
            EditorGUILayout.PropertyField(_HDRPVolumeProfile);
#endif
        }

        private void DrawSkyboxFields(BlockadeLabsSkyboxGenerator generator)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Skybox Generation Options", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_modelVersion);

            if (EditorGUI.EndChangeCheck())
            {
                generator.ModelVersion = (SkyboxModel)_modelVersion.intValue;
                generator.SelectedStyle = null;
                _remix.boolValue = false;
                EditorUtility.SetDirty(generator);
            }

            if (generator.ModelVersion != _model)
            {
                _model = generator.ModelVersion;
                _options = null;
            }

            RenderStyleField(generator);
            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.PropertyField(_prompt, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
            EditorStyles.textField.wordWrap = false;
            EditorGUILayout.PropertyField(_enhancePrompt);
            EditorGUILayout.PropertyField(_negativeText);
            EditorGUILayout.PropertyField(_remix);

            BlockadeGUI.DisableGroup(!_remix.boolValue, () =>
            {
                generator.RemixImage = (Texture2D)EditorGUILayout.ObjectField(new GUIContent(_remixImage.displayName, _remixImage.tooltip), _remixImage.objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();
            });

            EditorGUILayout.PropertyField(_seed);
            EditorGUILayout.Space(12);
        }

        private void RenderStyleField(BlockadeLabsSkyboxGenerator generator)
        {
            var styleId = _selectedStyle.FindPropertyRelative("_id");
            var styleName = _selectedStyle.FindPropertyRelative("_name");

            if (_options == null || _options.Length == 0)
            {
                FetchStyles(generator);

                if (string.IsNullOrWhiteSpace(styleName.stringValue))
                {
                    EditorGUILayout.HelpBox("Fetching skybox styles...", MessageType.Info);
                    return;
                }

                EditorGUILayout.LabelField(new GUIContent(styleName.stringValue, styleId.intValue.ToString()));
                return;
            }

            var index = -1;
            var currentOption = generator.SelectedStyle ?? GetSelection(styleId.intValue);

            if (currentOption != null)
            {
                for (int i = 0; i < _options.Length; i++)
                {
                    if (_options[i].tooltip == styleId.intValue.ToString())
                    {
                        index = i;
                        break;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            index = EditorGUILayout.Popup(new GUIContent(_selectedStyle.displayName, _selectedStyle.tooltip), index, _options);

            if (EditorGUI.EndChangeCheck())
            {
                currentOption = GetSelection(int.Parse(_options[index].tooltip));

                if (currentOption != null)
                {
                    generator.SelectedStyle = currentOption;
                    EditorUtility.SetDirty(generator);
                }
                else
                {
                    Debug.LogError("Failed to make a selection!");
                }
            }

            SkyboxStyle GetSelection(int selection)
            {
                foreach (var style in _styles)
                {
                    if (style.FamilyStyles != null)
                    {
                        foreach (var familyStyle in style.FamilyStyles)
                        {
                            if (familyStyle.Id == selection)
                            {
                                return familyStyle;
                            }
                        }
                    }
                    else
                    {
                        if (style.Id == selection)
                        {
                            return style;
                        }
                    }
                }

                return null;
            }
        }

        private SkyboxModel _model = SkyboxModel.Model3;
        private List<SkyboxStyle> _styles = new List<SkyboxStyle>();
        private GUIContent[] _options = Array.Empty<GUIContent>();

        public void FetchStyles(BlockadeLabsSkyboxGenerator generator)
        {
            if (generator.AllModelStyleFamilies == null)
            {
                return;
            }

            try
            {
                _styles.Clear();
                var styleOptions = new List<GUIContent>();

                foreach (var style in generator.AllModelStyleFamilies.OrderBy(style => style.SortOrder))
                {
                    var styleName = style.Name.Replace("/", " ");

                    if (style.FamilyStyles != null && style.FamilyStyles.Count > 0)
                    {
                        foreach (var familyStyle in style.FamilyStyles.OrderBy(familyStyle => familyStyle.SortOrder))
                        {
                            if (familyStyle.Model == generator.ModelVersion)
                            {
                                _styles.Add(familyStyle);
                                var name = familyStyle.Name.Replace("/", " ");
                                styleOptions.Add(new GUIContent($"{styleName}/{name}", familyStyle.Id.ToString()));
                            }
                        }
                    }
                    else
                    {
                        if (style.Model == generator.ModelVersion)
                        {
                            _styles.Add(style);
                            styleOptions.Add(new GUIContent($"{styleName}", style.Id.ToString()));
                        }
                    }
                }

                _options = styleOptions.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void DrawProgress(BlockadeLabsSkyboxGenerator generator)
        {
            BlockadeGUI.Horizontal(() =>
            {
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), generator.PercentageCompleted / 100f, "Generating Skybox");
                if (GUILayout.Button("Cancel", GUILayout.Width(80)))
                {
                    generator.Cancel();
                }
            });
        }

        internal static async void Initialize()
            => await InitializeAsync(false);

        internal static async Task InitializeAsync(bool sendAttribution)
        {
            if (_generatorEditor == null)
            {
                return;
            }

            var generator = (BlockadeLabsSkyboxGenerator)_generatorEditor.target;

            if (generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Generating)
            {
                return;
            }

            try
            {
                await generator.LoadAsync();

                if (sendAttribution &&
                    BlockadeLabsSkyboxGenerator.Configuration != null &&
                    !string.IsNullOrWhiteSpace(BlockadeLabsSkyboxGenerator.Configuration.ApiKey))
                {
                    VSAttribution.SendAttributionEvent("Initialization", "BlockadeLabs", BlockadeLabsSkyboxGenerator.Configuration.ApiKey);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize BlockadeLabsSkybox: {e.Message}");
            }
            finally
            {
                _generatorEditor.Repaint();
            }
        }
    }
}