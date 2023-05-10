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
        /// A variant of fractal brownian motion for creating terraced multifractals.
        /// </summary>
        public class TerracedTransform : INoiseTransform
        {
            [System.Serializable]
            public class TerracedParameters : BasicTransform.BasicParameters
            {
                [Min(1)]
                public int levels = 1;
            }

            TerracedParameters parameters;

            INoise noise;

            public TerracedTransform(TerracedParameters parameters, INoise noise)
            {
                this.parameters = parameters;
                this.noise = noise;
            }

            public float Evaluate(float3 point)
            {
                float noiseValue = 0;
                float frequency = parameters.frequency;
                float amplitude = 1;

                for (int i = 0; i < parameters.octaves; i++)
                {
                    float v = noise.Evaluate(point * frequency + parameters.offset);
                    noiseValue += (v + 1) * 0.5f * amplitude;
                    frequency *= parameters.lacunarity;
                    amplitude *= parameters.persistence;
                }

                noiseValue = Mathf.Round(noiseValue * parameters.levels) / parameters.levels;
                noiseValue -= parameters.lowerLimit;
                return noiseValue * parameters.strength;
            }

            public static float Evaluate(ref float3 point, ref BlittableParameters parameters)
            {
                float noiseValue = 0;
                float frequency = parameters.frequency;
                float amplitude = 1;
                NativeArray<int3> Grad3 = StaticOpenSimplex.GetGrad3();
                NativeArray<int> random = Factory.Randomise(parameters.seed);

                for (int i = 0; i < parameters.octaves; i++)
                {
                    float v = Factory.Evaluate(point * frequency + parameters.offset, random, Grad3);
                    noiseValue += (v + 1) * 0.5f * amplitude;
                    frequency *= parameters.lacunarity;
                    amplitude *= parameters.persistence;
                }
                
                Grad3.Dispose();
                random.Dispose();
                noiseValue = Mathf.Round(noiseValue * parameters.levels) / parameters.levels;
                noiseValue -= parameters.lowerLimit;
                return noiseValue * parameters.strength;
            }

            public float EvaluateMax()
            {
                float noiseValue = 1;
                for (int i = 1; i < parameters.octaves; i++)
                {
                    noiseValue += Mathf.Pow(parameters.persistence, i);
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