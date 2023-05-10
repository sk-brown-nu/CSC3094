using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// A component which previews a planet in Unity's Editor mode.
    /// Uses a singlethreaded approach to generation.
    /// Only generates 6 chunks and no adaptive LOD.
    /// Has a custom Unity editor in the inspector.
    /// </summary>
    /// <author>Stuart Brown</author>
    [ExecuteInEditMode]
    public class PlanetPreview : MonoBehaviour
    {
        [Range(1, 255)]
        public int resolution = 64;
        public bool autoUpdate = true;

        public ShapeSettings shapeSettings;
        public ColourSettings colourSettings;

        [HideInInspector]
        public bool shapeSettingsFoldout;
        [HideInInspector]
        public bool colourSettingsFoldout;

        ShapeGenerator shapeGenerator = new ShapeGenerator();
        ColourGenerator colourGenerator = new ColourGenerator();

        QuadTreePlanet planet;
        List<Chunk> sides;

        bool hasInitialised;

        void Initialise()
        {
            if (!Application.isPlaying)
            {
                shapeGenerator.UpdateSettings(shapeSettings);
                colourGenerator.UpdateSettings(colourSettings);

                planet = new QuadTreePlanet(transform.position, shapeSettings.radius, 0, 0);
                sides = new List<Chunk>();

                foreach (var quadTree in planet.quadTrees)
                {
                    var side = new Chunk(colourSettings.planetMaterial);
                    side.go.transform.parent = transform;
                    sides.Add(side);
                }

                hasInitialised = true;
            }
        }

        void GenerateMesh()
        {
            for (int i = 0; i < 6; i++)
            {
                sides[i].ConstructMesh(planet.centre, shapeGenerator, planet.quadTrees[i].rootNode.Centre, planet.quadTrees[i].rootNode.UAxis, planet.quadTrees[i].rootNode.VAxis, resolution);
            }
            shapeGenerator.CalculateElevationMinMax();
            colourGenerator.UpdateElevation(shapeGenerator.elevationBounds);
        }

        void GenerateColours()
        {
            colourGenerator.UpdateTexture();
            for (int i = 0; i < 6; i++)
            {
                sides[i].UpdateUVs(colourGenerator, planet.quadTrees[i].rootNode.Centre, planet.quadTrees[i].rootNode.UAxis, planet.quadTrees[i].rootNode.VAxis, resolution);
            }
        }

        public void GeneratePlanet()
        {
            if (!hasInitialised)
            {
                Initialise();
            }
            GenerateMesh();
            GenerateColours();
        }

        public void OnShapeSettingsUpdated()
        {
            if (autoUpdate)
            {
                if (!hasInitialised)
                {
                    Initialise();
                }
                shapeGenerator.UpdateSettings(shapeSettings);
                GenerateMesh();
            }
        }

        public void OnColourSettingsUpdated()
        {
            if (autoUpdate)
            {
                if (!hasInitialised)
                {
                    Initialise();
                }
                colourGenerator.UpdateSettings(colourSettings);
                GenerateColours();
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                foreach (var side in sides)
                {
                    DestroyImmediate(side.go);
                }
                sides.Clear();
                planet = null;
                shapeGenerator = null;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                GeneratePlanet();
            }
        }

        private void OnDestroy()
        {
            foreach (var side in sides)
            {
                DestroyImmediate(side.go);
            }
        }
    }
}