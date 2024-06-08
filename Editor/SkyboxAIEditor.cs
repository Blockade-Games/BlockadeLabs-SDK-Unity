using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    [CustomEditor(typeof(SkyboxAI))]
    internal class SkyboxAIEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("obfuscated_id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skybox_style_id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skybox_style_name"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("status"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
            GUI.enabled = true;
        }
    }
}