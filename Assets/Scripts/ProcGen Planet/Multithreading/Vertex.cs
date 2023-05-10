using Unity.Mathematics;

namespace ProcGenPlanet
{
    /// <summary>
    /// Represents a vertex with position, normal, and texture coordinate information.
    /// </summary>
    /// <author>Stuart Brown</author>
    public struct Vertex
    {
        public float3 position, normal;
        public float2 texCoord0;
    }
}