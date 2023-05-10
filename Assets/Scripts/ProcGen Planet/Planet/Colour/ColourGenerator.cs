using Unity.Mathematics;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Class for generating planet texture, updating the planet material, evaluation biome position.
    /// </summary>
    /// <remarks>
    /// This class is modified from tutorial by Sebastian Lague.
    /// found at https://github.com/SebLague/Procedural-Planets/blob/master/Procedural%20Planet%20E07/ColourGenerator.cs.
    /// </remarks>
    public class ColourGenerator
    {
        ColourSettings settings;
        Texture2D texture;
        const int textureResolution = 64;
        Noise.INoiseTransform biomeNoiseTransform;

        /// <summary>
        /// Updates the colour generator settings with the given <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The new colour generator settings.</param>
        public void UpdateSettings(ColourSettings settings)
        {
            this.settings = settings;
            // Increases or decreases texture size based on number of biomes.
            if (texture == null || texture.height != settings.biomeColourSettings.biomes.Length)
            {
                texture = new Texture2D(textureResolution * 2, settings.biomeColourSettings.biomes.Length, TextureFormat.RGBA32, false);
            }
            biomeNoiseTransform = Noise.Factory.CreateNoiseTransform(settings.biomeColourSettings.noise);
        }

        /// <summary>
        /// Updates _elevationBounds property of planet material.
        /// </summary>
        /// <param name="elevationBounds">Min and max value of elevation.</param>
        public void UpdateElevation(FloatRange elevationBounds)
        {
            settings.planetMaterial.SetVector("_elevationBounds", new Vector4(elevationBounds.MinValue, elevationBounds.MaxValue));
        }

        /// <summary>
        /// Generates texture where number of biomes = number of rows.
        /// Each biome has an ocean and elevation gradient.
        /// Updates planet material's _texture property.
        /// </summary>
        public void UpdateTexture()
        {
            Color[] colours = new Color[texture.width * texture.height];

            int colourIndex = 0;
            foreach (var biome in settings.biomeColourSettings.biomes)
            {
                for (int i = 0; i < textureResolution * 2; i++)
                {
                    Color gradientColour;
                    if (i < textureResolution)
                    {
                        float t = (float)i / (textureResolution - 1);
                        gradientColour = settings.oceanColour.Evaluate(t);
                    }
                    else
                    {
                        float t = (float)(i - textureResolution) / (textureResolution - 1);
                        gradientColour = biome.gradient.Evaluate(t);
                    }
                    Color tintColour = biome.tint;
                    colours[colourIndex] = gradientColour * (1 - biome.tintPercent) + tintColour * biome.tintPercent;
                    colourIndex++;
                }
            }
            texture.SetPixels(colours);
            texture.Apply();
            settings.planetMaterial.SetTexture("_texture", texture);
        }

        /// <summary>
        /// Based on colourSettings, finds the biome at a given point on a unity sphere.
        /// Biomes are ordered based on planet latitude.
        /// </summary>
        /// <param name="pointOnUnitSphere">The point on a unit sphere.</param>
        /// <returns>The index of the biome.</returns>
        public float EvaluateBiome(float3 pointOnUnitSphere)
        {
            float heightPercent = (pointOnUnitSphere.y + 1) / 2f;
            heightPercent += (biomeNoiseTransform.Evaluate(pointOnUnitSphere) - settings.biomeColourSettings.noiseOffset) * settings.biomeColourSettings.noiseStrength;
            float biomeIndex = 0;
            int numBiomes = settings.biomeColourSettings.biomes.Length;
            float blendRange = settings.biomeColourSettings.blendAmount / 2f + .001f;

            for (int i = 0; i < numBiomes; i++)
            {
                float distance = heightPercent - settings.biomeColourSettings.biomes[i].startHeight;
                float weight = Mathf.InverseLerp(-blendRange, blendRange, distance);
                biomeIndex *= (1 - weight);
                biomeIndex += i * weight;
            }
            return biomeIndex / Mathf.Max(1, numBiomes - 1);
        }

        /// <summary>
        /// A version of EvaluateBiome for jobs, using blittable version of colour settings.
        /// </summary>
        /// <param name="pointOnUnitSphere">The point on a unit sphere.</param>
        /// <param name="settings">The blittable colour settings.</param>
        /// <returns>The index of a biome.</returns>
        public static float EvaluateBiome(float3 pointOnUnitSphere, BlittableColourSettings settings)
        {
            float heightPercent = (pointOnUnitSphere.y + 1) / 2f;
            heightPercent += (Noise.Factory.EvaluateTransform(ref pointOnUnitSphere, ref settings.parameters) - settings.noiseOffset) * settings.noiseStrength;
            float biomeIndex = 0;
            int numBiomes = settings.biomeStartHeights.Length;
            float blendRange = settings.blendAmount / 2f + .001f;

            for (int i = 0; i < numBiomes; i++)
            {
                float distance = heightPercent - settings.biomeStartHeights[i];
                float weight = Mathf.InverseLerp(-blendRange, blendRange, distance);
                biomeIndex *= (1 - weight);
                biomeIndex += i * weight;
            }
            return biomeIndex / Mathf.Max(1, numBiomes - 1);
        }
    }
}