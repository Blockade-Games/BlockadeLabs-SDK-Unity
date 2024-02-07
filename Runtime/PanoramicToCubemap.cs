using UnityEngine;

namespace BlockadeLabsSDK
{
    public static class PanoramicToCubemap
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
    }
}