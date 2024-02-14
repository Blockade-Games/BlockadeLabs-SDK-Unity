using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace BlockadeLabsSDK.Editor
{
    internal enum ShowStartScreen
    {
        Always,
        OnUpdate,
        Never
    }

    [Serializable, CreateAssetMenu(fileName = "PluginSettings", menuName = "BlockadeLabsSDK/PluginSettings")]
    internal class PluginSettings : ScriptableObject
    {
        private const string _settingsGuid = "bb545e472ea14b640936128e690be415";

        public ShowStartScreen ShowStartScreen = 0;

        private static PluginSettings _settings;

        public void Save()
        {
            var saveIfDirty = typeof(AssetDatabase).GetMethod("SaveAssetIfDirty", new Type[] { typeof(UnityEngine.Object) });
            if (saveIfDirty != null)
            {
                EditorUtility.SetDirty(this);
                saveIfDirty.Invoke(null, new object[] { this });
            }
        }

        public static PluginSettings Get()
        {
            if (_settings == null)
            {
                var settingsPath = AssetDatabase.GUIDToAssetPath(_settingsGuid);
                _settings = AssetDatabase.LoadAssetAtPath<PluginSettings>(settingsPath);
            }

            return _settings;
        }
    }
}