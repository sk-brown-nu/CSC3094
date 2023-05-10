using UnityEditor;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Customer Editor class for the <see cref="PlanetPreview"/> component.
    /// Used to customize the Unity editor interface for creating and editing planets.
    /// </summary>
    /// <remarks>
    /// This class is modified from tutorial by Sebastian Lague.
    /// found at https://github.com/SebLague/Procedural-Planets/blob/master/Procedural%20Planet%20E07/Editor/PlanetEditor.cs.
    /// </remarks>
    [CustomEditor(typeof(PlanetPreview))]
    public class PlanetEditor : Editor
    {
        private PlanetPreview planet;

        private Editor shapeEditor;
        private Editor colourEditor;

        public override void OnInspectorGUI()
        {
            // Check if the target is a PlanetPreview component
            if (planet == null)
            {
                return;
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                if (check.changed)
                {
                    planet.GeneratePlanet();
                }
            }

            // Show the Generate Planet button
            if (GUILayout.Button("Generate Planet"))
            {
                planet.GeneratePlanet();
            }

            // Show the shape and colour settings editors
            DrawSettingsEditor(planet.shapeSettings, planet.OnShapeSettingsUpdated, ref planet.shapeSettingsFoldout, ref shapeEditor);
            DrawSettingsEditor(planet.colourSettings, planet.OnColourSettingsUpdated, ref planet.colourSettingsFoldout, ref colourEditor);
        }

        void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
        {
            if (settings != null)
            {
                foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    if (foldout)
                    {
                        CreateCachedEditor(settings, null, ref editor);
                        editor.OnInspectorGUI();

                        if (check.changed)
                        {
                            if (onSettingsUpdated != null)
                            {
                                onSettingsUpdated();
                            }
                        }
                    }
                }
            }
        }

        private void OnEnable()
        {
            planet = target as PlanetPreview;
        }
    }
}