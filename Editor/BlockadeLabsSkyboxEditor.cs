using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    [CustomEditor(typeof(BlockadeLabsSkybox))]
    public class BlockadeLabsSkyboxEditor : UnityEditor.Editor
    {
        private SerializedProperty _meshDensity;
        private SerializedProperty _depthScale;

        private void OnEnable()
        {
            _meshDensity = serializedObject.FindProperty("_meshDensity");
            _depthScale = serializedObject.FindProperty("_depthScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_meshDensity);
            EditorGUILayout.PropertyField(_depthScale);

            var skybox = (BlockadeLabsSkybox)target;

            if (GUILayout.Button("Move Scene Camera to Skybox"))
            {
                SceneView.lastActiveSceneView.AlignViewToObject(skybox.transform);
            }

            var meshFilter = skybox.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                bool hasUnsavedMesh = meshFilter.sharedMesh != null &&
                    string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(meshFilter.sharedMesh));

                if (hasUnsavedMesh)
                {
                    if (GUILayout.Button("Save Mesh"))
                    {
                        var path = EditorUtility.SaveFilePanelInProject("Save mesh", meshFilter.sharedMesh.name, "asset", "Save mesh");
                        if (string.IsNullOrEmpty(path))
                        {
                            return;
                        }

                        AssetDatabase.CreateAsset(meshFilter.sharedMesh, path);
                        AssetDatabase.SaveAssets();
                    }
                }
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                skybox.EditorPropertyChanged();
            }
        }
    }
}