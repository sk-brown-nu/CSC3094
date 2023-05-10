using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;
using static ProcGenPlanet.ChunkJob;
using static ProcGenPlanet.Stream;

namespace ProcGenPlanet
{
    /// <summary>
    /// </summary>
    /// <author>Stuart Brown</author>
    public class TestMultithread : MonoBehaviour
    {
        public ShapeSettings shapeSettings;
        public ColourSettings colourSettings;

        readonly ShapeGenerator shapeGenerator = new();
        readonly ColourGenerator colourGenerator = new();

        [SerializeField]
        int numberOfChunks;

        [SerializeField, Range(1, 255)]
        int resolution;

        [SerializeField]
        int numberOfLoops;
        private int count = 0;

        [SerializeField]
        float3 centre, uAxis, vAxis;

        ChunkData chunkData;

        void Awake()
        {
            shapeGenerator.UpdateSettings(shapeSettings);
            colourGenerator.UpdateSettings(colourSettings);
            shapeGenerator.CalculateElevationMinMax();
            colourGenerator.UpdateElevation(shapeGenerator.elevationBounds);
            colourGenerator.UpdateTexture();
            chunkData = new ChunkData()
            {
                origin = centre,
                uAxis = uAxis,
                vAxis = vAxis
            };
        }

        private void Update()
        {
            GenerateAllChunks();
            count++;
            if (count >= numberOfLoops)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }

        private void GenerateAllChunks()
        {
            NativeArray<ChunkData> chunkDataArray = new(numberOfChunks, Allocator.TempJob);

            for (int i = 0; i < numberOfChunks; i++)
            {
                chunkDataArray[i] = chunkData;
            }

            var resultStream = new Stream();
            resultStream.Initialise(numberOfChunks * (resolution + 1) * (resolution + 1), numberOfChunks * 6 * resolution * resolution);

            var job = new ChunkJob
            {
                resolution = resolution,
                planetCentre = new float3(transform.position),
                shapeSettings = shapeSettings.GetBlittableShapeSettings(),
                colourSettings = colourSettings.GetBlittableColourSettings(),
                chunksArray = chunkDataArray,
                stream = resultStream
            };

            var jobHandle = job.ScheduleParallel(job.JobLength, 1, default);

            jobHandle.Complete();

            job.shapeSettings.Dispose();
            job.colourSettings.Dispose();
            chunkDataArray.Dispose();

            Mesh[] meshes = new Mesh[numberOfChunks];

            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = new Mesh();
            }

            foreach (var mesh in meshes)
            {
                mesh.bounds = new(new float3(transform.position), new Vector3(4f * shapeSettings.radius, 4f * shapeSettings.radius, 4f * shapeSettings.radius));
            }

            MeshDataArray meshDataArray = AllocateWritableMeshData(numberOfChunks);

            var descriptor = new NativeArray<VertexAttributeDescriptor>(
                3, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );
            descriptor[0] = new VertexAttributeDescriptor(
                VertexAttribute.Position, dimension: 3
            );
            descriptor[1] = new VertexAttributeDescriptor(
                VertexAttribute.Normal, dimension: 3
            );
            descriptor[2] = new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0, dimension: 2
            );

            for (int i = 0; i < meshDataArray.Length; i++)
            {
                var meshData = meshDataArray[i];
                meshData.SetVertexBufferParams((resolution + 1) * (resolution + 1), descriptor);
                meshData.SetIndexBufferParams(6 * resolution * resolution, IndexFormat.UInt16);
                meshData.subMeshCount = 1;
                meshData.SetSubMesh(
                    0, new SubMeshDescriptor(0, 6 * resolution * resolution)
                    {
                        bounds = new(new float3(transform.position), new Vector3(4f * shapeSettings.radius, 4f * shapeSettings.radius, 4f * shapeSettings.radius)),
                        vertexCount = (resolution + 1) * (resolution + 1)
                    },
                    MeshUpdateFlags.DontRecalculateBounds |
                    MeshUpdateFlags.DontValidateIndices
                );
            }

            descriptor.Dispose();

            for (int i = 0; i < meshDataArray.Length; i++)
            {
                var meshData = meshDataArray[i];

                var startVertexIndex = i * (resolution + 1) * (resolution + 1);

                var verticesSlice = resultStream.vertices.GetSubArray(startVertexIndex, (resolution + 1) * (resolution + 1));
                var tempVertices = meshData.GetVertexData<SequentialVertex>();
                tempVertices.CopyFrom(verticesSlice);

                var trianglesSlice = resultStream.triangles.GetSubArray(0, 2 * resolution * resolution);
                var tempTriangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
                tempTriangles.CopyFrom(trianglesSlice);
            }

            ApplyAndDisposeWritableMeshData(meshDataArray, meshes);

            resultStream.Dispose();

            for (int i = 0; i < numberOfChunks; i++)
            {
                var chunk = new Chunk(colourSettings.planetMaterial);
                chunk.go.transform.parent = transform;
                chunk.go.GetComponent<MeshFilter>().sharedMesh = meshes[i];
                chunk.go.SetActive(false);
            }
        }
    }
}