using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK
{
#if UNITY_EDITOR
    [CustomEditor(typeof(BlockadeLabsSkybox))]
    public class BlockadeLabsSkyboxEditor : Editor
    {
        private SerializedProperty apiKey;
        private SerializedProperty assignToMaterial;
        private SerializedProperty skyboxStyleFields;
        private SerializedProperty skyboxStyles;
        private SerializedProperty skyboxStyleOptions;
        private SerializedProperty skyboxStyleOptionsIndex;
        private bool showApi = true;
        private bool showBasic = true;
        private bool showSkybox = true;

        void OnEnable()
        {
            apiKey = serializedObject.FindProperty("apiKey");
            assignToMaterial = serializedObject.FindProperty("assignToMaterial");
            skyboxStyleFields = serializedObject.FindProperty("skyboxStyleFields");
            skyboxStyles = serializedObject.FindProperty("skyboxStyles");
            skyboxStyleOptions = serializedObject.FindProperty("skyboxStyleOptions");
            skyboxStyleOptionsIndex = serializedObject.FindProperty("skyboxStyleOptionsIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var blockadeLabsSkybox = (BlockadeLabsSkybox)target;

            showApi = EditorGUILayout.Foldout(showApi, "API");

            if (showApi)
            {
                EditorGUILayout.PropertyField(apiKey, new GUIContent("API key"));
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
                                _ = blockadeLabsSkybox.CreateSkybox(
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

                if (field.type == "boolean")
                {
                    var fieldBoolValue = field.value == "true";
                    var toggleValue = EditorGUILayout.Toggle(field.name, fieldBoolValue, GUILayout.Width(EditorGUIUtility.currentViewWidth));
                
                    field.value = toggleValue ? "true" : "false";
                }
                else
                {
                    // Create label for field
                    EditorGUILayout.LabelField(field.name, GUILayout.Width(EditorGUIUtility.labelWidth));
                
                    // Create text field for field value
                    if (field.type == "textarea")
                    {
                        field.value = EditorGUILayout.TextArea(field.value, GUILayout.Height(100), GUILayout.Width(EditorGUIUtility.currentViewWidth));
                    }
                    else
                    {
                        field.value = EditorGUILayout.TextField(field.value, GUILayout.Width(EditorGUIUtility.currentViewWidth)); 
                    }
                }
                
                // End horizontal layout
                EditorGUILayout.EndHorizontal();
            }
        }
    }
#endif
}