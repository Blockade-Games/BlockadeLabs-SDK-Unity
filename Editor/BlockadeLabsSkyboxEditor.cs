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

        void OnEnable()
        {
            _apiKey = serializedObject.FindProperty("_apiKey");
            _assignToMaterial = serializedObject.FindProperty("_assignToMaterial");
            _selectedStyleFamilyIndex = serializedObject.FindProperty("_selectedStyleFamilyIndex");
            _selectedStyleIndex = serializedObject.FindProperty("_selectedStyleIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var blockadeLabsSkybox = (BlockadeLabsSkybox)target;

            DrawApiKey(blockadeLabsSkybox);

            if (blockadeLabsSkybox.Initialized)
            {
                var percentageCompleted = blockadeLabsSkybox.PercentageCompleted();
                bool generating = percentageCompleted >= 0 && percentageCompleted < 100;

                BlockadeGUI.DisableGroup(generating, () =>
                {
                    DrawSkyboxFields(blockadeLabsSkybox);

                    EditorGUILayout.PropertyField(_assignToMaterial);

                });

                if (generating)
                {
                    DrawProgress(blockadeLabsSkybox, percentageCompleted);
                }
                else
                {
                    if (GUILayout.Button("Generate Skybox"))
                    {
                        blockadeLabsSkybox.GenerateSkyboxAsync();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
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

            foreach (var field in blockadeLabsSkybox.SkyboxStyleFields)
            {
                BlockadeGUI.Horizontal(() =>
                {
                    if (field.type == "boolean")
                    {
                        var fieldBoolValue = field.value == "true";
                        var toggleValue = EditorGUILayout.Toggle(field.name, fieldBoolValue, GUILayout.Width(EditorGUIUtility.currentViewWidth));
                        field.value = toggleValue ? "true" : "false";
                    }
                    else
                    {
                        EditorGUILayout.LabelField(field.name, GUILayout.Width(EditorGUIUtility.labelWidth));
                        if (field.type == "textarea")
                        {
                            field.value = EditorGUILayout.TextArea(field.value, GUILayout.Height(60));
                        }
                        else
                        {
                            field.value = EditorGUILayout.TextField(field.value);
                        }
                    }
                });
            }
        }

        private void DrawProgress(BlockadeLabsSkybox blockadeLabsSkybox, float percentageCompleted)
        {
            BlockadeGUI.Horizontal(() =>
            {
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), percentageCompleted / 100f, "Generating Skybox");
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

            bool wasInitialzied = blockadeLabsSkybox.Initialized;

            try
            {
                await blockadeLabsSkybox.LoadOptionsAsync();

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