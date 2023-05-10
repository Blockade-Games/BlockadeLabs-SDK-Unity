using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK
{
#if UNITY_EDITOR
    [CustomEditor(typeof(BlockadeLabsSkybox))]
    public class BlockadeLabsSkyboxEditor : Editor
    {
        private SerializedProperty assignToMaterial;
        private SerializedProperty apiKey;
        private SerializedProperty skyboxStyleFields;
        private SerializedProperty skyboxStyles;
        private SerializedProperty skyboxStyleOptions;
        private SerializedProperty skyboxStyleOptionsIndex;
        private bool showApi = true;
        private bool showBasic = true;
        private bool showSkybox = true;

        void OnEnable()
        {
            assignToMaterial = serializedObject.FindProperty("assignToMaterial");
            apiKey = serializedObject.FindProperty("apiKey");
            skyboxStyleFields = serializedObject.FindProperty("skyboxStyleFields");
            skyboxStyles = serializedObject.FindProperty("skyboxStyles");
            skyboxStyleOptions = serializedObject.FindProperty("skyboxStyleOptions");
            skyboxStyleOptionsIndex = serializedObject.FindProperty("skyboxStyleOptionsIndex");
        }

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);

            serializedObject.Update();

            var blockadeLabsSkybox = (BlockadeLabsSkybox)target;

            showApi = EditorGUILayout.Foldout(showApi, "API");

            if (showApi)
            {
                EditorGUILayout.PropertyField(apiKey);
                GUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField("Initialize/Refresh");

                if (skyboxStyleFields.arraySize > 0)
                {
                    if (GUILayout.Button("Refresh"))
                    {
                        _ = blockadeLabsSkybox.GetSkyboxStyleOptions();
                    }
                }
                else
                {
                    if (GUILayout.Button("Initialize"))
                    {
                        _ = blockadeLabsSkybox.GetSkyboxStyleOptions();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (skyboxStyleFields.arraySize > 0)
            {
                showBasic = EditorGUILayout.Foldout(showBasic, "Basic Settings");

                if (showBasic)
                {
                    EditorGUILayout.PropertyField(assignToMaterial);
                }

                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    showSkybox = EditorGUILayout.Foldout(showSkybox, "Skybox Generator");

                    if (showSkybox)
                    {
                        // Iterate over skybox style fields and render them in the GUI
                        if (blockadeLabsSkybox.skyboxStyleFields.Count > 0)
                        {
                            RenderSkyboxEditorFields(blockadeLabsSkybox);
                        }
                    
                        if (blockadeLabsSkybox.PercentageCompleted() >= 0 && blockadeLabsSkybox.PercentageCompleted() < 100)
                        {
                            if (GUILayout.Button("Cancel (" + blockadeLabsSkybox.PercentageCompleted() + "%)"))
                            {
                                blockadeLabsSkybox.Cancel();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Generate Skybox"))
                            {
                                _ = blockadeLabsSkybox.InitializeSkyboxGeneration(
                                    blockadeLabsSkybox.skyboxStyleFields,
                                    blockadeLabsSkybox.skyboxStyles[blockadeLabsSkybox.skyboxStyleOptionsIndex].id
                                );
                            }
                        }
                    }
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void RenderSkyboxEditorFields(BlockadeLabsSkybox blockadeLabsSkybox)
        {
            // Begin horizontal layout
            EditorGUILayout.BeginHorizontal();
            
            // Create label for the style field
            EditorGUILayout.LabelField("Style", GUILayout.Width(EditorGUIUtility.labelWidth));

            blockadeLabsSkybox.skyboxStyleOptionsIndex = EditorGUILayout.Popup(
                blockadeLabsSkybox.skyboxStyleOptionsIndex,
                blockadeLabsSkybox.skyboxStyleOptions,
                GUILayout.Width(EditorGUIUtility.currentViewWidth)
            );

            // End horizontal layout
            EditorGUILayout.EndHorizontal();

            foreach (var field in blockadeLabsSkybox.skyboxStyleFields)
            {
                // Begin horizontal layout
                EditorGUILayout.BeginHorizontal();
                
                // Create label for field
                EditorGUILayout.LabelField(field.name, GUILayout.Width(EditorGUIUtility.labelWidth));
            
                // Create text field for field value
                if (field.name == "Prompt")
                {
                    field.value = EditorGUILayout.TextArea(field.value, GUILayout.Height(100), GUILayout.Width(EditorGUIUtility.currentViewWidth));
                }
                else
                {
                    field.value = EditorGUILayout.TextField(field.value, GUILayout.Width(EditorGUIUtility.currentViewWidth)); 
                }

                // End horizontal layout
                EditorGUILayout.EndHorizontal();
            }
        }
    }
#endif
}