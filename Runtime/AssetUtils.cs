#if UNITY_EDITOR
using System.IO;
using UnityEditor;

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
            var path = AssetDatabase.GenerateUniqueAssetPath(name);
            AssetDatabase.CreateFolder(GenerateFolderPath, name);
            return path;
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
    }
}

#endif