using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Saveable asset used to store the colour settings for a planet.
    /// Includes the planet material, biome colour settings, and ocean colour gradient.
    /// </summary>
    /// <remarks>
    /// This class is modified from tutorial by Sebastian Lague
    /// found at https://github.com/SebLague/Procedural-Planets/blob/master/Procedural%20Planet%20E07/ColourSettings.cs.
    /// </remarks>
    [CreateAssetMenu()]
    public class ColourSettings : ScriptableObject
    {
        public Material planetMaterial;
        public BiomeColourSettings biomeColourSettings;
        public Gradient oceanColour;

        /// <summary>
        /// Class used to store the colour settings for each biome on the planet.
        /// Includes an array of biomes with noise and blend amount for borders.
        /// </summary>
        [System.Serializable]
        public class BiomeColourSettings
        {
            public Biome[] biomes;
            public Noise.Settings noise;
            public float noiseOffset;
            public float noiseStrength;
            [Range(0,1)]
            public float blendAmount;

            /// <summary>
            /// A serializable class representing a biome with its associated gradient, tint, start height.
            /// </summary>
            [System.Serializable]
            public class Biome
            {
                public Gradient gradient;
                public Color tint;
                [Range(0,1)]
                public float startHeight;
                [Range(0, 1)]
                public float tintPercent;
            }
        }

        /// <summary>
        /// Converts into a blittable version of the colour settings for use in a job system.
        /// </summary>
        /// <returns>A new instance of BlittableColourSettings</returns>
        public BlittableColourSettings GetBlittableColourSettings()
        {
            BlittableColourSettings blittableColourSettings = new()
            {
                parameters = biomeColourSettings.noise.GetBlittableParameters(),
                noiseOffset = biomeColourSettings.noiseOffset,
                noiseStrength = biomeColourSettings.noiseStrength,
                blendAmount = biomeColourSettings.blendAmount,
                biomeStartHeights = new NativeList<float>(biomeColourSettings.biomes.Length, Allocator.TempJob)
            };
            for (int i = 0; i < biomeColourSettings.biomes.Length; i++)
            {
                blittableColourSettings.biomeStartHeights.Add(biomeColourSettings.biomes[i].startHeight);
            }
            return blittableColourSettings;
        }
    }

    /// <summary>
    /// A struct that stores blittable colour settings for a planet.
    /// </summary>
    /// <seealso cref="Noise.BlittableParameters"/>
    /// <seealso cref="ColourSettings"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct BlittableColourSettings
    {
        public Noise.BlittableParameters parameters;
        public float noiseOffset;
        public float noiseStrength;
        public float blendAmount;
        public NativeList<float> biomeStartHeights;

        /// <summary>
        /// Release the memory allocated by the biome start heights native list.
        /// </summary>
        public void Dispose()
        {
            biomeStartHeights.Dispose();
        }

    }
}
