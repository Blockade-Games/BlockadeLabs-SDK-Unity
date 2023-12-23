using System.Collections.Generic;
using UnityEngine;

namespace BlockadeLabsSDK
{
    public static class TetrahedronMesh
    {
        public static Mesh GenerateMesh(int subdivisions)
        {
            var mesh = new Mesh();
            mesh.name = "Tetrahedron_" + subdivisions;

            int[] vertices = {
                1, 1, 1,
                -1, -1, 1,
                -1, 1, -1,
                1, -1, -1
            };

            int[] indices = {
                2, 1, 0,
                0, 3, 2,
                1, 3, 0,
                2, 3, 1
            };

            var vertexBuffer = CreateVertexBuffer(subdivisions, vertices, indices);

            // Make it a sphere
            for (var i = 0; i < vertexBuffer.Count; i++) {
                vertexBuffer[i] = Vector3.Normalize(vertexBuffer[i]) * 3;
			}

            vertexBuffer.Reverse();

            mesh.SetVertices(vertexBuffer);

            var uvBuffer = GenerateUVs(vertexBuffer);
            mesh.SetUVs(0, uvBuffer);

            var triangles = new int[vertexBuffer.Count];
            for (var i = 0; i < triangles.Length; i++)
            {
                triangles[i] = i;
            }

            mesh.indexFormat = (vertexBuffer.Count > 65535) ?
                UnityEngine.Rendering.IndexFormat.UInt32 :
                UnityEngine.Rendering.IndexFormat.UInt16;

            mesh.SetTriangles(triangles, 0);

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static List<Vector3> CreateVertexBuffer(int subdivisions, int[] vertices, int[] indices)
        {
            var vertexBuffer = new List<Vector3>();

            // iterate over all faces and apply a subdivision with the given detail value
            for (var i = 0; i < indices.Length; i += 3)
            {
                // get the vertices of the face
                var a = GetVertexByIndex(vertices, indices[i]);
                var b = GetVertexByIndex(vertices, indices[i + 1]);
                var c = GetVertexByIndex(vertices, indices[i + 2]);

                // perform subdivision
                SubdivideFace(a, b, c, subdivisions, vertexBuffer);
            }

            return vertexBuffer;
        }

        private static Vector3 GetVertexByIndex(int[] vertices, int index)
        {

            var stride = index * 3;
            return new Vector3(
                vertices[stride],
                vertices[stride + 1],
                vertices[stride + 2]);
        }

        private static void SubdivideFace(Vector3 a, Vector3 b, Vector3 c, int subdivisions, List<Vector3> vertexBuffer)
        {
            var cols = subdivisions + 1;

            // we use this multidimensional array as a data structure for creating the subdivision
            var v = new List<List<Vector3>>();

            // construct all of the vertices for this subdivision
            for (var i = 0; i <= cols; i++)
            {
                var colVerts = new List<Vector3>();

                var aj = Vector3.Lerp(a, c, i / (float)cols);
                var bj = Vector3.Lerp(b, c, i / (float)cols);

                var rows = cols - i;
                for (var j = 0; j <= rows; j++)
                {
                    if (j == 0 && i == cols)
                    {
                        colVerts.Add(aj);
                    }
                    else
                    {
                        colVerts.Add(Vector3.Lerp(aj, bj, j / (float)rows));
                    }
                }

                v.Add(colVerts);
            }

            // construct all of the faces
            for (var i = 0; i < cols; i++)
            {
                for (var j = 0; j < 2 * (cols - i) - 1; j++)
                {
                    var k = Mathf.FloorToInt(j / 2f);
                    if (j % 2 == 0)
                    {
                        vertexBuffer.Add(v[i][k + 1]);
                        vertexBuffer.Add(v[i + 1][k]);
                        vertexBuffer.Add(v[i][k]);
                    }
                    else
                    {
                        vertexBuffer.Add(v[i][k + 1]);
                        vertexBuffer.Add(v[i + 1][k + 1]);
                        vertexBuffer.Add(v[i + 1][k]);
                    }
                }
            }
        }

        private static List<Vector2> GenerateUVs(List<Vector3> vertexBuffer)
        {
            var uvBuffer = new List<Vector2>();
            foreach (var vertex in vertexBuffer)
            {
                var u = Azimuth(vertex) / 2f / Mathf.PI + 0.5f;
                var v = Inclination(vertex) / Mathf.PI + 0.5f;
                uvBuffer.Add(new Vector2(u, 1 - v));
            }

            CorrectUVs(vertexBuffer, uvBuffer);
            CorrectSeam(uvBuffer);
            return uvBuffer;
        }

        private static void CorrectUVs(List<Vector3> vertexBuffer, List<Vector2> uvBuffer)
        {
            for (int i = 0; i < vertexBuffer.Count; i += 3)
            {
                var a = vertexBuffer[i];
                var b = vertexBuffer[i + 1];
                var c = vertexBuffer[i + 2];

                var uvA = uvBuffer[i];
                var uvB = uvBuffer[i + 1];
                var uvC = uvBuffer[i + 2];

                var centroid = (a + b) / 3f;

                var azi = Azimuth(centroid);

                uvBuffer[i] = CorrectUV(uvA, a, azi);
                uvBuffer[i + 1] = CorrectUV(uvB, b, azi);
                uvBuffer[i + 2] = CorrectUV(uvC, c, azi);
            }
        }

        private static Vector2 CorrectUV(Vector2 uv, Vector3 vector, float azimuth)
        {
            if ((azimuth < 0f) && (uv.x == 1f))
            {
                uv.x = uv.x - 1f;
            }

            if ((vector.x == 0f) && (vector.z == 0f))
            {
                uv.x = azimuth / 2f / Mathf.PI + 0.5f;
            }

            return uv;
        }

        // Angle around the Y axis, counter-clockwise when looking from above.
        private static float Azimuth(Vector3 vector)
        {
            return Mathf.Atan2(vector.z, -vector.x);
        }

        // Angle above the XZ plane.
        private static float Inclination(Vector3 vector)
        {
            return Mathf.Atan2(-vector.y, Mathf.Sqrt((vector.x * vector.x) + (vector.z * vector.z)));
        }

        private static void CorrectSeam(List<Vector2> uvBuffer)
        {
            // handle case when face straddles the seam, see #3269
            for (var i = 0; i < uvBuffer.Count; i += 3)
            {
                // uv data of a single face
                var x0 = uvBuffer[i].x;
                var x1 = uvBuffer[i + 1].x;
                var x2 = uvBuffer[i + 2].x;

                var max = Mathf.Max(x0, x1, x2);
                var min = Mathf.Min(x0, x1, x2);

                // 0.9 is somewhat arbitrary
                if (max > 0.9f && min < 0.1f)
                {
                    if (x0 < 0.2f) uvBuffer[i] += Vector2.right;
                    if (x1 < 0.2f) uvBuffer[i + 1] += Vector2.right;
                    if (x2 < 0.2f) uvBuffer[i + 2] += Vector2.right;
                }
            }
        }
    }
}