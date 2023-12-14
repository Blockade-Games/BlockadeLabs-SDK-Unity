using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    [CustomEditor(typeof(BlockadeLabsSkybox))]
    public class BlockadeLabsSkyboxEditor : UnityEditor.Editor
    {
        private SerializedProperty _lowDensityMesh;
        private SerializedProperty _mediumDensityMesh;
        private SerializedProperty _highDensityMesh;
        private SerializedProperty _meshDensity;

        private void OnEnable()
        {
            _lowDensityMesh = serializedObject.FindProperty("_lowDensityMesh");
            _mediumDensityMesh = serializedObject.FindProperty("_mediumDensityMesh");
            _highDensityMesh = serializedObject.FindProperty("_highDensityMesh");
            _meshDensity = serializedObject.FindProperty("_meshDensity");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_lowDensityMesh);
            EditorGUILayout.PropertyField(_mediumDensityMesh);
            EditorGUILayout.PropertyField(_highDensityMesh);
            EditorGUILayout.PropertyField(_meshDensity);
            if (serializedObject.ApplyModifiedProperties())
            {
                var skybox = (BlockadeLabsSkybox)target;
                skybox.UpdateMesh();
            }
        }
    }
}