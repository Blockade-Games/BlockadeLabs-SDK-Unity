using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    [CustomEditor(typeof(BlockadeLabsSkyboxMesh))]
    internal class BlockadeLabsSkyboxMeshEditor : UnityEditor.Editor
    {
        private SerializedProperty _skyboxMetadata;
        private SerializedProperty _meshDensity;
        private SerializedProperty _depthScale;

        private void OnEnable()
        {
            _skyboxMetadata = serializedObject.FindProperty(nameof(_skyboxMetadata));
            _meshDensity = serializedObject.FindProperty(nameof(_meshDensity));
            _depthScale = serializedObject.FindProperty(nameof(_depthScale));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var skybox = (BlockadeLabsSkyboxMesh)target;

            if (GUILayout.Button("Move Scene Camera to Skybox"))
            {
                SceneView.lastActiveSceneView.AlignViewToObject(skybox.transform);
            }

            GUILayout.Space(16);

            BlockadeGUI.DisableGroup(skybox.BakedMesh, () =>
            {
                EditorGUILayout.PropertyField(_skyboxMetadata);
                EditorGUILayout.PropertyField(_meshDensity);
                EditorGUILayout.PropertyField(_depthScale);
            });

            GUILayout.Space(16);

            BlockadeGUI.Horizontal(() =>
            {
                BlockadeGUI.DisableGroup(!skybox.HasDepthTexture, () =>
                {
                    if (!skybox.BakedMesh && GUILayout.Button("Bake Depth to Mesh"))
                    {
                        skybox.BakeMesh();
                        EditorUtility.SetDirty(skybox);
                    }
                    else if (skybox.BakedMesh && GUILayout.Button("Unbake Depth from Mesh"))
                    {
                        skybox.BakedMesh = null;
                        EditorUtility.SetDirty(skybox);
                    }
                });

                if (GUILayout.Button("Save Prefab"))
                {
                    skybox.SavePrefab();
                }
            });

            if (serializedObject.ApplyModifiedProperties())
            {
                skybox.EditorPropertyChanged();
            }
        }
    }
}