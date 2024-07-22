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
            DrawPropertiesExcluding(serializedObject, "m_Script");
            GUI.enabled = true;
        }
    }
}