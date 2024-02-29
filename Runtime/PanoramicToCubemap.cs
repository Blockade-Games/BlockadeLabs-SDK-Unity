using UnityEngine;

namespace BlockadeLabsSDK
{
    internal static class PanoramicToCubemap
    {
        public static Cubemap Convert(Texture2D panoramicTexture, ComputeShader computeShader, int size)
        {
            RenderTexture renderTexture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32);
            renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
            renderTexture.volumeDepth = 6;
            renderTexture.enableRandomWrite = true;
            renderTexture.useMipMap = false;
            renderTexture.Create();

            int kernelHandle = computeShader.FindKernel("CSMain");
            computeShader.SetTexture(kernelHandle, "_PanoramicTexture", panoramicTexture);
            computeShader.SetTexture(kernelHandle, "_CubemapTexture", renderTexture);
            computeShader.SetInt("_Size", size);
            computeShader.Dispatch(kernelHandle, size / 8, size / 8, 6);

            var cubemap = new Cubemap(size, TextureFormat.ARGB32, false);
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                var face = (CubemapFace)faceIndex;
                Graphics.CopyTexture(renderTexture, faceIndex, 0, cubemap, faceIndex, 0);
            }

            renderTexture.Release();
            return cubemap;
        }

        public static Cubemap Convert(Texture2D panoramicTexture, int size)
        {
            var cubemap = new Cubemap(size, TextureFormat.RGBA32, false);

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                var face = (CubemapFace)faceIndex;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        Vector3 direction = GetDirectionForCubemapFace(x, y, size, face);
                        Vector2 uv = ConvertDirectionToUV(direction);
                        Color color = panoramicTexture.GetPixelBilinear(uv.x, uv.y);

                        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                        {
                            color = color.linear;
                        }

                        cubemap.SetPixel(face, x, y, color);
                    }
                }
            }

            cubemap.Apply();
            return cubemap;
        }

        private static Vector3 GetDirectionForCubemapFace(int x, int y, int size, CubemapFace face)
        {
            float s = (float)x / (size - 1);
            float t = (float)y / (size - 1);
            float u = s * 2 - 1;
            float v = t * 2 - 1;

            switch (face)
            {
                case CubemapFace.PositiveX:
                    return new Vector3(1, -v, -u).normalized;
                case CubemapFace.NegativeX:
                    return new Vector3(-1, -v, u).normalized;
                case CubemapFace.PositiveY:
                    return new Vector3(u, 1, v).normalized;
                case CubemapFace.NegativeY:
                    return new Vector3(u, -1, -v).normalized;
                case CubemapFace.PositiveZ:
                    return new Vector3(u, -v, 1).normalized;
                case CubemapFace.NegativeZ:
                    return new Vector3(-u, -v, -1).normalized;
                default:
                    return Vector3.zero;
            }
        }

        private static Vector2 ConvertDirectionToUV(Vector3 direction)
        {
            float u = 1 - Mathf.Atan2(direction.z, direction.x) / (2 * Mathf.PI) + 0.5f;
            float v = Mathf.Asin(direction.y) / Mathf.PI + 0.5f;
            return new Vector2(u, v);
        }
    }
}