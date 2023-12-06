using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    [CustomEditor(typeof(BlockadeLabsSkybox))]
    public class BlockadeLabsSkyboxEditor : UnityEditor.Editor
    {
        private SerializedProperty _apiKey;
        private SerializedProperty _assignToMaterial;
        private SerializedProperty _selectedStyleFamilyIndex;
        private SerializedProperty _selectedStyleIndex;
        private SerializedProperty _prompt;
        private SerializedProperty _negativeText;
        private SerializedProperty _remix;
        private SerializedProperty _seed;
        private SerializedProperty _enhancePrompt;

        void OnEnable()
        {
            _apiKey = serializedObject.FindProperty("_apiKey");
            _assignToMaterial = serializedObject.FindProperty("_assignToMaterial");
            _selectedStyleFamilyIndex = serializedObject.FindProperty("_selectedStyleFamilyIndex");
            _selectedStyleIndex = serializedObject.FindProperty("_selectedStyleIndex");
            _prompt = serializedObject.FindProperty("_prompt");
            _negativeText = serializedObject.FindProperty("_negativeText");
            _remix = serializedObject.FindProperty("_remix");
            _seed = serializedObject.FindProperty("_seed");
            _enhancePrompt = serializedObject.FindProperty("_enhancePrompt");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var blockadeLabsSkybox = (BlockadeLabsSkybox)target;


            bool generating = blockadeLabsSkybox.CurrentState == BlockadeLabsSkybox.State.Generating;
            BlockadeGUI.DisableGroup(generating, () =>
            {
                DrawApiKey(blockadeLabsSkybox);

                if (!string.IsNullOrWhiteSpace(blockadeLabsSkybox.LastError))
                {
                    EditorGUILayout.HelpBox(blockadeLabsSkybox.LastError, MessageType.Error);
                }

                if (blockadeLabsSkybox.CurrentState == BlockadeLabsSkybox.State.NeedApiKey)
                {
                    return;
                }

                DrawSkyboxFields(blockadeLabsSkybox);

                EditorGUILayout.PropertyField(_assignToMaterial);

                if (generating)
                {
                    DrawProgress(blockadeLabsSkybox);
                }
                else
                {
                    if (GUILayout.Button("Generate Skybox"))
                    {
                        blockadeLabsSkybox.GenerateSkyboxAsync();
                    }
                }
            });

            if (GUILayout.Button("Move Scene Camera to Skybox"))
            {
                SceneView.lastActiveSceneView.AlignViewToObject(blockadeLabsSkybox.transform);
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                blockadeLabsSkybox.EditorPropertyChanged();
            }
        }

        private void DrawApiKey(BlockadeLabsSkybox blockadeLabsSkybox)
        {
            BlockadeGUI.Horizontal(() =>
            {
                EditorGUILayout.PropertyField(_apiKey, new GUIContent("API key"));
                if (GUILayout.Button("Apply", GUILayout.Width(80)))
                {
                    InitializeAsync(blockadeLabsSkybox);
                }
            });
        }

        private void DrawSkyboxFields(BlockadeLabsSkybox blockadeLabsSkybox)
        {
            BlockadeGUI.Horizontal(() =>
            {
                EditorGUILayout.LabelField("Style", GUILayout.Width(EditorGUIUtility.labelWidth));
                var familyOptions = blockadeLabsSkybox.StyleFamilies.Select(f => f.name).ToArray();
                var selectedFamilyIndex = EditorGUILayout.Popup(_selectedStyleFamilyIndex.intValue, familyOptions);
                if (selectedFamilyIndex != _selectedStyleFamilyIndex.intValue)
                {
                    blockadeLabsSkybox.SelectedStyleFamily = blockadeLabsSkybox.StyleFamilies[selectedFamilyIndex];
                }

                var styleOptions = blockadeLabsSkybox.SelectedStyleFamily.items.Select(s => s.name).ToArray();
                var selectedStyleIndex = EditorGUILayout.Popup(_selectedStyleIndex.intValue, styleOptions);
                if (selectedStyleIndex != _selectedStyleIndex.intValue)
                {
                    blockadeLabsSkybox.SelectedStyle = blockadeLabsSkybox.SelectedStyleFamily.items[selectedStyleIndex];
                }
            });

            EditorGUILayout.PropertyField(_prompt, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
            EditorGUILayout.PropertyField(_negativeText);
            EditorGUILayout.PropertyField(_remix);
            EditorGUILayout.PropertyField(_seed);
            EditorGUILayout.PropertyField(_enhancePrompt);
        }

        private void DrawProgress(BlockadeLabsSkybox blockadeLabsSkybox)
        {
            BlockadeGUI.Horizontal(() =>
            {
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), blockadeLabsSkybox.PercentageCompleted / 100f, "Generating Skybox");
                if (GUILayout.Button("Cancel", GUILayout.Width(80)))
                {
                    blockadeLabsSkybox.Cancel();
                }
            });
        }

        private async void InitializeAsync(BlockadeLabsSkybox blockadeLabsSkybox)
        {
            if (!blockadeLabsSkybox.CheckApiKeyValid())
            {
                return;
            }

            bool wasInitialzied = blockadeLabsSkybox.CurrentState != BlockadeLabsSkybox.State.NeedApiKey;

            try
            {
                await blockadeLabsSkybox.LoadAsync();

                // send attribution event to verified solution
                if (!wasInitialzied)
                {
                    VSAttribution.SendAttributionEvent("Initialization", "BlockadeLabs", _apiKey.stringValue);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to initialize BlockadeLabsSkybox: " + e.Message);
            }
        }
    }
}