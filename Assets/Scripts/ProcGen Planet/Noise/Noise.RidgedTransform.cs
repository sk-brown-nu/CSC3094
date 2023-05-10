using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

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
        /// A variant of fractal brownian motion for creating ridged multifractals.
        /// </summary>
        public class RidgedTransform : INoiseTransform
        {
            [System.Serializable]
            public class RidgedParameters : BasicTransform.BasicParameters
            {
                public float exponent = 2;

                public float weight = 0.5f;
            }

            RidgedParameters parameters;

            INoise noise;

            public RidgedTransform(RidgedParameters parameters, INoise noise)
            {
                this.parameters = parameters;
                this.noise = noise;
            }

            public float Evaluate(float3 point)
            {
                float noiseValue = 0;
                float frequency = parameters.frequency;
                float amplitude = 1;
                float weight = 1;

                for (int i = 0; i < parameters.octaves; i++)
                {
                    float v = 1 - Mathf.Abs(noise.Evaluate(point * frequency + parameters.offset));
                    v = Mathf.Pow(v, parameters.exponent);
                    v *= weight;
                    weight = Mathf.Clamp01(v * parameters.weight);

                    noiseValue += v * amplitude;
                    frequency *= parameters.lacunarity;
                    amplitude *= parameters.persistence;
                }

                noiseValue -= parameters.lowerLimit;
                return noiseValue * parameters.strength;
            }

            public static float Evaluate(ref float3 point, ref BlittableParameters parameters)
            {
                float noiseValue = 0;
                float frequency = parameters.frequency;
                float amplitude = 1;
                float weight = 1;
                NativeArray<int3> Grad3 = StaticOpenSimplex.GetGrad3();
                NativeArray<int> random = Factory.Randomise(parameters.seed);

                for (int i = 0; i < parameters.octaves; i++)
                {
                    float v = 1 - Mathf.Abs(Factory.Evaluate(point * frequency + parameters.offset, random, Grad3));
                    v = Mathf.Pow(v, parameters.exponent);
                    v *= weight;
                    weight = Mathf.Clamp01(v * parameters.weight);

                    noiseValue += v * amplitude;
                    frequency *= parameters.lacunarity;
                    amplitude *= parameters.persistence;
                }

                Grad3.Dispose();
                random.Dispose();
                noiseValue -= parameters.lowerLimit;
                return noiseValue * parameters.strength;
            }

            public float EvaluateMax()
            {
                float noiseValue = Mathf.Clamp01(parameters.weight);
                for (int i = 1; i < parameters.octaves; i++)
                {
                    noiseValue += Mathf.Clamp01(parameters.weight) * Mathf.Pow(parameters.persistence, i);
                }
                noiseValue -= parameters.lowerLimit;
                return noiseValue * parameters.strength;
            }

            public float EvaluateMin()
            {
                float noiseValue = -parameters.lowerLimit;
                return noiseValue * parameters.strength;
            }
        }
    }
}