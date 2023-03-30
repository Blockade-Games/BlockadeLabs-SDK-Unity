using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK
{
#if UNITY_EDITOR
    [CustomEditor(typeof(BlockadeImaginarium))]
    public class BlockadeImaginariumEditor : Editor
    {
        private SerializedProperty assignToMaterial;
        private SerializedProperty assignToSpriteRenderer;
        private SerializedProperty enableGUI;
        private SerializedProperty enableSkyboxGUI;
        private SerializedProperty resultImage;
        private SerializedProperty apiKey;
        private SerializedProperty generatorFields;
        private SerializedProperty skyboxStyleFields;
        private SerializedProperty generators;
        private SerializedProperty skyboxStyles;
        private SerializedProperty generatorOptions;
        private SerializedProperty skyboxStyleOptions;
        private SerializedProperty generatorOptionsIndex;
        private SerializedProperty skyboxStyleOptionsIndex;
        private bool showApi = true;
        private bool showBasic = true;
        private bool showImagine = true;
        private bool showSkybox = true;
        private bool showOutput = true;

        void OnEnable()
        {
            assignToMaterial = serializedObject.FindProperty("assignToMaterial");
            assignToSpriteRenderer = serializedObject.FindProperty("assignToSpriteRenderer");
            enableGUI = serializedObject.FindProperty("enableGUI");
            enableSkyboxGUI = serializedObject.FindProperty("enableSkyboxGUI");
            apiKey = serializedObject.FindProperty("apiKey");
            resultImage = serializedObject.FindProperty("resultImage");
            generatorFields = serializedObject.FindProperty("generatorFields");
            skyboxStyleFields = serializedObject.FindProperty("skyboxStyleFields");
            generators = serializedObject.FindProperty("generators");
            skyboxStyles = serializedObject.FindProperty("skyboxStyles");
            generatorOptions = serializedObject.FindProperty("generatorOptions");
            skyboxStyleOptions = serializedObject.FindProperty("skyboxStyleOptions");
            generatorOptionsIndex = serializedObject.FindProperty("generatorOptionsIndex");
            skyboxStyleOptionsIndex = serializedObject.FindProperty("skyboxStyleOptionsIndex");
        }

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);

            serializedObject.Update();

            var blockadeImaginarium = (BlockadeImaginarium)target;

            showApi = EditorGUILayout.Foldout(showApi, "Api");

            if (showApi)
            {
                EditorGUILayout.PropertyField(apiKey);
            }

            showBasic = EditorGUILayout.Foldout(showBasic, "Basic Settings");

            if (showBasic)
            {
                EditorGUILayout.PropertyField(enableGUI);
                EditorGUILayout.PropertyField(enableSkyboxGUI);
                EditorGUILayout.PropertyField(assignToSpriteRenderer);
                EditorGUILayout.PropertyField(assignToMaterial);
            }

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                showSkybox = EditorGUILayout.Foldout(showSkybox, "Skybox");

                if (showSkybox)
                {
                    if (GUILayout.Button("Get Styles"))
                    {
                        _ = blockadeImaginarium.GetSkyboxStyleOptions();
                    }

                    // Iterate over skybox style fields and render them in the GUI
                    if (blockadeImaginarium.skyboxStyleFields.Count > 0)
                    {
                        RenderSkyboxEditorFields(blockadeImaginarium);
                    }
                    
                    if (blockadeImaginarium.PercentageCompleted() >= 0 && blockadeImaginarium.PercentageCompleted() < 100)
                    {
                        if (GUILayout.Button("Cancel (" + blockadeImaginarium.PercentageCompleted() + "%)"))
                        {
                            blockadeImaginarium.Cancel();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Generate Skybox"))
                        {
                            _ = blockadeImaginarium.InitializeSkyboxGeneration(
                                blockadeImaginarium.skyboxStyleFields,
                                blockadeImaginarium.skyboxStyles[blockadeImaginarium.skyboxStyleOptionsIndex].id
                            );
                        }
                    }
                }
                
                showImagine = EditorGUILayout.Foldout(showImagine, "Imagine");

                if (showImagine)
                {
                    if (GUILayout.Button("Get Generators"))
                    {
                        _ = blockadeImaginarium.GetGeneratorsWithFields();
                    }

                    // Iterate over generator fields and render them in the GUI
                    if (blockadeImaginarium.generatorFields.Count > 0)
                    {
                        RenderEditorFields(blockadeImaginarium);
                    }
                    
                    if (blockadeImaginarium.PercentageCompleted() >= 0 && blockadeImaginarium.PercentageCompleted() < 100)
                    {
                        if (GUILayout.Button("Cancel (" + blockadeImaginarium.PercentageCompleted() + "%)"))
                        {
                            blockadeImaginarium.Cancel();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Generate"))
                        {
                            _ = blockadeImaginarium.InitializeGeneration(
                                blockadeImaginarium.generatorFields,
                                blockadeImaginarium.generators[blockadeImaginarium.generatorOptionsIndex].generator
                            );
                        }
                    }
                }

                showOutput = EditorGUILayout.Foldout(showOutput, "Output");

                if (showOutput)
                {
                    EditorGUILayout.PropertyField(resultImage);
                    if (blockadeImaginarium.previewImage != null) GUILayout.Box(blockadeImaginarium.previewImage);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void RenderEditorFields(BlockadeImaginarium blockadeImaginarium)
        {
            EditorGUI.BeginChangeCheck();

            blockadeImaginarium.generatorOptionsIndex = EditorGUILayout.Popup(
                blockadeImaginarium.generatorOptionsIndex,
                blockadeImaginarium.generatorOptions
            );

            if (EditorGUI.EndChangeCheck())
            {
                blockadeImaginarium.GetGeneratorFields(blockadeImaginarium.generatorOptionsIndex);
            }

            foreach (var field in blockadeImaginarium.generatorFields)
            {
                // Begin horizontal layout
                EditorGUILayout.BeginHorizontal();
                
                var required = field.required ? "*" : "";
                // Create label for field
                EditorGUILayout.LabelField(field.key + required);

                // Create text field for field value
                field.value = EditorGUILayout.TextField(field.value);

                // End horizontal layout
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void RenderSkyboxEditorFields(BlockadeImaginarium blockadeImaginarium)
        {
            EditorGUI.BeginChangeCheck();

            blockadeImaginarium.skyboxStyleOptionsIndex = EditorGUILayout.Popup(
                blockadeImaginarium.skyboxStyleOptionsIndex,
                blockadeImaginarium.skyboxStyleOptions
            );

            if (EditorGUI.EndChangeCheck())
            {
                blockadeImaginarium.GetSkyboxStyleFields(blockadeImaginarium.skyboxStyleOptionsIndex);
            }

            foreach (var field in blockadeImaginarium.skyboxStyleFields)
            {
                // Begin horizontal layout
                EditorGUILayout.BeginHorizontal();
                
                // Create label for field
                EditorGUILayout.LabelField(field.name + "*");
            
                // Create text field for field value
                field.value = EditorGUILayout.TextField(field.value);
            
                // End horizontal layout
                EditorGUILayout.EndHorizontal();
            }
        }
    }
#endif
}