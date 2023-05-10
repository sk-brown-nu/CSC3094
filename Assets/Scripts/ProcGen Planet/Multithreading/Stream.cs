using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace ProcGenPlanet
{
    /// <summary>
    /// Struct that represents streams of vertices and indices.
    /// </summary>
    /// <author>Stuart Brown</author>
    public struct Stream
    {
        /// <summary>
        /// Struct that represents the vertex information sequentially.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SequentialVertex
        {
            public float3 position, normal;
            public float2 texCoord0;
        }

        /// <summary>
        /// The array of vertices.
        /// </summary>
        [NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction]
        public NativeArray<SequentialVertex> vertices;

        /// <summary>
        /// The array of triangles.
        /// </summary>
        [NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction]
        public NativeArray<TriangleUInt16> triangles;

        /// <summary>
        /// Initializes the vertex and index arrays.
        /// </summary>
        /// <param name="totalVertexCount">The total number of vertices.</param>
        /// <param name="totalIndexCount">The total number of indices.</param>
        public void Initialise(int totalVertexCount, int totalIndexCount)
        {
            vertices = new NativeArray<SequentialVertex>(totalVertexCount, Allocator.TempJob);
            triangles = new NativeArray<ushort>(totalIndexCount, Allocator.TempJob).Reinterpret<TriangleUInt16>(2);
        }

        /// <summary>
        /// Disposes the vertex and index arrays.
        /// </summary>
        public void Dispose()
        {
            vertices.Dispose();
            triangles.Dispose();
        }

        /// <summary>
        /// Sets the vertex at the given index.
        /// </summary>
        /// <param name="index">The index to set the vertex.</param>
        /// <param name="vertex">The vertex to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int index, Vertex vertex) => vertices[index] = new SequentialVertex
        {
            position = vertex.position,
            normal = vertex.normal,
            texCoord0 = vertex.texCoord0
        };

        /// <summary>
        /// Sets the triangle at the given index.
        /// </summary>
        /// <param name="index">The index to set the triangle.</param>
        /// <param name="triangle">The triangle to set.</param>
        public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
    }
}