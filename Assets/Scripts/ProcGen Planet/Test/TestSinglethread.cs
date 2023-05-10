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
    public class TestSinglethread : MonoBehaviour
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

        void Awake()
        {
            shapeGenerator.UpdateSettings(shapeSettings);
            colourGenerator.UpdateSettings(colourSettings);
            shapeGenerator.CalculateElevationMinMax();
            colourGenerator.UpdateElevation(shapeGenerator.elevationBounds);
            colourGenerator.UpdateTexture();
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
            for (int i = 0; i < numberOfChunks; i++)
            {
                var chunk = new Chunk(colourSettings.planetMaterial);
                chunk.ConstructMesh(transform.position, shapeGenerator, centre, uAxis, vAxis, resolution);
                chunk.go.SetActive(false);
            }
        }
    }
}