using System;
using System.Collections;
using System.Collections.Generic;
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
    /// A component which generates a planet in real time.
    /// Uses a multithreaded approach to generation.
    /// Generates chunk based on player position.
    /// Number of chunks per frame can be limited.
    /// Not visible in editor mode.
    /// </summary>
    /// <author>Stuart Brown</author>
    public class PlanetRealtime : MonoBehaviour
    {
        public ShapeSettings shapeSettings;
        public ColourSettings colourSettings;

        ShapeGenerator shapeGenerator = new ShapeGenerator();
        ColourGenerator colourGenerator = new ColourGenerator();

        [SerializeField]
        int maxDepth = 10;

        [SerializeField]
        float thresholdMultiplier = 2;

        [SerializeField, Range(1, 255)]
        int resolution = 32;

        [SerializeField]
        int chunkPoolSize = 256;

        [SerializeField]
        private Transform playerTransform;
        private Vector3 lastPosition;

        QuadTreePlanet planet;
        ChunkPool chunkPool;

        HashSet<BitArray> currentIDs;
        Dictionary<BitArray, Chunk> currentChunks;
        Dictionary<BitArray, QuadNode> chunksToBeGenerated;
        [SerializeField]
        int chunksPerFrame = 16;

        void Awake()
        {
            shapeGenerator.UpdateSettings(shapeSettings);
            colourGenerator.UpdateSettings(colourSettings);

            shapeGenerator.CalculateElevationMinMax();
            colourGenerator.UpdateElevation(shapeGenerator.elevationBounds);
            colourGenerator.UpdateTexture();

            planet = new QuadTreePlanet(transform.position, shapeSettings.radius, maxDepth, thresholdMultiplier);
            chunkPool = new ChunkPool(chunkPoolSize, transform, colourSettings.planetMaterial);

            currentIDs = new HashSet<BitArray>(new BitArrayComparer());
            currentChunks = new Dictionary<BitArray, Chunk>(new BitArrayComparer());
            chunksToBeGenerated = new Dictionary<BitArray, QuadNode>(new BitArrayComparer());
        }

        private void Update()
        {
            if (playerTransform.position != lastPosition)
            {
                UpdateChunks();
                lastPosition = playerTransform.position;
            }
            if (chunksToBeGenerated.Count > 0)
            {
                var idList = new List<BitArray>(chunksToBeGenerated.Keys);
                idList.Sort(new BitArrayLengthComparer());

                var chunksThisFrame = new List<QuadNode>();
                for (int i = 0; i < Math.Min(idList.Count, chunksPerFrame); i++)
                {
                    if (chunksToBeGenerated.TryGetValue(idList[i], out QuadNode node))
                    {
                        chunksThisFrame.Add(node);
                        chunksToBeGenerated.Remove(idList[i]);
                    }
                }

                GenerateChunks(chunksThisFrame);
            }
        }

        public void UpdateChunks()
        {
            var newLeaves = GetNewLeaves();
            var newIDs = GetNewIDs(newLeaves);

            if (newIDs.SetEquals(currentIDs)) return;

            var retiringIDs = GetRetiringIDs(newIDs);
            var hiringIDs = GetHiringIDs(newIDs);

            currentIDs = newIDs;

            RemoveRetiringChunks(retiringIDs);
            AddHiringChunks(newLeaves, hiringIDs);
        }

        private Dictionary<BitArray, QuadNode> GetNewLeaves()
        {
            var newLeaves = new Dictionary<BitArray, QuadNode>();
            foreach (var quadTree in planet.quadTrees)
            {
                quadTree.Insert(quadTree.rootNode, playerTransform.position);
                quadTree.GetAllLeafNodes(quadTree.rootNode, newLeaves);
                quadTree.CullLeaves(newLeaves, playerTransform.position);
            }
            return newLeaves;
        }

        private HashSet<BitArray> GetNewIDs(Dictionary<BitArray, QuadNode> newLeaves)
        {
            var newIDs = new HashSet<BitArray>(newLeaves.Keys, new BitArrayComparer());
            return newIDs;
        }

        private HashSet<BitArray> GetRetiringIDs(HashSet<BitArray> newIDs)
        {
            var retiringIDs = new HashSet<BitArray>(currentIDs, new BitArrayComparer());
            retiringIDs.ExceptWith(newIDs);
            return retiringIDs;
        }

        private HashSet<BitArray> GetHiringIDs(HashSet<BitArray> newIDs)
        {
            var hiringIDs = new HashSet<BitArray>(newIDs, new BitArrayComparer());
            hiringIDs.ExceptWith(currentIDs);
            return hiringIDs;
        }

        private void RemoveRetiringChunks(HashSet<BitArray> retiringIDs)
        {
            foreach (var id in retiringIDs)
            {
                if (currentChunks.TryGetValue(id, out var chunk))
                {
                    chunkPool.RetireChunk(chunk);
                    currentChunks.Remove(id);
                    chunksToBeGenerated.Remove(id);
                }
            }
        }

        private void AddHiringChunks(Dictionary<BitArray, QuadNode> newLeaves, HashSet<BitArray> hiringIDs)
        {
            foreach (var id in hiringIDs)
            {
                if (newLeaves.TryGetValue(id, out var leaf))
                {
                    var chunk = chunkPool.HireChunk();
                    chunk.id = id;
                    currentChunks.Add(id, chunk);
                    chunksToBeGenerated.Add(id, leaf);
                }
            }
        }

        private void GenerateChunks(List<QuadNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return;
            }

            NativeArray<ChunkData> chunkDataArray = new(nodes.Count, Allocator.TempJob);

            for (int i = 0; i < nodes.Count; i++)
            {
                QuadNode node = nodes[i];
                var chunkData = new ChunkData()
                {
                    origin = node.Centre,
                    uAxis = node.UAxis,
                    vAxis = node.VAxis
                };
                chunkDataArray[i] = chunkData;
            }

            var resultStream = new Stream();
            resultStream.Initialise(nodes.Count * (resolution + 1) * (resolution + 1), nodes.Count * 6 * resolution * resolution);

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

            Mesh[] meshes = new Mesh[nodes.Count];

            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = new Mesh();
            }

            foreach (var mesh in meshes)
            {
                mesh.bounds = new(new float3(transform.position), new Vector3(4f * shapeSettings.radius, 4f * shapeSettings.radius, 4f * shapeSettings.radius));
            }

            MeshDataArray meshDataArray = AllocateWritableMeshData(nodes.Count);

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

            for (int i = 0; i < nodes.Count; i++)
            {
                if (currentChunks.TryGetValue(nodes[i].ID, out var chunk))
                {
                    chunk.go.GetComponent<MeshFilter>().sharedMesh = meshes[i];
                }
            }
        }
    }
}