using System.Collections;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Represents a chunk of terrain with an associated id, game object and mesh.
    /// </summary>
    /// <author>Stuart Brown</author>
    class Chunk
    {
        public BitArray id;
        public GameObject go;
        public Mesh mesh;
        Material material;

        public Chunk(Material material)
        {
            go = new GameObject("Chunk");
            go.transform.parent = go.transform;

            mesh = new Mesh();
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            this.material = material;
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = this.material;

            go.AddComponent<WireframeGizmo>();
        }

        public void ConstructMesh(Vector3 planetCentre, ShapeGenerator shapeGenerator, Vector3 nodeCentre, Vector3 uAxis, Vector3 vAxis, int resolution)
        {
            Vector3[] vertexList = new Vector3[(resolution + 1) * (resolution + 1)];
            int[] indexList = new int[resolution * resolution * 6];
            int triIndex = 0;
            Vector2[] uv = (mesh.uv.Length == vertexList.Length) ? mesh.uv : new Vector2[vertexList.Length];

            int i = 0;
            for (int v = 0; v < (resolution + 1); v++)
            {
                for (int u = 0; u < (resolution + 1); u++)
                {
                    Vector2 percent = new Vector2(u, v) / resolution;

                    Vector3 pointOnCube = nodeCentre + (percent.x - .5f) * 2 * uAxis + (percent.y - .5f) * 2 * vAxis;
                    Vector3 pointOnSphere = pointOnCube.normalized;

                    float unscaledElevation = shapeGenerator.EvaluateElevation(pointOnSphere);
                    uv[i].y = unscaledElevation;

                    vertexList[i] = planetCentre + (pointOnSphere * shapeGenerator.GetAltitude(unscaledElevation));

                    if (u != resolution && v != resolution)
                    {
                        indexList[triIndex] = i;
                        indexList[triIndex + 1] = i + resolution + 1;
                        indexList[triIndex + 2] = i + 1;

                        indexList[triIndex + 3] = i + 1;
                        indexList[triIndex + 4] = i + resolution + 1;
                        indexList[triIndex + 5] = i + resolution + 2;
                        triIndex += 6;
                    }

                    i++;
                }
            }
            mesh.Clear();
            mesh.vertices = vertexList;
            mesh.triangles = indexList;
            mesh.RecalculateNormals();
            mesh.uv = uv;
        }

        public void UpdateUVs(ColourGenerator colourGenerator, Vector3 nodeCentre, Vector3 uAxis, Vector3 vAxis, int resolution)
        {
            Vector2[] uv = mesh.uv;

            int i = 0;
            for (int v = 0; v < (resolution + 1); v++)
            {
                for (int u = 0; u < (resolution + 1); u++)
                {
                    Vector2 percent = new Vector2(u, v) / resolution;

                    Vector3 pointOnCube = nodeCentre + (percent.x - .5f) * 2 * uAxis + (percent.y - .5f) * 2 * vAxis;
                    Vector3 pointOnSphere = pointOnCube.normalized;

                    uv[i].x = colourGenerator.EvaluateBiome(pointOnSphere);
                    i++;
                }
            }
            mesh.uv = uv;
        }
    }
}