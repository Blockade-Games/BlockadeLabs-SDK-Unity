using System;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    [CustomEditor(typeof(BlockadeLabsSkyboxGenerator))]
    public class BlockadeLabsSkyboxGeneratorEditor : UnityEditor.Editor
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

            if (_apiKey.stringValue != null)
            {
                InitializeAsync((BlockadeLabsSkyboxGenerator)target, false);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var generator = (BlockadeLabsSkyboxGenerator)target;
            var generating = generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Generating;

            if (!BlockadeLabsSkyboxGenerator.BlockadeLabsClient.HasValidAuthentication)
            {
                DrawApiKey();
            }

            BlockadeGUI.DisableGroup(generating || !BlockadeLabsSkyboxGenerator.BlockadeLabsClient.HasValidAuthentication, () =>
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_modelVersion);

                if (EditorGUI.EndChangeCheck() && generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Ready)
                {
                    generator.ModelVersion = (SkyboxModel)_modelVersion.intValue;
                    _remix.boolValue = false;
                }

                EditorGUILayout.PropertyField(_skyboxMesh);
                EditorGUILayout.PropertyField(_skyboxMaterial);
                EditorGUILayout.PropertyField(_depthMaterial);
                EditorGUILayout.PropertyField(_cubemapComputeShader);
#if UNITY_HDRP
                EditorGUILayout.PropertyField(_HDRPVolume);
                EditorGUILayout.PropertyField(_HDRPVolumeProfile);
#endif

                if (!string.IsNullOrWhiteSpace(generator.LastError))
                {
                    EditorGUILayout.HelpBox(generator.LastError, MessageType.Error);
                }

                if (generator.CurrentState == BlockadeLabsSkyboxGenerator.State.NeedApiKey || generator.StyleFamily?.Count == 0)
                {
                    return;
                }

                DrawSkyboxFields(generator);
                EditorGUILayout.Space(12);

                if (generating)
                {
                    DrawProgress(generator);
                }
                else
                {
                    if (GUILayout.Button("Generate Skybox"))
                    {
                        generator.GenerateSkyboxAsync();
                    }
                }
            });

            if (serializedObject.ApplyModifiedProperties())
            {
                generator.EditorPropertyChanged();
            }
        }

        private void DrawApiKey()
        {
            var apiKey = _apiKey.stringValue;

            if (!string.IsNullOrWhiteSpace(apiKey) && _configuration.objectReferenceValue == null)
            {
                var configuration = BlockadeLabsConfigurationInspector.GetOrCreateInstance();
                configuration.ApiKey = apiKey;
                BlockadeLabsSkyboxGenerator.BlockadeLabsClient = new BlockadeLabsClient(configuration);
                _configuration.objectReferenceValue = configuration;
                _apiKey.stringValue = string.Empty;
            }
            else if (_configuration.objectReferenceValue == null)
            {
                BlockadeLabsConfigurationInspector.GetOrCreateInstance();
            }

            CreateCachedEditor(_configuration.objectReferenceValue, typeof(BlockadeLabsConfigurationInspector), ref _configurationEditor);
            _configurationEditor.OnInspectorGUI();
        }

        private void DrawSkyboxFields(BlockadeLabsSkyboxGenerator generator)
        {
            EditorGUILayout.PropertyField(_selectedStyle);

            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.PropertyField(_prompt, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
            EditorStyles.textField.wordWrap = false;
            EditorGUILayout.PropertyField(_negativeText);
            EditorGUILayout.PropertyField(_remix);

            BlockadeGUI.DisableGroup(!_remix.boolValue, () =>
            {
                generator.RemixImage = (Texture2D)EditorGUILayout.ObjectField(new GUIContent(_remixImage.displayName, _remixImage.tooltip), _remixImage.objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();
            });

            EditorGUILayout.PropertyField(_seed);
            EditorGUILayout.PropertyField(_enhancePrompt);
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

        private async void InitializeAsync(BlockadeLabsSkyboxGenerator generator, bool sendAttribution)
        {
            if (!generator.CheckApiKeyValid())
            {
                return;
            }

            if (generator.CurrentState != BlockadeLabsSkyboxGenerator.State.NeedApiKey)
            {
                return;
            }

            try
            {
                await generator.LoadAsync();

                if (sendAttribution)
                {
                    VSAttribution.SendAttributionEvent("Initialization", "BlockadeLabs", _apiKey.stringValue);
                }

                Repaint();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to initialize BlockadeLabsSkybox: " + e.Message);
            }
        }
    }
}