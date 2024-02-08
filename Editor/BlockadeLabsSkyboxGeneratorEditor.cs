using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    [CustomEditor(typeof(BlockadeLabsSkyboxGenerator))]
    public class BlockadeLabsSkyboxGeneratorEditor : UnityEditor.Editor
    {
        private SerializedProperty _apiKey;
        private SerializedProperty _skyboxMesh;
        private SerializedProperty _skyboxMaterial;
        private SerializedProperty _depthMaterial;
        private SerializedProperty _cubemapComputeShader;
#if UNITY_HDRP
        private SerializedProperty _HDRPVolume;
        private SerializedProperty _HDRPVolumeProfile;
#endif
        private SerializedProperty _selectedStyleFamilyIndex;
        private SerializedProperty _selectedStyleIndex;
        private SerializedProperty _prompt;
        private SerializedProperty _negativeText;
        private SerializedProperty _remix;
        private SerializedProperty _seed;
        private SerializedProperty _enhancePrompt;

        private void OnEnable()
        {
            _apiKey = serializedObject.FindProperty("_apiKey");
            _skyboxMesh = serializedObject.FindProperty("_skyboxMesh");
            _skyboxMaterial = serializedObject.FindProperty("_skyboxMaterial");
            _depthMaterial = serializedObject.FindProperty("_depthMaterial");
            _cubemapComputeShader = serializedObject.FindProperty("_cubemapComputeShader");
#if UNITY_HDRP
            _HDRPVolume = serializedObject.FindProperty("_HDRPVolume");
            _HDRPVolumeProfile = serializedObject.FindProperty("_HDRPVolumeProfile");
#endif
            _selectedStyleFamilyIndex = serializedObject.FindProperty("_selectedStyleFamilyIndex");
            _selectedStyleIndex = serializedObject.FindProperty("_selectedStyleIndex");
            _prompt = serializedObject.FindProperty("_prompt");
            _negativeText = serializedObject.FindProperty("_negativeText");
            _remix = serializedObject.FindProperty("_remix");
            _seed = serializedObject.FindProperty("_seed");
            _enhancePrompt = serializedObject.FindProperty("_enhancePrompt");

            if (_apiKey.stringValue != null)
            {
                InitializeAsync((BlockadeLabsSkyboxGenerator)target, false);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var generator = (BlockadeLabsSkyboxGenerator)target;

            bool generating = generator.CurrentState == BlockadeLabsSkyboxGenerator.State.Generating;
            BlockadeGUI.DisableGroup(generating, () =>
            {
                DrawApiKey(generator);

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

                if (generator.CurrentState == BlockadeLabsSkyboxGenerator.State.NeedApiKey || generator.StyleFamilies?.Count == 0)
                {
                    return;
                }

                DrawSkyboxFields(generator);

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

        private void DrawApiKey(BlockadeLabsSkyboxGenerator generator)
        {
            BlockadeGUI.Horizontal(() =>
            {
                EditorGUILayout.PropertyField(_apiKey, new GUIContent("API key"));
                if (GUILayout.Button("Apply", GUILayout.Width(80)))
                {
                    InitializeAsync(generator, true);
                }
            });
        }

        private void DrawSkyboxFields(BlockadeLabsSkyboxGenerator generator)
        {
            BlockadeGUI.Horizontal(() =>
            {
                EditorGUILayout.LabelField("Style", GUILayout.Width(EditorGUIUtility.labelWidth));
                var familyOptions = generator.StyleFamilies.Select(f => f.name).ToArray();
                var selectedFamilyIndex = EditorGUILayout.Popup(_selectedStyleFamilyIndex.intValue, familyOptions);
                if (selectedFamilyIndex != _selectedStyleFamilyIndex.intValue)
                {
                    generator.SelectedStyleFamily = generator.StyleFamilies[selectedFamilyIndex];
                }

                var styleOptions = generator.SelectedStyleFamily.items.Select(s => s.name).ToArray();
                var selectedStyleIndex = EditorGUILayout.Popup(_selectedStyleIndex.intValue, styleOptions);
                if (selectedStyleIndex != _selectedStyleIndex.intValue)
                {
                    generator.SelectedStyle = generator.SelectedStyleFamily.items[selectedStyleIndex];
                }
            });


            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.PropertyField(_prompt, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
            EditorStyles.textField.wordWrap = false;
            EditorGUILayout.PropertyField(_negativeText);
            EditorGUILayout.PropertyField(_remix);
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