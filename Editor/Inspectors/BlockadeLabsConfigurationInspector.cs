using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    [CustomEditor(typeof(BlockadeLabsConfiguration))]
    internal class BlockadeLabsConfigurationInspector : UnityEditor.Editor
    {
        private static bool indent;
        private static bool triggerReload;

        private Texture _logo;
        private SerializedProperty _apiKey;
        private SerializedProperty _proxyDomainUrl;

        public static bool ShowApiHelpBox { get; set; }

        #region Project Settings Window

        [SettingsProvider]
        private static SettingsProvider Preferences()
            => GetSettingsProvider(nameof(BlockadeLabs), CheckReload);

        #endregion Project Settings Window

        #region Inspector Window

        private void OnEnable()
        {
            GetOrCreateInstance(target);

            _logo = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("7a7ad95e1e1c4ee488d47d37aa36e95a"));

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

        private void OnDisable()
            => CheckReload();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            GUILayout.Box(_logo, EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space();

            if (indent)
            {
                EditorGUI.indentLevel++;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_apiKey);

            if (GUILayout.Button("Apply", GUILayout.Width(64)))
            {
                if (!string.IsNullOrWhiteSpace(_apiKey.stringValue))
                {
                    VSAttribution.SendAttributionEvent("Initialization", "BlockadeLabs", _apiKey.stringValue);
                }

                triggerReload = true;
                EditorApplication.delayCall += CheckReload;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_proxyDomainUrl);

            if (GUILayout.Button("Apply", GUILayout.Width(64)))
            {
                triggerReload = true;
                EditorApplication.delayCall += CheckReload;
            }

            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrWhiteSpace(_apiKey.stringValue) || ShowApiHelpBox)
            {
                const string apiUrl = "https://skybox.blockadelabs.com/dashboard/api";
                var message = $"In order to use this package you need to provide an API key from Blockade Labs in the API section.\n<a href=\"{apiUrl}\">{apiUrl}</a>";
                EditorGUILayout.Space();

                if (BlockadeGUI.HelpBoxLinkButton(message, MessageType.Warning))
                {
                    Application.OpenURL(apiUrl);
                }
            }

            if (indent)
            {
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Inspector Window

        internal static async void CheckReload()
        {
            if (triggerReload)
            {
                triggerReload = false;
                await BlockadeLabsSkyboxGeneratorEditor.InitializeAsync(true);
                EditorApplication.delayCall += EditorUtility.RequestScriptReload;
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
            var instanceEditor = CreateEditor(instance);
            indent = true;
            instanceEditor.OnInspectorGUI();
            indent = false;
        }

        internal static BlockadeLabsConfiguration GetOrCreateInstance(object target = null)
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
