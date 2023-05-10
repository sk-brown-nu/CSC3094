using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace ProcGenPlanet
{
    /// <summary>
    /// Represents a triangle with vertices defined by unsigned 16-bit integers.
    /// </summary>
    /// <remarks>
    /// This struct is adapted from tutorial by CatLike Coding
    /// found at https://catlikecoding.com/unity/tutorials/procedural-meshes/square-grid/.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleUInt16
    {
        public ushort a, b, c;

        /// <summary>
        /// Implicitly converts an int3 type to a TriangleUInt16 type.
        /// </summary>
        /// <param name="t">The int3 to convert.</param>
        /// <returns>A TriangleUInt16 representing the same triangle as the given int3.</returns>
        public static implicit operator TriangleUInt16(int3 t) => new()
        {
            a = (ushort)t.x,
            b = (ushort)t.y,
            c = (ushort)t.z
        };
    }
}