#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockadeLabsSDK
{
    internal static class AssetUtils
    {
        private const string RootFolder = "Assets";
        private const string GenerateFolderName = "Blockade Labs SDK";
        private const string GenerateFolderPath = RootFolder + "/" + GenerateFolderName;

        internal static void CreateGenerateBlockadeLabsFolder()
        {
            if (!AssetDatabase.IsValidFolder(GenerateFolderPath))
            {
                AssetDatabase.CreateFolder(RootFolder, GenerateFolderName);
            }
        }

        internal static string CreateUniqueFolder(string name)
        {
            CreateGenerateBlockadeLabsFolder();
            var uniquePath = AssetDatabase.GenerateUniqueAssetPath(GenerateFolderPath + "/" + name);
            var uniqueName = uniquePath.Substring(uniquePath.LastIndexOf('/') + 1);
            AssetDatabase.CreateFolder(GenerateFolderPath, uniqueName);
            return uniquePath;
        }

        /// <summary>
        /// Try to create a folder with the given name. 
        /// </summary>
        /// <param name="name">Directory name.</param>
        /// <param name="path">Path to the directory.</param>
        /// <returns>Returns true if the folder was created.</returns>
        internal static bool TryCreateFolder(string name, out string path)
        {
            CreateGenerateBlockadeLabsFolder();
            path = $"{GenerateFolderPath}/{name}";

            if (AssetDatabase.IsValidFolder(path))
            {
                return false;
            }

            AssetDatabase.CreateFolder(GenerateFolderPath, name);
            return true;

        }

        internal static string CreateValidFilename(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            while (name.Contains("__"))
            {
                name = name.Replace("__", "_");
            }

            return name.TrimStart('_').TrimEnd('_').Trim();
        }

        internal static T LoadAsset<T>(string guid) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }

        internal static void PingAsset(Object asset)
        {
            EditorApplication.ExecuteMenuItem("Window/General/Project");
            EditorGUIUtility.PingObject(asset);
        }

        internal static string GetFolder(Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            return path.Substring(0, path.LastIndexOf('/'));
        }

        internal static GameObject SaveSceneToPrefab(string prefabPath)
        {
            var currentScene = SceneManager.GetActiveScene();

            var root = new GameObject("SkyboxScene");

            foreach (var go in currentScene.GetRootGameObjects())
            {
                if (go != root)
                {
                    go.transform.SetParent(root.transform, true);
                }
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            while (root.transform.childCount > 0)
            {
                var child = root.transform.GetChild(0);
                child.SetParent(null, true);
            }

            ObjectUtils.Destroy(root);

            return prefab;
        }

        internal static void SaveAssetNextTo(Object asset, Object nextToAsset)
        {
            var folder = GetFolder(nextToAsset);
            SaveAsset(asset, folder, asset.name);
        }

        internal static void SavePrefabNextTo(GameObject gameObject, Object nextToAsset)
        {
            var folder = GetFolder(nextToAsset);
            SavePrefab(gameObject, folder, gameObject.name);
        }

        internal static void SaveAsset(Object asset, string folder, string name)
        {
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            PingAsset(asset);
        }

        internal static void SavePrefab(GameObject gameObject, string folder, string name)
        {
            var prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}.prefab");
            var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            PingAsset(prefab);
        }

        internal static string ToProjectPath(this string @string)
        {
            return @string.Replace("\\", "/").Replace(Application.dataPath, "Assets");
        }
    }
}

#endif