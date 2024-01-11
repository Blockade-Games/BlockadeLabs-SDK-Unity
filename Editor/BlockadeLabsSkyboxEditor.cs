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

            EditorGUI.BeginDisabledGroup(!skybox.CanSave);
            if (GUILayout.Button("Save Prefab"))
            {
                skybox.SavePrefab();
            }
            EditorGUI.EndDisabledGroup();

            if (serializedObject.ApplyModifiedProperties())
            {
                skybox.EditorPropertyChanged();
            }
        }
    }
}