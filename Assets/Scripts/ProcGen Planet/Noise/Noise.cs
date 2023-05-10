using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;

namespace ProcGenPlanet
{
    /// <summary>
    /// A static class that provides classes for generating various types of noise functions,
    /// including OpenSimplex; noise transform, including Basic, Ridged and Terraced
    /// </summary>
    /// <author>Stuart Brown</author>
    public static partial class Noise
    {
        /// <summary>
        /// An interface for implementing variants of noise functions for 3D sampling.
        /// </summary>
        public interface INoise
        {
            float Evaluate(float3 point);
        }

        /// <summary>
        /// An interface for implementing variants of fractal brownian motion.
        /// </summary>
        public interface INoiseTransform
        {
            float Evaluate(float3 point);
            float EvaluateMax();
            float EvaluateMin();
        }

        /// <summary>
        /// Factory class for creating noise and transform objects and also delegating their functionality.
        /// </summary>
        public static class Factory
        {
            public static NativeArray<int> Randomise(int seed)
            {
                return StaticOpenSimplex.Randomise(seed);
            }

            public static float Evaluate(float3 point, NativeArray<int> random, NativeArray<int3> Grad3)
            {
                return StaticOpenSimplex.Evaluate(point, random, Grad3);
            }

            public static float EvaluateTransform(ref float3 point, ref BlittableParameters parameters)
            {
                switch (parameters.transformType)
                {
                    case BlittableParameters.TransformType.Basic:
                        return BasicTransform.Evaluate(ref point, ref parameters);
                    case BlittableParameters.TransformType.Ridged:
                        return RidgedTransform.Evaluate(ref point, ref parameters);
                    case BlittableParameters.TransformType.Terraced:
                        return TerracedTransform.Evaluate(ref point, ref parameters);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameters.transformType), parameters.transformType, null);
                }
            }


            public static INoise CreateNoise(Settings settings)
            {
                switch (settings.noiseType)
                {
                    case Settings.NoiseType.SimplexPerlin:
                        return new OpenSimplex(settings.seed);
                    default:
                        break;
                }
                return null;
            }

            public static INoiseTransform CreateNoiseTransform(Settings settings)
            {
                switch (settings.transformType)
                {
                    case Settings.TransformType.Basic:
                        return new BasicTransform(settings.basicParameters, CreateNoise(settings));
                    case Settings.TransformType.Ridged:
                        return new RidgedTransform(settings.ridgedParameters, CreateNoise(settings));
                    case Settings.TransformType.Terraced:
                        return new TerracedTransform(settings.terracedParameters, CreateNoise(settings));
                    default:
                        break;
                }
                return null;
            }
        }

        /// <summary>
        /// Blittable version of noise parameters for use in jobs and burst.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct BlittableParameters
        {
            public enum NoiseType : byte { SimplexPerlin = 0 };
            public NoiseType noiseType;
            public enum TransformType : byte { Basic = 0, Ridged = 1, Terraced = 2 };
            public TransformType transformType;
            public int seed;
            public float3 offset;
            public float frequency;
            public int octaves;
            public float lacunarity;
            public float persistence;
            public float strength;
            public float lowerLimit;
            public float exponent;
            public float weight;
            public int levels;
        }

        /// <summary>
        /// Noise settings for use in Unity inspector, each settings includes a noise and transform type.
        /// </summary>
        [System.Serializable]
        public class Settings
        {
            public enum NoiseType { SimplexPerlin };
            public NoiseType noiseType;

            public int seed = 0;

            public enum TransformType { Basic, Ridged, Terraced };
            public TransformType transformType;

            [ConditionalHide("transformType", 0)]
            public BasicTransform.BasicParameters basicParameters;
            [ConditionalHide("transformType", 1)]
            public RidgedTransform.RidgedParameters ridgedParameters;
            [ConditionalHide("transformType", 2)]
            public TerracedTransform.TerracedParameters terracedParameters;

            /// <summary>
            /// Creates a blittable version of the noise setting's parameters for use in Unity jobs and burst.
            /// </summary>
            /// <returns>Blittable version of noise parameters.</returns>
            public BlittableParameters GetBlittableParameters()
            {
                BlittableParameters blittableParameters = new()
                {
                    noiseType = (BlittableParameters.NoiseType)(byte)noiseType,
                    transformType = (BlittableParameters.TransformType)(byte)transformType,
                    seed = seed
                };

                switch (transformType)
                {
                    case TransformType.Basic:
                        blittableParameters.offset = basicParameters.offset;
                        blittableParameters.frequency = basicParameters.frequency;
                        blittableParameters.octaves = basicParameters.octaves;
                        blittableParameters.lacunarity = basicParameters.lacunarity;
                        blittableParameters.persistence = basicParameters.persistence;
                        blittableParameters.strength = basicParameters.strength;
                        blittableParameters.lowerLimit = basicParameters.lowerLimit;
                        blittableParameters.exponent = 0;
                        blittableParameters.weight = 0;
                        blittableParameters.levels = 0;
                        break;
                    case TransformType.Ridged:
                        blittableParameters.offset = ridgedParameters.offset;
                        blittableParameters.frequency = ridgedParameters.frequency;
                        blittableParameters.octaves = ridgedParameters.octaves;
                        blittableParameters.lacunarity = ridgedParameters.lacunarity;
                        blittableParameters.persistence = ridgedParameters.persistence;
                        blittableParameters.strength = ridgedParameters.strength;
                        blittableParameters.lowerLimit = ridgedParameters.lowerLimit;
                        blittableParameters.exponent = ridgedParameters.exponent;
                        blittableParameters.weight = ridgedParameters.weight;
                        blittableParameters.levels = 0;
                        break;
                    case TransformType.Terraced:
                        blittableParameters.offset = terracedParameters.offset;
                        blittableParameters.frequency = terracedParameters.frequency;
                        blittableParameters.octaves = terracedParameters.octaves;
                        blittableParameters.lacunarity = terracedParameters.lacunarity;
                        blittableParameters.persistence = terracedParameters.persistence;
                        blittableParameters.strength = terracedParameters.strength;
                        blittableParameters.lowerLimit = terracedParameters.lowerLimit;
                        blittableParameters.exponent = 0;
                        blittableParameters.weight = 0;
                        blittableParameters.levels = terracedParameters.levels;
                        break;
                    default:
                        break;
                }

                return blittableParameters;
            }
        }
    }
}