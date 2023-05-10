using System.Collections.Generic;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// A class that creates and manages a pool of chunks in the form of a queue.
    /// </summary>
    class ChunkPool
    {
        Queue<Chunk> chunkPool;
        Transform parent;
        Material material;

        public ChunkPool(int poolSize, Transform parent, Material material)
        {
            this.parent = parent;
            chunkPool = new Queue<Chunk>();
            this.material = material;

            for (int i = 0; i < poolSize; i++)
            {
                AddChunk();
            }
        }

        public void AddChunk()
        {
            Chunk chunk = new Chunk(material);
            chunk.go.transform.parent = parent;
            chunk.go.SetActive(false);
            chunkPool.Enqueue(chunk);
        }

        public Chunk HireChunk()
        {
            if (chunkPool.Count == 0)
            {
                AddChunk();
            }

            Chunk pooledChunk = chunkPool.Dequeue();
            pooledChunk.go.SetActive(true);
            return pooledChunk;
        }


        public void RetireChunk(Chunk chunk)
        {
            chunk.go.SetActive(false);
            chunkPool.Enqueue(chunk);
        }
    }
}