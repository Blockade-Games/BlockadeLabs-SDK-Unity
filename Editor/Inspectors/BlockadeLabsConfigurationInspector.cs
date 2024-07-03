using System;
using System.IO;
using UnityEditor;

namespace BlockadeLabsSDK.Editor
{
    [CustomEditor(typeof(BlockadeLabsConfiguration))]
    public class BlockadeLabsConfigurationInspector : UnityEditor.Editor
    {
        private static bool triggerReload;

        private SerializedProperty _apiKey;
        private SerializedProperty _proxyDomainUrl;

        #region Project Settings Window

        [SettingsProvider]
        private static SettingsProvider Preferences()
            => GetSettingsProvider(nameof(BlockadeLabs), CheckReload);

        #endregion Project Settings Window

        #region Inspector Window

        private void OnEnable()
        {
            GetOrCreateInstance(target);

            try
            {
                _apiKey = serializedObject.FindProperty(nameof(_apiKey));
                _proxyDomainUrl = serializedObject.FindProperty(nameof(_proxyDomainUrl));
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        private void OnDisable() => CheckReload();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_apiKey);
            EditorGUILayout.PropertyField(_proxyDomainUrl);

            if (EditorGUI.EndChangeCheck())
            {
                triggerReload = true;
            }

            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        #endregion Inspector Window

        private static void CheckReload()
        {
            if (triggerReload)
            {
                triggerReload = false;
                EditorUtility.RequestScriptReload();
            }
        }

        private static SettingsProvider GetSettingsProvider(string name, Action deactivateHandler)
            => new SettingsProvider($"Project/{name}", SettingsScope.Project, new[] { name })
            {
                label = name,
                guiHandler = OnPreferencesGui,
                deactivateHandler = deactivateHandler
            };

        private static void OnPreferencesGui(string searchContext)
        {
            if (EditorApplication.isPlaying ||
                EditorApplication.isCompiling)
            {
                return;
            }

            var instance = GetOrCreateInstance();

            if (Selection.activeObject != instance)
            {
                Selection.activeObject = instance;
            }

            var instanceEditor = CreateEditor(instance);
            instanceEditor.OnInspectorGUI();
        }

        private static BlockadeLabsConfiguration GetOrCreateInstance(Object target = null)
        {
            var update = false;
            BlockadeLabsConfiguration instance;

            const string resourcesDirectory = "Assets/Blockade Labs SDK/Resources";

            if (!Directory.Exists(resourcesDirectory))
            {
                Directory.CreateDirectory(resourcesDirectory);
                update = true;
            }

            if (target != null)
            {
                instance = target as BlockadeLabsConfiguration;

                var currentPath = AssetDatabase.GetAssetPath(instance);

                if (string.IsNullOrWhiteSpace(currentPath))
                {
                    return instance;
                }

                if (!currentPath.Contains("Resources"))
                {
                    var newPath = $"{resourcesDirectory}/{instance!.name}.asset";

                    if (!File.Exists(newPath))
                    {
                        File.Move(Path.GetFullPath(currentPath), Path.GetFullPath(newPath));
                        File.Move(Path.GetFullPath($"{currentPath}.meta"), Path.GetFullPath($"{newPath}.meta"));
                    }
                    else
                    {
                        AssetDatabase.DeleteAsset(currentPath);
                        var instances = AssetDatabase.FindAssets($"t:{nameof(BlockadeLabsConfiguration)}");

                        if (instances != null && instances.Length > 0)
                        {
                            var path = AssetDatabase.GUIDToAssetPath(instances[0]);
                            instance = AssetDatabase.LoadAssetAtPath<BlockadeLabsConfiguration>(path);
                        }
                    }

                    update = true;
                }
            }
            else
            {
                var instances = AssetDatabase.FindAssets($"t:{nameof(BlockadeLabsConfiguration)}");

                if (instances.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(instances[0]);
                    instance = AssetDatabase.LoadAssetAtPath<BlockadeLabsConfiguration>(path);
                }
                else
                {
                    instance = CreateInstance<BlockadeLabsConfiguration>();
                    AssetDatabase.CreateAsset(instance, $"{resourcesDirectory}/{nameof(BlockadeLabsConfiguration)}.asset");
                    update = true;
                }
            }

            if (update)
            {
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    EditorGUIUtility.PingObject(instance);
                };
            }

            return instance;
        }
    }
}
