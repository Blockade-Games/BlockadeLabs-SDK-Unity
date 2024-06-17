using System.Collections.Generic;
using UnityEngine;

namespace BlockadeLabsSDK
{
    internal static class BakeDepth
    {
        public static Mesh Bake(Mesh tetrahedronMesh, Texture2D depthMap, float depthScale)
        {
            var uvs = tetrahedronMesh.uv;
            var newVertices = tetrahedronMesh.vertices;
            var vertMap = new Dictionary<int, int>();

            if (!depthMap.isReadable)
            {
                throw new System.Exception("Depth map is not readable!");
            }

            for (int i = 0; i < tetrahedronMesh.vertexCount; i++)
            {
                var color = depthMap.GetPixelBilinear(uvs[i].x, uvs[i].y);
                if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                {
                    color = color.linear;
                }

                var depth = color.g;
                newVertices[i] = ComputeDepth(uvs[i], newVertices[i], depth, depthScale);
            }

            var mesh = new Mesh();
            mesh.name = tetrahedronMesh.name + "_DepthBaked";
            mesh.indexFormat = tetrahedronMesh.indexFormat;
            mesh.SetVertices(newVertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tetrahedronMesh.GetTriangles(0), 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector3 ComputeDepth(Vector2 uv, Vector3 input, float depth, float depthScale)
        {
            float displacement = Mathf.Clamp(1.0f / depth + 10 / depthScale, 0, depthScale);
            return input.normalized * displacement;
        }
    }
}