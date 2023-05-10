using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Saveable asset used to store the shape settings for a planet.
    /// Contains multiple layers of configurable noise.
    /// </summary>
    /// <remarks>
    /// This class is modified from tutorial by Sebastian Lague
    /// found at https://github.com/SebLague/Procedural-Planets/blob/master/Procedural%20Planet%20E07/ShapeSettings.cs.
    /// </remarks>
    [CreateAssetMenu()]
    public class ShapeSettings : ScriptableObject
    {
        public float radius = 1f;
        public NoiseLayer[] noiseLayers;

        /// <summary>
        /// Each noise layer contains a noise type and transform type used to generate terrain.
        /// The first noise layer can be used as a mask.
        /// </summary>
        [System.Serializable]
        public class NoiseLayer
        {
            public bool enabled = true;
            public bool useFirstLayerAsMask;
            public Noise.Settings noiseSettings;
        }

        /// <summary>
        /// Converts into a blittable version of the shape settings for use in a job system.
        /// </summary>
        /// <returns>A new instance of BlittableShapeSettings</returns>
        public BlittableShapeSettings GetBlittableShapeSettings()
        {
            BlittableShapeSettings blittableShapeSettings = new()
            {
                radius = radius,
                noiseLayers = new NativeList<BlittableShapeSettings.BlittableNoiseLayer>(noiseLayers.Length, Allocator.TempJob)
            };
            for (int i = 0; i < noiseLayers.Length; i++)
            {
                BlittableShapeSettings.BlittableNoiseLayer noiseLayer = new()
                {
                    enabled = noiseLayers[i].enabled,
                    useFirstLayerAsMask = noiseLayers[i].useFirstLayerAsMask,
                    parameters = noiseLayers[i].noiseSettings.GetBlittableParameters()
                };
                blittableShapeSettings.noiseLayers.Add(noiseLayer);
            }
            return blittableShapeSettings;
        }
    }

    /// <summary>
    /// A struct that stores blittable shape settings for a planet.
    /// </summary>
    /// <seealso cref="Noise.BlittableParameters"/>
    /// <seealso cref="ColourSettings"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct BlittableShapeSettings
    {
        public float radius;

        public NativeList<BlittableNoiseLayer> noiseLayers;

        [StructLayout(LayoutKind.Sequential)]
        public struct BlittableNoiseLayer
        {
            public bool enabled;
            public bool useFirstLayerAsMask;
            public Noise.BlittableParameters parameters;
        }

        public void Dispose()
        {
            noiseLayers.Dispose();
        }
    }
}
