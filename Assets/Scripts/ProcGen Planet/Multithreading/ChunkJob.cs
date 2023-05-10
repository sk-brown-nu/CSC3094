using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace ProcGenPlanet
{
    /// <summary>
    /// A job that generates mesh data for a planet terrain chunk in parallel.
    /// </summary>
    /// <author>Stuart Brown</author>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct ChunkJob : IJobFor
    {
        /// <summary>
        /// The data for a single chunk, including its origin and axes.
        /// </summary>
        public struct ChunkData
        {
            public float3 origin, uAxis, vAxis;
        }

        /// <summary>
        /// The total number of vertices for all chunks in the job.
        /// </summary>
        public int VertexCount => chunksArray.Length * (resolution + 1) * (resolution + 1);

        /// <summary>
        /// The total number of indices for all chunks in the job.
        /// </summary>
        public int IndexCount => chunksArray.Length * 2 * resolution * resolution;

        /// <summary>
        /// The total number of chunks in the job.
        /// </summary>
        public int JobLength => chunksArray.Length;

        [ReadOnly] public int resolution;
        [ReadOnly] public NativeArray<ChunkData> chunksArray;
        [ReadOnly] public float3 planetCentre;
        [ReadOnly] public BlittableShapeSettings shapeSettings;
        [ReadOnly] public BlittableColourSettings colourSettings;

        /// <summary>
        /// A stream for writing mesh data.
        /// </summary>
        [NativeDisableParallelForRestriction] public Stream stream;

        public void Execute(int i)
        {
            Execute(i, stream);
        }

        /// <summary>
        /// Creates vertices and triangles for a chunk of the planet mesh.
        /// </summary>
        /// <param name="chunkIndex">The index of the chunk to create vertices and triangles for.</param>
        /// <param name="stream">The stream to write the vertex and index data to.</param>
        private void Execute(int chunkIndex, Stream stream)
        {
            var chunk = chunksArray[chunkIndex];
            int vertexIndex = chunkIndex * (resolution + 1) * (resolution + 1);
            int triangleIndex = chunkIndex * 2 * resolution * resolution;

            // Loop through all the vertices in the chunk
            for (int i = 0; i < (resolution + 1) * (resolution + 1); i++)
            {
                int u = i % (resolution + 1);
                int v = i / (resolution + 1);

                float3 pointOnUnitSphere = CalculatePointOnUnitSphere(chunk, resolution, u, v);

                float biome = ColourGenerator.EvaluateBiome(pointOnUnitSphere, colourSettings);
                float elevation = ShapeGenerator.EvaluateElevation(pointOnUnitSphere, shapeSettings);

                CreateVertex(stream, vertexIndex, pointOnUnitSphere, biome, elevation);

                // If this is not the last row or column of vertices, create two triangles for this quad
                if (u != resolution && v != resolution)
                {
                    CreateTriangles(stream, triangleIndex, vertexIndex, resolution);
                    triangleIndex += 2;
                }

                vertexIndex++;
            }
        }

        /// <summary>
        /// Calculates a point on a unit sphere based on the chunk data, resolution, and UV coordinates.
        /// </summary>
        /// <param name="chunk">The chunk data.</param>
        /// <param name="resolution">The resolution of the mesh.</param>
        /// <param name="u">The U coordinate of the point on the mesh.</param>
        /// <param name="v">The V coordinate of the point on the mesh.</param>
        /// <returns>A point on a unit sphere.</returns>
        private float3 CalculatePointOnUnitSphere(ChunkData chunk, int resolution, int u, int v)
        {
            float2 percent = new float2(u, v) / resolution;
            float3 pointOnCube = chunk.origin + (percent.x - .5f) * 2 * chunk.uAxis + (percent.y - .5f) * 2 * chunk.vAxis;
            return normalize(pointOnCube - planetCentre);
        }

        /// <summary>
        /// Creates a new vertex at the specified index in the given stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="vertexIndex">The index of the vertex to create.</param>
        /// <param name="pointOnUnitSphere">The point on the unit sphere to create the vertex at.</param>
        /// <param name="biome">The biome value to assign to the vertex.</param>
        /// <param name="elevation">The elevation value to assign to the vertex.</param>
        private void CreateVertex(Stream stream, int vertexIndex, float3 pointOnUnitSphere, float biome, float elevation)
        {
            var vertex = new Vertex
            {
                texCoord0 = new float2(biome, elevation),
                position = planetCentre + (pointOnUnitSphere * ShapeGenerator.GetAltitude(elevation, shapeSettings.radius)),
            };
            vertex.normal = normalize(vertex.position - planetCentre);
            stream.SetVertex(vertexIndex, vertex);
        }

        /// <summary>
        /// Creates two triangles in the given stream using the specified triangle and vertex indices and resolution.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="triangleIndex">The starting index of the triangles to create.</param>
        /// <param name="vertexIndex">The index of the vertex to start with.</param>
        /// <param name="resolution">The resolution of the mesh.</param>
        private void CreateTriangles(Stream stream, int triangleIndex, int vertexIndex, int resolution)
        {
            stream.SetTriangle(triangleIndex, int3(vertexIndex, vertexIndex + resolution + 1, vertexIndex + 1));
            stream.SetTriangle(triangleIndex + 1, int3(vertexIndex + 1, vertexIndex + resolution + 1, vertexIndex + resolution + 2));
        }
    }
}