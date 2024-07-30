using System;
using UnityEngine;

namespace BlockadeLabs.Editor
{
    internal static class CubemapExtensions
    {
        private const string UnityEditorTextureUtil = "UnityEditor.TextureUtil, UnityEditor";

        private static Type TextureUtil { get; } = Type.GetType(UnityEditorTextureUtil);

        private static System.Reflection.MethodInfo CopyTextureIntoCubemapFace { get; } =
            TextureUtil.GetMethod(
                nameof(CopyTextureIntoCubemapFace),
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                null,
                new[] { typeof(Texture2D), typeof(Cubemap), typeof(CubemapFace) },
                null);

        internal static void SetCubemapTexture(this Cubemap cubemap, Texture2D texture, CubemapFace face)
            => CopyTextureIntoCubemapFace.Invoke(null, new object[] { texture, cubemap, (int)face });
    }
}
