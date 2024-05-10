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

        private static void CreateGenerateFolder()
        {
            if (!AssetDatabase.IsValidFolder(GenerateFolderPath))
            {
                AssetDatabase.CreateFolder(RootFolder, GenerateFolderName);
            }
        }

        internal static string CreateUniqueFolder(string name)
        {
            CreateGenerateFolder();
            var uniquePath = AssetDatabase.GenerateUniqueAssetPath(GenerateFolderPath + "/" + name);
            var uniqueName = uniquePath.Substring(uniquePath.LastIndexOf('/') + 1);
            AssetDatabase.CreateFolder(GenerateFolderPath, uniqueName);
            return uniquePath;
        }

        internal static string GetOrCreateFolder(string name)
        {
            CreateGenerateFolder();
            var path = GenerateFolderPath + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(GenerateFolderPath, name);
            }

            return path;
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

        internal static T LoadAsset<T>(string guid) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }

        internal static void PingAsset(UnityEngine.Object asset)
        {
            EditorApplication.ExecuteMenuItem("Window/General/Project");
            EditorGUIUtility.PingObject(asset);
        }

        internal static string GetFolder(UnityEngine.Object asset)
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
            var folder = AssetUtils.GetFolder(nextToAsset);
            SaveAsset(asset, folder, asset.name);
        }

        internal static void SavePrefabNextTo(GameObject gameObject, Object nextToAsset)
        {
            var folder = AssetUtils.GetFolder(nextToAsset);
            SaveAsset(gameObject, folder, gameObject.name);
        }

        internal static void SaveAsset(Object asset, string folder, string name)
        {
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetUtils.PingAsset(asset);
        }

        internal static void SavePrefab(GameObject gameObject, string folder, string name)
        {
            var prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}.prefab");
            var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            AssetUtils.PingAsset(prefab);
        }
    }
}

#endif