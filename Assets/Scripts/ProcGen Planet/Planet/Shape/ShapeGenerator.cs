using Unity.Mathematics;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Class for generating planet shape evaluating noise layers in settings.
    /// </summary>
    /// <remarks>
    /// This class is modified from tutorial by Sebastian Lague.
    /// found at https://github.com/SebLague/Procedural-Planets/blob/master/Procedural%20Planet%20E07/ShapeGenerator.cs.
    /// </remarks>
    public class ShapeGenerator
    {
        public ShapeSettings settings;
        Noise.INoiseTransform[] noiseTransforms;
        public FloatRange elevationBounds;


        /// <summary>
        /// Updates the shape generator settings with the given <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The new shape generator settings.</param>
        public void UpdateSettings(ShapeSettings settings)
        {
            this.settings = settings;
            noiseTransforms = new Noise.INoiseTransform[settings.noiseLayers.Length];
            for (int i = 0; i < noiseTransforms.Length; i++)
            {
                noiseTransforms[i] = Noise.Factory.CreateNoiseTransform(settings.noiseLayers[i].noiseSettings);
            }
            elevationBounds = new FloatRange();
        }

        /// <summary>
        /// Adds noise layers together to find elevation on a given point on sphere.
        /// </summary>
        /// <param name="pointOnSphere">A point on the planet.</param>
        /// <returns>The amount of elevation.</returns>
        public float EvaluateElevation(float3 pointOnSphere)
        {
            float firstLayerValue = 0;
            float elevation = 0;

            if (noiseTransforms.Length > 0)
            {
                firstLayerValue = noiseTransforms[0].Evaluate(pointOnSphere);
                if (settings.noiseLayers[0].enabled)
                {
                    elevation = firstLayerValue;
                }
            }

            for (int i = 1; i < noiseTransforms.Length; i++)
            {
                if (settings.noiseLayers[i].enabled)
                {
                    float mask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                    elevation += noiseTransforms[i].Evaluate(pointOnSphere) * mask;
                }
            }

            elevationBounds.AddValue(elevation);
            return elevation;
        }

        /// <summary>
        /// Variant of EvaluateElevation for jobs using blittable version of settings.
        /// </summary>
        /// <param name="pointOnSphere">The point on the planet.</param>
        /// <param name="settings">The blittable version of the shape settings.</param>
        /// <returns>The amount of elevation.</returns>
        public static float EvaluateElevation(float3 pointOnSphere, BlittableShapeSettings settings)
        {
            float firstLayerValue = 0;
            float elevation = 0;

            if (settings.noiseLayers.Length > 0)
            {
                var parameters = settings.noiseLayers[0].parameters;
                firstLayerValue = Noise.Factory.EvaluateTransform(ref pointOnSphere, ref parameters);
                if (settings.noiseLayers[0].enabled)
                {
                    elevation = firstLayerValue;
                }
            }

            for (int i = 1; i < settings.noiseLayers.Length; i++)
            {
                if (settings.noiseLayers[i].enabled)
                {
                    var parameters = settings.noiseLayers[i].parameters;
                    float mask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                    elevation += Noise.Factory.EvaluateTransform(ref pointOnSphere, ref parameters) * mask;
                }
            }

            return elevation;
        }

        /// <summary>
        /// Returns the elevation above sea level based on the settings radius.
        /// </summary>
        /// <param name="elevation">Elevation, either above are below sea level.</param>
        /// <returns>The elevation above sea level i.e. altitude.</returns>
        public float GetAltitude(float elevation)
        {
            return GetAltitude(elevation, settings.radius);
        }

        /// <summary>
        /// A static version of GetAltitude. Returns the elevation above sea level based on a radius.
        /// </summary>
        /// <param name="elevation">Elevation, either above are below sea level.</param>
        /// <param name="radius">The given radius of a planet.</param>
        /// <returns>The elevation above sea level i.e. altitude.</returns>
        public static float GetAltitude(float elevation, float radius)
        {
            float altitude = Mathf.Max(0, elevation);
            altitude = radius * (1 + altitude);
            return altitude;
        }

        /// <summary>
        /// Overestimates the maximum and minimum elevation of a planet,
        /// to save on actually calculating the elevation of every point to find the actual bounds.
        /// </summary>
        public void CalculateElevationMinMax()
        {
            float firstLayerValueMin = 0;
            float firstLayerValueMax = 0;
            float minElevation = 0;
            float maxElevation = 0;

            if (noiseTransforms.Length > 0)
            {
                firstLayerValueMin = noiseTransforms[0].EvaluateMin();
                firstLayerValueMax = noiseTransforms[0].EvaluateMax();
                if (settings.noiseLayers[0].enabled)
                {
                    minElevation = firstLayerValueMin;
                    maxElevation = firstLayerValueMax;
                }
            }

            for (int i = 1; i < noiseTransforms.Length; i++)
            {
                if (settings.noiseLayers[i].enabled)
                {
                    float minMask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValueMin : 1;
                    minElevation += noiseTransforms[i].EvaluateMin() * minMask;
                    float maxMask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValueMax : 1;
                    maxElevation += noiseTransforms[i].EvaluateMax() * maxMask;
                }
            }

            elevationBounds.AddValue(minElevation);
            elevationBounds.AddValue(-minElevation);
            elevationBounds.AddValue(maxElevation);
            elevationBounds.AddValue(-maxElevation);
        }
    }
}